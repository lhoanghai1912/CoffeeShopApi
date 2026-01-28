# Category Module Documentation

## 📂 Overview

Category Module quản lý danh mục sản phẩm (Coffee, Tea, Food, Freeze).

**Controller:** `CategoriesController`  
**Service:** `CategoryService`  
**Repository:** `CategoryRepository`  
**Entity:** `Category`

---

## 🎯 Key Features

1. **Simple CRUD** - Create, Read, Update, Delete categories
2. **Product Filtering** - Filter products by category
3. **Public Access** - Anyone can view categories (no auth required)
4. **Admin Management** - Only admins can create/update/delete

---

## 📡 API Endpoints

### 1. Get All Categories

**Endpoint:** `GET /api/categories`

**Authorization:** None (Public)

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "name": "Coffee",
      "productCount": 10
    },
    {
      "id": 2,
      "name": "Tea",
      "productCount": 10
    },
    {
      "id": 3,
      "name": "Food",
      "productCount": 6
    },
    {
      "id": 4,
      "name": "Freeze",
      "productCount": 4
    }
  ]
}
```

**cURL Example:**
```bash
curl -X GET https://localhost:5001/api/categories
```

---

### 2. Get Category by ID

**Endpoint:** `GET /api/categories/{id}`

**Authorization:** None (Public)

**Path Parameters:**
- `id` : int - Category ID

**Response:**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "name": "Coffee",
    "productCount": 10
  }
}
```

**Response (Not Found):**
```json
{
  "success": false,
  "message": "Không tìm thấy danh mục",
  "status": 404
}
```

---

### 3. Create Category (Admin)

**Endpoint:** `POST /api/categories`

**Authorization:** ADMIN or STAFF

**Request Body:**
```json
{
  "name": "Smoothie"
}
```

**Validation Rules:**
```csharp
✅ Name: Required, max 100 characters, unique
```

**Response (Success):**
```json
{
  "success": true,
  "message": "Tạo danh mục thành công",
  "data": {
    "id": 5,
    "name": "Smoothie",
    "productCount": 0
  }
}
```

**Response (Duplicate Name):**
```json
{
  "success": false,
  "message": "Tên danh mục đã tồn tại",
  "status": 400
}
```

---

### 4. Update Category (Admin)

**Endpoint:** `PUT /api/categories/{id}`

**Authorization:** ADMIN or STAFF

**Path Parameters:**
- `id` : int - Category ID

**Request Body:**
```json
{
  "name": "Smoothie & Juice"
}
```

**Response (Success):**
```json
{
  "success": true,
  "message": "Cập nhật danh mục thành công",
  "data": {
    "id": 5,
    "name": "Smoothie & Juice",
    "productCount": 0
  }
}
```

**Response (Not Found):**
```json
{
  "success": false,
  "message": "Không tìm thấy danh mục",
  "status": 404
}
```

---

### 5. Delete Category (Admin)

**Endpoint:** `DELETE /api/categories/{id}`

**Authorization:** ADMIN only

**Path Parameters:**
- `id` : int - Category ID

**Response (Success):**
```json
{
  "success": true,
  "message": "Xóa danh mục thành công"
}
```

**Response (Has Products):**
```json
{
  "success": false,
  "message": "Không thể xóa danh mục đang có sản phẩm",
  "status": 400
}
```

**Business Logic:**
- ✅ Check if category has products
- ✅ If has products → Cannot delete (or cascade delete if needed)
- ✅ If no products → Delete category

---

## 🏗️ Database Schema

### Categories Table

```sql
CREATE TABLE Categories (
    Id INT PRIMARY KEY IDENTITY,
    Name NVARCHAR(100) UNIQUE NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NOT NULL
);

-- Indexes
CREATE UNIQUE INDEX IX_Categories_Name ON Categories(Name);
```

### Seed Data

