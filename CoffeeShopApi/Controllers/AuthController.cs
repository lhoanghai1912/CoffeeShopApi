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

        var response = await _authService.LoginAsync(request);

        if (response == null)
        {
            return Unauthorized(ApiResponse<AuthResponse>.Fail("Sai tài khoản hoặc mật khẩu"));        }

        // Trả về thẳng object response (đã chứa Token, User Info)
        return Ok(response);
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest register)
    {
        var (user,errorMessage) = await _authService.RegisterAsync(register);
        if (user == null)
        {
            var msg = string.IsNullOrEmpty(errorMessage) ? "Đăng ký thất bại" : errorMessage;
            return BadRequest(ApiResponse<AuthResponse>.Fail(msg));
        }
        return Ok(ApiResponse<object>.Ok());
    }
}