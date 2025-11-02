using System.Threading.Tasks;

namespace YourShopManagement.API.Services.SmsService
{
    /// <summary>
    /// Interface cho dịch vụ gửi SMS
    /// </summary>
    public interface ISmsService
    {
        /// <summary>
        /// Gửi SMS tới số điện thoại
        /// </summary>
        /// <param name="phoneNumber">Số điện thoại nhận</param>
        /// <param name="message">Nội dung tin nhắn</param>
        /// <returns>True nếu gửi thành công, False nếu thất bại</returns>
        Task<bool> SendSmsAsync(string phoneNumber, string message);

        /// <summary>
        /// Gửi mật khẩu mới qua SMS
        /// </summary>
        /// <param name="phoneNumber">Số điện thoại nhận</param>
        /// <param name="newPassword">Mật khẩu mới</param>
        /// <returns>True nếu gửi thành công</returns>
        Task<bool> SendNewPasswordAsync(string phoneNumber, string newPassword);
    }
}
