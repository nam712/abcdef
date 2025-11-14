// Implementation of ICustomerRepository using Entity Framework Core
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YourShopManagement.API.Data;
using YourShopManagement.API.Models;
using YourShopManagement.API.DTOs.Common;
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
            // 🔒 Filter by current shop owner
            return await _context.Customers
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);
        }

        public async Task<IEnumerable<Customer>> GetAllAsync()
        {
            // 🔒 Auto-filter by shop_owner_id from DbContext
            return await _context.Customers.ToListAsync();
        }

        public async Task<Customer> AddAsync(Customer customer)
        {
            // shop_owner_id will be set automatically in service layer
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

        public async Task<(IEnumerable<Customer> data, int totalRecords)> GetPaginatedAsync(PaginationRequest request)
        {
            var query = _context.Customers.AsQueryable();

            // Search filter
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var searchLower = request.Search.ToLower();
                query = query.Where(c => c.CustomerName.ToLower().Contains(searchLower) ||
                                        c.CustomerCode.ToLower().Contains(searchLower) ||
                                        (c.Email != null && c.Email.ToLower().Contains(searchLower)) ||
                                        (c.Phone != null && c.Phone.Contains(request.Search)));
            }

            // Segment filter
            if (!string.IsNullOrWhiteSpace(request.Segment))
            {
                query = query.Where(c => c.Segment == request.Segment);
            }

            // Status filter
            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                query = query.Where(c => c.Status == request.Status);
            }

            var totalRecords = await query.CountAsync();
            
            var data = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return (data, totalRecords);
        }
    }

}
