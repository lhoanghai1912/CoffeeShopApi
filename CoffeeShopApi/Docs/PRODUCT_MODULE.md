# Product Module Documentation

## 📦 Overview

Product Module quản lý sản phẩm (đồ uống, đồ ăn) với hệ thống options phức tạp (Size, đường, topping).

**Controller:** `ProductsController`  
**Service:** `ProductService`  
**Repository:** `ProductRepository`  
**Entities:** `Product`, `OptionGroup`, `OptionItem`

---

## 🎯 Key Features

1. **Product Management**: CRUD operations
2. **Option System**: Flexible options (Size, Sugar Level, Toppings)
3. **Image Upload**: Upload và lưu trữ hình ảnh
4. **Category Filtering**: Lọc theo danh mục
5. **Search & Pagination**: Full-text search và phân trang
6. **Price Calculation**: Giá base + giá options

---

## 📡 API Endpoints

### 1. Get All Products (Paginated)

**Endpoint:** `GET /api/products/paged`

**Query Parameters:**
```
page        : int (default=1)
pageSize    : int (default=10)
search      : string? (search in Name, Description)
orderBy     : string? (e.g., "Name asc", "BasePrice desc")
filter      : string? (Gridify filter syntax)
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
        "description": "Cà phê Robusta đậm đà, thơm nồng",
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
            "displayOrder": 1,
            "optionItems": [
              {
                "id": 1,
                "name": "Nhỏ (S)",
                "priceAdjustment": 0,
                "isDefault": true,
                "displayOrder": 1
              },
              {
                "id": 2,
                "name": "Vừa (M)",
                "priceAdjustment": 5000,
                "isDefault": false,
                "displayOrder": 2
              }
            ]
          }
        ]
      }
    ],
    "totalCount": 30,
    "page": 1,
    "pageSize": 10,
    "totalPages": 3
  }
}
```

**Example cURL:**
```bash
curl -X GET "https://localhost:5001/api/products/paged?page=1&pageSize=10&search=cà%20phê"
```

---

### 2. Get Product by ID

**Endpoint:** `GET /api/products/{id}`

**Path Parameters:**
- `id` : int - Product ID

**Response:** (Same structure as above, single product)

**Use Case:** Chi tiết sản phẩm khi user click vào một product

---

### 3. Create Product (Admin Only)

**Endpoint:** `POST /api/products`

**Content-Type:** `multipart/form-data`

**Form Fields:**
- `FormField` : JSON string (CreateProductRequest)
- `Image` : File (optional)

**CreateProductRequest (JSON):**
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
      "displayOrder": 1,
      "optionItems": [
        {
          "name": "Nhỏ (S)",
          "priceAdjustment": 0,
          "isDefault": true,
          "displayOrder": 1
        },
        {
          "name": "Vừa (M)",
          "priceAdjustment": 5000,
          "displayOrder": 2
        },
        {
          "name": "Lớn (L)",
          "priceAdjustment": 10000,
          "displayOrder": 3
        }
      ]
    },
    {
      "name": "Mức đường",
      "isRequired": true,
      "allowMultiple": false,
      "displayOrder": 2,
      "optionItems": [
        { "name": "30%", "priceAdjustment": 0, "displayOrder": 1 },
        { "name": "50%", "priceAdjustment": 0, "displayOrder": 2 },
        { "name": "70%", "priceAdjustment": 0, "isDefault": true, "displayOrder": 3 },
        { "name": "100%", "priceAdjustment": 0, "displayOrder": 4 }
      ]
    },
    {
      "name": "Topping",
      "isRequired": false,
      "allowMultiple": true,
      "displayOrder": 3,
      "optionItems": [
        { "name": "Trân châu đen", "priceAdjustment": 10000, "displayOrder": 1 },
        { "name": "Trân châu trắng", "priceAdjustment": 10000, "displayOrder": 2 },
        { "name": "Thạch dừa", "priceAdjustment": 8000, "displayOrder": 3 }
      ]
    }
  ]
}
```

**Example with cURL:**
```bash
curl -X POST "https://localhost:5001/api/products" \
  -H "Authorization: Bearer {token}" \
  -F "FormField={JSON string above}" \
  -F "Image=@/path/to/image.jpg"
