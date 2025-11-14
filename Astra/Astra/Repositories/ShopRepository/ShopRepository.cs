using Microsoft.EntityFrameworkCore;
using YourShopManagement.API.Data;
using YourShopManagement.API.Models;

namespace YourShopManagement.API.Repositories.ShopRepository
{
    public class ShopRepository : IShopRepository
    {
        private readonly ApplicationDbContext _context;

        public ShopRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Shop>> GetAllAsync(int shopOwnerId)
        {
            return await _context.Shops
                .Include(s => s.BusinessCategory)
                .Where(s => s.ShopOwnerId == shopOwnerId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<Shop?> GetByIdAsync(int shopId, int shopOwnerId)
        {
            return await _context.Shops
                .Include(s => s.BusinessCategory)
                .FirstOrDefaultAsync(s => s.ShopId == shopId && s.ShopOwnerId == shopOwnerId);
        }

        public async Task<Shop> CreateAsync(Shop shop)
        {
            _context.Shops.Add(shop);
            await _context.SaveChangesAsync();
            
            // Load BusinessCategory nếu có
            if (shop.BusinessCategoryId.HasValue)
            {
                await _context.Entry(shop)
                    .Reference(s => s.BusinessCategory)
                    .LoadAsync();
            }
            
            return shop;
        }

        public async Task<Shop> UpdateAsync(Shop shop)
        {
            shop.UpdatedAt = DateTime.UtcNow;
            _context.Shops.Update(shop);
            await _context.SaveChangesAsync();
            
            // Load BusinessCategory nếu có
            if (shop.BusinessCategoryId.HasValue)
            {
                await _context.Entry(shop)
                    .Reference(s => s.BusinessCategory)
                    .LoadAsync();
            }
            
            return shop;
        }

        public async Task<bool> DeleteAsync(int shopId, int shopOwnerId)
        {
            var shop = await GetByIdAsync(shopId, shopOwnerId);
            if (shop == null)
            {
                return false;
            }

            // Soft delete - chuyển status thành inactive
            shop.Status = "inactive";
            shop.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            
            return true;
        }

        public async Task<IEnumerable<Shop>> SearchAsync(string? keyword, int shopOwnerId)
        {
            var query = _context.Shops
                .Include(s => s.BusinessCategory)
                .Where(s => s.ShopOwnerId == shopOwnerId);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.ToLower();
                query = query.Where(s =>
                    s.ShopCode.ToLower().Contains(keyword) ||
                    s.ShopName.ToLower().Contains(keyword) ||
                    (s.ShopAddress != null && s.ShopAddress.ToLower().Contains(keyword)) ||
                    (s.ShopPhone != null && s.ShopPhone.Contains(keyword)) ||
                    (s.ManagerName != null && s.ManagerName.ToLower().Contains(keyword))
                );
            }

            return await query
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Shop>> GetByStatusAsync(string status, int shopOwnerId)
        {
            return await _context.Shops
                .Include(s => s.BusinessCategory)
                .Where(s => s.ShopOwnerId == shopOwnerId && s.Status == status)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(int shopId, int shopOwnerId)
        {
            return await _context.Shops
                .AnyAsync(s => s.ShopId == shopId && s.ShopOwnerId == shopOwnerId);
        }

        public async Task<bool> ShopCodeExistsAsync(string shopCode, int shopOwnerId, int? excludeShopId = null)
        {
            var query = _context.Shops
                .Where(s => s.ShopOwnerId == shopOwnerId && s.ShopCode == shopCode);

            if (excludeShopId.HasValue)
            {
                query = query.Where(s => s.ShopId != excludeShopId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
