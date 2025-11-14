using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YourShopManagement.API.Data;
using YourShopManagement.API.Common;
using YourShopManagement.API.DTOs.Invoice;
using YourShopManagement.API.Models;
using YourShopManagement.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Npgsql.Internal;
using System.Reflection.Metadata;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using System.IO;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Layout.Borders;

namespace YourShopManagement.API.Services.InvoiceService
{
    public class InvoiceService : IInvoiceService
    {
        private readonly ApplicationDbContext _context;

        public InvoiceService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tạo hóa đơn mới - TỰ ĐỘNG TRỪ TỒN KHO
        /// </summary>
        public async Task<ApiResponse<InvoiceResponseDto>> CreateAsync(CreateInvoiceDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                // 1. Kiểm tra mã hóa đơn đã tồn tại chưa
                var existingInvoice = await _context.Invoices
                    .AnyAsync(i => i.InvoiceCode == dto.InvoiceCode);

                if (existingInvoice)
                {
                    return ApiResponse<InvoiceResponseDto>.FailResponse(
                        "Mã hóa đơn đã tồn tại",
                        "Invoice code already exists"
                    );
                }

                // 2. Kiểm tra nhân viên (BẮT BUỘC)
                var employeeExists = await _context.Employees
                    .AnyAsync(e => e.EmployeeId == dto.EmployeeId);

                if (!employeeExists)
                {
                    return ApiResponse<InvoiceResponseDto>.FailResponse(
                        "Nhân viên không tồn tại",
                        "Employee not found"
                    );
                }

                // 3. Kiểm tra khách hàng (nếu có)
                if (dto.CustomerId.HasValue)
                {
                    var customer = await _context.Customers
                        .FirstOrDefaultAsync(c => c.CustomerId == dto.CustomerId.Value);

                    if (customer == null)
                    {
                        return ApiResponse<InvoiceResponseDto>.FailResponse(
                            "Khách hàng không tồn tại",
                            "Customer not found"
                        );
                    }
                }

                // 4. Kiểm tra phương thức thanh toán (nếu có)
                if (dto.PaymentMethodId.HasValue)
                {
                    var paymentMethodExists = await _context.PaymentMethods
                        .AnyAsync(pm => pm.PaymentMethodId == dto.PaymentMethodId.Value && pm.IsActive);

                    if (!paymentMethodExists)
                    {
                        return ApiResponse<InvoiceResponseDto>.FailResponse(
                            "Phương thức thanh toán không hợp lệ",
                            "Payment method not found or inactive"
                        );
                    }
                }

                // 5. Kiểm tra sản phẩm và tồn kho
                var productIds = dto.Details.Select(d => d.ProductId).Distinct().ToList();
                var products = await _context.Products
                    .Where(p => productIds.Contains(p.ProductId))
                    .ToListAsync();

                if (products.Count != productIds.Count)
                {
                    return ApiResponse<InvoiceResponseDto>.FailResponse(
                        "Một hoặc nhiều sản phẩm không tồn tại",
                        "One or more products not found"
                    );
                }

                // 6. Kiểm tra tồn kho đủ không
                foreach (var detailDto in dto.Details)
                {
                    var product = products.First(p => p.ProductId == detailDto.ProductId);
                    
                    if (product.Stock < detailDto.Quantity)
                    {
                        return ApiResponse<InvoiceResponseDto>.FailResponse(
                            $"Sản phẩm '{product.ProductName}' không đủ tồn kho. Còn lại: {product.Stock}",
                            $"Insufficient stock for product {product.ProductCode}"
                        );
                    }
                }

                // 7. Tạo hóa đơn
                var invoice = new Invoice
                {
                    InvoiceCode = dto.InvoiceCode,
                    CustomerId = dto.CustomerId,
                    EmployeeId = dto.EmployeeId,
                    InvoiceDate = dto.InvoiceDate,
                    DiscountAmount = dto.DiscountAmount,
                    AmountPaid = dto.AmountPaid,
                    PaymentMethodId = dto.PaymentMethodId,
                    Notes = dto.Notes,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // 8. Tạo chi tiết hóa đơn và TRỪ TỒN KHO
                var details = new List<InvoiceDetail>();
                decimal totalAmount = 0;

                foreach (var detailDto in dto.Details)
                {
                    var product = products.First(p => p.ProductId == detailDto.ProductId);
                    var lineTotal = detailDto.Quantity * detailDto.UnitPrice;
                    totalAmount += lineTotal;

                    // Tạo chi tiết hóa đơn
                    var detail = new InvoiceDetail
                    {
                        ProductId = detailDto.ProductId,
                        Quantity = detailDto.Quantity,
                        UnitPrice = detailDto.UnitPrice,
                        LineTotal = lineTotal,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    details.Add(detail);

                    // TRỪ TỒN KHO
                    product.Stock -= detailDto.Quantity;
                    product.UpdatedAt = DateTime.UtcNow;
                }

                invoice.TotalAmount = totalAmount;
                invoice.FinalAmount = totalAmount - dto.DiscountAmount;
                invoice.InvoiceDetails = details;

                // 9. Xác định trạng thái thanh toán - chỉ có paid hoặc unpaid
                if (dto.AmountPaid >= invoice.FinalAmount)
                {
                    invoice.PaymentStatus = "paid";
                    invoice.AmountPaid = invoice.FinalAmount;
                }
                else
                {
                    invoice.PaymentStatus = "unpaid";
                }

                // 10. Cập nhật thông tin khách hàng (nếu có)
                if (dto.CustomerId.HasValue)
                {
                    var customer = await _context.Customers.FindAsync(dto.CustomerId.Value);
                    if (customer != null)
                    {
                        customer.TotalPurchaseAmount += invoice.FinalAmount;
                        customer.TotalPurchaseCount += 1;
                        
                        // Cập nhật công nợ nếu chưa thanh toán đủ
                        if (invoice.PaymentStatus != "paid")
                        {
                            customer.TotalDebt += (invoice.FinalAmount - invoice.AmountPaid);
                        }
                        
                        customer.UpdatedAt = DateTime.UtcNow;
                    }
                }

                // 11. Lưu vào database
                _context.Invoices.Add(invoice);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // 12. Load lại với thông tin đầy đủ
                var createdInvoice = await GetInvoiceWithDetailsAsync(invoice.InvoiceId);

                return ApiResponse<InvoiceResponseDto>.SuccessResponse(
                    createdInvoice!,
                    "Tạo hóa đơn thành công"
                );
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return ApiResponse<InvoiceResponseDto>.FailResponse(
                    "Đã xảy ra lỗi khi tạo hóa đơn",
                    new List<string> { ex.Message }
                );
            }
        }

        /// <summary>
        /// Lấy thông tin hóa đơn theo ID
        /// </summary>
        public async Task<ApiResponse<InvoiceResponseDto>> GetByIdAsync(int id)
        {
            try
            {
                var invoice = await GetInvoiceWithDetailsAsync(id);

                if (invoice == null)
                {
                    return ApiResponse<InvoiceResponseDto>.FailResponse(
                        "Không tìm thấy hóa đơn",
                        new List<string> { "Invoice not found" }
                    );
                }

                return ApiResponse<InvoiceResponseDto>.SuccessResponse(invoice);
            }
            catch (Exception ex)
            {
                return ApiResponse<InvoiceResponseDto>.FailResponse(
                    "Đã xảy ra lỗi",
                    new List<string> { ex.Message }
                );
            }
        }

        /// <summary>
        /// Helper: Load hóa đơn với đầy đủ thông tin
        /// </summary>
        private async Task<InvoiceResponseDto?> GetInvoiceWithDetailsAsync(int id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Employee)
                .Include(i => i.PaymentMethod)
                .Include(i => i.Shop)
                .Include(i => i.InvoiceDetails)
                    .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(i => i.InvoiceId == id);

            if (invoice == null) return null;

            return new InvoiceResponseDto
            {
                InvoiceId = invoice.InvoiceId,
                InvoiceCode = invoice.InvoiceCode,
                ShopId = invoice.ShopId,
                ShopName = invoice.Shop?.ShopName,
                CustomerId = invoice.CustomerId,
                CustomerName = invoice.Customer?.CustomerName ?? "Khách lẻ",
                CustomerPhone = invoice.Customer?.Phone,
                EmployeeId = invoice.EmployeeId,
                EmployeeName = invoice.Employee?.EmployeeName,
                PaymentMethodId = invoice.PaymentMethodId,
                PaymentMethodName = invoice.PaymentMethod?.MethodName,
                InvoiceDate = invoice.InvoiceDate,
                TotalAmount = invoice.TotalAmount,
                DiscountAmount = invoice.DiscountAmount,
                FinalAmount = invoice.FinalAmount,
                AmountPaid = invoice.AmountPaid,
                AmountRemaining = invoice.FinalAmount - invoice.AmountPaid,
                PaymentStatus = invoice.PaymentStatus,
                Notes = invoice.Notes,
                CreatedAt = invoice.CreatedAt,
                UpdatedAt = invoice.UpdatedAt,
                Details = invoice.InvoiceDetails?.Select(d => new InvoiceDetailResponseDto
                {
                    InvoiceDetailId = d.InvoiceDetailId,
                    ProductId = d.ProductId,
                    ProductCode = d.Product.ProductCode,
                    ProductName = d.Product.ProductName,
                    Unit = d.Product.Unit,
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice,
                    LineTotal = d.LineTotal
                }).ToList() ?? new List<InvoiceDetailResponseDto>()
            };
        }

