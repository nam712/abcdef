// Implementation of ICustomerService using ICustomerRepository and AutoMapper
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using YourShopManagement.API.DTOs;
using YourShopManagement.API.DTOs.Auth;  
using YourShopManagement.API.Models;
using YourShopManagement.API.Repositories;

namespace YourShopManagement.API.Services
{


    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _repository;
        private readonly IMapper _mapper;

        public CustomerService(ICustomerRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
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
    }

}
