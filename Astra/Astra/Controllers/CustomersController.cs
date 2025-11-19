using YourShopManagement.API.DTOs;
using YourShopManagement.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;  
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using YourShopManagement.API.DTOs.Common;

namespace YourShopManagement.API.Controllers  
{
   
    [ApiController]
    [Route("api/[controller]")]
  
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _service;

        public CustomersController(ICustomerService service)
        {
            _service = service;
        }

        // GET /api/customers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> GetAll()
        {
            try
            {
                var customers = await _service.GetAllAsync();
                if (customers == null || !customers.Any())
                {
                    return NotFound(new { message = "Chưa có khách hàng nào trong hệ thống." });
                }

                return Ok(customers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy danh sách khách hàng", detail = ex.Message });
            }
        }

        [HttpGet("paginated")]
        public async Task<ActionResult<PaginatedResponse<CustomerDto>>> GetPaginated([FromQuery] PaginationRequest request)
        {
            try
            {
                var result = await _service.GetPaginatedAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy danh sách khách hàng phân trang", detail = ex.Message });
            }
        }

        // GET /api/customers/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerDto>> GetById(int id)
        {
            try
            {
                var customer = await _service.GetByIdAsync(id);
                if (customer == null)
                    return NotFound(new { message = $"Không tìm thấy khách hàng với ID = {id}" });

                return Ok(customer);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy thông tin khách hàng", detail = ex.Message });
            }
        }

        // // GET /api/customers/search?name=
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> Search([FromQuery] string name)
        {
            try
            {
                var customers = await _service.SearchAsync(name ?? string.Empty);
                if (customers == null || !customers.Any())
                {
                    return NotFound(new { message = $"Không tìm thấy khách hàng với từ khóa '{name}'." });
                }

                return Ok(customers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi tìm kiếm khách hàng", detail = ex.Message });
            }
        }

        // POST /api/customers
        [HttpPost]
        public async Task<ActionResult<CustomerDto>> Create([FromBody] CustomerDto customerDto)
        {
            try
            {
                var created = await _service.AddAsync(customerDto);
                return CreatedAtAction(nameof(GetById), new { id = created.CustomerId }, created);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi tạo khách hàng mới", detail = ex.Message });
            }
        }

        // PUT /api/customers/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CustomerDto customerDto)
        {
            if (id != customerDto.CustomerId)
                return BadRequest(new { message = "ID trong URL và ID trong dữ liệu không khớp" });

            try
            {
                await _service.UpdateAsync(customerDto);
                return Ok(new { 
                    message = "Cập nhật khách hàng thành công", 
                    customerId = id,
                    customerCode = customerDto.CustomerCode,
                    customerName = customerDto.CustomerName
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật khách hàng", detail = ex.Message });
            }
        }

        // DELETE /api/customers/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var customer = await _service.GetByIdAsync(id);
                if (customer == null)
                    return NotFound(new { message = $"Không tìm thấy khách hàng với ID = {id}" });

                await _service.DeleteAsync(id);
                return Ok(new { 
                    message = "Xóa khách hàng thành công", 
                    customerId = id,
                    customerCode = customer.CustomerCode,
                    customerName = customer.CustomerName
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi xóa khách hàng", detail = ex.Message });
            }
        }
    }
}
