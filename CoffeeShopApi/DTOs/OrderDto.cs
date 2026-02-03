using CoffeeShopApi.Models.Enums;

namespace CoffeeShopApi.DTOs;

#region Request DTOs

/// <summary>
/// Request tạo đơn hàng mới
/// </summary>
public class CreateOrderRequest
{
    public int? UserId { get; set; }

    /// <summary>
    /// ID của địa chỉ giao hàng (UserAddress)
    /// Nếu có, backend sẽ tự động snapshot thông tin từ UserAddress vào Order
    /// </summary>
    public int? UserAddressId { get; set; }

    public string? Note { get; set; }
    public List<CreateOrderItemRequest>? Items { get; set; }
}

/// <summary>
/// Request thêm item vào đơn hàng
/// </summary>
public class CreateOrderItemRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; } = 1;
    public string? Note { get; set; }
    
    /// <summary>
    /// Danh sách các OptionItem IDs được chọn
    /// </summary>
    public List<int>? SelectedOptionItemIds { get; set; }
}

/// <summary>
/// Request cập nhật item trong đơn hàng
/// </summary>
public class UpdateOrderItemRequest
{
    public int Quantity { get; set; } = 1;
    public string? Note { get; set; }
    
    /// <summary>
    /// Danh sách các OptionItem IDs được chọn (thay thế hoàn toàn)
    /// </summary>
    public List<int>? SelectedOptionItemIds { get; set; }
}

/// <summary>
/// Request cập nhật thông tin đơn hàng
/// </summary>
public class UpdateOrderRequest
{
    /// <summary>
    /// ID của địa chỉ giao hàng (UserAddress)
    /// Nếu có, backend sẽ tự động snapshot thông tin từ UserAddress vào Order
    /// </summary>
    public int? UserAddressId { get; set; }

    public string? Note { get; set; }
}

/// <summary>
/// Request checkout đơn hàng
/// </summary>
public class CheckoutOrderRequest
{
    /// <summary>
    /// ID của địa chỉ giao hàng (UserAddress) - bắt buộc cho checkout
    /// Địa chỉ này sẽ được snapshot vào Order
    /// </summary>
    public int? UserAddressId { get; set; }

    /// <summary>
    /// Voucher áp dụng (optional)
    /// </summary>
    public int? VoucherId { get; set; }

    /// <summary>
    /// Ghi chú đơn hàng (optional)
    /// </summary>
    public string? Note { get; set; }
}

/// <summary>
/// Request hủy đơn hàng
/// </summary>
public class CancelOrderRequest
{
    public string? Reason { get; set; }
}

#endregion

#region Response DTOs

/// <summary>
/// Response đơn hàng
/// </summary>
public class OrderResponse
{
    public int Id { get; set; }
    public string OrderCode { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public string? UserName { get; set; }
    public OrderStatus Status { get; set; }
    public string StatusText => GetStatusText(Status);
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal FinalAmount { get; set; }
    public int? VoucherId { get; set; }
    public string? Note { get; set; }
    
    /// <summary>
    /// [SNAPSHOT] Tên người nhận hàng
    /// </summary>
    public string? RecipientName { get; set; }
    
    /// <summary>
    /// [SNAPSHOT] Địa chỉ giao hàng
    /// </summary>
    public string? ShippingAddress { get; set; }
    
    /// <summary>
    /// [SNAPSHOT] Số điện thoại nhận hàng
    /// </summary>
    public string? PhoneNumber { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancelReason { get; set; }
    public List<OrderItemResponse> Items { get; set; } = new();
    public int TotalItems => Items.Sum(i => i.Quantity);

    private static string GetStatusText(OrderStatus status) => status switch
    {
        OrderStatus.Draft => "Nháp",
        OrderStatus.Pending => "Chờ xử lý",
        OrderStatus.Confirmed => "Đã xác nhận",
        OrderStatus.Delivering => "Đang giao",
        OrderStatus.Paid => "Đã thanh toán",
        OrderStatus.Completed => "Hoàn thành",
        OrderStatus.Cancelled => "Đã hủy",
        _ => "Không xác định"
    };
}

/// <summary>
/// Response item trong đơn hàng
/// </summary>
public class OrderItemResponse
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductImageUrl { get; set; }
    public int Quantity { get; set; }
    public decimal BasePrice { get; set; }
    public decimal OptionPrice { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public string? Note { get; set; }
    public List<OrderItemOptionResponse> Options { get; set; } = new();
    
    /// <summary>
    /// Mô tả ngắn các options đã chọn
    /// Format: "OptionGroupName: OptionItemName1, OptionItemName2"
    /// Ví dụ: "Kích cỡ: Nhỏ (S), Mức đường: 70%, Topping: Trân châu trắng, Thạch dừa"
    /// </summary>
    public string OptionsDescription => string.Join("\n", Options
        .GroupBy(o => o.OptionGroupName)
        .Select(g => $"{g.Key}: {string.Join(", ", g.Select(x => x.OptionItemName))}"));
}

/// <summary>
/// Response option đã chọn trong item
/// </summary>
public class OrderItemOptionResponse
{
    public int Id { get; set; }
    public int OptionGroupId { get; set; }
    public string OptionGroupName { get; set; } = string.Empty;
    public int OptionItemId { get; set; }
    public string OptionItemName { get; set; } = string.Empty;
    public decimal PriceAdjustment { get; set; }
}

/// <summary>
/// Response tóm tắt đơn hàng (cho danh sách)
/// </summary>
public class OrderSummaryResponse
{
    public int Id { get; set; }
    public string OrderCode { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public string StatusText => Status switch
    {
        OrderStatus.Draft => "Nháp",
        OrderStatus.Pending => "Chờ xử lý",
        OrderStatus.Confirmed => "Đã xác nhận",
        OrderStatus.Delivering => "Đang giao",
        OrderStatus.Paid => "Đã thanh toán",
        OrderStatus.Completed => "Hoàn thành",
        OrderStatus.Cancelled => "Đã hủy",
        _ => "Không xác định"
    };
    public decimal FinalAmount { get; set; }
    public int TotalItems { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<OrderItemResponse> Items { get; set; } = new();

}

#endregion
