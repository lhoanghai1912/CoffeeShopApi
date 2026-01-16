using CoffeeShopApi.DTOs;
using CoffeeShopApi.Services;
using CoffeeShopApi.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CoffeeShopApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    #region Helper Methods

    /// <summary>
    /// Lấy UserId từ JWT token
    /// </summary>
    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("userId") ?? User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
        {
            return userId;
        }
        return null;
    }

    #endregion

    #region Profile Endpoints

    /// <summary>
    /// Lấy thông tin profile của user hiện tại
    /// </summary>
    [HttpGet("profile")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.Fail("Không xác định được user"));

        var profile = await _userService.GetProfileWithStatsAsync(userId.Value);
        if (profile == null)
            return NotFound(ApiResponse<object>.NotFound("Không tìm thấy user"));

        return Ok(ApiResponse<object>.Ok(profile));
    }

    /// <summary>
    /// Cập nhật thông tin profile của user hiện tại
    /// User KHÔNG được sửa: Username, Password
    /// </summary>
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.Fail("Không xác định được user"));

        try
        {
            var profile = await _userService.UpdateProfileAsync(userId.Value, request);
            if (profile == null)
                return NotFound(ApiResponse<object>.NotFound("Không tìm thấy user"));

            return Ok(ApiResponse<object>.Ok(profile, "Cập nhật thông tin thành công"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    #endregion

    #region Password Endpoints

    /// <summary>
    /// Đổi mật khẩu
    /// </summary>
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.Fail("Không xác định được user"));

        var (success, message) = await _userService.ChangePasswordAsync(userId.Value, request);

        if (!success)
            return BadRequest(ApiResponse<object>.Fail(message));

        return Ok(ApiResponse<object>.Ok(null, message));
    }

    #endregion

    #region Account Lifecycle Endpoints

    /// <summary>
    /// User tự deactivate tài khoản của mình
    /// </summary>
    [HttpPost("deactivate")]
    public async Task<IActionResult> DeactivateMyAccount([FromBody] DeactivateUserRequest? request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.Fail("Không xác định được user"));

        var success = await _userService.DeactivateUserAsync(userId.Value, request?.Reason);
        if (!success)
            return NotFound(ApiResponse<object>.NotFound("Không tìm thấy user"));

        return Ok(ApiResponse<object>.Ok(null, "Tài khoản đã được vô hiệu hóa"));
    }

    #endregion

    #region Admin Endpoints (Future - cần phân quyền)

    /// <summary>
    /// [ADMIN] Lấy thông tin profile của user khác
    /// </summary>
    [HttpGet("{userId:int}")]
    // [Authorize(Policy = "RequirePermission:user.view.all")]
    public async Task<IActionResult> GetUserProfile(int userId)
    {
        var profile = await _userService.GetProfileWithStatsAsync(userId);
        if (profile == null)
            return NotFound(ApiResponse<object>.NotFound("Không tìm thấy user"));

        return Ok(ApiResponse<object>.Ok(profile));
    }

    /// <summary>
    /// [ADMIN] Deactivate user
    /// </summary>
    [HttpPost("{userId:int}/deactivate")]
    // [Authorize(Policy = "RequirePermission:user.delete")]
    public async Task<IActionResult> DeactivateUser(int userId, [FromBody] DeactivateUserRequest? request)
    {
        var success = await _userService.DeactivateUserAsync(userId, request?.Reason);
        if (!success)
            return NotFound(ApiResponse<object>.NotFound("Không tìm thấy user"));

        return Ok(ApiResponse<object>.Ok(null, "Đã vô hiệu hóa tài khoản"));
    }

    /// <summary>
    /// [ADMIN] Reactivate user
    /// </summary>
    [HttpPost("{userId:int}/reactivate")]
    // [Authorize(Policy = "RequirePermission:user.update.all")]
    public async Task<IActionResult> ReactivateUser(int userId)
    {
        var success = await _userService.ReactivateUserAsync(userId);
        if (!success)
            return NotFound(ApiResponse<object>.NotFound("Không tìm thấy user"));

        return Ok(ApiResponse<object>.Ok(null, "Đã kích hoạt lại tài khoản"));
    }

    #endregion
}
