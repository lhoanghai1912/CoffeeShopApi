using Microsoft.AspNetCore.Http;
namespace CoffeeShopApi.DTOs;

public class ProductFormDataRequest
{
    public string FormField { get; set; } = string.Empty; // JSON object ch?a toàn b? thông tin s?n ph?m
    public IFormFile? Image { get; set; }
}
