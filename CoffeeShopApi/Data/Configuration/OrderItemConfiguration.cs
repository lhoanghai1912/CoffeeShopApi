using CoffeeShopApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoffeeShopApi.Data.Configuration;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(oi => oi.Id);

        builder.Property(oi => oi.Quantity)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(oi => oi.BasePrice)
            .HasColumnType("decimal(18,0)")
            .IsRequired();

        builder.Property(oi => oi.ProductName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(oi => oi.OptionPrice)
            .HasColumnType("decimal(18,0)")
            .HasDefaultValue(0);

        builder.Property(oi => oi.UnitPrice)
            .HasColumnType("decimal(18,0)")
            .HasDefaultValue(0);

        builder.Property(oi => oi.TotalPrice)
            .HasColumnType("decimal(18,0)")
            .HasDefaultValue(0);

        builder.Property(oi => oi.Note)
            .HasMaxLength(200);

        // Relationship với Order
        builder.HasOne(oi => oi.Order)
            .WithMany(o => o.OrderItems)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship với Product (reference only)
        builder.HasOne(oi => oi.Product)
            .WithMany()
            .HasForeignKey(oi => oi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship với OrderItemOptions
        builder.HasMany(oi => oi.OrderItemOptions)
            .WithOne(oio => oio.OrderItem)
            .HasForeignKey(oio => oio.OrderItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // Index
        builder.HasIndex(oi => oi.OrderId);
        builder.HasIndex(oi => oi.ProductId);
    }
}
