# Order Address Refactoring - UserAddressId Integration

## Tổng quan

Thay vì yêu cầu frontend gửi các trường địa chỉ riêng lẻ (`RecipientName`, `ShippingAddress`, `PhoneNumber`), API đã được refactor để chỉ cần gửi `UserAddressId`. Backend sẽ tự động snapshot thông tin địa chỉ từ `UserAddress` vào `Order`.

## Lợi ích

1. **API sạch hơn**: Giảm số lượng fields trong request
2. **Tính nhất quán**: Đảm bảo địa chỉ luôn đồng bộ với dữ liệu trong database
3. **Giảm lỗi**: Không cần frontend tự lấy và gửi từng trường
4. **Bảo toàn lịch sử**: Sử dụng snapshot pattern để order không bị ảnh hưởng khi user update/delete địa chỉ

## Thay đổi API

### 1. CreateOrder API

**Trước đây:**
```json
POST /api/orders
{
  "userId": 1,
  "note": "Ghi chú",
  "shippingAddress": "123 Đường ABC, Quận 1, TP.HCM",
  "phoneNumber": "0901234567",
  "items": [...]
}
```

**Bây giờ:**
```json
POST /api/orders
{
  "userId": 1,
  "userAddressId": 5,
  "note": "Ghi chú",
  "items": [...]
}
```

### 2. UpdateOrder API

**Trước đây:**
```json
PUT /api/orders/{orderId}
{
  "note": "Ghi chú mới",
  "shippingAddress": "456 Đường XYZ",
  "phoneNumber": "0909876543"
}
```

**Bây giờ:**
```json
PUT /api/orders/{orderId}
{
  "note": "Ghi chú mới",
  "userAddressId": 7
}
```

## Response Format (không đổi)

Response vẫn trả về đầy đủ thông tin địa chỉ đã được snapshot:

```json
{
  "id": 123,
  "orderCode": "ORD-20240315-0001",
  "userId": 1,
  "userName": "Nguyễn Văn A",
  "status": 1,
  "recipientName": "Nguyễn Văn A",
  "shippingAddress": "123 Đường ABC, Quận 1, TP.HCM",
  "phoneNumber": "0901234567",
  "subTotal": 100000,
  "discountAmount": 10000,
  "shippingFee": 15000,
  "finalAmount": 105000,
  "items": [...]
}
```

## Business Logic

### CreateOrder

1. Frontend gửi `UserAddressId` (optional)
2. Backend validate:
   - Kiểm tra `UserAddressId` có tồn tại
   - Kiểm tra địa chỉ có thuộc về `UserId` không
3. Backend snapshot:
   - `order.RecipientName = userAddress.RecipientName`
   - `order.ShippingAddress = userAddress.AddressLine`
   - `order.PhoneNumber = userAddress.PhoneNumber`
4. Lưu order với địa chỉ đã snapshot

### UpdateOrder

- Tương tự CreateOrder
- Chỉ cho phép update khi order ở trạng thái `Draft` hoặc `Pending`
- Nếu `UserAddressId` được cung cấp, sẽ thay thế hoàn toàn địa chỉ hiện tại

### CheckoutOrder

- Tiếp tục sử dụng `UserAddressId` (đã có từ trước)
- Logic snapshot tương tự

## Snapshot Pattern

**Tại sao không dùng Foreign Key?**

Order lưu **bản sao** (snapshot) của địa chỉ thay vì FK đến `UserAddress` vì:

1. **Bảo toàn lịch sử**: Khi user update/delete địa chỉ, order cũ vẫn giữ nguyên thông tin giao hàng ban đầu
2. **Immutability**: Order đã hoàn thành không nên bị thay đổi
3. **Compliance**: Đáp ứng yêu cầu kiểm toán và báo cáo

## Validation Rules

### CreateOrder
- ✅ `UserAddressId` là optional
- ✅ Nếu có `UserAddressId`, bắt buộc phải có `UserId`
- ✅ Địa chỉ phải thuộc về user đang tạo order
- ❌ Ném exception nếu `UserAddressId` không hợp lệ

### UpdateOrder
- ✅ Chỉ update khi order ở trạng thái `Draft` hoặc `Pending`
- ✅ `UserAddressId` là optional (nếu không gửi thì không update địa chỉ)
- ✅ Validate ownership tương tự CreateOrder

### CheckoutOrder
- ✅ `UserAddressId` là **bắt buộc** (vì đây là bước finalize order)
- ✅ Validate ownership

## Migration Guide cho Frontend

### Before
```typescript
// Tạo order mới
const createOrderRequest = {
  userId: currentUser.id,
  shippingAddress: selectedAddress.fullAddress,
  phoneNumber: selectedAddress.phoneNumber,
  note: orderNote,
  items: cartItems
};

await api.post('/orders', createOrderRequest);
```

### After
```typescript
// Tạo order mới - chỉ cần gửi ID
const createOrderRequest = {
  userId: currentUser.id,
  userAddressId: selectedAddress.id,  // Chỉ cần ID!
  note: orderNote,
  items: cartItems
};

await api.post('/orders', createOrderRequest);
```

## Testing Scenarios

### Test Case 1: Create Order với UserAddressId hợp lệ
```json
POST /api/orders
{
  "userId": 1,
  "userAddressId": 5,
  "items": [{"productId": 10, "quantity": 2}]
}
```
**Expected**: Order được tạo với địa chỉ từ UserAddress #5

### Test Case 2: Create Order với UserAddressId không thuộc về user
```json
POST /api/orders
{
  "userId": 1,
  "userAddressId": 999,  // Thuộc về user khác
  "items": [...]
}
```
**Expected**: `ArgumentException` - "Địa chỉ giao hàng không hợp lệ hoặc không thuộc về bạn"

### Test Case 3: Create Order không có UserAddressId
```json
POST /api/orders
{
  "userId": 1,
  "note": "Ghi chú",
  "items": [...]
}
```
**Expected**: Order được tạo thành công, các trường địa chỉ để null

### Test Case 4: Update Order với UserAddressId mới
```json
PUT /api/orders/123
{
  "userAddressId": 7
}
```
**Expected**: Address fields được update từ UserAddress #7

## Code Changes Summary

### Modified Files
1. **CoffeeShopApi/DTOs/OrderDto.cs**
   - `CreateOrderRequest`: Thêm `UserAddressId`, xóa `ShippingAddress` và `PhoneNumber`
   - `UpdateOrderRequest`: Thay `ShippingAddress` và `PhoneNumber` bằng `UserAddressId`

2. **CoffeeShopApi/Services/OrderService.cs**
   - `CreateOrderAsync()`: Thêm logic snapshot địa chỉ từ UserAddressId
   - `UpdateOrderAsync()`: Thêm logic snapshot địa chỉ từ UserAddressId

### No Changes Needed
- `OrderResponse`: Vẫn giữ nguyên `RecipientName`, `ShippingAddress`, `PhoneNumber`
- `CheckoutOrderAsync()`: Đã sử dụng UserAddressId từ trước
- Database schema: Không cần migration

## Notes

- API backward compatibility: ❌ Breaking change - frontend cần update
- Database migration: ✅ Không cần (chỉ thay đổi logic, không đổi schema)
- Existing data: ✅ Không ảnh hưởng (orders cũ vẫn có địa chỉ snapshot)
