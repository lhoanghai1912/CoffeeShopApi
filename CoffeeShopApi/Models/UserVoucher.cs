using System.ComponentModel.DataAnnotations;

namespace CoffeeShopApi.Models
{
    /// <summary>
    /// Junction table for assigning private vouchers to specific users
    /// Used for private vouchers like Birthday gifts, VIP rewards, etc.
    /// </summary>
    public class UserVoucher
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int VoucherId { get; set; }

        /// <summary>
        /// Whether this assigned voucher has been used by the user
        /// </summary>
        public bool IsUsed { get; set; } = false;

        /// <summary>
        /// When this voucher was assigned to the user
        /// </summary>
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When this voucher was used (null if not used yet)
        /// </summary>
        public DateTime? UsedAt { get; set; }

        /// <summary>
        /// Optional note for the assignment (e.g., "Birthday gift", "VIP reward")
        /// </summary>
        [MaxLength(200)]
        public string? Note { get; set; }

        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual Voucher Voucher { get; set; } = null!;
    }
}
