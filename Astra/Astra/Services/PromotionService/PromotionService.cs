using AutoMapper;
using YourShopManagement.API.DTOs.Promotion;
using YourShopManagement.API.Models;
using YourShopManagement.API.Repositories.PromotionRepository;

namespace YourShopManagement.API.Services.PromotionService
{
    public class PromotionService : IPromotionService
    {
        private readonly IPromotionRepository _promotionRepository;
        private readonly IMapper _mapper;

        public PromotionService(IPromotionRepository promotionRepository, IMapper mapper)
        {
            _promotionRepository = promotionRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<PromotionDto>> GetAllPromotionsAsync(int shopOwnerId)
        {
            var promotions = await _promotionRepository.GetAllAsync(shopOwnerId);
            return _mapper.Map<IEnumerable<PromotionDto>>(promotions);
        }

        public async Task<PromotionDto?> GetPromotionByIdAsync(int id, int shopOwnerId)
        {
            var promotion = await _promotionRepository.GetByIdAsync(id, shopOwnerId);
            return promotion == null ? null : _mapper.Map<PromotionDto>(promotion);
        }

        public async Task<PromotionDto> CreatePromotionAsync(CreatePromotionDto dto, int shopOwnerId)
        {
            // Validate dates
            if (dto.EndDate <= dto.StartDate)
            {
                throw new InvalidOperationException("Ngày kết thúc phải sau ngày bắt đầu");
            }

            // Check duplicate promotion code
            if (await _promotionRepository.PromotionCodeExistsAsync(dto.PromotionCode, shopOwnerId))
            {
                throw new InvalidOperationException($"Mã khuyến mãi '{dto.PromotionCode}' đã tồn tại");
            }

            // Validate promotion type and discount value
            if (dto.PromotionType == "percentage" && dto.DiscountValue > 100)
            {
                throw new InvalidOperationException("Giảm giá theo phần trăm không được vượt quá 100%");
            }

            var promotion = _mapper.Map<Promotion>(dto);
            promotion.ShopOwnerId = shopOwnerId;
            promotion.UsageCount = 0;
            promotion.CreatedAt = DateTime.UtcNow;
            promotion.UpdatedAt = DateTime.UtcNow;

            var createdPromotion = await _promotionRepository.CreateAsync(promotion);
            return _mapper.Map<PromotionDto>(createdPromotion);
        }

        public async Task<PromotionDto?> UpdatePromotionAsync(int id, UpdatePromotionDto dto, int shopOwnerId)
        {
            var existingPromotion = await _promotionRepository.GetByIdAsync(id, shopOwnerId);
            if (existingPromotion == null)
                return null;

            // Validate dates
            if (dto.EndDate <= dto.StartDate)
            {
                throw new InvalidOperationException("Ngày kết thúc phải sau ngày bắt đầu");
            }

            // Check duplicate promotion code
            if (await _promotionRepository.PromotionCodeExistsAsync(dto.PromotionCode, shopOwnerId, id))
            {
                throw new InvalidOperationException($"Mã khuyến mãi '{dto.PromotionCode}' đã tồn tại");
            }

            // Validate promotion type and discount value
            if (dto.PromotionType == "percentage" && dto.DiscountValue > 100)
            {
                throw new InvalidOperationException("Giảm giá theo phần trăm không được vượt quá 100%");
            }

            _mapper.Map(dto, existingPromotion);
            existingPromotion.UpdatedAt = DateTime.UtcNow;

            var updatedPromotion = await _promotionRepository.UpdateAsync(existingPromotion);
            return _mapper.Map<PromotionDto>(updatedPromotion);
        }

        public async Task<bool> DeletePromotionAsync(int id, int shopOwnerId)
        {
            return await _promotionRepository.DeleteAsync(id, shopOwnerId);
        }

        public async Task<IEnumerable<PromotionDto>> SearchPromotionsAsync(string keyword, int shopOwnerId)
        {
            var promotions = await _promotionRepository.SearchAsync(keyword, shopOwnerId);
            return _mapper.Map<IEnumerable<PromotionDto>>(promotions);
        }

        public async Task<IEnumerable<PromotionDto>> GetPromotionsByStatusAsync(string status, int shopOwnerId)
        {
            var promotions = await _promotionRepository.GetByStatusAsync(status, shopOwnerId);
            return _mapper.Map<IEnumerable<PromotionDto>>(promotions);
        }

        public async Task<IEnumerable<PromotionDto>> GetActivePromotionsAsync(int shopOwnerId)
        {
            var promotions = await _promotionRepository.GetActivePromotionsAsync(shopOwnerId);
            return _mapper.Map<IEnumerable<PromotionDto>>(promotions);
        }

        public async Task<IEnumerable<PromotionDto>> GetPromotionsByInvoiceIdAsync(int invoiceId, int shopOwnerId)
        {
            var promotions = await _promotionRepository.GetByInvoiceIdAsync(invoiceId, shopOwnerId);
            return _mapper.Map<IEnumerable<PromotionDto>>(promotions);
        }
    }
}
