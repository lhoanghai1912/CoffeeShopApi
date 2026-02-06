using System.ComponentModel.DataAnnotations;

namespace CoffeeShopApi.Models;

/// <summary>
/// Bảng mapping nhiều-nhiều giữa Product và OptionGroup (template)
/// Cho phép một OptionGroup được dùng lại cho nhiều Product
/// </summary>
public class ProductOptionGroup
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int ProductId { get; set; }
    public Product? Product { get; set; }

    [Required]
    public int OptionGroupId { get; set; }
    public OptionGroup? OptionGroup { get; set; }

    /// <summary>
    /// Thứ tự hiển thị của group trong product này
    /// (có thể khác với DisplayOrder mặc định của template)
    /// </summary>
    public int DisplayOrder { get; set; } = 0;
}
