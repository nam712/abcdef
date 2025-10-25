using Backend.Models.Momo;
using Backend.Models.Order;

namespace Backend.Services.Momo
{
    public interface IMomoService
    {
        Task<MomoCreatePaymentResponseModel> CreatePaymentMomo(OrderInfo orderInfo);
        MomoExecuteResponseModel PaymentExecuteAsync(IQueryCollection collection); 
    }
}
