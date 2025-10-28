// Data Transfer Object for PaymentMethod
using System;

namespace Backend.DTOs
{
    public class PaymentMethodDto
    {
        public int PaymentMethodId { get; set; }
        public string MethodName { get; set; }
        public string MethodCode { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }


}
