using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CoffeeShopApi.Models;

public class User
{
    [Key] 
    public int Id { get; set; }
    
    [Required] 
    public string Username { get; set; } = string.Empty;
    
    public string Password { get; set; } = string.Empty;
    
    public string? FullName { get; set; }
    
    [DefaultValue("")]
    public string? PhoneNumber { get; set; }= string.Empty;

    // --- Cấu hình Khóa Ngoại (Foreign Key) ---
    [JsonIgnore]
    public int? RoleId { get; set; } = 1;
    public Role? Role { get; set; }
}