```csharp
modelBuilder.Entity<Category>().HasData(
    new Category { Id = 1, Name = "Coffee", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    new Category { Id = 2, Name = "Tea", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    new Category { Id = 3, Name = "Food", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    new Category { Id = 4, Name = "Freeze", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
);
```

---

## 💡 Business Rules

### Category Validation

```csharp
✅ Name: Required, 1-100 characters
✅ Name must be unique (case-insensitive)
✅ Cannot delete category with products
```

### Product Count

```csharp
// Calculate dynamically when fetching categories
public async Task<IEnumerable<CategoryResponse>> GetAllAsync()
{
    var categories = await _context.Categories
        .Select(c => new CategoryResponse
        {
            Id = c.Id,
            Name = c.Name,
            ProductCount = c.Products.Count(p => p.IsActive)
        })
        .ToListAsync();
    
    return categories;
}
```

---

## 🔄 Integration with Products

### Get Products by Category

**Endpoint:** `GET /api/products/paged?filter=CategoryId=1`

**Example:**
```bash
# Get all Coffee products
GET /api/products/paged?filter=CategoryId=1&page=1&pageSize=20

# Get Tea products sorted by price
GET /api/products/paged?filter=CategoryId=2&orderBy=BasePrice asc
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
        "basePrice": 25000,
        "categoryId": 1,
        "categoryName": "Coffee"
      }
    ],
    "totalCount": 10,
    "page": 1,
    "pageSize": 20
  }
}
```

---

## 🔐 Authorization

| Action | Permission | Roles |
|--------|-----------|-------|
| GET /categories | None | Public |
| GET /categories/{id} | None | Public |
| POST /categories | `category.create` | ADMIN, STAFF |
| PUT /categories/{id} | `category.update` | ADMIN, STAFF |
| DELETE /categories/{id} | `category.delete` | ADMIN |

---

## 🐛 Common Errors

### 1. Category Not Found
```json
{
  "success": false,
  "message": "Không tìm thấy danh mục",
  "status": 404
}
```

### 2. Duplicate Category Name
```json
{
  "success": false,
  "message": "Tên danh mục đã tồn tại",
  "status": 400
}
```

### 3. Cannot Delete Category with Products
```json
{
  "success": false,
  "message": "Không thể xóa danh mục đang có sản phẩm. Vui lòng xóa hoặc chuyển sản phẩm sang danh mục khác trước.",
  "status": 400
}
```

### 4. Unauthorized
```json
{
  "success": false,
  "message": "Bạn không có quyền thực hiện thao tác này",
  "status": 403
}
```

---

## 📱 Frontend Integration Example

### React - Category Filter

```typescript
function ProductPage() {
  const [categories, setCategories] = useState([]);
  const [selectedCategory, setSelectedCategory] = useState(null);
  const [products, setProducts] = useState([]);
  
  useEffect(() => {
    // Fetch categories
    fetch('/api/categories')
      .then(res => res.json())
      .then(data => setCategories(data.data));
  }, []);
  
  useEffect(() => {
    // Fetch products by category
    const categoryFilter = selectedCategory 
      ? `filter=CategoryId=${selectedCategory}` 
      : '';
    
    fetch(`/api/products/paged?${categoryFilter}&page=1&pageSize=20`)
      .then(res => res.json())
      .then(data => setProducts(data.data.items));
  }, [selectedCategory]);
  
  return (
    <div>
      {/* Category Tabs */}
      <div className="category-tabs">
        <button 
          onClick={() => setSelectedCategory(null)}
          className={!selectedCategory ? 'active' : ''}
        >
          Tất cả
        </button>
        {categories.map(cat => (
          <button
            key={cat.id}
            onClick={() => setSelectedCategory(cat.id)}
            className={selectedCategory === cat.id ? 'active' : ''}
          >
            {cat.name} ({cat.productCount})
          </button>
        ))}
      </div>
      
      {/* Product Grid */}
      <div className="product-grid">
        {products.map(product => (
          <ProductCard key={product.id} product={product} />
        ))}
      </div>
    </div>
  );
}
```

