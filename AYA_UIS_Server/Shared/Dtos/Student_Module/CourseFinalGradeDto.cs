namespace Shared.Dtos.Student_Module
{
    /// <summary>
    /// Final-grade data returned to a student only when Published == true.
    /// The Published flag is enforced server-side; if this object is non-null
    /// the grade has been published and the student is allowed to see it.
    /// </summary>
    public class CourseFinalGradeDto
    {
        /// <summary>Instructor-entered final exam score (0–60).</summary>
        public int FinalScore { get; set; }
        /// <summary>Capped coursework total: min(40, midterm+quiz+asn+bonus).</summary>
        public decimal CourseworkTotal { get; set; }
        /// <summary>CourseworkTotal + FinalScore (0–100).</summary>
        public decimal Total { get; set; }
        /// <summary>Letter grade derived from Total: A / B / C / D / F.</summary>
        public string LetterGrade { get; set; } = string.Empty;
    }
}
