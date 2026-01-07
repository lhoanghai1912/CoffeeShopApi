using CoffeeShopApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoffeeShopApi.Data.Configuration;

public class OptionItemConfiguration : IEntityTypeConfiguration<OptionItem>
{
    public void Configure(EntityTypeBuilder<OptionItem> builder)
    {
        builder.HasKey(oi => oi.Id);

        builder.Property(oi => oi.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(oi => oi.PriceAdjustment)
            .HasColumnType("decimal(18,0)")
            .IsRequired();

        builder.HasOne(oi => oi.OptionGroup)
            .WithMany(og => og.OptionItems)
            .HasForeignKey(oi => oi.OptionGroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(oi => oi.OptionGroupId);

        builder.Property(oi => oi.FatherId)
            .IsRequired();
    }
}
