using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoffeeShopApi.Models;

/// <summary>
/// Bảng trung gian lưu các voucher được áp dụng cho đơn hàng
/// Hỗ trợ nhiều voucher cho 1 đơn hàng
/// </summary>
public class OrderVoucher
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// ID đơn hàng
    /// </summary>
    [Required]
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    /// <summary>
    /// ID voucher được áp dụng
    /// </summary>
    [Required]
    public int VoucherId { get; set; }
    public Voucher Voucher { get; set; } = null!;

    /// <summary>
    /// [SNAPSHOT] Mã voucher tại thời điểm áp dụng
    /// </summary>
    [MaxLength(50)]
    public string VoucherCode { get; set; } = string.Empty;

    /// <summary>
    /// [SNAPSHOT] Loại giảm giá tại thời điểm áp dụng
    /// </summary>
    public DiscountType DiscountType { get; set; }

    /// <summary>
    /// [SNAPSHOT] Giá trị giảm giá (% hoặc số tiền cố định)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal DiscountValue { get; set; }

    /// <summary>
    /// Số tiền thực tế được giảm từ voucher này
    /// </summary>
    [Column(TypeName = "decimal(18,0)")]
    public decimal DiscountAmount { get; set; }

    /// <summary>
    /// Thứ tự áp dụng voucher (voucher đầu tiên áp dụng trước)
    /// </summary>
    public int ApplyOrder { get; set; } = 0;

    /// <summary>
    /// Thời gian áp dụng voucher
    /// </summary>
    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
}
