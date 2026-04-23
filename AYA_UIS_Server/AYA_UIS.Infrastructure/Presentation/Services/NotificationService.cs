using Abstraction.Contracts;
using AYA_UIS.Core.Domain.Entities.Models;
using Domain.Contracts;
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

        public NotificationService(IUnitOfWork uow, IHubContext<NotificationHub> hub)
        {
            _uow = uow;
            _hub = hub;
        }

        public async Task SendAsync(Notification n, CancellationToken ct = default)
        {
            n.CreatedAt = DateTime.UtcNow;
            await _uow.Notifications.AddAsync(n);
            await _uow.SaveChangesAsync();
            await _hub.Clients.Group(n.UserId)
                .SendAsync("ReceiveNotification", MapToDto(n), ct);
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

            foreach (var n in list)
                await _hub.Clients.Group(n.UserId)
                    .SendAsync("ReceiveNotification", MapToDto(n), ct);
        }

        private static StudentNotificationDto MapToDto(Notification n) => new()
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
                CourseId        = n.CourseId,
                AssignmentTitle = n.AssignmentTitle,
                AssignmentId    = n.AssignmentId,
                Grade           = n.Grade,
                Max             = n.Max,
                RejectionReason = n.RejectionReason,
                QuizTitle       = n.QuizTitle,
                QuizId          = n.QuizId,
                LectureTitle    = n.LectureTitle,
                LectureId       = n.LectureId,
                InstructorName  = n.InstructorName,
                StudentName     = n.StudentName,
                StudentCode     = n.StudentCode,
                TargetStudentId = n.TargetStudentId,
                SubmittedAt     = null,
            },
        };
    }
}
