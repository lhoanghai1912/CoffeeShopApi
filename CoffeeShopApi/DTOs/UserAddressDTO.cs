using System.ComponentModel.DataAnnotations;

namespace CoffeeShopApi.DTOs;

#region Request DTOs

/// <summary>
/// Request thêm địa chỉ mới
/// </summary>
public class CreateUserAddressRequest
{
    /// <summary>
    /// Tên người nhận
    /// </summary>
    [Required(ErrorMessage = "Tên người nhận là bắt buộc")]
    [MaxLength(200, ErrorMessage = "Tên người nhận tối đa 200 ký tự")]
    public string RecipientName { get; set; } = string.Empty;

    /// <summary>
    /// Số điện thoại người nhận
    /// </summary>
    [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
    [MaxLength(20, ErrorMessage = "Số điện thoại tối đa 20 ký tự")]
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Địa chỉ đầy đủ
    /// </summary>
    [Required(ErrorMessage = "Địa chỉ là bắt buộc")]
    [MaxLength(500, ErrorMessage = "Địa chỉ tối đa 500 ký tự")]
    public string AddressLine { get; set; } = string.Empty;

    /// <summary>
    /// Nhãn địa chỉ (ví dụ: "Nhà", "Văn phòng")
    /// </summary>
    [MaxLength(50, ErrorMessage = "Nhãn tối đa 50 ký tự")]
    public string? Label { get; set; }

    /// <summary>
    /// Đặt làm địa chỉ mặc định
    /// </summary>
    public bool IsDefault { get; set; } = false;
}

/// <summary>
/// Request cập nhật địa chỉ
/// </summary>
public class UpdateUserAddressRequest
{
    /// <summary>
    /// Tên người nhận
    /// </summary>
    [MaxLength(200, ErrorMessage = "Tên người nhận tối đa 200 ký tự")]
    public string? RecipientName { get; set; }

    /// <summary>
    /// Số điện thoại người nhận
    /// </summary>
    [MaxLength(20, ErrorMessage = "Số điện thoại tối đa 20 ký tự")]
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Địa chỉ đầy đủ
    /// </summary>
    [MaxLength(500, ErrorMessage = "Địa chỉ tối đa 500 ký tự")]
    public string? AddressLine { get; set; }

    /// <summary>
    /// Nhãn địa chỉ
    /// </summary>
    [MaxLength(50, ErrorMessage = "Nhãn tối đa 50 ký tự")]
    public string? Label { get; set; }

    /// <summary>
    /// Đặt làm địa chỉ mặc định
    /// </summary>
    public bool? IsDefault { get; set; }
}

#endregion

#region Response DTOs

/// <summary>
/// Response thông tin địa chỉ
/// </summary>
public class UserAddressResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string RecipientName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string AddressLine { get; set; } = string.Empty;
    public string? Label { get; set; }
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Địa chỉ đầy đủ hiển thị: "RecipientName - PhoneNumber - AddressLine"
    /// </summary>
    public string FullDisplay => $"{RecipientName} - {PhoneNumber} - {AddressLine}";
}

/// <summary>
/// Response danh sách địa chỉ của user
/// </summary>
public class UserAddressListResponse
{
    public List<UserAddressResponse> Addresses { get; set; } = new();
    public int TotalCount { get; set; }
    public int? DefaultAddressId { get; set; }
}

#endregion
