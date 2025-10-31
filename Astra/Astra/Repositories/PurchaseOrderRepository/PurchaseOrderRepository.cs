using Microsoft.EntityFrameworkCore;
using YourShopManagement.API.Data;
using YourShopManagement.API.Models;

namespace YourShopManagement.API.Repositories
{
    public class PurchaseOrderRepository : IPurchaseOrderRepository
    {
        private readonly ApplicationDbContext _context;
        public PurchaseOrderRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PurchaseOrder>> GetAllAsync()
        {
            return await _context.PurchaseOrders
                .Include(p => p.Supplier)
                .Include(p => p.PurchaseOrderDetails)
                .ThenInclude(d => d.Product)
                .ToListAsync();
        }

        public async Task<PurchaseOrder?> GetByIdAsync(int id)
        {
            return await _context.PurchaseOrders
                .Include(p => p.Supplier)
                .Include(p => p.PurchaseOrderDetails)
                .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(p => p.PurchaseOrderId == id);
        }

        public async Task AddAsync(PurchaseOrder entity)
        {
            await _context.PurchaseOrders.AddAsync(entity);
        }

        public async Task DeleteAsync(PurchaseOrder entity)
        {
            _context.PurchaseOrders.Remove(entity);
            await Task.CompletedTask;
        }

        public async Task<bool> ExistsByCodeAsync(string poCode)
        {
            return await _context.PurchaseOrders.AnyAsync(p => p.PoCode == poCode);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}