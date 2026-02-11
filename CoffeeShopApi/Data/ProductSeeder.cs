using CoffeeShopApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;

namespace CoffeeShopApi.Data;

public static class ProductSeeder
{
    public static async Task SeedProductsWithOptions(AppDbContext context)
    {
        // ✅ Kiểm tra cả Products VÀ OptionGroups để tránh duplicate
        if (await context.Products.AnyAsync() || await context.OptionGroups.AnyAsync())
        {
            Console.WriteLine("⏭️  Data already exists. Skipping ProductSeeder.");
            return;
        }

        Console.WriteLine("🔧 Creating OptionGroup templates...");

        // ========== BƯỚC 1: TẠO TEMPLATE OPTION GROUPS (1 LẦN DUY NHẤT) ==========

        // Template 1: Kích cỡ (áp dụng cho TẤT CẢ sản phẩm)
        var templateSize = new OptionGroup
        {
            Name = "Kích cỡ",
            Description = "Kích cỡ sản phẩm",
            IsRequired = true,
            AllowMultiple = false,
            DisplayOrder = 1
        };
        context.OptionGroups.Add(templateSize);
        await context.SaveChangesAsync();

        context.OptionItems.AddRange(
            new OptionItem { OptionGroupId = templateSize.Id, Name = "Nhỏ (S)", PriceAdjustment = 0, IsDefault = true, DisplayOrder = 1 },
            new OptionItem { OptionGroupId = templateSize.Id, Name = "Vừa (M)", PriceAdjustment = 5000, DisplayOrder = 2 },
            new OptionItem { OptionGroupId = templateSize.Id, Name = "Lớn (L)", PriceAdjustment = 10000, DisplayOrder = 3 }
        );
        await context.SaveChangesAsync();
        Console.WriteLine($"  ✓ Template 'Kích cỡ' (ID: {templateSize.Id}) với 3 items");

        // Template 2: Mức đường
        var templateSugar = new OptionGroup
        {
            Name = "Mức đường",
            Description = "Độ ngọt của đồ uống",
            IsRequired = true,
            AllowMultiple = false,
            DisplayOrder = 2
        };
        context.OptionGroups.Add(templateSugar);
        await context.SaveChangesAsync();

        context.OptionItems.AddRange(
            new OptionItem { OptionGroupId = templateSugar.Id, Name = "0%", PriceAdjustment = 0, DisplayOrder = 1 },
            new OptionItem { OptionGroupId = templateSugar.Id, Name = "30%", PriceAdjustment = 0, DisplayOrder = 2 },
            new OptionItem { OptionGroupId = templateSugar.Id, Name = "50%", PriceAdjustment = 0, DisplayOrder = 3 },
            new OptionItem { OptionGroupId = templateSugar.Id, Name = "70%", PriceAdjustment = 0, IsDefault = true, DisplayOrder = 4 },
            new OptionItem { OptionGroupId = templateSugar.Id, Name = "100%", PriceAdjustment = 0, DisplayOrder = 5 }
        );
        await context.SaveChangesAsync();
        Console.WriteLine($"  ✓ Template 'Mức đường' (ID: {templateSugar.Id}) với 5 items");

        // Template 3: Mức đá
        var templateIce = new OptionGroup
        {
            Name = "Mức đá",
            Description = "Lượng đá trong đồ uống",
            IsRequired = true,
            AllowMultiple = false,
            DisplayOrder = 3
        };
        context.OptionGroups.Add(templateIce);
        await context.SaveChangesAsync();

        context.OptionItems.AddRange(
            new OptionItem { OptionGroupId = templateIce.Id, Name = "0%", PriceAdjustment = 0, DisplayOrder = 1 },
            new OptionItem { OptionGroupId = templateIce.Id, Name = "30%", PriceAdjustment = 0, DisplayOrder = 2 },
            new OptionItem { OptionGroupId = templateIce.Id, Name = "50%", PriceAdjustment = 0, DisplayOrder = 3 },
            new OptionItem { OptionGroupId = templateIce.Id, Name = "70%", PriceAdjustment = 0, IsDefault = true, DisplayOrder = 4 },
            new OptionItem { OptionGroupId = templateIce.Id, Name = "100%", PriceAdjustment = 0, DisplayOrder = 5 }
        );
        await context.SaveChangesAsync();
        Console.WriteLine($"  ✓ Template 'Mức đá' (ID: {templateIce.Id}) với 5 items");

        // Template 4: Topping
        var templateTopping = new OptionGroup
        {
            Name = "Topping",
            Description = "Topping thêm cho đồ uống",
            IsRequired = false,
            AllowMultiple = true,
            DisplayOrder = 4
        };
        context.OptionGroups.Add(templateTopping);
        await context.SaveChangesAsync();

        context.OptionItems.AddRange(
            new OptionItem { OptionGroupId = templateTopping.Id, Name = "Trân châu đen", PriceAdjustment = 10000, DisplayOrder = 1 },
            new OptionItem { OptionGroupId = templateTopping.Id, Name = "Trân châu trắng", PriceAdjustment = 10000, DisplayOrder = 2 },
            new OptionItem { OptionGroupId = templateTopping.Id, Name = "Thạch dừa", PriceAdjustment = 8000, DisplayOrder = 3 },
            new OptionItem { OptionGroupId = templateTopping.Id, Name = "Pudding", PriceAdjustment = 12000, DisplayOrder = 4 },
            new OptionItem { OptionGroupId = templateTopping.Id, Name = "Kem cheese", PriceAdjustment = 15000, DisplayOrder = 5 }
        );
        await context.SaveChangesAsync();
        Console.WriteLine($"  ✓ Template 'Topping' (ID: {templateTopping.Id}) với 5 items");

        Console.WriteLine($"📦 Tổng cộng: 4 OptionGroup templates, 18 OptionItems");

        // ========== BƯỚC 2: TẠO PRODUCTS ==========
        Console.WriteLine("\n🍵 Creating products...");

        // Lấy danh sách file ảnh thực tế trong wwwroot/images
        var imagesDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
        if (!Directory.Exists(imagesDir))
        {
            Directory.CreateDirectory(imagesDir);
        }

        var imageFilesList = Directory.GetFiles(imagesDir)
            .Select(f => Path.GetFileName(f) ?? "")
            .ToList();

        var imageFilesLookup = imageFilesList
            .GroupBy(f => f.ToLowerInvariant())
            .ToDictionary(g => g.Key, g => g.First());

        string RemoveVietnameseDiacritics(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            string normalized = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (char c in normalized)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }
            return sb.ToString()
                .Normalize(NormalizationForm.FormC)
                .Replace("đ", "d")
                .Replace("Đ", "D");
        }

