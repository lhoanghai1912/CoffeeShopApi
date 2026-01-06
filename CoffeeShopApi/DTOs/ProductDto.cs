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

    // He thong OptionGroups
    public List<CreateOptionGroupRequest>? OptionGroups { get; set; }

    // KHONG co IFormFile, KHONG co [FromForm]
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


