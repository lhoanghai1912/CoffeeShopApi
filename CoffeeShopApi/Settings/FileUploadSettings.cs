namespace CoffeeShopApi.Settings;

public class FileUploadSettings
{
    public int MaxFileSizeMB { get; set; } = 5;
    public string[] AllowedImageExtensions { get; set; } = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    public string[] AllowedImageMimeTypes { get; set; } = { "image/jpeg", "image/png", "image/gif", "image/webp" };
    public string UploadFolder { get; set; } = "images";
    
    public long MaxFileSizeBytes => MaxFileSizeMB * 1024 * 1024;
}
