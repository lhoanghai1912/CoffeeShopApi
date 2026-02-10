# Voucher Module Documentation

## 🎟️ Overview

Voucher Module quản lý mã giảm giá với 2 loại: **Public** (ai cũng dùng được) và **Private** (chỉ user được assign).

**Controller:** `VouchersController`  
**Service:** `VoucherService`  
**Repository:** Direct DbContext access  
**Entities:** `Voucher`, `VoucherUsage`, `UserVoucher`

---

## 🎯 Key Features

1. **Public Vouchers** - Code-based, anyone can use
2. **Private Vouchers** - Assigned to specific users (Birthday, VIP rewards)
3. **Discount Types** - Fixed Amount & Percentage
4. **Validation** - Expiry, usage limits, order constraints
5. **Assignment System** - Admin can assign vouchers to users

---

## 📡 API Endpoints

### Customer Endpoints

#### 1. Validate Voucher (by Code)

**Endpoint:** `POST /api/vouchers/validate`

**Authorization:** Required

**Request Body:**
```json
{
  "voucherCode": "WELCOME10K",
  "orderSubTotal": 100000
}
```

**Response (Valid):**
```json
{
  "success": true,
  "message": "Voucher hợp lệ",
  "data": {
    "isValid": true,
    "voucherId": 1,
    "voucherCode": "WELCOME10K",
    "discountType": "FixedAmount",
    "discountValue": 10000,
    "discountAmount": 10000,
    "minOrderValue": 50000,
    "maxDiscountAmount": null,
    "errorMessage": null
  }
}
```

**Response (Invalid):**
```json
{
  "success": false,
  "message": "Voucher đã hết hạn",
  "status": 400,
  "data": {
    "isValid": false,
    "errorMessage": "Voucher đã hết hạn"
  }
}
```

**Validation Rules:**
```csharp
✅ Voucher tồn tại
✅ IsActive = true
✅ Trong khoảng StartDate - EndDate
✅ Còn lượt sử dụng (UsageLimit)
✅ User chưa vượt quá UsageLimitPerUser
✅ Order đạt MinOrderValue
✅ Nếu Private: User phải được assigned và chưa dùng
```

---

#### 2. ⭐ Check Voucher (by ID) - Recommended

**Endpoint:** `POST /api/vouchers/check`

**Authorization:** Required

**Use Case:** Khi người dùng chọn voucher từ danh sách (đã có voucherId)

**Request Body:**
```json
{
  "voucherId": 2,
  "orderSubTotal": 150000
}
```

**Response (Valid):**
```json
{
  "success": true,
  "message": "Voucher khả dụng",
  "data": {
    "isValid": true,
    "voucherId": 2,
    "voucherCode": "SALE20",
    "voucherDescription": "Giảm 20% tối đa 50,000đ",
    "discountType": "Percentage",
    "discountValue": 20,
    "minOrderValue": 100000,
    "maxDiscountAmount": 50000,
    "orderSubTotal": 150000,
    "discountAmount": 30000,
    "finalAmount": 120000,
    "savedAmount": 30000,
    "percentageSaved": 20.0
  }
}
```

**Response (Invalid - MinOrderValue not met):**
```json
{
  "success": true,
  "message": "Đơn hàng phải từ 100,000đ để sử dụng voucher này",
  "data": {
    "isValid": false,
    "errorMessage": "Đơn hàng phải từ 100,000đ để sử dụng voucher này",
    "voucherId": 2,
    "orderSubTotal": 80000,
    "discountAmount": 0,
    "finalAmount": 80000
  }
}
```

**Response (Invalid - Expired):**
```json
{
  "success": true,
  "message": "Voucher đã hết hạn",
  "data": {
    "isValid": false,
    "errorMessage": "Voucher đã hết hạn",
    "voucherId": 2,
    "orderSubTotal": 150000,
    "discountAmount": 0,
    "finalAmount": 150000
  }
}
```

