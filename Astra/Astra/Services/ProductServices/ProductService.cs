using AutoMapper;
using YourShopManagement.API.Data;
using Microsoft.EntityFrameworkCore;
using ProductApi.DTOs;
using YourShopManagement.API.Models;

namespace Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ProductService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProductDto>> GetAllAsync()
        {
            // ⭐ CHỈ LẤY SẢN PHẨM ACTIVE VÀ CÒN HÀNG
            var items = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.Status == "active" && p.Stock > 0)
                .ToListAsync();
            return _mapper.Map<IEnumerable<ProductDto>>(items);
        }

        public async Task<ProductDto?> GetByIdAsync(int id)
        {
            var item = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductId == id);
            return item == null ? null : _mapper.Map<ProductDto>(item);
        }

        public async Task<ProductDto> CreateAsync(ProductCreateDto dto)
        {
            var entity = _mapper.Map<Product>(dto);
            _context.Products.Add(entity);
            await _context.SaveChangesAsync();
            return _mapper.Map<ProductDto>(entity);
        }

        public async Task<ProductDto?> UpdateAsync(int id, ProductUpdateDto dto)
        {
            var entity = await _context.Products.FindAsync(id);
            if (entity == null) return null;

            // Chỉ update các field không null (partial update)
            if (dto.ProductCode != null) entity.ProductCode = dto.ProductCode;
            if (dto.ProductName != null) entity.ProductName = dto.ProductName;
            if (dto.Description != null) entity.Description = dto.Description;
            if (dto.CategoryId.HasValue) entity.CategoryId = dto.CategoryId;
            if (dto.Brand != null) entity.Brand = dto.Brand;
            if (dto.SupplierName != null) entity.SupplierName = dto.SupplierName;
            if (dto.Price.HasValue) entity.Price = dto.Price.Value;
            if (dto.CostPrice.HasValue) entity.CostPrice = dto.CostPrice;
            if (dto.Stock.HasValue) entity.Stock = dto.Stock.Value;
            if (dto.MinStock.HasValue) entity.MinStock = dto.MinStock.Value;
            if (dto.Sku != null) entity.Sku = dto.Sku;
            if (dto.Barcode != null) entity.Barcode = dto.Barcode;
            if (dto.Unit != null) entity.Unit = dto.Unit;
            if (dto.ImageUrl != null) entity.ImageUrl = dto.ImageUrl;
            if (dto.Status != null) entity.Status = dto.Status;
            if (dto.Notes != null) entity.Notes = dto.Notes;
            if (dto.Weight.HasValue) entity.Weight = dto.Weight;
            if (dto.Dimension != null) entity.Dimension = dto.Dimension;

            entity.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return _mapper.Map<ProductDto>(entity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.Products.FindAsync(id);
            if (entity == null) return false;

            _context.Products.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
