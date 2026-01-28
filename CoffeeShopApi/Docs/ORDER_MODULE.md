# Order Module Documentation

## 📋 Overview

Order Module quản lý toàn bộ vòng đời đơn hàng từ Draft → Pending → Confirmed → Paid → Completed.

**Controller:** `OrdersController`  
**Service:** `OrderService`  
**Repository:** `OrderRepository`  
**Entities:** `Order`, `OrderItem`, `OrderItemOption`

---

## 🔄 Order Lifecycle

```
┌──────────┐     ┌─────────┐     ┌───────────┐     ┌──────┐     ┌───────────┐
│  Draft   │────>│ Pending │────>│ Confirmed │────>│ Paid │────>│ Completed │
└──────────┘     └─────────┘     └───────────┘     └──────┘     └───────────┘
     │                │                 │              │
     │                └─────────────────┴──────────────┘
     │                            │
     └────────────────────────────▼
                             ┌───────────┐
                             │ Cancelled │
                             └───────────┘
```

### Status Transitions

| From | To | Triggered By | Conditions |
|------|-----|--------------|-----------|
| (New) | **Draft** | `POST /orders` | Auto on create |
| Draft | **Pending** | `POST /orders/{id}/checkout` | Must have items + address |
| Pending | **Confirmed** | `POST /orders/{id}/confirm` | Staff only |
| Confirmed | **Paid** | `POST /orders/{id}/pay` | Payment received |
| Paid | **Completed** | Manual/Auto | Delivered |
| Draft/Pending/Confirmed | **Cancelled** | `POST /orders/{id}/cancel` | Before Paid |

---

## 📡 API Endpoints

### 1. Create Draft Order

**Endpoint:** `POST /api/orders`

**Request Body:**
```json
{
  "userId": 1,
  "note": "Ghi chú đơn hàng",
  "items": [
    {
      "productId": 1,
      "quantity": 2,
      "note": "Ít đường",
      "selectedOptionItemIds": [1, 5, 10]  // Size S, 50% sugar, Trân châu đen
    },
    {
      "productId": 5,
      "quantity": 1,
      "selectedOptionItemIds": [2, 8]  // Size M, 70% sugar
    }
  ]
}
```

**Response:**
```json
{
  "success": true,
  "message": "Tạo đơn hàng thành công",
  "data": {
    "id": 123,
    "orderCode": "ORD-20250128-00123",
    "userId": 1,
    "status": "Draft",
    "subTotal": 89000,
    "discountAmount": 0,
    "shippingFee": 0,
    "finalAmount": 89000,
    "note": "Ghi chú đơn hàng",
    "orderItems": [
      {
        "id": 1,
        "productId": 1,
        "productName": "Cà Phê Đen Đá",
        "productImageUrl": "/images/caphedenda.jpg",
        "quantity": 2,
        "unitPrice": 25000,
        "totalPrice": 70000,
        "note": "Ít đường",
        "selectedOptions": [
          { "id": 1, "name": "Nhỏ (S)", "priceAdjustment": 0 },
          { "id": 5, "name": "50%", "priceAdjustment": 0 },
          { "id": 10, "name": "Trân châu đen", "priceAdjustment": 10000 }
        ]
      }
    ],
    "createdAt": "2025-01-28T10:30:00Z"
  }
}
```

---

### 2. Add Item to Order

**Endpoint:** `POST /api/orders/{orderId}/items`

**Conditions:** Order phải ở trạng thái **Draft**

**Request Body:**
```json
{
  "productId": 3,
  "quantity": 1,
  "note": "Nhiều đá",
  "selectedOptionItemIds": [2, 7, 11, 13]  // Size M, 70%, Trân châu + Pudding
}
```

**Response:** Order object đầy đủ (như Create Order)

---

### 3. Update Order Item

**Endpoint:** `PUT /api/orders/{orderId}/items/{itemId}`

**Conditions:** Order phải ở trạng thái **Draft**

**Request Body:**
```json
{
  "quantity": 3,
  "note": "Ít đường, nhiều đá",
  "selectedOptionItemIds": [1, 5, 10]
}
```

