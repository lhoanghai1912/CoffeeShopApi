using CoffeeShopApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoffeeShopApi.Data.Configuration;

public class OptionGroupConfiguration : IEntityTypeConfiguration<OptionGroup>
{
    public void Configure(EntityTypeBuilder<OptionGroup> builder)
    {
        builder.HasKey(og => og.Id);

        builder.Property(og => og.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasOne(og => og.Product)
            .WithMany(p => p.OptionGroups)
            .HasForeignKey(og => og.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(og => og.ProductId);
    }
}
