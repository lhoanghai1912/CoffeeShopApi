# Architecture Documentation

## 📐 System Architecture

### Layered Architecture

CoffeeShopApi tuân theo **Clean Architecture** với các layer rõ ràng:

```
┌─────────────────────────────────────────────────────────┐
│                    Presentation Layer                    │
│                     (Controllers)                        │
│  • AuthController, UsersController, ProductsController  │
│  • Handles HTTP requests/responses                       │
│  • Model validation                                      │
│  • JWT token extraction                                  │
└─────────────────────────────────────────────────────────┘
                           ↓
┌─────────────────────────────────────────────────────────┐
│                   Business Logic Layer                   │
│                      (Services)                          │
│  • AuthService, UserService, ProductService             │
│  • Business rule validation                              │
│  • Transaction orchestration                             │
│  • DTO mapping                                           │
└─────────────────────────────────────────────────────────┘
                           ↓
┌─────────────────────────────────────────────────────────┐
│                   Data Access Layer                      │
│                    (Repositories)                        │
│  • ProductRepository, OrderRepository                    │
│  • CRUD operations                                       │
│  • Query optimization                                    │
│  • EF Core DbContext                                     │
└─────────────────────────────────────────────────────────┘
                           ↓
┌─────────────────────────────────────────────────────────┐
│                       Database                           │
│                     SQL Server                           │
│  • Tables, Indexes, Constraints                          │
│  • Stored Procedures (if any)                            │
└─────────────────────────────────────────────────────────┘
```

---

## 🔄 Request Lifecycle

### Example: Create Order Flow

```
1️⃣ Client sends POST /api/orders
   Body: { userId: 1, items: [...], note: "..." }
   Header: Authorization: Bearer {token}
   
   ↓

2️⃣ ASP.NET Core Middleware Pipeline
   ├─ Authentication Middleware
   │  └─ Validates JWT token
   │  └─ Sets User.Identity
   │
   ├─ Authorization Middleware
   │  └─ Checks permissions
   │
   └─ Model Validation
      └─ Validates CreateOrderRequest DTO
   
   ↓

3️⃣ OrdersController.Create(CreateOrderRequest request)
   ├─ Extracts UserId from JWT claims
   ├─ Calls OrderService.CreateOrderAsync(request)
   └─ Returns ApiResponse<OrderResponse>
   
   ↓

4️⃣ OrderService.CreateOrderAsync()
   ├─ Begins Database Transaction
   │
   ├─ Validates business rules:
   │  ├─ Check products exist
   │  ├─ Validate options
   │  └─ Calculate totals
   │
   ├─ Calls OrderRepository.CreateAsync(order)
   │  └─ Saves Order + OrderItems to database
   │
   ├─ Commits Transaction
   │
   └─ Maps Order entity to OrderResponse DTO
   
   ↓

5️⃣ OrderRepository.CreateAsync(Order order)
   ├─ context.Orders.Add(order)
   ├─ await context.SaveChangesAsync()
   └─ Returns saved order with generated ID
   
   ↓

6️⃣ SQL Server
   ├─ INSERT INTO Orders (...)
   ├─ INSERT INTO OrderItems (...)
   └─ Returns affected rows
   
   ↓

7️⃣ Response to Client
   {
     "success": true,
     "message": "Tạo đơn hàng thành công",
     "status": 200,
     "data": {
       "id": 123,
       "orderCode": "ORD-20250128-00123",
       "status": "Draft",
       ...
     }
   }
```

---

## 🏗️ Design Patterns

### 1. Repository Pattern

**Purpose:** Tách biệt logic truy cập dữ liệu khỏi business logic.

**Example:**
```csharp
// Interface
public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id);
    Task<IEnumerable<Product>> GetAllAsync();
    Task<Product> CreateAsync(Product product);
    Task UpdateAsync(Product product);
    Task<bool> DeleteAsync(int id);
}

// Implementation
public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;
    
    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.OptionGroups)
            .FirstOrDefaultAsync(p => p.Id == id);
    }
}
```

**Benefits:**
- ✅ Testability (dễ mock trong unit tests)
- ✅ Maintainability (thay đổi data access không ảnh hưởng business logic)
- ✅ Reusability (tái sử dụng queries)

---

### 2. Service Layer Pattern

**Purpose:** Chứa business logic, orchestrate nhiều repositories.

