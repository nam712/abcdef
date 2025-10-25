using AutoMapper;
using YourShopManagement.API.DTOs.Supplier;
using ProductApi.DTOs;
using ProductCategoryApi.DTOs;
using YourShopManagement.API.Models;

namespace OnlineStore.Api.Mappers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // 🧩 Supplier
            CreateMap<Supplier, SupplierInfoDto>().ReverseMap();
            CreateMap<Supplier, CreateSupplierDto>().ReverseMap();
            CreateMap<Supplier, UpdateSupplierDto>().ReverseMap();

            // 🧩 Product
            CreateMap<Product, ProductDto>().ReverseMap();
            CreateMap<Product, ProductCreateDto>().ReverseMap();

            // 🧩 ProductCategory
            CreateMap<ProductCategory, ProductCategoryDto>().ReverseMap();
            CreateMap<ProductCategory, ProductCategoryCreateDto>().ReverseMap();
        }
    }
}
