using CoffeeShopApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoffeeShopApi.Data.Configuration;

public class ProductDetailConfiguration : IEntityTypeConfiguration<ProductDetail>
{
    public void Configure(EntityTypeBuilder<ProductDetail> builder)
    {
        builder.HasKey(pd => pd.Id);

        builder.Property(pd => pd.Size)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(pd => pd.Price)
            .IsRequired()
            .HasColumnType("decimal(18,0)");

        builder.HasOne(pd => pd.Product)
            .WithMany(p => p.ProductDetails)
            .HasForeignKey(pd => pd.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
