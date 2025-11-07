// Interface for Employee service with CRUD and search methods
using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.DTOs;
using YourShopManagement.API.DTOs.Common;


namespace Backend.Services
{
    public interface IEmployeeService
    {
        Task<EmployeeDto> GetByIdAsync(int employeeId);
        Task<IEnumerable<EmployeeDto>> GetAllAsync();
        Task<EmployeeDto> AddAsync(EmployeeDto employeeDto);
        Task UpdateAsync(EmployeeDto employeeDto);
        Task DeleteAsync(int employeeId);

        Task<IEnumerable<EmployeeDto>> SearchAsync(string keyword);
        Task<IEnumerable<EmployeeDto>> GetByDepartmentAsync(string department);
        Task<IEnumerable<EmployeeDto>> GetByWorkStatusAsync(string workStatus);
        Task<EmployeeDto> AddOrUpdateWithAvatarAsync(EmployeeDto dto);
        Task DeleteAvatarAsync(int employeeId);
        
        // ✅ THÊM METHOD DEBUG
        Task<object> DebugDatabaseAsync();
        Task<EmployeeDto> GetByUsernameAsync(string username);
        Task<IEnumerable<EmployeeDto>> GetAllWithoutFilterAsync(); // ✅ THÊM

        // ✅ THÊM METHOD CHO EMPLOYEE TỰ CẬP NHẬT
        Task<EmployeeDto> UpdateByEmployeeAsync(int employeeId, EmployeeUpdateByEmployeeDto dto);

        Task<PaginatedResponse<EmployeeDto>> GetPaginatedAsync(PaginationRequest request);
    }


}
