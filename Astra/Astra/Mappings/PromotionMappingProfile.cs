using AutoMapper;
using YourShopManagement.API.DTOs.Promotion;
using YourShopManagement.API.Models;

namespace YourShopManagement.API.Mappings
{
    public class PromotionMappingProfile : Profile
    {
        public PromotionMappingProfile()
        {
            // Promotion -> PromotionDto
            CreateMap<Promotion, PromotionDto>()
                .ForMember(dest => dest.InvoiceCode,
                    opt => opt.MapFrom(src => src.Invoice != null ? src.Invoice.InvoiceCode : null));

            // CreatePromotionDto -> Promotion
            CreateMap<CreatePromotionDto, Promotion>()
                .ForMember(dest => dest.PromotionId, opt => opt.Ignore())
                .ForMember(dest => dest.ShopOwnerId, opt => opt.Ignore())
                .ForMember(dest => dest.InvoiceId, opt => opt.Ignore()) // Không gán invoice khi tạo
                .ForMember(dest => dest.UsageCount, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Invoice, opt => opt.Ignore());

            // UpdatePromotionDto -> Promotion
            CreateMap<UpdatePromotionDto, Promotion>()
                .ForMember(dest => dest.PromotionId, opt => opt.Ignore())
                .ForMember(dest => dest.ShopOwnerId, opt => opt.Ignore())
                .ForMember(dest => dest.InvoiceId, opt => opt.Ignore()) // Không cho phép update invoice_id qua API
                .ForMember(dest => dest.UsageCount, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Invoice, opt => opt.Ignore());
        }
    }
}
