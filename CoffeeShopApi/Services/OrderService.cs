using CoffeeShopApi.Data;
using CoffeeShopApi.DTOs;
using CoffeeShopApi.Models;
using CoffeeShopApi.Models.Enums;
using CoffeeShopApi.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShopApi.Services;

public interface IOrderService
{
    // Query
    Task<OrderResponse?> GetByIdAsync(int id);
    Task<OrderResponse?> GetByCodeAsync(string orderCode);
    Task<IEnumerable<OrderSummaryResponse>> GetByUserIdAsync(int userId);
    Task<IEnumerable<OrderSummaryResponse>> GetAllAsync();
    Task<IEnumerable<OrderSummaryResponse>> GetByStatusAsync(OrderStatus status);
    Task<PaginatedResponse<OrderSummaryResponse>> GetPagedAsync(int page, int pageSize, string? search, string? orderBy, string? filter);
    
    // Commands
    Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request);
    Task<OrderResponse> AddOrderItemAsync(int orderId, CreateOrderItemRequest request);
    Task<OrderResponse> UpdateOrderItemAsync(int orderId, int orderItemId, UpdateOrderItemRequest request);
    Task<OrderResponse> RemoveOrderItemAsync(int orderId, int orderItemId);
    Task<OrderResponse> UpdateOrderAsync(int orderId, UpdateOrderRequest request);
    Task<OrderResponse> CheckoutOrderAsync(int orderId, CheckoutOrderRequest request);
    Task<OrderResponse> ConfirmOrderAsync(int orderId);
    Task<OrderResponse> MarkAsPaidAsync(int orderId);
    Task<OrderResponse> CancelOrderAsync(int orderId, CancelOrderRequest request);
    Task<bool> DeleteOrderAsync(int id);
    
    // User ownership validation (không phân quyền, chỉ check ownership)
    /// <summary>
    /// Kiểm tra order có thuộc về user không
    /// </summary>
    Task<bool> IsOrderOwnedByUserAsync(int orderId, int userId);
    
    /// <summary>
    /// Lấy order của user hiện tại theo ID (chỉ trả về nếu thuộc về user)
    /// </summary>
    Task<OrderResponse?> GetUserOrderByIdAsync(int orderId, int userId);
}

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUserAddressService _userAddressService;
    private readonly AppDbContext _context;

    public OrderService(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IUserAddressService userAddressService,
        AppDbContext context)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _userAddressService = userAddressService;
        _context = context;
    }

    #region User Ownership Validation

    public async Task<bool> IsOrderOwnedByUserAsync(int orderId, int userId)
    {
        return await _context.Orders.AnyAsync(o => o.Id == orderId && o.UserId == userId);
    }

    public async Task<OrderResponse?> GetUserOrderByIdAsync(int orderId, int userId)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId);
        
        // Nếu không tìm thấy hoặc không thuộc về user -> return null
        if (order == null || order.UserId != userId)
            return null;
            
        return MapToResponse(order);
    }

    #endregion

    #region Query Methods



    public async Task<OrderResponse?> GetByIdAsync(int id)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(id);
        return order == null ? null : MapToResponse(order);
    }

    public async Task<OrderResponse?> GetByCodeAsync(string orderCode)
    {
        var order = await _orderRepository.GetByCodeAsync(orderCode);
        return order == null ? null : MapToResponse(order);
    }

    public async Task<IEnumerable<OrderSummaryResponse>> GetByUserIdAsync(int userId)
    {
        var orders = await _orderRepository.GetByUserIdAsync(userId);
        return orders.Select(MapToSummary);
    }

    public async Task<IEnumerable<OrderSummaryResponse>> GetAllAsync()
    {
        var orders = await _orderRepository.GetAllAsync();
        return orders.Select(MapToSummary);
    }

    public async Task<IEnumerable<OrderSummaryResponse>> GetByStatusAsync(OrderStatus status)
    {
        var orders = await _orderRepository.GetByStatusAsync(status);
        return orders.Select(MapToSummary);
    }

    public async Task<PaginatedResponse<OrderSummaryResponse>> GetPagedAsync(int page, int pageSize, string? search, string? orderBy, string? filter)
    {
        var (items, totalCount) = await _orderRepository.GetPagedAsync(page, pageSize, search, filter, orderBy);
        var mapped = items.Select(MapToSummary).ToList();
        return new PaginatedResponse<OrderSummaryResponse>(mapped, totalCount, page, pageSize);
    }

    #endregion

    #region Command Methods

    public async Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var order = new Order
            {
                OrderCode = await _orderRepository.GenerateOrderCodeAsync(),
                UserId = request.UserId,
                Status = OrderStatus.Pending,
                Note = request.Note,
                ShippingAddress = request.ShippingAddress,
                PhoneNumber = request.PhoneNumber
            };

            await _orderRepository.CreateAsync(order);

            // Thêm items nếu có
            if (request.Items != null && request.Items.Any())
            {
                foreach (var itemRequest in request.Items)
                {
                    await AddOrderItemInternalAsync(order, itemRequest);
                }
            }

            // Tính lại tổng
            await RecalculateOrderTotalAsync(order);
            await _orderRepository.UpdateAsync(order);

            await transaction.CommitAsync();

            return await GetByIdAsync(order.Id) ?? throw new Exception("Failed to create order");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<OrderResponse> AddOrderItemAsync(int orderId, CreateOrderItemRequest request)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId)
            ?? throw new ArgumentException($"Order với ID {orderId} không tồn tại");

        // Chỉ cho phép thêm item khi order đang Draft
        if (order.Status != OrderStatus.Draft)
            throw new InvalidOperationException("Chỉ có thể thêm sản phẩm khi đơn hàng đang ở trạng thái Nháp");

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            await AddOrderItemInternalAsync(order, request);
            await RecalculateOrderTotalAsync(order);
            await _orderRepository.UpdateAsync(order);

            await transaction.CommitAsync();

            return await GetByIdAsync(orderId) ?? throw new Exception("Failed to add order item");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<OrderResponse> UpdateOrderItemAsync(int orderId, int orderItemId, UpdateOrderItemRequest request)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId)
            ?? throw new ArgumentException($"Order với ID {orderId} không tồn tại");

        if (order.Status != OrderStatus.Draft)
            throw new InvalidOperationException("Chỉ có thể cập nhật sản phẩm khi đơn hàng đang ở trạng thái Nháp");

        var orderItem = order.OrderItems.FirstOrDefault(oi => oi.Id == orderItemId)
            ?? throw new ArgumentException($"OrderItem với ID {orderItemId} không tồn tại trong đơn hàng");

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Cập nhật quantity và note
            orderItem.Quantity = request.Quantity;
            orderItem.Note = request.Note;

            // Cập nhật options nếu có
            if (request.SelectedOptionItemIds != null)
            {
                // Xóa options cũ
                _context.OrderItemOptions.RemoveRange(orderItem.OrderItemOptions);

                // Thêm options mới
                var product = await _productRepository.GetByIdWithDetailsAsync(orderItem.ProductId)
                    ?? throw new ArgumentException($"Product với ID {orderItem.ProductId} không còn tồn tại");

                var (options, validationErrors) = await ValidateAndBuildOptionsAsync(
                    product, request.SelectedOptionItemIds);

                if (validationErrors.Any())
                    throw new ArgumentException(string.Join("; ", validationErrors));

                orderItem.OrderItemOptions = options;

                // Tính lại giá option
                orderItem.OptionPrice = options.Sum(o => o.PriceAdjustment);
            }

            // Tính lại giá item
            CalculateOrderItemPrices(orderItem);

            // Tính lại tổng order
            await RecalculateOrderTotalAsync(order);
            await _orderRepository.UpdateAsync(order);

            await transaction.CommitAsync();

            return await GetByIdAsync(orderId) ?? throw new Exception("Failed to update order item");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<OrderResponse> RemoveOrderItemAsync(int orderId, int orderItemId)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId)
            ?? throw new ArgumentException($"Order với ID {orderId} không tồn tại");

        if (order.Status != OrderStatus.Draft)
            throw new InvalidOperationException("Chỉ có thể xóa sản phẩm khi đơn hàng đang ở trạng thái Nháp");

        var orderItem = order.OrderItems.FirstOrDefault(oi => oi.Id == orderItemId)
            ?? throw new ArgumentException($"OrderItem với ID {orderItemId} không tồn tại trong đơn hàng");

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            _context.OrderItems.Remove(orderItem);
            await _context.SaveChangesAsync();

            // Reload order để tính lại
            order = await _orderRepository.GetByIdWithDetailsAsync(orderId)!;
            await RecalculateOrderTotalAsync(order!);
            await _orderRepository.UpdateAsync(order!);

            await transaction.CommitAsync();

            return await GetByIdAsync(orderId) ?? throw new Exception("Failed to remove order item");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<OrderResponse> UpdateOrderAsync(int orderId, UpdateOrderRequest request)
    {
        var order = await _orderRepository.GetByIdAsync(orderId)
            ?? throw new ArgumentException($"Order với ID {orderId} không tồn tại");

        if (order.Status != OrderStatus.Draft && order.Status != OrderStatus.Pending)
            throw new InvalidOperationException("Chỉ có thể cập nhật đơn hàng khi đang ở trạng thái Nháp hoặc Chờ xử lý");

        order.Note = request.Note ?? order.Note;
        order.ShippingAddress = request.ShippingAddress ?? order.ShippingAddress;
        order.PhoneNumber = request.PhoneNumber ?? order.PhoneNumber;

        await _orderRepository.UpdateAsync(order);

        return await GetByIdAsync(orderId) ?? throw new Exception("Failed to update order");
    }

    public async Task<OrderResponse> CheckoutOrderAsync(int orderId, CheckoutOrderRequest request)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId)
            ?? throw new ArgumentException($"Order với ID {orderId} không tồn tại");

        if (order.Status != OrderStatus.Draft)
            throw new InvalidOperationException("Chỉ có thể checkout đơn hàng đang ở trạng thái Nháp");

        if (!order.OrderItems.Any())
            throw new InvalidOperationException("Không thể checkout đơn hàng trống");

        // Validate và snapshot địa chỉ giao hàng
        if (!order.UserId.HasValue)
            throw new InvalidOperationException("Đơn hàng phải có UserId để checkout");

        if (!request.UserAddressId.HasValue)
            throw new InvalidOperationException("Vui lòng chọn địa chỉ giao hàng");

        // Validate UserAddressId thuộc về UserId
        var userAddress = await _userAddressService.GetAddressEntityAsync(request.UserAddressId.Value, order.UserId.Value);
        if (userAddress == null)
            throw new ArgumentException("Địa chỉ giao hàng không hợp lệ hoặc không thuộc về bạn");

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Validate lại giá của tất cả items (đảm bảo product/option vẫn còn và giá không đổi)
            var validationErrors = await ValidateOrderBeforeCheckoutAsync(order);
            if (validationErrors.Any())
                throw new InvalidOperationException($"Lỗi validation: {string.Join("; ", validationErrors)}");

            // ✅ SNAPSHOT địa chỉ giao hàng - Copy text data vào Order
            // Không dùng FK để đảm bảo lịch sử order không bị ảnh hưởng khi user update/delete địa chỉ
            order.RecipientName = userAddress.RecipientName;
            order.ShippingAddress = userAddress.AddressLine;
            order.PhoneNumber = userAddress.PhoneNumber;

            // Cập nhật ghi chú từ request
            if (!string.IsNullOrEmpty(request.Note))
                order.Note = request.Note;

            // TODO: Apply voucher nếu có
            if (request.VoucherId.HasValue)
            {
                order.VoucherId = request.VoucherId;
                // Xử lý logic voucher ở đây khi có Voucher entity
            }

            // Tính lại tổng cuối
            await RecalculateOrderTotalAsync(order);

            // Chuyển trạng thái
            order.Status = OrderStatus.Pending;

            await _orderRepository.UpdateAsync(order);
            await transaction.CommitAsync();

            return await GetByIdAsync(orderId) ?? throw new Exception("Failed to checkout order");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<OrderResponse> ConfirmOrderAsync(int orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId)
            ?? throw new ArgumentException($"Order với ID {orderId} không tồn tại");

        if (order.Status != OrderStatus.Pending)
            throw new InvalidOperationException("Chỉ có thể xác nhận đơn hàng đang Chờ xử lý");

        order.Status = OrderStatus.Confirmed;
        await _orderRepository.UpdateAsync(order);

        return await GetByIdAsync(orderId) ?? throw new Exception("Failed to confirm order");
    }

    public async Task<OrderResponse> MarkAsPaidAsync(int orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId)
            ?? throw new ArgumentException($"Order với ID {orderId} không tồn tại");

        if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Confirmed)
            throw new InvalidOperationException("Chỉ có thể đánh dấu thanh toán cho đơn hàng Chờ xử lý hoặc Đã xác nhận");

        order.Status = OrderStatus.Paid;
        order.PaidAt = DateTime.UtcNow;
        await _orderRepository.UpdateAsync(order);

        return await GetByIdAsync(orderId) ?? throw new Exception("Failed to mark order as paid");
    }

    public async Task<OrderResponse> CancelOrderAsync(int orderId, CancelOrderRequest request)
    {
        var order = await _orderRepository.GetByIdAsync(orderId)
            ?? throw new ArgumentException($"Order với ID {orderId} không tồn tại");

        // Chỉ cho phép hủy khi chưa Paid
        if (order.Status == OrderStatus.Paid || order.Status == OrderStatus.Completed)
            throw new InvalidOperationException("Không thể hủy đơn hàng đã thanh toán hoặc hoàn thành");

        if (order.Status == OrderStatus.Cancelled)
            throw new InvalidOperationException("Đơn hàng đã được hủy trước đó");

        order.Status = OrderStatus.Cancelled;
        order.CancelledAt = DateTime.UtcNow;
        order.CancelReason = request.Reason;

        // TODO: Rollback voucher nếu có
        if (order.VoucherId.HasValue)
        {
            // Xử lý rollback voucher ở đây
        }

        await _orderRepository.UpdateAsync(order);

        return await GetByIdAsync(orderId) ?? throw new Exception("Failed to cancel order");
    }

    public async Task<bool> DeleteOrderAsync(int id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null) return false;

        // Chỉ cho phép xóa đơn Draft hoặc đã Cancel
        if (order.Status != OrderStatus.Draft && order.Status != OrderStatus.Cancelled)
            throw new InvalidOperationException("Chỉ có thể xóa đơn hàng Nháp hoặc đã Hủy");

        return await _orderRepository.DeleteAsync(id);
    }

    #endregion

    #region Private Helper Methods

    private async Task AddOrderItemInternalAsync(Order order, CreateOrderItemRequest request)
    {
        // Validate product
        var product = await _productRepository.GetByIdWithDetailsAsync(request.ProductId)
            ?? throw new ArgumentException($"Sản phẩm với ID {request.ProductId} không tồn tại");

        // Validate và build options
        var (options, validationErrors) = await ValidateAndBuildOptionsAsync(
            product, request.SelectedOptionItemIds ?? new List<int>());

        if (validationErrors.Any())
            throw new ArgumentException(string.Join("; ", validationErrors));

        // Tạo OrderItem với snapshot
        var orderItem = new OrderItem
        {
            OrderId = order.Id,
            ProductId = product.Id,
            Quantity = request.Quantity > 0 ? request.Quantity : 1,
            BasePrice = product.BasePrice, // Snapshot
            ProductName = product.Name, // Snapshot
            ProductImageUrl = product.ImageUrl, // Snapshot
            OptionPrice = options.Sum(o => o.PriceAdjustment),
            Note = request.Note,
            OrderItemOptions = options
        };

        // Tính giá
        CalculateOrderItemPrices(orderItem);

        order.OrderItems.Add(orderItem);
        _context.OrderItems.Add(orderItem);
        await _context.SaveChangesAsync();
    }

    private async Task<(List<OrderItemOption> options, List<string> errors)> ValidateAndBuildOptionsAsync(
        Product product, List<int> selectedOptionItemIds)
    {
        var options = new List<OrderItemOption>();
        var errors = new List<string>();

        // Lấy tất cả OptionGroups của product
        var optionGroups = await _context.OptionGroups
            .Include(og => og.OptionItems)
            .Where(og => og.ProductId == product.Id)
            .ToListAsync();

        // Validate từng OptionGroup
        foreach (var group in optionGroups)
        {
            var selectedInGroup = group.OptionItems
                .Where(oi => selectedOptionItemIds.Contains(oi.Id))
                .ToList();

            // Kiểm tra Required
            if (group.IsRequired && !selectedInGroup.Any())
            {
                errors.Add($"Nhóm '{group.Name}' là bắt buộc");
                continue;
            }

            // Kiểm tra AllowMultiple
            if (!group.AllowMultiple && selectedInGroup.Count > 1)
            {
                errors.Add($"Nhóm '{group.Name}' chỉ được chọn 1 option");
                continue;
            }

            // Build OrderItemOptions với snapshot
            foreach (var item in selectedInGroup)
            {
                options.Add(new OrderItemOption
                {
                    OptionGroupId = group.Id,
                    OptionItemId = item.Id,
                    OptionGroupName = group.Name, // Snapshot
                    OptionItemName = item.Name, // Snapshot
                    PriceAdjustment = item.PriceAdjustment // Snapshot
                });
            }
        }

        // Validate: không cho phép chọn option không thuộc product
        var validOptionItemIds = optionGroups.SelectMany(og => og.OptionItems.Select(oi => oi.Id)).ToList();
        var invalidIds = selectedOptionItemIds.Except(validOptionItemIds).ToList();
        if (invalidIds.Any())
        {
            errors.Add($"Các option ID không hợp lệ: {string.Join(", ", invalidIds)}");
        }

        return (options, errors);
    }

    private static void CalculateOrderItemPrices(OrderItem orderItem)
    {
        orderItem.UnitPrice = orderItem.BasePrice + orderItem.OptionPrice;
        orderItem.TotalPrice = orderItem.UnitPrice * orderItem.Quantity;
    }

    private Task RecalculateOrderTotalAsync(Order order)
    {
        order.SubTotal = order.OrderItems.Sum(oi => oi.TotalPrice);
        order.FinalAmount = order.SubTotal - order.DiscountAmount + order.ShippingFee;
        
        if (order.FinalAmount < 0)
            order.FinalAmount = 0;

        return Task.CompletedTask;
    }

    private async Task<List<string>> ValidateOrderBeforeCheckoutAsync(Order order)
    {
        var errors = new List<string>();

        foreach (var item in order.OrderItems)
        {
            // Kiểm tra product còn tồn tại
            var product = await _productRepository.GetByIdWithDetailsAsync(item.ProductId);
            if (product == null)
            {
                errors.Add($"Sản phẩm '{item.ProductName}' không còn tồn tại trong hệ thống");
                continue;
            }

            // Cảnh báo nếu giá đã thay đổi (nhưng không block checkout vì đã snapshot)
            if (product.BasePrice != item.BasePrice)
            {
                // Log warning hoặc thông báo cho user biết giá đã thay đổi
                // Giá trong order vẫn giữ nguyên theo snapshot
            }

            // Kiểm tra options còn tồn tại
            foreach (var option in item.OrderItemOptions)
            {
                var optionItem = await _context.OptionItems.FindAsync(option.OptionItemId);
                if (optionItem == null)
                {
                    // Option đã bị xóa, nhưng order vẫn giữ snapshot
                    // Có thể log warning
                }
            }
        }

        return errors;
    }

    #endregion

    #region Mapping Methods

    private static OrderResponse MapToResponse(Order order)
    {
        return new OrderResponse
        {
            Id = order.Id,
            OrderCode = order.OrderCode,
            UserId = order.UserId,
            UserName = order.User?.FullName ?? order.User?.Username,
            Status = order.Status,
            SubTotal = order.SubTotal,
            DiscountAmount = order.DiscountAmount,
            ShippingFee = order.ShippingFee,
            FinalAmount = order.FinalAmount,
            VoucherId = order.VoucherId,
            Note = order.Note,
            RecipientName = order.RecipientName,
            ShippingAddress = order.ShippingAddress,
            PhoneNumber = order.PhoneNumber,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            PaidAt = order.PaidAt,
            CancelledAt = order.CancelledAt,
            CancelReason = order.CancelReason,
            Items = order.OrderItems.Select(MapToItemResponse).ToList()
        };
    }

    private static OrderItemResponse MapToItemResponse(OrderItem item)
    {
        return new OrderItemResponse
        {
            Id = item.Id,
            ProductId = item.ProductId,
            ProductName = item.ProductName,
            ProductImageUrl = item.ProductImageUrl,
            Quantity = item.Quantity,
            BasePrice = item.BasePrice,
            OptionPrice = item.OptionPrice,
            UnitPrice = item.UnitPrice,
            TotalPrice = item.TotalPrice,
            Note = item.Note,
            Options = item.OrderItemOptions.Select(MapToOptionResponse).ToList()
        };
    }

    private static OrderItemOptionResponse MapToOptionResponse(OrderItemOption option)
    {
        return new OrderItemOptionResponse
        {
            Id = option.Id,
            OptionGroupId = option.OptionGroupId,
            OptionGroupName = option.OptionGroupName,
            OptionItemId = option.OptionItemId,
            OptionItemName = option.OptionItemName,
            PriceAdjustment = option.PriceAdjustment
        };
    }

    private static OrderSummaryResponse MapToSummary(Order order)
    {
        return new OrderSummaryResponse
        {
            Id = order.Id,
            OrderCode = order.OrderCode,
            Status = order.Status,
            FinalAmount = order.FinalAmount,
            TotalItems = order.OrderItems.Sum(oi => oi.Quantity),
            CreatedAt = order.CreatedAt
        };
    }

    #endregion
}
