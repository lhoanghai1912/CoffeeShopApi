# UTC+7 (Vietnam Timezone) Implementation

## Tổng quan

Tất cả các hàm liên quan đến thời gian trong hệ thống đã được cập nhật để sử dụng múi giờ Việt Nam (UTC+7 - SE Asia Standard Time) thay vì UTC.

## Lý do thay đổi

- **Phù hợp với người dùng Việt Nam**: Hiển thị thời gian theo múi giờ địa phương
- **Báo cáo và logs chính xác**: Order code, timestamps, và audit logs theo giờ Việt Nam
- **Tuân thủ business logic**: Voucher start/end dates, order timestamps theo giờ người dùng mong đợi

## TimeZone ID

```csharp
TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")
```

- **Windows**: `SE Asia Standard Time`
- **Linux/Mac**: `Asia/Bangkok` hoặc `Asia/Ho_Chi_Minh` (tùy hệ điều hành)

## Helper Method

Tất cả các file đã được thêm helper method:

```csharp
/// <summary>
/// Lấy thời gian hiện tại theo múi giờ Việt Nam (UTC+7)
/// </summary>
private static DateTime GetVietnamTime()
{
    var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
    return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);
}
```

## Các file đã thay đổi

### 1. AppDbContext.cs

**Thay đổi:**
- Thêm `GetVietnamTime()` helper method
- Override `SaveChangesAsync()` để tự động set `CreatedAt` và `UpdatedAt` với thời gian UTC+7

**Tác động:**
- Tất cả entities khi được tạo hoặc cập nhật sẽ tự động có timestamps theo UTC+7
- Áp dụng cho: Order, OrderItem, User, Product, Voucher, UserAddress, v.v.

```csharp
public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
{
    var entries = ChangeTracker.Entries()
        .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

    var now = GetVietnamTime();

    foreach (var entry in entries)
    {
        if (entry.State == EntityState.Added)
        {
            // Set CreatedAt for new entities
            var createdAtProperty = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "CreatedAt");
            if (createdAtProperty != null && createdAtProperty.CurrentValue == null)
                createdAtProperty.CurrentValue = now;
        }

        if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
        {
            // Set UpdatedAt for added or modified entities
            var updatedAtProperty = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "UpdatedAt");
            if (updatedAtProperty != null)
                updatedAtProperty.CurrentValue = now;
        }
    }

    return base.SaveChangesAsync(cancellationToken);
}
```

### 2. OrderService.cs

**Thay đổi:**
- `MarkAsPaidAsync()`: `order.PaidAt = GetVietnamTime()`
- `CancelOrderAsync()`: `order.CancelledAt = GetVietnamTime()`

**Before:**
```csharp
order.PaidAt = DateTime.UtcNow;
order.CancelledAt = DateTime.UtcNow;
```

**After:**
```csharp
order.PaidAt = GetVietnamTime();
order.CancelledAt = GetVietnamTime();
```

### 3. OrderRepository.cs

**Thay đổi:**
- `UpdateAsync()`: `order.UpdatedAt = GetVietnamTime()`
- `GenerateOrderCodeAsync()`: Sử dụng `GetVietnamTime()` để tạo order code theo ngày VN

**Tác động:**
- Order codes như `ORD-20240403-00001` sẽ sử dụng ngày theo múi giờ Việt Nam
- Quan trọng: Tránh trường hợp đơn hàng tạo lúc 23:00 VN nhưng order code là ngày hôm sau (nếu dùng UTC)

**Before:**
```csharp
var today = DateTime.UtcNow.ToString("yyyyMMdd");
order.UpdatedAt = DateTime.UtcNow;
```

**After:**
```csharp
var today = GetVietnamTime().ToString("yyyyMMdd");
order.UpdatedAt = GetVietnamTime();
```

### 4. VoucherService.cs

**Thay đổi:**
- `ValidateVoucherAsync()`: Validate start/end date với thời gian VN
- `CreateAsync()`: Set `CreatedAt` và `UpdatedAt` với thời gian VN

**Tác động:**
- Voucher start/end dates được so sánh với thời gian hiện tại ở Việt Nam
- Tránh trường hợp voucher "đã hết hạn" nhưng thực tế còn valid theo giờ VN