**Example:**
```csharp
public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IVoucherService _voucherService;
    
    public async Task<OrderResponse> CheckoutOrderAsync(int orderId, CheckoutOrderRequest request)
    {
        // Transaction boundary
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Business logic
            var order = await _orderRepository.GetByIdAsync(orderId);
            ValidateOrderBeforeCheckout(order);
            
            // Apply voucher
            if (request.VoucherId.HasValue)
            {
                var voucher = await _voucherService.ApplyVoucherAsync(request.VoucherId.Value, order.UserId);
                order.DiscountAmount = CalculateDiscount(voucher, order.SubTotal);
            }
            
            // Snapshot address
            var address = await _addressService.GetByIdAsync(request.AddressId);
            order.ShippingAddress = address.FullAddress;
            
            // Save changes
            await _orderRepository.UpdateAsync(order);
            await transaction.CommitAsync();
            
            return MapToResponse(order);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
```

---

### 3. DTO Pattern (Data Transfer Object)

**Purpose:** Tách biệt API contract với database entities.

**Benefits:**
- ✅ Bảo mật: Không expose internal fields (Password, CreatedAt, etc.)
- ✅ Flexibility: Dễ thay đổi API response mà không ảnh hưởng database
- ✅ Validation: Centralized validation rules

**Example:**
```csharp
// Entity (Database)
public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }  // ❌ Không trả về client
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public Role Role { get; set; }
}

// DTO (API Response)
public class UserProfileResponse
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    // ✅ No Password field
    // ✅ Can add computed fields
    public int TotalOrders { get; set; }
}
```

---

### 4. Unit of Work Pattern (via DbContext)

**Purpose:** Quản lý transactions và ensure data consistency.

**Implementation:** EF Core's `DbContext` đã implement UoW pattern.

```csharp
// Explicit transaction
using var transaction = await _context.Database.BeginTransactionAsync();
try
{
    // Multiple operations
    _context.Orders.Add(order);
    _context.OrderItems.AddRange(items);
    await _context.SaveChangesAsync();
    
    await _voucherService.ApplyVoucherAsync(...);
    
    await transaction.CommitAsync(); // All or nothing
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

---

## 🔐 Security Architecture

### 1. Authentication Flow

```
┌──────────┐                                    ┌──────────────┐
│  Client  │                                    │  API Server  │
└────┬─────┘                                    └──────┬───────┘
     │                                                  │
     │  POST /api/auth/login                           │
     │  { username, password }                         │
     ├────────────────────────────────────────────────>│
     │                                                  │
     │                                        ┌─────────▼────────┐
     │                                        │ AuthController   │
     │                                        └─────────┬────────┘
     │                                                  │
     │                                        ┌─────────▼────────┐
     │                                        │  AuthService     │
     │                                        │ • Hash password  │
     │                                        │ • Query DB       │
     │                                        │ • Generate JWT   │
     │                                        └─────────┬────────┘
     │                                                  │
     │  200 OK                                         │
     │  { token: "eyJhbGc..." }                        │
     │<────────────────────────────────────────────────┤
     │                                                  │
     │  GET /api/orders                                │
     │  Header: Authorization: Bearer eyJhbG...        │
     ├────────────────────────────────────────────────>│
     │                                                  │
     │                                        ┌─────────▼────────┐
     │                                        │ JWT Middleware   │
     │                                        │ • Validate token │
     │                                        │ • Extract claims │
     │                                        │ • Set User.Id    │
     │                                        └─────────┬────────┘
     │                                                  │
     │  200 OK                                         │
     │  { orders: [...] }                              │
     │<────────────────────────────────────────────────┤
