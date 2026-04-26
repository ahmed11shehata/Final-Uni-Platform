namespace Shared.Dtos.Admin_Module
{
    /// <summary>Top-level response for GET /api/admin/final-grade/student/{code}</summary>
    public class AdminFinalGradeStudentDto
    {
        public string StudentId   { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string StudentCode { get; set; } = string.Empty;
        public List<AdminFinalGradeCourseDto> Courses { get; set; } = new();
    }

    /// <summary>One course entry in the final-grade audit view.</summary>
    public class AdminFinalGradeCourseDto
    {
        public int    CourseId      { get; set; }
        public string CourseCode    { get; set; } = string.Empty;
        public string CourseName    { get; set; } = string.Empty;

        // ── Coursework breakdown ──────────────────────────────
        public int     MidtermGrade    { get; set; }
        public int     MidtermMax      { get; set; }
        public decimal QuizScore       { get; set; }
        public decimal AssignmentScore { get; set; }
        public int     Bonus           { get; set; }

        /// <summary>min(40, midterm + quiz + asn + bonus)</summary>
        public decimal CourseworkTotal { get; set; }

        // ── Final grade ───────────────────────────────────────
        /// <summary>null = not yet assigned by instructor.</summary>
        public int?     FinalScore  { get; set; }
        /// <summary>null when FinalScore is null.</summary>
        public decimal? Total       { get; set; }
        /// <summary>A/B/C/D/F — null when not yet assigned.</summary>
        public string?  LetterGrade { get; set; }

        public bool Assigned  { get; set; }   // instructor saved a FinalGrade record
        public bool Published { get; set; }   // admin has published to student
    }

    /// <summary>Request body for POST /api/admin/final-grade/publish/{studentId}</summary>
    public class AdminPublishFinalGradeDto
    {
        /// <summary>If null, publish all assigned courses. Otherwise only the listed courseIds.</summary>
        public List<int>? CourseIds { get; set; }
    }

    /// <summary>One student entry inside a classification tab popup.</summary>
    public class AdminFinalGradeReviewStudentDto
    {
        public string StudentId          { get; set; } = string.Empty;
        public string StudentName        { get; set; } = string.Empty;
        public string StudentCode        { get; set; } = string.Empty;
        /// <summary>"First" | "Second" | "Third" | "Fourth"</summary>
        public string AcademicYear       { get; set; } = string.Empty;
        public int    RegisteredCourses  { get; set; }
        /// <summary>"progress" | "not_completed" | "completed"</summary>
        public string Status             { get; set; } = "progress";
    }

    /// <summary>Aggregated response for GET /api/admin/final-grade/students.</summary>
    public class AdminFinalGradeReviewListDto
    {
        public List<AdminFinalGradeReviewStudentDto> Progress     { get; set; } = new();
        public List<AdminFinalGradeReviewStudentDto> NotCompleted { get; set; } = new();
        public List<AdminFinalGradeReviewStudentDto> Completed    { get; set; } = new();
        public int Total              { get; set; }
        public bool CanPublishAll     { get; set; }
    }

    /// <summary>Request body for POST /api/admin/final-grade/classify/{studentId}</summary>
    public class AdminClassifyStudentDto
    {
        /// <summary>"progress" | "not_completed" | "completed"</summary>
        public string Status { get; set; } = "progress";
    }
}
