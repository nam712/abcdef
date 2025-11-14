using AutoMapper;
using Microsoft.EntityFrameworkCore;
using YourShopManagement.API.Data;
using YourShopManagement.API.DTOs.PurchaseOrder;
using YourShopManagement.API.Models;
using YourShopManagement.API.Repositories;

namespace YourShopManagement.API.Services
{
    public class PurchaseOrderService : IPurchaseOrderService
    {
        private readonly IPurchaseOrderRepository _repo;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PurchaseOrderService(
            IPurchaseOrderRepository repo, 
            ApplicationDbContext context,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor)
        {
            _repo = repo;
            _context = context;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<(bool, string, PurchaseOrderDto?)> CreateAsync(CreatePurchaseOrderDto dto)
        {
            if (await _repo.ExistsByCodeAsync(dto.PoCode))
                return (false, "Mã phiếu nhập đã tồn tại", null);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Lấy ShopOwnerId từ JWT
                var shopOwnerIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("shop_owner_id")?.Value;
                if (!int.TryParse(shopOwnerIdClaim, out int shopOwnerId))
                    return (false, "Không tìm thấy thông tin shop owner", null);

                // 1. Tạo PurchaseOrder
                var entity = _mapper.Map<PurchaseOrder>(dto);
                entity.ShopOwnerId = shopOwnerId;  // 🔒 Gán shop_owner_id từ JWT
                entity.Status = "pending";  // Phiếu mới luôn là pending
                entity.PurchaseOrderDetails = new List<PurchaseOrderDetail>();

                // 2. Lấy thông tin Supplier để lưu vào products
                var supplier = await _context.Suppliers
                    .FirstOrDefaultAsync(s => s.SupplierId == dto.SupplierId);
                
                if (supplier == null)
                    return (false, "Nhà cung cấp không tồn tại", null);

                // 3. Xử lý từng sản phẩm trong phiếu nhập
                decimal totalAmount = 0;
                foreach (var detailDto in dto.Details)
                {
                    // ⭐ CHECK: Sản phẩm đã tồn tại chưa (bằng ProductCode)?
                    var existingProduct = await _context.Products
                        .FirstOrDefaultAsync(p => p.ProductCode == detailDto.ProductCode);

                    int productId;

                    if (existingProduct != null)
                    {
                        // ✅ SẢN PHẨM ĐÃ TỒN TẠI → Dùng product_id có sẵn
                        productId = existingProduct.ProductId;
                        
                        // Cập nhật thông tin (NCC, giá vốn từ lần nhập gần nhất)
                        existingProduct.SupplierName = supplier.SupplierName;
                        existingProduct.CostPrice = detailDto.ImportPrice;
                        existingProduct.UpdatedAt = DateTime.UtcNow;
                    }
                    {
                        // ❌ SẢN PHẨM CHƯA TỒN TẠI → Tạo mới
                        var newProduct = new Product
                        {
                            ShopOwnerId = supplier.ShopOwnerId,  // 🔒 Sản phẩm thuộc shop_owner
                            ProductCode = detailDto.ProductCode,
                            ProductName = detailDto.ProductName,
                            CategoryId = detailDto.CategoryId,
                            Brand = detailDto.Brand,
                            SupplierName = supplier.SupplierName,
                            Price = detailDto.SuggestedPrice,
                            CostPrice = detailDto.ImportPrice,
                            Stock = 0,  // ⚠️ Chưa tăng stock (chờ confirm)
                            Unit = detailDto.Unit,
                            Barcode = detailDto.Barcode,
                            Sku = detailDto.Sku,
                            ImageUrl = detailDto.ImageUrl,
                            Weight = detailDto.Weight,
                            Dimension = detailDto.Dimension,
                            Status = "pending_import",  // Chưa active
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        _context.Products.Add(newProduct);
                        await _context.SaveChangesAsync();
                        
                        productId = newProduct.ProductId;
                    }

                    // 4. Tạo Purchase Order Detail
                    var detail = new PurchaseOrderDetail
                    {
                        ProductId = productId,  // ⭐ Dùng productId (mới hoặc cũ)
                        Quantity = detailDto.Quantity,
                        ImportPrice = detailDto.ImportPrice,
                        FinalAmount = detailDto.Quantity * detailDto.ImportPrice
                    };
                    entity.PurchaseOrderDetails.Add(detail);
                    totalAmount += detail.FinalAmount;
                }

                // 5. Cập nhật tổng tiền
                entity.TotalAmount = totalAmount;
                
                await _repo.AddAsync(entity);
                await _repo.SaveChangesAsync();
                
                await transaction.CommitAsync();

                var result = _mapper.Map<PurchaseOrderDto>(entity);
                return (true, "Tạo phiếu nhập thành công", result);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"Lỗi: {ex.Message}", null);
            }
        }

        public async Task<IEnumerable<PurchaseOrderDto>> GetAllAsync()
        {
            var list = await _repo.GetAllAsync();
            return _mapper.Map<IEnumerable<PurchaseOrderDto>>(list);
        }

        public async Task<PurchaseOrderDto?> GetByIdAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            return entity == null ? null : _mapper.Map<PurchaseOrderDto>(entity);
        }

        public async Task<(bool, string)> DeleteAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                return (false, "Không tìm thấy phiếu nhập");

            await _repo.DeleteAsync(entity);
            await _repo.SaveChangesAsync();
            return (true, "Đã xóa phiếu nhập");
        }

        /// <summary>
        /// ⭐ XÁC NHẬN NHẬP HÀNG: Tăng stock và active sản phẩm
        /// </summary>
        public async Task<(bool, string)> ConfirmPurchaseOrderAsync(int purchaseOrderId)
        {
            var purchaseOrder = await _context.PurchaseOrders
                .Include(po => po.PurchaseOrderDetails)
                    .ThenInclude(pod => pod.Product)
                .FirstOrDefaultAsync(po => po.PurchaseOrderId == purchaseOrderId);

            if (purchaseOrder == null)
                return (false, "Phiếu nhập không tồn tại");

            if (purchaseOrder.Status != "pending")
                return (false, $"Chỉ có thể xác nhận phiếu pending. Trạng thái hiện tại: {purchaseOrder.Status}");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Cập nhật stock cho từng sản phẩm
                foreach (var detail in purchaseOrder.PurchaseOrderDetails)
                {
                    var product = detail.Product;
                    
                    // ⭐ TĂNG STOCK
                    product.Stock += detail.Quantity;
                    
                    // Cập nhật giá vốn
                    product.CostPrice = detail.ImportPrice;
                    
                    // Active sản phẩm nếu đang pending_import
                    if (product.Status == "pending_import")
                    {
                        product.Status = "active";
                    }
                    
                    product.UpdatedAt = DateTime.UtcNow;
                }

                // 2. Đổi trạng thái phiếu nhập
                purchaseOrder.Status = "received";
                purchaseOrder.ActualDeliveryDate = DateTime.Now;
                purchaseOrder.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, "Xác nhận nhập hàng thành công");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"Lỗi: {ex.Message}");
            }
        }
    }
}
