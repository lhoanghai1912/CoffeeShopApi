using System.ComponentModel.DataAnnotations;

namespace CoffeeShopApi.Models;

public class Product
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty; // VD: Cà phê sữa đá

    public string? Description { get; set; } // Mô tả chung

    public string? ImageUrl { get; set; }

    public string Category { get; set; } = "Coffee";

    // Mối quan hệ: Một sản phẩm có nhiều chi tiết (size/biến thể)
    public ICollection<ProductDetail> ProductDetails { get; set; } = new List<ProductDetail>();
}