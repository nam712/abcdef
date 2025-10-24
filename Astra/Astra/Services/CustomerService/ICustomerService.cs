using YourShopManagement.API.DTOs;
// Interface for Customer service with CRUD and search methods
using System.Collections.Generic;
using System.Threading.Tasks;

namespace YourShopManagement.API.Services
{

    public interface ICustomerService
    {
    Task<CustomerDto?> GetByIdAsync(int customerId);
        Task<IEnumerable<CustomerDto>> GetAllAsync();
        Task<CustomerDto> AddAsync(CustomerDto customerDto);
        Task UpdateAsync(CustomerDto customerDto);
        Task DeleteAsync(int customerId);

        Task<IEnumerable<CustomerDto>> SearchAsync(string keyword);
        Task<IEnumerable<CustomerDto>> GetBySegmentAsync(string segment);
        Task<IEnumerable<CustomerDto>> GetByStatusAsync(string status);
    }

}
