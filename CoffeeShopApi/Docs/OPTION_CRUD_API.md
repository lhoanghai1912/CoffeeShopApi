# Option CRUD API Documentation

## Tổng quan

API quản lý Option Groups và Option Items cho hệ thống Coffee Shop.

### Models

- **OptionGroup**: Template nhóm tùy chọn (Kích cỡ, Mức đường, Topping)
- **OptionItem**: Các lựa chọn cụ thể trong nhóm (Size S, Size M, Size L)

---

## OptionGroups API

### 1. Get All Option Groups

**Endpoint:** `GET /api/OptionGroups`

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "name": "Kích cỡ",
      "description": "Chọn kích cỡ đồ uống",
      "isRequired": true,
      "allowMultiple": false,
      "displayOrder": 1,
      "dependsOnOptionItemId": null,
      "optionItems": [
        {
          "id": 1,
          "name": "Size S",
          "priceAdjustment": 0,
          "isDefault": true,
          "displayOrder": 1
        },
        {
          "id": 2,
          "name": "Size M",
          "priceAdjustment": 5000,
          "isDefault": false,
          "displayOrder": 2
        },
        {
          "id": 3,
          "name": "Size L",
          "priceAdjustment": 10000,
          "isDefault": false,
          "displayOrder": 3
        }
      ]
    }
  ]
}
```

### 2. Get Option Group by ID

**Endpoint:** `GET /api/OptionGroups/{id}`

**Response:**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "name": "Kích cỡ",
    "description": "Chọn kích cỡ đồ uống",
    "isRequired": true,
    "allowMultiple": false,
    "displayOrder": 1,
    "dependsOnOptionItemId": null,
    "optionItems": [...]
  }
}
```

### 3. Create Option Group

**Endpoint:** `POST /api/OptionGroups`

**Authorization:** Required - `product.create` permission

**Request Body:**
```json
{
  "name": "Kích cỡ",
  "description": "Chọn kích cỡ đồ uống",
  "isRequired": true,
  "allowMultiple": false,
  "displayOrder": 1,
  "dependsOnOptionItemId": null,
  "optionItems": [
    {
      "name": "Size S",
      "priceAdjustment": 0,
      "isDefault": true,
      "displayOrder": 1
    },
    {
      "name": "Size M",
      "priceAdjustment": 5000,
      "isDefault": false,
      "displayOrder": 2
    },
    {
      "name": "Size L",
      "priceAdjustment": 10000,
      "isDefault": false,
      "displayOrder": 3
    }
  ]
}
```

**Response:**
```json
{
  "success": true,
  "message": "Option group created successfully",
  "data": {
    "id": 1,
    "name": "Kích cỡ",
    "description": "Chọn kích cỡ đồ uống",
    "isRequired": true,
    "allowMultiple": false,
    "displayOrder": 1,
    "dependsOnOptionItemId": null,
    "optionItems": [...]
  }
}
```

### 4. Update Option Group

**Endpoint:** `PUT /api/OptionGroups/{id}`

**Authorization:** Required - `product.update` permission

**Request Body:**
```json
{
  "name": "Kích cỡ (Updated)",
  "description": "Chọn kích cỡ đồ uống của bạn",
  "isRequired": true,
  "allowMultiple": false,
  "displayOrder": 1,
  "dependsOnOptionItemId": null,
  "optionItems": []
}
```

**Note:** Để update option items, sử dụng OptionItems endpoints

**Response:**
```json
{
  "success": true,
  "message": "Option group updated successfully",
  "data": true
}
```

### 5. Delete Option Group

**Endpoint:** `DELETE /api/OptionGroups/{id}`

**Authorization:** Required - `product.delete` permission

**Warning:** Sẽ xóa tất cả option items trong group này

**Response:**
```json
{
  "success": true,
  "message": "Option group deleted successfully",
  "data": true
}
```

---

## OptionItems API

### 1. Get All Option Items

**Endpoint:** `GET /api/OptionItems`

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "name": "Size S",
      "priceAdjustment": 0,
      "isDefault": true,
      "displayOrder": 1
    },
    {
      "id": 2,
      "name": "Size M",
      "priceAdjustment": 5000,
      "isDefault": false,
      "displayOrder": 2
    }
  ]
}
```

### 2. Get Option Items by Option Group ID

**Endpoint:** `GET /api/OptionItems/group/{optionGroupId}`

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "name": "Size S",
      "priceAdjustment": 0,
      "isDefault": true,
      "displayOrder": 1
    },
    {
      "id": 2,
      "name": "Size M",
      "priceAdjustment": 5000,
      "isDefault": false,
      "displayOrder": 2
    }
  ]
}
```

### 3. Get Option Item by ID

**Endpoint:** `GET /api/OptionItems/{id}`

