using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using CoffeeShopApi.Shared;

namespace CoffeeShopApi.DTOs;

public class LoginRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
}

public class RegisterRequest
{
    public string Username { get; set; }
    
    [ValidPassword]
    public string Password { get; set; }
    public string? FullName { get; set; } 
    [DefaultValue("")]
    public string? PhoneNumber { get; set; }= string.Empty;
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