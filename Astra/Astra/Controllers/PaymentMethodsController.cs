using Microsoft.AspNetCore.Mvc;
using Backend.DTOs;
using Backend.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentMethodsController : ControllerBase
    {
        private readonly IPaymentMethodService _service;

        public PaymentMethodsController(IPaymentMethodService service)
        {
            _service = service;
        }

        // GET /api/paymentmethods
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PaymentMethodDto>>> GetAll()
        {
            try
            {
                var methods = await _service.GetAllAsync();
                if (methods == null || !methods.Any())
                    return NotFound(new { message = "Chưa có phương thức thanh toán nào trong hệ thống." });

                return Ok(methods);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy danh sách phương thức thanh toán", detail = ex.Message });
            }
        }

        // GET /api/paymentmethods/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentMethodDto>> GetById(int id)
        {
            try
            {
                var method = await _service.GetByIdAsync(id);
                if (method == null)
                    return NotFound(new { message = $"Không tìm thấy phương thức thanh toán với ID = {id}" });

                return Ok(method);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy thông tin phương thức thanh toán", detail = ex.Message });
            }
        }

        // GET /api/paymentmethods/search?keyword=
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<PaymentMethodDto>>> Search([FromQuery] string keyword)
        {
            try
            {
                var methods = await _service.SearchAsync(keyword ?? string.Empty);
                if (methods == null || !methods.Any())
                    return NotFound(new { message = $"Không tìm thấy phương thức thanh toán với từ khóa '{keyword}'." });

                return Ok(methods);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi tìm kiếm phương thức thanh toán", detail = ex.Message });
            }
        }

        // GET /api/paymentmethods/active
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<PaymentMethodDto>>> GetActive()
        {
            try
            {
                var methods = await _service.GetActiveAsync();
                if (methods == null || !methods.Any())
                    return NotFound(new { message = "Chưa có phương thức thanh toán đang hoạt động." });

                return Ok(methods);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy danh sách phương thức thanh toán đang hoạt động", detail = ex.Message });
            }
        }

        // POST /api/paymentmethods
        [HttpPost]
        public async Task<ActionResult<PaymentMethodDto>> Create([FromBody] PaymentMethodDto dto)
        {
            try
            {
                var created = await _service.AddAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.PaymentMethodId }, created);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi tạo phương thức thanh toán mới", detail = ex.Message });
            }
        }

        // PUT /api/paymentmethods/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] PaymentMethodDto dto)
        {
            if (id != dto.PaymentMethodId)
                return BadRequest(new { message = "ID trong URL và ID trong dữ liệu không khớp" });

            try
            {
                await _service.UpdateAsync(dto);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật phương thức thanh toán", detail = ex.Message });
            }
        }

        // DELETE /api/paymentmethods/{id}
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
                return StatusCode(500, new { message = "Lỗi khi xóa phương thức thanh toán", detail = ex.Message });
            }
        }
    }
}
