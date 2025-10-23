using YourShopManagement.API.Models;

namespace YourShopManagement.API.Repositories
{
    public interface ISupplierRepository
    {
        /// <summary>
        /// Lấy tất cả nhà cung cấp với phân trang
        /// </summary>
        Task<(List<Supplier> suppliers, int totalCount)> GetAllAsync(int page, int pageSize);

        /// <summary>
        /// Lấy nhà cung cấp theo ID
        /// </summary>
        Task<Supplier?> GetByIdAsync(int supplierId);

        /// <summary>
        /// Lấy nhà cung cấp theo mã
        /// </summary>
        Task<Supplier?> GetByCodeAsync(string supplierCode);

        /// <summary>
        /// Tạo nhà cung cấp mới
        /// </summary>
        Task<Supplier> CreateAsync(Supplier supplier);

        /// <summary>
        /// Cập nhật nhà cung cấp
        /// </summary>
        Task<Supplier> UpdateAsync(Supplier supplier);

        /// <summary>
        /// Xóa nhà cung cấp
        /// </summary>
        Task<bool> DeleteAsync(int supplierId);

        /// <summary>
        /// Tìm kiếm nhà cung cấp
        /// </summary>
        Task<(List<Supplier> suppliers, int totalCount)> SearchAsync(string? name, string? status, string? contactPerson, int page, int pageSize);

        /// <summary>
        /// Kiểm tra nhà cung cấp có sản phẩm không
        /// </summary>
        Task<bool> HasProductsAsync(int supplierId);

        /// <summary>
        /// Kiểm tra nhà cung cấp có đơn hàng không
        /// </summary>
        Task<bool> HasOrdersAsync(int supplierId);

        /// <summary>
        /// Lấy thống kê nhà cung cấp
        /// </summary>
        Task<(int total, int active, int inactive, int withProducts, int withOrders)> GetStatisticsAsync();
    }
}
