// Implementation of IPaymentMethodRepository using EF Core
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using YourShopManagement.API.Data;

namespace Backend.Repositories
{


    public class PaymentMethodRepository : IPaymentMethodRepository
    {
        private readonly ApplicationDbContext _context;

        public PaymentMethodRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PaymentMethod> GetByIdAsync(int paymentMethodId)
        {
            return await _context.PaymentMethods.FindAsync(paymentMethodId);
        }

        public async Task<IEnumerable<PaymentMethod>> GetAllAsync()
        {
            return await _context.PaymentMethods.ToListAsync();
        }

        public async Task<PaymentMethod> AddAsync(PaymentMethod paymentMethod)
        {
            _context.PaymentMethods.Add(paymentMethod);
            await _context.SaveChangesAsync();
            return paymentMethod;
        }

        public async Task UpdateAsync(PaymentMethod paymentMethod)
        {
            _context.PaymentMethods.Update(paymentMethod);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int paymentMethodId)
        {
            var method = await _context.PaymentMethods.FindAsync(paymentMethodId);
            if (method != null)
            {
                _context.PaymentMethods.Remove(method);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<PaymentMethod>> SearchAsync(string keyword)
        {
            return await _context.PaymentMethods
                .Where(pm => pm.MethodName.Contains(keyword) || pm.MethodCode.Contains(keyword))
                .ToListAsync();
        }

        public async Task<IEnumerable<PaymentMethod>> GetActiveAsync()
        {
            return await _context.PaymentMethods
                .Where(pm => pm.IsActive)
                .ToListAsync();
        }
    }


}
