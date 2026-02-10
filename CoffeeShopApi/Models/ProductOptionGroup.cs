using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

    /// <summary>
    /// Danh sách ID của OptionItems được phép hiển thị (JSON array)
    /// NULL = lấy tất cả items trong group
    /// [1,2] = chỉ lấy items có ID 1 và 2
    /// </summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? AllowedItemIdsJson { get; set; }

    /// <summary>
    /// Helper property để làm việc với AllowedItemIds
    /// </summary>
    [NotMapped]
    public List<int>? AllowedItemIds
    {
        get
        {
            if (string.IsNullOrWhiteSpace(AllowedItemIdsJson))
                return null;
            return System.Text.Json.JsonSerializer.Deserialize<List<int>>(AllowedItemIdsJson);
        }
        set
        {
            AllowedItemIdsJson = value == null || !value.Any()
                ? null
                : System.Text.Json.JsonSerializer.Serialize(value);
        }
    }
}

