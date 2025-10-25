namespace ProductApi.DTOs
{
    public class ProductDto
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; } = default!;
        public string ProductName { get; set; } = default!;
        public string? Description { get; set; }
        public int? CategoryId { get; set; }
        public string? Brand { get; set; }
        public int? SupplierId { get; set; }
        public decimal Price { get; set; }
        public decimal? CostPrice { get; set; }
        public int Stock { get; set; }
        public int? MinStock { get; set; }
        public string? Sku { get; set; }
        public string? Barcode { get; set; }
        public string? Unit { get; set; }
        public string? ImageUrl { get; set; }
        public string? Status { get; set; }
        public decimal? Weight { get; set; }
        public string? Dimension { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class ProductCreateDto
    {
        public string ProductCode { get; set; } = default!;
        public string ProductName { get; set; } = default!;
        public string? Description { get; set; }
        public int? CategoryId { get; set; }
        public string? Brand { get; set; }
        public int? SupplierId { get; set; }
        public decimal Price { get; set; }
        public decimal? CostPrice { get; set; }
        public int Stock { get; set; } = 0;
        public int? MinStock { get; set; }
        public string? Sku { get; set; }
        public string? Barcode { get; set; }
        public string? Unit { get; set; }
        public string? ImageUrl { get; set; }
        public string? Notes { get; set; }
        public decimal? Weight { get; set; }
        public string? Dimension { get; set; }
    }
}
