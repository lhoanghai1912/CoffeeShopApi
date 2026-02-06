using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoffeeShopApi.Models;

/// <summary>
/// OptionItem là một lựa chọn trong OptionGroup template
/// Ví dụ: "Size S", "Size M", "Size L" trong group "Kích cỡ"
/// </summary>
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

    /// <summary>
    /// Giá điều chỉnh khi chọn option này
    /// Dương = tăng giá, Âm = giảm giá, 0 = không đổi
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    [Required]
    public decimal PriceAdjustment { get; set; } = 0;

    /// <summary>
    /// Đây có phải option mặc định không?
    /// Nếu group.IsRequired và user không chọn, sẽ tự động chọn option có IsDefault = true
    /// </summary>
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// Thứ tự hiển thị trong group
    /// </summary>
    public int DisplayOrder { get; set; } = 0;
}
