using CoffeeShopApi.Data;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShopApi.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(AppDbContext context)
    {
        // Reset identity seed for ALL tables
        try 
        {
            // List of all tables with identity columns (khong co ProductDetails nua)
            var tables = new[]
            {
                "Users",
                "Roles",
                "Permissions",
                "Products",
                "Categories",
                "OptionGroups",
                "OptionItems",
                "Orders",
                "OrderItems",
                "OrderItemOptions"
            };

            foreach (var table in tables)
            {
                try
                {
                    // Get max ID and reseed for each table
                    var sql = $@"
                        DECLARE @maxId INT;
                        SELECT @maxId = ISNULL(MAX(Id), 0) FROM {table};
                        IF @maxId > 0
                            DBCC CHECKIDENT ('{table}', RESEED, @maxId);
                        ELSE
                            DBCC CHECKIDENT ('{table}', RESEED, 0);
                    ";
                    
                    context.Database.ExecuteSqlRaw(sql);
                }
                catch
                {
                    // Ignore errors for individual tables
                }
            }

            // Seed 30 products with option system
            await ProductSeeder.SeedProductsWithOptions(context);

            // Seed sample orders for testing
            try
            {
                await OrderSeeder.SeedSampleOrders(context);
            }
            catch
            {
                // ignore
            }
        }
        catch
        {
            // Ignore errors
        }
    }
}
