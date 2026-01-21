using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using CoffeeShopApi.Validation;

namespace CoffeeShopApi.DTOs;

#region Response DTOs

/// <summary>
/// Response DTO cho thông tin profile user
/// KHÔNG chứa Password, RoleId, hoặc thông tin nhạy cảm
/// </summary>
public class UserProfileResponse
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    
    /// <summary>
    /// Thống kê đơn hàng của user (optional, load khi cần)
    /// </summary>
    public UserOrderStats? OrderStats { get; set; }
    
    /// <summary>
    /// Danh sách địa chỉ giao hàng của user
    /// </summary>
    public List<UserAddressResponse>? Addresses { get; set; }
}

/// <summary>
/// Thống kê order của user
/// </summary>
public class UserOrderStats
{
    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }
    public int CompletedOrders { get; set; }
    public decimal TotalSpent { get; set; }
}

/// <summary>
/// Response cho danh sách users (admin view - future)
/// </summary>
public class UserListItemResponse
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

#endregion

#region Request DTOs

/// <summary>
/// Request DTO để cập nhật profile
/// User KHÔNG được sửa: Username, Password (qua luồng riêng)
/// </summary>
public class UpdateProfileRequest
{
    [MaxLength(200, ErrorMessage = "Họ tên không được quá 200 ký tự")]
    public string? FullName { get; set; }

    [MaxLength(20, ErrorMessage = "Số điện thoại không được quá 20 ký tự")]
    // [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    public string? PhoneNumber { get; set; }

    [MaxLength(200, ErrorMessage = "Email không được quá 200 ký tự")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string? Email { get; set; }
}

/// <summary>
/// Request DTO để đổi mật khẩu
/// </summary>
public class ChangePasswordRequest
{
    [Required(ErrorMessage = "Mật khẩu cũ là bắt buộc")]
    public string OldPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
    [PasswordComplexity]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc")]
    [Compare("NewPassword", ErrorMessage = "Xác nhận mật khẩu không khớp")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

/// <summary>
/// Request DTO để deactivate user (soft delete)
/// </summary>
// public class DeactivateUserRequest
// {
//     [MaxLength(500, ErrorMessage = "Lý do không được quá 500 ký tự")]
//     public string? Reason { get; set; }
// }

#endregion
