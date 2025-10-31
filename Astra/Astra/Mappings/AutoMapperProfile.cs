// AutoMapper profile for Customer and Employee entities
using AutoMapper;
using Backend.DTOs;
using Backend.Models;

namespace Backend.Mappings
{


    public class EntityMappingProfile : Profile
    {
        public EntityMappingProfile()
        {
            //CreateMap<Customer, CustomerDto>();
            //CreateMap<CustomerDto, Customer>();

            CreateMap<Employee, EmployeeDto>().ReverseMap();

            CreateMap<PaymentMethod, PaymentMethodDto>().ReverseMap();
        }
    }


}
