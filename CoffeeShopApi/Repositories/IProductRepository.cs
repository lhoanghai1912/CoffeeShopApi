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
    
    // UPDATE method
    Task<bool> UpdateWithOptionsAsync(Product product, ICollection<OptionGroup>? newOptionGroups);
    
    // DELETE method (override ?? xóa c? OptionGroups)
    Task<bool> DeleteWithOptionsAsync(int id);
}

