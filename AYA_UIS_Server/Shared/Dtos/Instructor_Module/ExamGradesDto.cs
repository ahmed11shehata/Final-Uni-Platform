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
        /// <summary>Public academic code the instructor types in the search box.</summary>
        public string StudentCode { get; set; } = string.Empty;
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
        /// <summary>Public academic code the instructor types in the search box.</summary>
        public string StudentCode { get; set; } = string.Empty;
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
        /// <summary>
        /// Assignments whose deadline has not yet passed and the student hasn't submitted.
        /// Treated as "pending" — they do not count as zero yet.
        /// </summary>
        public int PendingAssignments { get; set; }
        /// <summary>
        /// Quizzes whose end time has not yet passed and the student hasn't attempted.
        /// Treated as "pending" — they do not count as zero yet.
        /// </summary>
        public int PendingQuizzes { get; set; }
        /// <summary>
        /// Assignments past deadline with no Accepted submission — counted as explicit 0.
        /// </summary>
        public int MissedAssignments { get; set; }
        /// <summary>
        /// Quizzes past end time with no attempt — counted as explicit 0.
        /// </summary>
        public int MissedQuizzes { get; set; }
    }

    public class SetFinalGradeDto
    {
        /// <summary>Final exam score (0–60). Backend rejects anything outside this range.</summary>
        public int FinalScore { get; set; }
        /// <summary>
        /// Optional bonus (0–40). Backend further clamps it per-student to fill the gap up to 40
        /// of coursework, so it can never push a student's coursework over the 40-point cap.
        /// </summary>
        public int Bonus { get; set; } = 0;
    }
}
