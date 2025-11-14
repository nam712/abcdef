using AutoMapper;
using Microsoft.EntityFrameworkCore;
using YourShopManagement.API.DTOs.Auth;
using YourShopManagement.API.DTOs.Supplier;
using YourShopManagement.API.Models;
using YourShopManagement.API.Repositories;
using YourShopManagement.API.Helpers;
using YourShopManagement.API.Data;
using Microsoft.AspNetCore.Http;

namespace YourShopManagement.API.Services
{
    public class SupplierService : ISupplierService
    {
    private readonly ISupplierRepository _supplierRepository;
    private readonly IMapper _mapper;
    private readonly ApplicationDbContext _context;
    private readonly int _currentShopOwnerId;

        public SupplierService(ISupplierRepository supplierRepository, IMapper mapper, ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _supplierRepository = supplierRepository;
            _mapper = mapper;
            _context = context;
            
            // 🔒 Lấy shop_owner_id từ JWT token
            var claim = httpContextAccessor.HttpContext?.User?.FindFirst("shop_owner_id")?.Value;
            if (int.TryParse(claim, out var id))
                _currentShopOwnerId = id;
            else
                _currentShopOwnerId = 0;
        }

        /// <summary>
        /// Lấy danh sách tất cả nhà cung cấp
        /// </summary>
        public async Task<ApiResponse<SupplierListResponseDto>> GetAllSuppliersAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                // 🔒 Auto-filtered by ApplicationDbContext
                var (suppliers, totalCount) = await _supplierRepository.GetAllAsync(page, pageSize);
                var supplierDtos = _mapper.Map<List<SupplierInfoDto>>(suppliers);

                var response = new SupplierListResponseDto
                {
                    Suppliers = supplierDtos,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                };

                return ApiResponse<SupplierListResponseDto>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<SupplierListResponseDto>.FailResponse(
                    "Đã xảy ra lỗi khi lấy danh sách nhà cung cấp",
                    new List<string> { ex.Message }
                );
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết nhà cung cấp theo ID
        /// </summary>
        public async Task<ApiResponse<SupplierInfoDto>> GetSupplierByIdAsync(int supplierId)
        {
            try
            {
                var supplier = await _context.Suppliers
                    .FirstOrDefaultAsync(s => s.SupplierId == supplierId);

                if (supplier == null)
                {
                    return ApiResponse<SupplierInfoDto>.FailResponse("Không tìm thấy nhà cung cấp");
                }

                var supplierInfo = new SupplierInfoDto
                {
                    SupplierId = supplier.SupplierId,
                    SupplierCode = supplier.SupplierCode,
                    SupplierName = supplier.SupplierName,
                    ContactPerson = supplier.ContactPerson,
                    Phone = supplier.Phone,
                    Email = supplier.Email,
                    Address = supplier.Address,
                    TaxCode = supplier.TaxCode,
                    BankAccount = supplier.BankAccount,
                    BankName = supplier.BankName,
                    PriceList = supplier.PriceList,
                    LogoUrl = supplier.LogoUrl,
                    Status = supplier.Status,
                    Notes = supplier.Notes,
                    CreatedAt = supplier.CreatedAt,
                    UpdatedAt = supplier.UpdatedAt
                };

                return ApiResponse<SupplierInfoDto>.SuccessResponse(supplierInfo);
            }
            catch (Exception ex)
            {
                return ApiResponse<SupplierInfoDto>.FailResponse(
                    "Đã xảy ra lỗi khi lấy thông tin nhà cung cấp",
                    new List<string> { ex.Message }
                );
            }
        }

