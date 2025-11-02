using Backend.DTOs.Notification;
using Backend.Services.NotificationService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Roles = "ShopOwner")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        /// <summary>
        /// Get all notifications with optional filters
        /// </summary>
        /// <param name="userId">Optional user ID to filter notifications</param>
        /// <param name="isRead">Optional filter by read status</param>
        /// <param name="limit">Maximum number of notifications to return (default: 50)</param>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? userId = null, [FromQuery] bool? isRead = null, [FromQuery] int limit = 50)
        {
            try
            {
                var notifications = await _notificationService.GetAllNotificationsAsync(userId, isRead, limit);
                return Ok(new
                {
                    success = true,
                    data = notifications,
                    message = "Lấy danh sách thông báo thành công"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Lỗi khi lấy danh sách thông báo: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Get notification by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var notification = await _notificationService.GetNotificationByIdAsync(id);
                if (notification == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy thông báo"
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = notification,
                    message = "Lấy thông báo thành công"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Lỗi khi lấy thông báo: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Create a new notification
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateNotificationDto dto)
        {
            try
            {
                var notification = await _notificationService.CreateNotificationAsync(dto);
                return Ok(new
                {
                    success = true,
                    data = notification,
                    message = "Tạo thông báo thành công"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Lỗi khi tạo thông báo: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Update notification (mainly for marking as read)
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateNotificationDto dto)
        {
            try
            {
                var notification = await _notificationService.UpdateNotificationAsync(id, dto);
                if (notification == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy thông báo"
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = notification,
                    message = "Cập nhật thông báo thành công"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Lỗi khi cập nhật thông báo: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Delete a notification
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _notificationService.DeleteNotificationAsync(id);
                if (!result)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy thông báo"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Xóa thông báo thành công"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Lỗi khi xóa thông báo: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Get notification statistics
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats([FromQuery] int? userId = null)
        {
            try
            {
                var stats = await _notificationService.GetNotificationStatsAsync(userId);
                return Ok(new
                {
                    success = true,
                    data = stats,
                    message = "Lấy thống kê thành công"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Lỗi khi lấy thống kê: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Mark a notification as read
        /// </summary>
        [HttpPatch("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            try
            {
                var result = await _notificationService.MarkAsReadAsync(id);
                if (!result)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy thông báo"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Đã đánh dấu thông báo là đã đọc"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Lỗi khi đánh dấu đã đọc: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Mark all notifications as read
        /// </summary>
        [HttpPatch("read-all")]
        public async Task<IActionResult> MarkAllAsRead([FromQuery] int? userId = null)
        {
            try
            {
                await _notificationService.MarkAllAsReadAsync(userId);
                return Ok(new
                {
                    success = true,
                    message = "Đã đánh dấu tất cả thông báo là đã đọc"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Lỗi khi đánh dấu tất cả đã đọc: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Delete all notifications
        /// </summary>
        [HttpDelete("all")]
        public async Task<IActionResult> DeleteAll([FromQuery] int? userId = null)
        {
            try
            {
                await _notificationService.DeleteAllNotificationsAsync(userId);
                return Ok(new
                {
                    success = true,
                    message = "Đã xóa tất cả thông báo"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Lỗi khi xóa tất cả thông báo: {ex.Message}"
                });
            }
        }
    }
}
