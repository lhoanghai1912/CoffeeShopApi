# 📋 THIẾT KẾ ORDER & ORDERITEMS - COFFEESHOPAPP

## 1️⃣ TỔNG QUAN THIẾT KẾ

### 1.1 Cấu trúc bảng dữ liệu

```
┌─────────────┐     ┌─────────────────┐     ┌─────────────────────┐
│   Orders    │────<│   OrderItems    │────<│  OrderItemOptions   │
└─────────────┘     └─────────────────┘     └─────────────────────┘
      │                    │                         │
      │                    │                         │
      ▼                    ▼                         ▼
┌─────────────┐     ┌─────────────────┐     ┌─────────────────────┐
│    Users    │     │    Products     │     │  OptionGroups       │
└─────────────┘     └─────────────────┘     │  OptionItems        │
                                            └─────────────────────┘
```

---

## 2️⃣ BẢNG ORDERS

### Các trường và ý nghĩa:

| Field | Type | Mô tả |
|-------|------|-------|
| `Id` | int | Primary key |
| `OrderCode` | string(50) | Mã đơn hàng duy nhất (ORD-yyyyMMdd-xxxxx) |
| `UserId` | int? | FK → Users, nullable cho khách vãng lai POS |
| `Status` | enum | Trạng thái đơn hàng |
| `SubTotal` | decimal | Tổng tiền trước giảm giá |
| `DiscountAmount` | decimal | Số tiền được giảm (từ Voucher) |
| `ShippingFee` | decimal | Phí ship |
| `FinalAmount` | decimal | = SubTotal - DiscountAmount + ShippingFee |
| `VoucherId` | int? | FK → Vouchers (nullable, implement sau) |
| `Note` | string(500) | Ghi chú của khách |
| `ShippingAddress` | string(500) | Địa chỉ giao hàng |
| `PhoneNumber` | string(20) | SĐT nhận hàng |
| `CreatedAt` | DateTime | Thời gian tạo |
| `UpdatedAt` | DateTime | Thời gian cập nhật |
| `PaidAt` | DateTime? | Thời gian thanh toán |
| `CancelledAt` | DateTime? | Thời gian hủy |
| `CancelReason` | string(500) | Lý do hủy |

### OrderStatus Enum:

```csharp
public enum OrderStatus
{
    Draft = 0,       // Nháp - cho phép chỉnh sửa
    Pending = 1,     // Chờ xử lý (đã checkout)
    Confirmed = 2,   // Đã xác nhận, đang chuẩn bị
    Delivering = 3,  // Đang giao
    Paid = 4,        // Đã thanh toán
    Completed = 5,   // Hoàn thành
    Cancelled = 6    // Đã hủy
}
```

---

## 3️⃣ BẢNG ORDERITEMS

### Các trường và ý nghĩa:

| Field | Type | Mô tả |
|-------|------|-------|
| `Id` | int | Primary key |
| `OrderId` | int | FK → Orders |
| `ProductId` | int | FK → Products (reference only) |
| `Quantity` | int | Số lượng |
| `BasePrice` | decimal | **[SNAPSHOT]** Giá gốc tại thời điểm đặt |
| `ProductName` | string | **[SNAPSHOT]** Tên sản phẩm |
| `ProductImageUrl` | string? | **[SNAPSHOT]** Ảnh sản phẩm |
| `OptionPrice` | decimal | Tổng giá các options |
| `UnitPrice` | decimal | = BasePrice + OptionPrice |
| `TotalPrice` | decimal | = UnitPrice × Quantity |
| `Note` | string(200) | Ghi chú riêng cho item |
| `CreatedAt` | DateTime | Thời gian tạo |

### Tại sao SNAPSHOT?

1. **Product có thể bị disable/xóa** sau khi user đặt hàng
2. **Giá có thể thay đổi** - đơn hàng cũ phải giữ giá cũ
3. **Tên/ảnh có thể thay đổi** - lịch sử phải chính xác

---

## 4️⃣ BẢNG ORDERITEMOPTIONS

### Các trường và ý nghĩa:

| Field | Type | Mô tả |
|-------|------|-------|
| `Id` | int | Primary key |
| `OrderItemId` | int | FK → OrderItems |
| `OptionGroupId` | int | FK → OptionGroups (reference) |
| `OptionItemId` | int | FK → OptionItems (reference) |
| `OptionGroupName` | string | **[SNAPSHOT]** Tên nhóm option |
| `OptionItemName` | string | **[SNAPSHOT]** Tên option đã chọn |
| `PriceAdjustment` | decimal | **[SNAPSHOT]** Giá điều chỉnh |

### Tại sao không join trực tiếp OptionItems?

1. OptionItem có thể bị **xóa** sau khi order tạo
2. OptionItem có thể **đổi tên** (ví dụ: "Size L" → "Size Large")
3. **PriceAdjustment** có thể thay đổi
4. Đảm bảo **lịch sử order** luôn chính xác như lúc khách đặt

---

## 5️⃣ LUỒNG XỬ LÝ ORDER (BUSINESS FLOW)

### 5.1 Tạo Order

```
POST /api/orders
{
    "userId": 1,
    "note": "Giao trước 12h",
    "shippingAddress": "123 ABC Street",
    "phoneNumber": "0901234567",
    "items": [
        {
            "productId": 1,
            "quantity": 2,
            "selectedOptionItemIds": [1, 5, 10],
            "note": "Ít đá"
        }
    ]
}
```