        public async Task<ApiResponse<List<InvoiceListDto>>> GetAllAsync()
        {
            try
            {
                var invoices = await _context.Invoices
                    .Include(i => i.Customer)
                    .Include(i => i.Employee)
                    .OrderByDescending(i => i.InvoiceDate)
                    .Select(i => new InvoiceListDto
                    {
                        InvoiceId = i.InvoiceId,
                        InvoiceCode = i.InvoiceCode,
                        CustomerName = i.Customer != null ? i.Customer.CustomerName : "Khách lẻ",
                        EmployeeName = i.Employee != null ? i.Employee.EmployeeName : null,
                        InvoiceDate = i.InvoiceDate,
                        TotalAmount = i.TotalAmount,
                        AmountPaid = i.AmountPaid,
                        PaymentStatus = i.PaymentStatus,
                        CreatedAt = i.CreatedAt
                    })
                    .ToListAsync();

                return ApiResponse<List<InvoiceListDto>>.SuccessResponse(invoices);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<InvoiceListDto>>.FailResponse("Lỗi khi lấy danh sách hóa đơn", ex.Message);
            }
        }

        public async Task<ApiResponse<InvoiceResponseDto>> UpdateAsync(int id, UpdateInvoiceDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var invoice = await _context.Invoices
                    .Include(i => i.InvoiceDetails)
                    .FirstOrDefaultAsync(i => i.InvoiceId == id);

                if (invoice == null)
                {
                    return ApiResponse<InvoiceResponseDto>.FailResponse(
                        message: "Không tìm thấy hóa đơn",
                        error: "Invoice not found"
                    );
                }

                if (invoice.PaymentStatus == "paid")
                {
                    return ApiResponse<InvoiceResponseDto>.FailResponse(
                        message: "Không thể cập nhật hóa đơn đã thanh toán đủ",
                        error: "Cannot update paid invoice"
                    );
                }

                // Cập nhật thông tin cơ bản
                invoice.Notes = dto.Notes;
                invoice.DiscountAmount = dto.DiscountAmount;
                invoice.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var updatedInvoice = await GetInvoiceWithDetailsAsync(id);
                return ApiResponse<InvoiceResponseDto>.SuccessResponse(updatedInvoice!, "Cập nhật hóa đơn thành công");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return ApiResponse<InvoiceResponseDto>.FailResponse("Lỗi khi cập nhật hóa đơn", ex.Message);
            }
        }

