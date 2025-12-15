using CoffeeShopApi.Data;
using CoffeeShopApi.DTOs;
using CoffeeShopApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShopApi.Services;

public interface IProductService
{
    Task<IEnumerable<ProductResponse>> GetAllAsync();
    Task<ProductResponse?> GetByIdAsync(int id);
    Task<ProductResponse> CreateAsync(CreateProductRequest request);
    Task<bool> UpdateAsync(int id, UpdateProductRequest request);
    Task<bool> DeleteAsync(int id);
    Task<PagedResult<ProductResponse>> GetPagedAsync(int page, int pageSize,string? search, string? orderBy, string? filter);
}

public class ProductService : IProductService
{
    private readonly AppDbContext _context;

    public ProductService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProductResponse>> GetAllAsync()
    {
        var products = await _context.Products
            .Include(p => p.ProductDetails)
            .ToListAsync();

        return products.Select(MapToResponse);
    }
    public async Task<ProductResponse?> GetByIdAsync(int id)
    {
        var product = await _context.Products
            .Include(p => p.ProductDetails)
            .FirstOrDefaultAsync(p => p.Id == id);

        return product == null ? null : MapToResponse(product);
    }

    public async Task<ProductResponse> CreateAsync(CreateProductRequest request)
    {
        // 1. Tạo Product cha
        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            ImageUrl = request.ImageUrl,
            Category = request.Category,
            // 2. Map danh sách Size/Giá vào bảng con
            ProductDetails = request.ProductDetails.Select(d => new ProductDetail
            {
                Size = d.Size,
                Price = d.Price,
            }).ToList()
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return MapToResponse(product);
    }


    public async Task<bool> UpdateAsync(int id, UpdateProductRequest request)
    {
        var product = await _context.Products
            .Include(p => p.ProductDetails)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null) return false;

        product.Name = request.Name;
        product.Description = request.Description;
        product.Category = request.Category;
        product.ImageUrl = request.ImageUrl;

        _context.ProductDetails.RemoveRange(product.ProductDetails);

        foreach (var item in request.ProductDetails)
        {
            product.ProductDetails.Add(new ProductDetail
            {
                Size = item.Size,
                Price = item.Price,
            });
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return false;
        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return true;
    }

    public Task<PagedResult<ProductResponse>> GetPagedAsync(int page, int pageSize, string? orderBy, string? filter)
    {
        throw new NotImplementedException();
    }

    public async Task<PagedResult<ProductResponse>> GetPagedAsync(
        int page = 1,
        int pageSize = 10,
        string? search = null,
        string? filter = null,
        string? orderBy = null)
    {
        var query = _context.Products.Include(p => p.ProductDetails).AsQueryable();

        // Search theo tên hoặc mô tả
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(p => p.Name.Contains(search) || p.Description.Contains(search));
        }

        // Filter theo category
        if (!string.IsNullOrEmpty(filter))
        {
            query = query.Where(p => p.Category.ToLower() == filter.ToLower());
        }

        // Sắp xếp động
        if (!string.IsNullOrEmpty(orderBy))
        {
            var parts = orderBy.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var field = parts[0].ToLower();
            var isDesc = parts.Length > 1 && parts[1].ToLower() == "desc";

            switch (field)
            {
                case "id":
                    query = isDesc ? query.OrderByDescending(p => p.Id) : query.OrderBy(p => p.Id);
                    break;
                case "name":
                    query = isDesc ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name);
                    break;
                case "category":
                    query = isDesc ? query.OrderByDescending(p => p.Category) : query.OrderBy(p => p.Category);
                    break;
                default:
                    query = query.OrderBy(p => p.Id);
                    break;
            }
        }
        else
        {
            query = query.OrderBy(p => p.Id);
        }

        var totalRecords = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<ProductResponse>
        {
            Page = page,
            PageSize = pageSize,
            Items = items.Select(MapToResponse)
        };
    }
    private static ProductResponse MapToResponse(Product product)
    {
        return new ProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            ImageUrl = product.ImageUrl,
            Category = product.Category,
            ProductDetails = product.ProductDetails.Select(d => new ProductDetailResponse
            {
                Id = d.Id,
                Size = d.Size,
                Price = d.Price
            }).ToList()
        };
    }
}