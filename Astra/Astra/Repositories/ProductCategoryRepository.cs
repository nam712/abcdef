using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YourShopManagement.API.Data;
using YourShopManagement.API.Models;

namespace YourShopManagement.API.Repositories
{
    public class ProductCategoryRepository : IProductCategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductCategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProductCategory>> GetAllAsync()
        {
            return await _context.ProductCategories
                .Include(c => c.Children)
                .ToListAsync();
        }

        public async Task<ProductCategory?> GetByIdAsync(int id)
        {
            return await _context.ProductCategories
                .Include(c => c.Children)
                .FirstOrDefaultAsync(c => c.CategoryId == id);
        }

        public async Task<ProductCategory> AddAsync(ProductCategory category)
        {
            _context.ProductCategories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task UpdateAsync(ProductCategory category)
        {
            _context.ProductCategories.Update(category);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var category = await _context.ProductCategories.FindAsync(id);
            if (category != null)
            {
                _context.ProductCategories.Remove(category);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<ProductCategory>> GetChildrenAsync(int parentId)
        {
            return await _context.ProductCategories
                .Where(c => c.ParentCategoryId == parentId)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductCategory>> SearchAsync(string keyword)
        {
            return await _context.ProductCategories
                .Where(c => c.CategoryName.Contains(keyword) ||
                            c.Description!.Contains(keyword))
                .ToListAsync();
        }
    }
}
