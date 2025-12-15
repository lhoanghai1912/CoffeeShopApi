using CoffeeShopApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShopApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // 1. Cấu hình Role Code là duy nhất (Unique)
        modelBuilder.Entity<Role>()
            .HasIndex(r => r.Code)
            .IsUnique();

        // 2. Cấu hình User (Unique userName & Quan hệ với Role)
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Username).IsUnique();
            entity.HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // 2. Seed Data (Tạo sẵn 3 quyền c ơ bản)
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Code = "ADMIN", Name = "Admin" },
            new Role { Id = 2, Code = "CUSTOMER", Name = "Khách hàng" },
            new Role { Id = 3, Code = "STAFF", Name = "Nhân viên" }
        );

        // --- CẤU HÌNH MỚI CHO PRODUCT ---

        // Cấu hình quan hệ: 1 Product -> Nhiều ProductDetails
        // Khi xóa Product cha, tự động xóa hết các ProductDetail con (Cascade)
        modelBuilder.Entity<Product>()
            .HasMany(p => p.ProductDetails)
            .WithOne(d => d.Product)
            .HasForeignKey(d => d.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // A. Tạo Sản phẩm cha trước (Lưu ý ID)
        modelBuilder.Entity<Product>().HasData(
            new Product 
            { 
                Id = 1, 
                Name = "Cà phê đen đá", 
                Description = "Cà phê Robusta đậm đặc hương vị truyền thống", 
                Category = "Coffee",
            },
            new Product 
            { 
                Id = 2, 
                Name = "Bạc xỉu", 
                Description = "Sữa nóng pha thêm chút cà phê, ngọt ngào", 
                Category = "Coffee",
            },
            new Product 
            { 
                Id = 3, 
                Name = "Bánh Croissant", 
                Description = "Bánh sừng bò ngàn lớp thơm bơ", 
                Category = "Food",
            }
        );

        // B. Tạo Chi tiết giá (ProductDetail) gắn với ID cha
        modelBuilder.Entity<ProductDetail>().HasData(
            // 1. Giá cho Cà phê đen (Id=1)
            new ProductDetail { Id = 1, ProductId = 1, Size = "S", Price = 20000 },
            new ProductDetail { Id = 2, ProductId = 1, Size = "M", Price = 25000 },
            new ProductDetail { Id = 3, ProductId = 1, Size = "L", Price = 30000 },

            // 2. Giá cho Bạc xỉu (Id=2)
            new ProductDetail { Id = 4, ProductId = 2, Size = "S", Price = 28000 },
            new ProductDetail { Id = 5, ProductId = 2, Size = "M", Price = 35000 },

            // 3. Giá cho Bánh Croissant (Id=3) - Bánh thường chỉ có 1 size chuẩn
            new ProductDetail { Id = 6, ProductId = 3, Size = "Standard", Price = 45000 }
        );
    }
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    
    // Khai báo cả 2 bảng sản phẩm
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductDetail> ProductDetails { get; set; }

}