---

### 4. Remove Item from Order

**Endpoint:** `DELETE /api/orders/{orderId}/items/{itemId}`

**Conditions:** Order phải ở trạng thái **Draft**

**Response:**
```json
{
  "success": true,
  "message": "Xóa sản phẩm thành công",
  "data": { ... }  // Order updated
}
```

---

### 5. Update Order Info

**Endpoint:** `PUT /api/orders/{orderId}`

**Conditions:** Order phải ở trạng thái **Draft**

**Request Body:**
```json
{
  "note": "Giao trước 5pm",
  "shippingAddress": "123 Nguyễn Huệ, Q1, TP.HCM",
  "phoneNumber": "0912345678"
}
```

---

### 6. Checkout Order (Draft → Pending)

**Endpoint:** `POST /api/orders/{orderId}/checkout`

**Conditions:**
- Order phải có ít nhất 1 item
- Phải chọn địa chỉ giao hàng
- Order đang ở trạng thái **Draft**

**Request Body:**
```json
{
  "userAddressId": 5,
  "voucherId": 3,  // Optional
  "note": "Giao trước 5pm"
}
```

**Business Logic:**
1. ✅ Validate order items (products still exist, prices unchanged)
2. ✅ **Snapshot address** từ UserAddress vào Order
3. ✅ Apply voucher (nếu có)
   - Validate voucher (còn hạn, đủ điều kiện)
   - Atomic increment usage count
   - Calculate discount
4. ✅ Recalculate totals
5. ✅ Change status to **Pending**
6. ✅ If any step fails → Rollback transaction

**Response:**
```json
{
  "success": true,
  "message": "Đặt hàng thành công",
  "data": {
    "id": 123,
    "orderCode": "ORD-20250128-00123",
    "status": "Pending",
    "subTotal": 89000,
    "discountAmount": 10000,
    "shippingFee": 20000,
    "finalAmount": 99000,
    "recipientName": "Nguyễn Văn A",
    "shippingAddress": "123 Nguyễn Huệ, Q1, TP.HCM",
    "phoneNumber": "0912345678",
    "voucherId": 3,
    "voucherCode": "WELCOME10K",
    ...
  }
}
```

---

### 7. Confirm Order (Staff)

**Endpoint:** `POST /api/orders/{orderId}/confirm`

**Authorization:** STAFF or ADMIN

**Conditions:** Order phải ở trạng thái **Pending**

**Response:**
```json
{
  "success": true,
  "message": "Xác nhận đơn hàng thành công",
  "data": {
    "id": 123,
    "status": "Confirmed",
    ...
  }
}
```

---

### 8. Mark as Paid

**Endpoint:** `POST /api/orders/{orderId}/pay`

**Authorization:** STAFF or ADMIN

**Conditions:** Order phải ở trạng thái **Pending** hoặc **Confirmed**

**Response:**
```json
{
  "success": true,
  "message": "Thanh toán thành công",
  "data": {
    "id": 123,
    "status": "Paid",
    "paidAt": "2025-01-28T15:30:00Z",
    ...
  }
}
```

---

### 9. Cancel Order

**Endpoint:** `POST /api/orders/{orderId}/cancel`

**Conditions:** Order chưa được **Paid**

**Request Body:**
```json
{
  "reason": "Khách hủy đơn"
}
```

**Business Logic:**
1. ✅ Check order can be cancelled (not Paid/Completed)
2. ✅ **Rollback voucher** (if applied)
   - Decrement usage count
   - Reset IsUsed flag (for private vouchers)
3. ✅ Set status to **Cancelled**
4. ✅ Save cancel reason & timestamp

**Response:**
```json
{
  "success": true,
  "message": "Hủy đơn hàng thành công",
  "data": {
    "status": "Cancelled",
    "cancelledAt": "2025-01-28T16:00:00Z",
    "cancelReason": "Khách hủy đơn"
  }
}
```

---

## 🏗️ Database Schema

### Orders Table

