using System.ComponentModel.DataAnnotations;

namespace YourShopManagement.API.DTOs.Auth
{
    /// <summary>
    /// DTO cho chức năng quên mật khẩu
    /// </summary>
    public class ForgotPasswordDto
    {
        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [RegularExpression(@"^0[0-9]{9}$", ErrorMessage = "Số điện thoại phải có 10 chữ số và bắt đầu bằng số 0")]
        public string Phone { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response DTO sau khi gửi mật khẩu mới
    /// </summary>
    public class ForgotPasswordResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public DateTime? SentAt { get; set; }
    }
}
