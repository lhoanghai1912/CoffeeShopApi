using CoffeeShopApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;

namespace CoffeeShopApi.Data;

public static class ProductSeeder
{
    public static async Task SeedProductsWithOptions(AppDbContext context)
    {
        if (await context.Products.AnyAsync())
        {
            return;
        }

        // Lấy danh sách file ảnh thực tế trong wwwroot/images
        var imagesDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
        if (!Directory.Exists(imagesDir))
        {
            Directory.CreateDirectory(imagesDir);
        }

        // Lưu cả tên file gốc để mapping chính xác
        var imageFilesList = Directory.GetFiles(imagesDir)
            .Select(f => Path.GetFileName(f) ?? "")
            .ToList();

        // Tạo dictionary lowercase -> original name để tìm kiếm
        var imageFilesLookup = imageFilesList
            .GroupBy(f => f.ToLowerInvariant())
            .ToDictionary(g => g.Key, g => g.First());

        /// <summary>
        /// Chuyển tên sản phẩm tiếng Việt thành tên file ảnh (không dấu, viết thường)
        /// </summary>
        string RemoveVietnameseDiacritics(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            // Normalize để tách dấu khỏi ký tự gốc
            string normalized = text.Normalize(NormalizationForm.FormD);

            var sb = new StringBuilder();
            foreach (char c in normalized)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                // Bỏ qua các ký tự dấu (NonSpacingMark)
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }

            // Xử lý riêng ký tự đ/Đ (không bị tách bởi Normalize)
            return sb.ToString()
                .Normalize(NormalizationForm.FormC)
                .Replace("đ", "d")
                .Replace("Đ", "D");
        }

        /// <summary>
        /// Tìm file ảnh phù hợp với tên sản phẩm (hỗ trợ nhiều convention)
        /// </summary>
        string? FindImageFile(string productName)
        {
            // Tạo tên file chuẩn: loại bỏ dấu tiếng Việt và khoảng trắng
            string baseName = RemoveVietnameseDiacritics(productName)
                .Replace(" ", "")
                .Replace("-", "");

            // Thử tìm theo các convention khác nhau
            var possibleNames = new[]
            {
                baseName.ToLowerInvariant() + ".jpg",           // caphesuada.jpg
                baseName + ".jpg",                               // CaPheSuaDa.jpg (giữ nguyên case)
                productName.Replace(" ", "") + ".jpg",           // CàPhêSữaĐá.jpg (có dấu)
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
            // Coffee
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

            // Tea
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

            // Food
            new Product { Name = "Bánh Croissant Bơ", Description = "Bánh sừng bò ngàn lớp giòn rụm", BasePrice = 35000, CategoryId = 3 },
            new Product { Name = "Tiramisu", Description = "Bánh ngọt vị cà phê kem mascarpone", BasePrice = 45000, CategoryId = 3 },
            new Product { Name = "Cheesecake Chanh Dây", Description = "Bánh phô mai chua ngọt hài hòa", BasePrice = 48000, CategoryId = 3 },
            new Product { Name = "Mousse Chocolate", Description = "Bánh mousse mềm mịn vị sô-cô-la", BasePrice = 42000, CategoryId = 3 },
            new Product { Name = "Bánh Mì Que Pate", Description = "Bánh mì que giòn rụm với pate thơm", BasePrice = 15000, CategoryId = 3 },
            new Product { Name = "Donut Sô-cô-la", Description = "Bánh vòng chiên phủ sô-cô-la", BasePrice = 25000, CategoryId = 3 },

            // Freeze
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
                Console.WriteLine($"✓ {product.Name} => /images/{foundFile}");
            }
            else
            {
                product.ImageUrl = "/images/placeholder.jpg";
                Console.WriteLine($"✗ {product.Name} => placeholder (no matching image)");
            }
        }

        context.Products.AddRange(products);
        await context.SaveChangesAsync();

