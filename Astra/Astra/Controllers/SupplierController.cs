using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using YourShopManagement.API.DTOs.Supplier;
using YourShopManagement.API.Services;

namespace YourShopManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Tags("🏢 Supplier Management")]
    [AllowAnonymous]
    /// <summary>
    /// API quản lý nhà cung cấp - CRUD operations cho danh sách nhà cung cấp
    /// </summary>
    public class SupplierController : ControllerBase
    {
        private readonly ISupplierService _supplierService;

        public SupplierController(ISupplierService supplierService)
        {
            _supplierService = supplierService;
        }

        /// <summary>
        /// 📋 Lấy danh sách tất cả nhà cung cấp
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "ShopOwner")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAllSuppliers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _supplierService.GetAllSuppliersAsync(page, pageSize);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// 🔍 Lấy thông tin chi tiết nhà cung cấp theo ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "ShopOwner")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetSupplierById(int id)
        {
            var result = await _supplierService.GetSupplierByIdAsync(id);
            
            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// ➕ Thêm nhà cung cấp mới
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "ShopOwner")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateSupplier([FromBody] CreateSupplierDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Dữ liệu không hợp lệ",
                    errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList()
                });
            }

            var result = await _supplierService.CreateSupplierAsync(dto);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// ✏️ Cập nhật thông tin nhà cung cấp
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "ShopOwner")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateSupplier(int id, [FromBody] UpdateSupplierDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Dữ liệu không hợp lệ",
                    errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList()
                });
            }

            var result = await _supplierService.UpdateSupplierAsync(id, dto);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// 🗑️ Xóa nhà cung cấp
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "ShopOwner")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteSupplier(int id)
        {
            var result = await _supplierService.DeleteSupplierAsync(id);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// 🔍 Tìm kiếm nhà cung cấp theo tên
        /// </summary>
        [HttpGet("search")]
        [Authorize(Roles = "ShopOwner")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> SearchSuppliers([FromQuery] SearchSupplierDto searchDto)
        {
            var result = await _supplierService.SearchSuppliersAsync(searchDto);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
