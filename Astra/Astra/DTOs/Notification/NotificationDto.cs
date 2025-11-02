using System;

namespace Backend.DTOs.Notification
{
    public class NotificationDto
    {
        public int NotificationId { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = "info";
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public int? UserId { get; set; }
        public string? EntityType { get; set; }
        public int? EntityId { get; set; }
        public string? Action { get; set; }
        public string? Metadata { get; set; }
    }

    public class CreateNotificationDto
    {
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = "info"; // info, warning, success, error
        public int? UserId { get; set; }
        public string? EntityType { get; set; }
        public int? EntityId { get; set; }
        public string? Action { get; set; }
        public string? Metadata { get; set; }
    }

    public class UpdateNotificationDto
    {
        public bool IsRead { get; set; }
    }

    public class NotificationStatsDto
    {
        public int TotalCount { get; set; }
        public int UnreadCount { get; set; }
        public int ReadCount { get; set; }
    }
}
