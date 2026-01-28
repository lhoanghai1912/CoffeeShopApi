# API Reference - Quick Guide

## 🚀 Base URL

```
Development: https://localhost:5001
Production:  https://api.coffeeshop.com
```

---

## 📖 Table of Contents

- [Authentication](#authentication)
- [Users](#users)
- [Products](#products)
- [Categories](#categories)
- [Orders](#orders)
- [Vouchers](#vouchers)
- [Response Format](#response-format)
- [Error Codes](#error-codes)

---

## 🔐 Authentication

All protected endpoints require JWT token in Authorization header:

```
Authorization: Bearer {token}
```

### POST /api/auth/login

**Request:**
```json
{
  "username": "customer01",
  "password": "Customer@123"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "token": "eyJhbGc...",
    "user": { ... }
  }
}
```

### POST /api/auth/register

**Request:**
```json
{
  "username": "newuser",
  "password": "Pass@123",
  "confirmPassword": "Pass@123",
  "email": "user@example.com",
  "fullName": "Nguyen Van A",
  "phoneNumber": "0912345678"
}
```

### POST /api/auth/forgot-password

**Request:**
```json
{
  "emailOrUsername": "customer01"
}
```

### POST /api/auth/reset-password

**Request:**
```json
{
  "emailOrUsername": "customer01",
  "resetToken": "123456",
  "newPassword": "NewPass@123",
  "confirmNewPassword": "NewPass@123"
}
```

---

## 👤 Users

### GET /api/users/profile 🔒

**Headers:** `Authorization: Bearer {token}`

**Response:**
```json
{
  "success": true,
  "data": {
    "id": 3,
    "username": "customer01",
    "fullName": "Nguyen Van A",
    "email": "customer01@example.com"
  }
}
```

### PUT /api/users/profile 🔒

**Request:**
```json
{
  "fullName": "Updated Name",
  "email": "newemail@example.com",
  "phoneNumber": "0987654321"
}
```

### POST /api/users/change-password 🔒

**Request:**
```json
{
  "oldPassword": "OldPass@123",
  "newPassword": "NewPass@456",
  "confirmNewPassword": "NewPass@456"
}
```

### GET /api/users/addresses 🔒

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "recipientName": "Nguyen Van A",
      "phoneNumber": "0912345678",
      "addressLine": "123 Nguyen Hue, Q1, HCM",
      "label": "Home",
      "isDefault": true
    }
  ]
}
```

### POST /api/users/addresses 🔒

**Request:**
```json
{
  "recipientName": "Nguyen Van A",
  "phoneNumber": "0912345678",
  "addressLine": "123 Nguyen Hue, Q1, HCM",
  "label": "Home",
  "isDefault": false
}
```

### POST /api/users/addresses/{id}/set-default 🔒

---

## 📦 Products

### GET /api/products/paged

**Query Parameters:**
- `page` (default: 1)
- `pageSize` (default: 10)
- `search` (optional)
- `orderBy` (e.g., "BasePrice desc")
- `filter` (e.g., "CategoryId=1")

**Example:**
```
GET /api/products/paged?page=1&pageSize=20&search=coffee&filter=CategoryId=1
```

**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 1,
        "name": "Cà Phê Đen Đá",
        "description": "Cà phê Robusta đậm đà",
        "basePrice": 25000,
        "imageUrl": "/images/caphedenda.jpg",
        "categoryId": 1,
        "categoryName": "Coffee",
        "optionGroups": [
          {
            "id": 1,
            "name": "Kích cỡ",
            "isRequired": true,
            "allowMultiple": false,
            "optionItems": [
              {
                "id": 1,
                "name": "Nhỏ (S)",
                "priceAdjustment": 0,
                "isDefault": true
              }
            ]
          }
        ]
      }
    ],
    "totalCount": 30,
    "page": 1,
    "pageSize": 20,
    "totalPages": 2
  }
}
```

### GET /api/products/{id}

**Response:** Single product with full details

### POST /api/products 🔒👑

**Authorization:** ADMIN/STAFF

**Content-Type:** `multipart/form-data`

**Form Fields:**
- `FormField`: JSON string (CreateProductRequest)
- `Image`: File (optional)

**FormField (JSON):**
```json
{
  "name": "Cà Phê Cốt Dừa",
  "description": "Cà phê kết hợp cốt dừa béo ngậy",
  "basePrice": 38000,
  "categoryId": 1,
  "optionGroups": [
    {
      "name": "Kích cỡ",
      "isRequired": true,
      "allowMultiple": false,
      "optionItems": [
        { "name": "Nhỏ (S)", "priceAdjustment": 0, "isDefault": true },
        { "name": "Vừa (M)", "priceAdjustment": 5000 },
        { "name": "Lớn (L)", "priceAdjustment": 10000 }
      ]
    }
  ]
}
```

### PUT /api/products/{id} 🔒👑

**Authorization:** ADMIN/STAFF

Similar to POST

### DELETE /api/products/{id} 🔒👑

**Authorization:** ADMIN

---

## 📂 Categories

### GET /api/categories

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "name": "Coffee",
      "productCount": 10
    }
  ]
}
```

### GET /api/categories/{id}

### POST /api/categories 🔒👑

**Request:**
```json
{
  "name": "Smoothie"
}
```

### PUT /api/categories/{id} 🔒👑

**Request:**
```json
{
  "name": "Smoothie & Juice"
}
```

### DELETE /api/categories/{id} 🔒👑

---

## 📋 Orders

### GET /api/orders 🔒

**Query Parameters:**
- `page`, `pageSize`
- `status` (0-5)
- `search` (orderCode)

**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 123,
        "orderCode": "ORD-20250128-00123",
        "status": "Pending",
        "subTotal": 89000,
        "finalAmount": 99000,
        "createdAt": "2025-01-28T10:00:00Z"
      }
    ],
    "totalCount": 50,
    "page": 1,
    "pageSize": 10
  }
}
```

### GET /api/orders/{id} 🔒

**Response:** Full order details with items

### POST /api/orders 🔒

**Request:**
```json
{
  "userId": 1,
  "note": "Ghi chú đơn hàng",
  "items": [
    {
      "productId": 1,
      "quantity": 2,
      "note": "Ít đường",
      "selectedOptionItemIds": [1, 5, 10]
    }
  ]
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "id": 123,
    "orderCode": "ORD-20250128-00123",
    "status": "Draft",
    "subTotal": 50000,
    "orderItems": [...]
  }
}
```

### POST /api/orders/{id}/items 🔒

**Request:**
```json
{
  "productId": 3,
  "quantity": 1,
  "note": "Nhiều đá",
  "selectedOptionItemIds": [2, 7, 11]
}
```

### PUT /api/orders/{orderId}/items/{itemId} 🔒

**Request:**
```json
{
  "quantity": 3,
  "note": "Ít đường",
  "selectedOptionItemIds": [1, 5, 10]
}
```

### DELETE /api/orders/{orderId}/items/{itemId} 🔒

### POST /api/orders/{id}/checkout 🔒

**Request:**
```json
{
  "userAddressId": 5,
  "voucherId": 3,
  "note": "Giao trước 5pm"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "id": 123,
    "status": "Pending",
    "subTotal": 89000,
    "discountAmount": 10000,
    "shippingFee": 20000,
    "finalAmount": 99000,
    "recipientName": "Nguyen Van A",
    "shippingAddress": "123 Nguyen Hue, Q1, HCM"
  }
}
```

### POST /api/orders/{id}/confirm 🔒👑

**Authorization:** STAFF/ADMIN

### POST /api/orders/{id}/pay 🔒👑

**Authorization:** STAFF/ADMIN

### POST /api/orders/{id}/cancel 🔒

**Request:**
```json
{
  "reason": "Khách hủy đơn"
}
```

---

## 🎟️ Vouchers

### POST /api/vouchers/validate 🔒

**Request:**
```json
{
  "voucherCode": "WELCOME10K",
  "orderSubTotal": 100000
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "isValid": true,
    "voucherId": 1,
    "discountAmount": 10000,
    "errorMessage": null
  }
}
```

### GET /api/vouchers/active

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "code": "WELCOME10K",
      "description": "Giảm 10,000đ cho đơn đầu",
      "discountType": "FixedAmount",
      "discountValue": 10000,
      "minOrderValue": 50000,
      "startDate": "2025-01-01T00:00:00Z",
      "endDate": "2025-12-31T23:59:59Z",
      "remainingUses": 950
    }
  ]
}
```

### GET /api/vouchers/my-vouchers 🔒

**Query:** `?isUsed=false`

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "voucherId": 8,
      "voucher": {
        "code": "BIRTHDAY30K",
        "description": "Quà sinh nhật",
        "discountValue": 30000
      },
      "isUsed": false,
      "assignedAt": "2025-01-20T10:00:00Z"
    }
  ]
}
```

