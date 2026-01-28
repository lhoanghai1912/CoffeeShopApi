# CoffeeShopApi - .NET 8 Web API

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)
[![Status](https://img.shields.io/badge/status-Active-success)]()

Hệ thống quản lý quán cà phê với đầy đủ tính năng: Quản lý sản phẩm, đơn hàng, khách hàng, voucher và xác thực người dùng.

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

2. **Update `appsettings.json`**
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

5. **Open Swagger**
```
https://localhost:5001/swagger
```

---

## 📚 Documentation

### 📖 Core Documentation
- **[Master Documentation](./docs/README.md)** - Start here
- **[Architecture](./docs/ARCHITECTURE.md)** - System design & patterns
- **[Database Schema](./docs/DATABASE.md)** - ERD & tables

### 🔐 API Modules
- **[Authentication Module](./docs/AUTH_MODULE.md)** - Login, Register, JWT
- **[User Module](./docs/USER_MODULE.md)** - Profile, Addresses
- **[Product Module](./docs/PRODUCT_MODULE.md)** - Products, Categories, Options
- **[Order Module](./docs/ORDER_MODULE.md)** - Order Lifecycle & Checkout
- **[Voucher Module](./docs/VOUCHER_MODULE.md)** - Discount codes

### 🛠️ Operations
- **[Deployment Guide](./docs/DEPLOYMENT.md)** - Setup & Configuration
- **[Testing Guide](./docs/TESTING.md)** - Unit & Integration tests

---

## 🎯 Key Features

### 🔐 Authentication & Authorization
- JWT Bearer Token authentication
- Permission-based authorization
- Email verification
- Password reset via email
- BCrypt password hashing

### 📦 Product Management
- Products with multiple option groups (Size, Sugar, Toppings)
- Image upload
- Category filtering
- Full-text search & pagination
- Dynamic pricing

### 📋 Order Management
- Draft order system
- Address snapshotting
- Voucher integration
- Status workflow (Draft → Pending → Confirmed → Paid → Completed)
- Price validation before checkout

### 🎟️ Voucher System
- **Public vouchers** - Code-based, anyone can use
- **Private vouchers** - Assigned to specific users
- Fixed amount & percentage discounts
- Usage limits & constraints
- Automatic apply/rollback

### 👤 User Management
- Profile management
- Multiple delivery addresses
- Order history
- Password change

---

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────┐
│                    Presentation Layer                    │
│                     (Controllers)                        │
└─────────────────────────────────────────────────────────┘
                           ↓
┌─────────────────────────────────────────────────────────┐
│                   Business Logic Layer                   │
│                      (Services)                          │
└─────────────────────────────────────────────────────────┘
                           ↓
┌─────────────────────────────────────────────────────────┐
│                   Data Access Layer                      │
│                    (Repositories)                        │
└─────────────────────────────────────────────────────────┘
                           ↓
┌─────────────────────────────────────────────────────────┐
│                     SQL Server Database                  │
└─────────────────────────────────────────────────────────┘
```

**Design Patterns:**
- Repository Pattern
- Service Layer Pattern
- DTO Pattern
- Unit of Work (via DbContext)
- CQRS-lite

---

## 🛠️ Tech Stack

| Technology | Version | Purpose |
|------------|---------|---------|
| .NET | 8.0 | Framework |
| C# | 12.0 | Language |
| ASP.NET Core Web API | 8.0 | API Framework |
| Entity Framework Core | 9.0 | ORM |
| SQL Server | 2019+ | Database |
| JWT | - | Authentication |
| BCrypt.Net | 0.1.0 | Password Hashing |
| Swagger/OpenAPI | - | API Documentation |
| Gridify | 2.14.7 | Query/Filter/Sort |

---

## 📦 API Response Format

All endpoints return standardized `ApiResponse<T>`:

**Success:**
```json
{
  "success": true,
  "message": "Thành công",
  "status": 200,
  "data": { ... },
  "errors": null
}
```

**Error:**
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

## 🧪 Sample Accounts

| Username | Password | Role |
|----------|----------|------|
| admin | Admin@123 | ADMIN |
| staff01 | Staff@123 | STAFF |
| customer01 | Customer@123 | CUSTOMER |

---

## 📊 Database Statistics

- **Tables:** 15+
- **Sample Products:** 30 (Coffee, Tea, Food, Freeze)
- **Sample Vouchers:** 35 (20 public + 15 private)
- **Sample Users:** Seeded with addresses

---

## 🔗 API Endpoints

### Authentication
- `POST /api/auth/login` - User login
- `POST /api/auth/register` - User registration
- `POST /api/auth/forgot-password` - Request password reset
- `POST /api/auth/reset-password` - Reset password

### Products
- `GET /api/products/paged` - List products (paginated)
- `GET /api/products/{id}` - Get product details
- `POST /api/products` - Create product (Admin)
- `PUT /api/products/{id}` - Update product (Admin)
- `DELETE /api/products/{id}` - Delete product (Admin)

### Orders
- `POST /api/orders` - Create draft order
- `POST /api/orders/{id}/items` - Add item to order
- `POST /api/orders/{id}/checkout` - Checkout order
- `POST /api/orders/{id}/confirm` - Confirm order (Staff)
- `POST /api/orders/{id}/pay` - Mark as paid (Staff)
- `POST /api/orders/{id}/cancel` - Cancel order

### Vouchers
- `POST /api/vouchers/validate` - Validate voucher
- `GET /api/vouchers/active` - List active public vouchers
- `GET /api/vouchers/my-vouchers` - List user's private vouchers
- `POST /api/vouchers` - Create voucher (Admin)
- `POST /api/vouchers/assign` - Assign voucher to users (Admin)

### Users
- `GET /api/users/profile` - Get current user profile
- `PUT /api/users/profile` - Update profile
- `POST /api/users/change-password` - Change password
- `GET /api/users/addresses` - List user addresses
- `POST /api/users/addresses` - Add new address

**Full API documentation:** [Swagger UI](https://localhost:5001/swagger)

---

## 🐛 Common Issues

### Database Connection Error
```
Make sure SQL Server is running and connection string is correct in appsettings.json
```

### Migration Error
```bash
# Drop database and recreate
dotnet ef database drop
dotnet ef database update
```

### Port Already in Use
```json
// Change port in launchSettings.json
"applicationUrl": "https://localhost:5002;http://localhost:5003"
```

---

## 🤝 Contributing

1. Fork the project
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

---

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 👤 Author

**Lê Hoàng Hải**
- GitHub: [@lhoanghai1912](https://github.com/lhoanghai1912)
- Email: lhoanghai1912@example.com

---

## 🙏 Acknowledgments

- ASP.NET Core Team
- Entity Framework Core Team
- .NET Community

---

## 📈 Project Status

✅ **Active Development** - Regular updates & maintenance

**Latest Version:** 1.0.0  
**Last Updated:** January 28, 2025

---

## 📞 Support

- 📖 [Documentation](./docs/README.md)
- 🐛 [Issue Tracker](https://github.com/lhoanghai1912/CoffeeShopApi/issues)
- 💬 [Discussions](https://github.com/lhoanghai1912/CoffeeShopApi/discussions)

---

**⭐ If you find this project helpful, please give it a star!**
