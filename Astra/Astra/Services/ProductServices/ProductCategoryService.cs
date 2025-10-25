using AutoMapper;
using YourShopManagement.API.Data;
using Microsoft.EntityFrameworkCore;
using ProductCategoryApi.DTOs;
using YourShopManagement.API.Models;

namespace Services
{
    public class ProductCategoryService : IProductCategoryService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ProductCategoryService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProductCategoryDto>> GetAllAsync()
        {
            var list = await _context.ProductCategories
                .Include(c => c.Children)
                .ToListAsync();
            return _mapper.Map<IEnumerable<ProductCategoryDto>>(list);
        }

        public async Task<ProductCategoryDto?> GetByIdAsync(int id)
        {
            var item = await _context.ProductCategories
                .Include(c => c.Children)
                .FirstOrDefaultAsync(c => c.CategoryId == id);
            return item == null ? null : _mapper.Map<ProductCategoryDto>(item);
        }

        public async Task<ProductCategoryDto> CreateAsync(ProductCategoryCreateDto dto)
        {
            var entity = _mapper.Map<ProductCategory>(dto);
            _context.ProductCategories.Add(entity);
            await _context.SaveChangesAsync();
            return _mapper.Map<ProductCategoryDto>(entity);
        }

        public async Task<ProductCategoryDto?> UpdateAsync(int id, ProductCategoryCreateDto dto)
        {
            var entity = await _context.ProductCategories.FindAsync(id);
            if (entity == null) return null;

            _mapper.Map(dto, entity);
            await _context.SaveChangesAsync();
            return _mapper.Map<ProductCategoryDto>(entity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.ProductCategories.FindAsync(id);
            if (entity == null) return false;

            _context.ProductCategories.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
