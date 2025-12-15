using System.ComponentModel.DataAnnotations;

namespace CoffeeShopApi.DTOs;

// DTO cho response
public class ProductResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; } = string.Empty;
    public string Category { get; set; }  = string.Empty;
    public List<ProductDetailResponse> ProductDetails { get; set; } = new();
}

public class ProductDetailResponse
{
    public int Id { get; set; }
    public string Size { get; set; } = "Standard";
    public decimal Price { get; set; }
}

// Class con để hứng dữ liệu size + giá
public class ProductDetailDto
{
    public string Size { get; set; } = "Standard";
    public decimal Price { get; set; }
}

public class CreateProductRequest
{
    [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; } = string.Empty;

    // [Url(ErrorMessage = "URL hình ảnh không hợp lệ")]
    public string? ImageUrl { get; set; } = string.Empty;

    [Required(ErrorMessage = "Danh mục không được để trống")]
    public string Category { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phải có ít nhất 1 chi tiết giá")]
    [MinLength(1)]
    public List<ProductDetailDto> ProductDetails { get; set; } = new();
}

public class UpdateProductRequest : CreateProductRequest
{
}