### React - Admin Category Management

```typescript
function CategoryManagement() {
  const [categories, setCategories] = useState([]);
  const [editing, setEditing] = useState(null);
  
  const fetchCategories = async () => {
    const response = await fetch('/api/categories');
    const data = await response.json();
    setCategories(data.data);
  };
  
  useEffect(() => {
    fetchCategories();
  }, []);
  
  const handleCreate = async (name) => {
    const response = await fetch('/api/categories', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify({ name })
    });
    
    if (response.ok) {
      toast.success('Tạo danh mục thành công');
      await fetchCategories();
    }
  };
  
  const handleUpdate = async (id, name) => {
    const response = await fetch(`/api/categories/${id}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify({ name })
    });
    
    if (response.ok) {
      toast.success('Cập nhật thành công');
      setEditing(null);
      await fetchCategories();
    }
  };
  
  const handleDelete = async (id) => {
    if (!confirm('Bạn có chắc muốn xóa danh mục này?')) return;
    
    const response = await fetch(`/api/categories/${id}`, {
      method: 'DELETE',
      headers: { 'Authorization': `Bearer ${token}` }
    });
    
    if (response.ok) {
      toast.success('Xóa danh mục thành công');
      await fetchCategories();
    } else {
      const data = await response.json();
      toast.error(data.message);
    }
  };
  
  return (
    <div className="category-management">
      <h2>Quản lý Danh mục</h2>
      
      {/* Create Form */}
      <CategoryCreateForm onSubmit={handleCreate} />
      
      {/* Category List */}
      <table>
        <thead>
          <tr>
            <th>ID</th>
            <th>Tên danh mục</th>
            <th>Số sản phẩm</th>
            <th>Thao tác</th>
          </tr>
        </thead>
        <tbody>
          {categories.map(cat => (
            <tr key={cat.id}>
              <td>{cat.id}</td>
              <td>
                {editing === cat.id ? (
                  <input 
                    defaultValue={cat.name}
                    onBlur={(e) => handleUpdate(cat.id, e.target.value)}
                  />
                ) : (
                  cat.name
                )}
              </td>
              <td>{cat.productCount}</td>
              <td>
                <button onClick={() => setEditing(cat.id)}>Sửa</button>
                <button onClick={() => handleDelete(cat.id)}>Xóa</button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
```

---

## 📊 Statistics

### Category Product Distribution

```csharp
public async Task<CategoryStatsResponse> GetCategoryStatsAsync()
{
    var stats = await _context.Categories
        .Select(c => new
        {
            CategoryId = c.Id,
            CategoryName = c.Name,
            TotalProducts = c.Products.Count(p => p.IsActive),
            TotalRevenue = c.Products
                .SelectMany(p => p.OrderItems)
                .Where(oi => oi.Order.Status == OrderStatus.Paid)
                .Sum(oi => oi.TotalPrice)
        })
        .ToListAsync();
    
    return new CategoryStatsResponse
    {
        Categories = stats
    };
}
```

---

## 🔄 Future Enhancements

### 1. Category Hierarchy (Parent-Child)

```csharp
public class Category
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int? ParentCategoryId { get; set; }
    
    // Navigation
    public Category? ParentCategory { get; set; }
    public ICollection<Category> SubCategories { get; set; }
}
```

### 2. Category Images

```csharp
public class Category
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? ImageUrl { get; set; }
    public string? Description { get; set; }
}
```

### 3. Category Sorting/Ordering

```csharp
public class Category
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int DisplayOrder { get; set; } // Sort categories
    public bool IsActive { get; set; }    // Show/Hide
}
```

---

## 📖 Related Documentation

- 📦 [Product Module](./PRODUCT_MODULE.md)
- 🗄️ [Database Schema](./DATABASE.md)
- 🏗️ [Architecture](./ARCHITECTURE.md)
