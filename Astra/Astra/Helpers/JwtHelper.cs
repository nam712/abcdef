using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using YourShopManagement.API.Models;

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
        public string GenerateToken(ShopOwner shopOwner)
        {
            var secretKey = _configuration["JwtSettings:SecretKey"];
            var issuer = _configuration["JwtSettings:Issuer"];
            var audience = _configuration["JwtSettings:Audience"];
            var expiryInMinutes = int.Parse(_configuration["JwtSettings:ExpiryInMinutes"] ?? "60");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, shopOwner.ShopOwnerId.ToString()),
                new Claim(JwtRegisteredClaimNames.Name, shopOwner.ShopOwnerName),
                new Claim(JwtRegisteredClaimNames.Email, shopOwner.Email ?? ""),
                new Claim("phone", shopOwner.Phone),
                new Claim("shop_name", shopOwner.ShopName),
                new Claim("role", "ShopOwner"), // Role chủ shop
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryInMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Validate JWT Token
        /// </summary>
        public ClaimsPrincipal? ValidateToken(string token)
        {
            try
            {
                var secretKey = _configuration["JwtSettings:SecretKey"];
                var issuer = _configuration["JwtSettings:Issuer"];
                var audience = _configuration["JwtSettings:Audience"];

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(secretKey!);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return principal;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Lấy ShopOwnerId từ token
        /// </summary>
        public int? GetShopOwnerIdFromToken(string token)
        {
            var principal = ValidateToken(token);
            if (principal == null) return null;

            var subClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub);
            if (subClaim != null && int.TryParse(subClaim.Value, out int shopOwnerId))
            {
                return shopOwnerId;
            }

            return null;
        }
    }
}