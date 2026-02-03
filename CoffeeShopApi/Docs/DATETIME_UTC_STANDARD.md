# DateTime UTC Standard - Chuẩn hóa thời gian trong hệ thống

## ✅ Quyết định cuối cùng: LƯU UTC VÀO DATABASE

Sau nhiều lần thử nghiệm, chúng tôi quyết định **lưu tất cả DateTime dưới dạng UTC** trong database để đảm bảo tính nhất quán và tuân thủ best practice quốc tế.

---

## 🎯 Nguyên tắc cơ bản

### 1. **Lưu trữ: UTC**
Tất cả DateTime fields trong database **LUÔN LÀ UTC**:
- `CreatedAt`, `UpdatedAt`: UTC
- `PaidAt`, `CancelledAt`: UTC
- `StartDate`, `EndDate` (Voucher): UTC

### 2. **Hiển thị: Convert khi cần**
- API Response có thể trả về UTC (frontend tự convert)
- Hoặc có thể thêm computed property để convert sang giờ Việt Nam

### 3. **Input: Frontend gửi UTC**
- Frontend chuyển local time sang UTC trước khi gửi lên backend
- Backend nhận UTC, lưu thẳng vào DB

---

## 📁 Files đã cập nhật

### 1. **AppDbContext.cs**
```csharp
public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
{
    var now = DateTime.UtcNow; // ✅ Dùng UTC

    foreach (var entry in entries)
    {
        if (entry.State == EntityState.Added)
        {
            createdAtProperty.CurrentValue = now; // Set CreatedAt = UTC
        }
        
        if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
        {
            updatedAtProperty.CurrentValue = now; // Set UpdatedAt = UTC
        }
    }
}
```

**Tác dụng:** Tự động set `CreatedAt`, `UpdatedAt` = UTC cho mọi entity khi thêm/sửa.

---

### 2. **OrderService.cs**
```csharp
// ✅ Đã sửa
order.PaidAt = DateTime.UtcNow;
order.CancelledAt = DateTime.UtcNow;

// ❌ Đã XÓA
// private static DateTime GetVietnamTime() { ... }
```

**Thay đổi:**
- `MarkAsPaidAsync()`: `PaidAt = DateTime.UtcNow`
- `CancelOrderAsync()`: `CancelledAt = DateTime.UtcNow`
- **Xóa method `GetVietnamTime()`**

---

### 3. **OrderRepository.cs**
```csharp
// ✅ Đã sửa
order.UpdatedAt = DateTime.UtcNow;

public async Task<string> GenerateOrderCodeAsync()
{
    var today = DateTime.UtcNow.ToString("yyyyMMdd"); // ✅ Dùng UTC
    // Order code: ORD-20240403-00001
}
```

**Lưu ý:** Order code vẫn dùng UTC date, nhưng **không ảnh hưởng lớn** vì chỉ là prefix.

---

### 4. **VoucherService.cs**
```csharp
// ✅ Validate voucher
var now = DateTime.UtcNow;
if (now < voucher.StartDate) { ... }
if (now > voucher.EndDate) { ... }

// ✅ Create voucher
CreatedAt = DateTime.UtcNow,
UpdatedAt = DateTime.UtcNow
```

---

### 5. **VoucherSeeder.cs**
```csharp
// ✅ Seed vouchers
var now = DateTime.UtcNow;

new Voucher
{
    StartDate = now.AddDays(-30), // UTC
    EndDate = now.AddDays(60),     // UTC
    CreatedAt = now,
    UpdatedAt = now
}
```

---

## 🔄 Nếu muốn hiển thị giờ Việt Nam

### Option 1: Frontend tự convert
```javascript
// JavaScript/React
const vietnamTime = new Date(utcTimestamp).toLocaleString('vi-VN', {
  timeZone: 'Asia/Ho_Chi_Minh'
});
```

### Option 2: Thêm computed property trong DTO
```csharp
public class OrderResponse
{
    public DateTime CreatedAt { get; set; } // UTC from DB
    
    // Computed property (không lưu DB)
    public DateTime CreatedAtVN => ToVietnamTime(CreatedAt);
    
    private static DateTime ToVietnamTime(DateTime utc)
    {
        var tz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        return TimeZoneInfo.ConvertTimeFromUtc(utc, tz);
    }
}
```

