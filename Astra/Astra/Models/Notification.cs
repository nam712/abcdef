using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using YourShopManagement.API.Models; // để truy cập ShopOwner

namespace Backend.Models
{
    [Table("notifications")]
    public class Notification
    {
        [Key]
        [Column("notification_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int NotificationId { get; set; }

        [Required]
        [Column("message")]
        [StringLength(500)]
        public string Message { get; set; } = string.Empty;

        [Required]
        [Column("type")]
        [StringLength(20)]
        public string Type { get; set; } = "info"; // info, warning, success, error

        [Required]
        [Column("is_read")]
        public bool IsRead { get; set; } = false;

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("read_at")]
        public DateTime? ReadAt { get; set; }

        [Column("user_id")]
        public int? UserId { get; set; }

        [Column("entity_type")]
        [StringLength(50)]
        public string? EntityType { get; set; } // Product, Customer, Employee, etc.

        [Column("entity_id")]
        public int? EntityId { get; set; }

        [Column("action")]
        [StringLength(50)]
        public string? Action { get; set; } // Create, Update, Delete

        [Column("metadata")]
        [StringLength(1000)]
        public string? Metadata { get; set; } // JSON data for additional info

        // ✅ Thêm chủ shop
        [Required]
        [Column("shop_owner_id")]
        public int ShopOwnerId { get; set; }

        // ✅ Navigation property
        [ForeignKey("ShopOwnerId")]
        public virtual ShopOwner ShopOwner { get; set; } = null!;
    }
}