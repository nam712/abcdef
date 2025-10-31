using AutoMapper;
using YourShopManagement.API.DTOs.PurchaseOrder;
using YourShopManagement.API.Models;

namespace YourShopManagement.API.Mappings
{
    public class PurchaseOrderMappingProfile : Profile
    {
        public PurchaseOrderMappingProfile()
        {
            // Entity → DTO
            CreateMap<PurchaseOrder, PurchaseOrderDto>()
                .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier.SupplierName))
                .ForMember(dest => dest.Details, opt => opt.MapFrom(src => src.PurchaseOrderDetails));

            CreateMap<PurchaseOrderDetail, PurchaseOrderDetailDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.ProductName));

            // DTO → Entity (Create)
            CreateMap<CreatePurchaseOrderDto, PurchaseOrder>()
                .ForMember(dest => dest.PurchaseOrderId, opt => opt.Ignore())
                .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.Details.Sum(d => d.Quantity * d.ImportPrice)))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => "pending"))
                .ForMember(dest => dest.PaymentStatus, opt => opt.MapFrom(_ => "unpaid"))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));

            CreateMap<CreatePurchaseOrderDetailDto, PurchaseOrderDetail>()
                .ForMember(dest => dest.PurchaseOrderDetailId, opt => opt.Ignore())
                .ForMember(dest => dest.FinalAmount, opt => opt.MapFrom(src => src.Quantity * src.ImportPrice));
        }
    }
}