**Flow:**
1. Tạo Order với status = Draft
2. Validate từng item:
   - Product tồn tại?
   - OptionGroup IsRequired có được chọn?
   - OptionGroup AllowMultiple = false có chọn nhiều không?
   - OptionItems có thuộc Product không?
3. Snapshot giá và tên
4. Tính UnitPrice, TotalPrice
5. Tính SubTotal, FinalAmount

### 5.2 Thêm Item

```
POST /api/orders/{orderId}/items
{
    "productId": 2,
    "quantity": 1,
    "selectedOptionItemIds": [3, 7]
}
```

**Điều kiện:** Order.Status == Draft

### 5.3 Cập nhật Item

```
PUT /api/orders/{orderId}/items/{itemId}
{
    "quantity": 3,
    "selectedOptionItemIds": [1, 6, 10],
    "note": "Nhiều đá"
}
```

**Flow:**
1. Validate order status == Draft
2. Xóa options cũ
3. Validate và thêm options mới
4. Re-calculate giá

### 5.4 Checkout

```
POST /api/orders/{orderId}/checkout
{
    "voucherId": 5,
    "shippingAddress": "Updated address"
}
```

**Flow:**
1. Validate order không trống
2. Validate lại tất cả products/options còn tồn tại
3. Apply voucher (nếu có)
4. Lock order: Draft → Pending

### 5.5 Cancel Order

```
POST /api/orders/{orderId}/cancel
{
    "reason": "Khách đổi ý"
}
```

**Điều kiện:** Status != Paid && Status != Completed

---

## 6️⃣ VALIDATION RULES

### 6.1 OptionGroup Validation

```csharp
// IsRequired: Bắt buộc phải chọn ít nhất 1 option
if (group.IsRequired && !selectedInGroup.Any())
    errors.Add($"Nhóm '{group.Name}' là bắt buộc");

// AllowMultiple = false: Chỉ được chọn 1
if (!group.AllowMultiple && selectedInGroup.Count > 1)
    errors.Add($"Nhóm '{group.Name}' chỉ được chọn 1 option");
```

### 6.2 FatherId Logic

FatherId dùng để xử lý **OptionGroup phụ thuộc**:
- Ví dụ: Chọn "Size L" mới hiện "Extra shot espresso"
- Khi validate, kiểm tra FatherId có trong selectedOptions không

---

## 7️⃣ EDGE CASES & RULES

### 7.1 Product bị disable sau khi user thêm vào order
- **Draft order:** Validate khi checkout → báo lỗi
- **Paid order:** Vẫn hiển thị bình thường (đã snapshot)

### 7.2 OptionItem bị xóa
- **Draft order:** Validate khi checkout → báo lỗi
- **Paid order:** Vẫn hiển thị (đã snapshot tên + giá)

### 7.3 Giá Product thay đổi
- Order đã tạo giữ **giá snapshot cũ**
- Không tự động update giá

### 7.4 User chỉnh sửa order ở nhiều thiết bị
- Sử dụng **Transaction** khi update
- Client nên refresh order trước khi edit

### 7.5 Order timeout
- Có thể thêm job **auto-cancel Draft orders** sau X giờ
- Kiểm tra `CreatedAt` + timeout duration

---

## 8️⃣ BEST PRACTICES ĐÃ ÁP DỤNG

✅ **Snapshot giá tại OrderItem** - đảm bảo lịch sử chính xác

✅ **Enum cho OrderStatus** - type-safe, dễ maintain

✅ **Transaction khi checkout** - đảm bảo tính toàn vẹn

✅ **Không tin frontend** - validate tất cả ở backend

✅ **Repository Pattern** - tách biệt data access

✅ **Service Layer** - business logic tập trung

---

## 9️⃣ API ENDPOINTS

### Query
- `GET /api/orders` - Lấy tất cả orders
- `GET /api/orders/{id}` - Lấy order theo ID
- `GET /api/orders/code/{orderCode}` - Lấy theo mã đơn
- `GET /api/orders/user/{userId}` - Lấy orders của user
- `GET /api/orders/status/{status}` - Lấy theo trạng thái

### Commands
- `POST /api/orders` - Tạo order mới
- `POST /api/orders/{id}/items` - Thêm item
- `PUT /api/orders/{id}/items/{itemId}` - Cập nhật item
- `DELETE /api/orders/{id}/items/{itemId}` - Xóa item
- `PUT /api/orders/{id}` - Cập nhật thông tin order
- `POST /api/orders/{id}/checkout` - Checkout
- `POST /api/orders/{id}/confirm` - Xác nhận
- `POST /api/orders/{id}/pay` - Đánh dấu đã thanh toán
- `POST /api/orders/{id}/cancel` - Hủy order
- `DELETE /api/orders/{id}` - Xóa order (chỉ Draft/Cancelled)

---

## 🔟 MIGRATION

Chạy lệnh sau để apply migration:

```bash
cd CoffeeShopApi
dotnet ef database update
```

---

## 📝 TODO - MỞ RỘNG

- [ ] Voucher entity & logic
- [ ] Payment integration
- [ ] Order history pagination
- [ ] Real-time order status updates (SignalR)
- [ ] Unit tests cho OrderService
- [ ] Admin dashboard cho quản lý orders
