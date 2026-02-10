# Logic Tạo Sản Phẩm và Quản Lý Options

## Tổng quan kiến trúc

Hệ thống sử dụng **Template Pattern** để quản lý options:

```
OptionGroup (Template)
    ├── OptionItem 1
    ├── OptionItem 2
    └── OptionItem 3
         ↓ (được áp dụng cho nhiều Product qua mapping)
Product ←→ ProductOptionGroup (Mapping) ←→ OptionGroup
```

### Các thực thể chính

1. **OptionGroup** - Template nhóm tùy chọn (VD: "Kích cỡ", "Mức đường")
2. **OptionItem** - Các lựa chọn cụ thể (VD: "Size S", "Size M", "Size L")
3. **ProductOptionGroup** - Bảng mapping M-N giữa Product và OptionGroup
4. **Product** - Sản phẩm

---

## Workflow 1: Tạo Option Templates (Admin)

### Bước 1: Tạo OptionGroup với OptionItems

**API:** `POST /api/OptionGroups`

```json
{
  "name": "Kích cỡ",
  "description": "Chọn kích cỡ đồ uống",
  "isRequired": true,
  "allowMultiple": false,
  "displayOrder": 1,
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

**Kết quả:**
- Tạo OptionGroup Id = 1
- Tạo 3 OptionItems (Id = 1, 2, 3) thuộc OptionGroup Id = 1

### Bước 2: Tạo thêm các OptionGroups khác

```json
// Mức đường
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

// Topping (cho phép chọn nhiều)
{
  "name": "Topping",
  "description": "Thêm topping",
  "isRequired": false,
  "allowMultiple": true,
  "displayOrder": 3,
  "optionItems": [
    { "name": "Trân châu", "priceAdjustment": 5000, "displayOrder": 1 },
    { "name": "Thạch", "priceAdjustment": 5000, "displayOrder": 2 },
    { "name": "Pudding", "priceAdjustment": 8000, "displayOrder": 3 }
  ]
}
```

---

## Workflow 2: Tạo Product và Gán Options

### Phương án hiện tại (Cần cập nhật)

**Vấn đề:** Product DTOs hiện tại chưa có field để gán OptionGroups

#### Cần thêm vào DTOs:

```csharp
// CreateProductRequest
public class CreateProductRequest
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string? ImageUrl { get; set; }
    public decimal BasePrice { get; set; }
    public int CategoryId { get; set; }
    
    // ⭐ THÊM MỚI
    public List<int> OptionGroupIds { get; set; } = new();
    // Hoặc chi tiết hơn:
    public List<ProductOptionGroupRequest> OptionGroups { get; set; } = new();
}

public class ProductOptionGroupRequest
{
    public int OptionGroupId { get; set; }
    public int DisplayOrder { get; set; }
}
```

### Logic tạo Product với Options

```csharp
// ProductService.CreateAsync()
public async Task<ProductDto> CreateAsync(CreateProductRequest request)
{
    // 1. Tạo Product cơ bản
    var product = new Product
    {
        Name = request.Name,
        Description = request.Description,
        ImageUrl = request.ImageUrl,
        BasePrice = request.BasePrice,
        CategoryId = request.CategoryId
    };
    
    // 2. Tạo mappings với OptionGroups
    if (request.OptionGroupIds.Any())
    {
        foreach (var optionGroupId in request.OptionGroupIds)
        {
            // Kiểm tra OptionGroup có tồn tại không
            if (!await _optionGroupRepository.ExistsAsync(optionGroupId))
            {
                throw new ArgumentException($"OptionGroup {optionGroupId} not found");
            }
            
            product.ProductMappings.Add(new ProductOptionGroup
            {
                OptionGroupId = optionGroupId,
                DisplayOrder = request.OptionGroups
                    .FirstOrDefault(og => og.OptionGroupId == optionGroupId)
                    ?.DisplayOrder ?? 0
            });
        }
    }
    
    // 3. Lưu Product (cascade save ProductOptionGroups)
    await _repository.CreateAsync(product);
    
    return MapToDto(product);
}
```

### Ví dụ request tạo Product:

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
      "optionGroupId": 1,  // Kích cỡ
      "displayOrder": 1
    },
    {
      "optionGroupId": 2,  // Mức đường
      "displayOrder": 2
    },
    {
      "optionGroupId": 3,  // Topping
      "displayOrder": 3
    }
  ]
}
```

**Kết quả:**
- Tạo Product Id = 10
- Tạo 3 ProductOptionGroup mappings:
  - Product 10 ← Kích cỡ (OptionGroup 1)
  - Product 10 ← Mức đường (OptionGroup 2)
  - Product 10 ← Topping (OptionGroup 3)

---

## Workflow 3: Khách hàng đặt món

### Bước 1: Lấy thông tin Product với Options

