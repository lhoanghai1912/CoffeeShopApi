using CoffeeShopApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoffeeShopApi.Data.Configuration;

public class VoucherConfiguration : IEntityTypeConfiguration<Voucher>
{
    public void Configure(EntityTypeBuilder<Voucher> builder)
    {
        builder.HasKey(v => v.Id);

        // Code - required and unique
        builder.Property(v => v.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(v => v.Code)
            .IsUnique();

        // Description - optional
        builder.Property(v => v.Description)
            .HasMaxLength(200);

        // DiscountType - stored as int
        builder.Property(v => v.DiscountType)
            .IsRequired()
            .HasConversion<int>();

        // DiscountValue - required
        builder.Property(v => v.DiscountValue)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        // MinOrderValue - optional
        builder.Property(v => v.MinOrderValue)
            .HasColumnType("decimal(18,2)");

        // MaxDiscountAmount - optional
        builder.Property(v => v.MaxDiscountAmount)
            .HasColumnType("decimal(18,2)");

        // Dates
        builder.Property(v => v.StartDate)
            .IsRequired();

        builder.Property(v => v.EndDate)
            .IsRequired();

        // Index for active vouchers lookup
        builder.HasIndex(v => v.IsActive);

        // Index for public/private vouchers lookup
        builder.HasIndex(v => v.IsPublic);

        // Index for date range queries
        builder.HasIndex(v => new { v.StartDate, v.EndDate });

        // Relationship with Orders
        builder.HasMany(v => v.Orders)
            .WithOne(o => o.Voucher)
            .HasForeignKey(o => o.VoucherId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class VoucherUsageConfiguration : IEntityTypeConfiguration<VoucherUsage>
{
    public void Configure(EntityTypeBuilder<VoucherUsage> builder)
    {
        builder.HasKey(vu => vu.Id);

        // Composite unique index to prevent duplicate entries per user-voucher pair
        builder.HasIndex(vu => new { vu.VoucherId, vu.UserId })
            .IsUnique();

        // Index for user lookups
        builder.HasIndex(vu => vu.UserId);

        // Relationship with Voucher
        builder.HasOne(vu => vu.Voucher)
            .WithMany(v => v.VoucherUsages)
            .HasForeignKey(vu => vu.VoucherId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship with User
        builder.HasOne(vu => vu.User)
            .WithMany()
            .HasForeignKey(vu => vu.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class UserVoucherConfiguration : IEntityTypeConfiguration<UserVoucher>
{
    public void Configure(EntityTypeBuilder<UserVoucher> builder)
    {
        builder.HasKey(uv => uv.Id);

        // Composite unique index - each user can only be assigned the same voucher once
        builder.HasIndex(uv => new { uv.UserId, uv.VoucherId })
            .IsUnique();

        // Index for user lookups (get all vouchers assigned to a user)
        builder.HasIndex(uv => uv.UserId);

        // Index for voucher lookups (get all users assigned to a voucher)
        builder.HasIndex(uv => uv.VoucherId);

        // Index for unused vouchers lookup
        builder.HasIndex(uv => new { uv.UserId, uv.IsUsed });

        // Note - optional
        builder.Property(uv => uv.Note)
            .HasMaxLength(200);

        // Relationship with User
        builder.HasOne(uv => uv.User)
            .WithMany()
            .HasForeignKey(uv => uv.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship with Voucher
        builder.HasOne(uv => uv.Voucher)
            .WithMany(v => v.UserVouchers)
            .HasForeignKey(uv => uv.VoucherId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
