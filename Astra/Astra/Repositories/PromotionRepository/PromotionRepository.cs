using Microsoft.EntityFrameworkCore;
using YourShopManagement.API.Data;
using YourShopManagement.API.Models;

namespace YourShopManagement.API.Repositories.PromotionRepository
{
    public class PromotionRepository : IPromotionRepository
    {
        private readonly ApplicationDbContext _context;

        public PromotionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Promotion>> GetAllAsync(int shopOwnerId)
        {
            return await _context.Promotions
                .Include(p => p.Invoice)
                .Where(p => p.ShopOwnerId == shopOwnerId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Promotion?> GetByIdAsync(int id, int shopOwnerId)
        {
            return await _context.Promotions
                .Include(p => p.Invoice)
                .FirstOrDefaultAsync(p => p.PromotionId == id && p.ShopOwnerId == shopOwnerId);
        }

        public async Task<Promotion> CreateAsync(Promotion promotion)
        {
            _context.Promotions.Add(promotion);
            await _context.SaveChangesAsync();
            return promotion;
        }

        public async Task<Promotion?> UpdateAsync(Promotion promotion)
        {
            _context.Promotions.Update(promotion);
            await _context.SaveChangesAsync();
            return promotion;
        }

        public async Task<bool> DeleteAsync(int id, int shopOwnerId)
        {
            var promotion = await GetByIdAsync(id, shopOwnerId);
            if (promotion == null)
                return false;

            _context.Promotions.Remove(promotion);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Promotion>> SearchAsync(string keyword, int shopOwnerId)
        {
            var query = _context.Promotions
                .Include(p => p.Invoice)
                .Where(p => p.ShopOwnerId == shopOwnerId);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.ToLower();
                query = query.Where(p =>
                    p.PromotionCode.ToLower().Contains(keyword) ||
                    p.PromotionName.ToLower().Contains(keyword) ||
                    (p.Description != null && p.Description.ToLower().Contains(keyword))
                );
            }

            return await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
        }

        public async Task<IEnumerable<Promotion>> GetByStatusAsync(string status, int shopOwnerId)
        {
            return await _context.Promotions
                .Include(p => p.Invoice)
                .Where(p => p.Status == status && p.ShopOwnerId == shopOwnerId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Promotion>> GetActivePromotionsAsync(int shopOwnerId)
        {
            var now = DateTime.UtcNow;
            return await _context.Promotions
                .Include(p => p.Invoice)
                .Where(p => p.ShopOwnerId == shopOwnerId &&
                           p.Status == "active" &&
                           p.StartDate <= now &&
                           p.EndDate >= now)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Promotion>> GetByInvoiceIdAsync(int invoiceId, int shopOwnerId)
        {
            return await _context.Promotions
                .Include(p => p.Invoice)
                .Where(p => p.InvoiceId == invoiceId && p.ShopOwnerId == shopOwnerId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> PromotionCodeExistsAsync(string promotionCode, int shopOwnerId, int? excludeId = null)
        {
            var query = _context.Promotions
                .Where(p => p.PromotionCode == promotionCode && p.ShopOwnerId == shopOwnerId);

            if (excludeId.HasValue)
            {
                query = query.Where(p => p.PromotionId != excludeId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