        /// <summary>
        /// Tạo nhà cung cấp mới
        /// </summary>
        public async Task<ApiResponse<SupplierInfoDto>> CreateSupplierAsync(CreateSupplierDto dto)
        {
            try
            {
                // Kiểm tra mã nhà cung cấp đã tồn tại chưa
                var existingSupplier = await _context.Suppliers
                    .FirstOrDefaultAsync(s => s.SupplierCode == dto.SupplierCode);

                if (existingSupplier != null)
                {
                    return ApiResponse<SupplierInfoDto>.FailResponse(
                        "Mã nhà cung cấp đã tồn tại",
                        new List<string> { "Supplier code already exists" }
                    );
                }

                var supplier = new Supplier
                {
                    ShopOwnerId = _currentShopOwnerId, // 🔒 Set từ JWT
                    SupplierCode = dto.SupplierCode,
                    SupplierName = dto.SupplierName,
                    ContactPerson = dto.ContactPerson,
                    Phone = dto.Phone,
                    Email = dto.Email,
                    Address = dto.Address,
                    TaxCode = dto.TaxCode,
                    BankAccount = dto.BankAccount,
                    BankName = dto.BankName,
                    PriceList = dto.PriceList,
                    LogoUrl = dto.LogoUrl,
                    Status = dto.Status,
                    Notes = dto.Notes,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Suppliers.Add(supplier);
                await _context.SaveChangesAsync();

                var supplierInfo = new SupplierInfoDto
                {
                    SupplierId = supplier.SupplierId,
                    SupplierCode = supplier.SupplierCode,
                    SupplierName = supplier.SupplierName,
                    ContactPerson = supplier.ContactPerson,
                    Phone = supplier.Phone,
                    Email = supplier.Email,
                    Address = supplier.Address,
                    TaxCode = supplier.TaxCode,
                    BankAccount = supplier.BankAccount,
                    BankName = supplier.BankName,
                    PriceList = supplier.PriceList,
                    LogoUrl = supplier.LogoUrl,
                    Status = supplier.Status,
                    Notes = supplier.Notes,
                    CreatedAt = supplier.CreatedAt,
                    UpdatedAt = supplier.UpdatedAt
                };

                return ApiResponse<SupplierInfoDto>.SuccessResponse(supplierInfo, "Tạo nhà cung cấp thành công");
            }
            catch (Exception ex)
            {
                return ApiResponse<SupplierInfoDto>.FailResponse(
                    "Đã xảy ra lỗi khi tạo nhà cung cấp",
                    new List<string> { ex.Message }
                );
            }
        }

        /// <summary>
        /// Cập nhật thông tin nhà cung cấp
        /// </summary>
        public async Task<ApiResponse<SupplierInfoDto>> UpdateSupplierAsync(int supplierId, UpdateSupplierDto dto)
        {
            try
            {
                var supplier = await _context.Suppliers
                    .FirstOrDefaultAsync(s => s.SupplierId == supplierId);

                if (supplier == null)
                {
                    return ApiResponse<SupplierInfoDto>.FailResponse("Không tìm thấy nhà cung cấp");
                }

                // Kiểm tra mã nhà cung cấp đã tồn tại chưa (trừ chính nó)
                var existingSupplier = await _context.Suppliers
                    .FirstOrDefaultAsync(s => s.SupplierCode == dto.SupplierCode && s.SupplierId != supplierId);

                if (existingSupplier != null)
                {
                    return ApiResponse<SupplierInfoDto>.FailResponse(
                        "Mã nhà cung cấp đã tồn tại",
                        new List<string> { "Supplier code already exists" }
                    );
                }

                // Cập nhật thông tin
                supplier.SupplierCode = dto.SupplierCode;
                supplier.SupplierName = dto.SupplierName;
                supplier.ContactPerson = dto.ContactPerson;
                supplier.Phone = dto.Phone;
                supplier.Email = dto.Email;
                supplier.Address = dto.Address;
                supplier.TaxCode = dto.TaxCode;
                supplier.BankAccount = dto.BankAccount;
                supplier.BankName = dto.BankName;
                supplier.PriceList = dto.PriceList;
                supplier.LogoUrl = dto.LogoUrl;
                supplier.Status = dto.Status;
                supplier.Notes = dto.Notes;
                supplier.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var supplierInfo = new SupplierInfoDto
                {
                    SupplierId = supplier.SupplierId,
                    SupplierCode = supplier.SupplierCode,
                    SupplierName = supplier.SupplierName,
                    ContactPerson = supplier.ContactPerson,
                    Phone = supplier.Phone,
                    Email = supplier.Email,
                    Address = supplier.Address,
                    TaxCode = supplier.TaxCode,
                    BankAccount = supplier.BankAccount,
                    BankName = supplier.BankName,
                    PriceList = supplier.PriceList,
                    LogoUrl = supplier.LogoUrl,
                    Status = supplier.Status,
                    Notes = supplier.Notes,
                    CreatedAt = supplier.CreatedAt,
                    UpdatedAt = supplier.UpdatedAt
                };

                return ApiResponse<SupplierInfoDto>.SuccessResponse(supplierInfo, "Cập nhật nhà cung cấp thành công");
            }
            catch (Exception ex)
            {
                return ApiResponse<SupplierInfoDto>.FailResponse(
                    "Đã xảy ra lỗi khi cập nhật nhà cung cấp",
                    new List<string> { ex.Message }
                );
            }
        }

        /// <summary>
        /// Xóa nhà cung cấp
        /// </summary>
        public async Task<ApiResponse<bool>> DeleteSupplierAsync(int supplierId)
        {
            try
            {
                var supplier = await _context.Suppliers
                    .FirstOrDefaultAsync(s => s.SupplierId == supplierId);

                if (supplier == null)
                {
                    return ApiResponse<bool>.FailResponse("Không tìm thấy nhà cung cấp");
                }

                // Kiểm tra xem nhà cung cấp có sản phẩm hoặc đơn hàng không
                var hasProducts = await _context.Products.AnyAsync(p => p.SupplierName == supplier.SupplierName);
                var hasOrders = await _context.PurchaseOrders.AnyAsync(po => po.SupplierId == supplierId);

                if (hasProducts || hasOrders)
                {
                    return ApiResponse<bool>.FailResponse(
                        "Không thể xóa nhà cung cấp vì đã có sản phẩm hoặc đơn hàng liên quan"
                    );
                }

                _context.Suppliers.Remove(supplier);
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResponse(true, "Xóa nhà cung cấp thành công");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.FailResponse(
                    "Đã xảy ra lỗi khi xóa nhà cung cấp",
                    new List<string> { ex.Message }
                );
            }
        }

        /// <summary>
        /// Tìm kiếm nhà cung cấp
        /// </summary>
        public async Task<ApiResponse<SupplierListResponseDto>> SearchSuppliersAsync(SearchSupplierDto searchDto)
        {
            try
            {
                var query = _context.Suppliers.AsQueryable();

                // Áp dụng bộ lọc
                if (!string.IsNullOrEmpty(searchDto.Name))
                {
                    query = query.Where(s => s.SupplierName.Contains(searchDto.Name));
                }

                if (!string.IsNullOrEmpty(searchDto.Status))
                {
                    query = query.Where(s => s.Status == searchDto.Status);
                }

                if (!string.IsNullOrEmpty(searchDto.ContactPerson))
                {
                    query = query.Where(s => s.ContactPerson != null && s.ContactPerson.Contains(searchDto.ContactPerson));
                }

                var totalCount = await query.CountAsync();

                var suppliers = await query
                    .OrderBy(s => s.SupplierName)
                    .Skip((searchDto.Page - 1) * searchDto.PageSize)
                    .Take(searchDto.PageSize)
                    .Select(s => new SupplierInfoDto
                    {
                        SupplierId = s.SupplierId,
                        SupplierCode = s.SupplierCode,
                        SupplierName = s.SupplierName,
                        ContactPerson = s.ContactPerson,
                        Phone = s.Phone,
                        Email = s.Email,
                        Address = s.Address,
                        TaxCode = s.TaxCode,
                        BankAccount = s.BankAccount,
                        BankName = s.BankName,
                        PriceList = s.PriceList,
                        LogoUrl = s.LogoUrl,
                        Status = s.Status,
                        Notes = s.Notes,
                        CreatedAt = s.CreatedAt,
                        UpdatedAt = s.UpdatedAt
                    })
                    .ToListAsync();

                var response = new SupplierListResponseDto
                {
                    Suppliers = suppliers,
                    TotalCount = totalCount,
                    Page = searchDto.Page,
                    PageSize = searchDto.PageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / searchDto.PageSize)
                };

                return ApiResponse<SupplierListResponseDto>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<SupplierListResponseDto>.FailResponse(
                    "Đã xảy ra lỗi khi tìm kiếm nhà cung cấp",
                    new List<string> { ex.Message }
                );
            }
        }

