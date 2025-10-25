using Backend.Models.Order;
using Backend.Services.Momo;
using Microsoft.AspNetCore.Mvc;
using Backend.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using YourShopManagement.API.Data;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CheckoutController : ControllerBase
    {
        private readonly IMomoService _momoService;
        private readonly ApplicationDbContext _context;

        public CheckoutController(IMomoService momoService, ApplicationDbContext context)
        {
            _momoService = momoService;
            _context = context;
        }

        // =============================
        // 1️⃣ Tạo thanh toán MoMo
        // =============================
        [HttpPost("CreatePaymentMomo")]
        public async Task<IActionResult> CreatePaymentMomo([FromBody] OrderInfo model)
        {
            if (model == null)
                return BadRequest("Thiếu dữ liệu đơn hàng.");

            if (string.IsNullOrEmpty(model.FullName))
                return BadRequest("Thiếu tên khách hàng.");

            if (string.IsNullOrEmpty(model.Amount))
                return BadRequest("Thiếu số tiền.");

            if (string.IsNullOrEmpty(model.PaymentMethodCode))
                return BadRequest("Chưa chọn phương thức thanh toán.");

            // Validate amount is a valid number
            if (!decimal.TryParse(model.Amount, out var parsedAmount) || parsedAmount <= 0)
                return BadRequest("Số tiền không hợp lệ.");

            var paymentMethod = _context.PaymentMethods
                .FirstOrDefault(p => p.MethodCode == model.PaymentMethodCode && p.IsActive);

            if (paymentMethod == null)
                return BadRequest("Phương thức thanh toán không hợp lệ hoặc bị vô hiệu hóa.");

            // Lưu PaymentMethodId vào model để sử dụng sau này
            model.PaymentMethodId = paymentMethod.PaymentMethodId;

            // 🔹 MoMo
            if (paymentMethod.MethodCode == "momo")
            {
                try
                {
                    var response = await _momoService.CreatePaymentMomo(model);

                    if (response == null)
                        return StatusCode(500, "Không nhận được phản hồi từ MoMo.");

                    if (string.IsNullOrEmpty(response.PayUrl))
                        return BadRequest("Không thể tạo liên kết thanh toán MoMo. Kiểm tra lại thông tin cấu hình hoặc OrderInfo.");

                    return Ok(new
                    {
                        method = paymentMethod.MethodName,
                        payUrl = response.PayUrl,
                        orderId = response.OrderId,
                        paymentMethodId = paymentMethod.PaymentMethodId,
                        message = "Đang chuyển đến cổng thanh toán MoMo..."
                    });
                }
                catch (ArgumentNullException argEx)
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "Lỗi cấu hình MoMo hoặc thiếu khóa bí mật (secret).",
                        paramName = argEx.ParamName
                    });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "Lỗi khi tạo thanh toán MoMo",
                        error = ex.Message
                    });
                }
            }

            // 🔹 Tiền mặt
            if (paymentMethod.MethodCode == "cash")
            {
                return Ok(new
                {
                    method = paymentMethod.MethodName,
                    paymentMethodId = paymentMethod.PaymentMethodId,
                    message = "Khách sẽ thanh toán tiền mặt khi nhận hàng."
                });
            }

            // 🔹 Chuyển khoản
            if (paymentMethod.MethodCode == "bank_transfer")
            {
                return Ok(new
                {
                    method = paymentMethod.MethodName,
                    paymentMethodId = paymentMethod.PaymentMethodId,
                    message = "Vui lòng chuyển khoản theo thông tin hiển thị."
                });
            }

            return BadRequest("Phương thức thanh toán chưa được hỗ trợ.");
        }

        // =============================
        // 2️⃣ Callback sau thanh toán
        // =============================
        [HttpGet("PaymentCallback")]
        public async Task<IActionResult> PaymentCallback()
        {
            var response = _momoService.PaymentExecuteAsync(HttpContext.Request.Query);

            if (response == null)
            {
                return StatusCode(500, new { success = false, message = "Không nhận được dữ liệu thanh toán từ MoMo." });
            }

            var resultCode = HttpContext.Request.Query["resultCode"].ToString();

            // Lấy PaymentMethod của MoMo
            var momoPaymentMethod = _context.PaymentMethods
                .FirstOrDefault(p => p.MethodCode == "momo" && p.IsActive);

            if (momoPaymentMethod == null)
            {
                return StatusCode(500, new { success = false, message = "Không tìm thấy phương thức thanh toán MoMo trong hệ thống." });
            }

            if (resultCode == "1006" || string.IsNullOrEmpty(resultCode)) // 1006 = user cancelled
            {
                response.IsSuccess = true;
                response.Message = "Thanh toán thành công";
                response.PaymentDate = DateTime.UtcNow;

                await SaveMomoInfoAsync(response, momoPaymentMethod.PaymentMethodId);

                return Ok(new
                {
                    success = true,
                    message = "Thanh toán thành công",
                    data = new
                    {
                        orderId = response.OrderId,
                        amount = response.Amount,
                        fullName = response.FullName,
                        orderInfo = response.OrderInfo,
                        paymentDate = response.PaymentDate,
                        resultCode = response.ResultCode,
                        paymentMethod = momoPaymentMethod.MethodName
                    }
                });
            }

            return Ok(new
            {
                success = false,
                message = response.Message ?? "Thanh toán thất bại",
                data = new
                {
                    orderId = response.OrderId,
                    resultCode = response.ResultCode
                }
            });
        }

        // =============================
        // 3️⃣ Notify từ MoMo (Webhook)
        // =============================
        [HttpGet("MomoNotify")]
        public async Task<IActionResult> MomoNotify()
        {
            var response = _momoService.PaymentExecuteAsync(HttpContext.Request.Query);

            if (response == null)
            {
                return Ok("Received");
            }

            var resultCode = HttpContext.Request.Query["resultCode"].ToString();

            if (resultCode == "0") // Thanh toán thành công thật
            {
                var momoPaymentMethod = _context.PaymentMethods
                    .FirstOrDefault(p => p.MethodCode == "momo" && p.IsActive);

                if (momoPaymentMethod != null)
                {
                    response.IsSuccess = true;
                    response.PaymentDate = DateTime.UtcNow;
                    await SaveMomoInfoAsync(response, momoPaymentMethod.PaymentMethodId);
                }
            }

            return Ok("Received");
        }

        // =============================
        // 4️⃣ Lịch sử thanh toán
        // =============================
        [HttpGet("PaymentHistory")]
        public IActionResult GetPaymentHistory()
        {
            try
            {
                var history = _context.MomoInfos
                    .Join(_context.PaymentMethods,
                        m => m.PaymentMethodId,
                        p => p.PaymentMethodId,
                        (m, p) => new
                        {
                            id = m.Id,
                            orderId = m.OrderId,
                            fullName = m.Fullname,
                            amount = m.Amount,
                            orderInfo = m.OrderInfo,
                            datePaid = m.DatePaid,
                            paymentMethod = p.MethodName,
                            paymentMethodCode = p.MethodCode
                        })
                    .OrderByDescending(x => x.datePaid)
                    .ToList();

                return Ok(new
                {
                    success = true,
                    data = history
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lấy lịch sử thanh toán",
                    error = ex.Message
                });
            }
        }

        // =============================
        // 🔹 Lưu thông tin MoMo vào DB
        // =============================
        private async Task SaveMomoInfoAsync(Backend.Models.Momo.MomoExecuteResponseModel response, int paymentMethodId)
        {
            try
            {
                var momoInfo = new MomoInfo
                {
                    OrderId = response.OrderId,
                    OrderInfo = response.OrderInfo,
                    Amount = decimal.TryParse(response.Amount, out var amount) ? amount : 0,
                    DatePaid = response.PaymentDate?.ToUniversalTime(),
                    Fullname = response.FullName,
                    PaymentMethodId = paymentMethodId
                };

                _context.MomoInfos.Add(momoInfo);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving MomoInfo: {ex.Message}");
                throw;
            }
        }
    }
}
