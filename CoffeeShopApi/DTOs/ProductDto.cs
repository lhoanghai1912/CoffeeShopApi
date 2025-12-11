namespace CoffeeShopApi.DTOs;

public class CreateProductRequest
{
    public string Name { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public string Category { get; set; }
}

public class UpdateProductRequest : CreateProductRequest
{
    public bool IsAvailable { get; set; }
}