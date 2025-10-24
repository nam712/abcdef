using System.Collections.Generic;
using System.Threading.Tasks;
using YourShopManagement.API.DTOs.Auth;
using YourShopManagement.API.DTOs.PurchaseOrder;

namespace YourShopManagement.API.Services.Interfaces
{
    /// <summary>
    /// Interface cho Purchase Order Service
    /// </summary>
    public interface IPurchaseOrderService
    {
        /// <summary>
        /// Tạo phiếu nhập hàng mới
        /// </summary>
        Task<ApiResponse<PurchaseOrderResponseDto>> CreateAsync(CreatePurchaseOrderDto dto);

        /// <summary>
        /// Lấy thông tin phiếu nhập theo ID
        /// </summary>
        Task<ApiResponse<PurchaseOrderResponseDto>> GetByIdAsync(int id);

        /// <summary>
        /// Lấy danh sách tất cả phiếu nhập
        /// </summary>
        Task<ApiResponse<List<PurchaseOrderListDto>>> GetAllAsync();

        /// <summary>
        /// Cập nhật thông tin phiếu nhập
        /// </summary>
        Task<ApiResponse<PurchaseOrderResponseDto>> UpdateAsync(int id, UpdatePurchaseOrderDto dto);

        /// <summary>
        /// Xác nhận nhận hàng (cập nhật tồn kho)
        /// </summary>
        Task<ApiResponse<PurchaseOrderResponseDto>> ReceiveOrderAsync(int id, ReceivePurchaseOrderDto dto);

        /// <summary>
        /// Hủy phiếu nhập
        /// </summary>
        Task<ApiResponse<bool>> CancelAsync(int id);

        /// <summary>
        /// Xóa phiếu nhập
        /// </summary>
        Task<ApiResponse<bool>> DeleteAsync(int id);

        /// <summary>
        /// Tìm kiếm phiếu nhập theo mã hoặc nhà cung cấp
        /// </summary>
        Task<ApiResponse<List<PurchaseOrderListDto>>> SearchAsync(string keyword);

        /// <summary>
        /// Lấy phiếu nhập theo trạng thái
        /// </summary>
        Task<ApiResponse<List<PurchaseOrderListDto>>> GetByStatusAsync(string status);
    }
}