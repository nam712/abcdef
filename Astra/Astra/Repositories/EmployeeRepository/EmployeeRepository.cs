// Implementation of IEmployeeRepository using EF Core and AppDbContext
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using YourShopManagement.API.Data;
using YourShopManagement.API.DTOs.Common;

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
            // 🔒 Auto-filter by shop_owner_id from DbContext
            return await _context.Employees.FindAsync(employeeId);
        }

        public async Task<IEnumerable<Employee>> GetAllAsync()
        {
            Console.WriteLine("🔍 [DEBUG] EmployeeRepository.GetAllAsync() called");
            
            try
            {
                // 🔒 Auto-filter by shop_owner_id from ApplicationDbContext
                var employees = await _context.Employees.ToListAsync();
                Console.WriteLine($"🔍 [DEBUG] Found {employees.Count} employees in database");
                
                if (employees.Count > 0)
                {
                    Console.WriteLine("🔍 [DEBUG] Sample employees:");
                    foreach (var emp in employees.Take(3))
                    {
                        Console.WriteLine($"  - ID: {emp.EmployeeId}, Name: {emp.EmployeeName}, Username: {emp.Username}");
                    }
                }
                
                return employees;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [DEBUG] Error in GetAllAsync: {ex.Message}");
                throw;
            }
        }

        // ✅ THÊM METHOD BỎ QUA GLOBAL QUERY FILTER
        public async Task<IEnumerable<Employee>> GetAllWithoutFilterAsync()
        {
            Console.WriteLine("🔍 [DEBUG] EmployeeRepository.GetAllWithoutFilterAsync() called");
            
            try
            {
                var employees = await _context.Employees.IgnoreQueryFilters().ToListAsync();
                Console.WriteLine($"🔍 [DEBUG] Found {employees.Count} employees in database (without filter)");
                
                if (employees.Count > 0)
                {
                    Console.WriteLine("🔍 [DEBUG] Sample employees (without filter):");
                    foreach (var emp in employees.Take(5))
                    {
                        Console.WriteLine($"  - ID: {emp.EmployeeId}, Name: {emp.EmployeeName}, Username: {emp.Username}");
                    }
                }
                
                return employees;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [DEBUG] Error in GetAllWithoutFilterAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<Employee> AddAsync(Employee employee)
        {
            // shop_owner_id will be set in service layer
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

        public async Task<Employee> GetByUsernameAsync(string username)
        {
            Console.WriteLine($"🔍 [DEBUG] GetByUsernameAsync called with username: {username}");
            
            try
            {
                var employee = await _context.Employees
                    .FirstOrDefaultAsync(e => e.Username == username);
                    
                Console.WriteLine($"🔍 [DEBUG] Employee found: {employee != null}");
                
                if (employee != null)
                {
                    Console.WriteLine($"🔍 [DEBUG] Employee details: ID={employee.EmployeeId}, Name={employee.EmployeeName}");
                }
                
                return employee;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [DEBUG] Error in GetByUsernameAsync: {ex.Message}");
                throw;
            }
        }

        // ✅ THÊM METHOD BỎ QUA GLOBAL QUERY FILTER
        public async Task<Employee> GetByUsernameWithoutFilterAsync(string username)
        {
            Console.WriteLine($"🔍 [DEBUG] GetByUsernameWithoutFilterAsync called with username: {username}");
            
            try
            {
                var employee = await _context.Employees
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(e => e.Username == username);
                    
                Console.WriteLine($"🔍 [DEBUG] Employee found (without filter): {employee != null}");
                
                if (employee != null)
                {
                    Console.WriteLine($"🔍 [DEBUG] Employee details: ID={employee.EmployeeId}, Name={employee.EmployeeName}");
                }
                
                return employee;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [DEBUG] Error in GetByUsernameWithoutFilterAsync: {ex.Message}");
                throw;
            }
        }


        
        public async Task<(IEnumerable<Employee> data, int totalRecords)> GetPaginatedAsync(PaginationRequest request)
        {
            var query = _context.Employees.AsQueryable();

            // Search filter
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var searchLower = request.Search.ToLower();
                query = query.Where(e => e.EmployeeName.ToLower().Contains(searchLower) ||
                                        e.EmployeeCode.ToLower().Contains(searchLower) ||
                                        (e.Email != null && e.Email.ToLower().Contains(searchLower)) ||
                                        (e.Phone != null && e.Phone.Contains(request.Search)));
            }

            // Department filter (using Segment property)
            if (!string.IsNullOrWhiteSpace(request.Segment))
            {
                query = query.Where(e => e.Department == request.Segment);
            }

            // Work Status filter
            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                query = query.Where(e => e.WorkStatus == request.Status);
            }

            var totalRecords = await query.CountAsync();
            
            var data = await query
                .OrderByDescending(e => e.CreatedAt)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return (data, totalRecords);
        }
    }


}