```

**Response:**
```json
{
  "success": true,
  "message": "Tạo sản phẩm thành công",
  "data": {
    "id": 31,
    "name": "Cà Phê Cốt Dừa",
    ...
  }
}
```

---

### 4. Update Product (Admin Only)

**Endpoint:** `PUT /api/products/{id}`

**Content-Type:** `multipart/form-data`

**Form Fields:**
- `FormField` : JSON string (UpdateProductRequest)
- `Image` : File (optional, only if changing image)

**UpdateProductRequest (JSON):**
```json
{
  "name": "Cà Phê Cốt Dừa (Cập nhật)",
  "description": "Cà phê Việt Nam kết hợp cốt dừa thơm béo",
  "basePrice": 40000,
  "categoryId": 1,
  "imageUrl": "/images/caphecotdua.jpg"
}
```

**Note:** Để update options, cần gửi lại toàn bộ `optionGroups` array.

**Response:**
```json
{
  "success": true,
  "message": "Cập nhật sản phẩm thành công",
  "data": true
}
```

---

### 5. Delete Product (Admin Only)

**Endpoint:** `DELETE /api/products/{id}`

**Path Parameters:**
- `id` : int - Product ID

**Response:**
```json
{
  "success": true,
  "message": "Xóa sản phẩm thành công",
  "data": true
}
```

**Note:** Đây là **soft delete** - product vẫn còn trong database nhưng `IsActive = false`.

---

## 🏗️ Database Schema

### Products Table

```sql
CREATE TABLE Products (
    Id INT PRIMARY KEY IDENTITY,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000),
    BasePrice DECIMAL(18,2) NOT NULL,
    ImageUrl NVARCHAR(500),
    CategoryId INT NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NOT NULL,
    CONSTRAINT FK_Products_Categories FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
);

-- Indexes
CREATE INDEX IX_Products_CategoryId ON Products(CategoryId);
CREATE INDEX IX_Products_IsActive ON Products(IsActive);
CREATE INDEX IX_Products_Name ON Products(Name);
```

### OptionGroups Table

```sql
CREATE TABLE OptionGroups (
    Id INT PRIMARY KEY IDENTITY,
    ProductId INT NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    IsRequired BIT NOT NULL DEFAULT 0,
    AllowMultiple BIT NOT NULL DEFAULT 0,
    DisplayOrder INT NOT NULL DEFAULT 0,
    CONSTRAINT FK_OptionGroups_Products FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE
);

CREATE INDEX IX_OptionGroups_ProductId ON OptionGroups(ProductId);
```

### OptionItems Table

```sql
CREATE TABLE OptionItems (
    Id INT PRIMARY KEY IDENTITY,
    OptionGroupId INT NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    PriceAdjustment DECIMAL(18,2) NOT NULL DEFAULT 0,
    IsDefault BIT NOT NULL DEFAULT 0,
    DisplayOrder INT NOT NULL DEFAULT 0,
    CONSTRAINT FK_OptionItems_OptionGroups FOREIGN KEY (OptionGroupId) REFERENCES OptionGroups(Id) ON DELETE CASCADE
);

CREATE INDEX IX_OptionItems_OptionGroupId ON OptionItems(OptionGroupId);
```

---

## 💡 Business Rules

### 1. Product Validation

```csharp
✅ Name: Required, Max 200 characters
✅ BasePrice: > 0
✅ CategoryId: Must exist in Categories table
✅ ImageUrl: Optional, valid URL format
✅ Description: Optional, Max 1000 characters
```

### 2. Option Groups Rules

```csharp
✅ IsRequired = true: User MUST select an option
✅ AllowMultiple = true: User can select many options (e.g., Toppings)
✅ AllowMultiple = false: User can only select one (e.g., Size)
✅ DisplayOrder: Thứ tự hiển thị trên UI
```

### 3. Option Items Rules

```csharp
✅ PriceAdjustment: Có thể âm (discount) hoặc dương (extra charge)
✅ IsDefault = true: Tự động chọn khi load product
✅ Mỗi OptionGroup chỉ có TỐI ĐA 1 IsDefault = true
```

### 4. Price Calculation

```csharp
FinalPrice = BasePrice + SUM(SelectedOptions.PriceAdjustment)

Example:
- Cà Phê Đen Đá (Base): 25,000đ
- Size Lớn (L): +10,000đ
- Trân châu đen: +10,000đ
- Pudding: +12,000đ
-----------------------------------
Total: 57,000đ
```

---

## 🖼️ Image Upload

### Flow

```
1️⃣ Client uploads file via multipart/form-data
   ↓
2️⃣ FileUploadService validates:
   • File extension (.jpg, .png, .webp)
   • File size (< 5MB)
   • MIME type
   ↓
3️⃣ Generate unique filename: {productId}_{timestamp}.jpg
   ↓
4️⃣ Save to wwwroot/images/
   ↓
