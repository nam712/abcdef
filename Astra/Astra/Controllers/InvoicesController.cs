using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using YourShopManagement.API.Common;
using YourShopManagement.API.DTOs.Invoice;
using YourShopManagement.API.Services.Interfaces;

namespace YourShopManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class InvoicesController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;

        public InvoicesController(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        /// <summary>
        /// Lấy danh sách tất cả hóa đơn
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<InvoiceListDto>>>> GetAll()
        {
            var response = await _invoiceService.GetAllAsync();
            return Ok(response);
        }

        /// <summary>
        /// Lấy thông tin hóa đơn theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<InvoiceResponseDto>>> GetById(int id)
        {
            var response = await _invoiceService.GetByIdAsync(id);
            return Ok(response);
        }

        /// <summary>
        /// Tạo hóa đơn mới
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<InvoiceResponseDto>>> Create([FromBody] CreateInvoiceDto dto)
        {
            var response = await _invoiceService.CreateAsync(dto);
            return Ok(response);
        }

        /// <summary>
        /// Cập nhật thông tin hóa đơn
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<InvoiceResponseDto>>> Update(int id, [FromBody] UpdateInvoiceDto dto)
        {
            var response = await _invoiceService.UpdateAsync(id, dto);
            return Ok(response);
        }

        /// <summary>
        /// Thanh toán hóa đơn
        /// </summary>
        [HttpPost("{id}/payment")]
        public async Task<ActionResult<ApiResponse<InvoiceResponseDto>>> Payment(int id, [FromBody] PaymentInvoiceDto dto)
        {
            var response = await _invoiceService.PaymentAsync(id, dto);
            return Ok(response);
        }

        // /// <summary>
        // /// Xóa hóa đơn - Tạm thời chưa implement
        // /// </summary>
        // [HttpDelete("{id}")]
        // public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
        // {
        //     var response = await _invoiceService.DeleteAsync(id);
        //     return Ok(response);
        // }

        /// <summary>
        /// Tìm kiếm hóa đơn
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<ApiResponse<List<InvoiceListDto>>>> Search([FromQuery] string keyword)
        {
            var response = await _invoiceService.SearchAsync(keyword);
            return Ok(response);
        }

        /// <summary>
        /// Lọc hóa đơn theo trạng thái thanh toán
        /// </summary>
        [HttpGet("by-status/{status}")]
        public async Task<ActionResult<ApiResponse<List<InvoiceListDto>>>> GetByStatus(string status)
        {
            var response = await _invoiceService.GetByPaymentStatusAsync(status);
            return Ok(response);
        }

        /// <summary>
        /// Lấy hóa đơn theo khách hàng
        /// </summary>
        [HttpGet("by-customer/{customerId}")]
        public async Task<ActionResult<ApiResponse<List<InvoiceListDto>>>> GetByCustomer(int customerId)
        {
            var response = await _invoiceService.GetByCustomerIdAsync(customerId);
            return Ok(response);
        }
        /// <summary>
        /// Xuất hóa đơn ra file PDF
        /// </summary>
        [HttpGet("{id}/print")]
        public async Task<IActionResult> PrintInvoice(int id)
        {
            try
            {
                var fileBytes = await _invoiceService.GenerateInvoicePdfAsync(id);
                if (fileBytes == null)
                    return NotFound(ApiResponse<object>.FailResponse(
                        message: "Không tìm thấy hóa đơn",
                        error: "Invoice not found"
                    ));

                return File(fileBytes, "application/pdf", $"HoaDon_{id}.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.FailResponse(
                    message: "Lỗi khi tạo file PDF cho hóa đơn",
                    error: ex.Message
                ));
            }
        }

    }

}