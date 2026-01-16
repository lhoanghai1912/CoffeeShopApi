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

        // Ensure at least 20 orders exist. If fewer, create the missing ones.
        var existingCount = await context.Orders.CountAsync();
        var targetCount = 20;
        if (existingCount < targetCount)
        {
            var toCreate = targetCount - existingCount;

            // Get some products to create order items
            var products = await context.Products.Take(10).ToListAsync();
            if (!products.Any()) return;

            // Compute starting sequence for today's prefix to avoid duplicate codes
            var today = DateTime.UtcNow.ToString("yyyyMMdd");
            var prefix = $"ORD-{today}-";
            var lastOrder = await context.Orders
                .Where(o => o.OrderCode != null && o.OrderCode.StartsWith(prefix))
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

            var orders = new List<Order>();
            for (int i = 1; i <= toCreate; i++)
            {
                var product = products[(i - 1) % products.Count];

                var order = new Order
                {
                    OrderCode = $"{prefix}{nextNumber:D5}",
                    UserId = user.Id,
                    Status = Models.Enums.OrderStatus.Confirmed,
                    Note = $"Sample order {existingCount + i}",
                    PhoneNumber = user.PhoneNumber,
                    ShippingAddress = "123 Sample Street",
                    CreatedAt = DateTime.UtcNow.AddMinutes(-(existingCount + i) * 5),
                    UpdatedAt = DateTime.UtcNow.AddMinutes(-(existingCount + i) * 5)
                };

                nextNumber++;

                var orderItem = new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = (i % 3) + 1,
                    BasePrice = product.BasePrice,
                    ProductName = product.Name,
                    ProductImageUrl = product.ImageUrl,
                    CreatedAt = DateTime.UtcNow
                };

                // Find option groups for product
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

                // Add one topping if available
                var toppingGroup = await context.OptionGroups.FirstOrDefaultAsync(og => og.ProductId == product.Id && og.Name == "Topping");
                if (toppingGroup != null)
                {
                    var toppingItem = await context.OptionItems.FirstOrDefaultAsync(oi => oi.OptionGroupId == toppingGroup.Id);
                    if (toppingItem != null)
                    {
                        var oio = new OrderItemOption
                        {
                            OptionGroupId = toppingGroup.Id,
                            OptionItemId = toppingItem.Id,
                            OptionGroupName = toppingGroup.Name,
                            OptionItemName = toppingItem.Name,
                            PriceAdjustment = toppingItem.PriceAdjustment
                        };
                        orderItem.OrderItemOptions.Add(oio);
                        orderItem.OptionPrice += toppingItem.PriceAdjustment;
                    }
                }

                orderItem.UnitPrice = orderItem.BasePrice + orderItem.OptionPrice;
                orderItem.TotalPrice = orderItem.UnitPrice * orderItem.Quantity;

                order.SubTotal = orderItem.TotalPrice;
                order.FinalAmount = order.SubTotal - order.DiscountAmount + order.ShippingFee;

                order.OrderItems.Add(orderItem);
                orders.Add(order);
            }

            context.Orders.AddRange(orders);
            await context.SaveChangesAsync();
        }
        else
        {
            // If orders exist, update their OrderCode (if missing) and recalculate totals
            var existingOrders = await context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.OrderItemOptions)
                .ToListAsync();

            foreach (var order in existingOrders)
            {
                var changed = false;

                if (string.IsNullOrWhiteSpace(order.OrderCode))
                {
                    order.OrderCode = await GenerateOrderCode(context);
                    changed = true;
                }

                decimal subTotal = 0;
                foreach (var oi in order.OrderItems)
                {
                    // If snapshot prices missing, try to fill from product
                    if (oi.BasePrice == 0)
                    {
                        var product = await context.Products.FindAsync(oi.ProductId);
                        if (product != null)
                        {
                            oi.BasePrice = product.BasePrice;
                            oi.ProductName = product.Name;
                            oi.ProductImageUrl = product.ImageUrl;
                            changed = true;
                        }
                    }

                    // Recalc OptionPrice from OrderItemOptions
                    decimal optionPrice = 0;
                    foreach (var oio in oi.OrderItemOptions)
                    {
                        optionPrice += oio.PriceAdjustment;
                    }
                    oi.OptionPrice = optionPrice;

                    // Recalc unit and total
                    oi.UnitPrice = oi.BasePrice + oi.OptionPrice;
                    oi.TotalPrice = oi.UnitPrice * Math.Max(1, oi.Quantity);

                    subTotal += oi.TotalPrice;
                }

                if (order.SubTotal != subTotal)
                {
                    order.SubTotal = subTotal;
                    changed = true;
                }

                var finalAmount = order.SubTotal - order.DiscountAmount + order.ShippingFee;
                if (order.FinalAmount != finalAmount)
                {
                    order.FinalAmount = finalAmount;
                    changed = true;
                }

                if (changed)
                {
                    order.UpdatedAt = DateTime.UtcNow;
                }
            }

            await context.SaveChangesAsync();
        }
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
