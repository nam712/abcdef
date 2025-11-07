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
using Backend.Models;

namespace YourShopManagement.API.Services
{
    public interface IAuthService
    {
        Task<ApiResponse<LoginResponseDto>> RegisterAsync(RegisterDto dto);
        Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginDto dto);
        Task<ApiResponse<ShopOwnerInfoDto>> GetProfileAsync(int shopOwnerId);
        Task<ApiResponse<bool>> ChangePasswordAsync(int shopOwnerId, ChangePasswordDto dto);
        Task<ApiResponse<ForgotPasswordResponseDto>> ForgotPasswordAsync(ForgotPasswordDto dto);
        

        Task<ApiResponse<bool>> ChangeEmployeePasswordAsync(int employeeId, ChangePasswordDto dto);
        Task<ApiResponse<ForgotPasswordResponseDto>> EmployeeForgotPasswordAsync(ForgotPasswordDto dto);
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
        /// Đăng nhập - Thử ShopOwner trước, nếu không thành công thì thử Employee
        /// </summary>
        public async Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginDto dto)
        {
            try
            {
                // 1. Thử đăng nhập như ShopOwner trước
                var shopOwnerResult = await TryLoginAsShopOwnerAsync(dto);
                if (shopOwnerResult.Success)
                {
                    return shopOwnerResult;
                }

                // 2. Nếu không thành công, thử đăng nhập như Employee
                var employeeResult = await TryLoginAsEmployeeAsync(dto);
                if (employeeResult.Success)
                {
                    return employeeResult;
                }

                // 3. Nếu cả hai đều không thành công
                return ApiResponse<LoginResponseDto>.FailResponse(
                    "Số điện thoại hoặc mật khẩu không đúng",
                    new List<string> { "Invalid credentials" }
                );
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
        /// Thử đăng nhập như ShopOwner
        /// </summary>
        private async Task<ApiResponse<LoginResponseDto>> TryLoginAsShopOwnerAsync(LoginDto dto)
        {
            try
            {
                var shopOwner = await _context.ShopOwners
                    .Include(s => s.BusinessCategory)
                    .FirstOrDefaultAsync(s => s.Phone == dto.Phone);

                if (shopOwner == null || shopOwner.Status != "active")
                {
                    return ApiResponse<LoginResponseDto>.FailResponse("Not found");
                }

                bool isPasswordValid = false;
                
                Console.WriteLine($"🔍 [DEBUG] ShopOwner password analysis:");
                Console.WriteLine($"  - Length: {shopOwner.Password.Length}");
                Console.WriteLine($"  - Starts with '$2': {shopOwner.Password.StartsWith("$2")}");
                Console.WriteLine($"  - First 10 chars: {shopOwner.Password.Substring(0, Math.Min(10, shopOwner.Password.Length))}...");

                // CASE 1: BCrypt format
                if (shopOwner.Password.StartsWith("$2"))
                {
                    Console.WriteLine("🔍 [DEBUG] Detected BCrypt format - verifying...");
                    try
                    {
                        isPasswordValid = PasswordHelper.VerifyPassword(dto.Password, shopOwner.Password);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ [DEBUG] BCrypt verification failed: {ex.Message}");
                        isPasswordValid = false;
                    }
                }
                // CASE 2: SHA256 hex format (64 chars)
                else if (shopOwner.Password.Length == 64 && System.Text.RegularExpressions.Regex.IsMatch(shopOwner.Password, "^[a-fA-F0-9]{64}$"))
                {
                    Console.WriteLine("⚠️ [WARNING] Detected SHA256 HEX format - verifying...");
                    var sha256Hash = ComputeSHA256Hash(dto.Password);
                    isPasswordValid = shopOwner.Password.Equals(sha256Hash, StringComparison.OrdinalIgnoreCase);
                    
                    if (isPasswordValid)
                    {
                        Console.WriteLine("✅ [SECURITY] SHA256 HEX matched! Upgrading to BCrypt...");
                        shopOwner.Password = PasswordHelper.HashPassword(dto.Password);
                        shopOwner.UpdatedAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                        Console.WriteLine("✅ [SECURITY] Password upgraded to BCrypt");
                    }
                }
                // ✅ CASE 3: Base64-encoded SHA256 (44 chars)
                else if (shopOwner.Password.Length == 44)
                {
                    Console.WriteLine("⚠️ [WARNING] Detected possible BASE64-encoded SHA256 format!");
                    
                    try
                    {
                        // Thử decode Base64
                        var base64Bytes = Convert.FromBase64String(shopOwner.Password);
                        var base64Hex = BitConverter.ToString(base64Bytes).Replace("-", "").ToLowerInvariant();
                        
                        Console.WriteLine($"🔍 [DEBUG] Base64 decoded to hex: {base64Hex.Substring(0, Math.Min(20, base64Hex.Length))}...");
                        
                        // Compute SHA256 của password input
                        var sha256Hash = ComputeSHA256Hash(dto.Password);
                        
                        Console.WriteLine($"🔍 [DEBUG] Input SHA256: {sha256Hash.Substring(0, 20)}...");
                        Console.WriteLine($"🔍 [DEBUG] Comparing...");
                        
                        isPasswordValid = base64Hex.Equals(sha256Hash, StringComparison.OrdinalIgnoreCase);
                        
                        Console.WriteLine($"🔍 [DEBUG] Base64-SHA256 comparison result: {isPasswordValid}");
                        
                        if (isPasswordValid)
                        {
                            Console.WriteLine("✅ [SECURITY] Base64-SHA256 matched! Upgrading to BCrypt...");
                            shopOwner.Password = PasswordHelper.HashPassword(dto.Password);
                            shopOwner.UpdatedAt = DateTime.UtcNow;
                            await _context.SaveChangesAsync();
                            Console.WriteLine("✅ [SECURITY] Password upgraded to BCrypt");
                        }
                    }
                    catch (FormatException ex)
                    {
                        Console.WriteLine($"❌ [DEBUG] Base64 decode failed: {ex.Message}");
                        Console.WriteLine("🔍 [DEBUG] Trying plain text comparison as fallback...");
                        
                        isPasswordValid = shopOwner.Password == dto.Password;
                        
                        if (isPasswordValid)
                        {
                            Console.WriteLine("✅ [SECURITY] Plain text matched! Upgrading to BCrypt...");
                            shopOwner.Password = PasswordHelper.HashPassword(dto.Password);
                            shopOwner.UpdatedAt = DateTime.UtcNow;
                            await _context.SaveChangesAsync();
                            Console.WriteLine("✅ [SECURITY] Password upgraded to BCrypt");
                        }
                    }
                }
                // CASE 4: Unknown format (fallback plain text)
                else
                {
                    Console.WriteLine("🔍 [DEBUG] Unknown format - trying plain text comparison");
                    isPasswordValid = shopOwner.Password == dto.Password;
                    
                    if (isPasswordValid)
                    {
                        Console.WriteLine("✅ [SECURITY] Plain text matched! Hashing to BCrypt...");
                        shopOwner.Password = PasswordHelper.HashPassword(dto.Password);
                        shopOwner.UpdatedAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                        Console.WriteLine("✅ [SECURITY] Password hashed to BCrypt");
                    }
                }

                Console.WriteLine($"🔍 [DEBUG] Final password validation: {isPasswordValid}");

                if (!isPasswordValid)
                {
                    return ApiResponse<LoginResponseDto>.FailResponse("Invalid password");
                }

                // Generate JWT Token cho ShopOwner
                var token = _jwtHelper.GenerateTokenForShopOwner(shopOwner);
                var tokenExpiry = DateTime.UtcNow.AddHours(1);

                var response = new LoginResponseDto
                {
                    Success = true,
                    Message = "Đăng nhập thành công với quyền Chủ cửa hàng",
                    Token = token,
                    TokenExpiry = tokenExpiry,
                    UserType = "ShopOwner",
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

                return ApiResponse<LoginResponseDto>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [DEBUG] Exception in TryLoginAsShopOwnerAsync: {ex.Message}");
                Console.WriteLine($"❌ [DEBUG] Stack trace: {ex.StackTrace}");
                return ApiResponse<LoginResponseDto>.FailResponse("Error");
            }
        }

        /// <summary>
        /// ⚠️ LEGACY: Compute SHA256 hash (chỉ dùng để verify password cũ)
        /// </summary>
        private string ComputeSHA256Hash(string input)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(input);
                var hash = sha256.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        /// <summary>
        /// Thử đăng nhập như Employee
        /// </summary>
        private async Task<ApiResponse<LoginResponseDto>> TryLoginAsEmployeeAsync(LoginDto dto)
        {
            try
            {
                Console.WriteLine("🔍 [DEBUG] TryLoginAsEmployeeAsync started");
                Console.WriteLine($"🔍 [DEBUG] Looking for employee with username: '{dto.Phone}'");

                var employee = await _context.Employees
                    .IgnoreQueryFilters()
                    .Include(e => e.ShopOwner)
                    .FirstOrDefaultAsync(e => e.Username == dto.Phone);

                Console.WriteLine($"🔍 [DEBUG] Employee found (ignoring filters): {employee != null}");

                if (employee != null)
                {
                    Console.WriteLine($"🔍 [DEBUG] Employee details:");
                    Console.WriteLine($"  - ID: {employee.EmployeeId}");
                    Console.WriteLine($"  - Name: {employee.EmployeeName}");
                    Console.WriteLine($"  - Username: {employee.Username}");
                    Console.WriteLine($"  - ShopOwnerId: {employee.ShopOwnerId}");
                    Console.WriteLine($"  - WorkStatus: {employee.WorkStatus}");
                    Console.WriteLine($"  - Has Password: {!string.IsNullOrEmpty(employee.Password)}");
                    Console.WriteLine($"  - Password Length: {employee.Password?.Length ?? 0}");
                    
                    if (!string.IsNullOrEmpty(employee.Password))
                    {
                        Console.WriteLine($"  - Password starts with '$2': {employee.Password.StartsWith("$2")}");
                        Console.WriteLine($"  - Password first 10 chars: {employee.Password.Substring(0, Math.Min(10, employee.Password.Length))}...");
                    }
                }

                if (employee == null)
                {
                    Console.WriteLine("❌ [DEBUG] Employee not found");
                    return ApiResponse<LoginResponseDto>.FailResponse("Employee not found");
                }

                if (string.IsNullOrEmpty(employee.Password))
                {
                    Console.WriteLine("❌ [DEBUG] Employee has no password");
                    return ApiResponse<LoginResponseDto>.FailResponse("Employee has no password");
                }

                if (!string.IsNullOrEmpty(employee.WorkStatus) && 
                    employee.WorkStatus.ToLower() != "active" && 
                    employee.WorkStatus.ToLower() != "hoạt động")
                {
                    Console.WriteLine($"❌ [DEBUG] Employee not active. WorkStatus: {employee.WorkStatus}");
                    return ApiResponse<LoginResponseDto>.FailResponse("Employee not active");
                }

                // ✅ SỬA: Kiểm tra password - ưu tiên BCrypt, fallback plain text
                bool isPasswordValid = false;

                Console.WriteLine($"🔍 [DEBUG] Checking password...");
                Console.WriteLine($"🔍 [DEBUG] Input password: '{dto.Password}'");

                // ✅ CASE 1: Password đã được hash (BCrypt hoặc hash khác)
                if (employee.Password.Length > 20) // Hash thường dài hơn 20 ký tự
                {
                    Console.WriteLine("🔍 [DEBUG] Password appears to be HASHED (length > 20)");
                    
                    // Thử BCrypt verify
                    try
                    {
                        Console.WriteLine("🔍 [DEBUG] Attempting BCrypt verification...");
                        isPasswordValid = PasswordHelper.VerifyPassword(dto.Password, employee.Password);
                        Console.WriteLine($"🔍 [DEBUG] BCrypt verification result: {isPasswordValid}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ [DEBUG] BCrypt verification failed: {ex.Message}");
                        
                        // ⚠️ Nếu BCrypt fail, có thể password cũ dùng hash khác
                        // Trong trường hợp này, bạn cần re-hash lại password
                        Console.WriteLine("⚠️ [WARNING] Password is hashed but not BCrypt format!");
                        Console.WriteLine("⚠️ [ACTION REQUIRED] Please re-hash this employee's password!");
                        
                        isPasswordValid = false;
                    }
                }
                // ✅ CASE 2: Plain text password (tương thích ngược)
                else
                {
                    Console.WriteLine("🔍 [DEBUG] Password is PLAIN TEXT - Using direct comparison");
                    isPasswordValid = employee.Password == dto.Password;
                    Console.WriteLine($"🔍 [DEBUG] Plain text comparison result: {isPasswordValid}");
                    
                    // ✅ Nếu login thành công, hash lại password bằng BCrypt
                    if (isPasswordValid)
                    {
                        Console.WriteLine("⚠️ [SECURITY] Password is plain text! Re-hashing with BCrypt...");
                        employee.Password = PasswordHelper.HashPassword(dto.Password);
                        employee.UpdatedAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                        Console.WriteLine("✅ [SECURITY] Password has been re-hashed with BCrypt and saved");
                    }
                }

                Console.WriteLine($"🔍 [DEBUG] Final password validation result: {isPasswordValid}");

                if (!isPasswordValid)
                {
                    Console.WriteLine("❌ [DEBUG] Invalid password");
                    return ApiResponse<LoginResponseDto>.FailResponse("Invalid password");
                }

                Console.WriteLine("✅ [DEBUG] Employee login successful, generating token...");

                // Generate JWT Token cho Employee
                var token = _jwtHelper.GenerateTokenForEmployee(employee);
                var tokenExpiry = DateTime.UtcNow.AddHours(1);

                var response = new LoginResponseDto
                {
                    Success = true,
                    Message = "Đăng nhập thành công với quyền Nhân viên",
                    Token = token,
                    TokenExpiry = tokenExpiry,
                    UserType = "Employee",
                    Employee = new EmployeeInfoDto
                    {
                        EmployeeId = employee.EmployeeId,
                        ShopOwnerId = employee.ShopOwnerId,
                        EmployeeCode = employee.EmployeeCode,
                        EmployeeName = employee.EmployeeName,
                        Phone = employee.Phone,
                        Email = employee.Email,
                        Position = employee.Position,
                        Department = employee.Department,
                        Permissions = employee.Permissions,
                        AvatarUrl = employee.AvatarUrl,
                        WorkStatus = employee.WorkStatus,
                        ShopName = employee.ShopOwner?.ShopName ?? "",
                        CreatedAt = employee.CreatedAt
                    }
                };

                Console.WriteLine("✅ [DEBUG] Employee login response created successfully");
                return ApiResponse<LoginResponseDto>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [DEBUG] Exception in TryLoginAsEmployeeAsync: {ex.Message}");
                Console.WriteLine($"❌ [DEBUG] Stack trace: {ex.StackTrace}");
                return ApiResponse<LoginResponseDto>.FailResponse("Error");
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
        /// ✅ Employee đổi mật khẩu
        /// </summary>
        public async Task<ApiResponse<bool>> ChangeEmployeePasswordAsync(int employeeId, ChangePasswordDto dto)
        {
            try
            {
                var employee = await _context.Employees
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

                if (employee == null)
                {
                    return ApiResponse<bool>.FailResponse("Không tìm thấy nhân viên");
                }

                // Verify mật khẩu cũ
                bool isOldPasswordValid;
                if (employee.Password.StartsWith("$2")) // Đã được hash
                {
                    isOldPasswordValid = PasswordHelper.VerifyPassword(dto.OldPassword, employee.Password);
                }
                else // Plain text (tương thích cũ)
                {
                    isOldPasswordValid = employee.Password == dto.OldPassword;
                }

                if (!isOldPasswordValid)
                {
                    return ApiResponse<bool>.FailResponse("Mật khẩu cũ không đúng");
                }

                // ✅ Hash mật khẩu mới
                employee.Password = PasswordHelper.HashPassword(dto.NewPassword);
                employee.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ [DEBUG] Employee {employeeId} changed password successfully");
                return ApiResponse<bool>.SuccessResponse(true, "Đổi mật khẩu thành công");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [DEBUG] Error changing employee password: {ex.Message}");
                return ApiResponse<bool>.FailResponse(
                    "Đã xảy ra lỗi khi đổi mật khẩu",
                    new List<string> { ex.Message }
                );
            }
        }

        /// <summary>
        /// ✅ Employee quên mật khẩu - Gửi mật khẩu mới qua SMS
        /// </summary>
        public async Task<ApiResponse<ForgotPasswordResponseDto>> EmployeeForgotPasswordAsync(ForgotPasswordDto dto)
        {
            try
            {
                // 1. Tìm Employee theo username (có thể là số điện thoại)
                var employee = await _context.Employees
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(e => e.Username == dto.Phone || e.Phone == dto.Phone);

                if (employee == null)
                {
                    return ApiResponse<ForgotPasswordResponseDto>.FailResponse(
                        "Không tìm thấy nhân viên với số điện thoại này",
                        new List<string> { "Employee not found" }
                    );
                }

                // 2. Kiểm tra WorkStatus
                if (employee.WorkStatus?.ToLower() != "active" && employee.WorkStatus?.ToLower() != "hoạt động")
                {
                    return ApiResponse<ForgotPasswordResponseDto>.FailResponse(
                        "Tài khoản nhân viên không hoạt động. Vui lòng liên hệ quản lý.",
                        new List<string> { "Employee account not active" }
                    );
                }

                // 3. Tạo mật khẩu mới (8 ký tự: chữ + số)
                var newPassword = GenerateRandomPassword(8);

                // 4. Gửi mật khẩu mới qua SMS (nếu có số điện thoại)
                if (!string.IsNullOrEmpty(employee.Phone))
                {
                    var smsSent = await _smsService.SendNewPasswordAsync(employee.Phone, newPassword);
                    
                    if (!smsSent)
                    {
                        return ApiResponse<ForgotPasswordResponseDto>.FailResponse(
                            "Không thể gửi SMS. Vui lòng liên hệ quản lý.",
                            new List<string> { "SMS sending failed" }
                        );
                    }
                }

                // 5. ✅ Hash và cập nhật mật khẩu mới
                employee.Password = PasswordHelper.HashPassword(newPassword);
                employee.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ [DEBUG] Employee {employee.EmployeeId} password reset successfully");

                // 6. Trả về response
                var response = new ForgotPasswordResponseDto
                {
                    Success = true,
                    Message = "Mật khẩu mới đã được gửi về số điện thoại của bạn. Vui lòng kiểm tra tin nhắn SMS.",
                    Phone = MaskPhoneNumber(employee.Phone ?? employee.Username ?? ""),
                    SentAt = DateTime.UtcNow
                };

                return ApiResponse<ForgotPasswordResponseDto>.SuccessResponse(
                    response,
                    "Gửi mật khẩu mới thành công"
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [DEBUG] Error in employee forgot password: {ex.Message}");
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