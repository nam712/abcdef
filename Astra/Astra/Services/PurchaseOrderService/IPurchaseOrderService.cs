using YourShopManagement.API.DTOs.PurchaseOrder;

namespace YourShopManagement.API.Services
{
    public interface IPurchaseOrderService
    {
        Task<(bool Success, string Message, PurchaseOrderDto? Data)> CreateAsync(CreatePurchaseOrderDto dto);
        Task<IEnumerable<PurchaseOrderDto>> GetAllAsync();
        Task<PurchaseOrderDto?> GetByIdAsync(int id);
        Task<(bool Success, string Message)> DeleteAsync(int id);
    }
}