**Response (Not Found):**
```json
{
  "success": false,
  "message": "Voucher không tồn tại",
  "status": 404
}
```

**Frontend Example:**
```javascript
// Khi user chọn voucher từ danh sách
const checkVoucher = async (voucherId, cartTotal) => {
  const response = await fetch('/api/vouchers/check', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    },
    body: JSON.stringify({
      voucherId: voucherId,
      orderSubTotal: cartTotal
    })
  });

  const result = await response.json();

  if (result.success && result.data.isValid) {
    // Hiển thị thông tin giảm giá
    console.log(`Tiết kiệm: ${result.data.savedAmount.toLocaleString()}đ`);
    console.log(`Giảm: ${result.data.percentageSaved}%`);
    console.log(`Tổng cuối: ${result.data.finalAmount.toLocaleString()}đ`);
  } else {
    // Hiển thị lỗi
    alert(result.data.errorMessage || 'Voucher không khả dụng');
  }
};
```

**Difference from Validate endpoint:**
- ✅ Nhận `voucherId` thay vì `voucherCode` (thuận tiện hơn)
- ✅ Trả về `finalAmount` và `percentageSaved`
- ✅ Trả về `voucherDescription` để hiển thị
- ✅ Vẫn trả về HTTP 200 kể cả khi invalid (để FE xử lý dễ hơn)

---

#### 3. Get Active Public Vouchers

**Endpoint:** `GET /api/vouchers/active`

**Authorization:** None (Public)

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "code": "WELCOME10K",
      "description": "Giảm 10,000đ cho đơn hàng đầu tiên",
      "discountType": "FixedAmount",
      "discountValue": 10000,
      "minOrderValue": 50000,
      "maxDiscountAmount": null,
      "startDate": "2025-01-01T00:00:00Z",
      "endDate": "2025-12-31T23:59:59Z",
      "remainingUses": 950,
      "isPublic": true
    },
    {
      "id": 2,
      "code": "SALE20",
      "description": "Giảm 20% tối đa 50,000đ",
      "discountType": "Percentage",
      "discountValue": 20,
      "minOrderValue": 100000,
      "maxDiscountAmount": 50000,
      "startDate": "2025-01-15T00:00:00Z",
      "endDate": "2025-02-28T23:59:59Z",
      "remainingUses": 380,
      "isPublic": true
    }
  ]
}
```

**Note:** Chỉ trả về vouchers:
- `IsPublic = true`
- `IsActive = true`
- Còn hạn (EndDate >= now)
- Còn lượt sử dụng (RemainingUses > 0)

---

#### 4. Get My Vouchers (Private)

**Endpoint:** `GET /api/vouchers/my-vouchers`

**Authorization:** Required

**Query Parameters:**
- `isUsed` : bool? (filter by used status)

**Request Examples:**
```bash
# Get all assigned vouchers
GET /api/vouchers/my-vouchers

# Get unused vouchers only
GET /api/vouchers/my-vouchers?isUsed=false

