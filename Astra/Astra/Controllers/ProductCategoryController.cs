using Microsoft.AspNetCore.Mvc;
using Services;
using ProductCategoryApi.DTOs;
using Swashbuckle.AspNetCore.Annotations;

namespace ProductCategoryApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductCategoryController : ControllerBase
    {
        private readonly IProductCategoryService _service;
        public ProductCategoryController(IProductCategoryService service) => _service = service;

        [HttpGet("GetAll")]
        [SwaggerOperation(Summary = "Lấy danh sách danh mục sản phẩm")]
        public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Lấy danh mục theo ID")]
        public async Task<IActionResult> GetById(int id)
        {
            var r = await _service.GetByIdAsync(id);
            if (r == null) return NotFound();
            return Ok(r);
        }

        [HttpPost("Create")]
        [SwaggerOperation(Summary = "Tạo danh mục mới")]
        public async Task<IActionResult> Create([FromBody] ProductCategoryCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.CategoryId }, created);
        }

        [HttpPut("Update/{id}")]
        [SwaggerOperation(Summary = "Cập nhật danh mục")]
        public async Task<IActionResult> Update(int id, [FromBody] ProductCategoryCreateDto dto)
        {
            var updated = await _service.UpdateAsync(id, dto);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpDelete("Delete/{id}")]
        [SwaggerOperation(Summary = "Xóa danh mục theo ID")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
