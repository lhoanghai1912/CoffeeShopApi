using CoffeeShopApi.Models;

namespace CoffeeShopApi.Repositories;

public interface IOptionItemRepository
{
    Task<IEnumerable<OptionItem>> GetAllAsync();
    Task<IEnumerable<OptionItem>> GetByOptionGroupIdAsync(int optionGroupId);
    Task<OptionItem?> GetByIdAsync(int id);
    Task<OptionItem> CreateAsync(OptionItem optionItem);
    Task<bool> UpdateAsync(OptionItem optionItem);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}