# Get used vouchers only
GET /api/vouchers/my-vouchers?isUsed=true
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "userId": 3,
      "voucherId": 8,
      "voucher": {
        "id": 8,
        "code": "BIRTHDAY30K",
        "description": "🎂 Quà sinh nhật - Giảm 30,000đ",
        "discountType": "FixedAmount",
        "discountValue": 30000,
        "minOrderValue": 50000,
        "startDate": "2025-01-01T00:00:00Z",
        "endDate": "2025-12-31T23:59:59Z"
      },
      "isUsed": false,
      "assignedAt": "2025-01-20T10:30:00Z",
      "usedAt": null,
      "note": "🎂 Quà sinh nhật tháng 1"
    },
    {
      "id": 2,
      "userId": 3,
      "voucherId": 9,
      "voucher": {
        "id": 9,
        "code": "VIPREWARD50",
        "description": "💎 VIP Platinum - Giảm 50%",
        "discountType": "Percentage",
        "discountValue": 50,
        "minOrderValue": 100000,
        "maxDiscountAmount": 100000
      },
      "isUsed": true,
      "assignedAt": "2025-01-15T08:00:00Z",
      "usedAt": "2025-01-25T14:30:00Z",
      "note": "💎 VIP Member Reward"
    }
  ]
}
```

---

#### 5. Get Voucher by Code

**Endpoint:** `GET /api/vouchers/code/{code}`

**Authorization:** None (Public)

**Example:**
```bash
GET /api/vouchers/code/WELCOME10K
```

**Response:**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "code": "WELCOME10K",
    "description": "Giảm 10,000đ cho đơn hàng đầu tiên",
    "discountType": "FixedAmount",
    "discountValue": 10000,
    "minOrderValue": 50000,
    "maxDiscountAmount": null,
    "startDate": "2025-01-01T00:00:00Z",
    "endDate": "2025-12-31T23:59:59Z",
    "usageLimit": 1000,
    "usageLimitPerUser": 1,
    "currentUsageCount": 50,
    "remainingUses": 950,
    "isActive": true,
    "isPublic": true
  }
}
```

---

### Admin Endpoints

#### 6. Get Vouchers with Pagination

**Endpoint:** `GET /api/vouchers/Paged`

**Authorization:** ADMIN/STAFF

**Query Parameters (Standard):**
- `page` : int (default=1) - Page number
- `pageSize` : int (default=10) - Items per page
- `search` : string? (optional) - Search in code or description
- `orderBy` : string? (optional) - Sort expression (Gridify format)
- `filter` : string? (optional) - Filter expression (Gridify format)

**Filter Expression Syntax (Gridify):**
```bash
# Single condition
filter=IsActive=true
filter=IsPublic=false

# Multiple conditions (AND)
filter=IsActive=true,IsPublic=true

# Multiple conditions (OR)
filter=IsActive=true|IsPublic=false

# Comparison operators
filter=DiscountValue>10000
filter=MinOrderValue>=50000
filter=EndDate>2025-01-01

# String operations
filter=Code^=SALE        # StartsWith
filter=Code$=2025        # EndsWith
filter=Description@=gift # Contains
```

**OrderBy Expression Syntax:**
```bash
# Single field
orderBy=CreatedAt desc
orderBy=Code asc

# Multiple fields
orderBy=IsActive desc, CreatedAt desc
```

**Example Requests:**
```bash
# Basic pagination
GET /api/vouchers/Paged?page=1&pageSize=10

# Filter active vouchers
GET /api/vouchers/Paged?filter=IsActive=true

# Filter public vouchers
GET /api/vouchers/Paged?filter=IsPublic=true

# Filter active AND public
GET /api/vouchers/Paged?filter=IsActive=true,IsPublic=true

# Filter private vouchers with high discount
GET /api/vouchers/Paged?filter=IsPublic=false,DiscountValue>20000

# Search
GET /api/vouchers/Paged?search=BIRTHDAY

# Search + Filter
GET /api/vouchers/Paged?search=VIP&filter=IsActive=true,IsPublic=false

# Sort by discount value
GET /api/vouchers/Paged?orderBy=DiscountValue desc

# Combined (filter + search + sort + page)
GET /api/vouchers/Paged?page=1&pageSize=20&search=SALE&filter=IsActive=true&orderBy=CreatedAt desc
```

**Response:**
```json
{
  "success": true,
  "data": {
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 4,
    "totalCount": 35,
    "items": [
      {
        "id": 1,
        "code": "WELCOME10K",
        "description": "Giảm 10,000đ cho đơn đầu",
        "discountType": "FixedAmount",
        "discountValue": 10000,
        "minOrderValue": 50000,
        "maxDiscountAmount": null,
        "startDate": "2025-01-01T00:00:00Z",
        "endDate": "2025-12-31T23:59:59Z",
        "usageLimit": 1000,
        "usageLimitPerUser": 1,
        "currentUsageCount": 50,
        "remainingUses": 950,
        "isPublic": true,
        "isActive": true,
        "createdAt": "2025-01-01T00:00:00Z"
      }
    ]
  }
}
```