        /// <summary>
        /// Lấy thống kê nhà cung cấp
        /// </summary>
        public async Task<ApiResponse<SupplierStatisticsDto>> GetSupplierStatisticsAsync()
        {
            try
            {
                var totalSuppliers = await _context.Suppliers.CountAsync();
                var activeSuppliers = await _context.Suppliers.CountAsync(s => s.Status == "active");
                var inactiveSuppliers = await _context.Suppliers.CountAsync(s => s.Status == "inactive");
                
                // ❌ KHÔNG thể dùng s.Products.Any() vì không còn navigation property
                // ✅ Đếm products có supplier_name trùng với supplier
                var suppliersWithProducts = await (
                    from s in _context.Suppliers
                    join p in _context.Products on s.SupplierName equals p.SupplierName
                    select s.SupplierId
                ).Distinct().CountAsync();
                
                var suppliersWithOrders = await _context.Suppliers
                    .CountAsync(s => s.PurchaseOrders != null && s.PurchaseOrders.Any());

                var statistics = new SupplierStatisticsDto
                {
                    TotalSuppliers = totalSuppliers,
                    ActiveSuppliers = activeSuppliers,
                    InactiveSuppliers = inactiveSuppliers,
                    SuppliersWithProducts = suppliersWithProducts,
                    SuppliersWithOrders = suppliersWithOrders
                };

                return ApiResponse<SupplierStatisticsDto>.SuccessResponse(statistics);
            }
            catch (Exception ex)
            {
                return ApiResponse<SupplierStatisticsDto>.FailResponse(
                    "Đã xảy ra lỗi khi lấy thống kê nhà cung cấp",
                    new List<string> { ex.Message }
                );
            }
        }
    }
}
