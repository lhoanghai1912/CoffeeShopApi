using System.Threading.Tasks;
using CoffeeShopApi.DTOs;
using CoffeeShopApi.Services;
using CoffeeShopApi.Shared;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeShopApi.Controllers;

[Route("api/[controller]")]
[ApiController]

public class RolesController : ControllerBase
{
    private readonly IRoleService _service;

    public RolesController(IRoleService service)
    {
        _service = service;
    }
    
    // [HttpGet]
    // public async Task<IActionResult> GetPaged(
    //     [FromQuery] int page = 1,
    //     [FromQuery] int pageSize = 10,
    //     [FromQuery] string? search = null,
    //     [FromQuery] string? filter = null,
    //     [FromQuery] string? orderBy = null)
    // {
    //     var result = await _service.GetPagedAsync(page, pageSize, search, filter, orderBy);
    //     return Ok(ApiResponse<object>.Ok(result));
    // }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(ApiResponse<object>.Ok(result));
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var role = await _service.GetByIdAsync(id);
        if (role == null) return NotFound();
        return Ok(ApiResponse<object>.Ok(role));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateRoleRequest request)
    {
        var created = await _service.CreateAsync(request);
        return Ok(ApiResponse<object>.Ok(created));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateRoleRequest request)
    {
        var success = await _service.UpdateAsync(id, request);
        if (!success) return NotFound(ApiResponse<object>.NotFound());
        return Ok(ApiResponse<object>.Ok(success, "Cập nhật role thành công"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _service.DeleteAsync(id);
        if (!success) return NotFound(ApiResponse<object>.NotFound());
        return Ok(ApiResponse<object>.Ok(success, "Xóa role thành công"));
    }
}