namespace Shared.Dtos.Student_Module
{
    public class StudentTranscriptDto
    {
        public decimal Gpa { get; set; }
        public int TotalCredits { get; set; }
        /// <summary>
        /// Dictionary mapping year (as string) to semester data
        /// Example: { "1": { Fall: 3.5, Spring: 3.4 }, "2": { Fall: 3.6, Spring: null } }
        /// </summary>
        public Dictionary<string, YearSemesterDto>? Years { get; set; }
    }

    public class YearSemesterDto
    {
        /// <summary>
        /// GPA for Fall semester (null if not completed)
        /// </summary>
        public decimal? Fall { get; set; }

        /// <summary>
        /// GPA for Spring semester (null if not completed)
        /// </summary>
        public decimal? Spring { get; set; }
    }

    public class YearSemesterDataDto
    {
        public decimal Gpa { get; set; }
        public int Credits { get; set; }
        public List<CourseGradeDto> Courses { get; set; } = new();
    }

    public class CourseGradeDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Grade { get; set; }
        public int Max { get; set; }
        public string Letter { get; set; } = string.Empty;
        public decimal Points { get; set; }
        public int Credits { get; set; }
    }
}
