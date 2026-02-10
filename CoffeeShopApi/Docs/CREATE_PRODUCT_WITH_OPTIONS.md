# Hướng dẫn Tạo Sản Phẩm với Options

## Tổng quan

Khi tạo sản phẩm, bạn chỉ cần:
1. **Truyền ID của OptionGroup** - sản phẩm sẽ áp dụng nhóm option nào
2. **Tùy chọn filter OptionItems** - nếu muốn chỉ hiển thị một số items (VD: Size S, M nhưng không có Size L)

---

## Bước 1: Tạo OptionGroups (Admin - làm 1 lần)

Trước tiên, admin tạo các OptionGroups template:

```bash
# Tạo nhóm "Kích cỡ"
POST /api/OptionGroups
{
  "name": "Kích cỡ",
  "description": "Chọn kích cỡ đồ uống",
  "isRequired": true,
  "allowMultiple": false,
  "displayOrder": 1,
  "optionItems": [
    { "name": "Size S", "priceAdjustment": 0, "isDefault": true, "displayOrder": 1 },
    { "name": "Size M", "priceAdjustment": 5000, "displayOrder": 2 },
    { "name": "Size L", "priceAdjustment": 10000, "displayOrder": 3 }
  ]
}
# => Response: { "id": 1, ... }

# Tạo nhóm "Mức đường"
POST /api/OptionGroups
{
  "name": "Mức đường",
  "description": "Chọn độ ngọt",
  "isRequired": true,
  "allowMultiple": false,
  "displayOrder": 2,
  "optionItems": [
    { "name": "Không đường", "priceAdjustment": 0, "displayOrder": 1 },
    { "name": "Ít đường", "priceAdjustment": 0, "isDefault": true, "displayOrder": 2 },
    { "name": "Đường bình thường", "priceAdjustment": 0, "displayOrder": 3 }
  ]
}
# => Response: { "id": 2, ... }
```

**Kết quả sau bước 1:**
- OptionGroup ID 1: "Kích cỡ" (có 3 items: Size S, M, L)
- OptionGroup ID 2: "Mức đường" (có 3 items: Không đường, Ít đường, Đường bình thường)

---

## Bước 2: Tạo Product với Options

### Cách 1: Lấy tất cả OptionItems trong Group (đơn giản)

```json
POST /api/Products
{
  "name": "Cà phê sữa",
  "description": "Cà phê phin truyền thống pha với sữa đặc",
  "basePrice": 25000,
  "categoryId": 1,
  "imageUrl": "/images/cafe-sua.jpg",
  "optionGroups": [
    {
      "optionGroupId": 1,
      "displayOrder": 1
    },
    {
      "optionGroupId": 2,
      "displayOrder": 2
    }
  ]
}
```

**Kết quả:**
- Product sẽ có OptionGroup "Kích cỡ" với đầy đủ 3 sizes (S, M, L)
- Product sẽ có OptionGroup "Mức đường" với đầy đủ 3 mức

### Cách 2: Chỉ lấy một số OptionItems (có filter)

Ví dụ: Product này chỉ có Size S và M, không có Size L

```json
POST /api/Products
{
  "name": "Cà phê đá",
  "description": "Cà phê phin truyền thống pha đá",
  "basePrice": 20000,
  "categoryId": 1,
  "imageUrl": "/images/cafe-da.jpg",
  "optionGroups": [
    {
      "optionGroupId": 1,
      "displayOrder": 1,
      "allowedItemIds": [1, 2]  // ⭐ Chỉ lấy Size S (ID=1) và Size M (ID=2)
    },
    {
      "optionGroupId": 2,
      "displayOrder": 2
      // Không có allowedItemIds => lấy tất cả
    }
  ]
}
```

**Kết quả:**
- Product chỉ hiển thị Size S và M, không có Size L
- Mức đường vẫn đầy đủ 3 mức

---

## Bước 3: Lấy thông tin Product (Customer)

```bash
GET /api/Products/{id}
```

**Response:**
```json
{
  "id": 10,
  "name": "Cà phê đá",
  "basePrice": 20000,
  "optionGroups": [
    {
      "id": 1,
      "name": "Kích cỡ",
      "isRequired": true,
      "allowMultiple": false,
      "displayOrder": 1,
      "optionItems": [
        { "id": 1, "name": "Size S", "priceAdjustment": 0, "isDefault": true },
        { "id": 2, "name": "Size M", "priceAdjustment": 5000 }
        // ⭐ Không có Size L vì đã filter
      ]
    },
    {
      "id": 2,
      "name": "Mức đường",
      "isRequired": true,
      "allowMultiple": false,
      "displayOrder": 2,
      "optionItems": [
        { "id": 4, "name": "Không đường", "priceAdjustment": 0 },
        { "id": 5, "name": "Ít đường", "priceAdjustment": 0, "isDefault": true },
        { "id": 6, "name": "Đường bình thường", "priceAdjustment": 0 }
      ]
    }
  ]
}
```

---

## Bước 4: Cập nhật Product Options

### Thêm OptionGroup mới

