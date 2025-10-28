// Implementation of IEmployeeService using IEmployeeRepository and AutoMapper
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Backend.DTOs;
using Backend.Models;
using Backend.Repositories;

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
            var added = await _repository.AddAsync(employee);
            return _mapper.Map<EmployeeDto>(added);
        }

        public async Task UpdateAsync(EmployeeDto employeeDto)
        {
            var employee = _mapper.Map<Employee>(employeeDto);
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
            string oldAvatarUrl = null; // Lưu đường dẫn ảnh cũ để xóa

            if (dto.EmployeeId != 0)
            {
                // lấy bản ghi hiện có
                employee = await _repository.GetByIdAsync(dto.EmployeeId);
                if (employee == null)
                    throw new Exception("Không tìm thấy nhân viên để cập nhật.");

                // Lưu đường dẫn ảnh cũ trước khi map
                oldAvatarUrl = employee.AvatarUrl;

                // map giá trị mới vào đối tượng đang track
                _mapper.Map(dto, employee);
            }
            else
            {
                // thêm mới
                employee = _mapper.Map<Employee>(dto);
                employee.CreatedAt = DateTime.Now;
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
            employee.UpdatedAt = DateTime.Now;

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

    }


}
