using System.ComponentModel.DataAnnotations;

namespace YourShopManagement.API.DTOs.Shop
{
    /// <summary>
    /// DTO cập nhật thông tin cửa hàng
    /// </summary>
    public class UpdateShopDto
    {
        [Required(ErrorMessage = "Mã cửa hàng không được để trống")]
        [MaxLength(50, ErrorMessage = "Mã cửa hàng không được vượt quá 50 ký tự")]
        public string ShopCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên cửa hàng không được để trống")]
        [MaxLength(255, ErrorMessage = "Tên cửa hàng không được vượt quá 255 ký tự")]
        public string ShopName { get; set; } = string.Empty;

        [MaxLength(255, ErrorMessage = "Địa chỉ không được vượt quá 255 ký tự")]
        public string? ShopAddress { get; set; }

        [MaxLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? ShopPhone { get; set; }

        [MaxLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string? ShopEmail { get; set; }

        [MaxLength(255, ErrorMessage = "Tên quản lý không được vượt quá 255 ký tự")]
        public string? ManagerName { get; set; }

        [MaxLength(20, ErrorMessage = "Số điện thoại quản lý không được vượt quá 20 ký tự")]
        [Phone(ErrorMessage = "Số điện thoại quản lý không hợp lệ")]
        public string? ManagerPhone { get; set; }

        public int? BusinessCategoryId { get; set; }

        [MaxLength(20, ErrorMessage = "Trạng thái không được vượt quá 20 ký tự")]
        public string Status { get; set; } = "active";

        public string? Notes { get; set; }
    }
}
