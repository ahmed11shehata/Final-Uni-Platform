namespace Shared.Dtos.Instructor_Module
{
    public class SubmissionDto
    {
        public string Id { get; set; } = string.Empty;
        public string AssignmentId { get; set; } = string.Empty;
        public string AssignmentTitle { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string SubmittedAt { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string? FileUrl { get; set; }
        public string Status { get; set; } = string.Empty;
        public int? Grade { get; set; }
        public int MaxGrade { get; set; }
        public string? Feedback { get; set; }
        public string? RejectionReason { get; set; }
    }

    public class GradeSubmissionDto
    {
        public int Grade { get; set; }
        public string Feedback { get; set; } = string.Empty;
    }

    public class ApproveSubmissionDto
    {
        public int Grade { get; set; }
    }

    public class RejectSubmissionDto
    {
        public string Reason { get; set; } = string.Empty;
    }
}
