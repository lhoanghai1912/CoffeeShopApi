using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoffeeShopApi.Models;

/// <summary>
/// Chi tiết từng sản phẩm trong đơn hàng
/// Snapshot giá tại thời điểm đặt hàng để tránh thay đổi khi Product/Option thay đổi giá
/// </summary>
public class OrderItem
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Đơn hàng chứa item này
    /// </summary>
    [Required]
    public int OrderId { get; set; }
    public Order? Order { get; set; }

    /// <summary>
    /// Sản phẩm được đặt (reference, không dùng để tính giá)
    /// </summary>
    [Required]
    public int ProductId { get; set; }
    public Product? Product { get; set; }

    /// <summary>
    /// Số lượng
    /// </summary>
    [Required]
    public int Quantity { get; set; } = 1;

    /// <summary>
    /// [SNAPSHOT] Giá gốc của sản phẩm tại thời điểm đặt hàng
    /// </summary>
    [Column(TypeName = "decimal(18,0)")]
    [Required]
    public decimal BasePrice { get; set; } = 0;

    /// <summary>
    /// [SNAPSHOT] Tên sản phẩm tại thời điểm đặt hàng
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// [SNAPSHOT] Ảnh sản phẩm tại thời điểm đặt hàng
    /// </summary>
    public string? ProductImageUrl { get; set; }

    /// <summary>
    /// Tổng giá của các OptionItems được chọn
    /// </summary>
    [Column(TypeName = "decimal(18,0)")]
    public decimal OptionPrice { get; set; } = 0;

    /// <summary>
    /// Đơn giá = BasePrice + OptionPrice
    /// </summary>
    [Column(TypeName = "decimal(18,0)")]
    public decimal UnitPrice { get; set; } = 0;

    /// <summary>
    /// Tổng giá = UnitPrice * Quantity
    /// </summary>
    [Column(TypeName = "decimal(18,0)")]
    public decimal TotalPrice { get; set; } = 0;

    /// <summary>
    /// Ghi chú riêng cho item này (ví dụ: "Ít đá", "Không đường")
    /// </summary>
    [MaxLength(200)]
    public string? Note { get; set; }

    /// <summary>
    /// Thời gian tạo
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Các options đã chọn cho item này
    /// </summary>
    public ICollection<OrderItemOption> OrderItemOptions { get; set; } = new List<OrderItemOption>();
}
