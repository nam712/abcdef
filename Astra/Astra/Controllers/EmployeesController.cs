using Backend.DTOs;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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

        // GET /api/employees/search?name=
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<EmployeeDto>>> Search([FromQuery] string name)
        {
            try
            {
                var employees = await _service.SearchAsync(name ?? string.Empty);
                if (employees == null || !employees.Any())
                    return NotFound(new { message = $"Không tìm thấy nhân viên với từ khóa '{name}'." });

                return Ok(employees);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi tìm kiếm nhân viên", detail = ex.Message });
            }
        }

        // POST /api/employees
        [HttpPost]
        public async Task<ActionResult<EmployeeDto>> Create([FromBody] EmployeeDto employeeDto)
        {
            try
            {
                var created = await _service.AddAsync(employeeDto);
                return CreatedAtAction(nameof(GetById), new { id = created.EmployeeId }, created);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi tạo nhân viên mới", detail = ex.Message });
            }
        }

        // PUT /api/employees/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] EmployeeDto employeeDto)
        {
            if (id != employeeDto.EmployeeId)
                return BadRequest(new { message = "ID trong URL và ID trong dữ liệu không khớp" });

            try
            {
                await _service.UpdateAsync(employeeDto);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật nhân viên", detail = ex.Message });
            }
        }

        // DELETE /api/employees/{id}
        [HttpDelete("{id}")]
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

        // Delete avatar file
        [HttpDelete("{id}/avatar")]
        public async Task<IActionResult> DeleteAvatar(int id)
        {
            try
            {
                await _service.DeleteAvatarAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi xóa ảnh nhân viên", detail = ex.Message });
            }
        }
    }
}
