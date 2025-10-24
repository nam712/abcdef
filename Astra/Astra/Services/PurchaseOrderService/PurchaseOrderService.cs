using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YourShopManagement.API.Data;
using YourShopManagement.API.DTOs.Auth;
using YourShopManagement.API.DTOs.PurchaseOrder;
using YourShopManagement.API.Models;
using YourShopManagement.API.Services.Interfaces;

namespace YourShopManagement.API.Services.Implementations
{
    public class PurchaseOrderService : IPurchaseOrderService
    {
        private readonly ApplicationDbContext _context;

        public PurchaseOrderService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tạo phiếu nhập hàng mới
        /// </summary>
        public async Task<ApiResponse<PurchaseOrderResponseDto>> CreateAsync(CreatePurchaseOrderDto dto)
        {
            try
            {
                // 1. Kiểm tra mã phiếu nhập đã tồn tại chưa
                var existingPo = await _context.PurchaseOrders
                    .AnyAsync(po => po.PoCode == dto.PoCode);

                if (existingPo)
                {
                    return ApiResponse<PurchaseOrderResponseDto>.FailResponse(
                        "Mã phiếu nhập đã tồn tại",
                        new List<string> { "PO code already exists" }
                    );
                }

                // 2. Kiểm tra nhà cung cấp có tồn tại không
                var supplier = await _context.Suppliers
                    .FirstOrDefaultAsync(s => s.SupplierId == dto.SupplierId);

                if (supplier == null)
                {
                    return ApiResponse<PurchaseOrderResponseDto>.FailResponse(
                        "Nhà cung cấp không tồn tại",
                        new List<string> { "Supplier not found" }
                    );
                }

                // 3. Kiểm tra tất cả sản phẩm có tồn tại không
                var productIds = dto.Details.Select(d => d.ProductId).Distinct().ToList();
                var products = await _context.Products
                    .Where(p => productIds.Contains(p.ProductId))
                    .ToListAsync();

                if (products.Count != productIds.Count)
                {
                    return ApiResponse<PurchaseOrderResponseDto>.FailResponse(
                        "Một hoặc nhiều sản phẩm không tồn tại",
                        new List<string> { "One or more products not found" }
                    );
                }

                // 4. Tạo phiếu nhập
                var purchaseOrder = new PurchaseOrder
                {
                    PoCode = dto.PoCode,
                    SupplierId = dto.SupplierId,
                    PoDate = dto.PoDate,
                    ExpectedDeliveryDate = dto.ExpectedDeliveryDate,
                    Status = "pending",
                    PaymentStatus = "unpaid",
                    Notes = dto.Notes,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // 5. Tạo chi tiết phiếu nhập
                var details = new List<PurchaseOrderDetail>();
                decimal totalAmount = 0;

                foreach (var detailDto in dto.Details)
                {
                    var finalAmount = detailDto.Quantity * detailDto.ImportPrice;
                    totalAmount += finalAmount;

                    var detail = new PurchaseOrderDetail
                    {
                        ProductId = detailDto.ProductId,
                        Quantity = detailDto.Quantity,
                        ImportPrice = detailDto.ImportPrice,
                        FinalAmount = finalAmount
                    };

                    details.Add(detail);
                }

                purchaseOrder.TotalAmount = totalAmount;
                purchaseOrder.PurchaseOrderDetails = details;

                // 6. Lưu vào database
                _context.PurchaseOrders.Add(purchaseOrder);
                await _context.SaveChangesAsync();

                // 7. Load lại với thông tin đầy đủ
                var createdPo = await GetPurchaseOrderWithDetailsAsync(purchaseOrder.PurchaseOrderId);

                return ApiResponse<PurchaseOrderResponseDto>.SuccessResponse(
                    createdPo!,
                    "Tạo phiếu nhập hàng thành công"
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<PurchaseOrderResponseDto>.FailResponse(
                    "Đã xảy ra lỗi khi tạo phiếu nhập",
                    new List<string> { ex.Message }
                );
            }
        }

        /// <summary>
        /// Lấy thông tin phiếu nhập theo ID
        /// </summary>
        public async Task<ApiResponse<PurchaseOrderResponseDto>> GetByIdAsync(int id)
        {
            try
            {
                var purchaseOrder = await GetPurchaseOrderWithDetailsAsync(id);

                if (purchaseOrder == null)
                {
                    return ApiResponse<PurchaseOrderResponseDto>.FailResponse(
                        "Không tìm thấy phiếu nhập",
                        new List<string> { "Purchase order not found" }
                    );
                }

                return ApiResponse<PurchaseOrderResponseDto>.SuccessResponse(purchaseOrder);
            }
            catch (Exception ex)
            {
                return ApiResponse<PurchaseOrderResponseDto>.FailResponse(
                    "Đã xảy ra lỗi",
                    new List<string> { ex.Message }
                );
            }
        }

        /// <summary>
        /// Helper: Load phiếu nhập với đầy đủ thông tin
        /// </summary>
        private async Task<PurchaseOrderResponseDto?> GetPurchaseOrderWithDetailsAsync(int id)
        {
            var po = await _context.PurchaseOrders
                .Include(p => p.Supplier)
                .Include(p => p.PurchaseOrderDetails)
                    .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(p => p.PurchaseOrderId == id);

            if (po == null) return null;

            return new PurchaseOrderResponseDto
            {
                PurchaseOrderId = po.PurchaseOrderId,
                PoCode = po.PoCode,
                SupplierId = po.SupplierId,
                SupplierName = po.Supplier.SupplierName,
                SupplierPhone = po.Supplier.Phone,
                PoDate = po.PoDate,
                ExpectedDeliveryDate = po.ExpectedDeliveryDate,
                ActualDeliveryDate = po.ActualDeliveryDate,
                TotalAmount = po.TotalAmount,
                Status = po.Status,
                PaymentStatus = po.PaymentStatus,
                Notes = po.Notes,
                CreatedAt = po.CreatedAt,
                UpdatedAt = po.UpdatedAt,
                Details = po.PurchaseOrderDetails?.Select(d => new PurchaseOrderDetailResponseDto
                {
                    PurchaseOrderDetailId = d.PurchaseOrderDetailId,
                    ProductId = d.ProductId,
                    ProductCode = d.Product.ProductCode,
                    ProductName = d.Product.ProductName,
                    Unit = d.Product.Unit,
                    Quantity = d.Quantity,
                    ImportPrice = d.ImportPrice,
                    FinalAmount = d.FinalAmount
                }).ToList() ?? new List<PurchaseOrderDetailResponseDto>()
            };
        }

        // Các method khác sẽ implement ở Part 2
        public Task<ApiResponse<List<PurchaseOrderListDto>>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ApiResponse<PurchaseOrderResponseDto>> UpdateAsync(int id, UpdatePurchaseOrderDto dto)
        {
            throw new NotImplementedException();
        }

        public Task<ApiResponse<PurchaseOrderResponseDto>> ReceiveOrderAsync(int id, ReceivePurchaseOrderDto dto)
        {
            throw new NotImplementedException();
        }

        public Task<ApiResponse<bool>> CancelAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<ApiResponse<List<PurchaseOrderListDto>>> SearchAsync(string keyword)
        {
            throw new NotImplementedException();
        }

        public Task<ApiResponse<List<PurchaseOrderListDto>>> GetByStatusAsync(string status)
        {
            throw new NotImplementedException();
        }
    }
}