using System.ComponentModel.DataAnnotations;

namespace YourShopManagement.API.DTOs.Promotion
{
    public class CreatePromotionDto
    {
        [Required(ErrorMessage = "Mã khuyến mãi là bắt buộc")]
        [MaxLength(50, ErrorMessage = "Mã khuyến mãi không được quá 50 ký tự")]
        public string PromotionCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên khuyến mãi là bắt buộc")]
        [MaxLength(200, ErrorMessage = "Tên khuyến mãi không được quá 200 ký tự")]
        public string PromotionName { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required(ErrorMessage = "Loại khuyến mãi là bắt buộc")]
        [MaxLength(50)]
        public string PromotionType { get; set; } = string.Empty; // percentage, fixed, buy_x_get_y, free_shipping

        [Required(ErrorMessage = "Giá trị giảm giá là bắt buộc")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá trị giảm giá phải lớn hơn hoặc bằng 0")]
        public decimal DiscountValue { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Số tiền mua tối thiểu phải lớn hơn hoặc bằng 0")]
        public decimal? MinPurchaseAmount { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Số tiền giảm tối đa phải lớn hơn hoặc bằng 0")]
        public decimal? MaxDiscountAmount { get; set; }

        [Required(ErrorMessage = "Ngày bắt đầu là bắt buộc")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Ngày kết thúc là bắt buộc")]
        public DateTime EndDate { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "active";

        [Range(1, int.MaxValue, ErrorMessage = "Giới hạn sử dụng phải lớn hơn 0")]
        public int? UsageLimit { get; set; }

        public string? ApplicableProducts { get; set; } // JSON array of product IDs

        public string? ApplicableCustomers { get; set; } // JSON array of customer IDs
    }
}
