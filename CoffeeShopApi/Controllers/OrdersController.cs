using CoffeeShopApi.DTOs;
using CoffeeShopApi.Models.Enums;
using CoffeeShopApi.Services;
using CoffeeShopApi.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeShopApi.Controllers;

[Route("api/[controller]")]
[ApiController]
 //[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    #region Query Endpoints

    /// <summary>
    /// Lấy tất cả đơn hàng
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _orderService.GetAllAsync();
        return Ok(ApiResponse<object>.Ok(result));
    }

    [HttpGet("Paged")]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? orderBy = null,
        [FromQuery] string? filter = null)
    {
        var result = await _orderService.GetPagedAsync(page, pageSize, search, orderBy, filter);
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>
    /// Lấy đơn hàng theo ID
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _orderService.GetByIdAsync(id);
        if (result == null)
            return NotFound(ApiResponse<object>.NotFound("Không tìm thấy đơn hàng"));
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>
    /// Lấy đơn hàng theo mã đơn
    /// </summary>
    [HttpGet("code/{orderCode}")]
    public async Task<IActionResult> GetByCode(string orderCode)
    {
        var result = await _orderService.GetByCodeAsync(orderCode);
        if (result == null)
            return NotFound(ApiResponse<object>.NotFound("Không tìm thấy đơn hàng"));
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>
    /// Lấy đơn hàng của user
    /// </summary>
    [HttpGet("user/{userId:int}")]
    public async Task<IActionResult> GetByUserId(int userId)
    {
        var result = await _orderService.GetByUserIdAsync(userId);
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>
    /// Lấy đơn hàng theo trạng thái
    /// </summary>
    [HttpGet("status/{status}")]
    public async Task<IActionResult> GetByStatus(OrderStatus status)
    {
        var result = await _orderService.GetByStatusAsync(status);
        return Ok(ApiResponse<object>.Ok(result));
    }

    #endregion

    #region Command Endpoints

    /// <summary>
    /// Tạo đơn hàng mới
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request)
    {
        try
        {
            var result = await _orderService.CreateOrderAsync(request);
            return Ok(ApiResponse<object>.Ok(result, "Tạo đơn hàng thành công"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.Fail($"Lỗi hệ thống: {ex.Message}"));
        }
    }

    /// <summary>
    /// Thêm sản phẩm vào đơn hàng
    /// </summary>
    [HttpPost("{orderId:int}/items")]
    public async Task<IActionResult> AddItem(int orderId, [FromBody] CreateOrderItemRequest request)
    {
        try
        {
            var result = await _orderService.AddOrderItemAsync(orderId, request);
            return Ok(ApiResponse<object>.Ok(result, "Thêm sản phẩm thành công"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Cập nhật sản phẩm trong đơn hàng
    /// </summary>
    [HttpPut("{orderId:int}/items/{itemId:int}")]
    public async Task<IActionResult> UpdateItem(int orderId, int itemId, [FromBody] UpdateOrderItemRequest request)
    {
        try
        {
            var result = await _orderService.UpdateOrderItemAsync(orderId, itemId, request);
            return Ok(ApiResponse<object>.Ok(result, "Cập nhật sản phẩm thành công"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Xóa sản phẩm khỏi đơn hàng
    /// </summary>
    [HttpDelete("{orderId:int}/items/{itemId:int}")]
    public async Task<IActionResult> RemoveItem(int orderId, int itemId)
    {
        try
        {
            var result = await _orderService.RemoveOrderItemAsync(orderId, itemId);
            return Ok(ApiResponse<object>.Ok(result, "Xóa sản phẩm thành công"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Cập nhật thông tin đơn hàng (ghi chú, địa chỉ, SĐT)
    /// </summary>
    [HttpPut("{orderId:int}")]
    public async Task<IActionResult> Update(int orderId, [FromBody] UpdateOrderRequest request)
    {
        try
        {
            var result = await _orderService.UpdateOrderAsync(orderId, request);
            return Ok(ApiResponse<object>.Ok(result, "Cập nhật đơn hàng thành công"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Checkout đơn hàng (Draft -> Pending)
    /// </summary>
    [HttpPost("{orderId:int}/checkout")]
    public async Task<IActionResult> Checkout(int orderId, [FromBody] CheckoutOrderRequest request)
    {
        try
        {
            var result = await _orderService.CheckoutOrderAsync(orderId, request);
            return Ok(ApiResponse<object>.Ok(result, "Đặt hàng thành công"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Preview voucher cho đơn hàng (trước khi checkout)
    /// Dùng để hiển thị thông tin giảm giá khi nhập mã voucher
    /// </summary>
    [HttpPost("{orderId:int}/preview-voucher")]
    public async Task<IActionResult> PreviewVoucher(int orderId, [FromBody] PreviewVoucherRequest request)
    {
        try
        {
            // TODO: Lấy userId từ JWT token thay vì hardcode
            // var userId = GetCurrentUserId();
            var order = await _orderService.GetByIdAsync(orderId);
            if (order == null)
                return NotFound(ApiResponse<object>.NotFound("Không tìm thấy đơn hàng"));

            if (!order.UserId.HasValue)
                return BadRequest(ApiResponse<object>.Fail("Đơn hàng phải có UserId để preview voucher"));

            var result = await _orderService.PreviewVoucherAsync(orderId, request.VoucherCode, order.UserId.Value);
            return Ok(ApiResponse<object>.Ok(result));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Xác nhận đơn hàng (Pending -> Confirmed)
    /// </summary>
    [HttpPost("{orderId:int}/confirm")]
    public async Task<IActionResult> Confirm(int orderId)
    {
        try
        {
            var result = await _orderService.ConfirmOrderAsync(orderId);
            return Ok(ApiResponse<object>.Ok(result, "Xác nhận đơn hàng thành công"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Đánh dấu đã thanh toán (Pending/Confirmed -> Paid)
    /// </summary>
    [HttpPost("{orderId:int}/pay")]
    public async Task<IActionResult> MarkAsPaid(int orderId)
    {
        try
        {
            var result = await _orderService.MarkAsPaidAsync(orderId);
            return Ok(ApiResponse<object>.Ok(result, "Thanh toán thành công"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Hủy đơn hàng
    /// </summary>
    [HttpPost("{orderId:int}/cancel")]
    public async Task<IActionResult> Cancel(int orderId, [FromBody] CancelOrderRequest request)
    {
        try
        {
            var result = await _orderService.CancelOrderAsync(orderId, request);
            return Ok(ApiResponse<object>.Ok(result, "Hủy đơn hàng thành công"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Xóa đơn hàng (chỉ Draft hoặc Cancelled)
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var success = await _orderService.DeleteOrderAsync(id);
            if (!success)
                return NotFound(ApiResponse<object>.NotFound("Không tìm thấy đơn hàng"));
            return Ok(ApiResponse<object>.Ok(null, "Xóa đơn hàng thành công"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    #endregion
}
