using CoffeeShopApi.Data;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShopApi.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(AppDbContext context)
    {
        // ❌ XÓA PHẦN DBCC CHECKIDENT - Không cần reset identity mỗi lần start app
        // Chỉ seed data nếu DB rỗng

        try
        {
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

            // Seed sample user addresses for testing
            try
            {
                await UserAddressSeeder.SeedSampleAddresses(context);
            }
            catch
            {
                // ignore
            }

            // Seed sample vouchers for testing
            try
            {
                await VoucherSeeder.SeedSampleVouchers(context);
            }
            catch
            {
                // ignore
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Seeding error: {ex.Message}");
            throw;
        }
    }
}
