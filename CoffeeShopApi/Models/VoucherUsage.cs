using System.ComponentModel.DataAnnotations;

namespace CoffeeShopApi.Models
{
    /// <summary>
    /// Tracks voucher usage per user for enforcing UsageLimitPerUser
    /// </summary>
    public class VoucherUsage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int VoucherId { get; set; }

        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// Number of times this user has used this voucher
        /// </summary>
        public int UsageCount { get; set; } = 0;

        /// <summary>
        /// Last time this user used this voucher
        /// </summary>
        public DateTime LastUsedAt { get; set; } = DateTime.UtcNow;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Voucher Voucher { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}