```json
PUT /api/Products/10
{
  "name": "Cà phê đá",
  "basePrice": 20000,
  "categoryId": 1,
  "optionGroups": [
    {
      "optionGroupId": 1,
      "displayOrder": 1,
      "allowedItemIds": [1, 2]
    },
    {
      "optionGroupId": 2,
      "displayOrder": 2
    },
    {
      "optionGroupId": 3,  // ⭐ Thêm Topping
      "displayOrder": 3
    }
  ]
}
```

### Xóa OptionGroup

```json
PUT /api/Products/10
{
  "name": "Cà phê đá",
  "basePrice": 20000,
  "categoryId": 1,
  "optionGroups": [
    {
      "optionGroupId": 1,
      "displayOrder": 1
    }
    // ⭐ Không có OptionGroup 2 => sẽ bị xóa
  ]
}
```

### Cập nhật filter items

```json
PUT /api/Products/10
{
  "name": "Cà phê đá",
  "basePrice": 20000,
  "categoryId": 1,
  "optionGroups": [
    {
      "optionGroupId": 1,
      "displayOrder": 1,
      "allowedItemIds": [1, 2, 3]  // ⭐ Thêm Size L vào
    }
  ]
}
```

---

## Use Cases

### Use Case 1: Sản phẩm có đầy đủ sizes

```json
{
  "name": "Bạc xỉu",
  "basePrice": 25000,
  "categoryId": 1,
  "optionGroups": [
    { "optionGroupId": 1 }  // Lấy tất cả sizes
  ]
}
```

### Use Case 2: Sản phẩm chỉ có size nhỏ, trung bình

```json
{
  "name": "Espresso",
  "basePrice": 30000,
  "categoryId": 1,
  "optionGroups": [
    {
      "optionGroupId": 1,
      "allowedItemIds": [1, 2]  // Chỉ S và M
    }
  ]
}
```

### Use Case 3: Sản phẩm có nhiều nhóm options

```json
{
  "name": "Trà sữa trân châu",
  "basePrice": 35000,
  "categoryId": 2,
  "optionGroups": [
    {
      "optionGroupId": 1,  // Kích cỡ (tất cả)
      "displayOrder": 1
    },
    {
      "optionGroupId": 2,  // Mức đường (tất cả)
      "displayOrder": 2
    },
    {
      "optionGroupId": 3,  // Topping
      "displayOrder": 3,
      "allowedItemIds": [7, 9]  // Chỉ Trân châu và Pudding
    }
  ]
}
```

---

## Validation Rules

### OptionGroupId phải tồn tại
```json
{
  "optionGroupId": 999  // ❌ Error: OptionGroup with ID 999 not found
}
```

### AllowedItemIds phải thuộc OptionGroup đó
```json
{
  "optionGroupId": 1,  // OptionGroup "Kích cỡ" có items [1, 2, 3]
  "allowedItemIds": [1, 5]  // ❌ Error: OptionGroup 1 does not contain item with ID 5
}
```

---

## Database Structure

### ProductOptionGroup Table

| Column | Type | Description |
|--------|------|-------------|
| Id | int | Primary key |
| ProductId | int | FK to Products |
| OptionGroupId | int | FK to OptionGroups |
| DisplayOrder | int | Thứ tự hiển thị |
| AllowedItemIdsJson | nvarchar(max) | JSON array, VD: "[1,2]" |

**Ví dụ dữ liệu:**
```
Id | ProductId | OptionGroupId | DisplayOrder | AllowedItemIdsJson
---|-----------|---------------|--------------|-------------------
1  | 10        | 1             | 1            | [1,2]
2  | 10        | 2             | 2            | NULL
3  | 11        | 1             | 1            | NULL
```

---

## Migration Required

Chạy migration để thêm field `AllowedItemIdsJson`:

```bash
dotnet ef migrations add AddAllowedItemIdsToProductOptionGroup
dotnet ef database update
```

---

## Testing Examples

### Tạo OptionGroups trước
```bash
curl -X POST http://localhost:1912/api/OptionGroups \
  -H "Content-Type: application/json" \
  -d '{"name":"Kích cỡ","isRequired":true,"optionItems":[{"name":"Size S","priceAdjustment":0},{"name":"Size M","priceAdjustment":5000},{"name":"Size L","priceAdjustment":10000}]}'
```

### Tạo Product với filter
```bash
curl -X POST http://localhost:1912/api/Products \
  -H "Content-Type: application/json" \
  -d '{"name":"Cà phê đá","basePrice":20000,"categoryId":1,"optionGroups":[{"optionGroupId":1,"allowedItemIds":[1,2]}]}'
```

### Lấy Product
```bash
curl -X GET http://localhost:1912/api/Products/10
```

---

## Summary

✅ **Tạo Product đơn giản**: Chỉ cần truyền `optionGroupId`  
✅ **Filter items**: Thêm `allowedItemIds` để chọn items cụ thể  
✅ **Update linh hoạt**: Thay đổi options bất kỳ lúc nào  
✅ **Validation đầy đủ**: Kiểm tra IDs hợp lệ  
✅ **Response filter**: Chỉ trả về items được phép
