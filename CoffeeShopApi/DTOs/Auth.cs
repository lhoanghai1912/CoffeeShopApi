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
    public string FullName { get; set; } 
    public string? PhoneNumber { get; set; }
    public int? RoleId { get; set; } // FK trỏ về bảng Role
}

public class AuthResponse
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string? FullName { get; set; }
    public int? RoleId { get; set; }
    public string? PhoneNumber { get; set; } 
    public string Token { get; set; } 
}