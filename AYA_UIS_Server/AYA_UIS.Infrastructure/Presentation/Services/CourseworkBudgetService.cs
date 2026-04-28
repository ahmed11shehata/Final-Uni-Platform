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

            // Quiz total = questions × GradePerQuestion. Archived quizzes excluded.
            // Two flat queries + C# sum avoid the SQL Server
            // "aggregate on subquery" error that SUM(COUNT subquery) triggers.
            var activeQuizzes = await _ctx.Quizzes.AsNoTracking()
                .Where(q => q.CourseId == courseId && !q.IsArchived)
                .Select(q => new { q.Id, q.GradePerQuestion })
                .ToListAsync();

            decimal qMax = 0m;
            if (activeQuizzes.Count > 0)
            {
                var activeIds = activeQuizzes.Select(q => q.Id).ToList();

                var qCounts = await _ctx.QuizQuestions.AsNoTracking()
                    .Where(qq => activeIds.Contains(qq.QuizId))
                    .GroupBy(qq => qq.QuizId)
                    .Select(g => new { QuizId = g.Key, Count = g.Count() })
                    .ToListAsync();

                var countByQuizId = qCounts.ToDictionary(x => x.QuizId, x => x.Count);

                qMax = activeQuizzes.Sum(q =>
                {
                    int cnt        = countByQuizId.TryGetValue(q.Id, out var c) ? c : 0;
                    decimal grade  = q.GradePerQuestion <= 0m ? 1m : q.GradePerQuestion;
                    return cnt * grade;
                });
            }

            int mMax = await _ctx.MidtermGrades.AsNoTracking()
                .Where(m => m.CourseId == courseId)
                .Select(m => (int?)m.Max).MaxAsync() ?? 0;

            decimal used = aMax + qMax + mMax;
            return new CourseworkBudgetDto
            {
                CourseId       = courseId,
                Budget         = COURSEWORK_BUDGET,
                AssignmentMax  = aMax,
                QuizMax        = qMax,
                MidtermMax     = mMax,
                Used           = used,
                Remaining      = Math.Max(0m, COURSEWORK_BUDGET - used),
            };
        }

        public async Task<CourseworkBudgetValidation> ValidateAddAssignmentAsync(int courseId, int requestedPoints)
        {
            var b = await GetBudgetAsync(courseId);
            return Build(b, requestedPoints, "assignment");
        }

        public async Task<CourseworkBudgetValidation> ValidateAddQuizAsync(int courseId, decimal requestedTotalPoints)
        {
            var b = await GetBudgetAsync(courseId);
            return Build(b, requestedTotalPoints, "quiz");
        }

        public async Task<CourseworkBudgetValidation> ValidateUpdateQuizAsync(int courseId, decimal existingQuizTotalPoints, decimal newTotalPoints)
        {
            var b = await GetBudgetAsync(courseId);
            // Subtract this quiz's existing footprint (questions × points-per-question)
            // before checking the new total.
            decimal adjustedUsed = Math.Max(0m, b.Used - existingQuizTotalPoints);
            decimal adjustedRemaining = Math.Max(0m, COURSEWORK_BUDGET - adjustedUsed);
            decimal delta = newTotalPoints;
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
            decimal withoutMidterm = Math.Max(0m, b.Used - b.MidtermMax);
            decimal adjustedRemaining = Math.Max(0m, COURSEWORK_BUDGET - withoutMidterm);
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
        private static CourseworkBudgetValidation Build(CourseworkBudgetDto b, decimal requested, string label)
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

        private static string BlockMessage(decimal used, decimal remaining, decimal requested, string label) =>
            $"Cannot add this {label}. Coursework budget is limited to {COURSEWORK_BUDGET} points. " +
            $"Used: {used} / {COURSEWORK_BUDGET}  ·  Remaining: {remaining}  ·  Requested: {requested}";
    }
}
