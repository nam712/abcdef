namespace YourShopManagement.API.DTOs.Promotion
{
    public class PromotionDto
    {
        public int PromotionId { get; set; }
        public string PromotionCode { get; set; } = string.Empty;
        public string PromotionName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string PromotionType { get; set; } = string.Empty;
        public decimal DiscountValue { get; set; }
        public decimal? MinPurchaseAmount { get; set; }
        public decimal? MaxDiscountAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public int? UsageLimit { get; set; }
        public int UsageCount { get; set; }
        public string? ApplicableProducts { get; set; }
        public string? ApplicableCustomers { get; set; }
        public int ShopOwnerId { get; set; }
        public int? InvoiceId { get; set; }
        public string? InvoiceCode { get; set; } // From Invoice navigation
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
