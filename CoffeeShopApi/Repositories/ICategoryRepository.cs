using CoffeeShopApi.Models;

namespace CoffeeShopApi.Repositories;

public interface ICategoryRepository : IRepository<Category>
{
    Task<Category?> GetByNameAsync(string name);
    Task<bool> HasProductsAsync(int categoryId);
}
