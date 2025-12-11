using System.ComponentModel.DataAnnotations.Schema;

namespace CoffeeShopApi.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    [Column(TypeName = "decimal(18,0)")]
    public decimal Price { get; set; }
    
    public string? ImageUrl { get; set; }
    public string Category { get; set; } = "Coffee";
    public bool IsAvailable { get; set; } = true;
}