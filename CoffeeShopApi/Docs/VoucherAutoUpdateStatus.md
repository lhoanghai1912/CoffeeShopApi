# Hệ Thống Tự Động Cập Nhật Trạng Thái Voucher

## 📋 Tổng Quan

Hệ thống tự động cập nhật `IsActive` của voucher dựa trên `StartDate` và `EndDate`, đảm bảo:
- Voucher chỉ active khi trong thời gian hiệu lực
- Tự động inactive khi hết hạn hoặc chưa đến thời gian

## 🔧 Cấu Trúc

### 1. VoucherService - Method Cập Nhật

**Method:** `UpdateVoucherActiveStatusAsync()`

**Logic:**
```
- Nếu now < StartDate:       IsActive = false (chưa đến thời gian)
- Nếu StartDate <= now <= EndDate:  IsActive = true  (trong thời gian hiệu lực)
- Nếu now > EndDate:          IsActive = false (đã hết hạn)
```

**Return:** Số lượng voucher đã được cập nhật

### 2. VoucherStatusUpdateService - Background Service

**Đặc điểm:**
- Chạy tự động mỗi 1 giờ (có thể thay đổi)
- Chạy lần đầu ngay khi app khởi động
- Ghi log chi tiết về quá trình update

**Cấu hình interval:**
```csharp
private readonly TimeSpan _updateInterval = TimeSpan.FromHours(1); // Mặc định 1 giờ
```

Thay đổi nếu cần:
```csharp
TimeSpan.FromMinutes(30)  // 30 phút
TimeSpan.FromHours(6)     // 6 giờ
TimeSpan.FromDays(1)      // 1 ngày
```

### 3. API Endpoint - Trigger Thủ Công

**Endpoint:** `POST /api/vouchers/update-status`

**Mô tả:** Admin có thể trigger update thủ công bất kỳ lúc nào

**Response:**
```json
{
  "isSuccess": true,
  "data": {
    "updatedCount": 5
  },
  "message": "Đã cập nhật trạng thái cho 5 voucher",
  "errors": null
}
```

## 📊 Logs

Service ghi log tại các thời điểm:
- **Startup:** Khi service bắt đầu
- **Update Success:** Khi có voucher được cập nhật
- **Update Skip:** Khi không có voucher cần update (Debug level)
- **Error:** Khi có lỗi xảy ra
- **Shutdown:** Khi service dừng

**Xem logs:**
```
[Information] VoucherStatusUpdateService started at: 2026-02-03 10:00:00
[Information] Starting voucher status update at: 2026-02-03 10:00:00
[Information] Updated 3 vouchers at: 2026-02-03 10:00:00
```

## 🚀 Kích Hoạt

Service đã được đăng ký trong `Program.cs`:
```csharp
builder.Services.AddHostedService<VoucherStatusUpdateService>();
```

**Tự động hoạt động khi:**
- App khởi động (kể cả development/production)
- Chạy background không blocking main thread
- Tự động restart nếu có lỗi

## 🧪 Test

### Test 1: Voucher Chưa Đến Thời Gian
```sql
-- Tạo voucher bắt đầu từ ngày mai
INSERT INTO Vouchers (Code, StartDate, EndDate, IsActive, ...) 
VALUES ('FUTURE', DATEADD(day, 1, GETDATE()), DATEADD(day, 7, GETDATE()), 1, ...)

-- Sau khi service chạy -> IsActive = 0
```

### Test 2: Voucher Đã Hết Hạn
```sql
-- Tạo voucher đã hết hạn nhưng vẫn active
INSERT INTO Vouchers (Code, StartDate, EndDate, IsActive, ...) 
VALUES ('EXPIRED', DATEADD(day, -7, GETDATE()), DATEADD(day, -1, GETDATE()), 1, ...)

-- Sau khi service chạy -> IsActive = 0
```

### Test 3: Voucher Trong Thời Gian Hiệu Lực
```sql
-- Tạo voucher hợp lệ nhưng bị inactive
INSERT INTO Vouchers (Code, StartDate, EndDate, IsActive, ...) 
VALUES ('VALID', DATEADD(day, -1, GETDATE()), DATEADD(day, 7, GETDATE()), 0, ...)

-- Sau khi service chạy -> IsActive = 1
```

### Test 4: Trigger Thủ Công
```bash
POST http://localhost:5000/api/vouchers/update-status
```

## ⚙️ Cấu Hình Nâng Cao

### Thay Đổi Interval
**File:** `VoucherStatusUpdateService.cs`
```csharp
private readonly TimeSpan _updateInterval = TimeSpan.FromMinutes(30); // Từ 1h -> 30 phút
```

### Disable Service (Nếu Cần)
**File:** `Program.cs`
```csharp
// Comment dòng này:
// builder.Services.AddHostedService<VoucherStatusUpdateService>();
```

### Chỉ Chạy Ở Production
```csharp
if (app.Environment.IsProduction())
{
    builder.Services.AddHostedService<VoucherStatusUpdateService>();
}
```

## 🎯 Use Cases

1. **Marketing Campaign:** 
   - Tạo voucher trước, tự động active đúng giờ khuyến mãi
   - Tự động inactive sau khi campaign kết thúc

2. **Flash Sale:**
   - Voucher tự động active vào 0h00
   - Tự động inactive sau 24h

3. **Seasonal Promotion:**
   - Voucher Tết tự động active từ 29 Tết đến Mùng 10
   - Không cần nhớ enable/disable thủ công

4. **User Birthday:**
   - Private voucher active từ sinh nhật đến 7 ngày sau
   - Tự động hết hạn

## 📝 Notes

- Service sử dụng `GetVietnamTime()` cho tất cả so sánh thời gian (UTC+7)
- Atomic transaction: Cập nhật từng batch và commit
- Error handling: Lỗi không crash app, chỉ ghi log
- Performance: Chỉ query và update vouchers cần thiết