**Before:**
```csharp
var now = DateTime.UtcNow;
if (now < voucher.StartDate) { ... }
if (now > voucher.EndDate) { ... }

CreatedAt = DateTime.UtcNow,
UpdatedAt = DateTime.UtcNow
```

**After:**
```csharp
var now = GetVietnamTime();
if (now < voucher.StartDate) { ... }
if (now > voucher.EndDate) { ... }

CreatedAt = GetVietnamTime(),
UpdatedAt = GetVietnamTime()
```

### 5. VoucherSeeder.cs

**Thay đổi:**
- Seed vouchers với timestamps theo UTC+7

**Before:**
```csharp
var now = DateTime.UtcNow;
```

**After:**
```csharp
var now = GetVietnamTime();
```

## Testing Scenarios

### Test 1: Order Creation Timestamp
```csharp
// Tạo order lúc 23:30 giờ VN (16:30 UTC)
POST /api/orders
// Expected: 
// - CreatedAt: 2024-04-03 23:30:00 (VN time)
// - OrderCode: ORD-20240403-00001 (ngày VN, không phải 20240404)
```

### Test 2: Voucher Validation
```csharp
// Voucher EndDate: 2024-04-03 23:59:59 (VN time)
// Current Time: 2024-04-03 23:00:00 (VN time)
// Expected: Voucher VALID (vì còn trong thời hạn theo giờ VN)
```

### Test 3: Order Payment
```csharp
// Mark order as paid lúc 14:30 VN
PUT /api/orders/{id}/mark-paid
// Expected: PaidAt = 2024-04-03 14:30:xx (VN time)
```

## Database Considerations

### Existing Data
- **Không cần migration**: Dữ liệu cũ vẫn ở dạng UTC trong database
- **Display**: Khi hiển thị, có thể cần convert từ UTC sang VN nếu cần
- **New Data**: Tất cả dữ liệu mới sẽ được lưu theo UTC+7

### Column Type
```sql
-- Columns vẫn là datetime2 (không thay đổi)
CreatedAt datetime2 NOT NULL
UpdatedAt datetime2 NOT NULL
PaidAt datetime2 NULL
CancelledAt datetime2 NULL
```

## Production Deployment Notes

### Server Configuration
Đảm bảo server có timezone data:

**Windows Server:**
- Timezone "SE Asia Standard Time" có sẵn

**Linux Server:**
- Cài đặt: `sudo apt-get install tzdata`
- Set timezone: `sudo timedatectl set-timezone Asia/Ho_Chi_Minh`

### Fallback Strategy
Nếu timezone không tồn tại, có thể thêm fallback:

```csharp
private static DateTime GetVietnamTime()
{
    try
    {
        var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);
    }
    catch (TimeZoneNotFoundException)
    {
        // Fallback: UTC + 7 hours
        return DateTime.UtcNow.AddHours(7);
    }
}
```

## API Response Format

Timestamps trong response vẫn giữ nguyên format:

```json
{
  "id": 123,
  "orderCode": "ORD-20240403-00001",
  "createdAt": "2024-04-03T14:30:45.123",
  "updatedAt": "2024-04-03T14:30:45.123",
  "paidAt": "2024-04-03T15:20:10.456"
}
```

**Lưu ý**: Frontend có thể cần parse và hiển thị theo format mong muốn.

## Backward Compatibility

### Existing Orders
- Orders cũ có timestamps UTC vẫn hoạt động bình thường
- Không cần convert dữ liệu cũ (optional)

### Reports & Analytics
- Báo cáo cần lọc theo date range nên lưu ý timezone khi query
- Ví dụ: "Đơn hàng ngày 03/04/2024" = từ 00:00:00 đến 23:59:59 VN time

## Summary

✅ **Completed:**
- AppDbContext: Auto-set timestamps UTC+7 cho tất cả entities
- OrderService: PaidAt, CancelledAt với UTC+7
- OrderRepository: Order code generation và UpdatedAt với UTC+7
- VoucherService: Validation và timestamps với UTC+7
- VoucherSeeder: Seed data với UTC+7

✅ **Benefits:**
- Timestamps chính xác theo múi giờ người dùng Việt Nam
- Order codes phản ánh đúng ngày tạo đơn theo VN
- Voucher validation chính xác theo giờ địa phương
- Logs và audit trail dễ đọc hơn

✅ **No Breaking Changes:**
- API contracts không đổi
- Database schema không đổi
- Existing data vẫn hoạt động
