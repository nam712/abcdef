// Implementation of IEmployeeRepository using EF Core and AppDbContext
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using YourShopManagement.API.Data;

namespace Backend.Repositories
{


    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly ApplicationDbContext _context;

        public EmployeeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Employee> GetByIdAsync(int employeeId)
        {
            return await _context.Employees.FindAsync(employeeId);
        }

        public async Task<IEnumerable<Employee>> GetAllAsync()
        {
            return await _context.Employees.ToListAsync();
        }

        public async Task<Employee> AddAsync(Employee employee)
        {
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
            return employee;
        }

        public async Task UpdateAsync(Employee employee)
        {
            _context.Employees.Update(employee);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int employeeId)
        {
            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee != null)
            {
                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Employee>> SearchAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return await _context.Employees.ToListAsync();

            var lowerKeyword = keyword.ToLower();
            return await _context.Employees
                .Where(e => e.EmployeeName.ToLower().Contains(lowerKeyword) ||
                            e.EmployeeCode.ToLower().Contains(lowerKeyword) ||
                            (e.Email != null && e.Email.ToLower().Contains(lowerKeyword)))
                .ToListAsync();
        }

        public async Task<IEnumerable<Employee>> GetByDepartmentAsync(string department)
        {
            return await _context.Employees
                .Where(e => e.Department == department)
                .ToListAsync();
        }

        public async Task<IEnumerable<Employee>> GetByWorkStatusAsync(string workStatus)
        {
            return await _context.Employees
                .Where(e => e.WorkStatus == workStatus)
                .ToListAsync();
        }
    }


}