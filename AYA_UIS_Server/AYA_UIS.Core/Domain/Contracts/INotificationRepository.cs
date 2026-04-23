using AYA_UIS.Core.Domain.Entities.Models;

namespace AYA_UIS.Core.Domain.Contracts
{
    public interface INotificationRepository
    {
        Task<List<Notification>> GetForUserAsync(string userId);
        Task<int>                GetUnreadCountAsync(string userId);
        Task                     AddAsync(Notification notification);
        Task                     MarkReadAsync(int notificationId, string userId);
        Task                     MarkAllReadAsync(string userId);
        Task                     DeleteOlderThanAsync(DateTime cutoff);
    }
}
