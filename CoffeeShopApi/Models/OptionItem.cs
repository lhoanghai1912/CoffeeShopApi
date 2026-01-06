using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoffeeShopApi.Models;

public class OptionItem
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int OptionGroupId { get; set; }
    public OptionGroup? OptionGroup { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,0)")]
    [Required]
    public decimal PriceAdjustment { get; set; } = 0;

    public bool IsDefault { get; set; } = false;

    public int DisplayOrder { get; set; } = 0;
}
