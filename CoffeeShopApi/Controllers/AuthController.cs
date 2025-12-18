using System.Threading.Tasks;
using CoffeeShopApi.DTOs;
using CoffeeShopApi.Services;
using CoffeeShopApi.Shared;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeShopApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        if (result == null) return Unauthorized(ApiResponse<object>.Fail("Sai tài khoản hoặc mật khẩu"));
        return Ok(ApiResponse<object>.Ok(result));
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var user = await _authService.RegisterAsync(request);
        if (user == null) return BadRequest(ApiResponse<object>.Fail("Tài khoản đã tồn tại"));
        var data = new { message = "Đăng ký thành công", userId = user.Id };
        return Ok(ApiResponse<object>.Ok(data));
    }
}