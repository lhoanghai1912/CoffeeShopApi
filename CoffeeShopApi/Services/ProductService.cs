using CoffeeShopApi.Data;
using CoffeeShopApi.DTOs;
using CoffeeShopApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShopApi.Services;

public interface IProductService
{
    Task<IEnumerable<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(int id);
    Task<Product> CreateAsync(CreateProductRequest request);
    Task<bool> UpdateAsync(int id, UpdateProductRequest request);
    Task<bool> DeleteAsync(int id);
}

public class ProductService : IProductService
{
    private readonly AppDbContext _context;

    public ProductService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        // QUAN TRỌNG: Phải dùng Include để load kèm bảng con ProductDetails
        return await _context.Products
            .Include(p => p.ProductDetails) 
            .ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products
            .Include(p => p.ProductDetails)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Product> CreateAsync(CreateProductRequest request)
    {
        // 1. Tạo Product cha
        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            ImageUrl = request.ImageUrl,
            Category = request.Category,
            // 2. Map danh sách Size/Giá vào bảng con
            ProductDetails = request.Details.Select(d => new ProductDetail
            {
                Size = d.Size,
                Price = d.Price,
            }).ToList()
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task<bool> UpdateAsync(int id, UpdateProductRequest request)
    {
        // Load sản phẩm cũ kèm theo chi tiết cũ
        var product = await _context.Products
            .Include(p => p.ProductDetails)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null) return false;

        // 1. Cập nhật thông tin chung (Cha)
        product.Name = request.Name;
        product.Description = request.Description;
        product.Category = request.Category;
        product.ImageUrl = request.ImageUrl;

        // 2. Cập nhật chi tiết (Con)
        // Cách đơn giản nhất: Xóa hết chi tiết cũ, thêm chi tiết mới từ request
        // (Lưu ý: Cách này sẽ làm thay đổi ID của ProductDetails, cẩn thận nếu có OrderDetail đang reference tới nó)
        
        _context.ProductDetails.RemoveRange(product.ProductDetails); // Xóa cũ
        
        foreach (var item in request.Details)
        {
            product.ProductDetails.Add(new ProductDetail // Thêm mới
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

        // Vì đã cấu hình OnDelete(DeleteBehavior.Cascade) trong DbContext
        // Nên chỉ cần xóa cha, EF Core sẽ tự xóa con
        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return true;
    }
}