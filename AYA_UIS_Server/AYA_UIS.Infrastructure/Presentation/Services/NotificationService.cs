using Abstraction.Contracts;
using AYA_UIS.Core.Domain.Entities.Identity;
using AYA_UIS.Core.Domain.Entities.Models;
using Domain.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Presentation.Hubs;
using Shared.Dtos.Student_Module;

namespace Presentation.Services
{
    /// <summary>
    /// Persists notifications via IUnitOfWork and immediately pushes them
    /// to the target user's SignalR group ("ReceiveNotification" event).
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _uow;
        private readonly IHubContext<NotificationHub> _hub;
        private readonly UserManager<User> _userManager;

        public NotificationService(
            IUnitOfWork uow,
            IHubContext<NotificationHub> hub,
            UserManager<User> userManager)
        {
            _uow = uow;
            _hub = hub;
            _userManager = userManager;
        }

        public async Task SendAsync(Notification n, CancellationToken ct = default)
        {
            n.CreatedAt = DateTime.UtcNow;
            await _uow.Notifications.AddAsync(n);
            await _uow.SaveChangesAsync();

            bool isStudent = await IsStudentAsync(n.UserId);
            await _hub.Clients.Group(n.UserId)
                .SendAsync("ReceiveNotification", MapToDto(n, isStudent), ct);
        }

        public async Task SendManyAsync(IEnumerable<Notification> notifications, CancellationToken ct = default)
        {
            var list = notifications.ToList();
            if (list.Count == 0) return;

            foreach (var n in list)
            {
                n.CreatedAt = DateTime.UtcNow;
                await _uow.Notifications.AddAsync(n);
            }
            await _uow.SaveChangesAsync();

            // Cache role lookups across the batch — many SendMany calls fan out to
            // the same role (e.g. "all students in a course" → all Student).
            var roleCache = new Dictionary<string, bool>();
            foreach (var n in list)
            {
                if (!roleCache.TryGetValue(n.UserId, out var isStudent))
                {
                    isStudent = await IsStudentAsync(n.UserId);
                    roleCache[n.UserId] = isStudent;
                }
                await _hub.Clients.Group(n.UserId)
                    .SendAsync("ReceiveNotification", MapToDto(n, isStudent), ct);
            }
        }

        private async Task<bool> IsStudentAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return false;
            var user = await _userManager.FindByIdAsync(userId);
            return user != null && await _userManager.IsInRoleAsync(user, "Student");
        }

        // Mirrors NotificationController.BuildDetail — student recipients receive
        // null for every navigation-capable id so deep links cannot be constructed.
        private static StudentNotificationDto MapToDto(Notification n, bool isStudent) => new()
        {
            Id     = n.Id,
            Type   = n.Type,
            Title  = n.Title,
            Body   = n.Body,
            IsRead = false,
            Time   = "Just now",
            Detail = new StudentNotificationDetailDto
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
                SubmittedAt     = null,
            },
        };
    }
}
