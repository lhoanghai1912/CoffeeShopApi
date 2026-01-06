using CoffeeShopApi.Data;
using CoffeeShopApi.Models;
using Microsoft.EntityFrameworkCore;
using Gridify;

namespace CoffeeShopApi.Repositories;

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(AppDbContext context) : base(context)
    {
    }

    #region GET Methods

    public async Task<Product?> GetByIdWithDetailsAsync(int id)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.OptionGroups)
                .ThenInclude(og => og.OptionItems)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Product>> GetAllWithDetailsAsync()
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.OptionGroups)
                .ThenInclude(og => og.OptionItems)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetByCategoryIdAsync(int categoryId)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.OptionGroups)
                .ThenInclude(og => og.OptionItems)
            .Where(p => p.CategoryId == categoryId)
            .ToListAsync();
    }

    public async Task<(IEnumerable<Product> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? search, string? filter, string? orderBy)
    {
        page = Math.Max(1, page);
        pageSize = Math.Max(1, Math.Min(100, pageSize));

        var query = _dbSet.AsQueryable();

        // Search
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchValue = search.Trim().ToLower();
            query = query.Where(p => (p.Name != null && p.Name.ToLower().Contains(searchValue))
                                   || (p.Description != null && p.Description.ToLower().Contains(searchValue)));
        }

        // Gridify filtering and ordering
        var gridifyQuery = new GridifyQuery
        {
            Page = page,
            PageSize = pageSize,
            Filter = filter,
            OrderBy = orderBy
        };

        query = query.ApplyFiltering(gridifyQuery).ApplyOrdering(gridifyQuery);

        var totalCount = await query.CountAsync();

        var items = await query
            .ApplyPaging(gridifyQuery)
            .Include(p => p.Category)
            .Include(p => p.OptionGroups)
                .ThenInclude(og => og.OptionItems)
            .AsNoTracking()
            .ToListAsync();

        return (items, totalCount);
    }

    #endregion

    #region CREATE Method

    public async Task<Product> CreateWithOptionsAsync(Product product)
    {
        await _dbSet.AddAsync(product);
        await _context.SaveChangesAsync();
        return product;
    }

    #endregion

    #region UPDATE Method

    public async Task<bool> UpdateWithOptionsAsync(Product product, ICollection<OptionGroup>? newOptionGroups)
    {
        // Xóa OptionGroups c?
        if (product.OptionGroups.Any())
        {
            _context.OptionGroups.RemoveRange(product.OptionGroups);
        }

        // Thêm OptionGroups m?i
        if (newOptionGroups != null && newOptionGroups.Any())
        {
            foreach (var og in newOptionGroups)
            {
                og.ProductId = product.Id;
                product.OptionGroups.Add(og);
            }
        }

        product.UpdatedAt = DateTime.UtcNow;
        _dbSet.Update(product);
        await _context.SaveChangesAsync();
        return true;
    }

    #endregion

    #region DELETE Method

    public async Task<bool> DeleteWithOptionsAsync(int id)
    {
        var product = await GetByIdWithDetailsAsync(id);
        if (product == null) return false;

        // Xóa OptionGroups (cascade s? xóa OptionItems)
        if (product.OptionGroups.Any())
        {
            _context.OptionGroups.RemoveRange(product.OptionGroups);
        }

        _dbSet.Remove(product);
        await _context.SaveChangesAsync();
        return true;
    }

    #endregion

}
