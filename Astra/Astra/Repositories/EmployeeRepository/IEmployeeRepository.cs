using Backend.Models;
// Interface for Employee repository with async CRUD and search methods
using System.Collections.Generic;
using System.Threading.Tasks;
using YourShopManagement.API.DTOs.Common;

namespace Backend.Repositories
{
    public interface IEmployeeRepository
    {
        Task<Employee> GetByIdAsync(int employeeId);
        Task<IEnumerable<Employee>> GetAllAsync();
        Task<Employee> AddAsync(Employee employee);
        Task UpdateAsync(Employee employee);
        Task DeleteAsync(int employeeId);

        Task<IEnumerable<Employee>> SearchAsync(string keyword);
        Task<IEnumerable<Employee>> GetByDepartmentAsync(string department);
        Task<IEnumerable<Employee>> GetByWorkStatusAsync(string workStatus);
        Task<Employee> GetByUsernameAsync(string username);
        
        // ✅ THÊM METHODS BỎ QUA GLOBAL QUERY FILTER
        Task<IEnumerable<Employee>> GetAllWithoutFilterAsync();
        Task<Employee> GetByUsernameWithoutFilterAsync(string username);

        Task<(IEnumerable<Employee> data, int totalRecords)> GetPaginatedAsync(PaginationRequest request);
    }

}
