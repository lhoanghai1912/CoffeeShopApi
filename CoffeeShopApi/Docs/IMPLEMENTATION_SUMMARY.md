# Summary: Product Options Implementation

## ✅ Đã hoàn thành

### 1. **Models Updated**
- ✅ `ProductOptionGroup.cs` - Thêm `AllowedItemIdsJson` và helper property `AllowedItemIds`

### 2. **DTOs Updated**
- ✅ `ProductDto.cs` - Thêm `ProductOptionGroupRequest` với:
  - `OptionGroupId` - ID của OptionGroup template
  - `DisplayOrder` - Thứ tự hiển thị
  - `AllowedItemIds` - Filter items (optional)

### 3. **Services Updated**
- ✅ `ProductService.cs`:
  - `CreateAsync()` - Tạo Product với OptionGroups, validate IDs
  - `UpdateAsync()` - Cập nhật OptionGroups, xóa cũ thêm mới
  - `MapToResponse()` - Filter OptionItems theo AllowedItemIds

- ✅ `ProductRequestService.cs` - Update validation logic

### 4. **Repositories Created**
- ✅ `IOptionGroupRepo.cs` + `OptionGroupRepo.cs`
- ✅ `IOptionItemRepo.cs` + `OptionItemRepo.cs`

### 5. **Services Created**
- ✅ `OptionGroupService.cs` - CRUD cho OptionGroups
- ✅ `OptionItemService.cs` - CRUD cho OptionItems

### 6. **Controllers Created**
- ✅ `OptionGroupsController.cs` - 5 endpoints
- ✅ `OptionItemsController.cs` - 6 endpoints

### 7. **Documentation**
- ✅ `PRODUCT_OPTIONS_LOGIC.md` - Giải thích kiến trúc và workflows
- ✅ `CREATE_PRODUCT_WITH_OPTIONS.md` - Hướng dẫn sử dụng API
- ✅ `OPTION_CRUD_API.md` - API reference cho OptionGroups/Items
- ✅ `FILE_UPLOAD_SERVICE.md` - Hướng dẫn upload file
- ✅ `AddAllowedItemIdsJson.md` - Migration script

### 8. **Program.cs**
- ✅ Đăng ký Repositories: `IOptionGroupRepository`, `IOptionItemRepository`
- ✅ Đăng ký Services: `IOptionGroupService`, `IOptionItemService`

---

## 🔧 Cần thực hiện

### 1. Chạy Migration

```bash
dotnet ef migrations add AddAllowedItemIdsToProductOptionGroup --project CoffeeShopApi
dotnet ef database update --project CoffeeShopApi
```

Hoặc chạy SQL trực tiếp:

```sql
ALTER TABLE ProductOptionGroups
ADD AllowedItemIdsJson nvarchar(max) NULL;
```

### 2. Test API

#### Bước 1: Tạo OptionGroup templates

```bash
curl -X POST http://localhost:1912/api/OptionGroups \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{
    "name": "Kích cỡ",
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

#### Bước 2: Tạo Product với OptionGroups

**Lấy tất cả items:**
```bash
curl -X POST http://localhost:1912/api/Products \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{
    "name": "Cà phê sữa",
    "basePrice": 25000,
    "categoryId": 1,
    "optionGroups": [
      {"optionGroupId": 1, "displayOrder": 1}
    ]
  }'
```

**Chỉ lấy Size S và M:**
```bash
curl -X POST http://localhost:1912/api/Products \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{
    "name": "Espresso",
    "basePrice": 30000,
    "categoryId": 1,
    "optionGroups": [
      {"optionGroupId": 1, "displayOrder": 1, "allowedItemIds": [1, 2]}
    ]
  }'
```

#### Bước 3: Lấy Product

```bash
curl -X GET http://localhost:1912/api/Products/1 \
  -H "Authorization: Bearer <token>"
