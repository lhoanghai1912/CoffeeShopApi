using CoffeeShopApi.DTOs;
using CoffeeShopApi.Services;
using CoffeeShopApi.Shared;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeShopApi.Controllers;


[Route("api/[controller]")]
[ApiController]
//[ApiExplorerSettings(IgnoreApi = true)]

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
    public async Task<IActionResult> Create(CreateProductRequest request)
    {
        // validate request fields and collect errors
        var errors = new List<string>();

        if (request.Category != null)
        {
            if (request.Category.Id == null)
                errors.Add("categoryId is required");
            else if (request.Category.Id <= 0)
                errors.Add("categoryId must be a positive integer");
        }

        if (request.ProductDetails != null)
        {
            for (int i = 0; i < request.ProductDetails.Count; i++)
            {
                var d = request.ProductDetails[i];
                if (string.IsNullOrWhiteSpace(d.Size))
                    errors.Add($"productDetails[{i}].size is required");
                if (d.Price <= 0)
                    errors.Add($"productDetails[{i}].price must be greater than 0");
            }
        }

        if (errors.Count > 0)
            return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ", errors));

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
    public async Task<IActionResult> Update(int id, UpdateProductRequest request)
    {
        // validate request fields and collect errors
        var errors = new List<string>();

        if (request.Category != null )
        {
            if (request.Category.Id == null)
                errors.Add("categoryId ");
            else if (request.Category.Id <= 0)
                errors.Add("categoryId must be a positive integer");
        }

        if (request.ProductDetails != null)
        {
            for (int i = 0; i < request.ProductDetails.Count; i++)
            {
                var d = request.ProductDetails[i];
                if (string.IsNullOrWhiteSpace(d.Size))
                    errors.Add($"productDetails[{i}].size is required");
                if (d.Price <= 0)
                    errors.Add($"productDetails[{i}].price must be greater than 0");
            }
        }

        if (errors.Count > 0)
            return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ", errors));

        try
        {
            var success = await _service.UpdateAsync(id, request);
            if (!success) return NotFound(ApiResponse<object>.NotFound());
            return Ok(ApiResponse<object>.Ok(success, "Cập nhật product thành công"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _service.DeleteAsync(id);
        if (!success) return NotFound(ApiResponse<object>.NotFound());
        return Ok(ApiResponse<object>.Ok(success, "Xóa product thành công"));
    }
}       