using CoffeeShopApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShopApi.Data;

public static class VoucherSeeder
{
    /// <summary>
    /// Lấy thời gian hiện tại theo múi giờ Việt Nam (UTC+7)
    /// </summary>
    private static DateTime GetVietnamTime()
    {
        var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);
    }

    public static async Task SeedSampleVouchers(AppDbContext context)
    {
        // Kiểm tra đã có voucher chưa
        if (await context.Vouchers.AnyAsync())
            return;

        var now = GetVietnamTime();

        var vouchers = new List<Voucher>
        {
            // ==================== PUBLIC VOUCHERS (17 vouchers) ====================
            
            // --- Welcome & New User Vouchers ---
            new Voucher
            {
                Code = "WELCOME10K",
                Description = "Giảm 10,000đ cho đơn hàng đầu tiên",
                DiscountType = DiscountType.FixedAmount,
                DiscountValue = 10000,
                MinOrderValue = 50000,
                MaxDiscountAmount = null,
                StartDate = now.AddDays(-30),
                EndDate = now.AddDays(60),
                UsageLimit = 1000,
                UsageLimitPerUser = 1,
                CurrentUsageCount = 0,
                IsActive = true,
                IsPublic = true,
                CreatedAt = now,
                UpdatedAt = now
            },

            new Voucher
            {
                Code = "NEWBIE15",
                Description = "Giảm 15% cho khách mới (tối đa 30k)",
                DiscountType = DiscountType.Percentage,
                DiscountValue = 15,
                MinOrderValue = 100000,
                MaxDiscountAmount = 30000,
                StartDate = now.AddDays(-14),
                EndDate = now.AddDays(90),
                UsageLimit = 500,
                UsageLimitPerUser = 1,
                CurrentUsageCount = 0,
                IsActive = true,
                IsPublic = true,
                CreatedAt = now,
                UpdatedAt = now
            },

            // --- Flash Sale Vouchers ---
            new Voucher
            {
                Code = "FLASH20",
                Description = "Flash Sale - Giảm 20% (max 50k)",
                DiscountType = DiscountType.Percentage,
                DiscountValue = 20,
                MinOrderValue = 80000,
                MaxDiscountAmount = 50000,
                StartDate = now.AddDays(-7),
                EndDate = now.AddDays(7),
                UsageLimit = 200,
                UsageLimitPerUser = 2,
                CurrentUsageCount = 0,
                IsActive = true,
                IsPublic = true,
                CreatedAt = now,
                UpdatedAt = now
            },

            new Voucher
            {
                Code = "FLASH25K",
                Description = "Flash Sale - Giảm 25,000đ",
                DiscountType = DiscountType.FixedAmount,
                DiscountValue = 25000,
                MinOrderValue = 150000,
                MaxDiscountAmount = null,
                StartDate = now.AddDays(-3),
                EndDate = now.AddDays(14),
                UsageLimit = 300,
                UsageLimitPerUser = 3,
                CurrentUsageCount = 0,
                IsActive = true,
                IsPublic = true,
                CreatedAt = now,
                UpdatedAt = now
            },

            // --- Regular Discount Vouchers ---
            new Voucher
            {
                Code = "SAVE10",
                Description = "Tiết kiệm 10% (tối đa 20k)",
                DiscountType = DiscountType.Percentage,
                DiscountValue = 10,
                MinOrderValue = 70000,
                MaxDiscountAmount = 20000,
                StartDate = now.AddDays(-20),
                EndDate = now.AddDays(40),
                UsageLimit = null, // Unlimited
                UsageLimitPerUser = 5,
                CurrentUsageCount = 0,
                IsActive = true,
                IsPublic = true,
                CreatedAt = now,
                UpdatedAt = now
            },

            new Voucher
            {
                Code = "SAVE15K",
                Description = "Giảm ngay 15,000đ",
                DiscountType = DiscountType.FixedAmount,
                DiscountValue = 15000,
                MinOrderValue = 60000,
                MaxDiscountAmount = null,
                StartDate = now.AddDays(-15),
                EndDate = now.AddDays(45),
                UsageLimit = null,
                UsageLimitPerUser = 10,
                CurrentUsageCount = 0,
                IsActive = true,
                IsPublic = true,
                CreatedAt = now,
                UpdatedAt = now
            },

            // --- Big Order Vouchers ---
            new Voucher
            {
                Code = "BIGORDER50K",
                Description = "Giảm 50,000đ cho đơn từ 200,000đ",
                DiscountType = DiscountType.FixedAmount,
                DiscountValue = 50000,
                MinOrderValue = 200000,
                MaxDiscountAmount = null,
                StartDate = now.AddDays(-14),
                EndDate = now.AddDays(45),
                UsageLimit = 200,
                UsageLimitPerUser = 2,
                CurrentUsageCount = 0,
                IsActive = true,
                IsPublic = true,
                CreatedAt = now,
                UpdatedAt = now
            },

            new Voucher
            {
                Code = "BIGORDER30",
                Description = "Giảm 30% cho đơn từ 300k (max 120k)",
                DiscountType = DiscountType.Percentage,
                DiscountValue = 30,
                MinOrderValue = 300000,
                MaxDiscountAmount = 120000,
                StartDate = now.AddDays(-10),
                EndDate = now.AddDays(60),
                UsageLimit = 150,
                UsageLimitPerUser = 1,
                CurrentUsageCount = 0,
                IsActive = true,
                IsPublic = true,
                CreatedAt = now,
                UpdatedAt = now
            },

            new Voucher
            {
                Code = "MEGA100K",
                Description = "Giảm 100,000đ cho đơn từ 500,000đ",
                DiscountType = DiscountType.FixedAmount,
                DiscountValue = 100000,
                MinOrderValue = 500000,
                MaxDiscountAmount = null,
                StartDate = now.AddDays(-5),
                EndDate = now.AddDays(30),
                UsageLimit = 100,
                UsageLimitPerUser = 1,
                CurrentUsageCount = 0,
                IsActive = true,
                IsPublic = true,
                CreatedAt = now,
                UpdatedAt = now
            },

            // --- VIP / Frequent Customer Vouchers ---
            new Voucher
            {
                Code = "VIP15",
                Description = "VIP - Giảm 15% (max 100k, không giới hạn)",
                DiscountType = DiscountType.Percentage,
                DiscountValue = 15,
                MinOrderValue = 80000,
                MaxDiscountAmount = 100000,
                StartDate = now.AddDays(-60),
                EndDate = now.AddDays(365),
                UsageLimit = null,
                UsageLimitPerUser = null,
                CurrentUsageCount = 0,
                IsActive = true,
                IsPublic = true,
                CreatedAt = now,
                UpdatedAt = now
            },

            new Voucher
            {
                Code = "FREESHIP",
                Description = "Miễn phí ship (giảm 20k)",
                DiscountType = DiscountType.FixedAmount,
                DiscountValue = 20000,
                MinOrderValue = 50000,
                MaxDiscountAmount = null,
                StartDate = now.AddDays(-30),
                EndDate = now.AddDays(90),
                UsageLimit = null,
                UsageLimitPerUser = 3,
                CurrentUsageCount = 0,
                IsActive = true,
                IsPublic = true,
                CreatedAt = now,
                UpdatedAt = now
            },

            // --- Morning & Evening Vouchers ---
            new Voucher
            {
                Code = "MORNING5K",
                Description = "Buổi sáng vui vẻ - Giảm 5,000đ",
                DiscountType = DiscountType.FixedAmount,
                DiscountValue = 5000,
                MinOrderValue = 30000,
                MaxDiscountAmount = null,
                StartDate = now.AddDays(-7),
                EndDate = now.AddDays(30),
                UsageLimit = null,
                UsageLimitPerUser = 10,
                CurrentUsageCount = 0,
                IsActive = true,
                IsPublic = true,
                CreatedAt = now,
                UpdatedAt = now
            },

            new Voucher
            {
                Code = "EVENING10",
                Description = "Chiều tối thư giãn - Giảm 10%",
                DiscountType = DiscountType.Percentage,
                DiscountValue = 10,
                MinOrderValue = 50000,
                MaxDiscountAmount = 25000,
                StartDate = now.AddDays(-10),
                EndDate = now.AddDays(40),
                UsageLimit = null,
                UsageLimitPerUser = 5,
                CurrentUsageCount = 0,
                IsActive = true,
                IsPublic = true,
                CreatedAt = now,
                UpdatedAt = now
            },

            // --- Weekend Vouchers ---
            new Voucher
            {
                Code = "WEEKEND20K",
                Description = "Cuối tuần vui vẻ - Giảm 20,000đ",
                DiscountType = DiscountType.FixedAmount,
                DiscountValue = 20000,
                MinOrderValue = 100000,
                MaxDiscountAmount = null,
                StartDate = now.AddDays(-14),
                EndDate = now.AddDays(60),
                UsageLimit = null,
                UsageLimitPerUser = 4,
                CurrentUsageCount = 0,
                IsActive = true,
                IsPublic = true,
                CreatedAt = now,
                UpdatedAt = now
            },

            new Voucher
            {
                Code = "WEEKEND25",
                Description = "Weekend Deal - Giảm 25% (max 60k)",
                DiscountType = DiscountType.Percentage,
                DiscountValue = 25,
                MinOrderValue = 120000,
                MaxDiscountAmount = 60000,
                StartDate = now.AddDays(-7),
                EndDate = now.AddDays(30),
                UsageLimit = 400,
                UsageLimitPerUser = 2,
                CurrentUsageCount = 0,
                IsActive = true,
                IsPublic = true,
                CreatedAt = now,
                UpdatedAt = now
            },

            // --- Special Event Vouchers ---
            new Voucher
            {
                Code = "HAPPYDAY",
                Description = "Happy Day - Giảm 12% (max 40k)",
                DiscountType = DiscountType.Percentage,
                DiscountValue = 12,
                MinOrderValue = 60000,
                MaxDiscountAmount = 40000,
                StartDate = now.AddDays(-20),
                EndDate = now.AddDays(50),
                UsageLimit = null,
                UsageLimitPerUser = 7,
                CurrentUsageCount = 0,
                IsActive = true,
                IsPublic = true,
                CreatedAt = now,
                UpdatedAt = now
            },

            new Voucher
            {
                Code = "COFFEE8K",
                Description = "Coffee Lover - Giảm 8,000đ",
                DiscountType = DiscountType.FixedAmount,
                DiscountValue = 8000,
                MinOrderValue = 40000,
                MaxDiscountAmount = null,
                StartDate = now.AddDays(-25),
                EndDate = now.AddDays(75),
                UsageLimit = null,
                UsageLimitPerUser = 15,
                CurrentUsageCount = 0,
                IsActive = true,
                IsPublic = true,
                CreatedAt = now,
                UpdatedAt = now
            },

            // --- Test Vouchers (inactive/expired/future) ---
            new Voucher
            {
                Code = "EXPIRED10",
                Description = "Voucher đã hết hạn",
                DiscountType = DiscountType.FixedAmount,
                DiscountValue = 10000,
                MinOrderValue = null,
                MaxDiscountAmount = null,
                StartDate = now.AddDays(-60),
                EndDate = now.AddDays(-1),
                UsageLimit = 100,
                UsageLimitPerUser = 1,
                CurrentUsageCount = 0,
                IsActive = true,
                IsPublic = true,
                CreatedAt = now.AddDays(-60),
                UpdatedAt = now.AddDays(-60)
            },

            new Voucher
            {
                Code = "FUTURE30",
                Description = "Voucher sẽ có hiệu lực vào tháng tới",
                DiscountType = DiscountType.Percentage,
                DiscountValue = 30,
                MinOrderValue = 150000,
                MaxDiscountAmount = 80000,
                StartDate = now.AddDays(30),
                EndDate = now.AddDays(90),
                UsageLimit = 300,
                UsageLimitPerUser = 1,
                CurrentUsageCount = 0,
                IsActive = true,
                IsPublic = true,
                CreatedAt = now,
                UpdatedAt = now
            },

            new Voucher
            {
                Code = "INACTIVE5K",
                Description = "Voucher đã bị vô hiệu hóa",
                DiscountType = DiscountType.FixedAmount,
                DiscountValue = 5000,
                MinOrderValue = null,
                MaxDiscountAmount = null,
                StartDate = now.AddDays(-30),
                EndDate = now.AddDays(30),
                UsageLimit = 1000,
                UsageLimitPerUser = 5,
                CurrentUsageCount = 0,
                IsActive = false,
                IsPublic = true,
                CreatedAt = now.AddDays(-30),
                UpdatedAt = now
            },

            // ==================== PRIVATE VOUCHERS (15 vouchers) ====================

            // --- Birthday Vouchers ---
            new Voucher
            {
                Code = "BIRTHDAY30K",
                Description = "🎂 Quà sinh nhật - Giảm 30,000đ",
                DiscountType = DiscountType.FixedAmount,
                DiscountValue = 30000,
                MinOrderValue = 50000,
                MaxDiscountAmount = null,
                StartDate = now.AddDays(-30),
                EndDate = now.AddDays(365),
                UsageLimit = null,
                UsageLimitPerUser = 1,
                CurrentUsageCount = 0,
                IsActive = true,
                IsPublic = false,
                CreatedAt = now,
                UpdatedAt = now
            },

            new Voucher
            {
                Code = "BIRTHDAY40",
                Description = "🎉 Sinh nhật vui vẻ - Giảm 40%",
                DiscountType = DiscountType.Percentage,
                DiscountValue = 40,
                MinOrderValue = 80000,
                MaxDiscountAmount = 80000,
                StartDate = now.AddDays(-30),
                EndDate = now.AddDays(365),
                UsageLimit = null,
                UsageLimitPerUser = 1,
                CurrentUsageCount = 0,
                IsActive = true,
                IsPublic = false,
                CreatedAt = now,
                UpdatedAt = now
            },

            // --- VIP Rewards ---
            new Voucher
            {
                Code = "VIPREWARD50",
                Description = "💎 VIP Platinum - Giảm 50% (max 100k)",
                DiscountType = DiscountType.Percentage,
                DiscountValue = 50,
                MinOrderValue = 100000,
                MaxDiscountAmount = 100000,
                StartDate = now.AddDays(-7),
                EndDate = now.AddDays(60),
                UsageLimit = null,
                UsageLimitPerUser = 1,
                CurrentUsageCount = 0,
                IsActive = true,
                IsPublic = false,
                CreatedAt = now,
                UpdatedAt = now
            },

            new Voucher
            {
                Code = "VIPGOLD35",
                Description = "🏆 VIP Gold - Giảm 35% (max 70k)",
                DiscountType = DiscountType.Percentage,
                DiscountValue = 35,
                MinOrderValue = 80000,
                MaxDiscountAmount = 70000,
                StartDate = now.AddDays(-14),
                EndDate = now.AddDays(90),
                UsageLimit = null,
                UsageLimitPerUser = 1,
                CurrentUsageCount = 0,
                IsActive = true,
                IsPublic = false,
                CreatedAt = now,
                UpdatedAt = now
            },

            new Voucher
            {
                Code = "VIPSILVER60K",
                Description = "🥈 VIP Silver - Giảm 60,000đ",
                DiscountType = DiscountType.FixedAmount,
                DiscountValue = 60000,
                MinOrderValue = 150000,
                MaxDiscountAmount = null,
                StartDate = now.AddDays(-20),
                EndDate = now.AddDays(120),
                UsageLimit = null,
                UsageLimitPerUser = 1,
                CurrentUsageCount = 0,
                IsActive = true,
                IsPublic = false,
                CreatedAt = now,
                UpdatedAt = now
            },

            // --- Loyalty Rewards ---
            new Voucher
            {
                Code = "LOYALTY20K",
                Description = "❤️ Khách hàng thân thiết - Giảm 20,000đ",
                DiscountType = DiscountType.FixedAmount,
                DiscountValue = 20000,
                MinOrderValue = null,
                MaxDiscountAmount = null,
                StartDate = now.AddDays(-14),
                EndDate = now.AddDays(90),
                UsageLimit = null,
                UsageLimitPerUser = 1,
                CurrentUsageCount = 0,
                IsActive = true,
                IsPublic = false,
                CreatedAt = now,
                UpdatedAt = now
            },

            new Voucher
            {
                Code = "LOYALTY25",
                Description = "⭐ Loyalty Plus - Giảm 25% (max 50k)",
                DiscountType = DiscountType.Percentage,
                DiscountValue = 25,
                MinOrderValue = 60000,
                MaxDiscountAmount = 50000,
                StartDate = now.AddDays(-21),
                EndDate = now.AddDays(100),
                UsageLimit = null,
                UsageLimitPerUser = 1,
                CurrentUsageCount = 0,
                IsActive = true,
                IsPublic = false,
                CreatedAt = now,
                UpdatedAt = now
            },

            new Voucher
            {
                Code = "LOYALTY45K",
                Description = "💚 Loyalty Premium - Giảm 45,000đ",
                DiscountType = DiscountType.FixedAmount,
                DiscountValue = 45000,
                MinOrderValue = 120000,
                MaxDiscountAmount = null,
                StartDate = now.AddDays(-10),
                EndDate = now.AddDays(80),
                UsageLimit = null,
                UsageLimitPerUser = 1,
                CurrentUsageCount = 0,
                IsActive = true,
                IsPublic = false,
                CreatedAt = now,
                UpdatedAt = now
            },

            // --- Referral Rewards ---
            new Voucher
            {
                Code = "REFERRAL35K",
                Description = "👥 Giới thiệu bạn bè - Giảm 35,000đ",
                DiscountType = DiscountType.FixedAmount,
                DiscountValue = 35000,
                MinOrderValue = 70000,
                MaxDiscountAmount = null,
                StartDate = now.AddDays(-5),
                EndDate = now.AddDays(60),
                UsageLimit = null,
                UsageLimitPerUser = 1,
                CurrentUsageCount = 0,
                IsActive = true,
                IsPublic = false,
                CreatedAt = now,
                UpdatedAt = now
            },

            new Voucher
            {
                Code = "REFERRAL30",
                Description = "🎁 Referral Bonus - Giảm 30%",
                DiscountType = DiscountType.Percentage,
                DiscountValue = 30,
                MinOrderValue = 100000,
                MaxDiscountAmount = 60000,
                StartDate = now.AddDays(-7),
                EndDate = now.AddDays(70),
                UsageLimit = null,
                UsageLimitPerUser = 1,
                CurrentUsageCount = 0,
                IsActive = true,
                IsPublic = false,
                CreatedAt = now,
                UpdatedAt = now
            },

            // --- Thank You / Appreciation Vouchers ---
            new Voucher
            {
                Code = "THANKYOU15K",
                Description = "🙏 Cảm ơn bạn - Giảm 15,000đ",
                DiscountType = DiscountType.FixedAmount,
                DiscountValue = 15000,
                MinOrderValue = 50000,
                MaxDiscountAmount = null,
                StartDate = now.AddDays(-12),
                EndDate = now.AddDays(45),
                UsageLimit = null,
                UsageLimitPerUser = 1,
                CurrentUsageCount = 0,
                IsActive = true,
                IsPublic = false,
                CreatedAt = now,
                UpdatedAt = now
            },

            new Voucher
            {
                Code = "THANKYOU20",
                Description = "💝 Thank You Gift - Giảm 20%",
                DiscountType = DiscountType.Percentage,
                DiscountValue = 20,
                MinOrderValue = 80000,
                MaxDiscountAmount = 40000,
                StartDate = now.AddDays(-8),
                EndDate = now.AddDays(55),
                UsageLimit = null,
                UsageLimitPerUser = 1,
                CurrentUsageCount = 0,
                IsActive = true,
                IsPublic = false,
                CreatedAt = now,
                UpdatedAt = now
            },

            // --- Special Event Private Vouchers ---
            new Voucher
            {
                Code = "SPECIAL70K",
                Description = "✨ Sự kiện đặc biệt - Giảm 70,000đ",
                DiscountType = DiscountType.FixedAmount,
                DiscountValue = 70000,
                MinOrderValue = 200000,
                MaxDiscountAmount = null,
                StartDate = now.AddDays(-3),
                EndDate = now.AddDays(30),
                UsageLimit = null,
                UsageLimitPerUser = 1,
                CurrentUsageCount = 0,
                IsActive = true,
                IsPublic = false,
                CreatedAt = now,
                UpdatedAt = now
            },

            new Voucher
            {
                Code = "EXCLUSIVE50",
                Description = "🌟 Exclusive Member - Giảm 50% (max 150k)",
                DiscountType = DiscountType.Percentage,
                DiscountValue = 50,
                MinOrderValue = 180000,
                MaxDiscountAmount = 150000,
                StartDate = now.AddDays(-15),
                EndDate = now.AddDays(90),
                UsageLimit = null,
                UsageLimitPerUser = 1,
                CurrentUsageCount = 0,
                IsActive = true,
                IsPublic = false,
                CreatedAt = now,
                UpdatedAt = now
            },

            new Voucher
            {
                Code = "LUCKY88K",
                Description = "🍀 May mắn 88 - Giảm 88,000đ",
                DiscountType = DiscountType.FixedAmount,
                DiscountValue = 88000,
                MinOrderValue = 250000,
                MaxDiscountAmount = null,
                StartDate = now.AddDays(-1),
                EndDate = now.AddDays(20),
                UsageLimit = null,
                UsageLimitPerUser = 1,
                CurrentUsageCount = 0,
                IsActive = true,
                IsPublic = false,
                CreatedAt = now,
                UpdatedAt = now
            }
        };

        await context.Vouchers.AddRangeAsync(vouchers);
        await context.SaveChangesAsync();

        // Assign private vouchers to users
        await AssignPrivateVouchersToUsers(context);
    }

    private static async Task AssignPrivateVouchersToUsers(AppDbContext context)
    {
        var users = await context.Users.ToListAsync();
        if (!users.Any()) return;

        var privateVouchers = await context.Vouchers
            .Where(v => !v.IsPublic && v.IsActive)
            .ToListAsync();

        if (!privateVouchers.Any()) return;

        var random = new Random();
        var userVouchers = new List<UserVoucher>();

        // Định nghĩa các nhóm vouchers theo loại
        var birthdayVouchers = privateVouchers.Where(v => v.Code.Contains("BIRTHDAY")).ToList();
        var vipVouchers = privateVouchers.Where(v => v.Code.Contains("VIP")).ToList();
        var loyaltyVouchers = privateVouchers.Where(v => v.Code.Contains("LOYALTY")).ToList();
        var referralVouchers = privateVouchers.Where(v => v.Code.Contains("REFERRAL")).ToList();
        var thankYouVouchers = privateVouchers.Where(v => v.Code.Contains("THANKYOU")).ToList();
        var specialVouchers = privateVouchers.Where(v => v.Code.Contains("SPECIAL") || v.Code.Contains("EXCLUSIVE") || v.Code.Contains("LUCKY")).ToList();

        foreach (var user in users)
        {
            var assignedCount = 0;
            var usedNotes = new HashSet<string>();

            // Mỗi user đều nhận 2 birthday vouchers (100% user)
            foreach (var voucher in birthdayVouchers)
            {
                userVouchers.Add(new UserVoucher
                {
                    UserId = user.Id,
                    VoucherId = voucher.Id,
                    IsUsed = false,
                    AssignedAt = DateTime.UtcNow.AddDays(-random.Next(1, 30)),
                    Note = $"🎂 Quà sinh nhật tháng {DateTime.UtcNow.Month}"
                });
                assignedCount++;
            }

            // Random gán VIP vouchers (70% users nhận 1-2 vouchers)
            if (random.Next(100) < 70)
            {
                var vipCount = random.Next(1, Math.Min(3, vipVouchers.Count + 1));
                var selectedVips = vipVouchers.OrderBy(x => random.Next()).Take(vipCount);
                foreach (var voucher in selectedVips)
                {
                    userVouchers.Add(new UserVoucher
                    {
                        UserId = user.Id,
                        VoucherId = voucher.Id,
                        IsUsed = random.Next(100) < 20, // 20% chance đã sử dụng
                        AssignedAt = DateTime.UtcNow.AddDays(-random.Next(5, 60)),
                        UsedAt = random.Next(100) < 20 ? DateTime.UtcNow.AddDays(-random.Next(1, 10)) : null,
                        Note = "💎 VIP Member Reward"
                    });
                    assignedCount++;
                }
            }

            // Loyalty vouchers (90% users nhận 2-3 vouchers)
            if (random.Next(100) < 90)
            {
                var loyaltyCount = random.Next(2, Math.Min(4, loyaltyVouchers.Count + 1));
                var selectedLoyalty = loyaltyVouchers.OrderBy(x => random.Next()).Take(loyaltyCount);
                foreach (var voucher in selectedLoyalty)
                {
                    userVouchers.Add(new UserVoucher
                    {
                        UserId = user.Id,
                        VoucherId = voucher.Id,
                        IsUsed = random.Next(100) < 15, // 15% chance đã sử dụng
                        AssignedAt = DateTime.UtcNow.AddDays(-random.Next(10, 90)),
                        UsedAt = random.Next(100) < 15 ? DateTime.UtcNow.AddDays(-random.Next(1, 20)) : null,
                        Note = "❤️ Khách hàng thân thiết"
                    });
                    assignedCount++;
                }
            }

            // Referral vouchers (50% users nhận 1-2 vouchers)
            if (random.Next(100) < 50)
            {
                var referralCount = random.Next(1, Math.Min(3, referralVouchers.Count + 1));
                var selectedReferral = referralVouchers.OrderBy(x => random.Next()).Take(referralCount);
                foreach (var voucher in selectedReferral)
                {
                    userVouchers.Add(new UserVoucher
                    {
                        UserId = user.Id,
                        VoucherId = voucher.Id,
                        IsUsed = random.Next(100) < 10,
                        AssignedAt = DateTime.UtcNow.AddDays(-random.Next(1, 30)),
                        UsedAt = random.Next(100) < 10 ? DateTime.UtcNow.AddDays(-random.Next(1, 15)) : null,
                        Note = "👥 Giới thiệu bạn bè thành công"
                    });
                    assignedCount++;
                }
            }

            // Thank You vouchers (80% users nhận 1-2 vouchers)
            if (random.Next(100) < 80)
            {
                var thankYouCount = random.Next(1, Math.Min(3, thankYouVouchers.Count + 1));
                var selectedThankYou = thankYouVouchers.OrderBy(x => random.Next()).Take(thankYouCount);
                foreach (var voucher in selectedThankYou)
                {
                    userVouchers.Add(new UserVoucher
                    {
                        UserId = user.Id,
                        VoucherId = voucher.Id,
                        IsUsed = random.Next(100) < 5,
                        AssignedAt = DateTime.UtcNow.AddDays(-random.Next(1, 40)),
                        UsedAt = random.Next(100) < 5 ? DateTime.UtcNow.AddDays(-random.Next(1, 10)) : null,
                        Note = "🙏 Cảm ơn bạn đã ủng hộ"
                    });
                    assignedCount++;
                }
            }

            // Special vouchers (30% users nhận 1-2 vouchers - may mắn)
            if (random.Next(100) < 30)
            {
                var specialCount = random.Next(1, Math.Min(3, specialVouchers.Count + 1));
                var selectedSpecial = specialVouchers.OrderBy(x => random.Next()).Take(specialCount);
                foreach (var voucher in selectedSpecial)
                {
                    userVouchers.Add(new UserVoucher
                    {
                        UserId = user.Id,
                        VoucherId = voucher.Id,
                        IsUsed = false, // Special vouchers thường chưa dùng
                        AssignedAt = DateTime.UtcNow.AddDays(-random.Next(1, 15)),
                        Note = "✨ Voucher đặc biệt dành cho bạn"
                    });
                    assignedCount++;
                }
            }

            // Nếu user có ít hơn 15 private vouchers, gán thêm random vouchers
            while (assignedCount < 15 && privateVouchers.Any())
            {
                var remainingVouchers = privateVouchers
                    .Where(pv => !userVouchers.Any(uv => uv.UserId == user.Id && uv.VoucherId == pv.Id))
                    .ToList();

                if (!remainingVouchers.Any()) break;

                var extraVoucher = remainingVouchers[random.Next(remainingVouchers.Count)];
                userVouchers.Add(new UserVoucher
                {
                    UserId = user.Id,
                    VoucherId = extraVoucher.Id,
                    IsUsed = random.Next(100) < 10,
                    AssignedAt = DateTime.UtcNow.AddDays(-random.Next(1, 60)),
                    UsedAt = random.Next(100) < 10 ? DateTime.UtcNow.AddDays(-random.Next(1, 20)) : null,
                    Note = "🎁 Bonus voucher"
                });
                assignedCount++;
            }
        }

        if (userVouchers.Any())
        {
            await context.UserVouchers.AddRangeAsync(userVouchers);
            await context.SaveChangesAsync();
        }
    }
}

