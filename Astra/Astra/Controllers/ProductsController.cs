using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;
using ProductApi.DTOs;
using Swashbuckle.AspNetCore.Annotations;

namespace ProductApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Tags("📦 Product Management")]
    [Authorize(Roles = "ShopOwner")]
   
    public class ProductController : ControllerBase
    {
        private readonly IProductService _service;
        
        public ProductController(IProductService service) => _service = service;

        [HttpGet("GetAll")]
        [SwaggerOperation(Summary = "📋 Lấy danh sách tất cả sản phẩm")]
        public async Task<IActionResult> GetAll()
        {
            Console.WriteLine("📋 GetAll products called");
            try
            {
                var products = await _service.GetAllAsync();
                Console.WriteLine($"✅ Service returned products count: {products?.Count() ?? 0}");
                
                // Ensure we return an empty array instead of null
                var productList = products?.ToList() ?? new List<ProductDto>();
                
                return Ok(new
                {
                    success = true,
                    message = "Lấy danh sách sản phẩm thành công",
                    data = productList
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception: {ex.Message}");
                Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"❌ Inner exception: {ex.InnerException.Message}");
                }
                
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Server error: {ex.Message}",
                    data = new List<ProductDto>()
                });
            }
        }

        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "🔍 Lấy sản phẩm theo ID")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _service.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Không tìm thấy sản phẩm"
                });
            }
            
            return Ok(new
            {
                success = true,
                message = "Lấy thông tin sản phẩm thành công",
                data = product
            });
        }

        // ❌ POST endpoint đã bị XÓA
        // Sản phẩm chỉ được tạo tự động qua phiếu nhập hàng (POST /api/purchase-orders)

        [HttpPut("Update/{id}")]
        [SwaggerOperation(Summary = "✏️ Cập nhật sản phẩm theo ID")]
        public async Task<IActionResult> Update(int id, [FromBody] ProductUpdateDto dto)
        {
            var updated = await _service.UpdateAsync(id, dto);
            if (updated == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Không tìm thấy sản phẩm"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Cập nhật sản phẩm thành công",
                data = updated
            });
        }

        [HttpDelete("Delete/{id}")]
        [SwaggerOperation(Summary = "🗑️ Xóa sản phẩm theo ID")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Không tìm thấy sản phẩm"
                });
            }
            
            return Ok(new
            {
                success = true,
                message = "Xóa sản phẩm thành công"
            });
        }
    }
}
