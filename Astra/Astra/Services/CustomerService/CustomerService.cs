// Implementation of ICustomerService using ICustomerRepository and AutoMapper
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using YourShopManagement.API.DTOs;
using YourShopManagement.API.DTOs.Auth;  
using YourShopManagement.API.Models;
using YourShopManagement.API.Repositories;
using YourShopManagement.API.DTOs.Common;
using YourShopManagement.API.Data;
using Microsoft.AspNetCore.Http;

namespace YourShopManagement.API.Services
{


    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _repository;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;
        private readonly int _currentShopOwnerId;

        public CustomerService(ICustomerRepository repository, IMapper mapper, ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _context = context;
            
            // 🔒 Lấy shop_owner_id từ JWT token
            var claim = httpContextAccessor.HttpContext?.User?.FindFirst("shop_owner_id")?.Value;
            if (int.TryParse(claim, out var id))
                _currentShopOwnerId = id;
            else
                _currentShopOwnerId = 0;
        }

        public async Task<CustomerDto?> GetByIdAsync(int customerId)
        {
            var customer = await _repository.GetByIdAsync(customerId);
            return customer == null ? null : _mapper.Map<CustomerDto>(customer);
        }

        public async Task<IEnumerable<CustomerDto>> GetAllAsync()
        {
            var customers = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<CustomerDto>>(customers);
        }

        public async Task<CustomerDto> AddAsync(CustomerDto customerDto)
        {
            var customer = _mapper.Map<Customer>(customerDto);
            // 🔒 Set shop_owner_id from current user
            customer.ShopOwnerId = _currentShopOwnerId;
            var added = await _repository.AddAsync(customer);
            return _mapper.Map<CustomerDto>(added);
        }

        public async Task UpdateAsync(CustomerDto customerDto)
        {
            var customer = _mapper.Map<Customer>(customerDto);
            await _repository.UpdateAsync(customer);
        }

        public async Task DeleteAsync(int customerId)
        {
            await _repository.DeleteAsync(customerId);
        }

        public async Task<IEnumerable<CustomerDto>> SearchAsync(string keyword)
        {
            var customers = await _repository.SearchAsync(keyword);
            return _mapper.Map<IEnumerable<CustomerDto>>(customers);
        }

        public async Task<IEnumerable<CustomerDto>> GetBySegmentAsync(string segment)
        {
            var customers = await _repository.GetBySegmentAsync(segment);
            return _mapper.Map<IEnumerable<CustomerDto>>(customers);
        }

        public async Task<IEnumerable<CustomerDto>> GetByStatusAsync(string status)
        {
            var customers = await _repository.GetByStatusAsync(status);
            return _mapper.Map<IEnumerable<CustomerDto>>(customers);
        }

        public async Task<PaginatedResponse<CustomerDto>> GetPaginatedAsync(PaginationRequest request)
        {
            var (data, totalRecords) = await _repository.GetPaginatedAsync(request);
            var customerDtos = _mapper.Map<IEnumerable<CustomerDto>>(data);
            
            return PaginatedResponse<CustomerDto>.Create(customerDtos, request.Page, request.PageSize, totalRecords);
        }
    }

}
