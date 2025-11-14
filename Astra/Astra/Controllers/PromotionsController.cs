using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YourShopManagement.API.Common;
using YourShopManagement.API.DTOs.Promotion;
using YourShopManagement.API.Services.PromotionService;

namespace YourShopManagement.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PromotionsController : ControllerBase
    {
        private readonly IPromotionService _promotionService;

        public PromotionsController(IPromotionService promotionService)
        {
            _promotionService = promotionService;
        }

        // GET: api/promotions
        [HttpGet]
        public async Task<IActionResult> GetAllPromotions()
        {
            try
            {
                var shopOwnerIdClaim = User.FindFirst("shop_owner_id")?.Value;
                if (string.IsNullOrEmpty(shopOwnerIdClaim) || !int.TryParse(shopOwnerIdClaim, out int shopOwnerId))
                {
                    return Unauthorized(new ApiResponse<object>(false, "Không tìm thấy thông tin chủ shop"));
                }

                var promotions = await _promotionService.GetAllPromotionsAsync(shopOwnerId);
                return Ok(new ApiResponse<IEnumerable<PromotionDto>>(true, "Lấy danh sách khuyến mãi thành công", null, promotions));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>(false, $"Lỗi: {ex.Message}"));
            }
        }

        // GET: api/promotions/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPromotionById(int id)
        {
            try
            {
                var shopOwnerIdClaim = User.FindFirst("shop_owner_id")?.Value;
                if (string.IsNullOrEmpty(shopOwnerIdClaim) || !int.TryParse(shopOwnerIdClaim, out int shopOwnerId))
                {
                    return Unauthorized(new ApiResponse<object>(false, "Không tìm thấy thông tin chủ shop"));
                }

                var promotion = await _promotionService.GetPromotionByIdAsync(id, shopOwnerId);
                if (promotion == null)
                {
                    return NotFound(new ApiResponse<object>(false, "Không tìm thấy khuyến mãi"));
                }

                return Ok(new ApiResponse<PromotionDto>(true, "Lấy thông tin khuyến mãi thành công", null, promotion));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>(false, $"Lỗi: {ex.Message}"));
            }
        }

        // POST: api/promotions
        [HttpPost]
        public async Task<IActionResult> CreatePromotion([FromBody] CreatePromotionDto dto)
        {
            try
            {
                var shopOwnerIdClaim = User.FindFirst("shop_owner_id")?.Value;
                if (string.IsNullOrEmpty(shopOwnerIdClaim) || !int.TryParse(shopOwnerIdClaim, out int shopOwnerId))
                {
                    return Unauthorized(new ApiResponse<object>(false, "Không tìm thấy thông tin chủ shop"));
                }

                var promotion = await _promotionService.CreatePromotionAsync(dto, shopOwnerId);
                return CreatedAtAction(nameof(GetPromotionById), new { id = promotion.PromotionId },
                    new ApiResponse<PromotionDto>(true, "Tạo khuyến mãi thành công", null, promotion));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<object>(false, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>(false, $"Lỗi: {ex.Message}"));
            }
        }

        // PUT: api/promotions/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePromotion(int id, [FromBody] UpdatePromotionDto dto)
        {
            try
            {
                var shopOwnerIdClaim = User.FindFirst("shop_owner_id")?.Value;
                if (string.IsNullOrEmpty(shopOwnerIdClaim) || !int.TryParse(shopOwnerIdClaim, out int shopOwnerId))
                {
                    return Unauthorized(new ApiResponse<object>(false, "Không tìm thấy thông tin chủ shop"));
                }

                var promotion = await _promotionService.UpdatePromotionAsync(id, dto, shopOwnerId);
                if (promotion == null)
                {
                    return NotFound(new ApiResponse<object>(false, "Không tìm thấy khuyến mãi"));
                }

                return Ok(new ApiResponse<PromotionDto>(true, "Cập nhật khuyến mãi thành công", null, promotion));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<object>(false, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>(false, $"Lỗi: {ex.Message}"));
            }
        }

        // DELETE: api/promotions/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePromotion(int id)
        {
            try
            {
                var shopOwnerIdClaim = User.FindFirst("shop_owner_id")?.Value;
                if (string.IsNullOrEmpty(shopOwnerIdClaim) || !int.TryParse(shopOwnerIdClaim, out int shopOwnerId))
                {
                    return Unauthorized(new ApiResponse<object>(false, "Không tìm thấy thông tin chủ shop"));
                }

                var result = await _promotionService.DeletePromotionAsync(id, shopOwnerId);
                if (!result)
                {
                    return NotFound(new ApiResponse<object>(false, "Không tìm thấy khuyến mãi"));
                }

                return Ok(new ApiResponse<object>(true, "Xóa khuyến mãi thành công"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>(false, $"Lỗi: {ex.Message}"));
            }
        }

        // GET: api/promotions/search?keyword=xxx
        [HttpGet("search")]
        public async Task<IActionResult> SearchPromotions([FromQuery] string keyword)
        {
            try
            {
                var shopOwnerIdClaim = User.FindFirst("shop_owner_id")?.Value;
                if (string.IsNullOrEmpty(shopOwnerIdClaim) || !int.TryParse(shopOwnerIdClaim, out int shopOwnerId))
                {
                    return Unauthorized(new ApiResponse<object>(false, "Không tìm thấy thông tin chủ shop"));
                }

                var promotions = await _promotionService.SearchPromotionsAsync(keyword, shopOwnerId);
                return Ok(new ApiResponse<IEnumerable<PromotionDto>>(true, "Tìm kiếm khuyến mãi thành công", null, promotions));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>(false, $"Lỗi: {ex.Message}"));
            }
        }

        // GET: api/promotions/status/{status}
        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetPromotionsByStatus(string status)
        {
            try
            {
                var shopOwnerIdClaim = User.FindFirst("shop_owner_id")?.Value;
                if (string.IsNullOrEmpty(shopOwnerIdClaim) || !int.TryParse(shopOwnerIdClaim, out int shopOwnerId))
                {
                    return Unauthorized(new ApiResponse<object>(false, "Không tìm thấy thông tin chủ shop"));
                }

                var promotions = await _promotionService.GetPromotionsByStatusAsync(status, shopOwnerId);
                return Ok(new ApiResponse<IEnumerable<PromotionDto>>(true, "Lấy danh sách khuyến mãi theo trạng thái thành công", null, promotions));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>(false, $"Lỗi: {ex.Message}"));
            }
        }

        // GET: api/promotions/active
        [HttpGet("active")]
        public async Task<IActionResult> GetActivePromotions()
        {
            try
            {
                var shopOwnerIdClaim = User.FindFirst("shop_owner_id")?.Value;
                if (string.IsNullOrEmpty(shopOwnerIdClaim) || !int.TryParse(shopOwnerIdClaim, out int shopOwnerId))
                {
                    return Unauthorized(new ApiResponse<object>(false, "Không tìm thấy thông tin chủ shop"));
                }

                var promotions = await _promotionService.GetActivePromotionsAsync(shopOwnerId);
                return Ok(new ApiResponse<IEnumerable<PromotionDto>>(true, "Lấy danh sách khuyến mãi đang hoạt động thành công", null, promotions));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>(false, $"Lỗi: {ex.Message}"));
            }
        }

        // GET: api/promotions/invoice/{invoiceId}
        [HttpGet("invoice/{invoiceId}")]
        public async Task<IActionResult> GetPromotionsByInvoiceId(int invoiceId)
        {
            try
            {
                var shopOwnerIdClaim = User.FindFirst("shop_owner_id")?.Value;
                if (string.IsNullOrEmpty(shopOwnerIdClaim) || !int.TryParse(shopOwnerIdClaim, out int shopOwnerId))
                {
                    return Unauthorized(new ApiResponse<object>(false, "Không tìm thấy thông tin chủ shop"));
                }

                var promotions = await _promotionService.GetPromotionsByInvoiceIdAsync(invoiceId, shopOwnerId);
                return Ok(new ApiResponse<IEnumerable<PromotionDto>>(true, "Lấy danh sách khuyến mãi theo hóa đơn thành công", null, promotions));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>(false, $"Lỗi: {ex.Message}"));
            }
        }
    }
}
