using Microsoft.AspNetCore.Http;

namespace CoffeeShopApi.Services;

public interface IFileUploadService
{
    Task<string> UploadImageAsync(IFormFile image, string folder = "images");
}

public class FileUploadService : IFileUploadService
{
    public async Task<string> UploadImageAsync(IFormFile image, string folder = "images")
    {
        if (image == null || image.Length == 0)
            throw new ArgumentException("Image file is required");

        var ext = Path.GetExtension(image.FileName);
        var fileName = $"product_{Guid.NewGuid()}{ext}";
        var savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folder, fileName);
        
        Directory.CreateDirectory(Path.GetDirectoryName(savePath)!);
        
        using (var stream = new FileStream(savePath, FileMode.Create))
        {
            await image.CopyToAsync(stream);
        }
        
        return $"/{folder}/{fileName}";
    }
}