        public async Task<ApiResponse<InvoiceResponseDto>> PaymentAsync(int id, PaymentInvoiceDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var invoice = await _context.Invoices
                    .Include(i => i.Customer)
                    .FirstOrDefaultAsync(i => i.InvoiceId == id);

                if (invoice == null)
                {
                    return ApiResponse<InvoiceResponseDto>.FailResponse(
                        "Không tìm thấy hóa đơn",
                        "Invoice not found"
                    );
                }

                if (invoice.PaymentStatus == "paid")
                {
                    return ApiResponse<InvoiceResponseDto>.FailResponse(
                        "Hóa đơn đã được thanh toán",
                        "Invoice already paid"
                    );
                }

                // Kiểm tra số tiền phải bằng đúng tổng tiền (chỉ cho thanh toán toàn bộ)
                if (dto.Amount != invoice.FinalAmount)
                {
                    return ApiResponse<InvoiceResponseDto>.FailResponse(
                        $"Số tiền thanh toán phải bằng tổng tiền hóa đơn: {invoice.FinalAmount:N0}",
                        "Amount must equal invoice total"
                    );
                }

                // Tìm PaymentMethod từ PaymentType
                var paymentMethod = await _context.PaymentMethods
                    .FirstOrDefaultAsync(pm => pm.MethodCode.ToLower() == dto.PaymentType.ToLower() && pm.IsActive);

                if (paymentMethod == null)
                {
                    return ApiResponse<InvoiceResponseDto>.FailResponse(
                        "Phương thức thanh toán không hợp lệ",
                        "Invalid payment method"
                    );
                }

                // Cập nhật thanh toán
                invoice.PaymentMethodId = paymentMethod.PaymentMethodId;
                invoice.AmountPaid = invoice.FinalAmount;
                invoice.PaymentStatus = "paid";
                invoice.UpdatedAt = DateTime.UtcNow;

                // Giảm công nợ khách hàng (nếu có)
                if (invoice.Customer != null && invoice.Customer.TotalDebt > 0)
                {
                    invoice.Customer.TotalDebt = Math.Max(0, invoice.Customer.TotalDebt - invoice.FinalAmount);
                    invoice.Customer.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var updatedInvoice = await GetInvoiceWithDetailsAsync(id);

                // Trả về kết quả
                var paymentMethodName = paymentMethod.MethodName;
                var message = dto.PaymentType.ToLower() == "momo" 
                    ? $"Thanh toán qua {paymentMethodName} thành công" 
                    : $"Thanh toán tiền mặt ({invoice.FinalAmount:N0}) thành công";

                return ApiResponse<InvoiceResponseDto>.SuccessResponse(
                    updatedInvoice!, 
                    message
                );
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return ApiResponse<InvoiceResponseDto>.FailResponse(
                    "Lỗi khi thanh toán hóa đơn", 
                    ex.Message
                );
            }
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var invoice = await _context.Invoices
                    .Include(i => i.InvoiceDetails)
                        .ThenInclude(d => d.Product)
                    .Include(i => i.Customer)
                    .FirstOrDefaultAsync(i => i.InvoiceId == id);

                if (invoice == null)
                {
                    return ApiResponse<bool>.FailResponse(
                        message: "Không tìm thấy hóa đơn",
                        error: "Invoice not found"
                    );
                }

                if (invoice.PaymentStatus == "paid")
                {
                    return ApiResponse<bool>.FailResponse(
                        message: "Không thể xóa hóa đơn đã thanh toán",
                        error: "Cannot delete paid invoice"
                    );
                }

                // Hoàn lại tồn kho
                foreach (var detail in invoice.InvoiceDetails)
                {
                    detail.Product.Stock += detail.Quantity;
                    detail.Product.UpdatedAt = DateTime.UtcNow;
                }

                // Cập nhật thông tin khách hàng (nếu có)
                if (invoice.Customer != null)
                {
                    invoice.Customer.TotalPurchaseAmount -= invoice.FinalAmount;
                    invoice.Customer.TotalPurchaseCount -= 1;
                    invoice.Customer.TotalDebt -= (invoice.FinalAmount - invoice.AmountPaid);
                    invoice.Customer.UpdatedAt = DateTime.UtcNow;
                }

                // Xóa hóa đơn và chi tiết
                _context.InvoiceDetails.RemoveRange(invoice.InvoiceDetails);
                _context.Invoices.Remove(invoice);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return ApiResponse<bool>.SuccessResponse(true, "Xóa hóa đơn thành công");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return ApiResponse<bool>.FailResponse("Lỗi khi xóa hóa đơn", ex.Message);
            }
        }

        public async Task<ApiResponse<List<InvoiceListDto>>> SearchAsync(string keyword)
        {
            try
            {
                var query = _context.Invoices
                    .Include(i => i.Customer)
                    .Include(i => i.Employee)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    keyword = keyword.Trim().ToLower();
                    query = query.Where(i =>
                        i.InvoiceCode.ToLower().Contains(keyword) ||
                        (i.Customer != null && i.Customer.CustomerName.ToLower().Contains(keyword)) ||
                        (i.Customer != null && i.Customer.Phone != null && i.Customer.Phone.Contains(keyword)) ||
                        (i.Employee != null && i.Employee.EmployeeName.ToLower().Contains(keyword))
                    );
                }

                var invoices = await query
                    .OrderByDescending(i => i.InvoiceDate)
                    .Select(i => new InvoiceListDto
                    {
                        InvoiceId = i.InvoiceId,
                        InvoiceCode = i.InvoiceCode,
                        CustomerName = i.Customer != null ? i.Customer.CustomerName : "Khách lẻ",
                        EmployeeName = i.Employee != null ? i.Employee.EmployeeName : null,
                        InvoiceDate = i.InvoiceDate,
                        TotalAmount = i.TotalAmount,
                        AmountPaid = i.AmountPaid,
                        PaymentStatus = i.PaymentStatus,
                        CreatedAt = i.CreatedAt
                    })
                    .ToListAsync();

                return ApiResponse<List<InvoiceListDto>>.SuccessResponse(invoices);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<InvoiceListDto>>.FailResponse("Lỗi khi tìm kiếm hóa đơn", ex.Message);
            }
        }

        public async Task<ApiResponse<List<InvoiceListDto>>> GetByPaymentStatusAsync(string paymentStatus)
        {
            try
            {
                var invoices = await _context.Invoices
                    .Include(i => i.Customer)
                    .Include(i => i.Employee)
                    .Where(i => i.PaymentStatus == paymentStatus)
                    .OrderByDescending(i => i.InvoiceDate)
                    .Select(i => new InvoiceListDto
                    {
                        InvoiceId = i.InvoiceId,
                        InvoiceCode = i.InvoiceCode,
                        CustomerName = i.Customer != null ? i.Customer.CustomerName : "Khách lẻ",
                        EmployeeName = i.Employee != null ? i.Employee.EmployeeName : null,
                        InvoiceDate = i.InvoiceDate,
                        TotalAmount = i.TotalAmount,
                        AmountPaid = i.AmountPaid,
                        PaymentStatus = i.PaymentStatus,
                        CreatedAt = i.CreatedAt
                    })
                    .ToListAsync();

                return ApiResponse<List<InvoiceListDto>>.SuccessResponse(invoices);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<InvoiceListDto>>.FailResponse("Lỗi khi lọc hóa đơn theo trạng thái", ex.Message);
            }
        }

        public async Task<ApiResponse<List<InvoiceListDto>>> GetByCustomerIdAsync(int customerId)
        {
            try
            {
                var invoices = await _context.Invoices
                    .Include(i => i.Customer)
                    .Include(i => i.Employee)
                    .Where(i => i.CustomerId == customerId)
                    .OrderByDescending(i => i.InvoiceDate)
                    .Select(i => new InvoiceListDto
                    {
                        InvoiceId = i.InvoiceId,
                        InvoiceCode = i.InvoiceCode,
                        CustomerName = i.Customer != null ? i.Customer.CustomerName : "Khách lẻ",
                        EmployeeName = i.Employee != null ? i.Employee.EmployeeName : null,
                        InvoiceDate = i.InvoiceDate,
                        TotalAmount = i.TotalAmount,
                        AmountPaid = i.AmountPaid,
                        PaymentStatus = i.PaymentStatus,
                        CreatedAt = i.CreatedAt
                    })
                    .ToListAsync();

                return ApiResponse<List<InvoiceListDto>>.SuccessResponse(invoices);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<InvoiceListDto>>.FailResponse("Lỗi khi lấy hóa đơn theo khách hàng", ex.Message);
            }
        }
        public async Task<byte[]> GenerateInvoicePdfAsync(int invoiceId)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Employee)
                .Include(i => i.PaymentMethod)
                .Include(i => i.Shop)
                .Include(i => i.InvoiceDetails)
                    .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);

            if (invoice == null)
                return null;

            using (var ms = new MemoryStream())
            {
                var writer = new PdfWriter(ms);
                var pdf = new PdfDocument(writer);
                var document = new iText.Layout.Document(pdf);

                // ====== FONT ======
                var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                var normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

                // ====== HEADER ======
                document.Add(new Paragraph("CỬA HÀNG YOURSHOP")
                    .SetFontSize(16)
                    .SetFont(boldFont)
                    .SetTextAlignment(TextAlignment.CENTER));

                document.Add(new Paragraph("Địa chỉ: 123 Đường ABC, Quận XYZ, TP. HCM")
                    .SetFontSize(10)
                    .SetFont(normalFont)
                    .SetTextAlignment(TextAlignment.CENTER));

                document.Add(new Paragraph("SĐT: 0123 456 789 | Email: yourshop@gmail.com")
                    .SetFontSize(10)
                    .SetFont(normalFont)
                    .SetTextAlignment(TextAlignment.CENTER));

                document.Add(new Paragraph("\nHÓA ĐƠN BÁN HÀNG")
                    .SetFontSize(18)
                    .SetFont(boldFont)
                    .SetTextAlignment(TextAlignment.CENTER));

                // ====== THÔNG TIN HÓA ĐƠN ======
                document.Add(new Paragraph($"Mã hóa đơn: {invoice.InvoiceCode}")
                    .SetFontSize(11)
                    .SetFont(normalFont));
                document.Add(new Paragraph($"Ngày lập: {invoice.InvoiceDate:dd/MM/yyyy HH:mm}")
                    .SetFontSize(11)
                    .SetFont(normalFont));
                document.Add(new Paragraph($"Khách hàng: {invoice.Customer?.CustomerName ?? "N/A"}")
                    .SetFontSize(11)
                    .SetFont(normalFont));
                document.Add(new Paragraph($"SĐT: {invoice.Customer?.Phone ?? "N/A"}")
                    .SetFontSize(11)
                    .SetFont(normalFont));
                document.Add(new Paragraph($"Nhân viên: {invoice.Employee?.EmployeeName ?? "N/A"}")
                    .SetFontSize(11)
                    .SetFont(normalFont));

                document.Add(new Paragraph("------------------------------------------------------------"));

                // ====== BẢNG SẢN PHẨM ======
                var table = new iText.Layout.Element.Table(new float[] { 1, 4, 2, 2, 2 }).UseAllAvailableWidth();
                table.SetFontSize(10);

                table.AddHeaderCell(new Cell().Add(new Paragraph("STT").SetFont(boldFont)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Tên sản phẩm").SetFont(boldFont)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Số lượng").SetFont(boldFont)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Đơn giá").SetFont(boldFont)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Thành tiền").SetFont(boldFont)));

                int index = 1;
                foreach (var d in invoice.InvoiceDetails)
                {
                    table.AddCell(new Cell().Add(new Paragraph(index.ToString()).SetFont(normalFont)));
                    table.AddCell(new Cell().Add(new Paragraph(d.Product?.ProductName ?? "N/A").SetFont(normalFont)));
                    table.AddCell(new Cell().Add(new Paragraph(d.Quantity.ToString()).SetFont(normalFont)));
                    table.AddCell(new Cell().Add(new Paragraph($"{d.UnitPrice:N0} ₫").SetFont(normalFont)));
                    table.AddCell(new Cell().Add(new Paragraph($"{(d.Quantity * d.UnitPrice):N0} ₫").SetFont(normalFont)));
                    index++;
                }

                document.Add(table);
                document.Add(new Paragraph("------------------------------------------------------------"));

                // ====== TỔNG KẾT ======
                document.Add(new Paragraph($"Tổng tiền hàng: {invoice.TotalAmount:N0} ₫").SetFontSize(11).SetFont(normalFont));
                document.Add(new Paragraph($"Giảm giá: {invoice.DiscountAmount:N0} ₫").SetFontSize(11).SetFont(normalFont));
                document.Add(new Paragraph($"Tổng thanh toán: {invoice.FinalAmount:N0} ₫")
                    .SetFontSize(12)
                    .SetFont(boldFont));
                document.Add(new Paragraph($"Đã thanh toán: {invoice.AmountPaid:N0} ₫").SetFontSize(11).SetFont(normalFont));
                document.Add(new Paragraph($"Trạng thái: {(invoice.PaymentStatus == "paid" ? "Đã thanh toán" : "Chưa thanh toán")}")
                    .SetFontSize(11).SetFont(normalFont));
                document.Add(new Paragraph($"Phương thức: {invoice.PaymentMethod?.MethodName ?? "N/A"}")
                    .SetFontSize(11).SetFont(normalFont));

                document.Add(new Paragraph("\nNgười lập hóa đơn: _____________________")
                    .SetTextAlignment(TextAlignment.RIGHT)
                    .SetFontSize(11)
                    .SetFont(normalFont));

                document.Add(new Paragraph("\nXin cảm ơn quý khách đã mua hàng!")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(10)
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_OBLIQUE)));

                document.Close();
                return ms.ToArray();
            }
        }
    }
}