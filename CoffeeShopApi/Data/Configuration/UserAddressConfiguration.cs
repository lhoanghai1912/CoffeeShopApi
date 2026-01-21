using CoffeeShopApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoffeeShopApi.Data.Configuration;

public class UserAddressConfiguration : IEntityTypeConfiguration<UserAddress>
{
    public void Configure(EntityTypeBuilder<UserAddress> builder)
    {
        builder.HasKey(ua => ua.Id);

        // RecipientName - required
        builder.Property(ua => ua.RecipientName)
            .IsRequired()
            .HasMaxLength(200);

        // PhoneNumber - required
        builder.Property(ua => ua.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);

        // AddressLine - required
        builder.Property(ua => ua.AddressLine)
            .IsRequired()
            .HasMaxLength(500);

        // Label - optional
        builder.Property(ua => ua.Label)
            .HasMaxLength(50);

        // Index on UserId for efficient lookup
        builder.HasIndex(ua => ua.UserId);

        // Composite index for finding default address per user
        builder.HasIndex(ua => new { ua.UserId, ua.IsDefault })
            .HasFilter("[IsDefault] = 1");

        // Relationship with User
        builder.HasOne(ua => ua.User)
            .WithMany(u => u.UserAddresses)
            .HasForeignKey(ua => ua.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
