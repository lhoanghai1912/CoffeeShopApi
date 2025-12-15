using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoffeeShopApi.Models;

public class User
{
    [Key] public int Id { get; set; }
    [Required] public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;

// --- Cấu hình Khóa Ngoại (Foreign Key) ---
    public int RoleId { get; set; } // FK trỏ về bảng Role

    [ForeignKey("RoleId")]
    public Role Role { get; set; } = null!; // Navigation Property}
}