using YourShopManagement.API.DTOs.Promotion;

namespace YourShopManagement.API.Services.PromotionService
{
    public interface IPromotionService
    {
        Task<IEnumerable<PromotionDto>> GetAllPromotionsAsync(int shopOwnerId);
        Task<PromotionDto?> GetPromotionByIdAsync(int id, int shopOwnerId);
        Task<PromotionDto> CreatePromotionAsync(CreatePromotionDto dto, int shopOwnerId);
        Task<PromotionDto?> UpdatePromotionAsync(int id, UpdatePromotionDto dto, int shopOwnerId);
        Task<bool> DeletePromotionAsync(int id, int shopOwnerId);
        Task<IEnumerable<PromotionDto>> SearchPromotionsAsync(string keyword, int shopOwnerId);
        Task<IEnumerable<PromotionDto>> GetPromotionsByStatusAsync(string status, int shopOwnerId);
        Task<IEnumerable<PromotionDto>> GetActivePromotionsAsync(int shopOwnerId);
        Task<IEnumerable<PromotionDto>> GetPromotionsByInvoiceIdAsync(int invoiceId, int shopOwnerId);
    }
}