**Use Cases:**
```javascript
// Frontend - Load vouchers with filters
const loadVouchers = async (filters) => {
  const params = new URLSearchParams({
    page: filters.page || 1,
    pageSize: filters.pageSize || 10
  });

  // Add filter (Gridify format)
  const filterParts = [];
  if (filters.isActive !== undefined) {
    filterParts.push(`IsActive=${filters.isActive}`);
  }
  if (filters.isPublic !== undefined) {
    filterParts.push(`IsPublic=${filters.isPublic}`);
  }
  if (filterParts.length > 0) {
    params.append('filter', filterParts.join(','));
  }

  // Add search
  if (filters.search) {
    params.append('search', filters.search);
  }

  // Add orderBy
  if (filters.orderBy) {
    params.append('orderBy', filters.orderBy);
  }

  const response = await fetch(`/api/vouchers/Paged?${params}`, {
    headers: { 'Authorization': `Bearer ${token}` }
  });

  return await response.json();
};

// Examples
const activePublicVouchers = await loadVouchers({ 
  page: 1, 
  pageSize: 20, 
  isActive: true, 
  isPublic: true,
  orderBy: 'CreatedAt desc'
});

const highValuePrivateVouchers = await loadVouchers({ 
  isPublic: false,
  filter: 'DiscountValue>50000'
});
```

---

#### 7. Get All Vouchers (No Pagination)

**Endpoint:** `GET /api/vouchers/all`

**Authorization:** ADMIN/STAFF

**Query Parameters:**
- `isActive` : bool? (filter by active status)

**Example Requests:**
```bash
# Get all vouchers (no pagination)
GET /api/vouchers/all

# Get only active vouchers
GET /api/vouchers/all?isActive=true

# Get inactive vouchers
GET /api/vouchers/all?isActive=false
```

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
      "remainingUses": 950,
      "isPublic": true,
      "isActive": true
    },
    {
      "id": 2,
      "code": "SALE20",
      "description": "Giảm 20% tối đa 50,000đ",
      "discountType": "Percentage",
      "discountValue": 20,
      "remainingUses": 380,
      "isPublic": true,
      "isActive": true
    }
  ]
}
```

**Use Cases:**
```javascript
// For dropdown/select options
const loadVoucherOptions = async () => {
  const response = await fetch('/api/vouchers/all?isActive=true', {
    headers: { 'Authorization': `Bearer ${token}` }
  });
  const result = await response.json();

  return result.data.map(v => ({
    value: v.id,
    label: `${v.code} - ${v.description}`
  }));
};

// Example: Assign voucher dropdown
<select>
  {voucherOptions.map(option => (
    <option key={option.value} value={option.value}>
      {option.label}
    </option>
  ))}
