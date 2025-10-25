using System.Collections.Generic;
using System.Threading.Tasks;
using YourShopManagement.API.Models;

namespace YourShopManagement.API.Repositories
{
    public interface IProductCategoryRepository
    {
        Task<IEnumerable<ProductCategory>> GetAllAsync();
        Task<ProductCategory?> GetByIdAsync(int id);
        Task<ProductCategory> AddAsync(ProductCategory category);
        Task UpdateAsync(ProductCategory category);
        Task DeleteAsync(int id);
        Task<IEnumerable<ProductCategory>> GetChildrenAsync(int parentId);
        Task<IEnumerable<ProductCategory>> SearchAsync(string keyword);
    }
}
