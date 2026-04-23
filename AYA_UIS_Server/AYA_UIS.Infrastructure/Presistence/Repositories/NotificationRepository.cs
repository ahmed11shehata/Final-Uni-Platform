using AYA_UIS.Core.Domain.Contracts;
using AYA_UIS.Core.Domain.Entities.Models;
using Microsoft.EntityFrameworkCore;
using Presistence;

namespace Presistence.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly UniversityDbContext _ctx;

        public NotificationRepository(UniversityDbContext ctx) => _ctx = ctx;

        public async Task<List<Notification>> GetForUserAsync(string userId)
            => await _ctx.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .AsNoTracking()
                .ToListAsync();

        public async Task AddAsync(Notification notification)
            => await _ctx.Notifications.AddAsync(notification);

        public async Task MarkReadAsync(int notificationId, string userId)
        {
            var n = await _ctx.Notifications
                .FirstOrDefaultAsync(x => x.Id == notificationId && x.UserId == userId);
            if (n != null) { n.IsRead = true; }
        }

        public async Task MarkAllReadAsync(string userId)
        {
            var list = await _ctx.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();
            foreach (var n in list) n.IsRead = true;
        }

        public async Task<int> GetUnreadCountAsync(string userId)
            => await _ctx.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);

        public async Task DeleteOlderThanAsync(DateTime cutoff)
        {
            var old = await _ctx.Notifications
                .Where(n => n.CreatedAt < cutoff)
                .ToListAsync();
            if (old.Count > 0)
            {
                _ctx.Notifications.RemoveRange(old);
                await _ctx.SaveChangesAsync();
            }
        }
    }
}
