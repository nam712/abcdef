using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace YourShopManagement.API.Services.SmsService
{
    /// <summary>
    /// Dịch vụ gửi SMS
    /// Bạn có thể tích hợp với các nhà cung cấp như:
    /// - Twilio (quốc tế)
    /// - Esms.vn (Việt Nam)
    /// - Vietguys (Việt Nam)
    /// - Hoặc giả lập trong môi trường development
    /// </summary>
    public class SmsService : ISmsService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SmsService> _logger;
        private readonly HttpClient _httpClient;
        private readonly bool _isProduction;

        public SmsService(
            IConfiguration configuration, 
            ILogger<SmsService> logger, 
            HttpClient httpClient)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClient;
            _isProduction = _configuration.GetValue<bool>("Sms:IsProduction");
        }

        /// <summary>
        /// Gửi SMS
        /// </summary>
        public async Task<bool> SendSmsAsync(string phoneNumber, string message)
        {
            try
            {
                // Nếu không phải môi trường production, chỉ log ra console
                if (!_isProduction)
                {
                    _logger.LogInformation("===========================================");
                    _logger.LogInformation("📱 SMS SIMULATION (Development Mode)");
                    _logger.LogInformation($"To: {phoneNumber}");
                    _logger.LogInformation($"Message: {message}");
                    _logger.LogInformation($"Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    _logger.LogInformation("===========================================");
                    Console.WriteLine($"\n📱 [SMS] To: {phoneNumber}\n📝 Message: {message}\n");
                    return true;
                }

                // Tích hợp với API SMS thật (ví dụ: Esms.vn)
                return await SendViaSmsProviderAsync(phoneNumber, message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Lỗi khi gửi SMS: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gửi mật khẩu mới qua SMS
        /// </summary>
        public async Task<bool> SendNewPasswordAsync(string phoneNumber, string newPassword)
        {
            var message = $"[Astra Shop] Mat khau moi cua ban la: {newPassword}\n" +
                         $"Vui long dang nhap va doi mat khau ngay.\n" +
                         $"Khong chia se mat khau voi bat ky ai!";

            return await SendSmsAsync(phoneNumber, message);
        }

        /// <summary>
        /// Gửi SMS qua nhà cung cấp thực tế
        /// Ví dụ này sử dụng Esms.vn API
        /// </summary>
        private async Task<bool> SendViaSmsProviderAsync(string phoneNumber, string message)
        {
            try
            {
                var apiKey = _configuration["Sms:ApiKey"];
                var secretKey = _configuration["Sms:SecretKey"];
                var brandName = _configuration["Sms:BrandName"] ?? "Astra";
                var apiUrl = _configuration["Sms:ApiUrl"];

                if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiUrl))
                {
                    _logger.LogWarning("⚠️ SMS API chưa được cấu hình");
                    return false;
                }

                // Chuẩn bị request cho Esms.vn (hoặc provider khác)
                var requestData = new
                {
                    Phone = phoneNumber,
                    Content = message,
                    ApiKey = apiKey,
                    SecretKey = secretKey,
                    SmsType = 8 // 8 = SMS số ngẫu nhiên (không cần Brandname)
                };

                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation($"✅ SMS sent successfully: {responseContent}");
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"❌ SMS sending failed: {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Exception when sending SMS via provider: {ex.Message}");
                return false;
            }
        }
    }
}
