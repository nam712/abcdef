using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YourShopManagement.API.Data;
using YourShopManagement.API.DTOs.Auth;
using YourShopManagement.API.Helpers;
using YourShopManagement.API.Models;
using YourShopManagement.API.Services.SmsService;

namespace YourShopManagement.API.Services
{
    public interface IAuthService
    {
        Task<ApiResponse<LoginResponseDto>> RegisterAsync(RegisterDto dto);
        Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginDto dto);
        Task<ApiResponse<ShopOwnerInfoDto>> GetProfileAsync(int shopOwnerId);
        Task<ApiResponse<bool>> ChangePasswordAsync(int shopOwnerId, ChangePasswordDto dto);
        Task<ApiResponse<ForgotPasswordResponseDto>> ForgotPasswordAsync(ForgotPasswordDto dto);
    }

    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtHelper _jwtHelper;
        private readonly ISmsService _smsService;

        public AuthService(
            ApplicationDbContext context, 
            JwtHelper jwtHelper,
            ISmsService smsService)
        {
            _context = context;
            _jwtHelper = jwtHelper;
            _smsService = smsService;
        }

        /// <summary>
        /// Đăng ký tài khoản ShopOwner mới
        /// </summary>
        public async Task<ApiResponse<LoginResponseDto>> RegisterAsync(RegisterDto dto)
        {
            try
            {
                // 1. Kiểm tra số điện thoại đã tồn tại chưa
                var existingShopOwner = await _context.ShopOwners
                    .FirstOrDefaultAsync(s => s.Phone == dto.Phone);

                if (existingShopOwner != null)
                {
                    return ApiResponse<LoginResponseDto>.FailResponse(
                        "Số điện thoại đã được đăng ký",
                        new List<string> { "Phone number already exists" }
                    );
                }

                // 2. Kiểm tra email đã tồn tại (nếu có)
                if (!string.IsNullOrEmpty(dto.Email))
                {
                    var existingEmail = await _context.ShopOwners
                        .FirstOrDefaultAsync(s => s.Email == dto.Email);

                    if (existingEmail != null)
                    {
                        return ApiResponse<LoginResponseDto>.FailResponse(
                            "Email đã được đăng ký",
                            new List<string> { "Email already exists" }
                        );
                    }
                }

                // 3. Kiểm tra Business Category có tồn tại không
                if (dto.BusinessCategoryId.HasValue)
                {
                    var categoryExists = await _context.BusinessCategories
                        .AnyAsync(c => c.CategoryId == dto.BusinessCategoryId.Value);

                    if (!categoryExists)
                    {
                        return ApiResponse<LoginResponseDto>.FailResponse(
                            "Ngành hàng không tồn tại",
                            new List<string> { "Business category not found" }
                        );
                    }
                }

                // 4. Kiểm tra điều khoản
                if (!dto.TermsAndConditionsAgreed)
                {
                    return ApiResponse<LoginResponseDto>.FailResponse(
                        "Bạn phải đồng ý với điều khoản & dịch vụ",
                        new List<string> { "Terms and conditions must be agreed" }
                    );
                }

                // 5. Hash password
                var hashedPassword = PasswordHelper.HashPassword(dto.Password);

                // 6. Tạo ShopOwner mới
                var shopOwner = new ShopOwner
                {
                    ShopOwnerName = dto.ShopOwnerName,
                    Phone = dto.Phone,
                    Email = dto.Email,
                    Address = dto.Address,
                    DateOfBirth = dto.DateOfBirth,
                    Gender = dto.Gender,
                    TaxCode = dto.TaxCode,
                    BusinessCategoryId = dto.BusinessCategoryId,
                    ShopName = dto.ShopName,
                    ShopAddress = dto.ShopAddress,
                    ShopDescription = dto.ShopDescription,
                    Password = hashedPassword,
                    TermsAndConditionsAgreed = dto.TermsAndConditionsAgreed,
                    Status = "active",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.ShopOwners.Add(shopOwner);
                await _context.SaveChangesAsync();

                // 7. Generate JWT Token
                var token = _jwtHelper.GenerateToken(shopOwner);
                var tokenExpiry = DateTime.UtcNow.AddHours(1);

                // 8. Load lại với BusinessCategory
                var createdShopOwner = await _context.ShopOwners
                    .Include(s => s.BusinessCategory)
                    .FirstOrDefaultAsync(s => s.ShopOwnerId == shopOwner.ShopOwnerId);

                var response = new LoginResponseDto
                {
                    Success = true,
                    Message = "Đăng ký thành công",
                    Token = token,
                    TokenExpiry = tokenExpiry,
                    ShopOwner = new ShopOwnerInfoDto
                    {
                        ShopOwnerId = createdShopOwner!.ShopOwnerId,
                        ShopOwnerName = createdShopOwner.ShopOwnerName,
                        Phone = createdShopOwner.Phone,
                        Email = createdShopOwner.Email,
                        ShopName = createdShopOwner.ShopName,
                        ShopLogoUrl = createdShopOwner.ShopLogoUrl,
                        AvatarUrl = createdShopOwner.AvatarUrl,
                        Status = createdShopOwner.Status,
                        BusinessCategoryId = createdShopOwner.BusinessCategoryId,
                        BusinessCategoryName = createdShopOwner.BusinessCategory?.CategoryName,
                        CreatedAt = createdShopOwner.CreatedAt
                    }
                };

                return ApiResponse<LoginResponseDto>.SuccessResponse(response, "Đăng ký thành công");
            }
            catch (Exception ex)
            {
                return ApiResponse<LoginResponseDto>.FailResponse(
                    "Đã xảy ra lỗi khi đăng ký",
                    new List<string> { ex.Message }
                );
            }
        }

        /// <summary>
        /// Đăng nhập
        /// </summary>
        public async Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginDto dto)
        {
            try
            {
                // 1. Tìm ShopOwner theo Phone
                var shopOwner = await _context.ShopOwners
                    .Include(s => s.BusinessCategory)
                    .FirstOrDefaultAsync(s => s.Phone == dto.Phone);

                if (shopOwner == null)
                {
                    return ApiResponse<LoginResponseDto>.FailResponse(
                        "Số điện thoại hoặc mật khẩu không đúng",
                        new List<string> { "Invalid credentials" }
                    );
                }

                // 2. Kiểm tra trạng thái tài khoản
                if (shopOwner.Status != "active")
                {
                    return ApiResponse<LoginResponseDto>.FailResponse(
                        "Tài khoản đã bị khóa hoặc vô hiệu hóa",
                        new List<string> { "Account is not active" }
                    );
                }

                // 3. Verify password
                var isPasswordValid = PasswordHelper.VerifyPassword(dto.Password, shopOwner.Password);
                if (!isPasswordValid)
                {
                    return ApiResponse<LoginResponseDto>.FailResponse(
                        "Số điện thoại hoặc mật khẩu không đúng",
                        new List<string> { "Invalid credentials" }
                    );
                }

                // 4. Generate JWT Token
                var token = _jwtHelper.GenerateToken(shopOwner);
                var tokenExpiry = DateTime.UtcNow.AddHours(1);

                var response = new LoginResponseDto
                {
                    Success = true,
                    Message = "Đăng nhập thành công",
                    Token = token,
                    TokenExpiry = tokenExpiry,
                    ShopOwner = new ShopOwnerInfoDto
                    {
                        ShopOwnerId = shopOwner.ShopOwnerId,
                        ShopOwnerName = shopOwner.ShopOwnerName,
                        Phone = shopOwner.Phone,
                        Email = shopOwner.Email,
                        ShopName = shopOwner.ShopName,
                        ShopLogoUrl = shopOwner.ShopLogoUrl,
                        AvatarUrl = shopOwner.AvatarUrl,
                        Status = shopOwner.Status,
                        BusinessCategoryId = shopOwner.BusinessCategoryId,
                        BusinessCategoryName = shopOwner.BusinessCategory?.CategoryName,
                        CreatedAt = shopOwner.CreatedAt
                    }
                };

                return ApiResponse<LoginResponseDto>.SuccessResponse(response, "Đăng nhập thành công");
            }
            catch (Exception ex)
            {
                return ApiResponse<LoginResponseDto>.FailResponse(
                    "Đã xảy ra lỗi khi đăng nhập",
                    new List<string> { ex.Message }
                );
            }
        }

        /// <summary>
        /// Lấy thông tin profile
        /// </summary>
        public async Task<ApiResponse<ShopOwnerInfoDto>> GetProfileAsync(int shopOwnerId)
        {
            try
            {
                var shopOwner = await _context.ShopOwners
                    .Include(s => s.BusinessCategory)
                    .FirstOrDefaultAsync(s => s.ShopOwnerId == shopOwnerId);

                if (shopOwner == null)
                {
                    return ApiResponse<ShopOwnerInfoDto>.FailResponse("Không tìm thấy thông tin người dùng");
                }

                var info = new ShopOwnerInfoDto
                {
                    ShopOwnerId = shopOwner.ShopOwnerId,
                    ShopOwnerName = shopOwner.ShopOwnerName,
                    Phone = shopOwner.Phone,
                    Email = shopOwner.Email,
                    ShopName = shopOwner.ShopName,
                    ShopLogoUrl = shopOwner.ShopLogoUrl,
                    AvatarUrl = shopOwner.AvatarUrl,
                    Status = shopOwner.Status,
                    BusinessCategoryId = shopOwner.BusinessCategoryId,
                    BusinessCategoryName = shopOwner.BusinessCategory?.CategoryName,
                    CreatedAt = shopOwner.CreatedAt
                };

                return ApiResponse<ShopOwnerInfoDto>.SuccessResponse(info);
            }
            catch (Exception ex)
            {
                return ApiResponse<ShopOwnerInfoDto>.FailResponse(
                    "Đã xảy ra lỗi",
                    new List<string> { ex.Message }
                );
            }
        }

        /// <summary>
        /// Đổi mật khẩu
        /// </summary>
        public async Task<ApiResponse<bool>> ChangePasswordAsync(int shopOwnerId, ChangePasswordDto dto)
        {
            try
            {
                var shopOwner = await _context.ShopOwners
                    .FirstOrDefaultAsync(s => s.ShopOwnerId == shopOwnerId);

                if (shopOwner == null)
                {
                    return ApiResponse<bool>.FailResponse("Không tìm thấy người dùng");
                }

                // Verify mật khẩu cũ
                var isOldPasswordValid = PasswordHelper.VerifyPassword(dto.OldPassword, shopOwner.Password);
                if (!isOldPasswordValid)
                {
                    return ApiResponse<bool>.FailResponse("Mật khẩu cũ không đúng");
                }

                // Hash mật khẩu mới
                shopOwner.Password = PasswordHelper.HashPassword(dto.NewPassword);
                shopOwner.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResponse(true, "Đổi mật khẩu thành công");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.FailResponse(
                    "Đã xảy ra lỗi",
                    new List<string> { ex.Message }
                );
            }
        }

        /// <summary>
        /// Quên mật khẩu - Gửi mật khẩu mới về số điện thoại
        /// </summary>
        public async Task<ApiResponse<ForgotPasswordResponseDto>> ForgotPasswordAsync(ForgotPasswordDto dto)
        {
            try
            {
                // 1. Tìm ShopOwner theo số điện thoại
                var shopOwner = await _context.ShopOwners
                    .FirstOrDefaultAsync(s => s.Phone == dto.Phone);

                if (shopOwner == null)
                {
                    // Không tiết lộ thông tin tài khoản có tồn tại hay không (bảo mật)
                    return ApiResponse<ForgotPasswordResponseDto>.FailResponse(
                        "Số điện thoại không tồn tại trong hệ thống",
                        new List<string> { "Phone number not found" }
                    );
                }

                // 2. Kiểm tra trạng thái tài khoản
                if (shopOwner.Status != "active")
                {
                    return ApiResponse<ForgotPasswordResponseDto>.FailResponse(
                        "Tài khoản đã bị khóa hoặc vô hiệu hóa. Vui lòng liên hệ hỗ trợ.",
                        new List<string> { "Account is not active" }
                    );
                }

                // 3. Tạo mật khẩu mới (8 ký tự: chữ + số)
                var newPassword = GenerateRandomPassword(8);

                // 4. Gửi mật khẩu mới qua SMS
                var smsSent = await _smsService.SendNewPasswordAsync(shopOwner.Phone, newPassword);

                if (!smsSent)
                {
                    return ApiResponse<ForgotPasswordResponseDto>.FailResponse(
                        "Không thể gửi SMS. Vui lòng thử lại sau.",
                        new List<string> { "SMS sending failed" }
                    );
                }

                // 5. Cập nhật mật khẩu mới trong database
                shopOwner.Password = PasswordHelper.HashPassword(newPassword);
                shopOwner.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // 6. Trả về response
                var response = new ForgotPasswordResponseDto
                {
                    Success = true,
                    Message = "Mật khẩu mới đã được gửi về số điện thoại của bạn. Vui lòng kiểm tra tin nhắn SMS.",
                    Phone = MaskPhoneNumber(shopOwner.Phone),
                    SentAt = DateTime.UtcNow
                };

                return ApiResponse<ForgotPasswordResponseDto>.SuccessResponse(
                    response, 
                    "Gửi mật khẩu mới thành công"
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<ForgotPasswordResponseDto>.FailResponse(
                    "Đã xảy ra lỗi khi xử lý yêu cầu",
                    new List<string> { ex.Message }
                );
            }
        }

        /// <summary>
        /// Tạo mật khẩu ngẫu nhiên
        /// </summary>
        private string GenerateRandomPassword(int length)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789abcdefghjkmnpqrstuvwxyz";
            var random = new Random();
            var password = new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            
            return password;
        }

        /// <summary>
        /// Che số điện thoại (ví dụ: 0912***678)
        /// </summary>
        private string MaskPhoneNumber(string phone)
        {
            if (string.IsNullOrEmpty(phone) || phone.Length < 6)
                return phone;

            return phone.Substring(0, 4) + "***" + phone.Substring(phone.Length - 3);
        }
    }
}