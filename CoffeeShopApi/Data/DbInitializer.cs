using CoffeeShopApi.Data;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShopApi.Data;

public static class DbInitializer
{
    public static void Initialize(AppDbContext context)
    {
        // Reset identity seed for ALL tables to ensure they continue from the last used ID
        try 
        {
            // List of all tables with identity columns
            var tables = new[]
            {
                "Users",
                "Roles",
                "Permissions",
                "Products",
                "ProductDetails",
                "Categories"
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
                    // Ignore errors for individual tables (e.g., if table doesn't exist or no Id column)
                }
            }
        }
        catch
        {
            // Ignore errors (e.g. if not using SQL Server or lack of permissions)
        }
    }
}
