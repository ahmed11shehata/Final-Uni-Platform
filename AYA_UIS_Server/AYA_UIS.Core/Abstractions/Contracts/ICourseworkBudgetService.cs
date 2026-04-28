using Shared.Dtos.Instructor_Module;

namespace Abstraction.Contracts
{
    /// <summary>
    /// Single source of truth for the per-course coursework budget.
    /// Total = 100 (Final 60 + Coursework 40). Coursework caps for a course are:
    ///   sum(Assignments.Points where !IsArchived)
    ///   + sum(Quizzes.Questions.Count * Quizzes.GradePerQuestion where !IsArchived)
    ///   + max(MidtermGrade.Max for this course, 0)
    /// All create/update paths that affect any of those numbers must validate
    /// here so the backend is authoritative even if the frontend is bypassed.
    /// Archived rows from Reset Material are excluded from the active budget.
    /// </summary>
    public interface ICourseworkBudgetService
    {
        Task<CourseworkBudgetDto> GetBudgetAsync(int courseId);

        /// <summary>True if adding this many assignment points would still fit ≤ 40.</summary>
        Task<CourseworkBudgetValidation> ValidateAddAssignmentAsync(int courseId, int requestedPoints);

        /// <summary>True if adding a new quiz with this total point value (questions × points-per-question) fits ≤ 40.</summary>
        Task<CourseworkBudgetValidation> ValidateAddQuizAsync(int courseId, int requestedTotalPoints);

        /// <summary>
        /// True if changing an existing quiz's total point value (questions × points-per-question) still fits ≤ 40.
        /// Pass the existing quiz's previous total points so it is subtracted before validating the new value.
        /// </summary>
        Task<CourseworkBudgetValidation> ValidateUpdateQuizAsync(int courseId, int existingQuizTotalPoints, int newTotalPoints);

        /// <summary>True if setting midterm Max to this value still fits ≤ 40.</summary>
        Task<CourseworkBudgetValidation> ValidateMidtermMaxAsync(int courseId, int requestedMidtermMax);
    }

    public sealed class CourseworkBudgetValidation
    {
        public bool   Ok        { get; init; }
        public int    Used      { get; init; }
        public int    Remaining { get; init; }
        public int    Requested { get; init; }
        public string Message   { get; init; } = string.Empty;
    }
}
