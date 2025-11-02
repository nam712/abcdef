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

        [HttpPost("Create")]
        [SwaggerOperation(Summary = "➕ Tạo sản phẩm mới")]
        public async Task<IActionResult> Create([FromBody] ProductCreateDto dto)
        {
            Console.WriteLine($"📥 Creating product: {System.Text.Json.JsonSerializer.Serialize(dto)}");

            // Xử lý ảnh base64 nếu có
            if (!string.IsNullOrEmpty(dto.ImageUrl) && dto.ImageUrl.StartsWith("data:image"))
            {
                try
                {
                    var base64Data = dto.ImageUrl.Substring(dto.ImageUrl.IndexOf(",") + 1);
                    var bytes = Convert.FromBase64String(base64Data);
                    var fileName = $"product_{DateTime.Now.Ticks}.png";
                    var filePath = Path.Combine("wwwroot", "uploads", "products", fileName);

                    // Đảm bảo thư mục tồn tại
                    var dir = Path.GetDirectoryName(filePath);
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    System.IO.File.WriteAllBytes(filePath, bytes);

                    // Lưu đường dẫn file thay cho base64
                    dto.ImageUrl = $"/uploads/products/{fileName}";
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error saving image: {ex.Message}");
                    return BadRequest(new
                    {
                        success = false,
                        message = "Ảnh sản phẩm không hợp lệ hoặc không thể lưu."
                    });
                }
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                Console.WriteLine($"❌ Validation errors: {string.Join(", ", errors)}");

                return BadRequest(new
                {
                    success = false,
                    message = "Dữ liệu không hợp lệ",
                    errors = errors
                });
            }

            try
            {
                var created = await _service.CreateAsync(dto);
                Console.WriteLine($"✅ Product created: {created.ProductId}");

                return Ok(new
                {
                    success = true,
                    message = "Tạo sản phẩm thành công",
                    data = created
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
                Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    Console.WriteLine($"❌ Inner exception: {ex.InnerException.Message}");
                    Console.WriteLine($"❌ Inner stack trace: {ex.InnerException.StackTrace}");
                }

                return BadRequest(new
                {
                    success = false,
                    message = $"Lỗi: {ex.Message}. Inner: {ex.InnerException?.Message ?? "N/A"}"
                });
            }
        }

        [HttpPut("Update/{id}")]
        [SwaggerOperation(Summary = "✏️ Cập nhật sản phẩm theo ID")]
        public async Task<IActionResult> Update(int id)
        {
            // Hỗ trợ multipart/form-data (FormData) cho cập nhật sản phẩm có ảnh
            var form = await Request.ReadFormAsync();
            var dto = new ProductCreateDto
            {
                ProductCode = form["productCode"],
                ProductName = form["productName"],
                Description = form["description"],
                CategoryId = int.TryParse(form["categoryId"], out var cid) ? cid : 0,
                Brand = form["brand"],
                SupplierId = int.TryParse(form["supplierId"], out var sid) ? sid : 0,
                Price = decimal.TryParse(form["price"], out var price) ? price : 0,
                CostPrice = decimal.TryParse(form["costPrice"], out var cost) ? cost : 0,
                Stock = int.TryParse(form["stock"], out var stock) ? stock : 0,
                MinStock = int.TryParse(form["minStock"], out var minStock) ? minStock : 0,
                Sku = form["sku"],
                Barcode = form["barcode"],
                Unit = form["unit"],
                Notes = form["notes"],
                Weight = decimal.TryParse(form["weight"], out var weight) ? weight : (decimal?)null,
                Dimension = form["dimension"],
                ImageUrl = null // sẽ xử lý bên dưới
            };

            // Xử lý file ảnh nếu có
            var file = form.Files.FirstOrDefault();
            if (file != null && file.Length > 0)
            {
                var fileName = $"product_{DateTime.Now.Ticks}.png";
                var filePath = Path.Combine("wwwroot", "uploads", "products", fileName);
                var dir = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                dto.ImageUrl = $"/uploads/products/{fileName}";
            }
            else if (!string.IsNullOrEmpty(form["imageUrl"]))
            {
                dto.ImageUrl = form["imageUrl"];
            }

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
