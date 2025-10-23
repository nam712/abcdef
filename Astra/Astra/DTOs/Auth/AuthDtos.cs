using System;
using System.ComponentModel.DataAnnotations;

namespace YourShopManagement.API.DTOs.Auth
{
    // ==================== REGISTER DTO ====================
    public class RegisterDto
    {
        [Required(ErrorMessage = "Tên chủ shop không được để trống")]
        [MaxLength(255)]
        public string ShopOwnerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [MaxLength(20)]
        public string Phone { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(255)]
        public string? Address { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [MaxLength(10)]
        public string? Gender { get; set; }

        [MaxLength(50)]
        public string? TaxCode { get; set; }

        public int? BusinessCategoryId { get; set; }

        [Required(ErrorMessage = "Tên cửa hàng không được để trống")]
        [MaxLength(255)]
        public string ShopName { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? ShopAddress { get; set; }

        [MaxLength(255)]
        public string? ShopDescription { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        [MaxLength(255)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Xác nhận mật khẩu không được để trống")]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Bạn phải đồng ý với điều khoản & dịch vụ")]
        public bool TermsAndConditionsAgreed { get; set; } = false;
    }

    // ==================== LOGIN DTO ====================
    public class LoginDto
    {
        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        public string Password { get; set; } = string.Empty;
    }

    // ==================== LOGIN RESPONSE DTO ====================
    public class LoginResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Token { get; set; }
        public DateTime? TokenExpiry { get; set; }
        public ShopOwnerInfoDto? ShopOwner { get; set; }
    }

    // ==================== SHOP OWNER INFO DTO ====================
    public class ShopOwnerInfoDto
    {
        public int ShopOwnerId { get; set; }
        public string ShopOwnerName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public string? ShopLogoUrl { get; set; }
        public string? AvatarUrl { get; set; }
        public string Status { get; set; } = string.Empty;
        public int? BusinessCategoryId { get; set; }
        public string? BusinessCategoryName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // ==================== CHANGE PASSWORD DTO ====================
    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "Mật khẩu cũ không được để trống")]
        public string OldPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu mới không được để trống")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Xác nhận mật khẩu không được để trống")]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }

    // ==================== API RESPONSE DTO ====================
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string>? Errors { get; set; }

        public static ApiResponse<T> SuccessResponse(T data, string message = "Thành công")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse<T> FailResponse(string message, List<string>? errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Errors = errors
            };
        }
    }
}