5️⃣ Return relative URL: /images/filename.jpg
```

### Configuration

```csharp
// appsettings.json
{
  "FileUpload": {
    "MaxFileSizeInMB": 5,
    "AllowedExtensions": [".jpg", ".jpeg", ".png", ".webp"],
    "UploadDirectory": "wwwroot/images"
  }
}
```

### Example Service

```csharp
public class FileUploadService : IFileUploadService
{
    public async Task<string> UploadImageAsync(IFormFile file)
    {
        // Validate
        if (file.Length > 5 * 1024 * 1024)
            throw new ArgumentException("File quá lớn (max 5MB)");
        
        var ext = Path.GetExtension(file.FileName).ToLower();
        if (!new[] { ".jpg", ".jpeg", ".png", ".webp" }.Contains(ext))
            throw new ArgumentException("Định dạng file không hợp lệ");
        
        // Generate filename
        var fileName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine("wwwroot", "images", fileName);
        
        // Save
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }
        
        return $"/images/{fileName}";
    }
}
```

---

## 🔍 Search & Filter

### Gridify Filter Syntax

```bash
# Filter by category
GET /api/products/paged?filter=CategoryId=1

# Search by name
GET /api/products/paged?search=cà phê

# Filter by price range
GET /api/products/paged?filter=BasePrice>=20000,BasePrice<=50000

# Order by price descending
GET /api/products/paged?orderBy=BasePrice desc

# Combined
GET /api/products/paged?filter=CategoryId=1&search=trà&orderBy=BasePrice asc&page=1&pageSize=20
```

---

## 🎨 Frontend Integration Example

### React Component (Add to Cart)

```typescript
interface Product {
  id: number;
  name: string;
  basePrice: number;
  imageUrl: string;
  optionGroups: OptionGroup[];
}

interface CartItem {
  productId: number;
  productName: string;
  quantity: number;
  selectedOptions: { id: number; name: string; price: number }[];
  totalPrice: number;
}

function ProductDetail({ productId }: { productId: number }) {
  const [product, setProduct] = useState<Product | null>(null);
  const [selectedOptions, setSelectedOptions] = useState<number[]>([]);
  
  useEffect(() => {
    fetch(`/api/products/${productId}`)
      .then(res => res.json())
      .then(data => setProduct(data.data));
  }, [productId]);
  
  const calculateTotal = () => {
    if (!product) return 0;
    let total = product.basePrice;
    
    selectedOptions.forEach(optionId => {
      const option = product.optionGroups
        .flatMap(g => g.optionItems)
        .find(o => o.id === optionId);
      if (option) total += option.priceAdjustment;
    });
    
    return total;
  };
  
  const handleAddToCart = () => {
    const cartItem: CartItem = {
      productId: product.id,
      productName: product.name,
      quantity: 1,
      selectedOptions: selectedOptions.map(id => {
        const option = product.optionGroups
          .flatMap(g => g.optionItems)
          .find(o => o.id === id);
        return { id, name: option.name, price: option.priceAdjustment };
      }),
      totalPrice: calculateTotal()
    };
    
    // Add to cart state/Redux/Context
    addToCart(cartItem);
  };
  
  return (
    <div>
      <h1>{product?.name}</h1>
      <img src={product?.imageUrl} />
      <p>Giá: {calculateTotal().toLocaleString()}đ</p>
      
      {product?.optionGroups.map(group => (
        <div key={group.id}>
          <h3>{group.name} {group.isRequired && '*'}</h3>
          {group.optionItems.map(item => (
            <label key={item.id}>
              <input
                type={group.allowMultiple ? 'checkbox' : 'radio'}
                name={`group-${group.id}`}
                value={item.id}
                onChange={() => handleOptionChange(item.id, group.allowMultiple)}
              />
              {item.name} {item.priceAdjustment > 0 && `(+${item.priceAdjustment}đ)`}
            </label>
          ))}
        </div>
      ))}
      
      <button onClick={handleAddToCart}>Thêm vào giỏ</button>
    </div>
  );
}
```

---

## 🐛 Common Errors

### 1. Product not found
```json
{
  "success": false,
  "message": "Không tìm thấy sản phẩm",
  "status": 404
}
```

### 2. Validation Error
```json
{
  "success": false,
  "message": "Validation Error",
  "status": 400,
  "errors": [
    "Tên sản phẩm không được để trống",
    "Giá sản phẩm phải lớn hơn 0"
  ]
}
```

### 3. Image Upload Error
```json
{
  "success": false,
  "message": "File quá lớn (max 5MB)",
  "status": 400
}
```

---

## 📖 Related Documentation

- 📂 [Category Module](./CATEGORY_MODULE.md)
- 📋 [Order Module](./ORDER_MODULE.md) (How products are used in orders)
- 🗄️ [Database Schema](./DATABASE.md)
