namespace Shared.Dtos.Instructor_Module
{
    public class ExamGradesDto
    {
        public string ExamType { get; set; } = string.Empty;
        public List<StudentExamGradeDto> Grades { get; set; } = new();
    }

    public class StudentExamGradeDto
    {
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public int? Grade { get; set; }
        public int MaxGrade { get; set; }
        public bool Submitted { get; set; }
    }

    public class PostExamGradeDto
    {
        public string ExamType { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public int Grade { get; set; }
    }

    public class SetMidtermGradeDto
    {
        /// <summary>Grade value (0..Max)</summary>
        public int Grade { get; set; }
        /// <summary>Max mark for this midterm (instructor-defined, max 40)</summary>
        public int Max { get; set; }
        /// <summary>Publish to student immediately</summary>
        public bool Published { get; set; } = false;
    }

    // ── Final Grade ──────────────────────────────────────────────────────────

    public class FinalGradeStatusDto
    {
        public bool Locked { get; set; }
        /// <summary>ISO date string of the final exam, null if no schedule found.</summary>
        public string? ExamDate { get; set; }
        /// <summary>Human-readable exam time range, e.g. "09:00 – 11:00".</summary>
        public string? ExamTime { get; set; }
        /// <summary>ISO datetime when grading unlocks (exam end + 24 h), null if no schedule.</summary>
        public string? UnlockAt { get; set; }
    }

    public class FinalGradeStudentDto
    {
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        /// <summary>Stored midterm grade value.</summary>
        public int MidtermGrade { get; set; }
        /// <summary>Max the instructor set for the midterm.</summary>
        public int MidtermMax { get; set; }
        /// <summary>Sum of student's quiz attempt scores for this course.</summary>
        public decimal QuizScore { get; set; }
        /// <summary>Sum of student's accepted assignment grades for this course.</summary>
        public decimal AssignmentScore { get; set; }
        /// <summary>Stored bonus (0–10).</summary>
        public int Bonus { get; set; }
        /// <summary>Capped coursework total: min(40, midterm+quiz+asn+bonus).</summary>
        public decimal CourseworkTotal { get; set; }
        /// <summary>Stored final exam score (0–60), null if not yet saved.</summary>
        public int? FinalScore { get; set; }
        /// <summary>CourseworkTotal + FinalScore, null if final not saved.</summary>
        public decimal? Total { get; set; }
        /// <summary>Letter grade derived from Total.</summary>
        public string? LetterGrade { get; set; }
        public bool Submitted { get; set; }
    }

    public class SetFinalGradeDto
    {
        /// <summary>Final exam score (0–60).</summary>
        public int FinalScore { get; set; }
        /// <summary>Optional bonus (0–10).</summary>
        public int Bonus { get; set; } = 0;
    }
}
