using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DatabaseFixController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public DatabaseFixController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Fix Notifications table data types (EntityId và UserId từ VARCHAR → INTEGER)
        /// CHỈ CHẠY MỘT LẦN!
        /// </summary>
        [HttpPost("fix-notifications-table")]
        public async Task<IActionResult> FixNotificationsTable()
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                
                await using var connection = new NpgsqlConnection(connectionString);
                await connection.OpenAsync();

                // Xóa dữ liệu cũ
                await using (var cmd = new NpgsqlCommand(@"TRUNCATE TABLE ""Notifications""", connection))
                {
                    await cmd.ExecuteNonQueryAsync();
                }

                // Fix EntityId
                await using (var cmd = new NpgsqlCommand(@"
                    ALTER TABLE ""Notifications"" 
                    ALTER COLUMN ""EntityId"" TYPE INTEGER USING (""EntityId""::INTEGER)", connection))
                {
                    await cmd.ExecuteNonQueryAsync();
                }

                // Fix UserId
                await using (var cmd = new NpgsqlCommand(@"
                    ALTER TABLE ""Notifications"" 
                    ALTER COLUMN ""UserId"" TYPE INTEGER USING (""UserId""::INTEGER)", connection))
                {
                    await cmd.ExecuteNonQueryAsync();
                }

                return Ok(new
                {
                    success = true,
                    message = "Đã sửa kiểu dữ liệu của bảng Notifications thành công!"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Lỗi: {ex.Message}"
                });
            }
        }
    }
}
