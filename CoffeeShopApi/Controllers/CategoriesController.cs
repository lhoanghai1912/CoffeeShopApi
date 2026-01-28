using CoffeeShopApi.DTOs;
using CoffeeShopApi.Services;
using CoffeeShopApi.Shared;
using CoffeeShopApi.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeShopApi.Controllers;

[Route("api/[controller]")]
[ApiController]
// [Authorize] // ✅ Bắt buộc login

public class CategoriesController :  ControllerBase
{
    private readonly ICategoryservice _service;

    public CategoriesController(ICategoryservice service)
    {
        _service = service;
    }

    [HttpGet]
    //[AllowAnonymous] // ✅ Public
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(ApiResponse<object>.Ok(result));
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var role = await _service.GetByIdAsync(id);
        if (role == null) return NotFound(ApiResponse<object>.NotFound());
        return Ok(ApiResponse<object>.Ok(role));
    }

    [HttpPost]
    //[RequirePermission("category.create")] // ✅ ADMIN + STAFF
    public async Task<IActionResult> Create(CreateCategoryRequest request)
    {
        var created = await _service.CreateAsync(request);
        return Ok(ApiResponse<object>.Ok(created));
    }

    [HttpPut("{id}")]
    //[RequirePermission("category.update")] // ✅ ADMIN + STAFF
    public async Task<IActionResult> Update(int id, UpdateCategoryRequest request)
    {
        var success = await _service.UpdateAsync(id, request);
        if (!success) return NotFound(ApiResponse<object>.NotFound());
        return Ok(ApiResponse<object>.Ok(success, "Cập nhật danh mục thành công"));
    }

    [HttpDelete("{id}")]
    //[RequirePermission("category.delete")] // ✅ ADMIN only
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _service.DeleteAsync(id);
        if (!success) return NotFound(ApiResponse<object>.NotFound());
        return Ok(ApiResponse<object>.Ok(success, "Xóa danh mục thành công"));
    }
}