```

Expected response:
```json
{
  "success": true,
  "data": {
    "id": 1,
    "name": "Espresso",
    "basePrice": 30000,
    "optionGroups": [
      {
        "id": 1,
        "name": "Kích cỡ",
        "optionItems": [
          {"id": 1, "name": "Size S", "priceAdjustment": 0},
          {"id": 2, "name": "Size M", "priceAdjustment": 5000}
          // Không có Size L vì đã filter
        ]
      }
    ]
  }
}
```

---

## 📚 API Endpoints

### OptionGroups
- `GET /api/OptionGroups` - Lấy tất cả
- `GET /api/OptionGroups/{id}` - Lấy theo ID
- `POST /api/OptionGroups` - Tạo mới (với items)
- `PUT /api/OptionGroups/{id}` - Cập nhật
- `DELETE /api/OptionGroups/{id}` - Xóa

### OptionItems
- `GET /api/OptionItems` - Lấy tất cả
- `GET /api/OptionItems/group/{optionGroupId}` - Lấy theo group
- `GET /api/OptionItems/{id}` - Lấy theo ID
- `POST /api/OptionItems/group/{optionGroupId}` - Tạo mới
- `PUT /api/OptionItems/{id}` - Cập nhật
- `DELETE /api/OptionItems/{id}` - Xóa

### Products (updated)
- `POST /api/Products` - Tạo với OptionGroups
- `PUT /api/Products/{id}` - Cập nhật OptionGroups
- `GET /api/Products/{id}` - Lấy với OptionGroups (filtered)

---

## 🎯 Use Cases

### 1. Sản phẩm có đầy đủ options
```json
{
  "name": "Bạc xỉu",
  "basePrice": 25000,
  "categoryId": 1,
  "optionGroups": [
    {"optionGroupId": 1}  // Lấy tất cả sizes
  ]
}
```

### 2. Sản phẩm chỉ có size nhỏ, trung bình
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

### 3. Sản phẩm có nhiều nhóm options
```json
{
  "name": "Trà sữa",
  "basePrice": 35000,
  "categoryId": 2,
  "optionGroups": [
    {"optionGroupId": 1, "displayOrder": 1},  // Kích cỡ (all)
    {"optionGroupId": 2, "displayOrder": 2},  // Mức đường (all)
    {
      "optionGroupId": 3,
      "displayOrder": 3,
      "allowedItemIds": [7, 9]  // Chỉ Trân châu và Pudding
    }
  ]
}
```

---

## ⚠️ Important Notes

### Validation Rules
1. `OptionGroupId` phải tồn tại
2. `AllowedItemIds` phải thuộc `OptionGroup` đó
3. Không được truyền IDs âm hoặc 0

### Database
- `AllowedItemIdsJson` lưu dạng JSON: `"[1,2,3]"` hoặc `NULL`
- `NULL` = lấy tất cả items
- Helper property `AllowedItemIds` tự động serialize/deserialize

### Response Filter
- Khi GET Product, chỉ trả về items trong `AllowedItemIds`
- Nếu `AllowedItemIds` = null, trả về tất cả

---

## 🔍 Debug Tips

### Check ProductOptionGroup mappings
```sql
SELECT 
    p.Name as ProductName,
    og.Name as OptionGroupName,
    pog.DisplayOrder,
    pog.AllowedItemIdsJson
FROM ProductOptionGroups pog
JOIN Products p ON pog.ProductId = p.Id
JOIN OptionGroups og ON pog.OptionGroupId = og.Id;
```

### Check OptionItems
```sql
SELECT 
    og.Name as GroupName,
    oi.Name as ItemName,
    oi.PriceAdjustment,
    oi.IsDefault
FROM OptionItems oi
JOIN OptionGroups og ON oi.OptionGroupId = og.Id
ORDER BY og.Name, oi.DisplayOrder;
```

---

## 📖 References

- [PRODUCT_OPTIONS_LOGIC.md](CoffeeShopApi/Docs/PRODUCT_OPTIONS_LOGIC.md) - Kiến trúc và workflows
- [CREATE_PRODUCT_WITH_OPTIONS.md](CoffeeShopApi/Docs/CREATE_PRODUCT_WITH_OPTIONS.md) - Hướng dẫn sử dụng
- [OPTION_CRUD_API.md](CoffeeShopApi/Docs/OPTION_CRUD_API.md) - API reference
- [FILE_UPLOAD_SERVICE.md](CoffeeShopApi/Docs/FILE_UPLOAD_SERVICE.md) - Upload file
