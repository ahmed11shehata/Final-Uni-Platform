using Abstraction.Contracts;
using Microsoft.EntityFrameworkCore;
using Presistence;
using Shared.Dtos.Instructor_Module;

namespace Presentation.Services
{
    /// <summary>
    /// Backend-authoritative coursework budget calculator. The total grade is 100:
    ///   60 = Final exam, 40 = Coursework (assignments + quizzes + midterm).
    /// Bonus is per-student and never reserves course-level budget — it is bounded
    /// at student-evaluation time by (40 - earned coursework before bonus).
    ///
    /// Quiz max formula: questions × points-per-question. Archived rows from the
    /// Reset Material flow are excluded so they never inflate the active budget.
    /// </summary>
    public class CourseworkBudgetService : ICourseworkBudgetService
    {
        public const int COURSEWORK_BUDGET = 40;
        private readonly UniversityDbContext _ctx;

        public CourseworkBudgetService(UniversityDbContext ctx) => _ctx = ctx;

        public async Task<CourseworkBudgetDto> GetBudgetAsync(int courseId)
        {
            int aMax = await _ctx.Assignments.AsNoTracking()
                .Where(a => a.CourseId == courseId && !a.IsArchived)
                .SumAsync(a => (int?)a.Points) ?? 0;

            // Quiz total = Questions.Count * GradePerQuestion. Archived quizzes
            // (Reset Material soft-delete) are excluded.
            int qMax = await _ctx.Quizzes.AsNoTracking()
                .Where(q => q.CourseId == courseId && !q.IsArchived)
                .Select(q => q.Questions.Count * (q.GradePerQuestion <= 0 ? 1 : q.GradePerQuestion))
                .SumAsync();

            int mMax = await _ctx.MidtermGrades.AsNoTracking()
                .Where(m => m.CourseId == courseId)
                .Select(m => (int?)m.Max).MaxAsync() ?? 0;

            int used = aMax + qMax + mMax;
            return new CourseworkBudgetDto
            {
                CourseId       = courseId,
                Budget         = COURSEWORK_BUDGET,
                AssignmentMax  = aMax,
                QuizMax        = qMax,
                MidtermMax     = mMax,
                Used           = used,
                Remaining      = Math.Max(0, COURSEWORK_BUDGET - used),
            };
        }

        public async Task<CourseworkBudgetValidation> ValidateAddAssignmentAsync(int courseId, int requestedPoints)
        {
            var b = await GetBudgetAsync(courseId);
            return Build(b, requestedPoints, "assignment");
        }

        public async Task<CourseworkBudgetValidation> ValidateAddQuizAsync(int courseId, int requestedTotalPoints)
        {
            var b = await GetBudgetAsync(courseId);
            return Build(b, requestedTotalPoints, "quiz");
        }

        public async Task<CourseworkBudgetValidation> ValidateUpdateQuizAsync(int courseId, int existingQuizTotalPoints, int newTotalPoints)
        {
            var b = await GetBudgetAsync(courseId);
            // Subtract this quiz's existing footprint (questions × points-per-question)
            // before checking the new total.
            int adjustedUsed = Math.Max(0, b.Used - existingQuizTotalPoints);
            int adjustedRemaining = Math.Max(0, COURSEWORK_BUDGET - adjustedUsed);
            int delta = newTotalPoints;
            bool ok = delta <= adjustedRemaining;
            return new CourseworkBudgetValidation
            {
                Ok        = ok,
                Used      = adjustedUsed,
                Remaining = adjustedRemaining,
                Requested = delta,
                Message   = ok
                    ? string.Empty
                    : BlockMessage(adjustedUsed, adjustedRemaining, delta, "quiz update"),
            };
        }

        public async Task<CourseworkBudgetValidation> ValidateMidtermMaxAsync(int courseId, int requestedMidtermMax)
        {
            // For midterm, the requested value REPLACES the existing midterm cap, it doesn't add to it.
            var b = await GetBudgetAsync(courseId);
            int withoutMidterm = Math.Max(0, b.Used - b.MidtermMax);
            int adjustedRemaining = Math.Max(0, COURSEWORK_BUDGET - withoutMidterm);
            bool ok = requestedMidtermMax <= adjustedRemaining;
            return new CourseworkBudgetValidation
            {
                Ok        = ok,
                Used      = withoutMidterm,
                Remaining = adjustedRemaining,
                Requested = requestedMidtermMax,
                Message   = ok
                    ? string.Empty
                    : BlockMessage(withoutMidterm, adjustedRemaining, requestedMidtermMax, "midterm max"),
            };
        }

        // ──────────────────────────────────────────────────────────
        // helpers
        // ──────────────────────────────────────────────────────────
        private static CourseworkBudgetValidation Build(CourseworkBudgetDto b, int requested, string label)
        {
            bool ok = requested <= b.Remaining;
            return new CourseworkBudgetValidation
            {
                Ok        = ok,
                Used      = b.Used,
                Remaining = b.Remaining,
                Requested = requested,
                Message   = ok ? string.Empty : BlockMessage(b.Used, b.Remaining, requested, label),
            };
        }

        private static string BlockMessage(int used, int remaining, int requested, string label) =>
            $"Cannot add this {label}. Coursework budget is limited to {COURSEWORK_BUDGET} points. " +
            $"Used: {used} / {COURSEWORK_BUDGET}  ·  Remaining: {remaining}  ·  Requested: {requested}";
    }
}
