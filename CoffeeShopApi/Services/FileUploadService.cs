using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using CoffeeShopApi.Settings;

namespace CoffeeShopApi.Services;

public interface IFileUploadService
{
    Task<string> UploadImageAsync(IFormFile image, string? folder = null);
    Task<bool> DeleteImageAsync(string imageUrl);
    bool ValidateImage(IFormFile image, out string errorMessage);
}

public class FileUploadService : IFileUploadService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<FileUploadService> _logger;
    private readonly FileUploadSettings _settings;

    public FileUploadService(
        IWebHostEnvironment environment,
        ILogger<FileUploadService> logger,
        IOptions<FileUploadSettings> settings)
    {
        _environment = environment;
        _logger = logger;
        _settings = settings.Value;
    }

    public bool ValidateImage(IFormFile image, out string errorMessage)
    {
        errorMessage = string.Empty;

        if (image == null || image.Length == 0)
        {
            errorMessage = "Image file is required";
            return false;
        }

        if (image.Length > _settings.MaxFileSizeBytes)
        {
            errorMessage = $"File size exceeds maximum allowed size of {_settings.MaxFileSizeMB}MB";
            return false;
        }

        var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) || !_settings.AllowedImageExtensions.Contains(extension))
        {
            errorMessage = $"File extension '{extension}' is not allowed. Allowed extensions: {string.Join(", ", _settings.AllowedImageExtensions)}";
            return false;
        }

        if (!string.IsNullOrEmpty(image.ContentType) && !_settings.AllowedImageMimeTypes.Contains(image.ContentType.ToLowerInvariant()))
        {
            errorMessage = $"File type '{image.ContentType}' is not allowed. Allowed types: {string.Join(", ", _settings.AllowedImageMimeTypes)}";
            return false;
        }

        var fileName = Path.GetFileName(image.FileName);
        if (fileName.Contains("..") || fileName.Contains("/") || fileName.Contains("\\"))
        {
            errorMessage = "Invalid file name. Path traversal detected";
            return false;
        }

        return true;
    }

    public async Task<string> UploadImageAsync(IFormFile image, string? folder = null)
    {
        try
        {
            if (!ValidateImage(image, out string errorMessage))
            {
                _logger.LogWarning("Image validation failed: {ErrorMessage}", errorMessage);
                throw new ArgumentException(errorMessage);
            }

            folder ??= _settings.UploadFolder;

            var ext = Path.GetExtension(image.FileName).ToLowerInvariant();
            var fileName = $"product_{Guid.NewGuid()}{ext}";

            var uploadPath = Path.Combine(_environment.WebRootPath, folder);
            Directory.CreateDirectory(uploadPath);

            var savePath = Path.Combine(uploadPath, fileName);

            _logger.LogInformation("Uploading image: {FileName} to {SavePath}", image.FileName, savePath);

            await using (var stream = new FileStream(savePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            var imageUrl = $"/{folder}/{fileName}";
            _logger.LogInformation("Image uploaded successfully: {ImageUrl}", imageUrl);

            return imageUrl;
        }
        catch (Exception ex) when (ex is not ArgumentException)
        {
            _logger.LogError(ex, "Error uploading image: {FileName}", image?.FileName);
            throw new InvalidOperationException("An error occurred while uploading the image", ex);
        }
    }

    public async Task<bool> DeleteImageAsync(string imageUrl)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                return false;

            var relativePath = imageUrl.TrimStart('/');
            var filePath = Path.Combine(_environment.WebRootPath, relativePath);

            if (File.Exists(filePath))
            {
                await Task.Run(() => File.Delete(filePath));
                _logger.LogInformation("Image deleted successfully: {FilePath}", filePath);
                return true;
            }

            _logger.LogWarning("Image not found for deletion: {FilePath}", filePath);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting image: {ImageUrl}", imageUrl);
            return false;
        }
    }
}
