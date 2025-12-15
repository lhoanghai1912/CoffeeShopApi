namespace CoffeeShopApi.DTOs;

// Class con để hứng dữ liệu size + giá
public class ProductDetailDto
{
    public string Size { get; set; } = "Standard";
    public decimal Price { get; set; }
}

public class CreateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string Category { get; set; } = "Coffee";
    
    // Thay vì 1 giá, giờ nhận 1 danh sách
    public List<ProductDetailDto> Details { get; set; } = new();
}

public class UpdateProductRequest : CreateProductRequest
{
}