```sql
CREATE TABLE Orders (
    Id INT PRIMARY KEY IDENTITY,
    OrderCode NVARCHAR(50) UNIQUE NOT NULL,  -- ORD-yyyyMMdd-xxxxx
    UserId INT,
    Status INT NOT NULL,  -- Enum: Draft=0, Pending=1, Confirmed=2, Paid=3, Completed=4, Cancelled=5
    
    -- Address Snapshot (copied from UserAddress at checkout)
    RecipientName NVARCHAR(100),
    ShippingAddress NVARCHAR(500),
    PhoneNumber NVARCHAR(20),
    
    -- Pricing
    SubTotal DECIMAL(18,2) NOT NULL DEFAULT 0,
    DiscountAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    ShippingFee DECIMAL(18,2) NOT NULL DEFAULT 0,
    FinalAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    
    -- Voucher
    VoucherId INT,
    
    -- Notes & Timestamps
    Note NVARCHAR(500),
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NOT NULL,
    PaidAt DATETIME2,
    CancelledAt DATETIME2,
    CancelReason NVARCHAR(500),
    
    CONSTRAINT FK_Orders_Users FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT FK_Orders_Vouchers FOREIGN KEY (VoucherId) REFERENCES Vouchers(Id) ON DELETE SET NULL
);

-- Indexes
CREATE INDEX IX_Orders_UserId ON Orders(UserId);
CREATE INDEX IX_Orders_Status ON Orders(Status);
CREATE INDEX IX_Orders_OrderCode ON Orders(OrderCode);
CREATE INDEX IX_Orders_CreatedAt ON Orders(CreatedAt DESC);
```

### OrderItems Table

```sql
CREATE TABLE OrderItems (
    Id INT PRIMARY KEY IDENTITY,
    OrderId INT NOT NULL,
    
    -- Product Snapshot (frozen at order time)
    ProductId INT NOT NULL,
    ProductName NVARCHAR(200) NOT NULL,
    ProductImageUrl NVARCHAR(500),
    ProductBasePrice DECIMAL(18,2) NOT NULL,
    
    -- Order specific
    Quantity INT NOT NULL DEFAULT 1,
    UnitPrice DECIMAL(18,2) NOT NULL,  -- BasePrice + Options
    TotalPrice DECIMAL(18,2) NOT NULL,  -- UnitPrice * Quantity
    Note NVARCHAR(500),
    
    CONSTRAINT FK_OrderItems_Orders FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE CASCADE,
    CONSTRAINT FK_OrderItems_Products FOREIGN KEY (ProductId) REFERENCES Products(Id)
);

CREATE INDEX IX_OrderItems_OrderId ON OrderItems(OrderId);
```

### OrderItemOptions Table

```sql
CREATE TABLE OrderItemOptions (
    Id INT PRIMARY KEY IDENTITY,
    OrderItemId INT NOT NULL,
    
    -- Option Snapshot
    OptionItemId INT NOT NULL,
    OptionGroupName NVARCHAR(100) NOT NULL,
    OptionItemName NVARCHAR(100) NOT NULL,
    PriceAdjustment DECIMAL(18,2) NOT NULL,
    
    CONSTRAINT FK_OrderItemOptions_OrderItems FOREIGN KEY (OrderItemId) REFERENCES OrderItems(Id) ON DELETE CASCADE,
    CONSTRAINT FK_OrderItemOptions_OptionItems FOREIGN KEY (OptionItemId) REFERENCES OptionItems(Id)
);

CREATE INDEX IX_OrderItemOptions_OrderItemId ON OrderItemOptions(OrderItemId);
```

---

## 💡 Business Rules

### 1. Order Code Generation

```csharp
Format: ORD-{yyyyMMdd}-{sequence}
Example: ORD-20250128-00123

// Code
public async Task<string> GenerateOrderCodeAsync()
{
    var today = DateTime.UtcNow.ToString("yyyyMMdd");
    var prefix = $"ORD-{today}-";
    
    var lastOrder = await _context.Orders
        .Where(o => o.OrderCode.StartsWith(prefix))
        .OrderByDescending(o => o.OrderCode)
        .FirstOrDefaultAsync();
    
    int sequence = 1;
    if (lastOrder != null)
    {
        var lastSequence = lastOrder.OrderCode.Substring(prefix.Length);
        sequence = int.Parse(lastSequence) + 1;
    }
    
    return $"{prefix}{sequence:D5}";
}
```

