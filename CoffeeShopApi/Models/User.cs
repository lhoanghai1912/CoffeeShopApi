using System.ComponentModel.DataAnnotations;

namespace CoffeeShopApi.Models;

public class User
{
    [Key]
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty; // Lưu ý: Thực tế nên hash password
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = "Customer"; // Admin, Staff, Customer
}