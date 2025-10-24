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
        /// 🔑 Đăng nhập vào hệ thống
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/auth/login
        ///     {
        ///        "phone": "0912345678",
        ///        "password": "Password123"
        ///     }
        /// </remarks>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        [Tags("🔐 Authentication")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
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

            var result = await _authService.LoginAsync(dto);

            if (!result.Success)
            {
                return Unauthorized(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// 👤 Lấy thông tin profile ShopOwner (yêu cầu JWT token)
        /// </summary>
        [HttpGet("profile")]
        [Authorize(Roles = "ShopOwner")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        [Tags("👤 User Profile")]
        public async Task<IActionResult> GetProfile()
        {
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

            var result = await _authService.GetProfileAsync(shopOwnerId);

            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// 🔒 Đổi mật khẩu (yêu cầu JWT token)
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
        /// 🧪 Test endpoint - Kiểm tra JWT token có hợp lệ không
        /// </summary>
        [HttpGet("test")]
        [Authorize(Roles = "ShopOwner")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        [Tags("🧪 Testing")]
        public IActionResult TestAuth()
        {
            var shopOwnerId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            var shopOwnerName = User.FindFirst(JwtRegisteredClaimNames.Name)?.Value;
            var phone = User.FindFirst("phone")?.Value;
            var shopName = User.FindFirst("shop_name")?.Value;
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            return Ok(new
            {
                success = true,
                message = "Token hợp lệ - Authentication thành công! ✅",
                data = new
                {
                    shopOwnerId,
                    shopOwnerName,
                    phone,
                    shopName,
                    role,
                    message = "Chủ shop có FULL quyền truy cập toàn bộ hệ thống"
                }
            });
        }

        /// <summary>
        /// 🔄 Refresh token (Optional - để sau này mở rộng)
        /// </summary>
        [HttpPost("refresh-token")]
        [Authorize(Roles = "ShopOwner")]
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
        [Authorize(Roles = "ShopOwner")]
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