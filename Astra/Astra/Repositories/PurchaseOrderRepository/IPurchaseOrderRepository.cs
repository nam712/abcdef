using YourShopManagement.API.Models;

namespace YourShopManagement.API.Repositories
{
    public interface IPurchaseOrderRepository
    {
        Task<IEnumerable<PurchaseOrder>> GetAllAsync();
        Task<PurchaseOrder?> GetByIdAsync(int id);
        Task AddAsync(PurchaseOrder entity);
        Task DeleteAsync(PurchaseOrder entity);
        Task<bool> ExistsByCodeAsync(string poCode);
        Task SaveChangesAsync();
    }
}