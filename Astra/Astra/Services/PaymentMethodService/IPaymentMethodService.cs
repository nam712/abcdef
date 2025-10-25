using Backend.DTOs;
// Interface for PaymentMethod service with CRUD and search methods
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Backend.Services
{

    public interface IPaymentMethodService
    {
        Task<PaymentMethodDto> GetByIdAsync(int paymentMethodId);
        Task<IEnumerable<PaymentMethodDto>> GetAllAsync();
        Task<PaymentMethodDto> AddAsync(PaymentMethodDto paymentMethodDto);
        Task UpdateAsync(PaymentMethodDto paymentMethodDto);
        Task DeleteAsync(int paymentMethodId);

        Task<IEnumerable<PaymentMethodDto>> SearchAsync(string keyword);
        Task<IEnumerable<PaymentMethodDto>> GetActiveAsync();
    }


}
