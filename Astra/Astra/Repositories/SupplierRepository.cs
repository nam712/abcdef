using Microsoft.EntityFrameworkCore;
using YourShopManagement.API.Data;
using YourShopManagement.API.Models;

namespace YourShopManagement.API.Repositories
{
    public class SupplierRepository : ISupplierRepository
    {
        private readonly ApplicationDbContext _context;

        public SupplierRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy tất cả nhà cung cấp với phân trang
        /// </summary>
        public async Task<(List<Supplier> suppliers, int totalCount)> GetAllAsync(int page, int pageSize)
        {
            var query = _context.Suppliers.AsQueryable();
            var totalCount = await query.CountAsync();

            var suppliers = await query
                .OrderBy(s => s.SupplierName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (suppliers, totalCount);
        }

        /// <summary>
        /// Lấy nhà cung cấp theo ID
        /// </summary>
        public async Task<Supplier?> GetByIdAsync(int supplierId)
        {
            return await _context.Suppliers
                .FirstOrDefaultAsync(s => s.SupplierId == supplierId);
        }

        /// <summary>
        /// Lấy nhà cung cấp theo mã
        /// </summary>
        public async Task<Supplier?> GetByCodeAsync(string supplierCode)
        {
            return await _context.Suppliers
                .FirstOrDefaultAsync(s => s.SupplierCode == supplierCode);
        }

        /// <summary>
        /// Tạo nhà cung cấp mới
        /// </summary>
        public async Task<Supplier> CreateAsync(Supplier supplier)
        {
            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();
            return supplier;
        }

        /// <summary>
        /// Cập nhật nhà cung cấp
        /// </summary>
        public async Task<Supplier> UpdateAsync(Supplier supplier)
        {
            _context.Suppliers.Update(supplier);
            await _context.SaveChangesAsync();
            return supplier;
        }

        /// <summary>
        /// Xóa nhà cung cấp
        /// </summary>
        public async Task<bool> DeleteAsync(int supplierId)
        {
            var supplier = await _context.Suppliers
                .FirstOrDefaultAsync(s => s.SupplierId == supplierId);

            if (supplier == null)
                return false;

            _context.Suppliers.Remove(supplier);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Tìm kiếm nhà cung cấp
        /// </summary>
        public async Task<(List<Supplier> suppliers, int totalCount)> SearchAsync(string? name, string? status, string? contactPerson, int page, int pageSize)
        {
            var query = _context.Suppliers.AsQueryable();

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(s => s.SupplierName.Contains(name));
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(s => s.Status == status);
            }

            if (!string.IsNullOrEmpty(contactPerson))
            {
                query = query.Where(s => s.ContactName != null && s.ContactName.Contains(contactPerson));
            }

            var totalCount = await query.CountAsync();

            var suppliers = await query
                .OrderBy(s => s.SupplierName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (suppliers, totalCount);
        }

        /// <summary>
        /// Kiểm tra nhà cung cấp có sản phẩm không
        /// </summary>
        public async Task<bool> HasProductsAsync(int supplierId)
        {
            return await _context.Products.AnyAsync(p => p.SupplierId == supplierId);
        }

        /// <summary>
        /// Kiểm tra nhà cung cấp có đơn hàng không
        /// </summary>
        public async Task<bool> HasOrdersAsync(int supplierId)
        {
            return await _context.PurchaseOrders.AnyAsync(po => po.SupplierId == supplierId);
        }

        /// <summary>
        /// Lấy thống kê nhà cung cấp
        /// </summary>
        public async Task<(int total, int active, int inactive, int withProducts, int withOrders)> GetStatisticsAsync()
        {
            var total = await _context.Suppliers.CountAsync();
            var active = await _context.Suppliers.CountAsync(s => s.Status == "active");
            var inactive = await _context.Suppliers.CountAsync(s => s.Status == "inactive");
            var withProducts = await _context.Suppliers
                .CountAsync(s => s.Products != null && s.Products.Any());
            var withOrders = await _context.Suppliers
                .CountAsync(s => s.PurchaseOrders != null && s.PurchaseOrders.Any());

            return (total, active, inactive, withProducts, withOrders);
        }
    }
}
