using System.ComponentModel.DataAnnotations;

namespace CoffeeShopApi.Models;

/// <summary>
/// OptionGroup là template có thể được dùng lại cho nhiều Product
/// Ví dụ: "Kích cỡ", "Mức đường", "Topping" là các template group
/// </summary>
public class OptionGroup
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Mô tả cho admin (VD: "Kích cỡ sản phẩm", "Độ ngọt của đồ uống")
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Nhóm này có bắt buộc chọn không?
    /// </summary>
    [Required]
    public bool IsRequired { get; set; } = false;

    /// <summary>
    /// Cho phép chọn nhiều option trong nhóm này?
    /// </summary>
    [Required]
    public bool AllowMultiple { get; set; } = false;

    /// <summary>
    /// Thứ tự hiển thị mặc định (có thể override trong ProductOptionGroup)
    /// </summary>
    public int DisplayOrder { get; set; } = 0;

    /// <summary>
    /// [DEPENDENCY] Group này chỉ hiển thị khi OptionItem với Id này được chọn
    /// Ví dụ: "Extra shot espresso" chỉ hiện khi chọn "Size L"
    /// Null = luôn hiển thị
    /// </summary>
    public int? DependsOnOptionItemId { get; set; }

    // Navigation properties
    public ICollection<OptionItem> OptionItems { get; set; } = new List<OptionItem>();

    /// <summary>
    /// Danh sách Product sử dụng template này (qua bảng mapping)
    /// </summary>
    public ICollection<ProductOptionGroup> ProductMappings { get; set; } = new List<ProductOptionGroup>();
}
