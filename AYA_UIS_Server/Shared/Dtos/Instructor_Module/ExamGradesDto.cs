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
}
