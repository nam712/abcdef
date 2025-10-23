using System;
using System.Security.Cryptography;
using System.Text;

namespace YourShopManagement.API.Helpers
{
    /// <summary>
    /// Helper class để xử lý hash và verify password
    /// </summary>
    public static class PasswordHelper
    {
        /// <summary>
        /// Hash password sử dụng SHA256
        /// </summary>
        /// <param name="password">Mật khẩu gốc</param>
        /// <returns>Mật khẩu đã hash</returns>
        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        /// <summary>
        /// Verify password với hash đã lưu
        /// </summary>
        /// <param name="password">Mật khẩu cần kiểm tra</param>
        /// <param name="hashedPassword">Mật khẩu đã hash</param>
        /// <returns>True nếu khớp, False nếu không khớp</returns>
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hashedPassword))
                return false;

            var hashedInput = HashPassword(password);
            return hashedInput == hashedPassword;
        }
    }
}