namespace Shared.Dtos.Student_Module
{
    public class FullCourseDetailDto
    {
        public CourseMetaDto Meta { get; set; } = new();
        public List<CourseLectureDto> Lectures { get; set; } = new();
        public List<CourseAssignmentDto> Assignments { get; set; } = new();
        public List<CourseQuizSummaryDto> Quizzes { get; set; } = new();
        public CourseMidtermDto? Midterm { get; set; }
    }

    /// <summary>
    /// Matches frontend CourseDetailPage meta shape.
    /// </summary>
    public class CourseMetaDto
    {
        public string Id { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Instructor { get; set; } = string.Empty;
        public int Level { get; set; }
        public int Credits { get; set; }
        public string Semester { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Shade { get; set; } = string.Empty;
        public string Light { get; set; } = string.Empty;
        public int Progress { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