        string? FindImageFile(string productName)
        {
            string baseName = RemoveVietnameseDiacritics(productName)
                .Replace(" ", "")
                .Replace("-", "");

            var possibleNames = new[]
            {
                baseName.ToLowerInvariant() + ".jpg",
                baseName + ".jpg",
                productName.Replace(" ", "") + ".jpg",
            };

            foreach (var name in possibleNames)
            {
                var lowerName = name.ToLowerInvariant();
                if (imageFilesLookup.TryGetValue(lowerName, out var actualFileName))
                {
                    return actualFileName;
                }
            }
            return null;
        }

        var products = new List<Product>
        {
            // Coffee (CategoryId = 1)
            new Product { Name = "Cà Phê Đen Đá", Description = "Cà phê Robusta đậm đà, thơm nồng", BasePrice = 25000, CategoryId = 1 },
            new Product { Name = "Cà Phê Sữa Đá", Description = "Hương vị cà phê Việt Nam truyền thống", BasePrice = 29000, CategoryId = 1 },
            new Product { Name = "Bạc Xỉu", Description = "Sữa nóng pha cà phê, vị ngọt nhẹ", BasePrice = 32000, CategoryId = 1 },
            new Product { Name = "Latte", Description = "Espresso hòa quyện cùng sữa tươi", BasePrice = 45000, CategoryId = 1 },
            new Product { Name = "Cappuccino", Description = "Cân bằng hoàn hảo giữa espresso và bọt sữa", BasePrice = 45000, CategoryId = 1 },
            new Product { Name = "Americano", Description = "Espresso pha loãng với nước nóng", BasePrice = 35000, CategoryId = 1 },
            new Product { Name = "Caramel Macchiato", Description = "Sữa tươi vani, espresso và sốt caramel", BasePrice = 50000, CategoryId = 1 },
            new Product { Name = "Mocha", Description = "Espresso kết hợp sô-cô-la Bỉ", BasePrice = 48000, CategoryId = 1 },
            new Product { Name = "Flat White", Description = "Espresso đậm đà với lớp sữa mịn", BasePrice = 46000, CategoryId = 1 },
            new Product { Name = "Cà Phê Cốt Dừa", Description = "Cà phê kết hợp cốt dừa béo ngậy", BasePrice = 38000, CategoryId = 1 },

            // Tea (CategoryId = 2)
            new Product { Name = "Trà Đào Cam Sả", Description = "Trà đen thơm lừng với đào và cam sả", BasePrice = 45000, CategoryId = 2 },
            new Product { Name = "Trà Sen Vàng", Description = "Trà ô long thanh mát với hạt sen", BasePrice = 45000, CategoryId = 2 },
            new Product { Name = "Trà Vải Hoa Hồng", Description = "Vị ngọt vải thiều hòa quyện hoa hồng", BasePrice = 42000, CategoryId = 2 },
            new Product { Name = "Trà Sữa Truyền Thống", Description = "Trà đen pha sữa tươi thơm béo", BasePrice = 35000, CategoryId = 2 },
            new Product { Name = "Matcha Latte", Description = "Bột matcha Nhật Bản nguyên chất", BasePrice = 48000, CategoryId = 2 },
            new Product { Name = "Hồng Trà Sữa", Description = "Hồng trà thơm với sữa tươi", BasePrice = 38000, CategoryId = 2 },
            new Product { Name = "Trà Chanh Dây", Description = "Trà xanh chua ngọt với chanh dây", BasePrice = 40000, CategoryId = 2 },
            new Product { Name = "Trà Oolong Tứ Quý", Description = "Trà oolong cao cấp, hương thơm tự nhiên", BasePrice = 52000, CategoryId = 2 },
            new Product { Name = "Trà Dâu", Description = "Trà xanh kết hợp dâu tây tươi", BasePrice = 43000, CategoryId = 2 },
            new Product { Name = "Trà Atiso Mật Ong", Description = "Trà atiso thanh nhiệt với mật ong", BasePrice = 36000, CategoryId = 2 },

            // Food (CategoryId = 3) - Chỉ có Size, không có Sugar/Ice/Topping
            new Product { Name = "Bánh Croissant Bơ", Description = "Bánh sừng bò ngàn lớp giòn rụm", BasePrice = 35000, CategoryId = 3 },
            new Product { Name = "Tiramisu", Description = "Bánh ngọt vị cà phê kem mascarpone", BasePrice = 45000, CategoryId = 3 },
            new Product { Name = "Cheesecake Chanh Dây", Description = "Bánh phô mai chua ngọt hài hòa", BasePrice = 48000, CategoryId = 3 },
            new Product { Name = "Mousse Chocolate", Description = "Bánh mousse mềm mịn vị sô-cô-la", BasePrice = 42000, CategoryId = 3 },
            new Product { Name = "Bánh Mì Que Pate", Description = "Bánh mì que giòn rụm với pate thơm", BasePrice = 15000, CategoryId = 3 },
            new Product { Name = "Donut Sô-cô-la", Description = "Bánh vòng chiên phủ sô-cô-la", BasePrice = 25000, CategoryId = 3 },

            // Freeze (CategoryId = 4)
            new Product { Name = "Matcha Đá Xay", Description = "Bột trà xanh xay cùng đá và sữa", BasePrice = 55000, CategoryId = 4 },
            new Product { Name = "Cookie Đá Xay", Description = "Bánh Oreo xay mịn với kem tươi", BasePrice = 55000, CategoryId = 4 },
            new Product { Name = "Caramel Frappuccino", Description = "Cà phê xay đá với sốt caramel", BasePrice = 58000, CategoryId = 4 },
            new Product { Name = "Choco Mint Freeze", Description = "Sô-cô-la bạc hà xay mát lạnh", BasePrice = 56000, CategoryId = 4 },
        };

