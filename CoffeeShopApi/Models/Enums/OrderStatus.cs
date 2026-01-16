namespace CoffeeShopApi.Models.Enums;

/// <summary>
/// Trạng thái của đơn hàng
/// </summary>
public enum OrderStatus
{
    /// <summary>
    /// Đơn hàng mới tạo, chưa hoàn tất - cho phép chỉnh sửa
    /// </summary>
    Draft = 0,
    
    /// <summary>
    /// Đơn hàng đã submit, chờ xử lý/thanh toán
    /// </summary>
    Pending = 1,
    
    /// <summary>
    /// Đơn hàng đã xác nhận, đang chuẩn bị
    /// </summary>
    Confirmed = 2,
    
    /// <summary>
    /// Đơn hàng đang được giao
    /// </summary>
    Delivering = 3,
    
    /// <summary>
    /// Đơn hàng đã thanh toán thành công
    /// </summary>
    Paid = 4,
    
    /// <summary>
    /// Đơn hàng đã hoàn thành
    /// </summary>
    Completed = 5,
    
    /// <summary>
    /// Đơn hàng đã bị hủy
    /// </summary>
    Cancelled = 6
}
