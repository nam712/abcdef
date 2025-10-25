namespace ProductCategoryApi.DTOs
{
    public class ProductCategoryDto
    {
        public int CategoryId { get; set; }

        public string CategoryName { get; set; } = default!;

        public string? Description { get; set; }

        public int? ParentCategoryId { get; set; }

        public string Status { get; set; } = "active";

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }

    public class ProductCategoryCreateDto
    {
        public string CategoryName { get; set; } = default!;

        public string? Description { get; set; }

        public int? ParentCategoryId { get; set; }

        public string? Status { get; set; } = "active";
    }
}
