using CoffeeShopApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShopApi.Data;

public static class OrderSeeder
{
    public static async Task SeedSampleOrders(AppDbContext context)
    {
        // Ensure there is at least one user
        var user = await context.Users.FirstOrDefaultAsync();
        if (user == null)
        {
            user = new User { Username = "testuser", Password = "password", FullName = "Test User", PhoneNumber = "0123456789", RoleId = 3 };
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }

        // Get products to build orders
        var products = await context.Products.Take(10).ToListAsync();
        if (!products.Any()) return;

        // Remove existing orders and related items/options to start fresh
        var existingOrders = await context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.OrderItemOptions)
            .ToListAsync();

        if (existingOrders.Any())
        {
            // Remove options
            var allOptions = existingOrders.SelectMany(o => o.OrderItems).SelectMany(oi => oi.OrderItemOptions).ToList();
            if (allOptions.Any()) context.RemoveRange(allOptions);

            // Remove order items
            var allItems = existingOrders.SelectMany(o => o.OrderItems).ToList();
            if (allItems.Any()) context.RemoveRange(allItems);

            // Remove orders
            context.Orders.RemoveRange(existingOrders);
            await context.SaveChangesAsync();
        }

        // Create one order per OrderStatus value to cover all cases
        var statuses = Enum.GetValues(typeof(Models.Enums.OrderStatus)).Cast<Models.Enums.OrderStatus>().ToList();
        var orders = new List<Order>();
        var prefix = DateTime.UtcNow.ToString("yyyyMMdd");
        int seq = 1;

        foreach (var status in statuses)
        {
            var product = products[(seq - 1) % products.Count];

            var order = new Order
            {
                OrderCode = $"ORD-{prefix}-{seq:D5}",
                UserId = user.Id,
                Status = status,
                Note = $"Seed order {seq} - {status}",
                PhoneNumber = user.PhoneNumber,
                ShippingAddress = "123 Sample Street",
                CreatedAt = DateTime.UtcNow.AddMinutes(-seq * 10),
                UpdatedAt = DateTime.UtcNow.AddMinutes(-seq * 10)
            };

            var orderItem = new OrderItem
            {
                ProductId = product.Id,
                Quantity = (seq % 3) + 1,
                BasePrice = product.BasePrice,
                ProductName = product.Name,
                ProductImageUrl = product.ImageUrl,
                CreatedAt = DateTime.UtcNow
            };

            // Add default size option if exists
            var sizeGroup = await context.OptionGroups.FirstOrDefaultAsync(og => og.ProductId == product.Id && og.Name == "Size");
            if (sizeGroup != null)
            {
                var sizeItem = await context.OptionItems.FirstOrDefaultAsync(oi => oi.OptionGroupId == sizeGroup.Id && oi.IsDefault)
                               ?? await context.OptionItems.FirstOrDefaultAsync(oi => oi.OptionGroupId == sizeGroup.Id);
                if (sizeItem != null)
                {
                    var oio = new OrderItemOption
                    {
                        OptionGroupId = sizeGroup.Id,
                        OptionItemId = sizeItem.Id,
                        OptionGroupName = sizeGroup.Name,
                        OptionItemName = sizeItem.Name,
                        PriceAdjustment = sizeItem.PriceAdjustment
                    };
                    orderItem.OrderItemOptions.Add(oio);
                    orderItem.OptionPrice += sizeItem.PriceAdjustment;
                }
            }

            orderItem.UnitPrice = orderItem.BasePrice + orderItem.OptionPrice;
            orderItem.TotalPrice = orderItem.UnitPrice * orderItem.Quantity;

            order.SubTotal = orderItem.TotalPrice;
            order.FinalAmount = order.SubTotal - order.DiscountAmount + order.ShippingFee;

            // Set status-specific timestamps
            if (status == Models.Enums.OrderStatus.Paid || status == Models.Enums.OrderStatus.Completed)
            {
                order.PaidAt = DateTime.UtcNow.AddMinutes(-5);
            }

            if (status == Models.Enums.OrderStatus.Cancelled)
            {
                order.CancelledAt = DateTime.UtcNow.AddMinutes(-3);
                order.CancelReason = "Seeded cancellation";
            }

            order.OrderItems.Add(orderItem);
            orders.Add(order);
            seq++;
        }

        // Additionally create 20 more orders cycling through all statuses
        var additionalCount = 20;
        var additionalOrders = new List<Order>();
        for (int i = 0; i < additionalCount; i++)
        {
            var status = statuses[i % statuses.Count];
            var product = products[i % products.Count];

            // Note: use GenerateOrderCode later after adding all to avoid querying saved orders repeatedly
            var ord = new Order
            {
                OrderCode = string.Empty, // will be generated below
                UserId = user.Id,
                Status = status,
                Note = $"Additional seed order {i + 1} - {status}",
                PhoneNumber = user.PhoneNumber,
                ShippingAddress = "123 Sample Street",
                CreatedAt = DateTime.UtcNow.AddMinutes(-(i + seq) * 3),
                UpdatedAt = DateTime.UtcNow.AddMinutes(-(i + seq) * 3)
            };

            var oi = new OrderItem
            {
                ProductId = product.Id,
                Quantity = ((i + 1) % 4) + 1,
                BasePrice = product.BasePrice,
                ProductName = product.Name,
                ProductImageUrl = product.ImageUrl,
                CreatedAt = DateTime.UtcNow
            };

            var sg = await context.OptionGroups.FirstOrDefaultAsync(g => g.ProductId == product.Id && g.Name == "Size");
            if (sg != null)
            {
                var si = await context.OptionItems.FirstOrDefaultAsync(x => x.OptionGroupId == sg.Id && x.IsDefault)
                         ?? await context.OptionItems.FirstOrDefaultAsync(x => x.OptionGroupId == sg.Id);
                if (si != null)
                {
                    var oio = new OrderItemOption
                    {
                        OptionGroupId = sg.Id,
                        OptionItemId = si.Id,
                        OptionGroupName = sg.Name,
                        OptionItemName = si.Name,
                        PriceAdjustment = si.PriceAdjustment
                    };
                    oi.OrderItemOptions.Add(oio);
                    oi.OptionPrice += si.PriceAdjustment;
                }
            }

            oi.UnitPrice = oi.BasePrice + oi.OptionPrice;
            oi.TotalPrice = oi.UnitPrice * oi.Quantity;

            ord.SubTotal = oi.TotalPrice;
            ord.FinalAmount = ord.SubTotal - ord.DiscountAmount + ord.ShippingFee;

            if (status == Models.Enums.OrderStatus.Paid || status == Models.Enums.OrderStatus.Completed)
                ord.PaidAt = DateTime.UtcNow.AddMinutes(-2);
            if (status == Models.Enums.OrderStatus.Cancelled)
            {
                ord.CancelledAt = DateTime.UtcNow.AddMinutes(-1);
                ord.CancelReason = "Additional seeded cancel";
            }

            ord.OrderItems.Add(oi);
            additionalOrders.Add(ord);
        }

        // Assign OrderCode sequentially and add all orders at once
        var allToAdd = orders.Concat(additionalOrders).ToList();
        var todayPrefix = DateTime.UtcNow.ToString("yyyyMMdd");
        int counter = 1;
        foreach (var o in allToAdd)
        {
            o.OrderCode = $"ORD-{todayPrefix}-{counter:D5}";
            counter++;
        }

        context.Orders.AddRange(allToAdd);
        await context.SaveChangesAsync();
    }

    private static async Task<string> GenerateOrderCode(AppDbContext context)
    {
        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        var prefix = $"ORD-{today}-";

        var lastOrder = await context.Orders
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
