# Database Schema Documentation

## 🗄️ Overview

CoffeeShopApi sử dụng **SQL Server** với **Entity Framework Core 9** làm ORM.

- **Database Type:** SQL Server 2019+
- **ORM:** Entity Framework Core 9.0
- **Migrations:** Code-First approach
- **Text Encoding:** NVARCHAR (Unicode support)

---

## 📊 Entity Relationship Diagram (ERD)

```
┌─────────────────┐
│     Roles       │
│─────────────────│
│ PK Id           │
│    Code         │
│    Name         │
└─────────────────┘
         │ 1
         │
         │ N
┌─────────────────┐         ┌──────────────────┐
│   Permissions   │    N:N  │ RolePermissions  │
│─────────────────│◄────────┤──────────────────│
│ PK Id           │         │ PK Id            │
│    Code         │         │ FK RoleId        │
│    Name         │         │ FK PermissionId  │
│    Module       │         └──────────────────┘
└─────────────────┘                │
                                   │ N
                                   │
                                   │ 1
         ┌─────────────────────────┘
         │
         │
┌─────────────────┐      1:N      ┌──────────────────┐
│     Users       │◄───────────────┤  UserAddresses   │
│─────────────────│                │──────────────────│
│ PK Id           │                │ PK Id            │
│ FK RoleId       │                │ FK UserId        │
│    Username     │                │    RecipientName │
│    Password     │                │    PhoneNumber   │
│    Email        │                │    AddressLine   │
│    FullName     │                │    Label         │
│    PhoneNumber  │                │    IsDefault     │
│    IsActive     │                └──────────────────┘
└─────────────────┘
         │ 1
         │
         ├────────────────┐
         │                │
         │ N              │ N
┌─────────────────┐  ┌──────────────────┐
│     Orders      │  │  VoucherUsages   │
│─────────────────│  │──────────────────│
│ PK Id           │  │ PK Id            │
│ FK UserId       │  │ FK UserId        │
│ FK VoucherId    │  │ FK VoucherId     │
│    OrderCode    │  │ FK OrderId       │
│    Status       │  │    UsedAt        │
│    SubTotal     │  └──────────────────┘
│    Discount     │
│    ShippingFee  │         ┌──────────────────┐
│    FinalAmount  │    N:N  │  UserVouchers    │
│    RecipientName│◄────────┤──────────────────│
│    ShippingAddr │         │ PK Id            │
│    PhoneNumber  │         │ FK UserId        │
└─────────────────┘         │ FK VoucherId     │
         │ 1                │    IsUsed        │
         │                  │    AssignedAt    │
         │ N                │    UsedAt        │
┌─────────────────┐         └──────────────────┘
│   OrderItems    │                │
│─────────────────│                │
│ PK Id           │                │ N
│ FK OrderId      │                │
│ FK ProductId    │                │ 1
│    ProductName  │         ┌──────────────────┐
│    Quantity     │         │    Vouchers      │
│    UnitPrice    │         │──────────────────│
│    TotalPrice   │         │ PK Id            │
└─────────────────┘         │    Code          │
         │ 1                │    DiscountType  │
         │                  │    DiscountValue │
         │ N                │    MinOrderValue │
┌──────────────────┐        │    UsageLimit    │
│OrderItemOptions  │        │    IsPublic      │
│──────────────────│        │    IsActive      │
│ PK Id            │        └──────────────────┘
│ FK OrderItemId   │
│ FK OptionItemId  │
│    OptionGroup   │
│    OptionItem    │
│    PriceAdjust   │
└──────────────────┘
         │ N
         │
         │ 1
┌─────────────────┐
│  OptionItems    │
│─────────────────│
│ PK Id           │
│ FK OptionGroupId│
│    Name         │
│    PriceAdjust  │
│    IsDefault    │
│    DisplayOrder │
└─────────────────┘
         │ N
         │
         │ 1
┌─────────────────┐      1:N      ┌──────────────────┐
│  OptionGroups   │◄───────────────┤    Products      │
│─────────────────│                │──────────────────│
│ PK Id           │                │ PK Id            │
│ FK ProductId    │                │ FK CategoryId    │
│    Name         │                │    Name          │
│    IsRequired   │                │    Description   │
│    AllowMultiple│                │    BasePrice     │
│    DisplayOrder │                │    ImageUrl      │
└─────────────────┘                │    IsActive      │
                                   └──────────────────┘
                                            │ N
                                            │
                                            │ 1
                                   ┌──────────────────┐
                                   │   Categories     │
                                   │──────────────────│
                                   │ PK Id            │
                                   │    Name          │
                                   └──────────────────┘
```

