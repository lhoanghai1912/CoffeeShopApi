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
        
        builder.HasMany(p => p.OptionGroups)
            .WithOne(og => og.Product)
            .HasForeignKey(og => og.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
