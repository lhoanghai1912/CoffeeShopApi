using CoffeeShopApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoffeeShopApi.Data.Configuration;

public class ProductOptionGroupConfiguration : IEntityTypeConfiguration<ProductOptionGroup>
{
    public void Configure(EntityTypeBuilder<ProductOptionGroup> builder)
    {
        builder.HasKey(pog => pog.Id);

        // Relationship: ProductOptionGroup -> Product
        builder.HasOne(pog => pog.Product)
            .WithMany(p => p.ProductOptionGroups)
            .HasForeignKey(pog => pog.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship: ProductOptionGroup -> OptionGroup
        builder.HasOne(pog => pog.OptionGroup)
            .WithMany(og => og.ProductMappings)
            .HasForeignKey(pog => pog.OptionGroupId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(pog => pog.ProductId);
        builder.HasIndex(pog => pog.OptionGroupId);
        
        // Unique constraint: mỗi product chỉ có thể link 1 lần với 1 option group
        builder.HasIndex(pog => new { pog.ProductId, pog.OptionGroupId }).IsUnique();
    }
}
