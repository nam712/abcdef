using System;
using BCrypt.Net;

namespace YourShopManagement.API.Helpers
{
    /// <summary>
    /// Helper class để xử lý hash và verify password
    /// </summary>
    public static class PasswordHelper
    {
        /// <summary>
        /// Hash password sử dụng BCrypt
        /// </summary>
        /// <param name="password">Mật khẩu gốc</param>
        /// <returns>Mật khẩu đã hash</returns>
        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            // ✅ QUAN TRỌNG: Dùng BCrypt với WorkFactor = 11 (mặc định)
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 11);
            
            Console.WriteLine($"🔒 [DEBUG] Password hashed successfully");
            Console.WriteLine($"  - Original length: {password.Length}");
            Console.WriteLine($"  - Hashed length: {hashedPassword.Length}");
            Console.WriteLine($"  - Starts with '$2': {hashedPassword.StartsWith("$2")}");
            Console.WriteLine($"  - First 10 chars: {hashedPassword.Substring(0, Math.Min(10, hashedPassword.Length))}...");
            
            return hashedPassword;
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

            try
            {
                Console.WriteLine($"🔍 [DEBUG] Verifying password with BCrypt");
                Console.WriteLine($"  - Input password length: {password.Length}");
                Console.WriteLine($"  - Hashed password length: {hashedPassword.Length}");
                Console.WriteLine($"  - Hashed starts with '$2': {hashedPassword.StartsWith("$2")}");
                
                bool result = BCrypt.Net.BCrypt.Verify(password, hashedPassword);
                
                Console.WriteLine($"  - Verification result: {result}");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [DEBUG] BCrypt verification error: {ex.Message}");
                return false;
            }
        }
    }
}