---

## 📋 Table Details

### 1. Users Table

**Purpose:** Lưu thông tin tài khoản người dùng

```sql
CREATE TABLE Users (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50) NOT NULL,
    Password NVARCHAR(255) NOT NULL,  -- BCrypt hashed
    Email NVARCHAR(100),
    FullName NVARCHAR(100) NOT NULL,
    PhoneNumber NVARCHAR(20),
    RoleId INT NOT NULL,
    
    -- Email Verification
    IsEmailVerified BIT NOT NULL DEFAULT 0,
    EmailVerificationCode NVARCHAR(10),
    EmailVerificationCodeExpiry DATETIME2,
    
    -- Password Reset
    PasswordResetToken NVARCHAR(10),
    PasswordResetTokenExpiry DATETIME2,
    
    -- Status
    IsActive BIT NOT NULL DEFAULT 1,
    
    -- Audit
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    LastLoginAt DATETIME2,
    
    CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleId) REFERENCES Roles(Id)
);

-- Indexes
CREATE UNIQUE INDEX IX_Users_Username ON Users(Username);
CREATE UNIQUE INDEX IX_Users_Email ON Users(Email) WHERE Email IS NOT NULL;
CREATE INDEX IX_Users_RoleId ON Users(RoleId);
CREATE INDEX IX_Users_IsActive ON Users(IsActive);
CREATE INDEX IX_Users_IsEmailVerified ON Users(IsEmailVerified);
```

**Columns:**
- `Password`: BCrypt hash (60 characters)
- `IsEmailVerified`: Bắt buộc true để login
- `IsActive`: Soft delete flag
- `EmailVerificationCode`: 6-digit code, expires after 15 minutes
- `PasswordResetToken`: 6-digit code, expires after 15 minutes

---

### 2. Roles Table

**Purpose:** Định nghĩa các vai trò hệ thống

```sql
CREATE TABLE Roles (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Code NVARCHAR(50) UNIQUE NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NOT NULL
);

-- Seed Data
INSERT INTO Roles (Id, Code, Name, CreatedAt, UpdatedAt) VALUES
(1, 'ADMIN', 'Admin', GETUTCDATE(), GETUTCDATE()),
(2, 'CUSTOMER', 'Khách hàng', GETUTCDATE(), GETUTCDATE()),
(3, 'STAFF', 'Nhân viên', GETUTCDATE(), GETUTCDATE());
```

**Seeded Roles:**
- `ADMIN`: Full access
- `CUSTOMER`: Own orders/profile
- `STAFF`: Manage products/orders

---

### 3. Permissions Table

**Purpose:** Định nghĩa các quyền chi tiết

```sql
CREATE TABLE Permissions (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Code NVARCHAR(100) UNIQUE NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Module NVARCHAR(50) NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NOT NULL
);

CREATE INDEX IX_Permissions_Module ON Permissions(Module);
```

**Permission Format:** `{module}.{action}[.scope]`

**Examples:**
- `product.view`
- `order.view.own`
- `order.update.all`
- `user.delete`

---

### 4. RolePermissions Table

**Purpose:** Many-to-Many mapping giữa Roles và Permissions

```sql
CREATE TABLE RolePermissions (
    Id INT PRIMARY KEY IDENTITY(1,1),
    RoleId INT NOT NULL,
    PermissionId INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    
    CONSTRAINT FK_RolePermissions_Roles FOREIGN KEY (RoleId) REFERENCES Roles(Id) ON DELETE CASCADE,
    CONSTRAINT FK_RolePermissions_Permissions FOREIGN KEY (PermissionId) REFERENCES Permissions(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_RolePermissions_Role_Permission UNIQUE (RoleId, PermissionId)
);

CREATE INDEX IX_RolePermissions_RoleId ON RolePermissions(RoleId);
CREATE INDEX IX_RolePermissions_PermissionId ON RolePermissions(PermissionId);
```

---

### 5. UserAddresses Table

**Purpose:** Lưu địa chỉ giao hàng của user

