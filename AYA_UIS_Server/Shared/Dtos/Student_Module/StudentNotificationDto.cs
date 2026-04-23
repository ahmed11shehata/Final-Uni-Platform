namespace Shared.Dtos.Student_Module
{
    public class StudentNotificationDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public string Time { get; set; } = string.Empty;

        public StudentNotificationDetailDto? Detail { get; set; }
    }

    public class StudentNotificationDetailDto
    {
        // Assignment
        public string? CourseName      { get; set; }
        public string? AssignmentTitle { get; set; }
        public int?    AssignmentId    { get; set; }
        public int?    Grade           { get; set; }
        public int?    Max             { get; set; }
        public string? RejectionReason { get; set; }

        // Quiz
        public string? QuizTitle { get; set; }
        public int?    QuizId    { get; set; }

        // Lecture
        public string? LectureTitle { get; set; }
        public int?    LectureId   { get; set; }

        // Course deep-link
        public int? CourseId { get; set; }

        // Instructor context (for student notifications)
        public string? InstructorName { get; set; }

        // Student context (for instructor / admin notifications)
        public string? StudentName     { get; set; }
        public string? StudentCode     { get; set; }
        public string? TargetStudentId { get; set; }
        public string? SubmittedAt     { get; set; }
    }
}