**API:** `GET /api/Products/10`

**Response:**
```json
{
  "id": 10,
  "name": "Cà phê sữa",
  "basePrice": 25000,
  "optionGroups": [
    {
      "id": 1,
      "name": "Kích cỡ",
      "isRequired": true,
      "allowMultiple": false,
      "displayOrder": 1,
      "optionItems": [
        { "id": 1, "name": "Size S", "priceAdjustment": 0, "isDefault": true },
        { "id": 2, "name": "Size M", "priceAdjustment": 5000 },
        { "id": 3, "name": "Size L", "priceAdjustment": 10000 }
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
    },
    {
      "id": 3,
      "name": "Topping",
      "isRequired": false,
      "allowMultiple": true,
      "displayOrder": 3,
      "optionItems": [
        { "id": 7, "name": "Trân châu", "priceAdjustment": 5000 },
        { "id": 8, "name": "Thạch", "priceAdjustment": 5000 },
        { "id": 9, "name": "Pudding", "priceAdjustment": 8000 }
      ]
    }
  ]
}
```

### Bước 2: Khách chọn options và tạo Order

**API:** `POST /api/Orders`

```json
{
  "orderItems": [
    {
      "productId": 10,
      "quantity": 2,
      "selectedOptions": [
        {
          "optionItemId": 2  // Size M (+5000)
        },
        {
          "optionItemId": 5  // Ít đường (+0)
        },
        {
          "optionItemId": 7  // Trân châu (+5000)
        },
        {
          "optionItemId": 9  // Pudding (+8000)
        }
      ]
    }
  ]
}
```

### Bước 3: Tính giá

```csharp
// OrderService.CalculatePrice()
decimal basePrice = 25000;
decimal optionsTotal = 5000 + 0 + 5000 + 8000; // 18000
decimal itemPrice = basePrice + optionsTotal;   // 43000
decimal totalPrice = itemPrice * 2;             // 86000 (x2 ly)
```

---

## Workflow 4: Cập nhật/Chỉnh sửa Options của Product

### Cách 1: Update toàn bộ Product (recommend)

**API:** `PUT /api/Products/10`

```json
{
  "name": "Cà phê sữa",
  "basePrice": 25000,
  "categoryId": 1,
  "optionGroups": [
    {
      "optionGroupId": 1,  // Kích cỡ (giữ nguyên)
      "displayOrder": 1
    },
    {
      "optionGroupId": 2,  // Mức đường (giữ nguyên)
      "displayOrder": 2
    },
    {
      "optionGroupId": 4,  // Mức đá (thêm mới)
      "displayOrder": 3
    }
    // Bỏ OptionGroup 3 (Topping)
  ]
}
```

**Logic ProductService.UpdateAsync():**

```csharp
// 1. Xóa tất cả ProductOptionGroup cũ
_context.ProductOptionGroups
    .Where(pog => pog.ProductId == productId)
    .ExecuteDelete();

// 2. Thêm ProductOptionGroup mới
foreach (var optionGroup in request.OptionGroups)
{
    product.ProductMappings.Add(new ProductOptionGroup
    {
        OptionGroupId = optionGroup.OptionGroupId,
        DisplayOrder = optionGroup.DisplayOrder
    });
}

// 3. Lưu
await _context.SaveChangesAsync();
```

### Cách 2: API riêng để quản lý Product Options

**Thêm ProductOptionsController:**

```csharp
// Gán OptionGroup cho Product
POST /api/Products/10/options
{
  "optionGroupId": 4,
  "displayOrder": 3
}

// Xóa OptionGroup khỏi Product
DELETE /api/Products/10/options/4

// Update DisplayOrder
PUT /api/Products/10/options/4
{
  "displayOrder": 1
}
```

---

## Workflow 5: Chỉnh sửa OptionGroup Template

### Trường hợp 1: Sửa OptionGroup (không ảnh hưởng Products)

**API:** `PUT /api/OptionGroups/1`

```json
{
  "name": "Kích cỡ (Updated)",
  "description": "Chọn kích cỡ của bạn",
  "isRequired": true,
  "allowMultiple": false,
  "displayOrder": 1
}
```

**Ảnh hưởng:**
- ✅ OptionGroup được update
- ✅ Tất cả Products đang dùng OptionGroup này sẽ thấy thay đổi
- ⚠️  Không ảnh hưởng Orders đã đặt

### Trường hợp 2: Thêm OptionItem vào OptionGroup

**API:** `POST /api/OptionItems/group/1`

```json
{
  "name": "Size XL",
  "priceAdjustment": 15000,
  "isDefault": false,
  "displayOrder": 4
}
```

**Ảnh hưởng:**
- ✅ Tất cả Products có OptionGroup 1 sẽ có thêm option "Size XL"
- ⚠️  Không ảnh hưởng Orders đã đặt

