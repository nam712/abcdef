using YourShopManagement.API.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace YourShopManagement.API.Repositories
{
    public interface ICustomerRepository
    {
        Task<Customer> GetByIdAsync(int customerId);
        Task<IEnumerable<Customer>> GetAllAsync();
        Task<Customer> AddAsync(Customer customer);
        Task UpdateAsync(Customer customer);
        Task DeleteAsync(int customerId);

        Task<IEnumerable<Customer>> SearchAsync(string keyword);
        Task<IEnumerable<Customer>> GetBySegmentAsync(string segment);
        Task<IEnumerable<Customer>> GetByStatusAsync(string status);
    }
}