```sql
CREATE TABLE UserAddresses (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    RecipientName NVARCHAR(100) NOT NULL,
    PhoneNumber NVARCHAR(20) NOT NULL,
    AddressLine NVARCHAR(500) NOT NULL,
    Label NVARCHAR(50),  -- 'Nhà riêng', 'Văn phòng', etc.
    IsDefault BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NOT NULL,
    
    CONSTRAINT FK_UserAddresses_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

CREATE INDEX IX_UserAddresses_UserId ON UserAddresses(UserId);
CREATE INDEX IX_UserAddresses_UserId_IsDefault ON UserAddresses(UserId, IsDefault);
```

**Business Rules:**
- Each user can have multiple addresses
- Only ONE address can be `IsDefault = true` per user
- First address is automatically default

---

### 6. Categories Table

**Purpose:** Danh mục sản phẩm

```sql
CREATE TABLE Categories (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) UNIQUE NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NOT NULL
);

CREATE UNIQUE INDEX IX_Categories_Name ON Categories(Name);

-- Seed Data
INSERT INTO Categories (Id, Name, CreatedAt, UpdatedAt) VALUES
(1, 'Coffee', GETUTCDATE(), GETUTCDATE()),
(2, 'Tea', GETUTCDATE(), GETUTCDATE()),
(3, 'Food', GETUTCDATE(), GETUTCDATE()),
(4, 'Freeze', GETUTCDATE(), GETUTCDATE());
```

---

### 7. Products Table

**Purpose:** Thông tin sản phẩm

```sql
CREATE TABLE Products (
    Id INT PRIMARY KEY IDENTITY(1,1),
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

CREATE INDEX IX_Products_CategoryId ON Products(CategoryId);
CREATE INDEX IX_Products_IsActive ON Products(IsActive);
CREATE INDEX IX_Products_Name ON Products(Name);
CREATE INDEX IX_Products_BasePrice ON Products(BasePrice);
```

**Columns:**
- `BasePrice`: Giá gốc chưa bao gồm options
- `ImageUrl`: Relative path (e.g., `/images/caphedenda.jpg`)
- `IsActive`: Soft delete flag

---

### 8. OptionGroups Table

**Purpose:** Nhóm tùy chọn (Size, Đường, Topping)

```sql
CREATE TABLE OptionGroups (
    Id INT PRIMARY KEY IDENTITY(1,1),
    ProductId INT NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    IsRequired BIT NOT NULL DEFAULT 0,
    AllowMultiple BIT NOT NULL DEFAULT 0,
    DisplayOrder INT NOT NULL DEFAULT 0,
    
    CONSTRAINT FK_OptionGroups_Products FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE
);

CREATE INDEX IX_OptionGroups_ProductId ON OptionGroups(ProductId);
CREATE INDEX IX_OptionGroups_ProductId_DisplayOrder ON OptionGroups(ProductId, DisplayOrder);
```

**Columns:**
- `IsRequired`: User phải chọn (e.g., Size)
- `AllowMultiple`: Cho phép chọn nhiều (e.g., Topping)
- `DisplayOrder`: Thứ tự hiển thị trên UI

---

### 9. OptionItems Table

**Purpose:** Các lựa chọn cụ thể trong nhóm

```sql
CREATE TABLE OptionItems (
    Id INT PRIMARY KEY IDENTITY(1,1),
    OptionGroupId INT NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    PriceAdjustment DECIMAL(18,2) NOT NULL DEFAULT 0,
    IsDefault BIT NOT NULL DEFAULT 0,
    DisplayOrder INT NOT NULL DEFAULT 0,
    
    CONSTRAINT FK_OptionItems_OptionGroups FOREIGN KEY (OptionGroupId) REFERENCES OptionGroups(Id) ON DELETE CASCADE
);

CREATE INDEX IX_OptionItems_OptionGroupId ON OptionItems(OptionGroupId);
CREATE INDEX IX_OptionItems_OptionGroupId_DisplayOrder ON OptionItems(OptionGroupId, DisplayOrder);
```

**Columns:**
- `PriceAdjustment`: Thêm/bớt giá (e.g., +10000 cho Size L)
- `IsDefault`: Tự động chọn khi load product
- Mỗi OptionGroup chỉ có tối đa 1 IsDefault = true

---

### 10. Orders Table

**Purpose:** Đơn hàng

