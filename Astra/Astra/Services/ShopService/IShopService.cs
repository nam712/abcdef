using YourShopManagement.API.DTOs.Shop;

namespace YourShopManagement.API.Services.ShopService
{
    public interface IShopService
    {
        Task<IEnumerable<ShopDto>> GetAllAsync(int shopOwnerId);
        Task<ShopDto?> GetByIdAsync(int shopId, int shopOwnerId);
        Task<ShopDto> CreateAsync(CreateShopDto dto, int shopOwnerId);
        Task<ShopDto?> UpdateAsync(int shopId, UpdateShopDto dto, int shopOwnerId);
        Task<bool> DeleteAsync(int shopId, int shopOwnerId);
        Task<IEnumerable<ShopDto>> SearchAsync(string? keyword, int shopOwnerId);
        Task<IEnumerable<ShopDto>> GetByStatusAsync(string status, int shopOwnerId);
    }
}
