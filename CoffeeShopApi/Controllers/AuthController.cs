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

    /// <summary>
    /// Đăng nhập
    /// </summary>
    /// <remarks>
    /// User phải đã xác thực email trước khi đăng nhập.
    /// </remarks>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        if (result == null) return Unauthorized(ApiResponse<object>.Fail("Sai tài khoản hoặc mật khẩu, hoặc email chưa được xác thực"));
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>
    /// Đăng ký tài khoản mới
    /// </summary>
    /// <remarks>
    /// Sau khi đăng ký, hệ thống sẽ gửi mã xác thực 6 số đến email.
    /// User cần xác thực email trước khi có thể đăng nhập.
    /// 
    /// Trong môi trường Development, response sẽ chứa VerificationCode để test.
    /// </remarks>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        
        if (result.UserId == 0)
            return BadRequest(ApiResponse<RegisterResponse>.Fail(result.Message));

        return Ok(ApiResponse<RegisterResponse>.Ok(result, result.Message));
    }

    #region Email Verification Endpoints
    //
    // /// <summary>
    // /// Xác thực email với mã 6 số
    // /// </summary>
    // /// <remarks>
    // /// Mã có hiệu lực 15 phút.
    // /// Sau khi xác thực thành công, tài khoản sẽ được kích hoạt và trả về token để auto-login.
    // /// </remarks>
    // [HttpPost("verify-email")]
    // public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
    // {
    //     var result = await _authService.VerifyEmailAsync(request);
    //     
    //     if (!result.Success)
    //         return BadRequest(ApiResponse<VerifyEmailResponse>.Fail(result.Message));
    //
    //     return Ok(ApiResponse<VerifyEmailResponse>.Ok(result, result.Message));
    // }
    //
    // /// <summary>
    // /// Gửi lại mã xác thực email
    // /// </summary>
    // /// <remarks>
    // /// Rate limit: Tối đa 5 lần/ngày mỗi tài khoản.
    // /// Mã mới có hiệu lực 15 phút.
    // /// </remarks>
    // [HttpPost("resend-verification")]
    // public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationRequest request)
    // {
    //     var result = await _authService.ResendVerificationCodeAsync(request);
    //     
    //     if (!result.Success)
    //         return BadRequest(ApiResponse<ResendVerificationResponse>.Fail(result.Message));
    //
    //     return Ok(ApiResponse<ResendVerificationResponse>.Ok(result, result.Message));
    // }

    #endregion

    #region Forgot Password Endpoints

    /// <summary>
    /// Yêu cầu reset password - gửi mã 6 số qua email
    /// </summary>
    /// <remarks>
    /// Trong môi trường Development, response sẽ chứa ResetToken để test.
    /// Production sẽ gửi mã qua email.
    /// 
    /// Rate limit: Tối đa 5 lần/ngày mỗi tài khoản.
    /// Yêu cầu: Email phải đã được xác thực.
    /// </remarks>
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var result = await _authService.ForgotPasswordAsync(request);
        
        if (!result.Success)
            return BadRequest(ApiResponse<ForgotPasswordResponse>.Fail(result.Message));

        return Ok(ApiResponse<ForgotPasswordResponse>.Ok(result, result.Message));
    }

    /// <summary>
    /// Reset password với mã 6 số
    /// </summary>
    /// <remarks>
    /// Mã có hiệu lực 30 phút kể từ khi yêu cầu.
    /// Sau khi reset thành công, mã sẽ bị vô hiệu hóa.
    /// </remarks>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var result = await _authService.ResetPasswordAsync(request);
        
        if (!result.Success)
            return BadRequest(ApiResponse<ResetPasswordResponse>.Fail(result.Message));

        return Ok(ApiResponse<ResetPasswordResponse>.Ok(result, result.Message));
    }

    /// <summary>
    /// Kiểm tra reset code có hợp lệ không
    /// </summary>
    /// <remarks>
    /// Dùng để validate trước khi hiển thị form reset password.
    /// </remarks>
    [HttpPost("validate-reset-token")]
    public async Task<IActionResult> ValidateResetToken([FromBody] ValidateResetTokenRequest request)
    {
        var isValid = await _authService.ValidateResetTokenAsync(request.EmailOrUsername, request.ResetToken);
        
        if (!isValid)
            return BadRequest(ApiResponse<object>.Fail("Mã không hợp lệ hoặc đã hết hạn"));

        return Ok(ApiResponse<object>.Ok(new { isValid = true }, "Mã hợp lệ"));
    }

    #endregion
}

/// <summary>
/// Request để validate reset token
/// </summary>
public class ValidateResetTokenRequest
{
    public string EmailOrUsername { get; set; } = string.Empty;
    public string ResetToken { get; set; } = string.Empty;
}