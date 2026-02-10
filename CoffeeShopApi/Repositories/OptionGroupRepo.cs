using CoffeeShopApi.Data;
using CoffeeShopApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShopApi.Repositories;

public class OptionGroupRepo : IOptionGroupRepository
{
    private readonly AppDbContext _context;

    public OptionGroupRepo(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<OptionGroup>> GetAllAsync()
    {
        return await _context.OptionGroups
            .Include(og => og.OptionItems)
            .OrderBy(og => og.Name)
            .ToListAsync();
    }

    public async Task<OptionGroup?> GetByIdAsync(int id)
    {
        return await _context.OptionGroups
            .Include(og => og.OptionItems)
            .FirstOrDefaultAsync(og => og.Id == id);
    }

    public async Task<OptionGroup> CreateAsync(OptionGroup optionGroup)
    {
        _context.OptionGroups.Add(optionGroup);
        await _context.SaveChangesAsync();
        return optionGroup;
    }

    public async Task<bool> UpdateAsync(OptionGroup optionGroup)
    {
        _context.OptionGroups.Update(optionGroup);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var optionGroup = await _context.OptionGroups.FindAsync(id);
        if (optionGroup == null) return false;

        _context.OptionGroups.Remove(optionGroup);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.OptionGroups.AnyAsync(og => og.Id == id);
    }
}
