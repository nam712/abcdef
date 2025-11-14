using System;

namespace YourShopManagement.API.DTOs.Shop
{
    /// <summary>
    /// DTO trả về thông tin cửa hàng
    /// </summary>
    public class ShopDto
    {
        public int ShopId { get; set; }
        public int ShopOwnerId { get; set; }
        public string ShopCode { get; set; } = string.Empty;
        public string ShopName { get; set; } = string.Empty;
        public string? ShopAddress { get; set; }
        public string? ShopPhone { get; set; }
        public string? ShopEmail { get; set; }
        public string? ManagerName { get; set; }
        public string? ManagerPhone { get; set; }
        public int? BusinessCategoryId { get; set; }
        public string? BusinessCategoryName { get; set; }
        public string Status { get; set; } = "active";
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
