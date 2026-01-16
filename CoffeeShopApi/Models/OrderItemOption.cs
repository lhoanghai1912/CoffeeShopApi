using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoffeeShopApi.Models;

/// <summary>
/// Bảng trung gian lưu các OptionItems đã chọn cho mỗi OrderItem
/// SNAPSHOT toàn bộ thông tin option tại thời điểm đặt hàng
/// 
/// Lý do snapshot thay vì join trực tiếp với OptionItems:
/// 1. OptionItem có thể bị xóa hoặc đổi tên sau khi order tạo
/// 2. PriceAdjustment có thể thay đổi
/// 3. Đảm bảo lịch sử order luôn chính xác như lúc khách đặt
/// </summary>
public class OrderItemOption
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// OrderItem chứa option này
    /// </summary>
    [Required]
    public int OrderItemId { get; set; }
    public OrderItem? OrderItem { get; set; }

    /// <summary>
    /// Reference đến OptionGroup (để truy vấn, không dùng để hiển thị)
    /// </summary>
    [Required]
    public int OptionGroupId { get; set; }
    public OptionGroup? OptionGroup { get; set; }

    /// <summary>
    /// Reference đến OptionItem (để truy vấn, không dùng để hiển thị)
    /// </summary>
    [Required]
    public int OptionItemId { get; set; }
    public OptionItem? OptionItem { get; set; }

    /// <summary>
    /// [SNAPSHOT] Tên nhóm option tại thời điểm đặt hàng
    /// Ví dụ: "Size", "Topping", "Đường"
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string OptionGroupName { get; set; } = string.Empty;

    /// <summary>
    /// [SNAPSHOT] Tên option đã chọn tại thời điểm đặt hàng
    /// Ví dụ: "Size L", "Trân châu đen", "30% đường"
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string OptionItemName { get; set; } = string.Empty;

    /// <summary>
    /// [SNAPSHOT] Giá điều chỉnh tại thời điểm đặt hàng
    /// Có thể âm (giảm giá) hoặc dương (tăng giá)
    /// </summary>
    [Column(TypeName = "decimal(18,0)")]
    [Required]
    public decimal PriceAdjustment { get; set; } = 0;
}
