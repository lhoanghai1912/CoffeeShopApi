using CoffeeShopApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoffeeShopApi.Data.Configuration;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.OrderCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(o => o.OrderCode)
            .IsUnique();

        builder.Property(o => o.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(o => o.SubTotal)
            .HasColumnType("decimal(18,0)")
            .HasDefaultValue(0);

        builder.Property(o => o.DiscountAmount)
            .HasColumnType("decimal(18,0)")
            .HasDefaultValue(0);

        builder.Property(o => o.ShippingFee)
            .HasColumnType("decimal(18,0)")
            .HasDefaultValue(0);

        builder.Property(o => o.FinalAmount)
            .HasColumnType("decimal(18,0)")
            .HasDefaultValue(0);

        builder.Property(o => o.Note)
            .HasMaxLength(500);

        builder.Property(o => o.ShippingAddress)
            .HasMaxLength(500);

        builder.Property(o => o.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(o => o.CancelReason)
            .HasMaxLength(500);

        // Relationship với User
        builder.HasOne(o => o.User)
            .WithMany()
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        // Relationship với OrderItems
        builder.HasMany(o => o.OrderItems)
            .WithOne(oi => oi.Order)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Index cho tìm kiếm
        builder.HasIndex(o => o.UserId);
        builder.HasIndex(o => o.Status);
        builder.HasIndex(o => o.CreatedAt);
    }
}