```sql
CREATE TABLE Orders (
    Id INT PRIMARY KEY IDENTITY(1,1),
    OrderCode NVARCHAR(50) UNIQUE NOT NULL,  -- ORD-20250128-00001
    UserId INT,
    Status INT NOT NULL,  -- 0=Draft, 1=Pending, 2=Confirmed, 3=Paid, 4=Completed, 5=Cancelled
    
    -- Address Snapshot (frozen at checkout)
    RecipientName NVARCHAR(100),
    ShippingAddress NVARCHAR(500),
    PhoneNumber NVARCHAR(20),
    
    -- Pricing
    SubTotal DECIMAL(18,2) NOT NULL DEFAULT 0,
    DiscountAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    ShippingFee DECIMAL(18,2) NOT NULL DEFAULT 0,
    FinalAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    
    -- Voucher
    VoucherId INT,
    
    -- Notes & Timestamps
    Note NVARCHAR(500),
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NOT NULL,
    PaidAt DATETIME2,
    CancelledAt DATETIME2,
    CancelReason NVARCHAR(500),
    
    CONSTRAINT FK_Orders_Users FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT FK_Orders_Vouchers FOREIGN KEY (VoucherId) REFERENCES Vouchers(Id) ON DELETE SET NULL
);

CREATE INDEX IX_Orders_UserId ON Orders(UserId);
CREATE INDEX IX_Orders_Status ON Orders(Status);
CREATE UNIQUE INDEX IX_Orders_OrderCode ON Orders(OrderCode);
CREATE INDEX IX_Orders_CreatedAt ON Orders(CreatedAt DESC);
CREATE INDEX IX_Orders_VoucherId ON Orders(VoucherId);
```

**Status Enum:**
```csharp
public enum OrderStatus
{
    Draft = 0,
    Pending = 1,
    Confirmed = 2,
    Paid = 3,
    Completed = 4,
    Cancelled = 5
}
```

**Address Snapshot:**
- Copy từ UserAddress at checkout
- Không dùng FK để tránh bị ảnh hưởng khi user update/delete address

---

### 11. OrderItems Table

**Purpose:** Chi tiết sản phẩm trong đơn hàng

```sql
CREATE TABLE OrderItems (
    Id INT PRIMARY KEY IDENTITY(1,1),
    OrderId INT NOT NULL,
    
    -- Product Snapshot
    ProductId INT NOT NULL,
    ProductName NVARCHAR(200) NOT NULL,
    ProductImageUrl NVARCHAR(500),
    ProductBasePrice DECIMAL(18,2) NOT NULL,
    
    -- Order specific
    Quantity INT NOT NULL DEFAULT 1,
    UnitPrice DECIMAL(18,2) NOT NULL,  -- BasePrice + Options
    TotalPrice DECIMAL(18,2) NOT NULL,  -- UnitPrice * Quantity
    Note NVARCHAR(500),
    
    CONSTRAINT FK_OrderItems_Orders FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE CASCADE,
    CONSTRAINT FK_OrderItems_Products FOREIGN KEY (ProductId) REFERENCES Products(Id)
);

CREATE INDEX IX_OrderItems_OrderId ON OrderItems(OrderId);
CREATE INDEX IX_OrderItems_ProductId ON OrderItems(ProductId);
```

**Product Snapshot:**
- Lưu thông tin sản phẩm tại thời điểm đặt hàng
- Tránh bị ảnh hưởng khi product thay đổi giá

---

### 12. OrderItemOptions Table

**Purpose:** Lưu options đã chọn cho mỗi OrderItem

```sql
CREATE TABLE OrderItemOptions (
    Id INT PRIMARY KEY IDENTITY(1,1),
    OrderItemId INT NOT NULL,
    
    -- Option Snapshot
    OptionItemId INT NOT NULL,
    OptionGroupName NVARCHAR(100) NOT NULL,
    OptionItemName NVARCHAR(100) NOT NULL,
    PriceAdjustment DECIMAL(18,2) NOT NULL,
    
    CONSTRAINT FK_OrderItemOptions_OrderItems FOREIGN KEY (OrderItemId) REFERENCES OrderItems(Id) ON DELETE CASCADE,
    CONSTRAINT FK_OrderItemOptions_OptionItems FOREIGN KEY (OptionItemId) REFERENCES OptionItems(Id)
);

CREATE INDEX IX_OrderItemOptions_OrderItemId ON OrderItemOptions(OrderItemId);
CREATE INDEX IX_OrderItemOptions_OptionItemId ON OrderItemOptions(OptionItemId);
```

