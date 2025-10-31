using Microsoft.AspNetCore.Mvc;
using YourShopManagement.API.DTOs.PurchaseOrder;
using YourShopManagement.API.Services;
using YourShopManagement.API.Services.Interfaces;

namespace YourShopManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Tags("📦 Purchase Order Management")]
    public class PurchaseOrderController : ControllerBase
    {
        private readonly IPurchaseOrderService _service;

        public PurchaseOrderController(IPurchaseOrderService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePurchaseOrderDto dto)
        {
            var (success, msg, data) = await _service.CreateAsync(dto);
            if (!success) return BadRequest(new { success, msg });
            return Ok(data);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, msg) = await _service.DeleteAsync(id);
            if (!success) return NotFound(new { success, msg });
            return Ok(new { success, msg });
        }
    }
}