---

## 📊 So sánh UTC vs Local Time

| Tiêu chí | UTC (✅ Chọn) | Local Time (❌ Không chọn) |
|----------|---------------|---------------------------|
| Tính nhất quán | ✅ Luôn đồng nhất | ❌ Phụ thuộc server timezone |
| Query dễ dàng | ✅ Không cần convert | ❌ Phải xử lý timezone |
| Deploy nhiều nơi | ✅ Không vấn đề | ❌ Lỗi nếu server khác timezone |
| Daylight Saving | ✅ Không ảnh hưởng | ❌ Có thể lỗi DST |
| Best Practice | ✅ Chuẩn quốc tế | ❌ Không khuyến khích |

---

## 🧪 Testing

### Test 1: Create Order
```sh
POST /api/orders
Response:
{
  "createdAt": "2024-04-03T02:30:45Z", // UTC (có chữ Z)
  "updatedAt": "2024-04-03T02:30:45Z"
}
```

### Test 2: Mark as Paid
```sh
PUT /api/orders/123/mark-paid
Response:
{
  "paidAt": "2024-04-03T03:15:20Z" // UTC
}
```

### Test 3: Cancel Order
```sh
PUT /api/orders/123/cancel
Response:
{
  "cancelledAt": "2024-04-03T04:00:00Z" // UTC
}
```

**Kết quả mong đợi:**
- Tất cả timestamps đều có suffix `Z` (= UTC)
- Không chênh lệch 7 tiếng giữa các fields
- CreatedAt và UpdatedAt chỉ chênh vài milliseconds

---

## ⚠️ Lưu ý quan trọng

1. **Database**: Columns `datetime2` lưu UTC
2. **EF Core**: Mặc định serialize DateTime có `Kind=Utc` thành `"2024-04-03T02:30:45Z"`
3. **Frontend**: Cần parse UTC và convert sang local time khi hiển thị
4. **Voucher dates**: StartDate/EndDate cũng là UTC, frontend cần aware khi chọn date

---

## 📝 Migration notes

### Dữ liệu cũ (nếu có)
Nếu database có dữ liệu cũ lưu theo giờ VN (UTC+7), có 2 cách:

#### Option 1: Convert data (khuyến nghị nếu data ít)
```sql
-- Convert về UTC (trừ 7 tiếng)
UPDATE Orders SET CreatedAt = DATEADD(HOUR, -7, CreatedAt);
UPDATE Orders SET UpdatedAt = DATEADD(HOUR, -7, UpdatedAt);
UPDATE Orders SET PaidAt = DATEADD(HOUR, -7, PaidAt) WHERE PaidAt IS NOT NULL;
```

#### Option 2: Không convert (chấp nhận sai lệch)
- Dữ liệu cũ giữ nguyên
- Dữ liệu mới từ sau khi deploy sẽ là UTC
- Có thể hiển thị cả 2 chuẩn (với note cho user)

---

## ✅ Checklist triển khai

- [x] AppDbContext: Dùng `DateTime.UtcNow` trong `SaveChangesAsync()`
- [x] OrderService: `PaidAt`, `CancelledAt` = UTC
- [x] OrderRepository: `UpdatedAt` = UTC, `GenerateOrderCodeAsync()` dùng UTC
- [x] VoucherService: Validate và create với UTC
- [x] VoucherSeeder: Seed data với UTC
- [x] Xóa tất cả `GetVietnamTime()` helper methods
- [ ] Update documentation cho frontend team
- [ ] Test thoroughly trên dev/staging
- [ ] (Optional) Convert dữ liệu cũ nếu cần

---

## 🎓 Tài liệu tham khảo

- [Microsoft Docs: DateTime Best Practices](https://docs.microsoft.com/en-us/dotnet/standard/datetime/choosing-between-datetime)
- [Why UTC?](https://stackoverflow.com/questions/2532729/daylight-saving-time-and-time-zone-best-practices)
- [EF Core DateTime Handling](https://learn.microsoft.com/en-us/ef/core/modeling/value-conversions)

---

**Tóm lại:** 
- ✅ **Lưu UTC**
- ✅ **Convert khi hiển thị** (nếu cần)
- ✅ **Đơn giản, nhất quán, chuẩn quốc tế**
