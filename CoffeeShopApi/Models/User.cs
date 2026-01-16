using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CoffeeShopApi.Models;

public class User
{
    [Key] 
    public int Id { get; set; }
    
    /// <summary>
    /// Tên đăng nhập - unique, dùng cho Authentication
    /// </summary>
    [Required] 
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;
    
    /// <summary>
    /// Mật khẩu đã hash (BCrypt) - KHÔNG BAO GIỜ trả ra API
    /// </summary>
    [JsonIgnore]
    public string Password { get; set; } = string.Empty;
    
    /// <summary>
    /// Họ tên hiển thị - dùng cho UI và Order
    /// </summary>
    [MaxLength(200)]
    public string? FullName { get; set; }
    
    /// <summary>
    /// Số điện thoại - dùng cho Order và liên hệ
    /// </summary>
    [MaxLength(20)]
    public string? PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Email (optional) - dùng cho thông báo, khôi phục mật khẩu
    /// </summary>
    [MaxLength(200)]
    public string? Email { get; set; }

    /// <summary>
    /// Trạng thái tài khoản: true = Active, false = Inactive (soft delete)
    /// User inactive không thể login nhưng order cũ vẫn giữ
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Thời gian tạo tài khoản
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Thời gian cập nhật thông tin gần nhất
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Thời gian đăng nhập gần nhất - tracking behavior
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    // --- Cấu hình Khóa Ngoại (Foreign Key) ---
    /// <summary>
    /// RoleId - tạm thời hardcode CUSTOMER, chuẩn bị cho phân quyền tương lai
    /// </summary>
    [JsonIgnore]
    public int? RoleId { get; set; } = 2; // Default: CUSTOMER
    public Role? Role { get; set; }
}