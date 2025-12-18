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
    public DbSet<ProductDetail> ProductDetails { get; set; }
    public DbSet<Category> Categories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations
        modelBuilder.ApplyConfiguration(new ProductConfiguration());
        modelBuilder.ApplyConfiguration(new ProductDetailConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new RoleConfiguration());
        modelBuilder.ApplyConfiguration(new CategoryConfiguration());
        modelBuilder.ApplyConfiguration(new PermissionConfiguration());
        modelBuilder.ApplyConfiguration(new RolePermissionConfiguration());

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

        // 5. Seed Products (Cập nhật CategoryId chuẩn 18 món)
        modelBuilder.Entity<Product>().HasData(
            new Product
            {
                Id = 1, Name = "Cà phê đen đá", Description = "Cà phê Robusta đậm đặc hương vị truyền thống",
                CategoryId = 1
            },
            new Product
            {
                Id = 2, Name = "Bạc xỉu", Description = "Sữa nóng pha thêm chút cà phê, ngọt ngào", CategoryId = 1
            },
            new Product
            {
                Id = 3, Name = "Bánh abcd", Description = "Bánh sừng bò ngàn lớp thơm bơ", CategoryId = 3
            },
            new Product
            {
                Id = 4, Name = "Cà phê sữa đá", Description = "Hương vị cà phê Việt Nam truyền thống kết hợp sữa đặc",
                CategoryId = 1
            },
            new Product
            {
                Id = 5, Name = "Latte Nóng", Description = "Espresso nhẹ nhàng hòa quyện với lớp sữa tươi đánh nóng",
                CategoryId = 1
            },
            new Product
            {
                Id = 6, Name = "Cappuccino", Description = "Sự cân bằng hoàn hảo giữa Espresso, sữa nóng và bọt sữa",
                CategoryId = 1
            },
            new Product
            {
                Id = 7, Name = "Americano Đá", Description = "Espresso pha loãng với nước và đá, thanh nhẹ",
                CategoryId = 1
            },
            new Product
            {
                Id = 8, Name = "Caramel Macchiato", Description = "Sữa tươi, vani, Espresso và sốt Caramel ngọt ngào",
                CategoryId = 1
            },
            new Product
            {
                Id = 9, Name = "Trà Đào Cam Sả", Description = "Trà đen thơm lừng kết hợp đào ngâm và sả tươi",
                CategoryId = 2
            },
            new Product
            {
                Id = 10, Name = "Trà Sen Vàng", Description = "Trà ô long thanh mát kết hợp hạt sen bùi bùi",
                CategoryId = 2
            },
            new Product
            {
                Id = 11, Name = "Trà Vải Hoa Hồng", Description = "Vị ngọt ngào của vải thiều hòa quyện hương hoa hồng",
                CategoryId = 2
            },
            new Product
            {
                Id = 12, Name = "Matcha Đá Xay", Description = "Bột trà xanh Nhật Bản xay nhuyễn với sữa tươi",
                CategoryId = 4
            },
            new Product
            {
                Id = 13, Name = "Cookie Đá Xay", Description = "Bánh Oreo xay cùng sữa và đá, phủ lớp kem tươi",
                CategoryId = 4
            },
            new Product
            {
                Id = 14, Name = "Tiramisu", Description = "Bánh ngọt vị cà phê và rượu rum, lớp kem phô mai",
                CategoryId = 3
            },
            new Product
            {
                Id = 15, Name = "Cheesecake Chanh Dây", Description = "Bánh phô mai chua ngọt vị chanh dây",
                CategoryId = 3
            },
            new Product
            {
                Id = 16, Name = "Mousse Chocolate", Description = "Bánh mousse mềm mịn đậm đà vị sô cô la",
                CategoryId = 3
            },
            new Product
            {
                Id = 17, Name = "Bánh Mì Que Pate", Description = "Bánh mì que giòn rụm với nhân pate cay nồng",
                CategoryId = 3
            },
            new Product
            {
                Id = 18, Name = "Bánh Donut Socola", Description = "Bánh vòng chiên phủ lớp sốt socola đen",
                CategoryId = 3
            }
        );

        // 6. Seed ProductDetails (Cập nhật: Mỗi món ít nhất 2 size)
        modelBuilder.Entity<ProductDetail>().HasData(
            // --- Món 1: Đen đá (3 size) ---
            new ProductDetail { Id = 1, ProductId = 1, Size = "S", Price = 20000 },
            new ProductDetail { Id = 2, ProductId = 1, Size = "M", Price = 25000 },
            new ProductDetail { Id = 3, ProductId = 1, Size = "L", Price = 30000 },

            // --- Món 2: Bạc xỉu (2 size) ---
            new ProductDetail { Id = 4, ProductId = 2, Size = "S", Price = 28000 },
            new ProductDetail { Id = 5, ProductId = 2, Size = "M", Price = 35000 },

            // --- Món 3: Croissant (2 size - Mới thêm size S) ---
            new ProductDetail { Id = 6, ProductId = 3, Size = "M", Price = 45000 }, // Đổi từ Standard -> M
            new ProductDetail { Id = 7, ProductId = 3, Size = "S", Price = 35000 }, // Thêm size nhỏ

            // --- Món 4: Cà phê sữa đá (2 size: M, L) ---
            new ProductDetail { Id = 8, ProductId = 4, Size = "M", Price = 29000 },
            new ProductDetail { Id = 9, ProductId = 4, Size = "L", Price = 35000 },

            // --- Món 5: Latte (2 size: M, L) ---
            new ProductDetail { Id = 10, ProductId = 5, Size = "M", Price = 45000 },
            new ProductDetail { Id = 11, ProductId = 5, Size = "L", Price = 55000 },

            // --- Món 6: Cappuccino (2 size: M, L) ---
            new ProductDetail { Id = 12, ProductId = 6, Size = "M", Price = 45000 },
            new ProductDetail { Id = 13, ProductId = 6, Size = "L", Price = 55000 },

            // --- Món 7: Americano (2 size: M, L) ---
            new ProductDetail { Id = 14, ProductId = 7, Size = "M", Price = 35000 },
            new ProductDetail { Id = 15, ProductId = 7, Size = "L", Price = 41000 },

            // --- Món 8: Caramel Macchiato (2 size: M, L) ---
            new ProductDetail { Id = 16, ProductId = 8, Size = "M", Price = 50000 },
            new ProductDetail { Id = 17, ProductId = 8, Size = "L", Price = 60000 },

            // --- Món 9: Trà Đào (2 size: M, L) ---
            new ProductDetail { Id = 18, ProductId = 9, Size = "M", Price = 45000 },
            new ProductDetail { Id = 19, ProductId = 9, Size = "L", Price = 55000 },

            // --- Món 10: Trà Sen (2 size: M, L) ---
            new ProductDetail { Id = 20, ProductId = 10, Size = "M", Price = 45000 },
            new ProductDetail { Id = 21, ProductId = 10, Size = "L", Price = 55000 },

            // --- Món 11: Trà Vải (2 size: M, L) ---
            new ProductDetail { Id = 22, ProductId = 11, Size = "M", Price = 42000 },
            new ProductDetail { Id = 23, ProductId = 11, Size = "L", Price = 52000 },

            // --- Món 12: Matcha (2 size: M, L) ---
            new ProductDetail { Id = 24, ProductId = 12, Size = "M", Price = 55000 },
            new ProductDetail { Id = 25, ProductId = 12, Size = "L", Price = 65000 },

            // --- Món 13: Cookie (2 size: M, L) ---
            new ProductDetail { Id = 26, ProductId = 13, Size = "M", Price = 55000 },
            new ProductDetail { Id = 27, ProductId = 13, Size = "L", Price = 65000 },

            // --- Món 14: Tiramisu (2 size: S, M) ---
            new ProductDetail { Id = 28, ProductId = 14, Size = "M", Price = 39000 },
            new ProductDetail { Id = 29, ProductId = 14, Size = "S", Price = 29000 },

            // --- Món 15: Cheesecake (2 size: S, M) ---
            new ProductDetail { Id = 30, ProductId = 15, Size = "M", Price = 39000 },
            new ProductDetail { Id = 31, ProductId = 15, Size = "S", Price = 29000 },

            // --- Món 16: Mousse (2 size: S, M) ---
            new ProductDetail { Id = 32, ProductId = 16, Size = "M", Price = 35000 },
            new ProductDetail { Id = 33, ProductId = 16, Size = "S", Price = 25000 },

            // --- Món 17: Bánh Mì Que (2 size: 1 cái, Combo) ---
            new ProductDetail { Id = 34, ProductId = 17, Size = "1 cái", Price = 15000 },
            new ProductDetail { Id = 35, ProductId = 17, Size = "Combo 3 cái", Price = 39000 },

            // --- Món 18: Donut (2 size: S, M) ---
            new ProductDetail { Id = 36, ProductId = 18, Size = "M", Price = 25000 },
            new ProductDetail { Id = 37, ProductId = 18, Size = "S", Price = 15000 }
        );
    }
}