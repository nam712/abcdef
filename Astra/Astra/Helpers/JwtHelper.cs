using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using YourShopManagement.API.Models;
using Backend.Models;

namespace YourShopManagement.API.Helpers
{
    public class JwtHelper
    {
        private readonly IConfiguration _configuration;

        public JwtHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Generate JWT Token cho ShopOwner
        /// </summary>
        public string GenerateTokenForShopOwner(ShopOwner shopOwner)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
            var issuer = jwtSettings["Issuer"] ?? "YourShopManagementAPI_Dev";
            var audience = jwtSettings["Audience"] ?? "YourShopManagementClient_Dev";

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, shopOwner.ShopOwnerId.ToString()),
                new Claim(JwtRegisteredClaimNames.Name, shopOwner.ShopOwnerName ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new Claim(ClaimTypes.Role, "ShopOwner"),
                new Claim("user_type", "ShopOwner"),
                new Claim("phone", shopOwner.Phone),
                new Claim("shop_owner_id", shopOwner.ShopOwnerId.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Generate JWT Token cho Employee
        /// </summary>
        public string GenerateTokenForEmployee(Employee employee)
        {
            Console.WriteLine($"🔍 [DEBUG] GenerateTokenForEmployee called for Employee ID: {employee.EmployeeId}");
            
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
            var issuer = jwtSettings["Issuer"] ?? "YourShopManagementAPI_Dev";
            var audience = jwtSettings["Audience"] ?? "YourShopManagementClient_Dev";

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // ✅ SỬA: Đảm bảo EmployeeId được lưu đúng vào claim "sub"
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, employee.EmployeeId.ToString()), // ✅ QUAN TRỌNG: Employee ID
                new Claim(JwtRegisteredClaimNames.Name, employee.EmployeeName ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new Claim(ClaimTypes.Role, "Employee"),
                new Claim("user_type", "Employee"),
                new Claim("phone", employee.Phone ?? ""),
                new Claim("employee_id", employee.EmployeeId.ToString()), // ✅ THÊM CLAIM RIÊNG
                new Claim("employee_code", employee.EmployeeCode ?? ""),
                new Claim("position", employee.Position ?? ""),
                new Claim("department", employee.Department ?? ""),
                new Claim("permissions", employee.Permissions ?? "")
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            
            Console.WriteLine($"✅ [DEBUG] JWT Token generated successfully for Employee {employee.EmployeeId}");
            Console.WriteLine($"🔍 [DEBUG] Claims added:");
            Console.WriteLine($"  - sub (EmployeeId): {employee.EmployeeId}");
            Console.WriteLine($"  - name: {employee.EmployeeName}");
            Console.WriteLine($"  - employee_code: {employee.EmployeeCode}");
            
            return tokenString;
        }

        /// <summary>
        /// Generate Token (backward compatibility - cho ShopOwner)
        /// </summary>
        public string GenerateToken(ShopOwner shopOwner)
        {
            return GenerateTokenForShopOwner(shopOwner);
        }
    }
}