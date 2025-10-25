namespace Backend.Models.Order
{
    public class OrderInfo
    {
        public string FullName { get; set; }
        public string? OrderId { get; set; } // Nullable vì sẽ được tạo tự động
        public string OrderInformation { get; set; }
        public string Amount { get; set; }
        public string PaymentMethodCode { get; set; }
        public int? PaymentMethodId { get; set; } // Thêm để lưu ID của payment method
    }
}
