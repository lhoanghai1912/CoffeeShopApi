using CoffeeShopApi.Data.Configuration;
using CoffeeShopApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShopApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    
    // Option System (thay the ProductDetail)
    public DbSet<OptionGroup> OptionGroups { get; set; }
    public DbSet<OptionItem> OptionItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations
        modelBuilder.ApplyConfiguration(new ProductConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new RoleConfiguration());
        modelBuilder.ApplyConfiguration(new CategoryConfiguration());
        modelBuilder.ApplyConfiguration(new PermissionConfiguration());
        modelBuilder.ApplyConfiguration(new RolePermissionConfiguration());
        modelBuilder.ApplyConfiguration(new OptionGroupConfiguration());
        modelBuilder.ApplyConfiguration(new OptionItemConfiguration());

        // 1. Seed Roles
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Code = "ADMIN", Name = "Admin" },
            new Role { Id = 2, Code = "CUSTOMER", Name = "Khách hàng" },
            new Role { Id = 3, Code = "STAFF", Name = "Nhân viên" }
        );

        // 2. Seed Permissions
        modelBuilder.Entity<Permission>().HasData(
            // Product permissions
            new Permission { Id = 1, Code = "product.view", Name = "Xem sản phẩm", Module = "Product" },
            new Permission { Id = 2, Code = "product.create", Name = "Tạo sản phẩm", Module = "Product" },
            new Permission { Id = 3, Code = "product.update", Name = "Sửa sản phẩm", Module = "Product" },
            new Permission { Id = 4, Code = "product.delete", Name = "Xóa sản phẩm", Module = "Product" },
            
            // Category permissions
            new Permission { Id = 5, Code = "category.view", Name = "Xem danh mục", Module = "Category" },
            new Permission { Id = 6, Code = "category.create", Name = "Tạo danh mục", Module = "Category" },
            new Permission { Id = 7, Code = "category.update", Name = "Sửa danh mục", Module = "Category" },
            new Permission { Id = 8, Code = "category.delete", Name = "Xóa danh mục", Module = "Category" },
            
            // Order permissions
            new Permission { Id = 9, Code = "order.view.own", Name = "Xem đơn hàng của mình", Module = "Order" },
            new Permission { Id = 10, Code = "order.view.all", Name = "Xem tất cả đơn hàng", Module = "Order" },
            new Permission { Id = 11, Code = "order.create", Name = "Tạo đơn hàng", Module = "Order" },
            new Permission { Id = 12, Code = "order.update.own", Name = "Sửa đơn hàng của mình", Module = "Order" },
            new Permission { Id = 13, Code = "order.update.all", Name = "Sửa tất cả đơn hàng", Module = "Order" },
            new Permission { Id = 14, Code = "order.cancel.own", Name = "Hủy đơn của mình", Module = "Order" },
            new Permission { Id = 15, Code = "order.cancel.all", Name = "Hủy bất kỳ đơn nào", Module = "Order" },
            
            // User permissions
            new Permission { Id = 16, Code = "user.view.own", Name = "Xem thông tin cá nhân", Module = "User" },
            new Permission { Id = 17, Code = "user.view.all", Name = "Xem tất cả user", Module = "User" },
            new Permission { Id = 18, Code = "user.update.own", Name = "Sửa thông tin cá nhân", Module = "User" },
            new Permission { Id = 19, Code = "user.update.all", Name = "Sửa thông tin user khác", Module = "User" },
            new Permission { Id = 20, Code = "user.delete", Name = "Xóa user", Module = "User" },
            
            // Role & Permission management
            new Permission { Id = 21, Code = "role.manage", Name = "Quản lý role", Module = "System" },
            new Permission { Id = 22, Code = "permission.assign", Name = "Phân quyền", Module = "System" }
        );

        // 3. Seed RolePermissions
        // ADMIN - Có tất cả quyền
        for (int i = 1; i <= 22; i++)
        {
            modelBuilder.Entity<RolePermission>().HasData(
                new { RoleId = 1, PermissionId = i }
            );
        }

        // CUSTOMER - Chỉ có quyền xem và tạo order của mình
        modelBuilder.Entity<RolePermission>().HasData(
            new { RoleId = 3, PermissionId = 1 },  // product.view
            new { RoleId = 3, PermissionId = 5 },  // category.view
            new { RoleId = 3, PermissionId = 9 },  // order.view.own
            new { RoleId = 3, PermissionId = 11 }, // order.create
            new { RoleId = 3, PermissionId = 14 }, // order.cancel.own
            new { RoleId = 3, PermissionId = 16 }, // user.view.own
            new { RoleId = 3, PermissionId = 18 }  // user.update.own
        );

        // STAFF - Có quyền quản lý product, category, xem tất cả order
        modelBuilder.Entity<RolePermission>().HasData(
            new { RoleId = 2, PermissionId = 1 },  // product.view
            new { RoleId = 2, PermissionId = 2 },  // product.create
            new { RoleId = 2, PermissionId = 3 },  // product.update
            new { RoleId = 2, PermissionId = 5 },  // category.view
            new { RoleId = 2, PermissionId = 6 },  // category.create
            new { RoleId = 2, PermissionId = 7 },  // category.update
            new { RoleId = 2, PermissionId = 10 }, // order.view.all
            new { RoleId = 2, PermissionId = 13 }, // order.update.all
            new { RoleId = 2, PermissionId = 16 }, // user.view.own
            new { RoleId = 2, PermissionId = 18 }  // user.update.own
        );

        // 4. Seed Categories
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Coffee" },
            new Category { Id = 2, Name = "Tea" },
            new Category { Id = 3, Name = "Food" },
            new Category { Id = 4, Name = "Freeze" }
        );

        // 5. Seed Products - Chi seed thong tin co ban, OptionGroups se duoc seed qua ProductSeeder
        // (Khong seed o day de tranh conflict voi ProductSeeder)
    }
}