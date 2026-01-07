using System.ComponentModel.DataAnnotations;

namespace CoffeeShopApi.Models;

public class OptionGroup
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int ProductId { get; set; }
    public Product? Product { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public bool IsRequired { get; set; } = false;

    [Required]
    public bool AllowMultiple { get; set; } = false;

    public int DisplayOrder { get; set; } = 0;

    public ICollection<OptionItem> OptionItems { get; set; } = new List<OptionItem>();

    // FatherId: song song v?i ProductId
    public int FatherId { get; set; }
}
