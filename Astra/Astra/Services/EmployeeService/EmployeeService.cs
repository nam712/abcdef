// Implementation of IEmployeeService using IEmployeeRepository and AutoMapper
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Backend.DTOs;
using Backend.Models;
using Backend.Repositories;
using YourShopManagement.API.DTOs.Common;

namespace Backend.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _repository;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _env;

        public EmployeeService(IEmployeeRepository repository, IMapper mapper, IWebHostEnvironment env)
        {
            _repository = repository;
            _mapper = mapper;
            _env = env;
        }

        public async Task<EmployeeDto> GetByIdAsync(int employeeId)
        {
            var employee = await _repository.GetByIdAsync(employeeId);
            return employee == null ? null : _mapper.Map<EmployeeDto>(employee);
        }

        public async Task<IEnumerable<EmployeeDto>> GetAllAsync()
        {
            var employees = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<EmployeeDto>>(employees);
        }

        public async Task<EmployeeDto> AddAsync(EmployeeDto employeeDto)
        {
            var employee = _mapper.Map<Employee>(employeeDto);
            
            // ✅ THÊM: Hash password trước khi lưu (nếu có password)
            if (!string.IsNullOrEmpty(employee.Password))
            {
                employee.Password = YourShopManagement.API.Helpers.PasswordHelper.HashPassword(employee.Password);
                Console.WriteLine("🔒 [DEBUG] Employee password hashed when creating new employee");
            }
            
            var added = await _repository.AddAsync(employee);
            return _mapper.Map<EmployeeDto>(added);
        }

        public async Task UpdateAsync(EmployeeDto employeeDto)
        {
            var employee = _mapper.Map<Employee>(employeeDto);
            
            // ✅ THÊM: Hash password nếu password được thay đổi
            if (!string.IsNullOrEmpty(employee.Password) && !employee.Password.StartsWith("$2"))
            {
                employee.Password = YourShopManagement.API.Helpers.PasswordHelper.HashPassword(employee.Password);
                Console.WriteLine("🔒 [DEBUG] Employee password hashed when updating");
            }
            
            await _repository.UpdateAsync(employee);
        }
        public async Task DeleteAsync(int employeeId)
        {
            try
            {
                var emp = await _repository.GetByIdAsync(employeeId);
                if (emp == null)
                    throw new Exception("Không tìm thấy nhân viên để xóa.");

                await DeleteAvatarAsync(employeeId);

                await _repository.DeleteAsync(employeeId);
                Console.WriteLine($"✅ Đã xóa nhân viên ID = {employeeId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi khi xóa nhân viên: {ex.Message}");
                throw; // để controller trả về 500
            }
        }


        public async Task<IEnumerable<EmployeeDto>> SearchAsync(string keyword)
        {
            var employees = await _repository.SearchAsync(keyword);
            return _mapper.Map<IEnumerable<EmployeeDto>>(employees);
        }

        public async Task<IEnumerable<EmployeeDto>> GetByDepartmentAsync(string department)
        {
            var employees = await _repository.GetByDepartmentAsync(department);
            return _mapper.Map<IEnumerable<EmployeeDto>>(employees);
        }

        public async Task<IEnumerable<EmployeeDto>> GetByWorkStatusAsync(string workStatus)
        {
            var employees = await _repository.GetByWorkStatusAsync(workStatus);
            return _mapper.Map<IEnumerable<EmployeeDto>>(employees);
        }
        public async Task<EmployeeDto> AddOrUpdateWithAvatarAsync(EmployeeDto dto)
        {
            Employee employee;
            string oldAvatarUrl = null;

            if (dto.EmployeeId != 0)
            {
                employee = await _repository.GetByIdAsync(dto.EmployeeId);
                if (employee == null)
                    throw new Exception("Không tìm thấy nhân viên để cập nhật.");

                oldAvatarUrl = employee.AvatarUrl;
                
                // Lưu password cũ để so sánh
                var oldPassword = employee.Password;
                
                _mapper.Map(dto, employee);
                
                // ✅ THÊM: Hash password mới nếu password được thay đổi
                if (!string.IsNullOrEmpty(dto.Password) && dto.Password != oldPassword)
                {
                    // Chỉ hash nếu password chưa được hash
                    if (!dto.Password.StartsWith("$2"))
                    {
                        employee.Password = YourShopManagement.API.Helpers.PasswordHelper.HashPassword(dto.Password);
                        Console.WriteLine("🔒 [DEBUG] Employee password updated and hashed");
                    }
                }
                else
                {
                    // Giữ nguyên password cũ nếu không thay đổi
                    employee.Password = oldPassword;
                }
            }
            else
            {
                employee = _mapper.Map<Employee>(dto);
                employee.CreatedAt = DateTime.UtcNow;
                
                // ✅ THÊM: Hash password khi tạo mới
                if (!string.IsNullOrEmpty(employee.Password))
                {
                    employee.Password = YourShopManagement.API.Helpers.PasswordHelper.HashPassword(employee.Password);
                    Console.WriteLine("🔒 [DEBUG] New employee password hashed");
                }
            }

            string avatarUrl = employee.AvatarUrl;

            if (dto.AvatarFile != null)
            {
                var ext = Path.GetExtension(dto.AvatarFile.FileName).ToLower();
                if (!new[] { ".jpg", ".jpeg", ".png" }.Contains(ext))
                    throw new Exception("Chỉ hỗ trợ định dạng .jpg, .jpeg, .png");

                if (dto.AvatarFile.Length > 5 * 1024 * 1024)
                    throw new Exception("Kích thước ảnh tối đa 5MB");

                var folder = Path.Combine(_env.WebRootPath, "uploads", "avatars");
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                // Xóa ảnh cũ nếu có (chỉ khi đang update và có ảnh cũ)
                if (!string.IsNullOrEmpty(oldAvatarUrl))
                {
                    try
                    {
                        var fileName = Path.GetFileName(oldAvatarUrl);
                        var oldPath = Path.Combine(folder, fileName);
                        
                        Console.WriteLine($"🧹 [DEBUG] Chuẩn bị xóa ảnh cũ: {oldPath}");
                        
                        if (File.Exists(oldPath))
                        {
                            File.Delete(oldPath);
                            Console.WriteLine("✅ Đã xóa ảnh cũ thành công.");
                        }
                        else
                        {
                            Console.WriteLine("⚠️ Ảnh cũ không tồn tại, bỏ qua xóa.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Lỗi khi xóa ảnh cũ: {ex.Message}");
                        // Không throw exception để tiếp tục upload ảnh mới
                    }
                }

                // Lưu ảnh mới
                var newFileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(folder, newFileName);
                
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.AvatarFile.CopyToAsync(stream);
                }

                avatarUrl = $"/uploads/avatars/{newFileName}";
                Console.WriteLine($"✅ Đã lưu ảnh mới: {avatarUrl}");
            }
            else if (!string.IsNullOrEmpty(dto.AvatarUrl))
            {
                // Giữ nguyên ảnh cũ nếu không có file mới và có AvatarUrl từ form
                avatarUrl = dto.AvatarUrl;
            }

            employee.AvatarUrl = avatarUrl;
            employee.UpdatedAt = DateTime.UtcNow; // ✅ ĐÃ ĐÚNG

            if (employee.EmployeeId == 0)
                await _repository.AddAsync(employee);
            else
                await _repository.UpdateAsync(employee);

            return _mapper.Map<EmployeeDto>(employee);
        }



        public async Task DeleteAvatarAsync(int employeeId)
        {
            var emp = await _repository.GetByIdAsync(employeeId);
            if (emp == null)
                throw new Exception("Không tìm thấy nhân viên.");

            try
            {
                if (!string.IsNullOrEmpty(emp.AvatarUrl))
                {
                    // Đảm bảo chỉ lấy tên file (tránh lỗi đường dẫn sai)
                    var fileName = Path.GetFileName(emp.AvatarUrl);
                    var fullPath = Path.Combine(_env.WebRootPath, "uploads", "avatars", fileName);

                    Console.WriteLine($"🧹 [DEBUG] Xóa file: {fullPath}");

                    // Nếu file tồn tại thì xóa
                    if (File.Exists(fullPath))
                    {
                        File.Delete(fullPath);
                        Console.WriteLine("✅ File ảnh đã được xóa thành công.");
                    }
                    else
                    {
                        Console.WriteLine("⚠️ File không tồn tại, bỏ qua xóa.");
                    }

                    emp.AvatarUrl = null;
                    await _repository.UpdateAsync(emp);
                }
                else
                {
                    Console.WriteLine("⚠️ Nhân viên không có AvatarUrl, bỏ qua.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi khi xóa avatar: {ex.Message}");
            }
        }

        // ✅ THÊM CÁC METHOD DEBUG
        public async Task<object> DebugDatabaseAsync()
        {
            try
            {
                Console.WriteLine("🔍 [DEBUG] EmployeeService.DebugDatabaseAsync() started");
                
                var employees = await _repository.GetAllAsync();
                var employeeList = employees.ToList();
                
                Console.WriteLine($"🔍 [DEBUG] Service: Repository returned {employeeList.Count} employees");
                
                foreach (var emp in employeeList.Take(5)) // Chỉ log 5 employee đầu tiên
                {
                    Console.WriteLine($"  - Employee {emp.EmployeeId}: {emp.EmployeeName} (Username: {emp.Username}, WorkStatus: {emp.WorkStatus})");
                }

                return new
                {
                    totalEmployees = employeeList.Count,
                    employees = employeeList.Select(e => new
                    {
                        e.EmployeeId,
                        e.EmployeeName,
                        e.Username,
                        e.WorkStatus,
                        e.Phone,
                        HasPassword = !string.IsNullOrEmpty(e.Password),
                        PasswordFirst10 = !string.IsNullOrEmpty(e.Password) ? 
                            e.Password.Substring(0, Math.Min(10, e.Password.Length)) + "..." : "",
                        e.CreatedAt,
                        e.ShopOwnerId
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [DEBUG] Error in EmployeeService.DebugDatabaseAsync: {ex.Message}");
                Console.WriteLine($"❌ [DEBUG] Stack trace: {ex.StackTrace}");
                throw;
            }
        }
        
        public async Task<PaginatedResponse<EmployeeDto>> GetPaginatedAsync(PaginationRequest request)
        {
            var (data, totalRecords) = await _repository.GetPaginatedAsync(request);
            var employeeDtos = _mapper.Map<IEnumerable<EmployeeDto>>(data);
            
            return PaginatedResponse<EmployeeDto>.Create(employeeDtos, request.Page, request.PageSize, totalRecords);
        }

        public async Task<EmployeeDto> GetByUsernameAsync(string username)
        {
            Console.WriteLine($"🔍 [DEBUG] EmployeeService.GetByUsernameAsync called with: {username}");
            
            var employee = await _repository.GetByUsernameAsync(username);
            
            Console.WriteLine($"🔍 [DEBUG] Service: Repository returned employee: {employee != null}");
            
            if (employee != null)
            {
                Console.WriteLine($"🔍 [DEBUG] Service: Employee ID={employee.EmployeeId}, Name={employee.EmployeeName}");
            }
            
            return employee == null ? null : _mapper.Map<EmployeeDto>(employee);
        }

        // ✅ THÊM METHOD BỎ QUA GLOBAL QUERY FILTER
        public async Task<IEnumerable<EmployeeDto>> GetAllWithoutFilterAsync()
        {
            Console.WriteLine("🔍 [DEBUG] EmployeeService.GetAllWithoutFilterAsync called");
            
            var employees = await _repository.GetAllWithoutFilterAsync();
            var result = _mapper.Map<IEnumerable<EmployeeDto>>(employees);
            
            Console.WriteLine($"🔍 [DEBUG] Service: Mapped {result.Count()} employees without filter");
            
            return result;
        }

        // ✅ THÊM METHOD CHO EMPLOYEE TỰ CẬP NHẬT (CHỈ CÁC TRƯỜNG ĐƯỢC PHÉP)
        public async Task<EmployeeDto> UpdateByEmployeeAsync(int employeeId, EmployeeUpdateByEmployeeDto dto)
        {
            Console.WriteLine($"🔍 [DEBUG] UpdateByEmployeeAsync called for EmployeeId: {employeeId}");
            
            var employee = await _repository.GetByIdAsync(employeeId);
            if (employee == null)
                throw new Exception("Không tìm thấy nhân viên để cập nhật.");

            Console.WriteLine($"🔒 [DEBUG] TRƯỚC KHI CẬP NHẬT:");
            Console.WriteLine($"  - EmployeeCode: {employee.EmployeeCode} (KHÔNG ĐỔI)");
            Console.WriteLine($"  - EmployeeName: {employee.EmployeeName} (KHÔNG ĐỔI)");
            Console.WriteLine($"  - Position: {employee.Position} (KHÔNG ĐỔI)");
            Console.WriteLine($"  - Salary: {employee.Salary} (KHÔNG ĐỔI)");
            Console.WriteLine($"  - Phone: {employee.Phone} → {dto.Phone ?? employee.Phone}");

            // ✅ CHỈ CẬP NHẬT CÁC TRƯỜNG ĐƯỢC PHÉP - GIỮ NGUYÊN CÁC TRƯỜNG KHÁC
            employee.Phone = dto.Phone ?? employee.Phone;
            employee.Email = dto.Email ?? employee.Email;
            employee.Address = dto.Address ?? employee.Address;
            
            if (dto.DateOfBirth.HasValue)
                employee.DateOfBirth = DateTime.SpecifyKind(dto.DateOfBirth.Value, DateTimeKind.Utc); // ✅ SỬA
                
            employee.Gender = dto.Gender ?? employee.Gender;
            employee.IdCard = dto.IdCard ?? employee.IdCard;
            employee.BankAccount = dto.BankAccount ?? employee.BankAccount;
            employee.BankName = dto.BankName ?? employee.BankName;
            employee.Notes = dto.Notes ?? employee.Notes;

            // ✅ CẬP NHẬT MẬT KHẨU (NẾU CÓ) - HASH TRƯỚC KHI LƯU
            if (!string.IsNullOrEmpty(dto.Password))
            {
                employee.Password = YourShopManagement.API.Helpers.PasswordHelper.HashPassword(dto.Password);
                Console.WriteLine("🔒 [DEBUG] Employee password updated and hashed");
            }

            // ✅ XỬ LÝ AVATAR FILE (NẾU CÓ)
            if (dto.AvatarFile != null)
            {
                var ext = Path.GetExtension(dto.AvatarFile.FileName).ToLower();
                if (!new[] { ".jpg", ".jpeg", ".png" }.Contains(ext))
                    throw new Exception("Chỉ hỗ trợ định dạng .jpg, .jpeg, .png");

                if (dto.AvatarFile.Length > 5 * 1024 * 1024)
                    throw new Exception("Kích thước ảnh tối đa 5MB");

                var folder = Path.Combine(_env.WebRootPath, "uploads", "avatars");
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                // Xóa ảnh cũ nếu có
                if (!string.IsNullOrEmpty(employee.AvatarUrl))
                {
                    try
                    {
                        var oldFileName = Path.GetFileName(employee.AvatarUrl);
                        var oldPath = Path.Combine(folder, oldFileName);
                        if (File.Exists(oldPath))
                            File.Delete(oldPath);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ [DEBUG] Lỗi khi xóa ảnh cũ: {ex.Message}");
                    }
                }

                // Lưu ảnh mới
                var newFileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(folder, newFileName);
                
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.AvatarFile.CopyToAsync(stream);
                }

                employee.AvatarUrl = $"/uploads/avatars/{newFileName}";
                Console.WriteLine($"✅ [DEBUG] Employee avatar updated: {employee.AvatarUrl}");
            }
            else if (!string.IsNullOrEmpty(dto.AvatarUrl) && dto.AvatarUrl != employee.AvatarUrl)
            {
                employee.AvatarUrl = dto.AvatarUrl;
            }

            // ❌ CÁC TRƯỜNG SAU KHÔNG BAO GIỜ THAY ĐỔI (ĐƯỢC BẢO VỆ)
            // employee.ShopOwnerId - KHÔNG ĐỔI
            // employee.EmployeeCode - KHÔNG ĐỔI  
            // employee.EmployeeName - KHÔNG ĐỔI
            // employee.Position - KHÔNG ĐỔI
            // employee.Department - KHÔNG ĐỔI
            // employee.HireDate - KHÔNG ĐỔI
            // employee.Salary - KHÔNG ĐỔI
            // employee.SalaryType - KHÔNG ĐỔI
            // employee.Username - KHÔNG ĐỔI
            // employee.Permissions - KHÔNG ĐỔI
            // employee.WorkStatus - KHÔNG ĐỔI

            // ✅ THÊM: Convert tất cả DateTime properties sang UTC trước khi save
            ConvertEmployeeDateTimesToUtc(employee);

            employee.UpdatedAt = DateTime.UtcNow; // ✅ SỬA: Dùng UtcNow thay vì Now
            await _repository.UpdateAsync(employee);

            Console.WriteLine($"✅ [DEBUG] Employee {employeeId} updated successfully by self");
            Console.WriteLine($"🔒 [DEBUG] CÁC TRƯỜNG ĐƯỢC BẢO VỆ VẪN NGUYÊN:");
            Console.WriteLine($"  - EmployeeCode: {employee.EmployeeCode}");
            Console.WriteLine($"  - Position: {employee.Position}");
            Console.WriteLine($"  - Salary: {employee.Salary}");
            Console.WriteLine($"  - WorkStatus: {employee.WorkStatus}");

            return _mapper.Map<EmployeeDto>(employee);
        }

        /// <summary>
        /// ✅ Helper method: Convert tất cả DateTime properties sang UTC
        /// </summary>
        private void ConvertEmployeeDateTimesToUtc(Employee employee)
        {
            // Convert CreatedAt
            if (employee.CreatedAt.Kind == DateTimeKind.Unspecified)
                employee.CreatedAt = DateTime.SpecifyKind(employee.CreatedAt, DateTimeKind.Utc);
            else if (employee.CreatedAt.Kind == DateTimeKind.Local)
                employee.CreatedAt = employee.CreatedAt.ToUniversalTime();

            // Convert UpdatedAt
            if (employee.UpdatedAt.Kind == DateTimeKind.Unspecified)
                employee.UpdatedAt = DateTime.SpecifyKind(employee.UpdatedAt, DateTimeKind.Utc);
            else if (employee.UpdatedAt.Kind == DateTimeKind.Local)
                employee.UpdatedAt = employee.UpdatedAt.ToUniversalTime();

            // Convert HireDate
            if (employee.HireDate.Kind == DateTimeKind.Unspecified)
                employee.HireDate = DateTime.SpecifyKind(employee.HireDate, DateTimeKind.Utc);
            else if (employee.HireDate.Kind == DateTimeKind.Local)
                employee.HireDate = employee.HireDate.ToUniversalTime();

            // Convert DateOfBirth (nullable)
            if (employee.DateOfBirth.HasValue)
            {
                var dob = employee.DateOfBirth.Value;
                if (dob.Kind == DateTimeKind.Unspecified)
                    employee.DateOfBirth = DateTime.SpecifyKind(dob, DateTimeKind.Utc);
                else if (dob.Kind == DateTimeKind.Local)
                    employee.DateOfBirth = dob.ToUniversalTime();
            }

            Console.WriteLine("✅ [DEBUG] Đã convert tất cả DateTime sang UTC");
        }
    }
}
