using Backend.Models;
using Microsoft.EntityFrameworkCore;
using YourShopManagement.API.Data;

namespace Backend.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly ApplicationDbContext _context;

        public NotificationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Notification>> GetAllAsync(int? userId = null, bool? isRead = null, int limit = 50)
        {
            var query = _context.Notifications.AsQueryable();

            if (userId.HasValue)
            {
                query = query.Where(n => n.UserId == userId.Value || n.UserId == null);
            }

            if (isRead.HasValue)
            {
                query = query.Where(n => n.IsRead == isRead.Value);
            }

            return await query
                .OrderByDescending(n => n.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<Notification?> GetByIdAsync(int id)
        {
            return await _context.Notifications.FindAsync(id);
        }

        public async Task<Notification> CreateAsync(Notification notification)
        {
            notification.CreatedAt = DateTime.UtcNow;
            notification.IsRead = false;

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            return notification;
        }

        public async Task<Notification?> UpdateAsync(int id, Notification notification)
        {
            var existing = await _context.Notifications.FindAsync(id);
            if (existing == null) return null;

            existing.Message = notification.Message;
            existing.Type = notification.Type;
            existing.IsRead = notification.IsRead;
            existing.EntityType = notification.EntityType;
            existing.EntityId = notification.EntityId;
            existing.Action = notification.Action;
            existing.Metadata = notification.Metadata;

            if (notification.IsRead && !existing.ReadAt.HasValue)
            {
                existing.ReadAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null) return false;

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetUnreadCountAsync(int? userId = null)
        {
            var query = _context.Notifications.Where(n => !n.IsRead);

            if (userId.HasValue)
            {
                query = query.Where(n => n.UserId == userId.Value || n.UserId == null);
            }

            return await query.CountAsync();
        }

        public async Task<bool> MarkAsReadAsync(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null) return false;

            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkAllAsReadAsync(int? userId = null)
        {
            var query = _context.Notifications.Where(n => !n.IsRead);

            if (userId.HasValue)
            {
                query = query.Where(n => n.UserId == userId.Value || n.UserId == null);
            }

            var notifications = await query.ToListAsync();
            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAllAsync(int? userId = null)
        {
            var query = _context.Notifications.AsQueryable();

            if (userId.HasValue)
            {
                query = query.Where(n => n.UserId == userId.Value || n.UserId == null);
            }

            var notifications = await query.ToListAsync();
            _context.Notifications.RemoveRange(notifications);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
