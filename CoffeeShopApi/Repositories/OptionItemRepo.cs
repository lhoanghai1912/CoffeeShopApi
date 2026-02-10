using CoffeeShopApi.Data;
using CoffeeShopApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShopApi.Repositories;

public class OptionItemRepo : IOptionItemRepository
{
    private readonly AppDbContext _context;

    public OptionItemRepo(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<OptionItem>> GetAllAsync()
    {
        return await _context.OptionItems
            .Include(oi => oi.OptionGroup)
            .OrderBy(oi => oi.OptionGroupId)
            .ThenBy(oi => oi.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<OptionItem>> GetByOptionGroupIdAsync(int optionGroupId)
    {
        return await _context.OptionItems
            .Where(oi => oi.OptionGroupId == optionGroupId)
            .OrderBy(oi => oi.Name)
            .ToListAsync();
    }

    public async Task<OptionItem?> GetByIdAsync(int id)
    {
        return await _context.OptionItems
            .Include(oi => oi.OptionGroup)
            .FirstOrDefaultAsync(oi => oi.Id == id);
    }

    public async Task<OptionItem> CreateAsync(OptionItem optionItem)
    {
        _context.OptionItems.Add(optionItem);
        await _context.SaveChangesAsync();
        return optionItem;
    }

    public async Task<bool> UpdateAsync(OptionItem optionItem)
    {
        _context.OptionItems.Update(optionItem);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var optionItem = await _context.OptionItems.FindAsync(id);
        if (optionItem == null) return false;

        _context.OptionItems.Remove(optionItem);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.OptionItems.AnyAsync(oi => oi.Id == id);
    }
}
