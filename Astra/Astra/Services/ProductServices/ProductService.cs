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
            var items = await _context.Products
                .Include(p => p.Supplier)
                .Include(p => p.Category)
                .ToListAsync();
            return _mapper.Map<IEnumerable<ProductDto>>(items);
        }

        public async Task<ProductDto?> GetByIdAsync(int id)
        {
            var item = await _context.Products
                .Include(p => p.Supplier)
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

        public async Task<ProductDto?> UpdateAsync(int id, ProductCreateDto dto)
        {
            var entity = await _context.Products.FindAsync(id);
            if (entity == null) return null;

            _mapper.Map(dto, entity);
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
