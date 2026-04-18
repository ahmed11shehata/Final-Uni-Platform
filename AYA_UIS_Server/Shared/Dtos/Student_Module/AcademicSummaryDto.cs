namespace Shared.Dtos.Student_Module
{
    public class AcademicSummaryDto
    {
        public decimal Gpa { get; set; }
        public int CompletedCredits { get; set; }
        public int RemainingCredits { get; set; }
        public int CurrentYear { get; set; }
        public string Standing { get; set; } = string.Empty;
        public int CoursesThisSemester { get; set; }
        public int ActiveAssignments { get; set; }
        public int UpcomingQuizzes { get; set; }
        public string NextDeadline { get; set; } = string.Empty;
        public int OverallRank { get; set; }
        public int TotalStudents { get; set; }
    }
}
