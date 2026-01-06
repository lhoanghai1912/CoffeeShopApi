using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoffeeShopApi.Models;

public class Product
{
    [Key] public int Id { get; set; }

    [Required] [MaxLength(100)] public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    // Gia goc (cho cau hinh co ban nhat)
    [Column(TypeName = "decimal(18,0)")]
    [Required]
    public decimal BasePrice { get; set; } = 0;

    // Category
    public int? CategoryId { get; set; } = 1;
    public Category? Category { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // He thong tuy chon (thay the ProductDetail)
    public ICollection<OptionGroup> OptionGroups { get; set; } = new List<OptionGroup>();
}