---

### 13. Vouchers Table

**Purpose:** Mã giảm giá

```sql
CREATE TABLE Vouchers (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Code NVARCHAR(50) UNIQUE NOT NULL,
    Description NVARCHAR(500),
    
    -- Discount Settings
    DiscountType INT NOT NULL,  -- 0=FixedAmount, 1=Percentage
    DiscountValue DECIMAL(18,2) NOT NULL,
    MinOrderValue DECIMAL(18,2),
    MaxDiscountAmount DECIMAL(18,2),
    
    -- Time Constraints
    StartDate DATETIME2 NOT NULL,
    EndDate DATETIME2 NOT NULL,
    
    -- Usage Limits
    UsageLimit INT,  -- NULL = unlimited
    UsageLimitPerUser INT,  -- NULL = unlimited
    CurrentUsageCount INT NOT NULL DEFAULT 0,
    
    -- Type
    IsPublic BIT NOT NULL DEFAULT 1,  -- 1=Public, 0=Private
    
    -- Status
    IsActive BIT NOT NULL DEFAULT 1,
    
    -- Audit
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NOT NULL
);

CREATE UNIQUE INDEX IX_Vouchers_Code ON Vouchers(Code);
CREATE INDEX IX_Vouchers_IsPublic_IsActive ON Vouchers(IsPublic, IsActive);
CREATE INDEX IX_Vouchers_EndDate ON Vouchers(EndDate);
```

**Discount Types:**
```csharp
public enum DiscountType
{
    FixedAmount = 0,  // e.g., -10,000đ
    Percentage = 1    // e.g., -20%
}
```

---

### 14. UserVouchers Table

**Purpose:** Gán private voucher cho user

```sql
CREATE TABLE UserVouchers (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    VoucherId INT NOT NULL,
    IsUsed BIT NOT NULL DEFAULT 0,
    AssignedAt DATETIME2 NOT NULL,
    UsedAt DATETIME2,
    Note NVARCHAR(200),
    
    CONSTRAINT FK_UserVouchers_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT FK_UserVouchers_Vouchers FOREIGN KEY (VoucherId) REFERENCES Vouchers(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_UserVouchers_User_Voucher UNIQUE (UserId, VoucherId)
);

CREATE INDEX IX_UserVouchers_UserId ON UserVouchers(UserId);
CREATE INDEX IX_UserVouchers_VoucherId ON UserVouchers(VoucherId);
CREATE INDEX IX_UserVouchers_UserId_IsUsed ON UserVouchers(UserId, IsUsed);
```

---

### 15. VoucherUsages Table

**Purpose:** Lịch sử sử dụng public voucher

```sql
CREATE TABLE VoucherUsages (
    Id INT PRIMARY KEY IDENTITY(1,1),
    VoucherId INT NOT NULL,
    UserId INT,
    OrderId INT,
    UsedAt DATETIME2 NOT NULL,
    
    CONSTRAINT FK_VoucherUsages_Vouchers FOREIGN KEY (VoucherId) REFERENCES Vouchers(Id) ON DELETE CASCADE,
    CONSTRAINT FK_VoucherUsages_Users FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT FK_VoucherUsages_Orders FOREIGN KEY (OrderId) REFERENCES Orders(Id)
);

CREATE INDEX IX_VoucherUsages_VoucherId ON VoucherUsages(VoucherId);
CREATE INDEX IX_VoucherUsages_UserId ON VoucherUsages(UserId);
CREATE INDEX IX_VoucherUsages_OrderId ON VoucherUsages(OrderId);
CREATE INDEX IX_VoucherUsages_UsedAt ON VoucherUsages(UsedAt DESC);
```

---

## 🔗 Relationships Summary

