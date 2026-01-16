using CoffeeShopApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoffeeShopApi.Data.Configuration;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        // Username - unique, indexed for login lookup
        builder.Property(u => u.Username)
            .IsRequired()
            .HasMaxLength(100);
        builder.HasIndex(u => u.Username)
            .IsUnique();

        // Password - required, không có index (hash không cần search)
        builder.Property(u => u.Password)
            .IsRequired();

        // FullName - optional, for display
        builder.Property(u => u.FullName)
            .HasMaxLength(200);

        // PhoneNumber - optional, indexed for order lookup
        builder.Property(u => u.PhoneNumber)
            .HasMaxLength(20);
        builder.HasIndex(u => u.PhoneNumber);

        // Email - optional, unique if provided
        builder.Property(u => u.Email)
            .HasMaxLength(200);
        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasFilter("[Email] IS NOT NULL");

        // IsActive - indexed for filtering active users
        builder.Property(u => u.IsActive)
            .HasDefaultValue(true);
        builder.HasIndex(u => u.IsActive);

        // Timestamps
        builder.Property(u => u.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(u => u.UpdatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // LastLoginAt - nullable
        builder.Property(u => u.LastLoginAt);

        // Role relationship
        builder.HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
