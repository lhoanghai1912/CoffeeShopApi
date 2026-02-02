using CoffeeShopApi.Data;
using CoffeeShopApi.Models;
using CoffeeShopApi.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Gridify;

namespace CoffeeShopApi.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int id);
    Task<Order?> GetByIdWithDetailsAsync(int id);
    Task<Order?> GetByCodeAsync(string orderCode);
    Task<IEnumerable<Order>> GetByUserIdAsync(int userId);
    Task<IEnumerable<Order>> GetAllAsync();
    Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status);
    Task<(IEnumerable<Order> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, string? search, string? filter, string? orderBy);
    Task<Order> CreateAsync(Order order);
    Task<Order> UpdateAsync(Order order);
    Task<bool> DeleteAsync(int id);
    Task<string> GenerateOrderCodeAsync();
}

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _context;

    public OrderRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetByIdAsync(int id)
    {
        return await _context.Orders.FindAsync(id);
    }

    public async Task<Order?> GetByIdWithDetailsAsync(int id)
    {
        return await _context.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.OrderItemOptions)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<Order?> GetByCodeAsync(string orderCode)
    {
        return await _context.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.OrderItemOptions)
            .FirstOrDefaultAsync(o => o.OrderCode == orderCode);
    }

    public async Task<IEnumerable<Order>> GetByUserIdAsync(int userId)
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.OrderItemOptions)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetAllAsync()
    {
        return await _context.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.OrderItemOptions)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status)
    {
        return await _context.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.OrderItemOptions)
            .Where(o => o.Status == status)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<(IEnumerable<Order> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, string? search, string? filter, string? orderBy)
    {
        page = Math.Max(1, page);
        pageSize = Math.Max(1, Math.Min(100, pageSize));

        var query = _context.Orders.AsQueryable();

        // search by order code, phone number or user name
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(o => (o.OrderCode != null && o.OrderCode.ToLower().Contains(s))
                                     || (o.PhoneNumber != null && o.PhoneNumber.ToLower().Contains(s))
                                     || (o.User != null && o.User.FullName != null && o.User.FullName.ToLower().Contains(s))
                                     || (o.User != null && o.User.UserName != null && o.User.UserName.ToLower().Contains(s)));
        }

        // Apply Gridify filtering/ordering if available
        var gridifyQuery = new GridifyQuery
        {
            Page = page,
            PageSize = pageSize,
            Filter = filter,
            OrderBy = orderBy
        };

        // Apply filtering/ordering/paging
        query = query.ApplyFiltering(gridifyQuery).ApplyOrdering(gridifyQuery);

        var totalCount = await query.CountAsync();

        var items = await query
            .ApplyPaging(gridifyQuery)
            .Include(o => o.User)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.OrderItemOptions)
            .AsNoTracking()
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Order> CreateAsync(Order order)
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task<Order> UpdateAsync(Order order)
    {
        order.UpdatedAt = DateTime.UtcNow;
        _context.Orders.Update(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null) return false;

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<string> GenerateOrderCodeAsync()
    {
        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        var prefix = $"ORD-{today}-";
        
        var lastOrder = await _context.Orders
            .Where(o => o.OrderCode.StartsWith(prefix))
            .OrderByDescending(o => o.OrderCode)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (lastOrder != null)
        {
            var lastNumberStr = lastOrder.OrderCode.Replace(prefix, "");
            if (int.TryParse(lastNumberStr, out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        return $"{prefix}{nextNumber:D5}";
    }
}
