using System.Security.Claims;
using Domain.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Shared.Dtos.Student_Module;

namespace Presentation.Controllers
{
    /// <summary>
    /// Shared notification endpoints — accessible to Student, Instructor, and Admin roles.
    /// All queries are scoped to the currently authenticated user.
    /// </summary>
    [ApiController]
    [Route("api/notifications")]
    [Authorize]
    [EnableRateLimiting("PolicyLimitRate")]
    public class NotificationController : ControllerBase
    {
        private readonly IUnitOfWork _uow;

        public NotificationController(IUnitOfWork uow) => _uow = uow;

        private string? UserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

        // ──────────────────────────────────────────────────────────────
        //  GET /api/notifications
        // ──────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var uid = UserId;
            if (string.IsNullOrEmpty(uid))
                return Unauthorized();

            var list = await _uow.Notifications.GetForUserAsync(uid);

            // Students must never receive deep-link navigation payloads — even on
            // legacy rows that already have CourseId/AssignmentId/QuizId/LectureId
            // stamped. Display-only fields (CourseName, AssignmentTitle, etc.) stay.
            // Admin/Instructor notifications retain their full detail.
            bool isStudent = User.IsInRole("Student");

            var dto = list.Select(n => new StudentNotificationDto
            {
                Id     = n.Id,
                Type   = n.Type,
                Title  = n.Title,
                Body   = n.Body,
                IsRead = n.IsRead,
                Time   = FormatTimeAgo(n.CreatedAt),
                Detail = BuildDetail(n, isStudent),
            }).ToList();

            return Ok(new { success = true, data = dto });
        }

        // ──────────────────────────────────────────────────────────────
        //  GET /api/notifications/unread-count
        // ──────────────────────────────────────────────────────────────
        [HttpGet("unread-count")]
        public async Task<IActionResult> UnreadCount()
        {
            var uid = UserId;
            if (string.IsNullOrEmpty(uid)) return Unauthorized();
            var count = await _uow.Notifications.GetUnreadCountAsync(uid);
            return Ok(new { success = true, data = new { count } });
        }

        // ──────────────────────────────────────────────────────────────
        //  PUT /api/notifications/{id}/read
        // ──────────────────────────────────────────────────────────────
        [HttpPut("{id:int}/read")]
        public async Task<IActionResult> MarkRead(int id)
        {
            var uid = UserId;
            if (string.IsNullOrEmpty(uid)) return Unauthorized();
            await _uow.Notifications.MarkReadAsync(id, uid);
            await _uow.SaveChangesAsync();
            return Ok(new { success = true });
        }

        // ──────────────────────────────────────────────────────────────
        //  PUT /api/notifications/read-all
        // ──────────────────────────────────────────────────────────────
        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllRead()
        {
            var uid = UserId;
            if (string.IsNullOrEmpty(uid)) return Unauthorized();
            await _uow.Notifications.MarkAllReadAsync(uid);
            await _uow.SaveChangesAsync();
            return Ok(new { success = true });
        }

        // ──────────────────────────────────────────────────────────────
        //  Helpers
        // ──────────────────────────────────────────────────────────────
        // When isStudent==true, every navigation-capable id is nulled so a student
        // client cannot construct a deep-link route, even for old DB rows that were
        // saved with the ids populated. Title/name fields stay so the list/detail
        // popup still renders meaningful labels.
        private static StudentNotificationDetailDto BuildDetail(
            AYA_UIS.Core.Domain.Entities.Models.Notification n, bool isStudent) => new()
        {
            CourseName      = n.CourseName,
            CourseId        = isStudent ? null : n.CourseId,
            AssignmentTitle = n.AssignmentTitle,
            AssignmentId    = isStudent ? null : n.AssignmentId,
            Grade           = n.Grade,
            Max             = n.Max,
            RejectionReason = n.RejectionReason,
            QuizTitle       = n.QuizTitle,
            QuizId          = isStudent ? null : n.QuizId,
            LectureTitle    = n.LectureTitle,
            LectureId       = isStudent ? null : n.LectureId,
            InstructorName  = n.InstructorName,
            StudentName     = n.StudentName,
            StudentCode     = n.StudentCode,
            TargetStudentId = isStudent ? null : n.TargetStudentId,
        };

        internal static string FormatTimeAgo(DateTime dt)
        {
            var diff = DateTime.UtcNow - dt;
            if (diff.TotalMinutes < 1)  return "Just now";
            if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes} min ago";
            if (diff.TotalHours   < 24) return $"{(int)diff.TotalHours} hr ago";
            if (diff.TotalDays    < 2)  return "Yesterday";
            return dt.ToString("MMM d");
        }
    }
}