### GET /api/vouchers/code/{code}

### GET /api/vouchers 🔒👑

**Authorization:** ADMIN/STAFF

**Description:** Get vouchers with pagination

**Query Parameters:**
- `page` (default: 1)
- `pageSize` (default: 10)
- `isActive` (optional)
- `search` (optional)
- `isPublic` (optional)

**Example:**
```bash
GET /api/vouchers?page=1&pageSize=20&isActive=true&search=BIRTHDAY
```

**Response:**
```json
{
  "success": true,
  "data": {
    "pageNumber": 1,
    "pageSize": 20,
    "totalPages": 2,
    "totalCount": 35,
    "items": [...]
  }
}
```

### GET /api/vouchers/all 🔒👑

**Authorization:** ADMIN/STAFF

**Description:** Get all vouchers without pagination (for dropdown/select)

**Query Parameters:**
- `isActive` (optional)

**Example:**
```bash
GET /api/vouchers/all?isActive=true
```

**Response:**
```json
{
  "success": true,
  "data": [...]
}
```

### POST /api/vouchers 🔒👑

**Request:**
```json
{
  "code": "FLASH30K",
  "description": "Flash Sale",
  "discountType": "FixedAmount",
  "discountValue": 30000,
  "minOrderValue": 100000,
  "maxDiscountAmount": null,
  "startDate": "2025-02-01T00:00:00Z",
  "endDate": "2025-02-28T23:59:59Z",
  "usageLimit": 500,
  "usageLimitPerUser": 3,
  "isPublic": true
}
```

