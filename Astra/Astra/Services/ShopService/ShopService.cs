using AutoMapper;
using YourShopManagement.API.DTOs.Shop;
using YourShopManagement.API.Models;
using YourShopManagement.API.Repositories.ShopRepository;

namespace YourShopManagement.API.Services.ShopService
{
    public class ShopService : IShopService
    {
        private readonly IShopRepository _repository;
        private readonly IMapper _mapper;

        public ShopService(IShopRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ShopDto>> GetAllAsync(int shopOwnerId)
        {
            var shops = await _repository.GetAllAsync(shopOwnerId);
            return _mapper.Map<IEnumerable<ShopDto>>(shops);
        }

        public async Task<ShopDto?> GetByIdAsync(int shopId, int shopOwnerId)
        {
            var shop = await _repository.GetByIdAsync(shopId, shopOwnerId);
            return shop == null ? null : _mapper.Map<ShopDto>(shop);
        }

        public async Task<ShopDto> CreateAsync(CreateShopDto dto, int shopOwnerId)
        {
            // Kiểm tra mã cửa hàng đã tồn tại chưa
            var codeExists = await _repository.ShopCodeExistsAsync(dto.ShopCode, shopOwnerId);
            if (codeExists)
            {
                throw new InvalidOperationException($"Mã cửa hàng '{dto.ShopCode}' đã tồn tại");
            }

            var shop = _mapper.Map<Shop>(dto);
            shop.ShopOwnerId = shopOwnerId;
            shop.CreatedAt = DateTime.UtcNow;
            shop.UpdatedAt = DateTime.UtcNow;

            var created = await _repository.CreateAsync(shop);
            return _mapper.Map<ShopDto>(created);
        }

        public async Task<ShopDto?> UpdateAsync(int shopId, UpdateShopDto dto, int shopOwnerId)
        {
            var existingShop = await _repository.GetByIdAsync(shopId, shopOwnerId);
            if (existingShop == null)
            {
                return null;
            }

            // Kiểm tra mã cửa hàng đã tồn tại chưa (trừ chính cửa hàng này)
            var codeExists = await _repository.ShopCodeExistsAsync(dto.ShopCode, shopOwnerId, shopId);
            if (codeExists)
            {
                throw new InvalidOperationException($"Mã cửa hàng '{dto.ShopCode}' đã tồn tại");
            }

            _mapper.Map(dto, existingShop);
            existingShop.UpdatedAt = DateTime.UtcNow;

            var updated = await _repository.UpdateAsync(existingShop);
            return _mapper.Map<ShopDto>(updated);
        }

        public async Task<bool> DeleteAsync(int shopId, int shopOwnerId)
        {
            return await _repository.DeleteAsync(shopId, shopOwnerId);
        }

        public async Task<IEnumerable<ShopDto>> SearchAsync(string? keyword, int shopOwnerId)
        {
            var shops = await _repository.SearchAsync(keyword, shopOwnerId);
            return _mapper.Map<IEnumerable<ShopDto>>(shops);
        }

        public async Task<IEnumerable<ShopDto>> GetByStatusAsync(string status, int shopOwnerId)
        {
            var shops = await _repository.GetByStatusAsync(status, shopOwnerId);
            return _mapper.Map<IEnumerable<ShopDto>>(shops);
        }
    }
}
