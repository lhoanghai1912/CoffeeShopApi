using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoffeeShopApi.Models;

/// <summary>
/// Địa chỉ giao hàng của user - hỗ trợ quản lý nhiều địa chỉ
/// </summary>
public class UserAddress
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// User sở hữu địa chỉ này
    /// </summary>
    [Required]
    public int UserId { get; set; }
    public User? User { get; set; }

    /// <summary>
    /// Tên người nhận (ví dụ: "Nguyễn Văn A")
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string RecipientName { get; set; } = string.Empty;

    /// <summary>
    /// Số điện thoại người nhận
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Địa chỉ đầy đủ (ví dụ: "123 Đường ABC, Phường XYZ, Quận 1, TP.HCM")
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string AddressLine { get; set; } = string.Empty;

    /// <summary>
    /// Nhãn địa chỉ (ví dụ: "Nhà", "Văn phòng", "Công ty")
    /// </summary>
    [MaxLength(50)]
    public string? Label { get; set; }

    /// <summary>
    /// Đánh dấu địa chỉ mặc định - chỉ có 1 địa chỉ default cho mỗi user
    /// </summary>
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// Thời gian tạo
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Thời gian cập nhật gần nhất
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
