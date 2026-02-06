using CoffeeShopApi.Models;

namespace CoffeeShopApi.Repositories;

public interface IProductRepository : IRepository<Product>
{
    // GET methods
    Task<Product?> GetByIdWithDetailsAsync(int id);
    Task<IEnumerable<Product>> GetAllWithDetailsAsync();
    Task<IEnumerable<Product>> GetByCategoryIdAsync(int categoryId);
    Task<(IEnumerable<Product> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, string? search, string? filter, string? orderBy);
    
    // CREATE method
    Task<Product> CreateWithOptionsAsync(Product product);
    
    // UPDATE method - nh?n danh sách OptionGroup template IDs ?? mapping
    Task<bool> UpdateWithOptionsAsync(Product product, ICollection<int>? optionGroupIds);

    // DELETE method (xóa c? ProductOptionGroups mappings)
    Task<bool> DeleteWithOptionsAsync(int id);
}

