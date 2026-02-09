# File Upload Service - Hướng dẫn sử dụng

## Tổng quan

`FileUploadService` đã được refactor theo đúng chuẩn quy định với các cải tiến:

### ✅ Cải tiến chính

1. **Validation đầy đủ**
   - Kiểm tra file size (max 5MB mặc định)
   - Kiểm tra file extension (.jpg, .jpeg, .png, .gif, .webp)
   - Kiểm tra MIME type
   - Phòng chống path traversal attacks

2. **Configuration-driven**
   - Sử dụng `appsettings.json` để cấu hình
   - Dễ dàng thay đổi giới hạn và quy tắc

3. **Dependency Injection**
   - `IWebHostEnvironment` để lấy wwwroot path
   - `ILogger` để ghi log
   - `IOptions<FileUploadSettings>` để đọc cấu hình

4. **Error Handling**
   - Try-catch đầy đủ
   - Logging chi tiết
   - Error messages rõ ràng

5. **Best Practices**
   - Async disposal (`await using`)
   - Resource management đúng chuẩn
   - Security: validate file name, prevent path traversal

6. **Tính năng mới**
   - `ValidateImage()`: Validate trước khi upload
   - `DeleteImageAsync()`: Xóa file cũ

## Cấu hình (appsettings.json)

```json
{
  "FileUploadSettings": {
    "MaxFileSizeMB": 5,
    "AllowedImageExtensions": [ ".jpg", ".jpeg", ".png", ".gif", ".webp" ],
    "AllowedImageMimeTypes": [ "image/jpeg", "image/png", "image/gif", "image/webp" ],
    "UploadFolder": "images"
  }
}
```

### Tham số cấu hình

| Tham số | Mô tả | Mặc định |
|---------|-------|----------|
| `MaxFileSizeMB` | Kích thước file tối đa (MB) | 5 |
| `AllowedImageExtensions` | Các extension được phép | .jpg, .jpeg, .png, .gif, .webp |
| `AllowedImageMimeTypes` | Các MIME type được phép | image/jpeg, image/png, image/gif, image/webp |
| `UploadFolder` | Thư mục upload | images |

## Sử dụng

### 1. Upload file

```csharp
[HttpPost]
public async Task<IActionResult> Create([FromForm] ProductFormDataRequest form)
{
    try
    {
        // Validate image trước khi upload
        if (form.Image != null)
        {
            if (!_fileUploadService.ValidateImage(form.Image, out string imageError))
            {
                return BadRequest(ApiResponse<object>.Fail(imageError));
            }
            
            // Upload image
            var imageUrl = await _fileUploadService.UploadImageAsync(form.Image);
            request.ImageUrl = imageUrl;
        }

        // ... xử lý tiếp
    }
    catch (InvalidOperationException ex)
    {
        return StatusCode(500, ApiResponse<object>.Fail($"File upload error: {ex.Message}"));
    }
}
```

### 2. Xóa file cũ

```csharp
// Xóa ảnh cũ khi update
if (!string.IsNullOrEmpty(oldImageUrl))
{
    await _fileUploadService.DeleteImageAsync(oldImageUrl);
}
```

### 3. Upload vào thư mục khác

```csharp
// Upload vào thư mục "products"
var imageUrl = await _fileUploadService.UploadImageAsync(form.Image, "products");
```

## Validation Messages

Service trả về các error messages sau:

| Error | Mô tả |
|-------|-------|
| "Image file is required" | File rỗng hoặc null |
| "File size exceeds maximum allowed size of {X}MB" | File quá lớn |
| "File extension '{ext}' is not allowed..." | Extension không hợp lệ |
| "File type '{type}' is not allowed..." | MIME type không hợp lệ |
| "Invalid file name. Path traversal detected" | File name chứa ký tự nguy hiểm |

## Logging

Service ghi log các sự kiện:

```
[Information] Uploading image: photo.jpg to D:\...\wwwroot\images
[Information] Image uploaded successfully: /images/product_guid.jpg
[Warning] Image validation failed: File size exceeds maximum allowed size of 5MB
[Error] Error uploading image: photo.jpg
```

## Security

### Path Traversal Prevention

Service kiểm tra file name để phòng chống path traversal attacks:

```csharp
// ❌ KHÔNG cho phép
"../../../etc/passwd"
"..\\..\\windows\\system32\\config"
"images/../secret.txt"

// ✅ Cho phép
"photo.jpg"
"IMG_20230101.png"
```

### File Extension Validation

Chỉ cho phép upload các file ảnh:

```csharp
// ❌ KHÔNG cho phép
"malware.exe"
"script.php"
"shell.asp"

// ✅ Cho phép
"photo.jpg"
"image.png"
"avatar.webp"
```

## React Native Client

### Gửi đúng multipart/form-data

```typescript
const formData = new FormData();
formData.append('FormField', JSON.stringify(productData));
formData.append('Image', {
  uri: imageAsset.uri,
  type: imageAsset.type || 'image/jpeg',
  name: imageAsset.fileName || 'photo.jpg',
} as any);

const response = await fetch(`${apiUrl}/api/Products`, {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Accept': 'application/json',
    // KHÔNG set Content-Type - để fetch tự động set
  },
  body: formData,
});
```

### Debug tip

```typescript
// Log FormData để kiểm tra
console.log('FormData _parts:', (formData as any)._parts);
// Phải có dạng:
// [
//   ["FormField", "{...}"],
//   ["Image", {uri, type, name}]
// ]
```

## Troubleshooting

### Vấn đề: imageUrl luôn rỗng

**Nguyên nhân**: FE không gửi file thực tế, chỉ gửi thông tin file (uri, name, type)

**Giải pháp**: Đảm bảo gửi FormData đúng chuẩn multipart/form-data, không stringify

### Vấn đề: Network request failed

**Nguyên nhân**: 
- Server không chạy
- Android chặn HTTP (cần `usesCleartextTraffic="true"`)
- Header Content-Type bị set thủ công sai

**Giải pháp**: 
1. Kiểm tra server đang chạy
2. Thêm vào `AndroidManifest.xml`:
```xml
<application
    android:usesCleartextTraffic="true"
    ...>
```
3. KHÔNG set thủ công header `Content-Type`

### Vấn đề: File size exceeds maximum

**Giải pháp**: Tăng `MaxFileSizeMB` trong appsettings.json hoặc resize ảnh trước khi upload

### Vấn đề: File extension not allowed

**Giải pháp**: Thêm extension vào `AllowedImageExtensions` trong appsettings.json

## Testing với curl

```bash
# Gửi file thực tế (phải có file trên máy)
curl -X POST \
  -H "Authorization: Bearer <token>" \
  -F "FormField={\"name\":\"Test\",\"basePrice\":100,...}" \
  -F "Image=@D:\path\to\image.jpg;type=image/jpeg" \
  http://localhost:1912/api/Products
```

**Lưu ý**: Không thể test upload file từ React Native emulator bằng curl, vì curl chạy trên máy host, không truy cập được file trong emulator.
