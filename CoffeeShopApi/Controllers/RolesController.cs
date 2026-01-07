using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CoffeeShopApi.Attributes;
using CoffeeShopApi.DTOs;
using CoffeeShopApi.Services;
using CoffeeShopApi.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeShopApi.Controllers;

[Route("api/[controller]")]
[ApiController]
//[Authorize] // ✅ Bắt buộc phải login
public class RolesController : ControllerBase
{
    private readonly IRoleService _service;

    public RolesController(IRoleService service)
    {
        _service = service;
    }

    [HttpGet]
    //[RequirePermission("role.manage")] // ✅ Chỉ ADMIN xem được danh sách roles
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(ApiResponse<object>.Ok(result));
    }
    
    [HttpGet("{id}")]
    //[RequirePermission("role.manage")] // ✅ Chỉ ADMIN xem được chi tiết role
    public async Task<IActionResult> GetById(int id)
    {
        var role = await _service.GetByIdAsync(id);
        if (role == null) return NotFound(ApiResponse<object>.NotFound());
        return Ok(ApiResponse<object>.Ok(role));
    }

    [HttpPost]
    //[RequirePermission("role.manage")] // ✅ Chỉ ADMIN tạo được role
    public async Task<IActionResult> Create(CreateRoleRequest request)
    {
        var created = await _service.CreateAsync(request);
        return Ok(ApiResponse<object>.Ok(created));
    }

    [HttpPut("{id}")]
    //[RequirePermission("role.manage")] // ✅ Chỉ ADMIN sửa được role
    public async Task<IActionResult> Update(int id, UpdateRoleRequest request)
    {
        var success = await _service.UpdateAsync(id, request);
        if (!success) return NotFound(ApiResponse<object>.NotFound());
        return Ok(ApiResponse<object>.Ok(success, "Cập nhật role thành công"));
    }

    [HttpDelete("{id}")]
    //[RequirePermission("role.manage")] // ✅ Chỉ ADMIN xóa được role
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _service.DeleteAsync(id);
        if (!success) return NotFound(ApiResponse<object>.NotFound());
        return Ok(ApiResponse<object>.Ok(success, "Xóa role thành công"));
    }
}