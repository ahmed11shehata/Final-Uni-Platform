using AYA_UIS.Core.Domain.Entities.Identity;

namespace AYA_UIS.Core.Domain.Entities.Models
{
    /// <summary>
    /// Notification types (frontend keys):
    ///   Student:    grade_approved | grade_rejected | quiz_published | lecture_uploaded | assignment_published | password_changed
    ///   Instructor: submission_new | submission_updated | quiz_ended
    ///   Admin:      gpa_low | student_failed | user_registered | system_alert
    /// </summary>
    public class Notification : BaseEntities<int>
    {
        public string UserId { get; set; } = string.Empty;
        public User? User { get; set; }

        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;

        // ── Assignment context ──
        public int?    AssignmentId    { get; set; }
        public string? AssignmentTitle { get; set; }

        // ── Course context ──
        public int?    CourseId        { get; set; }
        public string? CourseName      { get; set; }

        // ── Quiz context ──
        public int?    QuizId          { get; set; }
        public string? QuizTitle       { get; set; }

        // ── Lecture context ──
        public int?    LectureId       { get; set; }
        public string? LectureTitle    { get; set; }

        // ── Grade context ──
        public int? Grade          { get; set; }
        public int? Max            { get; set; }
        public string? RejectionReason { get; set; }

        // ── Student context (instructor / admin notifications) ──
        public string? StudentName     { get; set; }
        public string? StudentCode     { get; set; }
        public string? TargetStudentId { get; set; }   // GUID — for admin → ManageUsers deep-link

        // ── Instructor context (student notifications) ──
        public string? InstructorName  { get; set; }

        public bool     IsRead    { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
