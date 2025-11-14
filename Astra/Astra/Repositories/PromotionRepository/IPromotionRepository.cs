using YourShopManagement.API.Models;

namespace YourShopManagement.API.Repositories.PromotionRepository
{
    public interface IPromotionRepository
    {
        Task<IEnumerable<Promotion>> GetAllAsync(int shopOwnerId);
        Task<Promotion?> GetByIdAsync(int id, int shopOwnerId);
        Task<Promotion> CreateAsync(Promotion promotion);
        Task<Promotion?> UpdateAsync(Promotion promotion);
        Task<bool> DeleteAsync(int id, int shopOwnerId);
        Task<IEnumerable<Promotion>> SearchAsync(string keyword, int shopOwnerId);
        Task<IEnumerable<Promotion>> GetByStatusAsync(string status, int shopOwnerId);
        Task<IEnumerable<Promotion>> GetActivePromotionsAsync(int shopOwnerId);
        Task<IEnumerable<Promotion>> GetByInvoiceIdAsync(int invoiceId, int shopOwnerId);
        Task<bool> PromotionCodeExistsAsync(string promotionCode, int shopOwnerId, int? excludeId = null);
    }
}
