using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoffeeShopApi.Models
{
    public enum DiscountType
    {
        FixedAmount = 1,
        Percentage = 2
    }

    public class Voucher
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Description { get; set; }

        [Required]
        public DiscountType DiscountType { get; set; }

        /// <summary>
        /// For FixedAmount: The exact discount value (e.g., 2.00 means -$2)
        /// For Percentage: The percentage value (e.g., 10 means 10%)
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountValue { get; set; }

        /// <summary>
        /// Minimum order subtotal required to use this voucher
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? MinOrderValue { get; set; }

        /// <summary>
        /// Maximum discount amount (used for Percentage type to cap the discount)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? MaxDiscountAmount { get; set; }

        /// <summary>
        /// Voucher valid from this date
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Voucher valid until this date
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Maximum total times this voucher can be used system-wide (null = unlimited)
        /// </summary>
        public int? UsageLimit { get; set; }

        /// <summary>
        /// Maximum times a single user can use this voucher (null = unlimited)
        /// </summary>
        public int? UsageLimitPerUser { get; set; }

        /// <summary>
        /// Current count of how many times this voucher has been used
        /// </summary>
        public int CurrentUsageCount { get; set; } = 0;

        /// <summary>
        /// Whether the voucher is active (can be manually disabled)
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// True = Public voucher (anyone can use with code)
        /// False = Private voucher (only assigned users can use)
        /// </summary>
        public bool IsPublic { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<VoucherUsage> VoucherUsages { get; set; } = new List<VoucherUsage>();
        public virtual ICollection<UserVoucher> UserVouchers { get; set; } = new List<UserVoucher>();
    }
}
