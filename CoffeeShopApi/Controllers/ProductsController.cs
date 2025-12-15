using CoffeeShopApi.DTOs;

using CoffeeShopApi.Services;
using CoffeeShopApi.Shared;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeShopApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;

    public ProductsController(IProductService service)
    {
        _service = service;
    }

    [HttpGet()]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? filter = null,
        [FromQuery] string? orderBy = null)
    {
        var result = await _service.GetPagedAsync(page, pageSize, search, filter, orderBy);
        return Ok(ApiResponse<object>.Ok(result));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _service.GetByIdAsync(id);
        if (product == null) return NotFound();
        return Ok(ApiResponse<object>.Ok(product));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateProductRequest request)
    {
        var createdProduct = await _service.CreateAsync(request);
        return Ok(ApiResponse<object>.Ok(createdProduct));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateProductRequest request)
    {
        var success = await _service.UpdateAsync(id, request);
        if (!success) return NotFound(ApiResponse<object>.NotFound());
        return Ok(ApiResponse<object>.Ok(success,"Cập nhật sản phẩm thành công"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _service.DeleteAsync(id);
        if (!success) return NotFound(ApiResponse<object>.NotFound());
        return Ok(ApiResponse<object>.Ok(success,"Xóa sản phẩm thành công"));
    }
}