| Relationship | Type | Description |
|--------------|------|-------------|
| Users ↔ Roles | Many-to-One | Each user has one role |
| Roles ↔ Permissions | Many-to-Many | Via RolePermissions |
| Users ↔ UserAddresses | One-to-Many | User can have multiple addresses |
| Users ↔ Orders | One-to-Many | User can have multiple orders |
| Categories ↔ Products | One-to-Many | Each product belongs to one category |
| Products ↔ OptionGroups | One-to-Many | Product has multiple option groups |
| OptionGroups ↔ OptionItems | One-to-Many | Group has multiple items |
| Orders ↔ OrderItems | One-to-Many | Order has multiple items |
| OrderItems ↔ OrderItemOptions | One-to-Many | Item has selected options |
| Vouchers ↔ Orders | One-to-Many | Voucher can be used in many orders |
| Users ↔ Vouchers | Many-to-Many | Via UserVouchers (private vouchers) |
| Vouchers ↔ VoucherUsages | One-to-Many | Track voucher usage history |

---

## 🔑 Indexing Strategy

### Primary Keys
- All tables use **INT IDENTITY(1,1)** for PKs
- Provides fast lookups and joins

### Unique Indexes
```sql
Users.Username          -- Login lookup
Users.Email             -- Unique email check
Orders.OrderCode        -- Quick order search
Vouchers.Code           -- Validate voucher
Categories.Name         -- Prevent duplicate categories
```

### Foreign Key Indexes
```sql
-- Automatically indexed by EF Core
Users.RoleId
Products.CategoryId
Orders.UserId
OrderItems.OrderId
etc.
```

### Composite Indexes
```sql
UserAddresses(UserId, IsDefault)  -- Find default address
OptionGroups(ProductId, DisplayOrder)  -- Display in order
Vouchers(IsPublic, IsActive)  -- Filter active public vouchers
```

### Performance Indexes
```sql
Orders.CreatedAt DESC  -- List recent orders
Orders.Status          -- Filter by status
VoucherUsages.UsedAt DESC  -- Recent usage
```

---

## 🛡️ Data Integrity

### Cascading Deletes

```sql
-- CASCADE DELETE
UserAddresses ON DELETE CASCADE  -- Delete user → delete addresses
OptionGroups ON DELETE CASCADE   -- Delete product → delete options
OrderItems ON DELETE CASCADE     -- Delete order → delete items
RolePermissions ON DELETE CASCADE

-- SET NULL
Orders.VoucherId ON DELETE SET NULL  -- Delete voucher → keep order but clear voucherId

-- NO ACTION (Prevent Delete)
Products.CategoryId  -- Cannot delete category with products
```

### Check Constraints

```csharp
// Application level validation
Products.BasePrice > 0
Vouchers.DiscountValue > 0
Vouchers.StartDate < EndDate
Orders.SubTotal >= 0
OrderItems.Quantity >= 1
```

---

## 📈 Statistics

### Table Sizes (Estimated)

| Table | Rows (Estimate) | Size |
|-------|-----------------|------|
| Users | 10,000 | ~5 MB |
| UserAddresses | 30,000 | ~10 MB |
| Products | 100 | ~500 KB |
| OptionGroups | 300 | ~100 KB |
| OptionItems | 1,500 | ~500 KB |
| Orders | 50,000 | ~50 MB |
| OrderItems | 150,000 | ~100 MB |
| OrderItemOptions | 300,000 | ~150 MB |
| Vouchers | 50 | ~50 KB |
| VoucherUsages | 100,000 | ~20 MB |

**Total:** ~335 MB (for production with 10K users)

---

## 🔧 Maintenance

### Regular Tasks

```sql
-- Rebuild indexes (monthly)
ALTER INDEX ALL ON Users REBUILD;
ALTER INDEX ALL ON Orders REBUILD;
ALTER INDEX ALL ON OrderItems REBUILD;

-- Update statistics (weekly)
UPDATE STATISTICS Users;
UPDATE STATISTICS Orders;
UPDATE STATISTICS OrderItems;

-- Clean up expired verification codes (daily)
UPDATE Users
SET EmailVerificationCode = NULL,
    EmailVerificationCodeExpiry = NULL
WHERE EmailVerificationCodeExpiry < GETUTCDATE();

UPDATE Users
SET PasswordResetToken = NULL,
    PasswordResetTokenExpiry = NULL
WHERE PasswordResetTokenExpiry < GETUTCDATE();
```

---

## 📖 Related Documentation

- 🏗️ [Architecture](./ARCHITECTURE.md)
- 📦 [Product Module](./PRODUCT_MODULE.md)
- 📋 [Order Module](./ORDER_MODULE.md)
- 🎟️ [Voucher Module](./VOUCHER_MODULE.md)
- 🚀 [Deployment Guide](./DEPLOYMENT.md)