### 2. Price Calculation

```csharp
// Item Level
UnitPrice = ProductBasePrice + SUM(SelectedOptions.PriceAdjustment)
TotalPrice = UnitPrice * Quantity

// Order Level
SubTotal = SUM(OrderItems.TotalPrice)
DiscountAmount = CalculateVoucherDiscount(SubTotal, Voucher)
ShippingFee = 20,000đ (fixed, hoặc tính theo khoảng cách)
FinalAmount = SubTotal - DiscountAmount + ShippingFee
```

### 3. Address Snapshotting

**Problem:** User có thể sửa/xóa địa chỉ sau khi đặt hàng.

**Solution:** Copy toàn bộ thông tin địa chỉ vào Order at checkout:

```csharp
// At checkout
var userAddress = await _addressService.GetByIdAsync(request.UserAddressId);
order.RecipientName = userAddress.RecipientName;
order.ShippingAddress = userAddress.FullAddress;
order.PhoneNumber = userAddress.PhoneNumber;
```

**Benefit:** Order history không bị ảnh hưởng khi user update/delete address.

### 4. Product & Option Snapshotting

**Problem:** Product price hoặc option có thể thay đổi sau khi đặt hàng.

**Solution:** Snapshot vào OrderItem:

```csharp
var orderItem = new OrderItem
{
    ProductId = product.Id,
    ProductName = product.Name,           // Snapshot
    ProductImageUrl = product.ImageUrl,   // Snapshot
    ProductBasePrice = product.BasePrice, // Snapshot
    UnitPrice = calculatedPrice,          // Snapshot
    TotalPrice = calculatedPrice * quantity
};

// Snapshot selected options
foreach (var optionId in request.SelectedOptionItemIds)
{
    var option = await _context.OptionItems.FindAsync(optionId);
    orderItem.OrderItemOptions.Add(new OrderItemOption
    {
        OptionItemId = option.Id,
        OptionGroupName = option.OptionGroup.Name,  // Snapshot
        OptionItemName = option.Name,                // Snapshot
        PriceAdjustment = option.PriceAdjustment    // Snapshot
    });
}
```

---

## 🔒 Authorization Rules

| Action | Rule |
|--------|------|
| Create Draft Order | Authenticated user |
| View Own Orders | `order.view.own` permission |
| View All Orders | `order.view.all` (STAFF/ADMIN) |
| Checkout | Order must belong to user |
| Confirm Order | `order.update.all` (STAFF/ADMIN) |
| Mark as Paid | `order.update.all` (STAFF/ADMIN) |
| Cancel Own Order | `order.cancel.own` (before Paid) |
| Cancel Any Order | `order.cancel.all` (ADMIN) |

---

## 🐛 Common Errors

### 1. Order Not Found
```json
{
  "success": false,
  "message": "Không tìm thấy đơn hàng",
  "status": 404
}
```

### 2. Invalid Status Transition
```json
{
  "success": false,
  "message": "Chỉ có thể checkout đơn hàng đang ở trạng thái Nháp",
  "status": 400
}
```

### 3. Empty Order
```json
{
  "success": false,
  "message": "Không thể checkout đơn hàng trống",
  "status": 400
}
```

### 4. Product Price Changed
```json
{
  "success": false,
  "message": "Lỗi validation: Giá sản phẩm 'Cà Phê Sữa Đá' đã thay đổi. Vui lòng làm mới giỏ hàng.",
  "status": 400
}
```

### 5. Voucher Validation Failed
```json
{
  "success": false,
  "message": "Voucher không hợp lệ: Voucher đã hết lượt sử dụng",
  "status": 400
}
```

---

## 📖 Related Documentation

- 📦 [Product Module](./PRODUCT_MODULE.md)
- 🎟️ [Voucher Module](./VOUCHER_MODULE.md)
- 👤 [User Module](./USER_MODULE.md) (User Addresses)
