using CoffeeShopApi.DTOs;
using CoffeeShopApi.Services;
using CoffeeShopApi.Shared;
using CoffeeShopApi.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeShopApi.Controllers;


[Route("api/[controller]")]
[ApiController]
[Authorize] // ✅ Bắt buộc phải login

public class ProductsController : ControllerBase
{
    private readonly IProductService _service;

    public ProductsController(IProductService service)
    {
        _service = service;
    }

    [HttpGet("Paged")]
    public async Task<IActionResult> GetAllPaginated(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? orderBy = null,
        [FromQuery] string? filter = null)
    {
        var result = await _service.GetPagedAsync(page, pageSize, search, orderBy, filter);
        return Ok(ApiResponse<object>.Ok(result));
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(ApiResponse<object>.Ok(result));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null) return NotFound(ApiResponse<object>.NotFound());
        return Ok(ApiResponse<object>.Ok(result));
    }
    
    [HttpPost]
    public async Task<IActionResult> Create([FromForm] ProductFormDataRequest form)
    {
        // Kiểm tra dữ liệu nhận được
        if (string.IsNullOrWhiteSpace(form.FormField))
            return BadRequest("FormField is empty!");

        // Parse JSON
        CreateProductRequest? request;
        try
        {
            request = System.Text.Json.JsonSerializer.Deserialize<CreateProductRequest>(form.FormField);
        }
        catch (Exception ex)
        {
            // Log lỗi ex.Message nếu cần
            return BadRequest("JSON parse error: " + ex.Message);
        }
        if (request == null)
            return BadRequest("Cannot parse FormField to CreateProductRequest!");

        // Handle image upload
        if (form.Image != null && form.Image.Length > 0)
        {
            var ext = Path.GetExtension(form.Image.FileName);
            var fileName = $"product_{Guid.NewGuid()}{ext}";
            var savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(savePath)!);
            using (var stream = new FileStream(savePath, FileMode.Create))
            {
                await form.Image.CopyToAsync(stream);
            }
            request.ImageUrl = $"/images/{fileName}";
        }

        // validate request fields and collect errors
        var errors = new List<string>();
        if (request.Category != null)
        {
            if (request.Category.Id == null)
                errors.Add("categoryId is required");
            else if (request.Category.Id <= 0)
                errors.Add("categoryId must be a positive integer");
        }
        if (request.OptionGroups != null)
        {
            for (int i = 0; i < request.OptionGroups.Count; i++)
            {
                var og = request.OptionGroups[i];
                if (string.IsNullOrWhiteSpace(og.Name))
                    errors.Add($"optionGroups[{i}].name is required");
                if (og.OptionItems != null)
                {
                    for (int j = 0; j < og.OptionItems.Count; j++)
                    {
                        var oi = og.OptionItems[j];
                        if (string.IsNullOrWhiteSpace(oi.Name))
                            errors.Add($"optionGroups[{i}].optionItems[{j}].name is required");
                    }
                }
            }
        }
        if (errors.Count > 0)
            return BadRequest(ApiResponse<object>.Fail("Du lieu khong hop le", errors));

        try
        {
            var created = await _service.CreateAsync(request);
            return Ok(ApiResponse<object>.Ok(created));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    [HttpPut("{id}")]
    //[RequirePermission("product.update")]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> Update(int id, [FromForm] ProductFormDataRequest form)
    {
        // Parse formField to UpdateProductRequest
        var options = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        var request = System.Text.Json.JsonSerializer.Deserialize<UpdateProductRequest>(form.FormField, options);
        if (request == null)
            return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ"));

        Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(request));


        // Handle image upload
        if (form.Image != null && form.Image.Length > 0)
        {
            var ext = Path.GetExtension(form.Image.FileName);
            var fileName = $"product_{Guid.NewGuid()}{ext}";
            var savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(savePath)!);
            using (var stream = new FileStream(savePath, FileMode.Create))
            {
                await form.Image.CopyToAsync(stream);
            }
            request.ImageUrl = $"/images/{fileName}";
        }

        // validate request fields and collect errors
        var errors = new List<string>();
        if (request.Category != null)
        {
            if (request.Category.Id == null)
                errors.Add("categoryId is required");
            else if (request.Category.Id <= 0)
                errors.Add("categoryId must be a positive integer");
        }
        if (request.OptionGroups != null)
        {
            for (int i = 0; i < request.OptionGroups.Count; i++)
            {
                var og = request.OptionGroups[i];
                if (string.IsNullOrWhiteSpace(og.Name))
                    errors.Add($"optionGroups[{i}].name is required");
                if (og.OptionItems != null)
                {
                    for (int j = 0; j < og.OptionItems.Count; j++)
                    {
                        var oi = og.OptionItems[j];
                        if (string.IsNullOrWhiteSpace(oi.Name))
                            errors.Add($"optionGroups[{i}].optionItems[{j}].name is required");
                    }
                }
            }
        }
        if (errors.Count > 0)
            return BadRequest(ApiResponse<object>.Fail("Du lieu khong hop le", errors));

        try
        {
            var success = await _service.UpdateAsync(id, request);
            if (!success) return NotFound(ApiResponse<object>.NotFound());
            return Ok(ApiResponse<object>.Ok(success, "Cap nhat product thanh cong"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    [HttpDelete("{id}")]
    //[RequirePermission("product.delete")] // Chi ADMIN duoc xoa
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _service.DeleteAsync(id);
        if (!success) return NotFound(ApiResponse<object>.NotFound());
        return Ok(ApiResponse<object>.Ok(success, "Xoa product thanh cong"));
    }
}