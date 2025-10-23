using YourShopManagement.API.DTOs.Auth;
using YourShopManagement.API.DTOs.Supplier;
using YourShopManagement.API.Helpers;
using Microsoft.EntityFrameworkCore;

namespace YourShopManagement.API.Services
{
    public interface ISupplierService
    {
        /// <summary>
        /// Lấy danh sách tất cả nhà cung cấp
        /// </summary>
        Task<ApiResponse<SupplierListResponseDto>> GetAllSuppliersAsync(int page = 1, int pageSize = 10);

        /// <summary>
        /// Lấy thông tin chi tiết nhà cung cấp theo ID
        /// </summary>
        Task<ApiResponse<SupplierInfoDto>> GetSupplierByIdAsync(int supplierId);

        /// <summary>
        /// Tạo nhà cung cấp mới
        /// </summary>
        Task<ApiResponse<SupplierInfoDto>> CreateSupplierAsync(CreateSupplierDto dto);

        /// <summary>
        /// Cập nhật thông tin nhà cung cấp
        /// </summary>
        Task<ApiResponse<SupplierInfoDto>> UpdateSupplierAsync(int supplierId, UpdateSupplierDto dto);

        /// <summary>
        /// Xóa nhà cung cấp
        /// </summary>
        Task<ApiResponse<bool>> DeleteSupplierAsync(int supplierId);

        /// <summary>
        /// Tìm kiếm nhà cung cấp
        /// </summary>
        Task<ApiResponse<SupplierListResponseDto>> SearchSuppliersAsync(SearchSupplierDto searchDto);

        /// <summary>
        /// Lấy thống kê nhà cung cấp
        /// </summary>
        Task<ApiResponse<SupplierStatisticsDto>> GetSupplierStatisticsAsync();
    }
}
