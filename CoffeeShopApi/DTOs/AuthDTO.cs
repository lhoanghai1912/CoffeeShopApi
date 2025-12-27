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
    public string Username { get; set; }
    
    
    [DefaultValue("")]
    [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
    [PasswordComplexity]
    public string Password { get; set; }
    [DefaultValue("")]

    public string? FullName { get; set; } 
    [DefaultValue("")]
    public string? PhoneNumber { get; set; }= string.Empty;
    [DefaultValue(1)]

    [JsonIgnore]
    public int? RoleId { get; set; }
}

public class AuthResponse
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string? FullName { get; set; }
    public object? Role { get; set; } 
    public string? PhoneNumber { get; set; } 
    public string Token { get; set; } 
}