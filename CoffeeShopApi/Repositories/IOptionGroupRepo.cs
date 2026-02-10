using CoffeeShopApi.Models;

namespace CoffeeShopApi.Repositories;

public interface IOptionGroupRepository
{
    Task<IEnumerable<OptionGroup>> GetAllAsync();
    Task<OptionGroup?> GetByIdAsync(int id);
    Task<OptionGroup> CreateAsync(OptionGroup optionGroup);
    Task<bool> UpdateAsync(OptionGroup optionGroup);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}