### Trường hợp 3: Xóa OptionItem

**API:** `DELETE /api/OptionItems/3` (Size L)

**Ảnh hưởng:**
- ⚠️  **Nguy hiểm**: Nếu có Orders đã chọn option này, cần xử lý:
  - Option 1: Soft delete (thêm field `IsDeleted`, không hiển thị cho đơn mới)
  - Option 2: Kiểm tra trước khi xóa (không cho xóa nếu đang được dùng)
  - Option 3: Xóa luôn (lưu lại name trong OrderItemOption)

**Recommend: Soft Delete**

```csharp
// OptionItem model
public bool IsDeleted { get; set; } = false;

// Khi xóa
optionItem.IsDeleted = true;
await _context.SaveChangesAsync();

// Query: chỉ lấy items chưa xóa
_context.OptionItems.Where(oi => !oi.IsDeleted);
```

---

## Best Practices

### 1. Validation khi tạo Order

```csharp
// Kiểm tra required options
foreach (var optionGroup in product.OptionGroups.Where(og => og.IsRequired))
{
    if (!orderItem.SelectedOptions.Any(so => 
        so.OptionItem.OptionGroupId == optionGroup.Id))
    {
        throw new ValidationException(
            $"Option group '{optionGroup.Name}' is required");
    }
}

// Kiểm tra allowMultiple
foreach (var optionGroup in product.OptionGroups.Where(og => !og.AllowMultiple))
{
    var selectedCount = orderItem.SelectedOptions.Count(so => 
        so.OptionItem.OptionGroupId == optionGroup.Id);
        
    if (selectedCount > 1)
    {
        throw new ValidationException(
            $"Option group '{optionGroup.Name}' allows only one selection");
    }
}
```

### 2. Cache OptionGroups

```csharp
// OptionGroups ít thay đổi, nên cache
services.AddMemoryCache();

var cachedOptionGroups = await _cache.GetOrCreateAsync(
    "option-groups",
    async entry =>
    {
        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
        return await _optionGroupRepository.GetAllAsync();
    });
```

### 3. Audit log khi sửa Options

```csharp
// Log mọi thay đổi về Options
_logger.LogInformation(
    "OptionGroup {Id} updated by {UserId}. Old: {Old}, New: {New}",
    optionGroup.Id,
    userId,
    JsonSerializer.Serialize(oldState),
    JsonSerializer.Serialize(newState));
```

### 4. Versioning cho OptionGroups

Nếu cần lưu lại lịch sử thay đổi:

```csharp
public class OptionGroupVersion
{
    public int Id { get; set; }
    public int OptionGroupId { get; set; }
    public int Version { get; set; }
    public string Snapshot { get; set; } // JSON
    public DateTime CreatedAt { get; set; }
}
```

---

## Tóm tắt Flow

```
1. ADMIN: Tạo OptionGroups (templates)
   ↓
2. ADMIN: Tạo Products và gán OptionGroups
   ↓
3. CUSTOMER: Xem Product, chọn OptionItems
   ↓
4. CUSTOMER: Tạo Order với selectedOptions
   ↓
5. SYSTEM: Tính giá = basePrice + sum(optionItem.priceAdjustment)
   ↓
6. ADMIN: Có thể sửa OptionGroups (ảnh hưởng toàn bộ Products)
   ↓
7. ADMIN: Có thể sửa Product-OptionGroup mapping (chỉ ảnh hưởng Product đó)
```

---

## Cần implement

### 1. Cập nhật ProductDTO
```csharp
public class ProductDto
{
    // ... existing fields
    public List<OptionGroupDto> OptionGroups { get; set; } = new();
}
```

### 2. Cập nhật ProductService

```csharp
public async Task<ProductDto> GetByIdAsync(int id)
{
    var product = await _repository.GetByIdAsync(id);
    
    // Load OptionGroups qua ProductOptionGroups
    var optionGroups = await _context.ProductOptionGroups
        .Where(pog => pog.ProductId == id)
        .Include(pog => pog.OptionGroup)
            .ThenInclude(og => og.OptionItems)
        .OrderBy(pog => pog.DisplayOrder)
        .Select(pog => pog.OptionGroup)
        .ToListAsync();
    
    var dto = MapToDto(product);
    dto.OptionGroups = optionGroups.Select(MapOptionGroupToDto).ToList();
    
    return dto;
}
```

### 3. Cập nhật ProductRepository

```csharp
public async Task<Product?> GetByIdAsync(int id)
{
    return await _context.Products
        .Include(p => p.Category)
        .Include(p => p.ProductMappings)
            .ThenInclude(pog => pog.OptionGroup)
                .ThenInclude(og => og.OptionItems)
        .FirstOrDefaultAsync(p => p.Id == id);
}
```

---

Bạn cần tôi implement code cụ thể cho phần nào?
