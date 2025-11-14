using YourShopManagement.API.Models;

namespace YourShopManagement.API.Repositories.ShopRepository
{
    public interface IShopRepository
    {
        Task<IEnumerable<Shop>> GetAllAsync(int shopOwnerId);
        Task<Shop?> GetByIdAsync(int shopId, int shopOwnerId);
        Task<Shop> CreateAsync(Shop shop);
        Task<Shop> UpdateAsync(Shop shop);
        Task<bool> DeleteAsync(int shopId, int shopOwnerId);
        Task<IEnumerable<Shop>> SearchAsync(string? keyword, int shopOwnerId);
        Task<IEnumerable<Shop>> GetByStatusAsync(string status, int shopOwnerId);
        Task<bool> ExistsAsync(int shopId, int shopOwnerId);
        Task<bool> ShopCodeExistsAsync(string shopCode, int shopOwnerId, int? excludeShopId = null);
    }
}
