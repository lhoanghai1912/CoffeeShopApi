using CoffeeShopApi.Data;
using CoffeeShopApi.DTOs;
using CoffeeShopApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShopApi.Services;

public interface ICategoryservice
{
    CategoryResponse? ToCategoryResponse(Category? Category);
    
    Task<IEnumerable<CategoryResponse>> GetAllAsync();
    Task<CategoryResponse?> GetByIdAsync(int id);
    Task<CategoryResponse> CreateAsync(CreateCategoryRequest request);
    Task<bool> UpdateAsync(int id, UpdateCategoryRequest request);
    Task<bool> DeleteAsync(int id);
    // Task<PagedResult<CategoryResponse>> GetPagedAsync(int page, int pageSize,string? search, string? orderBy, string? filter);

}



public class CategoriesService : ICategoryservice
{
    private readonly AppDbContext _context;

    public CategoriesService(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<CategoryResponse>> GetAllAsync()
    {
        var Categories = await _context.Categories.ToListAsync();
        return Categories.Select(MapToResponse);
    }

    // public async Task<PagedResult<CategoryResponse>> GetPagedAsync(int page, int pageSize, string? search, string? orderBy, string? filter)
    // {
    //     var query = _context.Categories.AsQueryable();
    //
    //     // search
    //     if (!string.IsNullOrWhiteSpace(search))
    //     {
    //         query = query.SearchContains(search, "Name");
    //     }
    //
    //     // filters
    //     if (!string.IsNullOrWhiteSpace(filter))
    //     {
    //         query = query.ApplyFilters(filter);
    //     }
    //
    //     // ordering
    //     if (!string.IsNullOrWhiteSpace(orderBy))
    //     {
    //         query = query.ApplyOrdering(orderBy);
    //     }
    //
    //     return await query.ToPagedResultAsync(page, pageSize, MapToResponse);
    // }
    //
    public CategoryResponse? ToCategoryResponse(Category? Category)
    {
        if (Category == null) return null;
        return new CategoryResponse
        {
            Id = Category.Id,
            Name = Category.Name
        };
    }

    private static CategoryResponse MapToResponse(Category Category)
    {
        return new CategoryResponse
        {
            Id = Category.Id,
            Name = Category.Name
        };
    }

    public async Task<CategoryResponse?> GetByIdAsync(int id)
    {
        var Category = await _context.Categories.FindAsync(id);
        return Category == null ? null : MapToResponse(Category);
    }

    public async Task<CategoryResponse> CreateAsync(CreateCategoryRequest request)
    {
        var Category = new Category
        {
            Name = request.Name
        };
        _context.Categories.Add(Category);
        await _context.SaveChangesAsync();
        return MapToResponse(Category);
    }

    public async Task<bool> UpdateAsync(int id, UpdateCategoryRequest request)
    {
        var Category = await _context.Categories.FindAsync(id);
        if (Category == null) return false;
        Category.Name = request.Name;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var Category = await _context.Categories.FindAsync(id);
        if (Category == null) return false;
        _context.Categories.Remove(Category);
        await _context.SaveChangesAsync();
        return true;
    }

    // public async Task<PagedResult<CategoryResponse>> GetPagedAsync(int page, int pageSize, string? search, string? orderBy, string? filter)
    // {
    //     var query = _context.Categories.AsQueryable();
    //     if (!string.IsNullOrEmpty(filter))
    //         query = query.Where(r => r.Name.Contains(filter) );
    //
    //     if (!string.IsNullOrEmpty(orderBy))
    //     {
    //         if (orderBy.Equals("name", StringComparison.OrdinalIgnoreCase))
    //             query = query.OrderBy(r => r.Name);
    //     }
    //
    //     var total = await query.CountAsync();
    //     var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
    //
    //     return new PagedResult<CategoryResponse>
    //     {
    //         Page = page,
    //         PageSize = pageSize,
    //         TotalRecords = total,
    //         Items = items.Select(MapToResponse)
    //     };
    // }
}
