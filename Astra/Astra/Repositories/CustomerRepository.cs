// Implementation of ICustomerRepository using Entity Framework Core
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YourShopManagement.API.Data;
using YourShopManagement.API.Models;
using Microsoft.EntityFrameworkCore;

namespace YourShopManagement.API.Repositories
{

    public class CustomerRepository : ICustomerRepository
    {
        private readonly ApplicationDbContext _context;

        public CustomerRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Customer> GetByIdAsync(int customerId)
        {
            return await _context.Customers.FindAsync(customerId);
        }

        public async Task<IEnumerable<Customer>> GetAllAsync()
        {
            return await _context.Customers.ToListAsync();
        }

        public async Task<Customer> AddAsync(Customer customer)
        {
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            return customer;
        }

        public async Task UpdateAsync(Customer customer)
        {
            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int customerId)
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer != null)
            {
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Customer>> SearchAsync(string keyword)
        {
            return await _context.Customers
                .Where(c => c.CustomerName.Contains(keyword) ||
                            c.CustomerCode.Contains(keyword) ||
                            c.Email.Contains(keyword))
                .ToListAsync();
        }

        public async Task<IEnumerable<Customer>> GetBySegmentAsync(string segment)
        {
            return await _context.Customers
                .Where(c => c.Segment == segment)
                .ToListAsync();
        }

        public async Task<IEnumerable<Customer>> GetByStatusAsync(string status)
        {
            return await _context.Customers
                .Where(c => c.Status == status)
                .ToListAsync();
        }
    }

}
