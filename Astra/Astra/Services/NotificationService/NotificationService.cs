using Backend.DTOs.Notification;
using Backend.Models;
using Backend.Repositories;

namespace Backend.Services.NotificationService
{
    public interface INotificationService
    {
        Task<IEnumerable<NotificationDto>> GetAllNotificationsAsync(int? userId = null, bool? isRead = null, int limit = 50);
        Task<NotificationDto?> GetNotificationByIdAsync(int id);
        Task<NotificationDto> CreateNotificationAsync(CreateNotificationDto dto);
        Task<NotificationDto?> UpdateNotificationAsync(int id, UpdateNotificationDto dto);
        Task<bool> DeleteNotificationAsync(int id);
        Task<NotificationStatsDto> GetNotificationStatsAsync(int? userId = null);
        Task<bool> MarkAsReadAsync(int id);
        Task<bool> MarkAllAsReadAsync(int? userId = null);
        Task<bool> DeleteAllNotificationsAsync(int? userId = null);
    }

    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _repository;

        public NotificationService(INotificationRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<NotificationDto>> GetAllNotificationsAsync(int? userId = null, bool? isRead = null, int limit = 50)
        {
            var notifications = await _repository.GetAllAsync(userId, isRead, limit);
            return notifications.Select(MapToDto);
        }

        public async Task<NotificationDto?> GetNotificationByIdAsync(int id)
        {
            var notification = await _repository.GetByIdAsync(id);
            return notification == null ? null : MapToDto(notification);
        }

        public async Task<NotificationDto> CreateNotificationAsync(CreateNotificationDto dto)
        {
            var notification = new Notification
            {
                Message = dto.Message,
                Type = dto.Type,
                UserId = dto.UserId,
                EntityType = dto.EntityType,
                EntityId = dto.EntityId,
                Action = dto.Action,
                Metadata = dto.Metadata
            };

            var created = await _repository.CreateAsync(notification);
            return MapToDto(created);
        }

        public async Task<NotificationDto?> UpdateNotificationAsync(int id, UpdateNotificationDto dto)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return null;

            existing.IsRead = dto.IsRead;
            if (dto.IsRead && !existing.ReadAt.HasValue)
            {
                existing.ReadAt = DateTime.UtcNow;
            }

            var updated = await _repository.UpdateAsync(id, existing);
            return updated == null ? null : MapToDto(updated);
        }

        public async Task<bool> DeleteNotificationAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }

        public async Task<NotificationStatsDto> GetNotificationStatsAsync(int? userId = null)
        {
            var all = await _repository.GetAllAsync(userId);
            var unreadCount = await _repository.GetUnreadCountAsync(userId);

            return new NotificationStatsDto
            {
                TotalCount = all.Count(),
                UnreadCount = unreadCount,
                ReadCount = all.Count() - unreadCount
            };
        }

        public async Task<bool> MarkAsReadAsync(int id)
        {
            return await _repository.MarkAsReadAsync(id);
        }

        public async Task<bool> MarkAllAsReadAsync(int? userId = null)
        {
            return await _repository.MarkAllAsReadAsync(userId);
        }

        public async Task<bool> DeleteAllNotificationsAsync(int? userId = null)
        {
            return await _repository.DeleteAllAsync(userId);
        }

        private NotificationDto MapToDto(Notification notification)
        {
            return new NotificationDto
            {
                NotificationId = notification.NotificationId,
                Message = notification.Message,
                Type = notification.Type,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt,
                ReadAt = notification.ReadAt,
                UserId = notification.UserId,
                EntityType = notification.EntityType,
                EntityId = notification.EntityId,
                Action = notification.Action,
                Metadata = notification.Metadata
            };
        }
    }
}
