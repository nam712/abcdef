using Backend.DTOs;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization; 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using YourShopManagement.API.DTOs.Common;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    //[Authorize(Roles = "ShopOwner,Employee")]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _service;

        public EmployeesController(IEmployeeService service)
        {
            _service = service;
        }

        // GET /api/employees
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetAll()
        {
            try
            {
                var employees = await _service.GetAllAsync();
                if (employees == null || !employees.Any())
                    return NotFound(new { message = "Chưa có nhân viên nào trong hệ thống." });

                return Ok(employees);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy danh sách nhân viên", detail = ex.Message });
            }
        }

        // GET /api/employees/paginated
        [HttpGet("paginated")]
        public async Task<ActionResult<PaginatedResponse<EmployeeDto>>> GetPaginated([FromQuery] PaginationRequest request)
        {
            try
            {
                var result = await _service.GetPaginatedAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy danh sách nhân viên phân trang", detail = ex.Message });
            }
        }

        // GET /api/employees/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<EmployeeDto>> GetById(int id)
        {
            try
            {
                var employee = await _service.GetByIdAsync(id);
                if (employee == null)
                    return NotFound(new { message = $"Không tìm thấy nhân viên với ID = {id}" });

                return Ok(employee);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy thông tin nhân viên", detail = ex.Message });
            }
        }

        // POST /api/employees - Chỉ ShopOwner mới có quyền tạo
        [HttpPost]
        [Authorize(Roles = "ShopOwner")]
        public async Task<ActionResult<EmployeeDto>> Create([FromBody] EmployeeDto employeeDto)
        {
            try
            {
                // Lấy ShopOwnerId từ JWT token
                var shopOwnerIdClaim = User.FindFirst("shop_owner_id")?.Value;
                if (string.IsNullOrEmpty(shopOwnerIdClaim) || !int.TryParse(shopOwnerIdClaim, out int shopOwnerId))
                {
                    return Unauthorized(new { message = "Không xác định được chủ cửa hàng" });
                }

                // Set ShopOwnerId cho employee
                employeeDto.ShopOwnerId = shopOwnerId;

                var created = await _service.AddAsync(employeeDto);
                return CreatedAtAction(nameof(GetById), new { id = created.EmployeeId }, created);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi tạo nhân viên mới", detail = ex.Message });
            }
        }

        // PUT /api/employees/{id} - ShopOwner có thể sửa tất cả, Employee chỉ sửa thông tin cá nhân được phép
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] EmployeeDto employeeDto)
        {
            if (id != employeeDto.EmployeeId)
                return BadRequest(new { message = "ID trong URL và ID trong dữ liệu không khớp" });

            try
            {
                var userType = User.FindFirst("user_type")?.Value;
                // ✅ SỬA: Sử dụng employee_id hoặc NameIdentifier
                var userId = User.FindFirst("employee_id")?.Value ?? 
                            User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                            User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

                // Nếu là Employee, chỉ cho phép sửa thông tin của chính mình VÀ CHỈ CÁC TRƯỜNG ĐƯỢC PHÉP
                if (userType == "Employee")
                {
                    if (!int.TryParse(userId, out int employeeId) || employeeId != id)
                    {
                        return BadRequest(new { 
                            success = false,
                            message = "Nhân viên chỉ được phép sửa thông tin của chính mình",
                            allowedEmployeeId = employeeId,
                            requestedId = id
                        });
                    }

                    // ❌ TỪNG CHỐI - Employee không được dùng endpoint này để cập nhật toàn bộ
                    return BadRequest(new { 
                        message = "Nhân viên không được phép sử dụng endpoint này. Vui lòng sử dụng PUT /api/employees/{id}/update-profile",
                        allowedEndpoint = $"/api/employees/{id}/update-profile",
                        allowedFields = new[] {
                            "phone", "email", "address", "dateOfBirth", "gender", 
                            "idCard", "bankAccount", "bankName", "password", "avatarUrl", "notes"
                        }
                    });
                }

                // ShopOwner có thể cập nhật toàn bộ
                await _service.UpdateAsync(employeeDto);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật nhân viên", detail = ex.Message });
            }
        }

        // ✅ THÊM ENDPOINT CHO EMPLOYEE LẤY THÔNG TIN CỦA MÌNH
        /// <summary>
        /// 👤 Employee lấy thông tin cá nhân để hiển thị/chỉnh sửa
        /// </summary>
        [HttpGet("{id}/profile")]
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> GetProfileByEmployee(int id)
        {
            try
            {
                Console.WriteLine("🔍 [DEBUG] GetProfileByEmployee called");
                Console.WriteLine($"🔍 [DEBUG] Requested ID: {id}");
                
                // ✅ SỬA: Lấy userId từ claim "employee_id" thay vì "sub"
                var userId = User.FindFirst("employee_id")?.Value ?? 
                            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                Console.WriteLine($"🔍 [DEBUG] UserId from employee_id claim: '{userId}'");

                // Kiểm tra Employee chỉ được xem thông tin của chính mình
                if (!int.TryParse(userId, out int employeeId))
                {
                    Console.WriteLine($"❌ [DEBUG] Failed to parse userId '{userId}' to int");
                    return BadRequest(new { 
                        success = false,
                        message = "Không thể parse EmployeeId từ token",
                        userId = userId
                    });
                }
                
                Console.WriteLine($"🔍 [DEBUG] Parsed employeeId: {employeeId}");
                
                if (employeeId != id)
                {
                    Console.WriteLine($"❌ [DEBUG] EmployeeId mismatch: {employeeId} != {id}");
                    return BadRequest(new { 
                        success = false,
                        message = "Bạn chỉ được phép xem thông tin của chính mình",
                        allowedEmployeeId = employeeId,
                        requestedId = id
                    });
                }

                var employee = await _service.GetByIdAsync(id);
                if (employee == null)
                    return NotFound(new { message = $"Không tìm thấy nhân viên với ID = {id}" });

                return Ok(new
                {
                    success = true,
                    message = "Thông tin cá nhân",
                    data = employee,
                    editableFields = new[] {
                        "phone", "email", "address", "dateOfBirth", "gender", 
                        "idCard", "bankAccount", "bankName", "password", "avatarUrl", "notes"
                    },
                    readOnlyFields = new[] {
                        "shopOwnerId", "employeeCode", "employeeName", "position", 
                        "department", "hireDate", "salary", "salaryType", "username", 
                        "permissions", "workStatus", "createdAt", "updatedAt"
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [DEBUG] Exception: {ex.Message}");
                return StatusCode(500, new { message = "Lỗi khi lấy thông tin cá nhân", detail = ex.Message });
            }
        }


        // GET /api/employees/search?name=
        // [HttpGet("search")]
        // public async Task<ActionResult<IEnumerable<EmployeeDto>>> Search([FromQuery] string name)
        // {
        //     try
        //     {
        //         var employees = await _service.SearchAsync(name ?? string.Empty);
        //         if (employees == null || !employees.Any())
        //             return NotFound(new { message = $"Không tìm thấy nhân viên với từ khóa '{name}'." });

        //         return Ok(employees);
        //     }
        //     catch (Exception ex)
        //     {
        //         return StatusCode(500, new { message = "Lỗi khi tìm kiếm nhân viên", detail = ex.Message });
        //     }
        // }


        // ✅ THÊM ENDPOINT RIÊNG CHO EMPLOYEE TỰ CẬP NHẬT
        /// <summary>
        /// 👤 Employee tự cập nhật thông tin cá nhân (chỉ các trường được phép)
        /// </summary>
        [HttpPut("{id}/update-profile")]
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> UpdateProfileByEmployee(int id, [FromBody] EmployeeUpdateByEmployeeDto dto)
        {
            if (id != dto.EmployeeId)
                return BadRequest(new { message = "ID trong URL và ID trong dữ liệu không khớp" });

            try
            {
                Console.WriteLine("🔍 [DEBUG] UpdateProfileByEmployee called");
                Console.WriteLine($"🔍 [DEBUG] Requested ID: {id}");
                
                // Lấy userId từ claim "employee_id" thay vì "sub"
                var userId = User.FindFirst("employee_id")?.Value ?? 
                            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                Console.WriteLine($"🔍 [DEBUG] UserId from employee_id claim: '{userId}'");

                // Kiểm tra Employee chỉ được sửa thông tin của chính mình
                if (!int.TryParse(userId, out int employeeId))
                {
                    Console.WriteLine($"❌ [DEBUG] Failed to parse userId '{userId}' to int");
                    return BadRequest(new { 
                        success = false,
                        message = "Không thể parse EmployeeId từ token",
                        userId = userId
                    });
                }
                
                Console.WriteLine($"🔍 [DEBUG] Parsed employeeId: {employeeId}");
                
                if (employeeId != id)
                {
                    Console.WriteLine($"❌ [DEBUG] EmployeeId mismatch: {employeeId} != {id}");
                    return BadRequest(new { 
                        success = false,
                        message = "Bạn chỉ được phép sửa thông tin của chính mình",
                        allowedEmployeeId = employeeId,
                        requestedId = id
                    });
                }

                var result = await _service.UpdateByEmployeeAsync(id, dto);
                
                return Ok(new
                {
                    success = true,
                    message = "Cập nhật thông tin cá nhân thành công",
                    data = result,
                    updatedFields = new[] {
                        "phone", "email", "address", "dateOfBirth", "gender", 
                        "idCard", "bankAccount", "bankName", "notes", "avatar"
                    },
                    protectedFields = new[] {
                        "shopOwnerId", "employeeCode", "employeeName", "position", 
                        "department", "hireDate", "salary", "salaryType", "username", 
                        "permissions", "workStatus"
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [DEBUG] Exception: {ex.Message}");
                return StatusCode(500, new { message = "Lỗi khi cập nhật thông tin cá nhân", detail = ex.Message });
            }
        }

        // DELETE /api/employees/{id} - Chỉ ShopOwner mới có quyền xóa
        [HttpDelete("{id}")]
        [Authorize(Roles = "ShopOwner")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi xóa nhân viên", detail = ex.Message });
            }
        }

        [HttpPost("upload")]
        [Authorize(Roles = "ShopOwner")]
        public async Task<ActionResult<EmployeeDto>> UploadEmployee([FromForm] EmployeeDto employeeDto)
        {
            try
            {
                var created = await _service.AddOrUpdateWithAvatarAsync(employeeDto);
                return Ok(created);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi upload ảnh nhân viên", detail = ex.Message });
            }
        }

        // // Delete avatar file - Employee có thể xóa ảnh của chính mình
        // [HttpDelete("{id}/avatar")]
        // public async Task<IActionResult> DeleteAvatar(int id)
        // {
        //     try
        //     {
        //         var userType = User.FindFirst("user_type")?.Value;
        //         // ✅ SỬA: Sử dụng employee_id hoặc NameIdentifier
        //         var userId = User.FindFirst("employee_id")?.Value ?? 
        //                     User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
        //                     User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        //         // Nếu là Employee, chỉ cho phép xóa ảnh của chính mình
        //         if (userType == "Employee")
        //         {
        //             if (!int.TryParse(userId, out int employeeId) || employeeId != id)
        //             {
        //                 return BadRequest(new { 
        //                     success = false,
        //                     message = "Nhân viên chỉ được phép xóa ảnh của chính mình",
        //                     allowedEmployeeId = employeeId,
        //                     requestedId = id
        //                 });
        //             }
        //         }

        //         await _service.DeleteAvatarAsync(id);
        //         return NoContent();
        //     }
        //     catch (Exception ex)
        //     {
        //         return StatusCode(500, new { message = "Lỗi khi xóa ảnh nhân viên", detail = ex.Message });
        //     }
        // }

        // // ✅ THÊM ENDPOINT SO SÁNH
        // [HttpGet("compare-debug")]
        // [AllowAnonymous]
        // public async Task<IActionResult> CompareDebug()
        // {
        //     try
        //     {
        //         Console.WriteLine("🔍 [DEBUG] CompareDebug started");
                
        //         // 1. Gọi GetAllAsync (như endpoint /api/employees bình thường)
        //         var normalEmployees = await _service.GetAllAsync();
        //         var normalList = normalEmployees.ToList();
                
        //         Console.WriteLine($"🔍 [DEBUG] Normal GetAllAsync returned: {normalList.Count} employees");
                
        //         // 2. Gọi DebugDatabaseAsync
        //         var debugResult = await _service.DebugDatabaseAsync();
                
        //         Console.WriteLine("🔍 [DEBUG] DebugDatabaseAsync completed");
                
        //         return Ok(new
        //         {
        //             message = "Comparison completed",
        //             normalMethod = new
        //             {
        //                 totalEmployees = normalList.Count,
        //                 employees = normalList.Take(3).Select(e => new
        //                 {
        //                     e.EmployeeId,
        //                     e.EmployeeName,
        //                     e.Username,
        //                     e.WorkStatus
        //                 }).ToList()
        //             },
        //             debugMethod = debugResult
        //         });
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine($"❌ [DEBUG] Exception in CompareDebug: {ex.Message}");
        //         return StatusCode(500, new { 
        //             message = "Lỗi compare debug", 
        //             detail = ex.Message,
        //             stackTrace = ex.StackTrace
        //         });
        //     }
        // }

        // ✅ CẬP NHẬT ENDPOINT DEBUG
        /// <summary>
        /// 🧪 Debug endpoint - Kiểm tra Employee có tồn tại không
        /// </summary>
        // [HttpGet("debug/{username}")]
        // [AllowAnonymous] // Tạm thời cho phép anonymous để debug
        // public async Task<IActionResult> DebugEmployee(string username)
        // {
        //     try
        //     {
        //         Console.WriteLine($"🔍 [DEBUG] DebugEmployee called with username: {username}");
                
        //         // 1. Kiểm tra bằng GetAllAsync trước
        //         Console.WriteLine("🔍 [DEBUG] Calling normal GetAllAsync...");
        //         var normalEmployees = await _service.GetAllAsync();
        //         var normalList = normalEmployees.ToList();
        //         var normalTarget = normalList.FirstOrDefault(e => e.Username == username);
                
        //         Console.WriteLine($"🔍 [DEBUG] Normal method found: {normalTarget != null}");
                
        //         // 2. Kiểm tra database trực tiếp
        //         Console.WriteLine("🔍 [DEBUG] Calling DebugDatabaseAsync...");
        //         var debugResult = await _service.DebugDatabaseAsync();
                
        //         // 3. Tìm employee cụ thể bằng username
        //         Console.WriteLine("🔍 [DEBUG] Calling GetByUsernameAsync...");
        //         var targetEmployee = await _service.GetByUsernameAsync(username);

        //         return Ok(new
        //         {
        //             message = targetEmployee != null 
        //                 ? $"Tìm thấy employee với username: {username}" 
        //                 : $"Không tìm thấy employee với username: {username}",
                        
        //             normalMethod = new
        //             {
        //                 found = normalTarget != null,
        //                 totalEmployees = normalList.Count,
        //                 targetEmployee = normalTarget != null ? new
        //                 {
        //                     normalTarget.EmployeeId,
        //                     normalTarget.EmployeeName,
        //                     normalTarget.Username,
        //                     normalTarget.WorkStatus
        //                 } : null
        //             },
                    
        //             directMethod = new
        //             {
        //                 found = targetEmployee != null,
        //                 targetEmployee = targetEmployee != null ? new
        //                 {
        //                     targetEmployee.EmployeeId,
        //                     targetEmployee.EmployeeName,
        //                     targetEmployee.Username,
        //                     targetEmployee.WorkStatus,
        //                     HasPassword = !string.IsNullOrEmpty(targetEmployee.Password),
        //                     PasswordLength = targetEmployee.Password?.Length ?? 0
        //                 } : null
        //             },
                    
        //             databaseInfo = debugResult
        //         });
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine($"❌ [DEBUG] Exception in DebugEmployee: {ex.Message}");
        //         return StatusCode(500, new { 
        //             message = "Lỗi debug", 
        //             detail = ex.Message,
        //             stackTrace = ex.StackTrace
        //         });
        //     }
        // }

        // // ✅ THÊM ENDPOINT DEBUG DATABASE TỔNG QUÁT
        // [HttpGet("debug-db")]
        // [AllowAnonymous]
        // public async Task<IActionResult> DebugDatabase()
        // {
        //     try
        //     {
        //         var result = await _service.DebugDatabaseAsync();
        //         return Ok(result);
        //     }
        //     catch (Exception ex)
        //     {
        //         return StatusCode(500, new { 
        //             message = "Lỗi debug database", 
        //             detail = ex.Message,
        //             stackTrace = ex.StackTrace
        //         });
        //     }
        // }

        // ✅ THÊM ENDPOINT TEST GLOBAL QUERY FILTER
        // [HttpGet("test-filter")]
        // [AllowAnonymous]
        // public async Task<IActionResult> TestGlobalQueryFilter()
        // {
        //     try
        //     {
        //         Console.WriteLine("🔍 [DEBUG] TestGlobalQueryFilter started");
                
        //         // 1. Test với Global Query Filter (thông thường)
        //         Console.WriteLine("🔍 [DEBUG] Testing WITH Global Query Filter...");
        //         var withFilter = await _service.GetAllAsync();
        //         var withFilterList = withFilter.ToList();
                
        //         // 2. Test mà không có Global Query Filter  
        //         Console.WriteLine("🔍 [DEBUG] Testing WITHOUT Global Query Filter...");
        //         var withoutFilter = await _service.GetAllWithoutFilterAsync();
        //         var withoutFilterList = withoutFilter.ToList();
                
        //         return Ok(new
        //         {
        //             message = "Global Query Filter Test",
        //             withGlobalQueryFilter = new
        //             {
        //                 totalEmployees = withFilterList.Count,
        //                 employees = withFilterList.Take(3).Select(e => new
        //                 {
        //                     e.EmployeeId,
        //                     e.EmployeeName,
        //                     e.Username,
        //                     e.ShopOwnerId
        //                 }).ToList()
        //             },
        //             withoutGlobalQueryFilter = new
        //             {
        //                 totalEmployees = withoutFilterList.Count,
        //                 employees = withoutFilterList.Take(5).Select(e => new
        //                 {
        //                     e.EmployeeId,
        //                     e.EmployeeName,
        //                     e.Username,
        //                     e.ShopOwnerId
        //                 }).ToList()
        //             },
        //             explanation = withFilterList.Count != withoutFilterList.Count ? 
        //                 "Global Query Filter đang hoạt động và lọc employees theo ShopOwnerId" :
        //                 "Global Query Filter không hoạt động hoặc không có filter"
        //         });
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine($"❌ [DEBUG] Exception in TestGlobalQueryFilter: {ex.Message}");
        //         return StatusCode(500, new { 
        //             message = "Lỗi test filter", 
        //             detail = ex.Message,
        //             stackTrace = ex.StackTrace
        //         });
        //     }
        // }
    }
}
