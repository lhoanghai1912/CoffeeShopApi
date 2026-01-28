# CoffeeShopApi Documentation

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/)
[![Documentation](https://img.shields.io/badge/docs-complete-success)](https://github.com/lhoanghai1912/CoffeeShopApi/tree/master/docs)
[![API](https://img.shields.io/badge/API-49%20endpoints-blue)]()

> 📚 **Complete documentation for CoffeeShopApi** - Professional .NET 8 Web API for Coffee Shop management system

---

## 🚀 Quick Start

**New to the project?** Start here:

1. **[API Reference](./API_REFERENCE.md)** ⭐ - All 49 endpoints in one place
2. **[Architecture](./ARCHITECTURE.md)** - System design & patterns
3. **[Database Schema](./DATABASE.md)** - Complete ERD with 15 tables
4. **[Deployment Guide](./DEPLOYMENT.md)** - Deploy to production

**Complete navigation:** See [INDEX.md](./INDEX.md) for full documentation map

---

## 📚 Documentation Structure

### 🚀 Quick Start
- **[API Reference](./API_REFERENCE.md)** - ⭐ Quick API endpoint reference (START HERE!)
- **[Git Commit Guide](./GIT_COMMIT_GUIDE.md)** - How to commit & push changes

### 🏗️ Core Documentation
- **[Architecture](./ARCHITECTURE.md)** - System design, data flow, design patterns
- **[Database Schema](./DATABASE.md)** - Complete ERD, tables, relationships, indexes

### 📡 API Modules (Detailed)
- **[Authentication Module](./AUTH_MODULE.md)** - Login, Register, JWT, Password Reset
- **[User Module](./USER_MODULE.md)** - Profile, Addresses, Password Management
- **[Product Module](./PRODUCT_MODULE.md)** - Products, Options (Size/Sugar/Topping), Categories
- **[Order Module](./ORDER_MODULE.md)** - Order Lifecycle (Draft → Completed), Checkout
- **[Voucher Module](./VOUCHER_MODULE.md)** - Public/Private Vouchers, Discount Logic
- **[Category Module](./CATEGORY_MODULE.md)** - Category CRUD, Product Filtering

### 🚢 Deployment & Operations
- **[Deployment Guide](./DEPLOYMENT.md)** - IIS, Docker, Azure, CI/CD
- **[Changelog](./CHANGELOG.md)** - All changes and updates

---

## 🚀 Quick Start

### Prerequisites
- .NET 8 SDK
- SQL Server (LocalDB hoặc full instance)
- Visual Studio 2022 hoặc VS Code

### Installation

1. **Clone repository**
```bash
git clone https://github.com/lhoanghai1912/CoffeeShopApi.git
cd CoffeeShopApi
```

2. **Update connection string** trong `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=CoffeeShopDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

3. **Run migrations**
```bash
cd CoffeeShopApi
dotnet ef database update
```

4. **Run application**
```bash
dotnet run
```

5. **Open Swagger UI**
```
https://localhost:5001/swagger
```

---

## 📁 Project Structure

```
CoffeeShopApi/
├── Controllers/          # HTTP endpoints (API layer)
├── Services/             # Business logic
├── Repositories/         # Data access layer
├── Data/                 # DbContext, Configurations, Seeders
├── Models/               # Entity classes
├── DTOs/                 # Data Transfer Objects
├── Shared/               # Utilities, helpers
├── Authorization/        # Custom authorization
├── Migrations/           # EF Core migrations
├── wwwroot/              # Static files
└── docs/                 # Documentation (you are here)
```

---

## 🔐 Authentication

Hệ thống sử dụng **JWT Bearer Token** authentication:

1. Login qua `/api/auth/login` để nhận token
2. Gửi token trong header: `Authorization: Bearer {token}`
3. Token có thời hạn 7 ngày

**Example:**
```bash
# Login
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"Admin@123"}'

# Use token
curl -X GET https://localhost:5001/api/users/profile \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

---

## 📦 API Response Format

Tất cả endpoints trả về chuẩn `ApiResponse<T>`:

**Success Response:**
```json
{
  "success": true,
  "message": "Thành công",
  "status": 200,
  "data": { ... },
  "errors": null
}
```

**Error Response:**
```json
{
  "success": false,
  "message": "Validation Error",
  "status": 400,
  "data": null,
  "errors": ["Tên sản phẩm không được để trống"]
}
```

---

## 🧪 Testing

### Sample Accounts

| Username | Password | Role |
|----------|----------|------|
| admin | Admin@123 | ADMIN |
| staff01 | Staff@123 | STAFF |
| customer01 | Customer@123 | CUSTOMER |

### Postman Collection

Import file `CoffeeShopApi.postman_collection.json` vào Postman để test nhanh các API.

---

## 🛠️ Tech Stack

- **Framework:** .NET 8 Web API
- **ORM:** Entity Framework Core 9
- **Database:** SQL Server
- **Authentication:** JWT Bearer Tokens
- **Password Hashing:** BCrypt.Net
- **Email:** SMTP (configurable)
- **API Documentation:** Swagger/OpenAPI
- **Dependency Injection:** Built-in .NET DI Container

---

## 📖 Key Features

### 1. Authentication & Authorization
- JWT-based authentication
- Permission-based authorization
- Email verification
- Password reset via email

### 2. Product Management
- Products with multiple option groups (Size, Sugar, Toppings)
- Image upload
- Category filtering
- Full-text search

### 3. Order Management
- Draft order system
- Address snapshotting
- Voucher integration
- Status workflow (Draft → Pending → Confirmed → Paid → Completed)

### 4. Voucher System
- Public vouchers (code-based)
- Private vouchers (user-assigned)
- Fixed amount & percentage discounts
- Usage limits & constraints

### 5. User Management
- Profile management
- Multiple delivery addresses
- Order history
- Password change

---

## 🔗 Related Links

- [GitHub Repository](https://github.com/lhoanghai1912/CoffeeShopApi)
- [API Documentation (Swagger)](https://localhost:5001/swagger)
- [Issue Tracker](https://github.com/lhoanghai1912/CoffeeShopApi/issues)

---

## 📝 License

This project is licensed under the MIT License.

---

## 👥 Contributors

- Lê Hoàng Hải (@lhoanghai1912)

---

**Next Steps:**
- 📖 Read [Architecture Documentation](./ARCHITECTURE.md)
- 🔐 Learn about [Authentication Module](./AUTH_MODULE.md)
- 📦 Explore [Product Module](./PRODUCT_MODULE.md)