```

### 2. Permission-Based Authorization

**Roles:**
- `ADMIN`: Full access
- `STAFF`: Product/Order management
- `CUSTOMER`: Own orders/profile only

**Permission Format:** `{module}.{action}[.scope]`

Examples:
- `product.view` - Anyone can view products
- `order.update.own` - Update own orders
- `order.update.all` - Staff can update any order

**Implementation:**
```csharp
[Authorize]
[RequirePermission("order.update.all")]
public async Task<IActionResult> UpdateOrder(int id, UpdateOrderRequest request)
{
    // Only ADMIN/STAFF with permission can access
}
```

---

## 📊 Data Flow Patterns

### 1. CQRS-lite (Command Query Responsibility Segregation)

Tách biệt **read** và **write** operations:

**Query (Read):**
```csharp
// Optimized for reading
public async Task<OrderResponse?> GetByIdAsync(int id)
{
    return await _context.Orders
        .AsNoTracking() // Read-only, no change tracking
        .Include(o => o.OrderItems)
        .Include(o => o.User)
        .Select(o => new OrderResponse { ... }) // Project to DTO
        .FirstOrDefaultAsync(o => o.Id == id);
}
```

**Command (Write):**
```csharp
// Handles state changes, transactions
public async Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request)
{
    using var transaction = await _context.Database.BeginTransactionAsync();
    try
    {
        var order = new Order { ... };
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        return MapToResponse(order);
    }
    catch { await transaction.RollbackAsync(); throw; }
}
```

---

### 2. Event Sourcing (Simplified)

**Order Status Transitions** được track qua status field:

```
Draft → Pending → Confirmed → Paid → Completed
                     ↓
                 Cancelled
```

Mỗi transition có business rules:
- `Draft → Pending`: Phải có items, địa chỉ
- `Pending → Confirmed`: Chỉ STAFF mới confirm được
- `Paid → Completed`: Auto sau khi giao hàng
- `Cancel`: Chỉ trước khi Paid

---

## 🗄️ Database Design Principles

### 1. Normalization

Database được thiết kế theo **3NF (Third Normal Form)**:
- ✅ Không có duplicate data
- ✅ Relationships qua Foreign Keys
- ✅ Lookup tables (Categories, Roles, Permissions)

### 2. Soft Delete

Không xóa vật lý, chỉ đánh dấu `IsActive = false`:
```csharp
public async Task<bool> DeleteProductAsync(int id)
{
    var product = await _context.Products.FindAsync(id);
    product.IsActive = false; // Soft delete
    await _context.SaveChangesAsync();
    return true;
}
```

### 3. Audit Fields

Tất cả entities có:
- `CreatedAt`: Timestamp tạo
- `UpdatedAt`: Timestamp cập nhật cuối
- `IsActive`: Soft delete flag

### 4. Address Snapshotting

**Problem:** User có thể thay đổi/xóa địa chỉ sau khi đặt hàng.

**Solution:** Snapshot địa chỉ vào Order:
```csharp
public class Order
{
    public int? UserAddressId { get; set; } // Reference (nullable)
    
    // Snapshot fields (immutable)
    public string RecipientName { get; set; }
    public string ShippingAddress { get; set; }
    public string PhoneNumber { get; set; }
}
```

---

## 🚀 Performance Optimization

### 1. Eager Loading

Load related data cùng lúc để tránh N+1 queries:
```csharp
var orders = await _context.Orders
    .Include(o => o.OrderItems)
        .ThenInclude(oi => oi.Product)
    .Include(o => o.User)
    .ToListAsync();
```

### 2. AsNoTracking

Tắt change tracking cho read-only queries:
```csharp
var products = await _context.Products
    .AsNoTracking() // 30-40% faster
    .ToListAsync();
```

### 3. Pagination

Luôn dùng pagination cho list endpoints:
```csharp
var query = _context.Products.AsQueryable();
var total = await query.CountAsync();
var items = await query
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync();
```

### 4. Indexes

Database có indexes trên:
- Foreign keys
- Frequently queried columns (Email, Username)
- Composite indexes cho complex queries

---

## 📦 Dependency Injection

### Service Registration

```csharp
// Program.cs
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
// ...
```

**Lifetimes:**
- `Scoped`: Mỗi HTTP request một instance (DbContext, Services, Repositories)
- `Transient`: Mỗi lần inject một instance mới
- `Singleton`: Một instance duy nhất cho toàn app (Configuration, Logger)

---

## 🧪 Testing Strategy

### 1. Unit Tests
- Test business logic trong Services
- Mock repositories

### 2. Integration Tests
- Test end-to-end flow
- Use in-memory database

### 3. API Tests
- Use Postman/xUnit
- Test authentication, validation, error handling

---

## 📖 Next Steps

- 🔐 [Authentication Module](./AUTH_MODULE.md)
- 📦 [Product Module](./PRODUCT_MODULE.md)
- 📋 [Order Module](./ORDER_MODULE.md)
- 🗄️ [Database Schema](./DATABASE.md)
