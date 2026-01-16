using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CoffeeShopApi.Models.Enums;

namespace CoffeeShopApi.Models;

/// <summary>
/// Đơn hàng - chứa thông tin tổng quan về đơn đặt hàng
/// </summary>
public class Order
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Mã đơn hàng duy nhất (để hiển thị cho khách)
    /// Format: ORD-yyyyMMdd-xxxxx
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string OrderCode { get; set; } = string.Empty;

    /// <summary>
    /// User đặt hàng (có thể null nếu khách vãng lai tại POS)
    /// </summary>
    public int? UserId { get; set; }
    public User? User { get; set; }

    /// <summary>
    /// Trạng thái đơn hàng
    /// </summary>
    [Required]
    public OrderStatus Status { get; set; } = OrderStatus.Draft;

    /// <summary>
    /// Tổng tiền trước giảm giá (sum của tất cả OrderItems)
    /// </summary>
    [Column(TypeName = "decimal(18,0)")]
    public decimal SubTotal { get; set; } = 0;

    /// <summary>
    /// Số tiền được giảm (từ Voucher)
    /// </summary>
    [Column(TypeName = "decimal(18,0)")]
    public decimal DiscountAmount { get; set; } = 0;

    /// <summary>
    /// Phí ship (nếu có)
    /// </summary>
    [Column(TypeName = "decimal(18,0)")]
    public decimal ShippingFee { get; set; } = 0;

    /// <summary>
    /// Tổng tiền sau giảm giá = SubTotal - DiscountAmount + ShippingFee
    /// </summary>
    [Column(TypeName = "decimal(18,0)")]
    public decimal FinalAmount { get; set; } = 0;

    /// <summary>
    /// Voucher áp dụng (nullable, sẽ implement sau)
    /// </summary>
    public int? VoucherId { get; set; }
    // public Voucher? Voucher { get; set; } // Uncomment khi có Voucher entity

    /// <summary>
    /// Ghi chú của khách hàng
    /// </summary>
    [MaxLength(500)]
    public string? Note { get; set; }

    /// <summary>
    /// Địa chỉ giao hàng (nếu có)
    /// </summary>
    [MaxLength(500)]
    public string? ShippingAddress { get; set; }

    /// <summary>
    /// Số điện thoại nhận hàng
    /// </summary>
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Thời gian tạo đơn
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Thời gian cập nhật gần nhất
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Thời gian thanh toán (nếu đã thanh toán)
    /// </summary>
    public DateTime? PaidAt { get; set; }

    /// <summary>
    /// Thời gian hủy (nếu bị hủy)
    /// </summary>
    public DateTime? CancelledAt { get; set; }

    /// <summary>
    /// Lý do hủy đơn
    /// </summary>
    [MaxLength(500)]
    public string? CancelReason { get; set; }

    /// <summary>
    /// Danh sách các items trong đơn hàng
    /// </summary>
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