### PUT /api/vouchers/{id} 🔒👑

### DELETE /api/vouchers/{id} 🔒👑

### POST /api/vouchers/assign 🔒👑

**Request:**
```json
{
  "voucherId": 8,
  "userIds": [1, 2, 3, 5, 10],
  "note": "Quà sinh nhật tháng 2"
}
```

---

## 📊 Response Format

### Success Response

```json
{
  "success": true,
  "message": "Thành công",
  "status": 200,
  "data": { ... },
  "errors": null
}
```

### Error Response

```json
{
  "success": false,
  "message": "Validation Error",
  "status": 400,
  "data": null,
  "errors": [
    "Tên sản phẩm không được để trống",
    "Giá sản phẩm phải lớn hơn 0"
  ]
}
```

### Paginated Response

```json
{
  "success": true,
  "data": {
    "items": [...],
    "totalCount": 100,
    "page": 1,
    "pageSize": 10,
    "totalPages": 10
  }
}
```

---

## ❌ Error Codes

| Status Code | Meaning | Example |
|-------------|---------|---------|
| 200 | Success | Request completed successfully |
| 201 | Created | Resource created successfully |
| 400 | Bad Request | Validation error, invalid input |
| 401 | Unauthorized | Missing or invalid token |
| 403 | Forbidden | No permission to access resource |
| 404 | Not Found | Resource does not exist |
| 409 | Conflict | Duplicate username/email |
| 500 | Internal Server Error | Server error, check logs |

---

## 🔑 Authorization Levels

| Symbol | Meaning |
|--------|---------|
| 🔒 | Requires authentication (any user) |
| 👑 | Requires admin/staff role |
| (none) | Public access |

---

## 💡 Quick Examples

### Example 1: Login and Get Profile

```bash
# 1. Login
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"customer01","password":"Customer@123"}'

# Response: { "data": { "token": "eyJhbGc..." } }

# 2. Get Profile
curl -X GET https://localhost:5001/api/users/profile \
  -H "Authorization: Bearer eyJhbGc..."
```

### Example 2: Create and Checkout Order

```bash
# 1. Create draft order
curl -X POST https://localhost:5001/api/orders \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "userId": 1,
    "items": [
      {
        "productId": 1,
        "quantity": 2,
        "selectedOptionItemIds": [1, 5, 10]
      }
    ]
  }'

# Response: { "data": { "id": 123, "status": "Draft" } }

# 2. Checkout
curl -X POST https://localhost:5001/api/orders/123/checkout \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "userAddressId": 5,
    "voucherId": 3
  }'
```

### Example 3: Filter Products by Category

```bash
# Get all Coffee products
curl -X GET "https://localhost:5001/api/products/paged?filter=CategoryId=1&page=1&pageSize=20"

# Search products
curl -X GET "https://localhost:5001/api/products/paged?search=trà&orderBy=BasePrice%20desc"
```

---

## 📖 Full Documentation

For detailed documentation, see:
- [Authentication Module](./AUTH_MODULE.md)
- [User Module](./USER_MODULE.md)
- [Product Module](./PRODUCT_MODULE.md)
- [Order Module](./ORDER_MODULE.md)
- [Voucher Module](./VOUCHER_MODULE.md)

---

**Last Updated:** January 28, 2025  
**API Version:** 1.0.0
