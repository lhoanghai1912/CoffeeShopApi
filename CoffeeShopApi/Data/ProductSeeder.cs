using CoffeeShopApi.Models;
using Microsoft.EntityFrameworkCore;

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
        var imageFiles = Directory.GetFiles(imagesDir)
            .Select(f => Path.GetFileName(f) ?? "")
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Quy tắc đặt tên file ảnh: không dấu, viết thường, thay khoảng trắng bằng gạch ngang
        string ToImageFileName(string name)
        {
            string fileName = name.ToLowerInvariant()
                .Replace(" ", "")
                .Replace("đ", "d")
                .Replace("ă", "a").Replace("â", "a").Replace("á", "a").Replace("à", "a").Replace("ả", "a").Replace("ã", "a").Replace("ạ", "a")
                .Replace("ê", "e").Replace("é", "e").Replace("è", "e").Replace("ẻ", "e").Replace("ẽ", "e").Replace("ẹ", "e")
                .Replace("ô", "o").Replace("ơ", "o").Replace("ó", "o").Replace("ò", "o").Replace("ỏ", "o").Replace("õ", "o").Replace("ọ", "o")
                .Replace("ư", "u").Replace("ú", "u").Replace("ù", "u").Replace("ủ", "u").Replace("ũ", "u").Replace("ụ", "u")
                .Replace("í", "i").Replace("ì", "i").Replace("ỉ", "i").Replace("ĩ", "i").Replace("ị", "i")
                .Replace("ý", "y").Replace("ỳ", "y").Replace("ỷ", "y").Replace("ỹ", "y").Replace("ỵ", "y")
                .Replace("ấ", "a").Replace("ầ", "a").Replace("ẩ", "a").Replace("ẫ", "a").Replace("ậ", "a")
                .Replace("ấ", "a").Replace("ầ", "a").Replace("ẩ", "a").Replace("ẫ", "a").Replace("ậ", "a")
                .Replace("ấ", "a").Replace("ầ", "a").Replace("ẩ", "a").Replace("ẫ", "a").Replace("ậ", "a");
            return fileName + ".jpg";
        }

        var products = new List<Product>
        {
            new Product { Name = "Ca Phe Den Da", Description = "Ca phe Robusta dam da", BasePrice = 25000, CategoryId = 1 },
            new Product { Name = "Ca Phe Sua Da", Description = "Huong vi ca phe Viet Nam", BasePrice = 29000, CategoryId = 1 },
            new Product { Name = "Bac Xiu", Description = "Sua nong pha ca phe", BasePrice = 32000, CategoryId = 1 },
            new Product { Name = "Latte", Description = "Espresso hoa quyen", BasePrice = 45000, CategoryId = 1 },
            new Product { Name = "Cappuccino", Description = "Can bang hoan hao", BasePrice = 45000, CategoryId = 1 },
            new Product { Name = "Americano", Description = "Espresso pha loang", BasePrice = 35000, CategoryId = 1 },
            new Product { Name = "Caramel Macchiato", Description = "Sua tuoi vani Espresso", BasePrice = 50000, CategoryId = 1 },
            new Product { Name = "Mocha", Description = "Espresso ket hop socola", BasePrice = 48000, CategoryId = 1 },
            new Product { Name = "Flat White", Description = "Espresso dam da", BasePrice = 46000, CategoryId = 1 },
            new Product { Name = "Ca Phe Cot Dua", Description = "Ca phe ket hop cot dua", BasePrice = 38000, CategoryId = 1 },
            new Product { Name = "Tra Dao Cam Sa", Description = "Tra den thom lung", BasePrice = 45000, CategoryId = 2 },
            new Product { Name = "Tra Sen Vang", Description = "Tra o long thanh mat", BasePrice = 45000, CategoryId = 2 },
            new Product { Name = "Tra Vai Hoa Hong", Description = "Vi ngot vai thieu", BasePrice = 42000, CategoryId = 2 },
            new Product { Name = "Tra Sua Truyen Thong", Description = "Tra den pha sua", BasePrice = 35000, CategoryId = 2 },
            new Product { Name = "Matcha Latte", Description = "Bot matcha Nhat Ban", BasePrice = 48000, CategoryId = 2 },
            new Product { Name = "Hong Tra Sua", Description = "Hong tra thom", BasePrice = 38000, CategoryId = 2 },
            new Product { Name = "Tra Chanh Leo", Description = "Tra xanh chua ngot", BasePrice = 40000, CategoryId = 2 },
            new Product { Name = "Tra Oolong Tu Quy", Description = "Tra oolong cao cap", BasePrice = 52000, CategoryId = 2 },
            new Product { Name = "Tra Dau", Description = "Tra xanh ket hop dau", BasePrice = 43000, CategoryId = 2 },
            new Product { Name = "Tra Atiso Mat Ong", Description = "Tra atiso thanh nhiet", BasePrice = 36000, CategoryId = 2 },
            new Product { Name = "Banh Croissant Bo", Description = "Banh sung bo ngan lop", BasePrice = 35000, CategoryId = 3 },
            new Product { Name = "Tiramisu", Description = "Banh ngot vi ca phe", BasePrice = 45000, CategoryId = 3 },
            new Product { Name = "Cheesecake Chanh Day", Description = "Banh pho mai chua ngot", BasePrice = 48000, CategoryId = 3 },
            new Product { Name = "Mousse Chocolate", Description = "Banh mousse mem min", BasePrice = 42000, CategoryId = 3 },
            new Product { Name = "Banh Mi Que Pate", Description = "Banh mi que gion rum", BasePrice = 15000, CategoryId = 3 },
            new Product { Name = "Donut Socola", Description = "Banh vong chien", BasePrice = 25000, CategoryId = 3 },
            new Product { Name = "Matcha Da Xay", Description = "Bot tra xanh xay", BasePrice = 55000, CategoryId = 4 },
            new Product { Name = "Cookie Da Xay", Description = "Banh Oreo xay", BasePrice = 55000, CategoryId = 4 },
            new Product { Name = "Caramel Frappuccino", Description = "Ca phe xay da", BasePrice = 58000, CategoryId = 4 },
            new Product { Name = "Choco Mint Freeze", Description = "Socola bac ha xay", BasePrice = 56000, CategoryId = 4 },
        };

        // Gán ImageUrl dựa vào file thực tế
        foreach (var product in products)
        {
            var fileName = ToImageFileName(product.Name);
            if (imageFiles.Contains(fileName))
            {
                product.ImageUrl = $"/images/{fileName}";
            }
            else
            {
                product.ImageUrl = "/images/placeholder.jpg";
            }
        }

        context.Products.AddRange(products);
        await context.SaveChangesAsync();

        foreach (var product in products)
        {
            // Luon co OptionGroup Size
            var sizeGroup = new OptionGroup { ProductId = product.Id, Name = "Size", IsRequired = true, AllowMultiple = false, DisplayOrder = 1 };
            context.OptionGroups.Add(sizeGroup);
            await context.SaveChangesAsync();

            context.OptionItems.AddRange(
                new OptionItem { OptionGroupId = sizeGroup.Id, Name = "Nho (S)", PriceAdjustment = 0, IsDefault = true, DisplayOrder = 1 },
                new OptionItem { OptionGroupId = sizeGroup.Id, Name = "Vua (M)", PriceAdjustment = 5000, DisplayOrder = 2 },
                new OptionItem { OptionGroupId = sizeGroup.Id, Name = "Lon (L)", PriceAdjustment = 10000, DisplayOrder = 3 }
            );

            // Chi them Muc duong va Topping neu khong phai banh ngot (CategoryId != 3)
            if (product.CategoryId != 3)
            {
                var sugarGroup = new OptionGroup { ProductId = product.Id, Name = "Muc duong", IsRequired = true, AllowMultiple = false, DisplayOrder = 2 };
                context.OptionGroups.Add(sugarGroup);
                await context.SaveChangesAsync();

                context.OptionItems.AddRange(
                    new OptionItem { OptionGroupId = sugarGroup.Id, Name = "30%", PriceAdjustment = 0, DisplayOrder = 1 },
                    new OptionItem { OptionGroupId = sugarGroup.Id, Name = "50%", PriceAdjustment = 0, DisplayOrder = 2 },
                    new OptionItem { OptionGroupId = sugarGroup.Id, Name = "70%", PriceAdjustment = 0, IsDefault = true, DisplayOrder = 3 },
                    new OptionItem { OptionGroupId = sugarGroup.Id, Name = "100%", PriceAdjustment = 0, DisplayOrder = 4 }
                );

                var toppingGroup = new OptionGroup { ProductId = product.Id, Name = "Topping", IsRequired = false, AllowMultiple = true, DisplayOrder = 3 };
                context.OptionGroups.Add(toppingGroup);
                await context.SaveChangesAsync();

                context.OptionItems.AddRange(
                    new OptionItem { OptionGroupId = toppingGroup.Id, Name = "Tran chau den", PriceAdjustment = 10000, DisplayOrder = 1 },
                    new OptionItem { OptionGroupId = toppingGroup.Id, Name = "Tran chau trang", PriceAdjustment = 10000, DisplayOrder = 2 },
                    new OptionItem { OptionGroupId = toppingGroup.Id, Name = "Thach dua", PriceAdjustment = 8000, DisplayOrder = 3 },
                    new OptionItem { OptionGroupId = toppingGroup.Id, Name = "Pudding", PriceAdjustment = 12000, DisplayOrder = 4 },
                    new OptionItem { OptionGroupId = toppingGroup.Id, Name = "Kem cheese", PriceAdjustment = 15000, DisplayOrder = 5 }
                );
            }
        }

        await context.SaveChangesAsync();
    }
}
