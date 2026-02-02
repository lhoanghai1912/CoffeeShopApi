using CoffeeShopApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShopApi.Data;

public static class UserAddressSeeder
{
    public static async Task SeedSampleAddresses(AppDbContext context)
    {
        // Kiểm tra đã có địa chỉ chưa
        if (await context.UserAddresses.AnyAsync())
            return;

        // Lấy tất cả user để seed địa chỉ
        var users = await context.Users.ToListAsync();
        if (!users.Any())
            return;

        var addresses = new List<UserAddress>();

        foreach (var user in users)
        {
            // Mỗi user có 3-4 địa chỉ mẫu
            addresses.Add(new UserAddress
            {
                UserId = user.Id,
                RecipientName = user.FullName ?? user.UserName,
                PhoneNumber = user.PhoneNumber ?? "0988123456",
                AddressLine = "123 Đường Nguyễn Huệ, Phường Bến Nghé, Quận 1, TP.HCM",
                Label = "Nhà",
                IsDefault = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

            addresses.Add(new UserAddress
            {
                UserId = user.Id,
                RecipientName = user.FullName ?? user.UserName,
                PhoneNumber = "0912345678",
                AddressLine = "456 Đường Lê Lợi, Phường Bến Thành, Quận 1, TP.HCM",
                Label = "Văn phòng",
                IsDefault = false,
                CreatedAt = DateTime.UtcNow.AddMinutes(-10),
                UpdatedAt = DateTime.UtcNow.AddMinutes(-10)
            });

            addresses.Add(new UserAddress
            {
                UserId = user.Id,
                RecipientName = "Người thân",
                PhoneNumber = "0909876543",
                AddressLine = "789 Đường Trần Hưng Đạo, Phường Cầu Ông Lãnh, Quận 1, TP.HCM",
                Label = "Nhà người thân",
                IsDefault = false,
                CreatedAt = DateTime.UtcNow.AddMinutes(-20),
                UpdatedAt = DateTime.UtcNow.AddMinutes(-20)
            });

            addresses.Add(new UserAddress
            {
                UserId = user.Id,
                RecipientName = user.FullName ?? user.UserName,
                PhoneNumber = "0977654321",
                AddressLine = "101 Đường Võ Văn Tần, Phường 6, Quận 3, TP.HCM",
                Label = "Khác",
                IsDefault = false,
                CreatedAt = DateTime.UtcNow.AddMinutes(-30),
                UpdatedAt = DateTime.UtcNow.AddMinutes(-30)
            });
        }

        context.UserAddresses.AddRange(addresses);
        await context.SaveChangesAsync();
    }
}
