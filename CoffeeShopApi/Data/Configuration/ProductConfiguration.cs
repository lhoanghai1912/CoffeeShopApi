using CoffeeShopApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoffeeShopApi.Data.Configuration;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.BasePrice)
            .HasColumnType("decimal(18,0)")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(p => p.Description);

        builder.Property(p => p.ImageUrl);

        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(d => d.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship với OptionGroup qua bảng mapping ProductOptionGroup
        // Được cấu hình trong ProductOptionGroupConfiguration
    }
}