        foreach (var product in products)
        {
            // Luôn có OptionGroup Size
            var sizeGroup = new OptionGroup { ProductId = product.Id, Name = "Kích cỡ", IsRequired = true, AllowMultiple = false, DisplayOrder = 1 };
            context.OptionGroups.Add(sizeGroup);
            await context.SaveChangesAsync();

            context.OptionItems.AddRange(
                new OptionItem { OptionGroupId = sizeGroup.Id, Name = "Nhỏ (S)", PriceAdjustment = 0, IsDefault = true, DisplayOrder = 1 },
                new OptionItem { OptionGroupId = sizeGroup.Id, Name = "Vừa (M)", PriceAdjustment = 5000, DisplayOrder = 2 },
                new OptionItem { OptionGroupId = sizeGroup.Id, Name = "Lớn (L)", PriceAdjustment = 10000, DisplayOrder = 3 }
            );

            // Chỉ thêm Mức đường và Topping nếu không phải bánh ngọt (CategoryId != 3)
            if (product.CategoryId != 3)
            {
                var sugarGroup = new OptionGroup { ProductId = product.Id, Name = "Mức đường", IsRequired = true, AllowMultiple = false, DisplayOrder = 2 };
                context.OptionGroups.Add(sugarGroup);
                await context.SaveChangesAsync();

                context.OptionItems.AddRange(
                    new OptionItem { OptionGroupId = sugarGroup.Id, Name = "0%", PriceAdjustment = 0, DisplayOrder = 1 },
                    new OptionItem { OptionGroupId = sugarGroup.Id, Name = "30%", PriceAdjustment = 0, DisplayOrder = 2 },
                    new OptionItem { OptionGroupId = sugarGroup.Id, Name = "50%", PriceAdjustment = 0, DisplayOrder = 3 },
                    new OptionItem { OptionGroupId = sugarGroup.Id, Name = "70%", PriceAdjustment = 0, IsDefault = true, DisplayOrder = 4 },
                    new OptionItem { OptionGroupId = sugarGroup.Id, Name = "100%", PriceAdjustment = 0, DisplayOrder = 5 }
                );

                var iceGroup = new OptionGroup { ProductId = product.Id, Name = "Mức đá", IsRequired = true, AllowMultiple = false, DisplayOrder = 3 };
                context.OptionGroups.Add(iceGroup);
                await context.SaveChangesAsync();

                context.OptionItems.AddRange(
                    new OptionItem { OptionGroupId = iceGroup.Id, Name = "0%", PriceAdjustment = 0, DisplayOrder = 1 },
                    new OptionItem { OptionGroupId = iceGroup.Id, Name = "30%", PriceAdjustment = 0, DisplayOrder = 2 },
                    new OptionItem { OptionGroupId = iceGroup.Id, Name = "50%", PriceAdjustment = 0, DisplayOrder = 3 },
                    new OptionItem
                    {
                        OptionGroupId = iceGroup.Id, Name = "70%", PriceAdjustment = 0, IsDefault = true,
                        DisplayOrder = 4
                    },
                    new OptionItem { OptionGroupId = iceGroup.Id, Name = "100%", PriceAdjustment = 0, DisplayOrder = 5 }
                );

                var toppingGroup = new OptionGroup { ProductId = product.Id, Name = "Topping", IsRequired = false, AllowMultiple = true, DisplayOrder = 4 };
                context.OptionGroups.Add(toppingGroup);
                await context.SaveChangesAsync();

                context.OptionItems.AddRange(
                    new OptionItem { OptionGroupId = toppingGroup.Id, Name = "Trân châu đen", PriceAdjustment = 10000, DisplayOrder = 1 },
                    new OptionItem { OptionGroupId = toppingGroup.Id, Name = "Trân châu trắng", PriceAdjustment = 10000, DisplayOrder = 2 },
                    new OptionItem { OptionGroupId = toppingGroup.Id, Name = "Thạch dừa", PriceAdjustment = 8000, DisplayOrder = 3 },
                    new OptionItem { OptionGroupId = toppingGroup.Id, Name = "Pudding", PriceAdjustment = 12000, DisplayOrder = 4 },
                    new OptionItem { OptionGroupId = toppingGroup.Id, Name = "Kem cheese", PriceAdjustment = 15000, DisplayOrder = 5 }
                );
            }
        }

        await context.SaveChangesAsync();
    }
}
