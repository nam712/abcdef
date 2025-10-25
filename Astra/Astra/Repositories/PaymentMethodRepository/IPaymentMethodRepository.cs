using Backend.Models;
// Interface for PaymentMethod repository with CRUD and search methods
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend.Repositories
{

    public interface IPaymentMethodRepository
    {
        Task<PaymentMethod> GetByIdAsync(int paymentMethodId);
        Task<IEnumerable<PaymentMethod>> GetAllAsync();
        Task<PaymentMethod> AddAsync(PaymentMethod paymentMethod);
        Task UpdateAsync(PaymentMethod paymentMethod);
        Task DeleteAsync(int paymentMethodId);

        Task<IEnumerable<PaymentMethod>> SearchAsync(string keyword);
        Task<IEnumerable<PaymentMethod>> GetActiveAsync();
    }


}
