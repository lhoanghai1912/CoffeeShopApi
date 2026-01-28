using CoffeeShopApi.DTOs;
using CoffeeShopApi.Services;
using CoffeeShopApi.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CoffeeShopApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class VouchersController : ControllerBase
{
    private readonly IVoucherService _voucherService;

    public VouchersController(IVoucherService voucherService)
    {
        _voucherService = voucherService;
    }

    #region Customer Endpoints

    /// <summary>
    /// [Customer] Validate voucher code trước khi checkout
    /// </summary>
    [HttpPost("validate")]
    [Authorize]
    public async Task<IActionResult> ValidateVoucher([FromBody] ValidateVoucherRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.Unauthorized("Không xác định được user"));

        var result = await _voucherService.ValidateVoucherAsync(
            request.VoucherCode, 
            userId.Value, 
            request.OrderSubTotal);

        if (!result.IsValid)
            return BadRequest(ApiResponse<object>.Fail(result.ErrorMessage ?? "Voucher không hợp lệ"));

        return Ok(ApiResponse<object>.Ok(result, "Voucher hợp lệ"));
    }

    /// <summary>
    /// [Customer] Lấy danh sách voucher được gán cho user hiện tại (private vouchers)
    /// </summary>
    [HttpGet("my-vouchers")]
    [Authorize]
    public async Task<IActionResult> GetMyVouchers([FromQuery] bool? isUsed = null)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.Unauthorized("Không xác định được user"));

        var vouchers = await _voucherService.GetUserVouchersAsync(userId.Value, isUsed);
        return Ok(ApiResponse<object>.Ok(vouchers));
    }

    /// <summary>
    /// [Public] Lấy danh sách voucher công khai đang hoạt động
    /// </summary>
    [HttpGet("active")]
    [AllowAnonymous]
    public async Task<IActionResult> GetActiveVouchers()
    {
        var vouchers = await _voucherService.GetAllAsync(isActive: true);
        
        // Chỉ trả về public vouchers còn hạn và còn lượt sử dụng
        var now = DateTime.UtcNow;
        var availableVouchers = vouchers
            .Where(v => v.IsPublic && 
                       v.EndDate >= now && 
                       (v.RemainingUses == null || v.RemainingUses > 0))
            .ToList();

        return Ok(ApiResponse<object>.Ok(availableVouchers));
    }

    /// <summary>
    /// [Public] Lấy thông tin chi tiết voucher theo code
    /// </summary>
    [HttpGet("code/{code}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetVoucherByCode(string code)
    {
        var voucher = await _voucherService.GetByCodeAsync(code);
        if (voucher == null)
            return NotFound(ApiResponse<object>.NotFound("Không tìm thấy voucher"));

        var response = VoucherResponse.FromEntity(voucher);
        return Ok(ApiResponse<object>.Ok(response));
    }

    #endregion

    #region Admin Endpoints

    /// <summary>
    /// [Admin] Lấy danh sách vouchers với phân trang (bao gồm inactive)
    /// </summary>
    [HttpGet("Paged")]
    //[Authorize] // TODO: Add [Authorize(Policy = "RequirePermission:voucher.view.all")]
    public async Task<IActionResult> GetVouchersPaged(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10, 
        [FromQuery] bool? isActive = null,
        [FromQuery] string? search = null,
        [FromQuery] bool? isPublic = null)
    {
        var pagedResult = await _voucherService.GetPagedAsync(page, pageSize, isActive, search, isPublic);
        return Ok(ApiResponse<object>.Ok(pagedResult));
    }

    /// <summary>
    /// [Admin] Lấy tất cả vouchers không phân trang (cho dropdown/select)
    /// </summary>
    [HttpGet]
    //[Authorize] // TODO: Add [Authorize(Policy = "RequirePermission:voucher.view.all")]
    public async Task<IActionResult> GetAllVouchers([FromQuery] bool? isActive = null)
    {
        var vouchers = await _voucherService.GetAllAsync(isActive);
        return Ok(ApiResponse<object>.Ok(vouchers));
    }

    /// <summary>
    /// [Admin] Lấy thông tin chi tiết voucher theo ID
    /// </summary>
    [HttpGet("{id:int}")]
    //[Authorize] // TODO: Add permission check
    public async Task<IActionResult> GetVoucherById(int id)
    {
        var voucher = await _voucherService.GetByIdAsync(id);
        if (voucher == null)
            return NotFound(ApiResponse<object>.NotFound("Không tìm thấy voucher"));

        var response = VoucherResponse.FromEntity(voucher);
        return Ok(ApiResponse<object>.Ok(response));
    }

    /// <summary>
    /// [Admin] Tạo voucher mới
    /// </summary>
    [HttpPost]
    //[Authorize] // TODO: Add [Authorize(Policy = "RequirePermission:voucher.create")]
    public async Task<IActionResult> CreateVoucher([FromBody] CreateVoucherRequest request)
    {
        try
        {
            var voucher = await _voucherService.CreateAsync(request);
            return Ok(ApiResponse<object>.Ok(voucher, "Tạo voucher thành công"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// [Admin] Cập nhật thông tin voucher
    /// </summary>
    [HttpPut("{id:int}")]
    //[Authorize] // TODO: Add [Authorize(Policy = "RequirePermission:voucher.update")]
    public async Task<IActionResult> UpdateVoucher(int id, [FromBody] UpdateVoucherRequest request)
    {
        try
        {
            var voucher = await _voucherService.UpdateAsync(id, request);
            if (voucher == null)
                return NotFound(ApiResponse<object>.NotFound("Không tìm thấy voucher"));

            return Ok(ApiResponse<object>.Ok(voucher, "Cập nhật voucher thành công"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// [Admin] Vô hiệu hóa voucher (soft delete)
    /// </summary>
    [HttpDelete("{id:int}")]
    //[Authorize] // TODO: Add [Authorize(Policy = "RequirePermission:voucher.delete")]
    public async Task<IActionResult> DeleteVoucher(int id)
    {
        var success = await _voucherService.DeleteAsync(id);
        if (!success)
            return NotFound(ApiResponse<object>.NotFound("Không tìm thấy voucher"));

        return Ok(ApiResponse<object>.Ok(null, "Vô hiệu hóa voucher thành công"));
    }

    /// <summary>
    /// [Admin] Gán private voucher cho danh sách users
    /// </summary>
    [HttpPost("assign")]
    //[Authorize] // TODO: Add [Authorize(Policy = "RequirePermission:voucher.assign")]
    public async Task<IActionResult> AssignVoucherToUsers([FromBody] AssignVoucherRequest request)
    {
        try
        {
            var assignedCount = await _voucherService.AssignVoucherToUsersAsync(
                request.VoucherId, 
                request.UserIds, 
                request.Note);

            return Ok(ApiResponse<object>.Ok(
                new { AssignedCount = assignedCount }, 
                $"Đã gán voucher cho {assignedCount} user"));
        }
        catch (ArgumentException ex)
        {
            return NotFound(ApiResponse<object>.NotFound(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// [Admin] Lấy danh sách users được gán một voucher cụ thể
    /// </summary>
    [HttpGet("{id:int}/assignments")]
    //[Authorize] // TODO: Add permission check
    public async Task<IActionResult> GetVoucherAssignments(int id)
    {
        var assignments = await _voucherService.GetVoucherAssignmentsAsync(id);
        return Ok(ApiResponse<object>.Ok(assignments));
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
}
