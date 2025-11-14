using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YourShopManagement.API.DTOs.Shop;
using YourShopManagement.API.Services.ShopService;
using System.Security.Claims;

namespace YourShopManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ShopsController : ControllerBase
    {
        private readonly IShopService _service;

        public ShopsController(IShopService service)
        {
            _service = service;
        }

        /// <summary>
        /// Lấy danh sách tất cả các cửa hàng của ShopOwner
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var shopOwnerIdClaim = User.FindFirst("shop_owner_id")?.Value;
                if (string.IsNullOrEmpty(shopOwnerIdClaim) || !int.TryParse(shopOwnerIdClaim, out var shopOwnerId))
                {
                    return Unauthorized(new { message = "Không xác định được chủ cửa hàng" });
                }

                var result = await _service.GetAllAsync(shopOwnerId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi", error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết một cửa hàng
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var shopOwnerIdClaim = User.FindFirst("shop_owner_id")?.Value;
                if (string.IsNullOrEmpty(shopOwnerIdClaim) || !int.TryParse(shopOwnerIdClaim, out var shopOwnerId))
                {
                    return Unauthorized(new { message = "Không xác định được chủ cửa hàng" });
                }

                var result = await _service.GetByIdAsync(id, shopOwnerId);
                if (result == null)
                {
                    return NotFound(new { message = "Không tìm thấy cửa hàng" });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi", error = ex.Message });
            }
        }

        /// <summary>
        /// Tạo cửa hàng mới
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateShopDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var shopOwnerIdClaim = User.FindFirst("shop_owner_id")?.Value;
                if (string.IsNullOrEmpty(shopOwnerIdClaim) || !int.TryParse(shopOwnerIdClaim, out var shopOwnerId))
                {
                    return Unauthorized(new { message = "Không xác định được chủ cửa hàng" });
                }

                var created = await _service.CreateAsync(dto, shopOwnerId);
                return CreatedAtAction(nameof(GetById), new { id = created.ShopId }, created);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi", error = ex.Message });
            }
        }

        /// <summary>
        /// Cập nhật thông tin cửa hàng
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateShopDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var shopOwnerIdClaim = User.FindFirst("shop_owner_id")?.Value;
                if (string.IsNullOrEmpty(shopOwnerIdClaim) || !int.TryParse(shopOwnerIdClaim, out var shopOwnerId))
                {
                    return Unauthorized(new { message = "Không xác định được chủ cửa hàng" });
                }

                var updated = await _service.UpdateAsync(id, dto, shopOwnerId);
                if (updated == null)
                {
                    return NotFound(new { message = "Không tìm thấy cửa hàng" });
                }

                return Ok(updated);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi", error = ex.Message });
            }
        }

        /// <summary>
        /// Xóa cửa hàng (soft delete - chuyển status thành inactive)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var shopOwnerIdClaim = User.FindFirst("shop_owner_id")?.Value;
                if (string.IsNullOrEmpty(shopOwnerIdClaim) || !int.TryParse(shopOwnerIdClaim, out var shopOwnerId))
                {
                    return Unauthorized(new { message = "Không xác định được chủ cửa hàng" });
                }

                var result = await _service.DeleteAsync(id, shopOwnerId);
                if (!result)
                {
                    return NotFound(new { message = "Không tìm thấy cửa hàng" });
                }

                return Ok(new { message = "Xóa cửa hàng thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi", error = ex.Message });
            }
        }

        /// <summary>
        /// Tìm kiếm cửa hàng
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string? keyword)
        {
            try
            {
                var shopOwnerIdClaim = User.FindFirst("shop_owner_id")?.Value;
                if (string.IsNullOrEmpty(shopOwnerIdClaim) || !int.TryParse(shopOwnerIdClaim, out var shopOwnerId))
                {
                    return Unauthorized(new { message = "Không xác định được chủ cửa hàng" });
                }

                var result = await _service.SearchAsync(keyword, shopOwnerId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi", error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy danh sách cửa hàng theo trạng thái
        /// </summary>
        [HttpGet("by-status/{status}")]
        public async Task<IActionResult> GetByStatus(string status)
        {
            try
            {
                var shopOwnerIdClaim = User.FindFirst("shop_owner_id")?.Value;
                if (string.IsNullOrEmpty(shopOwnerIdClaim) || !int.TryParse(shopOwnerIdClaim, out var shopOwnerId))
                {
                    return Unauthorized(new { message = "Không xác định được chủ cửa hàng" });
                }

                var result = await _service.GetByStatusAsync(status, shopOwnerId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi", error = ex.Message });
            }
        }
    }
}
