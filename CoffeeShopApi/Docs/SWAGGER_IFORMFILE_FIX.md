# ✅ FIXED - Swagger IFormFile Error

## 🔍 Vấn đề

**Error:**
```
Swashbuckle.AspNetCore.SwaggerGen.SwaggerGeneratorException: 
Error reading parameter(s) for action 
CoffeeShopApi.Controllers.UsersController.UploadAvatar (CoffeeShopApi) 
as [FromForm] attribute used with IFormFile.
```

**Nguyên nhân:** 
`[FromForm]` attribute không cần thiết khi sử dụng `IFormFile` parameter. ASP.NET Core tự động bind `IFormFile` từ multipart/form-data.

---

## ✅ Giải pháp

### Fix: Xóa [FromForm] attribute

**File:** `CoffeeShopApi\Controllers\UsersController.cs`

**Trước:**
```csharp
[HttpPost("avatar")]
[Consumes("multipart/form-data")]
public async Task<IActionResult> UploadAvatar([FromForm] IFormFile file)
```

**Sau:**
```csharp
[HttpPost("avatar")]
[Consumes("multipart/form-data")]
public async Task<IActionResult> UploadAvatar(IFormFile file)
```

**Reason:**
- ✅ `IFormFile` tự động bind từ form-data
- ✅ `[Consumes("multipart/form-data")]` đã chỉ định content type
- ✅ Không cần `[FromForm]` explicit

---

## 📝 Swagger Configuration (Alternative)

Nếu muốn giữ `[FromForm]`, cần config Swagger:

**File:** `Program.cs`

```csharp
builder.Services.AddSwaggerGen(options =>
{
    // ... existing config
    
    // ⭐ Support IFormFile with [FromForm]
    options.OperationFilter<FileUploadOperationFilter>();
});

// Filter class
public class FileUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var fileParams = context.MethodInfo
            .GetParameters()
            .Where(p => p.ParameterType == typeof(IFormFile))
            .ToList();

        if (fileParams.Any())
        {
            operation.RequestBody = new OpenApiRequestBody
            {
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Properties = fileParams.ToDictionary(
                                p => p.Name ?? "file",
                                p => new OpenApiSchema
                                {
                                    Type = "string",
                                    Format = "binary"
                                }
                            )
                        }
                    }
                }
            };
        }
    }
}
```

**Nhưng cách đơn giản hơn:** Xóa `[FromForm]` ✅

---

## ✅ Status

**Error:** Fixed ✅

**Changes:**
- [x] Remove `[FromForm]` from `UploadAvatar` method
- [x] Keep `[Consumes("multipart/form-data")]`
- [x] Swagger works correctly

**API works:**
```bash
curl -X POST http://localhost:1912/api/users/avatar \
  -H "Authorization: Bearer <token>" \
  -F "file=@avatar.jpg"
```

---

## 📊 Other IFormFile Endpoints

**Check các endpoints khác:**

```csharp
// ✅ Correct
[HttpPost("upload")]
[Consumes("multipart/form-data")]
public async Task<IActionResult> Upload(IFormFile file) { }

// ❌ Avoid
[HttpPost("upload")]
public async Task<IActionResult> Upload([FromForm] IFormFile file) { }

// ✅ Multiple files
[HttpPost("upload-multiple")]
[Consumes("multipart/form-data")]
public async Task<IActionResult> UploadMultiple(List<IFormFile> files) { }

// ✅ With other form fields
[HttpPost("upload-with-data")]
[Consumes("multipart/form-data")]
public async Task<IActionResult> UploadWithData(
    IFormFile file,
    [FromForm] string description) { }
```

---

## 📖 References

- [Swashbuckle IFormFile Issue](https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/1029)
- [ASP.NET Core File Upload](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads)

---

## ✅ Checklist

- [x] Remove `[FromForm]` attribute
- [x] Keep `[Consumes("multipart/form-data")]`
- [x] Verify Swagger UI works
- [x] Test API endpoint
- [x] Update documentation

**Status:** Production Ready ✅
