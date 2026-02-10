using System.Collections.Generic;
using System.ComponentModel;

namespace CoffeeShopApi.DTOs;

public class CreateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; } = string.Empty;
    public decimal BasePrice { get; set; } // Gia goc

    // FE gui category id as { "id": 3 } or just categoryId
    public int? CategoryId { get; set; }
    // Or FE gui nested category object { "category": { "id": 3, "name": "..." } }
    public CategoryDto? Category { get; set; }

    // ⭐ Danh sách OptionGroups áp dụng cho Product
    public List<ProductOptionGroupRequest>? OptionGroups { get; set; } = new();

    // KHONG co IFormFile, KHONG co [FromForm]
}

/// <summary>
/// Request để gán OptionGroup cho Product
/// </summary>
public class ProductOptionGroupRequest
{
    /// <summary>
    /// ID của OptionGroup template
    /// </summary>
    public int OptionGroupId { get; set; }

    /// <summary>
    /// Thứ tự hiển thị (optional, mặc định = 0)
    /// </summary>
    public int DisplayOrder { get; set; } = 0;

    /// <summary>
    /// Danh sách ID của OptionItems được phép (optional)
    /// NULL hoặc empty = lấy tất cả items trong group
    /// [1, 2] = chỉ hiển thị items có ID 1 và 2
    /// </summary>
    public List<int>? AllowedItemIds { get; set; }
}

public class CategoryDto
{
    public int? Id { get; set; }
}

public class UpdateProductRequest : CreateProductRequest
{
}

public class ProductResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; } = string.Empty;
    public decimal BasePrice { get; set; } // Gia goc
    public CategoryResponse? Category { get; set; } = new();

    // He thong OptionGroups (thay the ProductDetails)
    public List<OptionGroupDto> OptionGroups { get; set; } = new();
}