        // Gán ImageUrl dựa vào file thực tế
        foreach (var product in products)
        {
            var foundFile = FindImageFile(product.Name);
            if (foundFile != null)
            {
                product.ImageUrl = $"/images/{foundFile}";
                Console.WriteLine($"  ✓ {product.Name} => /images/{foundFile}");
            }
            else
            {
                product.ImageUrl = "/images/placeholder.jpg";
                Console.WriteLine($"  ✗ {product.Name} => placeholder (no matching image)");
            }
        }

        context.Products.AddRange(products);
        await context.SaveChangesAsync();
        Console.WriteLine($"\n📦 Created {products.Count} products");

        // ========== BƯỚC 3: MAP PRODUCTS VỚI TEMPLATE OPTION GROUPS ==========
        Console.WriteLine("\n🔗 Mapping products to OptionGroup templates...");

        foreach (var product in products)
        {
            // Tất cả products đều có Size
            context.ProductOptionGroups.Add(new ProductOptionGroup
            {
                ProductId = product.Id,
                OptionGroupId = templateSize.Id,
                DisplayOrder = 1
            });

            // Chỉ products không phải Food (CategoryId != 3) mới có Sugar, Ice, Topping
            if (product.CategoryId != 3)
            {
                context.ProductOptionGroups.Add(new ProductOptionGroup
                {
                    ProductId = product.Id,
                    OptionGroupId = templateSugar.Id,
                    DisplayOrder = 2
                });

                context.ProductOptionGroups.Add(new ProductOptionGroup
                {
                    ProductId = product.Id,
                    OptionGroupId = templateIce.Id,
                    DisplayOrder = 3
                });

                context.ProductOptionGroups.Add(new ProductOptionGroup
                {
                    ProductId = product.Id,
                    OptionGroupId = templateTopping.Id,
                    DisplayOrder = 4
                });
            }
        }

        await context.SaveChangesAsync();

        // Thống kê
        var drinkCount = products.Count(p => p.CategoryId != 3);
        var foodCount = products.Count(p => p.CategoryId == 3);
        var totalMappings = drinkCount * 4 + foodCount * 1;
        Console.WriteLine($"  ✓ {drinkCount} drinks × 4 groups = {drinkCount * 4} mappings");
        Console.WriteLine($"  ✓ {foodCount} foods × 1 group = {foodCount * 1} mappings");
        Console.WriteLine($"📦 Total: {totalMappings} ProductOptionGroup mappings");

        Console.WriteLine("\n✅ Seed completed successfully!");
        Console.WriteLine($"   - OptionGroups: 4 templates (thay vì {products.Count * 4} như cũ)");
        Console.WriteLine($"   - OptionItems: 18 items (thay vì {products.Count * 18} như cũ)");
        Console.WriteLine($"   - ProductOptionGroups: {totalMappings} mappings");
    }
}
