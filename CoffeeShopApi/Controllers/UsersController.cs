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
    private readonly IUserAddressService _userAddressService;
    private readonly IFileUploadService _fileUploadService;

    public UsersController(
        IUserService userService, 
        IUserAddressService userAddressService,
        IFileUploadService fileUploadService)
    {
        _userService = userService;
        _userAddressService = userAddressService;
        _fileUploadService = fileUploadService;
    }

    #region Address Endpoints

    /// <summary>
    /// Lấy danh sách địa chỉ của user hiện tại
    /// </summary>
    [HttpGet("addresses")]
    public async Task<IActionResult> GetMyAddresses()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.Unauthorized("Không xác định được user"));

        var list = await _userAddressService.GetByUserIdAsync(userId.Value);
        return Ok(ApiResponse<object>.Ok(list));
    }

    /// <summary>
    /// Lấy thông tin một địa chỉ của user
    /// </summary>
    [HttpGet("addresses/{addressId:int}")]
    public async Task<IActionResult> GetMyAddress(int addressId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.Unauthorized("Không xác định được user"));

        var addr = await _userAddressService.GetByIdAsync(addressId, userId.Value);
        if (addr == null)
            return NotFound(ApiResponse<object>.NotFound("Không tìm thấy địa chỉ"));

        return Ok(ApiResponse<object>.Ok(addr));
    }

    /// <summary>
    /// Thêm địa chỉ mới cho user hiện tại
    /// </summary>
    [HttpPost("addresses")]
    public async Task<IActionResult> CreateMyAddress([FromBody] CreateUserAddressRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.Unauthorized("Không xác định được user"));

        try
        {
            var created = await _userAddressService.CreateAsync(userId.Value, request);
            return Ok(ApiResponse<object>.Ok(created, "Thêm địa chỉ thành công"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Cập nhật địa chỉ của user hiện tại
    /// </summary>
    [HttpPut("addresses/{addressId:int}")]
    public async Task<IActionResult> UpdateMyAddress(int addressId, [FromBody] UpdateUserAddressRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.Unauthorized("Không xác định được user"));

        try
        {
            var updated = await _userAddressService.UpdateAsync(addressId, userId.Value, request);
            if (updated == null)
                return NotFound(ApiResponse<object>.NotFound("Không tìm thấy địa chỉ"));

            return Ok(ApiResponse<object>.Ok(updated, "Cập nhật địa chỉ thành công"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Đặt một địa chỉ là mặc định
    /// </summary>
    [HttpPost("addresses/{addressId:int}/default")]
    public async Task<IActionResult> SetDefaultAddress(int addressId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.Unauthorized("Không xác định được user"));

        var result = await _userAddressService.SetDefaultAsync(addressId, userId.Value);
        if (result == null)
            return NotFound(ApiResponse<object>.NotFound("Không tìm thấy địa chỉ"));

        return Ok(ApiResponse<object>.Ok(result, "Đặt địa chỉ mặc định thành công"));
    }

    /// <summary>
    /// Xóa địa chỉ của user
    /// </summary>
    [HttpDelete("addresses/{addressId:int}")]
    public async Task<IActionResult> DeleteMyAddress(int addressId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.Unauthorized("Không xác định được user"));

        var success = await _userAddressService.DeleteAsync(addressId, userId.Value);
        if (!success)
            return NotFound(ApiResponse<object>.NotFound("Không tìm thấy địa chỉ"));

        return Ok(ApiResponse<object>.Ok(null, "Xóa địa chỉ thành công"));
    }

    #endregion


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
            return Unauthorized(ApiResponse<object>.Unauthorized("Không xác định được user"));

        var profile = await _userService.GetProfileWithStatsAsync(userId.Value);
        if (profile == null)
            return NotFound(ApiResponse<object>.NotFound("Không tìm thấy user"));

        return Ok(ApiResponse<object>.Ok(profile));
    }

    /// <summary>
    /// Cập nhật thông tin profile của user hiện tại
    /// User KHÔNG được sửa: UserName, Password
    /// </summary>
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.Unauthorized("Không xác định được user"));

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

    /// <summary>
    /// ⭐ Upload avatar cho user hiện tại
    /// </summary>
    [HttpPost("avatar")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadAvatar(IFormFile file)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.Unauthorized("Không xác định được user"));

        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<object>.Fail("Vui lòng chọn file ảnh"));

        try
        {
            // Validate file là ảnh
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    "File không hợp lệ. Chỉ chấp nhận: " + string.Join(", ", allowedExtensions)));
            }

            // Validate kích thước (max 5MB)
            const long maxFileSize = 5 * 1024 * 1024; // 5MB
            if (file.Length > maxFileSize)
            {
                return BadRequest(ApiResponse<object>.Fail("Kích thước file không được vượt quá 5MB"));
            }

            // Get old avatar để xóa (nếu có)
            var user = await _userService.GetByIdAsync(userId.Value);
            var oldAvatarUrl = user?.AvatarUrl;

            // Upload file mới
            var uploadResult = await _fileUploadService.UploadFileAsync(
                file, 
                "avatars", 
                $"user_{userId}");

            if (!uploadResult.Success)
            {
                return BadRequest(ApiResponse<object>.Fail(uploadResult.Message ?? "Upload failed"));
            }

            // Update AvatarUrl trong database
            var updated = await _userService.UpdateAvatarAsync(userId.Value, uploadResult.FileUrl);
            if (!updated)
            {
                // Rollback: Xóa file vừa upload
                await _fileUploadService.DeleteFileAsync(uploadResult.FileUrl);
                return BadRequest(ApiResponse<object>.Fail("Không thể cập nhật avatar"));
            }

            // Xóa avatar cũ (nếu có)
            if (!string.IsNullOrEmpty(oldAvatarUrl))
            {
                await _fileUploadService.DeleteFileAsync(oldAvatarUrl);
            }

            return Ok(ApiResponse<object>.Ok(new
            {
                avatarUrl = uploadResult.FileUrl,
                fileName = uploadResult.FileName,
                fileSize = uploadResult.FileSize
            }, "Upload avatar thành công"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.Fail($"Lỗi khi upload avatar: {ex.Message}"));
        }
    }

    /// <summary>
    /// ⭐ Xóa avatar của user hiện tại
    /// </summary>
    [HttpDelete("avatar")]
    public async Task<IActionResult> DeleteAvatar()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.Unauthorized("Không xác định được user"));

        try
        {
            // Get current avatar
            var user = await _userService.GetByIdAsync(userId.Value);
            if (user == null)
                return NotFound(ApiResponse<object>.NotFound("Không tìm thấy user"));

            if (string.IsNullOrEmpty(user.AvatarUrl))
                return BadRequest(ApiResponse<object>.Fail("User chưa có avatar"));

            var oldAvatarUrl = user.AvatarUrl;

            // Remove avatar from database
            var updated = await _userService.UpdateAvatarAsync(userId.Value, null);
            if (!updated)
                return BadRequest(ApiResponse<object>.Fail("Không thể xóa avatar"));

            // Delete physical file
            await _fileUploadService.DeleteFileAsync(oldAvatarUrl);

            return Ok(ApiResponse<object>.Ok(null, "Xóa avatar thành công"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.Fail($"Lỗi khi xóa avatar: {ex.Message}"));
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
            return Unauthorized(ApiResponse<object>.Unauthorized("Không xác định được user"));

        var (success, message) = await _userService.ChangePasswordAsync(userId.Value, request);

        if (!success)
            return BadRequest(ApiResponse<object>.Fail(message));

        return Ok(ApiResponse<object>.Ok(null, "Đổi mật khẩu thành công"));
    }

    /// <summary>
    /// Quên mật khẩu
    /// </summary>
    
    #endregion

    #region Account Lifecycle Endpoints

    // /// <summary>
    // /// User tự deactivate tài khoản của mình
    // /// </summary>
    // [HttpPost("deactivate")]
    // public async Task<IActionResult> DeactivateMyAccount([FromBody] DeactivateUserRequest? request)
    // {
    //     var userId = GetCurrentUserId();
    //     if (userId == null)
    //         return Unauthorized(ApiResponse<object>.Fail("Không xác định được user"));
    //
    //     var success = await _userService.DeactivateUserAsync(userId.Value, request?.Reason);
    //     if (!success)
    //         return NotFound(ApiResponse<object>.NotFound("Không tìm thấy user"));
    //
    //     return Ok(ApiResponse<object>.Ok(null, "Tài khoản đã được vô hiệu hóa"));
    // }

    #endregion

    #region Admin Endpoints (Future - cần phân quyền)

    // /// <summary>
    // /// [ADMIN] Lấy thông tin profile của user khác
    // /// </summary>
    // [HttpGet("{userId:int}")]
    // // [Authorize(Policy = "RequirePermission:user.view.all")]
    // public async Task<IActionResult> GetUserProfile(int userId)
    // {
    //     var profile = await _userService.GetProfileWithStatsAsync(userId);
    //     if (profile == null)
    //         return NotFound(ApiResponse<object>.NotFound("Không tìm thấy user"));
    //
    //     return Ok(ApiResponse<object>.Ok(profile));
    // }

    /// <summary>
    /// [ADMIN] Deactivate user
    /// </summary>
    // [HttpPost("{userId:int}/deactivate")]
    // // [Authorize(Policy = "RequirePermission:user.delete")]
    // public async Task<IActionResult> DeactivateUser(int userId, [FromBody] DeactivateUserRequest? request)
    // {
    //     var success = await _userService.DeactivateUserAsync(userId, request?.Reason);
    //     if (!success)
    //         return NotFound(ApiResponse<object>.NotFound("Không tìm thấy user"));
    //
    //     return Ok(ApiResponse<object>.Ok(null, "Đã vô hiệu hóa tài khoản"));
    // }

    /// <summary>
    /// [ADMIN] Reactivate user
    /// </summary>
    // [HttpPost("{userId:int}/reactivate")]
    // // [Authorize(Policy = "RequirePermission:user.update.all")]
    // public async Task<IActionResult> ReactivateUser(int userId)
    // {
    //     var success = await _userService.ReactivateUserAsync(userId);
    //     if (!success)
    //         return NotFound(ApiResponse<object>.NotFound("Không tìm thấy user"));
    //
    //     return Ok(ApiResponse<object>.Ok(null, "Đã kích hoạt lại tài khoản"));
    // }

    #endregion
}