</select>
```

**When to Use:**
- ✅ Dropdown/Select options
- ✅ Bulk operations (assign, export)
- ✅ Small datasets (< 100 items)
- ❌ Large datasets (use paginated endpoint)

---

#### 8. Get Voucher by ID

**Endpoint:** `GET /api/vouchers/{id}`

**Authorization:** ADMIN/STAFF

**Example:**
```bash
GET /api/vouchers/8
```

---

#### 9. Create Voucher

**Endpoint:** `POST /api/vouchers`

**Authorization:** ADMIN

**Request Body:**
```json
{
  "code": "FLASH30K",
  "description": "Flash Sale - Giảm 30,000đ",
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

**Validation Rules:**
```csharp
✅ Code: Required, Unique, 3-50 characters, Uppercase
✅ DiscountType: "FixedAmount" or "Percentage"
✅ DiscountValue: > 0
✅ If Percentage: DiscountValue <= 100
✅ MinOrderValue: >= 0 (optional)
✅ MaxDiscountAmount: >= 0 (optional, for Percentage only)
✅ StartDate < EndDate
✅ UsageLimit: > 0 (optional, null = unlimited)
✅ UsageLimitPerUser: > 0 (optional, null = unlimited)
```

**Response:**
```json
{
  "success": true,
  "message": "Tạo voucher thành công",
  "data": {
    "id": 15,
    "code": "FLASH30K",
    ...
  }
}
```

---

#### 10. Update Voucher

**Endpoint:** `PUT /api/vouchers/{id}`

**Authorization:** ADMIN

**Request Body:** (Same as Create)

**Response:**
```json
{
  "success": true,
  "message": "Cập nhật voucher thành công",
  "data": { ... }
}
```

---

#### 11. Delete Voucher (Soft Delete)

**Endpoint:** `DELETE /api/vouchers/{id}`

**Authorization:** ADMIN

**Response:**
```json
{
  "success": true,
  "message": "Vô hiệu hóa voucher thành công"
}
```

**Note:** Đây là soft delete (`IsActive = false`), không xóa vật lý.

---

#### 12. Assign Voucher to Users

**Endpoint:** `POST /api/vouchers/assign`

**Authorization:** ADMIN

**Request Body:**
```json
{
  "voucherId": 8,
  "userIds": [1, 2, 3, 5, 10],
  "note": "Quà sinh nhật tháng 2"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Đã gán voucher cho 5 user",
  "data": {
    "assignedCount": 5
  }
}
```

**Business Logic:**
1. ✅ Validate voucher tồn tại
2. ✅ Validate voucher là Private (`IsPublic = false`)
3. ✅ Check users tồn tại
4. ✅ Skip users đã được assign (unique constraint)
5. ✅ Bulk insert vào `UserVouchers`

---

#### 13. Get Voucher Assignments

**Endpoint:** `GET /api/vouchers/{id}/assignments`

**Authorization:** ADMIN/STAFF

**Example:**
```bash
GET /api/vouchers/8/assignments
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "userId": 3,
      "userName": "Nguyễn Văn A",
      "voucherId": 8,
      "isUsed": false,
      "assignedAt": "2025-01-20T10:30:00Z",
      "usedAt": null,
      "note": "Quà sinh nhật tháng 1"
    },
    {
      "id": 2,
      "userId": 5,
      "userName": "Trần Thị B",
      "voucherId": 8,
      "isUsed": true,
      "assignedAt": "2025-01-20T10:30:00Z",
      "usedAt": "2025-01-25T14:30:00Z",
      "note": "Quà sinh nhật tháng 1"
    }
  ]
}
```

---

## 💰 Discount Calculation Logic

### Fixed Amount

```csharp
if (voucher.DiscountType == DiscountType.FixedAmount)
{
    discountAmount = Math.Min(voucher.DiscountValue, orderSubTotal);
}

// Example:
// Voucher: -10,000đ
// Order: 50,000đ → Discount: 10,000đ
// Order: 8,000đ  → Discount: 8,000đ (không vượt quá order)
```

### Percentage

```csharp
if (voucher.DiscountType == DiscountType.Percentage)
{
    decimal calculated = orderSubTotal * (voucher.DiscountValue / 100);
    
    if (voucher.MaxDiscountAmount.HasValue)
    {
        discountAmount = Math.Min(calculated, voucher.MaxDiscountAmount.Value);
    }
    else
    {
        discountAmount = calculated;
    }
}

// Example:
// Voucher: -20% (max 50,000đ)
// Order: 100,000đ → 20% = 20,000đ → Final: 20,000đ
// Order: 500,000đ → 20% = 100,000đ → Cap at 50,000đ → Final: 50,000đ
```

---

## 🔄 Apply & Rollback Flow

### Apply Voucher (at Checkout)

```csharp
public async Task<Voucher?> ApplyVoucherAsync(int voucherId, int userId)
{
    // Start transaction
    using var transaction = await _context.Database.BeginTransactionAsync();
    try
    {
        var voucher = await _context.Vouchers.FindAsync(voucherId);
        
        // For PUBLIC vouchers
        if (voucher.IsPublic)
        {
            // Atomic increment usage count
            await _context.Database.ExecuteSqlRawAsync(
                "UPDATE Vouchers SET CurrentUsageCount = CurrentUsageCount + 1 WHERE Id = {0}",
                voucherId
            );
            
            // Track usage in VoucherUsages table
            _context.VoucherUsages.Add(new VoucherUsage
            {
                VoucherId = voucherId,
                UserId = userId,
                UsedAt = DateTime.UtcNow
            });
        }
        // For PRIVATE vouchers
        else
        {
            var userVoucher = await _context.UserVouchers
                .FirstOrDefaultAsync(uv => uv.VoucherId == voucherId && uv.UserId == userId);
            
            if (userVoucher == null || userVoucher.IsUsed)
                throw new InvalidOperationException("User không có quyền sử dụng voucher này");
            
            // Mark as used
            userVoucher.IsUsed = true;
            userVoucher.UsedAt = DateTime.UtcNow;
        }
        
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        
        return voucher;
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
}
```

### Rollback Voucher (when Order Cancelled)

```csharp
public async Task RollbackVoucherUsageAsync(int voucherId, int userId)
{
    var voucher = await _context.Vouchers.FindAsync(voucherId);
    
    if (voucher.IsPublic)
    {
        // Decrement usage count
        await _context.Database.ExecuteSqlRawAsync(
            "UPDATE Vouchers SET CurrentUsageCount = CurrentUsageCount - 1 WHERE Id = {0} AND CurrentUsageCount > 0",
            voucherId
        );
        
        // Delete usage record
        var usage = await _context.VoucherUsages
            .Where(vu => vu.VoucherId == voucherId && vu.UserId == userId)
            .OrderByDescending(vu => vu.UsedAt)
            .FirstOrDefaultAsync();
        
        if (usage != null)
            _context.VoucherUsages.Remove(usage);
    }
    else
    {
        // Reset IsUsed flag
        var userVoucher = await _context.UserVouchers
            .FirstOrDefaultAsync(uv => uv.VoucherId == voucherId && uv.UserId == userId);
        
        if (userVoucher != null)
        {
            userVoucher.IsUsed = false;
            userVoucher.UsedAt = null;
        }
    }
    
    await _context.SaveChangesAsync();
}
```

---

## 🏗️ Database Schema

### Vouchers Table

```sql
CREATE TABLE Vouchers (
    Id INT PRIMARY KEY IDENTITY,
    Code NVARCHAR(50) UNIQUE NOT NULL,
    Description NVARCHAR(500),
    
    -- Discount Settings
    DiscountType INT NOT NULL,  -- 0=FixedAmount, 1=Percentage
    DiscountValue DECIMAL(18,2) NOT NULL,
    MinOrderValue DECIMAL(18,2),
    MaxDiscountAmount DECIMAL(18,2),  -- For Percentage only
    
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

-- Indexes
CREATE UNIQUE INDEX IX_Vouchers_Code ON Vouchers(Code);
CREATE INDEX IX_Vouchers_IsPublic ON Vouchers(IsPublic);
CREATE INDEX IX_Vouchers_IsActive_EndDate ON Vouchers(IsActive, EndDate);
```

### UserVouchers Table (Private Voucher Assignments)

```sql
CREATE TABLE UserVouchers (
    Id INT PRIMARY KEY IDENTITY,
    UserId INT NOT NULL,
    VoucherId INT NOT NULL,
    IsUsed BIT NOT NULL DEFAULT 0,
    AssignedAt DATETIME2 NOT NULL,
    UsedAt DATETIME2,
    Note NVARCHAR(200),
    
    CONSTRAINT FK_UserVouchers_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT FK_UserVouchers_Vouchers FOREIGN KEY (VoucherId) REFERENCES Vouchers(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_UserVouchers_User_Voucher UNIQUE (UserId, VoucherId)  -- One assignment per user
);

-- Indexes
CREATE INDEX IX_UserVouchers_UserId ON UserVouchers(UserId);
CREATE INDEX IX_UserVouchers_VoucherId ON UserVouchers(VoucherId);
CREATE INDEX IX_UserVouchers_UserId_IsUsed ON UserVouchers(UserId, IsUsed);
```

### VoucherUsages Table (Public Voucher Usage History)

```sql
CREATE TABLE VoucherUsages (
    Id INT PRIMARY KEY IDENTITY,
    VoucherId INT NOT NULL,
    UserId INT,
    OrderId INT,
    UsedAt DATETIME2 NOT NULL,
    
    CONSTRAINT FK_VoucherUsages_Vouchers FOREIGN KEY (VoucherId) REFERENCES Vouchers(Id) ON DELETE CASCADE,
    CONSTRAINT FK_VoucherUsages_Users FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT FK_VoucherUsages_Orders FOREIGN KEY (OrderId) REFERENCES Orders(Id)
);

-- Indexes
CREATE INDEX IX_VoucherUsages_VoucherId ON VoucherUsages(VoucherId);
CREATE INDEX IX_VoucherUsages_UserId ON VoucherUsages(UserId);
CREATE INDEX IX_VoucherUsages_OrderId ON VoucherUsages(OrderId);
```

---

## 🎁 Sample Vouchers (Seeded)

### Public Vouchers (20)

| Code | Description | Type | Value | Min Order | Max Discount |
|------|-------------|------|-------|-----------|--------------|
| WELCOME10K | Giảm 10,000đ cho đơn đầu | Fixed | 10,000 | 50,000 | - |
| SALE20 | Giảm 20% | Percentage | 20% | 100,000 | 50,000 |
| BIGORDER50K | Giảm 50k cho đơn 200k+ | Fixed | 50,000 | 200,000 | - |
| VIP15 | VIP 15% | Percentage | 15% | 80,000 | 100,000 |
| FREESHIP | Miễn phí ship | Fixed | 20,000 | 50,000 | - |

### Private Vouchers (15)

| Code | Description | Type | Value | Target |
|------|-------------|------|-------|--------|
| BIRTHDAY30K | Quà sinh nhật | Fixed | 30,000 | All users |
| BIRTHDAY40 | Sinh nhật 40% | Percentage | 40% | All users |
| VIPREWARD50 | VIP Platinum | Percentage | 50% | VIP users |
| LOYALTY20K | Khách thân thiết | Fixed | 20,000 | Frequent buyers |
| REFERRAL35K | Giới thiệu bạn | Fixed | 35,000 | Referrals |

---

## 🐛 Common Errors

### 1. Voucher Expired
```json
{
  "success": false,
  "message": "Voucher đã hết hạn",
  "status": 400
}
```

### 2. Usage Limit Reached
```json
{
  "success": false,
  "message": "Voucher đã hết lượt sử dụng",
  "status": 400
}
```

### 3. Order Too Small
```json
{
  "success": false,
  "message": "Đơn hàng phải từ 100,000đ để sử dụng voucher này",
  "status": 400
}
```

### 4. User Not Eligible (Private Voucher)
```json
{
  "success": false,
  "message": "Bạn không có quyền sử dụng voucher này",
  "status": 400
}
```

### 5. Voucher Already Used (Private)
```json
{
  "success": false,
  "message": "Bạn đã sử dụng voucher này rồi",
  "status": 400
}
```

---

## 📖 Related Documentation

- 📋 [Order Module](./ORDER_MODULE.md) (Voucher integration in checkout)
- 🗄️ [Database Schema](./DATABASE.md)