**Response:**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "name": "Size S",
    "priceAdjustment": 0,
    "isDefault": true,
    "displayOrder": 1
  }
}
```

### 4. Create Option Item

**Endpoint:** `POST /api/OptionItems/group/{optionGroupId}`

**Authorization:** Required - `product.create` permission

**Request Body:**
```json
{
  "name": "Size XL",
  "priceAdjustment": 15000,
  "isDefault": false,
  "displayOrder": 4
}
```

**Response:**
```json
{
  "success": true,
  "message": "Option item created successfully",
  "data": {
    "id": 4,
    "name": "Size XL",
    "priceAdjustment": 15000,
    "isDefault": false,
    "displayOrder": 4
  }
}
```

### 5. Update Option Item

**Endpoint:** `PUT /api/OptionItems/{id}`

**Authorization:** Required - `product.update` permission

**Request Body:**
```json
{
  "name": "Size XL (Updated)",
  "priceAdjustment": 20000,
  "isDefault": false,
  "displayOrder": 4
}
```

**Response:**
```json
{
  "success": true,
  "message": "Option item updated successfully",
  "data": true
}
```

### 6. Delete Option Item

**Endpoint:** `DELETE /api/OptionItems/{id}`

**Authorization:** Required - `product.delete` permission

**Response:**
```json
{
  "success": true,
  "message": "Option item deleted successfully",
  "data": true
}
```

---

## Use Cases

### Use Case 1: Tạo nhóm "Kích cỡ" cho đồ uống

```bash
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
```

### Use Case 2: Tạo nhóm "Mức đường"

```bash
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
    { "name": "Đường bình thường", "priceAdjustment": 0, "displayOrder": 3 },
    { "name": "Nhiều đường", "priceAdjustment": 0, "displayOrder": 4 }
  ]
}
```

### Use Case 3: Tạo nhóm "Topping" (cho phép chọn nhiều)

```bash
POST /api/OptionGroups
{
  "name": "Topping",
  "description": "Thêm topping",
  "isRequired": false,
  "allowMultiple": true,
  "displayOrder": 3,
  "optionItems": [
    { "name": "Trân châu", "priceAdjustment": 5000, "displayOrder": 1 },
    { "name": "Thạch", "priceAdjustment": 5000, "displayOrder": 2 },
    { "name": "Pudding", "priceAdjustment": 8000, "displayOrder": 3 },
    { "name": "Kem cheese", "priceAdjustment": 10000, "displayOrder": 4 }
  ]
}
```

### Use Case 4: Thêm option item mới vào nhóm đã có

```bash
POST /api/OptionItems/group/1
{
  "name": "Size XL",
  "priceAdjustment": 15000,
  "isDefault": false,
  "displayOrder": 4
}
```

### Use Case 5: Cập nhật giá của option item

```bash
PUT /api/OptionItems/4
{
  "name": "Size XL",
  "priceAdjustment": 20000,
  "isDefault": false,
  "displayOrder": 4
}
```

---

## Validation Rules

### OptionGroup
- `name`: Required, max 100 characters
- `description`: Optional, max 500 characters
- `optionItems`: Must have at least 1 item when creating

### OptionItem
- `name`: Required, max 100 characters
- `priceAdjustment`: Decimal, can be positive (tăng giá) or negative (giảm giá)
- `displayOrder`: Integer, used for ordering items

---

## Error Responses

### 400 Bad Request
```json
{
  "success": false,
  "message": "Option group name is required"
}
```

### 404 Not Found
```json
{
  "success": false,
  "message": "OptionGroup with Id 999 not found"
}
```

### 500 Internal Server Error
```json
{
  "success": false,
  "message": "An error occurred while creating option group"
}
```

---

## Testing with Postman/curl

### Create Option Group
```bash
curl -X POST http://localhost:1912/api/OptionGroups \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{
    "name": "Kích cỡ",
    "description": "Chọn kích cỡ đồ uống",
    "isRequired": true,
    "allowMultiple": false,
    "displayOrder": 1,
    "optionItems": [
      {"name": "Size S", "priceAdjustment": 0, "isDefault": true, "displayOrder": 1},
      {"name": "Size M", "priceAdjustment": 5000, "displayOrder": 2},
      {"name": "Size L", "priceAdjustment": 10000, "displayOrder": 3}
    ]
  }'
```

### Get All Option Groups
```bash
curl -X GET http://localhost:1912/api/OptionGroups \
  -H "Authorization: Bearer <token>"
```

### Create Option Item
```bash
curl -X POST http://localhost:1912/api/OptionItems/group/1 \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{
    "name": "Size XL",
    "priceAdjustment": 15000,
    "isDefault": false,
    "displayOrder": 4
  }'
```
