namespace Backend.Models.Momo
{
    public class MomoExecuteResponseModel
    {
        public string OrderId { get; set; }
        public string Amount { get; set; }
        public string FullName { get; set; }
        public string OrderInfo { get; set; }
        public string ResultCode { get; set; }
        public string Message { get; set; }
        public bool IsSuccess { get; set; }
        public DateTime? PaymentDate { get; set; }
    }

}
