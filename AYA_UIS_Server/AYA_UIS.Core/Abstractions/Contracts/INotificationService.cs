using AYA_UIS.Core.Domain.Entities.Models;

namespace Abstraction.Contracts
{
    /// <summary>
    /// Creates, persists, and real-time-pushes notifications via SignalR.
    /// Use SendAsync for single notifications and SendManyAsync for bulk fan-out.
    /// </summary>
    public interface INotificationService
    {
        /// <summary>Persist one notification and push it via SignalR to the target user.</summary>
        Task SendAsync(Notification notification, CancellationToken ct = default);

        /// <summary>Persist many notifications (single SaveChanges) and push each via SignalR.</summary>
        Task SendManyAsync(IEnumerable<Notification> notifications, CancellationToken ct = default);
    }
}
