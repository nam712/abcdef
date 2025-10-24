using AutoMapper;
using YourShopManagement.API.DTOs;
using YourShopManagement.API.Models;

namespace YourShopManagement.API.Mappings
{
    public class CustomerMappingProfile : Profile
    {
        public CustomerMappingProfile()
        {
            CreateMap<Customer, CustomerDto>().ReverseMap();
        }
    }
}