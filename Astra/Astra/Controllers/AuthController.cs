using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using YourShopManagement.API.DTOs.Auth;
using YourShopManagement.API.Services;


namespace YourShopManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// 🔐 Đăng ký tài khoản ShopOwner mới
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/auth/register
        ///     {
        ///        "shopOwnerName": "Nguyễn Văn A",
        ///        "phone": "0912345678",
        ///        "email": "nguyenvana@example.com",
        ///        "shopName": "Cửa hàng ABC",
        ///        "password": "Password123",
        ///        "confirmPassword": "Password123",
        ///        "termsAndConditionsAgreed": true
        ///     }
        /// </remarks>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [Tags("🔐 Authentication")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            // Log để debug
            Console.WriteLine("========================================");
            Console.WriteLine("📥 RECEIVED REGISTRATION REQUEST");
            Console.WriteLine($"ShopOwnerName: {dto.ShopOwnerName}");
            Console.WriteLine($"Phone: {dto.Phone}");
            Console.WriteLine($"Email: {dto.Email}");
            Console.WriteLine($"Gender: {dto.Gender}");
            Console.WriteLine($"BusinessCategoryId: {dto.BusinessCategoryId}");
            Console.WriteLine($"ShopName: {dto.ShopName}");
            Console.WriteLine($"ShopAddress: {dto.ShopAddress}");
            Console.WriteLine($"TermsAgreed: {dto.TermsAndConditionsAgreed}");
            Console.WriteLine("========================================");

            // Kiểm tra ModelState
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                Console.WriteLine("❌ MODEL STATE INVALID:");
                foreach (var error in errors)
                {
                    Console.WriteLine($"  - {error}");
                }

                return BadRequest(new
                {
                    success = false,
                    message = "Dữ liệu không hợp lệ",
                    errors = errors
                });
            }

            var result = await _authService.RegisterAsync(dto);

            if (!result.Success)
            {
                Console.WriteLine($"❌ Registration failed: {result.Message}");
                return BadRequest(result);
            }

            Console.WriteLine("✅ REGISTRATION SUCCESSFUL!");
            return Ok(result);
        }

        /// <summary>
        /// 🔑 Đăng nhập vào hệ thống (ShopOwner hoặc Employee)
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/auth/login
        ///     {
        ///        "phone": "0912345678",
        ///        "password": "Password123"
        ///     }
        ///     
        /// Hệ thống sẽ tự động:
        /// 1. Thử đăng nhập như ShopOwner (bằng phone)
        /// 2. Nếu không thành công, thử đăng nhập như Employee (bằng username)
        /// 3. Trả về JWT token và thông tin tương ứng với role
        /// 
        /// LUU Y: 
        /// - ShopOwner: đăng nhập bằng PHONE + PASSWORD
        /// - Employee: đăng nhập bằng USERNAME + PASSWORD (username có thể là phone number)
        /// </remarks>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        [Tags("🔐 Authentication")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            Console.WriteLine("========================================");
            Console.WriteLine("🔑 LOGIN REQUEST RECEIVED");
            Console.WriteLine($"Phone/Username: {dto.Phone}");
            Console.WriteLine("1️⃣ Trying ShopOwner login (by phone)...");
            Console.WriteLine("2️⃣ If failed, trying Employee login (by username)...");
            Console.WriteLine("========================================");

            // Kiểm tra ModelState
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Dữ liệu không hợp lệ",
                    errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList()
                });
            }

            var result = await _authService.LoginAsync(dto);

            if (!result.Success)
            {
                Console.WriteLine($"❌ Login failed: {result.Message}");
                return Unauthorized(result);
            }

            Console.WriteLine($"✅ LOGIN SUCCESSFUL! UserType: {result.Data?.UserType}");
            
            if (result.Data?.UserType == "ShopOwner")
            {
                Console.WriteLine($"👑 ShopOwner: {result.Data.ShopOwner?.ShopOwnerName} - {result.Data.ShopOwner?.ShopName}");
            }
            else if (result.Data?.UserType == "Employee")
            {
                Console.WriteLine($"👤 Employee: {result.Data.Employee?.EmployeeName} - {result.Data.Employee?.Position}");
                Console.WriteLine($"   Username: {result.Data.Employee?.Phone} (used as login username)");
            }

            return Ok(result);
        }

        /// <summary>
        /// 👤 Lấy thông tin profile (yêu cầu JWT token)
        /// </summary>
        [HttpGet("profile")]
        [Authorize(Roles = "ShopOwner,Employee")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        [Tags("👤 User Profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userType = User.FindFirst("user_type")?.Value;
            var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Token không hợp lệ hoặc đã hết hạn"
                });
            }

            if (userType == "ShopOwner" && int.TryParse(userId, out int shopOwnerId))
            {
                var result = await _authService.GetProfileAsync(shopOwnerId);
                return result.Success ? Ok(result) : NotFound(result);
            }
            else if (userType == "Employee")
            {
                // Trả về thông tin Employee từ token claims
                return Ok(new
                {
                    success = true,
                    message = "Thông tin nhân viên",
                    data = new
                    {
                        userType = "Employee",
                        employeeId = userId,
                        employeeName = User.FindFirst(JwtRegisteredClaimNames.Name)?.Value,
                        employeeCode = User.FindFirst("employee_code")?.Value,
                        phone = User.FindFirst("phone")?.Value,
                        position = User.FindFirst("position")?.Value,
                        department = User.FindFirst("department")?.Value,
                        permissions = User.FindFirst("permissions")?.Value,
                        shopOwnerId = User.FindFirst("shop_owner_id")?.Value
                    }
                });
            }

            return BadRequest(new
            {
                success = false,
                message = "Loại người dùng không hợp lệ"
            });
        }

        /// <summary>
        /// 🔒 Đổi mật khẩu ShopOwner (yêu cầu JWT token)
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     PUT /api/auth/change-password
        ///     {
        ///        "oldPassword": "Password123",
        ///        "newPassword": "NewPassword123",
        ///        "confirmNewPassword": "NewPassword123"
        ///     }
        /// </remarks>
        [HttpPut("change-password")]
        [Authorize(Roles = "ShopOwner")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        [Tags("🔐 Authentication")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            // Kiểm tra ModelState
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Dữ liệu không hợp lệ",
                    errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList()
                });
            }

            // Lấy ShopOwnerId từ JWT token
            var shopOwnerIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            if (string.IsNullOrEmpty(shopOwnerIdClaim) || !int.TryParse(shopOwnerIdClaim, out int shopOwnerId))
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Token không hợp lệ hoặc đã hết hạn"
                });
            }

            var result = await _authService.ChangePasswordAsync(shopOwnerId, dto);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// 🔒 Employee đổi mật khẩu của chính mình (yêu cầu JWT token)
        /// </summary>
        [HttpPut("employee/change-password")]
        [Authorize(Roles = "Employee")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        [Tags("👤 Employee")]
        public async Task<IActionResult> EmployeeChangePassword([FromBody] ChangePasswordDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Dữ liệu không hợp lệ",
                    errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList()
                });
            }

            var employeeIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? 
                                 User.FindFirst("employee_id")?.Value;

            if (string.IsNullOrEmpty(employeeIdClaim) || !int.TryParse(employeeIdClaim, out int employeeId))
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Token không hợp lệ hoặc đã hết hạn"
                });
            }

            // ✅ GỌI SERVICE ĐỔI MẬT KHẨU
            var result = await _authService.ChangeEmployeePasswordAsync(employeeId, dto);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// 🔍 Employee quên mật khẩu - Gửi mật khẩu mới qua SMS
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/auth/employee/forgot-password
        ///     {
        ///        "phone": "0912345678"
        ///     }
        /// </remarks>
        [HttpPost("employee/forgot-password")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [Tags("👤 Employee")]
        public async Task<IActionResult> EmployeeForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Dữ liệu không hợp lệ",
                    errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList()
                });
            }

            // ✅ GỌI SERVICE QUÊN MẬT KHẨU EMPLOYEE
            var result = await _authService.EmployeeForgotPasswordAsync(dto);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// 🔍 Quên mật khẩu - Gửi mật khẩu mới qua SMS (ShopOwner only)
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/auth/forgot-password
        ///     {
        ///        "phone": "0912345678"
        ///     }
        /// </remarks>
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [Tags("🔐 Authentication")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            // Kiểm tra ModelState
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Dữ liệu không hợp lệ",
                    errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList()
                });
            }

            var result = await _authService.ForgotPasswordAsync(dto);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// 🧪 Test endpoint - Kiểm tra JWT token có hợp lệ không
        /// </summary>
        [HttpGet("test")]
        [Authorize(Roles = "ShopOwner,Employee")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        [Tags("🧪 Testing")]
        public IActionResult TestAuth()
        {
            var userType = User.FindFirst("user_type")?.Value;
            var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            var userName = User.FindFirst(JwtRegisteredClaimNames.Name)?.Value;
            var phone = User.FindFirst("phone")?.Value;
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            // ✅ THÊM DEBUG LOGGING CHO EMPLOYEE
            Console.WriteLine("🔍 [DEBUG] TestAuth called");
            Console.WriteLine($"  - UserType: {userType}");
            Console.WriteLine($"  - UserId (sub): {userId}");
            Console.WriteLine($"  - UserName: {userName}");
            Console.WriteLine($"  - Phone: {phone}");
            Console.WriteLine($"  - Role: {role}");

            var responseData = new
            {
                userId,
                userName,
                phone,
                role,
                userType
            };

            if (userType == "ShopOwner")
            {
                var shopName = User.FindFirst("shop_name")?.Value;
                return Ok(new
                {
                    success = true,
                    message = "Token hợp lệ - ShopOwner Authentication thành công! 👑",
                    data = new
                    {
                        shopOwnerId = userId,
                        shopOwnerName = userName,
                        phone,
                        shopName,
                        role,
                        userType,
                        message = "Chủ shop có FULL quyền truy cập toàn bộ hệ thống"
                    }
                });
            }
            else if (userType == "Employee")
            {
                var employeeId = User.FindFirst("employee_id")?.Value; // ✅ LẤY TỪ CLAIM RIÊNG
                var employeeCode = User.FindFirst("employee_code")?.Value;
                var position = User.FindFirst("position")?.Value;
                var department = User.FindFirst("department")?.Value;
                var permissions = User.FindFirst("permissions")?.Value;
                var shopOwnerId = User.FindFirst("shop_owner_id")?.Value;

                Console.WriteLine($"🔍 [DEBUG] Employee claims:");
                Console.WriteLine($"  - employee_id claim: {employeeId}");
                Console.WriteLine($"  - sub claim: {userId}");
                Console.WriteLine($"  - employee_code: {employeeCode}");

                return Ok(new
                {
                    success = true,
                    message = "Token hợp lệ - Employee Authentication thành công! 👤",
                    data = new
                    {
                        employeeId = userId, // Sử dụng sub claim
                        employeeIdFromClaim = employeeId, // Claim riêng để so sánh
                        employeeName = userName,
                        employeeCode,
                        phone,
                        position,
                        department,
                        permissions,
                        shopOwnerId,
                        role,
                        userType,
                        message = "Nhân viên có quyền truy cập theo phân quyền được cấp"
                    }
                });
            }

            return Ok(new
            {
                success = true,
                message = "Token hợp lệ nhưng không xác định được loại người dùng",
                data = responseData
            });
        }

        /// <summary>
        /// 🔄 Refresh token (Optional - để sau này mở rộng)
        /// </summary>
        [HttpPost("refresh-token")]
        [Authorize(Roles = "ShopOwner,Employee")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        [Tags("🔐 Authentication")]
        public IActionResult RefreshToken()
        {
            return Ok(new
            {
                success = false,
                message = "Chức năng Refresh Token sẽ được triển khai sau"
            });
        }

        /// <summary>
        /// 🚪 Logout (Client-side xóa token, server không cần xử lý)
        /// </summary>
        [HttpPost("logout")]
        [Authorize(Roles = "ShopOwner,Employee")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [Tags("🔐 Authentication")]
        public IActionResult Logout()
        {
            return Ok(new
            {
                success = true,
                message = "Đăng xuất thành công. Vui lòng xóa token ở phía client."
            });
        }
    }
}