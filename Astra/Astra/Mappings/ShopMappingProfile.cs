using AutoMapper;
using YourShopManagement.API.DTOs.Shop;
using YourShopManagement.API.Models;

namespace YourShopManagement.API.Mappings
{
    public class ShopMappingProfile : Profile
    {
        public ShopMappingProfile()
        {
            // Shop -> ShopDto
            CreateMap<Shop, ShopDto>()
                .ForMember(dest => dest.BusinessCategoryName, 
                    opt => opt.MapFrom(src => src.BusinessCategory != null ? src.BusinessCategory.CategoryName : null));

            // CreateShopDto -> Shop
            CreateMap<CreateShopDto, Shop>()
                .ForMember(dest => dest.ShopId, opt => opt.Ignore())
                .ForMember(dest => dest.ShopOwnerId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.ShopOwner, opt => opt.Ignore())
                .ForMember(dest => dest.BusinessCategory, opt => opt.Ignore())
                // .ForMember(dest => dest.PurchaseOrders, opt => opt.Ignore()) // Removed
                .ForMember(dest => dest.Invoices, opt => opt.Ignore());

            // UpdateShopDto -> Shop
            CreateMap<UpdateShopDto, Shop>()
                .ForMember(dest => dest.ShopId, opt => opt.Ignore())
                .ForMember(dest => dest.ShopOwnerId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.ShopOwner, opt => opt.Ignore())
                .ForMember(dest => dest.BusinessCategory, opt => opt.Ignore())
                // .ForMember(dest => dest.PurchaseOrders, opt => opt.Ignore()) // Removed
                .ForMember(dest => dest.Invoices, opt => opt.Ignore());
        }
    }
}
