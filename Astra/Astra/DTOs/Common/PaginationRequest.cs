using System.ComponentModel.DataAnnotations;

namespace YourShopManagement.API.DTOs.Common
{
    public class PaginationRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "Số trang phải lớn hơn 0")]
        public int Page { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "Kích thước trang phải từ 1 đến 100")]
        public int PageSize { get; set; } = 10;

        public string? Search { get; set; }
        public string? Segment { get; set; }
        public string? Status { get; set; }
    }
}
