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
            .Include(p => p.ProductOptionGroups.OrderBy(pog => pog.DisplayOrder))
                .ThenInclude(pog => pog.OptionGroup)
                    .ThenInclude(og => og!.OptionItems.OrderBy(oi => oi.DisplayOrder))
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Product>> GetAllWithDetailsAsync()
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.ProductOptionGroups.OrderBy(pog => pog.DisplayOrder))
                .ThenInclude(pog => pog.OptionGroup)
                    .ThenInclude(og => og!.OptionItems.OrderBy(oi => oi.DisplayOrder))
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetByCategoryIdAsync(int categoryId)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.ProductOptionGroups.OrderBy(pog => pog.DisplayOrder))
                .ThenInclude(pog => pog.OptionGroup)
                    .ThenInclude(og => og!.OptionItems.OrderBy(oi => oi.DisplayOrder))
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
            .Include(p => p.ProductOptionGroups.OrderBy(pog => pog.DisplayOrder))
                .ThenInclude(pog => pog.OptionGroup)
                    .ThenInclude(og => og!.OptionItems.OrderBy(oi => oi.DisplayOrder))
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

    /// <summary>
    /// C?p nh?t product và mapping v?i OptionGroup templates
    /// </summary>
    public async Task<bool> UpdateWithOptionsAsync(Product product, ICollection<int>? optionGroupIds)
    {
        // Xóa mappings c?
        if (product.ProductOptionGroups.Any())
        {
            _context.ProductOptionGroups.RemoveRange(product.ProductOptionGroups);
        }

        // Thêm mappings m?i
        if (optionGroupIds != null && optionGroupIds.Any())
        {
            int displayOrder = 1;
            foreach (var groupId in optionGroupIds)
            {
                product.ProductOptionGroups.Add(new ProductOptionGroup
                {
                    ProductId = product.Id,
                    OptionGroupId = groupId,
                    DisplayOrder = displayOrder++
                });
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

        // Xóa ProductOptionGroups mappings (cascade s? lo)
        if (product.ProductOptionGroups.Any())
        {
            _context.ProductOptionGroups.RemoveRange(product.ProductOptionGroups);
        }

        _dbSet.Remove(product);
        await _context.SaveChangesAsync();
        return true;
    }

    #endregion

}
