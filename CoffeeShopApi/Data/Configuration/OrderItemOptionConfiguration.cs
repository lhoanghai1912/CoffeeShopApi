using CoffeeShopApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoffeeShopApi.Data.Configuration;

public class OrderItemOptionConfiguration : IEntityTypeConfiguration<OrderItemOption>
{
    public void Configure(EntityTypeBuilder<OrderItemOption> builder)
    {
        builder.HasKey(oio => oio.Id);

        builder.Property(oio => oio.OptionGroupName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(oio => oio.OptionItemName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(oio => oio.PriceAdjustment)
            .HasColumnType("decimal(18,0)")
            .HasDefaultValue(0);

        // Relationship với OrderItem
        builder.HasOne(oio => oio.OrderItem)
            .WithMany(oi => oi.OrderItemOptions)
            .HasForeignKey(oio => oio.OrderItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship với OptionGroup (reference only)
        builder.HasOne(oio => oio.OptionGroup)
            .WithMany()
            .HasForeignKey(oio => oio.OptionGroupId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship với OptionItem (reference only)
        builder.HasOne(oio => oio.OptionItem)
            .WithMany()
            .HasForeignKey(oio => oio.OptionItemId)
            .OnDelete(DeleteBehavior.Restrict);

        // Index
        builder.HasIndex(oio => oio.OrderItemId);
        builder.HasIndex(oio => new { oio.OptionGroupId, oio.OptionItemId });
    }
}
