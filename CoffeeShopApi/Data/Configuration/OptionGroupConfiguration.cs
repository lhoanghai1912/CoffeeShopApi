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

        builder.Property(og => og.Description)
            .HasMaxLength(500);

        // DependsOnOptionItemId là nullable (không có FK constraint để tránh circular reference)
        builder.Property(og => og.DependsOnOptionItemId)
            .IsRequired(false);

        // Index cho Name để tìm kiếm nhanh
        builder.HasIndex(og => og.Name);
    }
}
