using ProductCategoryApi.DTOs;

namespace Services
{
    public interface IProductCategoryService
    {
        Task<IEnumerable<ProductCategoryDto>> GetAllAsync();
        Task<ProductCategoryDto?> GetByIdAsync(int id);
        Task<ProductCategoryDto> CreateAsync(ProductCategoryCreateDto dto);
        Task<ProductCategoryDto?> UpdateAsync(int id, ProductCategoryCreateDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
