using Microsoft.AspNetCore.Http;
namespace CoffeeShopApi.DTOs;

public class ProductFormDataRequest
{
    public string FormField { get; set; } = string.Empty; // JSON object chứa toàn bộ thông tin của sản phẩm
    public IFormFile? Image { get; set; }
}
