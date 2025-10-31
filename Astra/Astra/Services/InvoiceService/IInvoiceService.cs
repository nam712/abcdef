using System.Collections.Generic;
using System.Threading.Tasks;
using YourShopManagement.API.Common;
using YourShopManagement.API.DTOs.Invoice;

namespace YourShopManagement.API.Services.Interfaces
{
    /// <summary>
    /// Interface cho Invoice Service
    /// </summary>
    public interface IInvoiceService
    {
        /// <summary>
        /// Tạo hóa đơn mới (tự động trừ tồn kho)
        /// </summary>
        Task<ApiResponse<InvoiceResponseDto>> CreateAsync(CreateInvoiceDto dto);

        /// <summary>
        /// Lấy thông tin hóa đơn theo ID
        /// </summary>
        Task<ApiResponse<InvoiceResponseDto>> GetByIdAsync(int id);

        /// <summary>
        /// Lấy danh sách tất cả hóa đơn
        /// </summary>
        Task<ApiResponse<List<InvoiceListDto>>> GetAllAsync();

        /// <summary>
        /// Cập nhật thông tin hóa đơn
        /// </summary>
        Task<ApiResponse<InvoiceResponseDto>> UpdateAsync(int id, UpdateInvoiceDto dto);

        /// <summary>
        /// Thanh toán hóa đơn (toàn bộ hoặc một phần)
        /// </summary>
        Task<ApiResponse<InvoiceResponseDto>> PaymentAsync(int id, PaymentInvoiceDto dto);

        /// <summary>
        /// Xóa hóa đơn (hoàn tồn kho)
        /// </summary>
        Task<ApiResponse<bool>> DeleteAsync(int id);

        /// <summary>
        /// Tìm kiếm hóa đơn theo mã hoặc khách hàng
        /// </summary>
        Task<ApiResponse<List<InvoiceListDto>>> SearchAsync(string keyword);

        /// <summary>
        /// Lấy hóa đơn theo trạng thái thanh toán
        /// </summary>
        Task<ApiResponse<List<InvoiceListDto>>> GetByPaymentStatusAsync(string paymentStatus);

        /// <summary>
        /// Lấy hóa đơn theo khách hàng
        /// </summary>
        Task<ApiResponse<List<InvoiceListDto>>> GetByCustomerIdAsync(int customerId);

        /// <summary>
        /// In hóa đơn  
        /// </summary>
        Task<byte[]> GenerateInvoicePdfAsync(int invoiceId);

    }
}