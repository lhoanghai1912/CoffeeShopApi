using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using CoffeeShopApi.Settings;

namespace CoffeeShopApi.Services;

public interface IFileUploadService
{
    Task<string> UploadImageAsync(IFormFile image, string? folder = null);
    Task<bool> DeleteImageAsync(string imageUrl);
    bool ValidateImage(IFormFile image, out string errorMessage);

    /// <summary>
    /// ⭐ Upload file với custom options
    /// </summary>
    Task<FileUploadResult> UploadFileAsync(IFormFile file, string folder, string? customFileName = null);

    /// <summary>
    /// ⭐ Xóa file theo URL
    /// </summary>
    Task<bool> DeleteFileAsync(string fileUrl);
}

/// <summary>
/// ⭐ Result object cho file upload
/// </summary>
public class FileUploadResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
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

    /// <summary>
    /// ⭐ Upload file với custom options (cho avatar, documents, etc.)
    /// </summary>
    public async Task<FileUploadResult> UploadFileAsync(IFormFile file, string folder, string? customFileName = null)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return new FileUploadResult
                {
                    Success = false,
                    Message = "File is required"
                };
            }

            // Validate file size
            if (file.Length > _settings.MaxFileSizeBytes)
            {
                return new FileUploadResult
                {
                    Success = false,
                    Message = $"File size exceeds maximum allowed size of {_settings.MaxFileSizeMB}MB"
                };
            }

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            // Generate filename
            var fileName = string.IsNullOrEmpty(customFileName)
                ? $"{Guid.NewGuid()}{ext}"
                : $"{customFileName}_{DateTime.UtcNow:yyyyMMdd}_{Guid.NewGuid().ToString("N")[..8]}{ext}";

            // Create upload directory
            var uploadPath = Path.Combine(_environment.WebRootPath, _settings.UploadFolder, folder);
            Directory.CreateDirectory(uploadPath);

            var savePath = Path.Combine(uploadPath, fileName);

            _logger.LogInformation("Uploading file: {FileName} to {SavePath}", file.FileName, savePath);

            // Save file
            await using (var stream = new FileStream(savePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var fileUrl = $"/{_settings.UploadFolder}/{folder}/{fileName}";
            _logger.LogInformation("File uploaded successfully: {FileUrl}", fileUrl);

            return new FileUploadResult
            {
                Success = true,
                Message = "File uploaded successfully",
                FileUrl = fileUrl,
                FileName = fileName,
                FileSize = file.Length
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file: {FileName}", file?.FileName);
            return new FileUploadResult
            {
                Success = false,
                Message = $"Error uploading file: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// ⭐ Xóa file theo URL (alias cho DeleteImageAsync)
    /// </summary>
    public async Task<bool> DeleteFileAsync(string fileUrl)
    {
        return await DeleteImageAsync(fileUrl);
    }
}
