using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CoffeeShopApi.Shared;
using CoffeeShopApi.Validation;

namespace CoffeeShopApi.DTOs;

public class LoginRequest
{
    [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
    [DefaultValue("admin")]
    public string Username { get; set; }

    [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
    [DefaultValue("Abcd@1234")]
    
    public string Password { get; set; }
}

public class RegisterRequest
{
    [DefaultValue("")]
    [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
    public string Username { get; set; } = string.Empty;
    
    [DefaultValue("")]
    [Required(ErrorMessage = "Email là bắt buộc")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string Email { get; set; } = string.Empty;
    
    [DefaultValue("")]
    [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
    [PasswordComplexity]
    public string Password { get; set; } = string.Empty;

    [DefaultValue("")]
    public string? FullName { get; set; } 
    
    [DefaultValue("")]
    public string? PhoneNumber { get; set; } = string.Empty;

    [JsonIgnore]
    public int? RoleId { get; set; }
}

/// <summary>
/// Response sau khi đăng ký - yêu cầu verify email
/// </summary>
public class RegisterResponse
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool RequiresEmailVerification { get; set; } = false;
    
    /// <summary>
    /// Verification code - CHỈ TRẢ TRONG MÔI TRƯỜNG DEV/TEST
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? VerificationCode { get; set; }
    public int ExpiresInMinutes { get; set; }
    /// <summary>
    /// JWT token returned after successful registration (used for auto-login)
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Token { get; set; }
}

public class AuthResponse
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string? FullName { get; set; }

    public string? Email { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Role { get; set; } 
    public string? PhoneNumber { get; set; } 
    public string Token { get; set; } 
}

#region Forgot Password DTOs

/// <summary>
/// Request quên mật khẩu - gửi email/username để nhận reset token
/// </summary>
public class ForgotPasswordRequest
{
    /// <summary>
    /// Email hoặc Username của tài khoản cần reset password
    /// </summary>
    [Required(ErrorMessage = "Email hoặc Username là bắt buộc")]
    public string EmailOrUsername { get; set; } = string.Empty;
}

/// <summary>
/// Response sau khi request forgot password
/// </summary>
public class ForgotPasswordResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Reset token - CHỈ TRẢ TRONG MÔI TRƯỜNG DEV/TEST
    /// Production sẽ gửi qua email và không trả về
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ResetToken { get; set; }
    
    /// <summary>
    /// Thời gian hết hạn của token (phút)
    /// </summary>
    public int ExpiresInMinutes { get; set; }
}

/// <summary>
/// Request reset password với token
/// </summary>
public class ResetPasswordRequest
{
    /// <summary>
    /// Email hoặc Username của tài khoản
    /// </summary>
    [Required(ErrorMessage = "Email hoặc Username là bắt buộc")]
    public string EmailOrUsername { get; set; } = string.Empty;

    /// <summary>
    /// Token nhận được từ email/forgot-password response
    /// </summary>
    [Required(ErrorMessage = "Reset token là bắt buộc")]
    public string ResetToken { get; set; } = string.Empty;

    /// <summary>
    /// Mật khẩu mới
    /// </summary>
    [Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
    [PasswordComplexity]
    public string NewPassword { get; set; } = string.Empty;

    /// <summary>
    /// Xác nhận mật khẩu mới
    /// </summary>
    [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc")]
    [Compare("NewPassword", ErrorMessage = "Xác nhận mật khẩu không khớp")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

/// <summary>
/// Response sau khi reset password
/// </summary>
public class ResetPasswordResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

#endregion

#region Email Verification DTOs

/// <summary>
/// Request xác thực email với mã 6 số
/// </summary>
public class VerifyEmailRequest
{
    /// <summary>
    /// Email cần xác thực
    /// </summary>
    [Required(ErrorMessage = "Email là bắt buộc")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Mã xác thực 6 số
    /// </summary>
    [Required(ErrorMessage = "Mã xác thực là bắt buộc")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã xác thực phải có 6 ký tự")]
    public string VerificationCode { get; set; } = string.Empty;
}

/// <summary>
/// Response sau khi verify email
/// </summary>

// public class VerifyEmailResponse
// {
//     public bool Success { get; set; }
//     public string Message { get; set; } = string.Empty;
//     
//     /// <summary>
//     /// Nếu verify thành công, trả về token để auto-login
//     /// </summary>
//     public string? Token { get; set; }
// }

/// <summary>
/// Request gửi lại mã xác thực email
/// </summary>
// public class ResendVerificationRequest
// {
//     /// <summary>
//     /// Email cần gửi lại mã
//     /// </summary>
//     [Required(ErrorMessage = "Email là bắt buộc")]
//     [EmailAddress(ErrorMessage = "Email không hợp lệ")]
//     public string Email { get; set; } = string.Empty;
// }

/// <summary>
/// Response sau khi resend verification
/// </summary>
// public class ResendVerificationResponse
// {
//     public bool Success { get; set; }
//     public string Message { get; set; } = string.Empty;
//     
//     /// <summary>
//     /// Verification code - CHỈ TRẢ TRONG MÔI TRƯỜNG DEV/TEST
//     /// </summary>
//     [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
//     public string? VerificationCode { get; set; }
//     public int ExpiresInMinutes { get; set; }
// }

#endregion