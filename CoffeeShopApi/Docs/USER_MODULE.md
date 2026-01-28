# User Module Documentation

## 👤 Overview

User Module quản lý thông tin cá nhân, địa chỉ giao hàng và bảo mật tài khoản.

**Controller:** `UsersController`  
**Service:** `UserService`, `UserAddressService`  
**Entities:** `User`, `UserAddress`

---

## 🎯 Key Features

1. **Profile Management** - View & update user information
2. **Address Management** - CRUD operations for delivery addresses
3. **Password Management** - Change password
4. **Order History** - View past orders
5. **Account Status** - Deactivate/Reactivate account

---

## 📡 API Endpoints

### Profile Management

#### 1. Get Current User Profile

**Endpoint:** `GET /api/users/profile`

**Authorization:** Required (any authenticated user)

**Response:**
```json
{
  "success": true,
  "data": {
    "id": 3,
    "username": "customer01",
    "fullName": "Nguyễn Văn A",
    "email": "customer01@example.com",
    "phoneNumber": "0912345678",
    "role": {
      "id": 2,
      "code": "CUSTOMER",
      "name": "Khách hàng"
    },
    "isEmailVerified": true,
    "isActive": true,
    "createdAt": "2025-01-15T10:00:00Z",
    "lastLoginAt": "2025-01-28T08:30:00Z"
  }
}
```

**cURL Example:**
```bash
curl -X GET https://localhost:5001/api/users/profile \
  -H "Authorization: Bearer {token}"
```

---

#### 2. Get Profile with Statistics

**Endpoint:** `GET /api/users/profile/stats`

**Authorization:** Required

**Response:**
```json
{
  "success": true,
  "data": {
    "id": 3,
    "username": "customer01",
    "fullName": "Nguyễn Văn A",
    "email": "customer01@example.com",
    "phoneNumber": "0912345678",
    "orderStats": {
      "totalOrders": 25,
      "pendingOrders": 2,
      "completedOrders": 20,
      "totalSpent": 2450000
    },
    "addresses": [
      {
        "id": 1,
        "recipientName": "Nguyễn Văn A",
        "phoneNumber": "0912345678",
        "addressLine": "123 Nguyễn Huệ, Q1, TP.HCM",
        "label": "Nhà riêng",
        "isDefault": true,
        "createdAt": "2025-01-15T10:00:00Z"
      },
      {
        "id": 2,
        "recipientName": "Nguyễn Văn A",
        "phoneNumber": "0912345678",
        "addressLine": "456 Lê Lợi, Q3, TP.HCM",
        "label": "Văn phòng",
        "isDefault": false,
        "createdAt": "2025-01-20T14:00:00Z"
      }
    ]
  }
}
```

---

#### 3. Update Profile

**Endpoint:** `PUT /api/users/profile`

**Authorization:** Required

**Request Body:**
```json
{
  "fullName": "Nguyễn Văn A (Updated)",
  "email": "newemail@example.com",
  "phoneNumber": "0987654321"
}
```

**Validation Rules:**
```csharp
✅ FullName: Optional, max 100 characters
✅ Email: Optional, must be unique, valid format
✅ PhoneNumber: Optional, Vietnamese format (10-11 digits)
```

**Response:**
```json
{
  "success": true,
  "message": "Cập nhật thông tin thành công",
  "data": {
    "id": 3,
    "username": "customer01",
    "fullName": "Nguyễn Văn A (Updated)",
    "email": "newemail@example.com",
    "phoneNumber": "0987654321"
  }
}
```

**Common Errors:**
```json
// Email already exists
{
  "success": false,
  "message": "Email đã được sử dụng bởi tài khoản khác",
  "status": 400
}
```

---

### Password Management

#### 4. Change Password

**Endpoint:** `POST /api/users/change-password`

**Authorization:** Required

**Request Body:**
```json
{
  "oldPassword": "OldPass@123",
  "newPassword": "NewPass@456",
  "confirmNewPassword": "NewPass@456"
}
```

**Validation Rules:**
```csharp
✅ OldPassword: Must match current password
✅ NewPassword: 
   - Min 8 characters
   - Must contain: Uppercase, Lowercase, Digit, Special char
   - Must differ from old password
✅ ConfirmNewPassword: Must match NewPassword
```

**Response (Success):**
```json
{
  "success": true,
  "message": "Đổi mật khẩu thành công",
  "data": {
    "success": true,
    "message": "Đổi mật khẩu thành công"
  }
}
```

**Response (Failed):**
```json
{
  "success": false,
  "message": "Mật khẩu cũ không đúng",
  "status": 400
}
```

**Business Logic:**
1. ✅ Verify old password with BCrypt
2. ✅ Check new password is different from old
3. ✅ Hash new password with BCrypt
4. ✅ Update password in database
5. ✅ (Optional) Invalidate all existing tokens

---

### Address Management

#### 5. List User Addresses

**Endpoint:** `GET /api/users/addresses`

**Authorization:** Required

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "userId": 3,
      "recipientName": "Nguyễn Văn A",
      "phoneNumber": "0912345678",
      "addressLine": "123 Nguyễn Huệ, Phường Bến Nghé, Quận 1, TP.HCM",
      "label": "Nhà riêng",
      "isDefault": true,
      "createdAt": "2025-01-15T10:00:00Z",
      "updatedAt": "2025-01-15T10:00:00Z"
    },
    {
      "id": 2,
      "userId": 3,
      "recipientName": "Nguyễn Văn A",
      "phoneNumber": "0987654321",
      "addressLine": "456 Lê Lợi, Phường Bến Thành, Quận 1, TP.HCM",
      "label": "Văn phòng",
      "isDefault": false,
      "createdAt": "2025-01-20T14:00:00Z",
      "updatedAt": "2025-01-20T14:00:00Z"
    }
  ]
}
```

**Note:** Addresses được sắp xếp: Default address trước, sau đó theo thời gian tạo.

---

#### 6. Get Address by ID

**Endpoint:** `GET /api/users/addresses/{id}`

**Authorization:** Required (must be owner)

**Response:**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "userId": 3,
    "recipientName": "Nguyễn Văn A",
    "phoneNumber": "0912345678",
    "addressLine": "123 Nguyễn Huệ, Phường Bến Nghé, Quận 1, TP.HCM",
    "label": "Nhà riêng",
    "isDefault": true,
    "createdAt": "2025-01-15T10:00:00Z",
    "updatedAt": "2025-01-15T10:00:00Z"
  }
}
```

---

#### 7. Create New Address

**Endpoint:** `POST /api/users/addresses`

**Authorization:** Required

**Request Body:**
```json
{
  "recipientName": "Nguyễn Văn A",
  "phoneNumber": "0912345678",
  "addressLine": "789 Võ Văn Tần, Quận 3, TP.HCM",
  "label": "Nhà bạn bè",
  "isDefault": false
}
```

**Validation Rules:**
```csharp
✅ RecipientName: Required, max 100 characters
✅ PhoneNumber: Required, Vietnamese format (10-11 digits)
✅ AddressLine: Required, max 500 characters
✅ Label: Optional, max 50 characters
✅ IsDefault: Optional, default = false
```

**Response:**
```json
{
  "success": true,
  "message": "Thêm địa chỉ thành công",
  "data": {
    "id": 3,
    "userId": 3,
    "recipientName": "Nguyễn Văn A",
    "phoneNumber": "0912345678",
    "addressLine": "789 Võ Văn Tần, Quận 3, TP.HCM",
    "label": "Nhà bạn bè",
    "isDefault": false,
    "createdAt": "2025-01-28T10:00:00Z"
  }
}
```

**Business Logic:**
- Nếu đây là địa chỉ đầu tiên → Tự động set `IsDefault = true`
- Nếu `IsDefault = true` → Unset default cho tất cả địa chỉ khác của user

---

#### 8. Update Address

**Endpoint:** `PUT /api/users/addresses/{id}`

**Authorization:** Required (must be owner)

**Request Body:**
```json
{
  "recipientName": "Nguyễn Văn A",
  "phoneNumber": "0987654321",
  "addressLine": "789 Võ Văn Tần (Updated), Quận 3, TP.HCM",
  "label": "Nhà mới",
  "isDefault": false
}
```

**Response:**
```json
{
  "success": true,
  "message": "Cập nhật địa chỉ thành công",
  "data": { ... }
}
```

---

#### 9. Set Default Address

**Endpoint:** `POST /api/users/addresses/{id}/set-default`

**Authorization:** Required (must be owner)

**Response:**
```json
{
  "success": true,
  "message": "Đã đặt làm địa chỉ mặc định",
  "data": { ... }
}
```

**Business Logic:**
1. ✅ Validate address belongs to user
2. ✅ Set `IsDefault = true` for this address
3. ✅ Set `IsDefault = false` for all other addresses of user

---

#### 10. Delete Address

**Endpoint:** `DELETE /api/users/addresses/{id}`

**Authorization:** Required (must be owner)

**Response:**
```json
{
  "success": true,
  "message": "Xóa địa chỉ thành công"
}
```

**Business Logic:**
- Nếu xóa default address → Tự động set default cho địa chỉ đầu tiên còn lại
- Nếu address đang được dùng trong orders → Vẫn xóa được (orders có snapshot)

---

### Admin Endpoints

#### 11. Get All Users (Admin/Staff)

**Endpoint:** `GET /api/users`

**Authorization:** ADMIN or STAFF

**Query Parameters:**
- `page` : int (default=1)
- `pageSize` : int (default=10)
- `search` : string? (search in username, fullName, email)
- `isActive` : bool? (filter by active status)
- `roleId` : int? (filter by role)

**Example:**
```bash
GET /api/users?page=1&pageSize=20&search=nguyen&isActive=true&roleId=2
```

**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 3,
        "username": "customer01",
        "fullName": "Nguyễn Văn A",
        "email": "customer01@example.com",
        "phoneNumber": "0912345678",
        "role": { "id": 2, "code": "CUSTOMER", "name": "Khách hàng" },
        "isActive": true,
        "isEmailVerified": true,
        "createdAt": "2025-01-15T10:00:00Z",
        "lastLoginAt": "2025-01-28T08:30:00Z"
      }
    ],
    "totalCount": 150,
    "page": 1,
    "pageSize": 20,
    "totalPages": 8
  }
}
```

---

#### 12. Get User by ID (Admin/Staff)

**Endpoint:** `GET /api/users/{id}`

**Authorization:** ADMIN or STAFF

**Response:** User object with full details

---

#### 13. Deactivate User (Admin)

**Endpoint:** `POST /api/users/{id}/deactivate`

**Authorization:** ADMIN only

**Request Body:**
```json
{
  "reason": "Vi phạm điều khoản sử dụng"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Vô hiệu hóa tài khoản thành công",
  "data": {
    "id": 5,
    "isActive": false,
    "updatedAt": "2025-01-28T10:00:00Z"
  }
}
```

**Business Logic:**
- User không thể login
- Existing JWT tokens vẫn valid cho đến khi hết hạn
- Orders/Data không bị xóa

---

#### 14. Reactivate User (Admin)

**Endpoint:** `POST /api/users/{id}/reactivate`

**Authorization:** ADMIN only

**Response:**
```json
{
  "success": true,
  "message": "Kích hoạt lại tài khoản thành công",
  "data": {
    "id": 5,
    "isActive": true,
    "updatedAt": "2025-01-28T10:00:00Z"
  }
}
```

---

## 🏗️ Database Schema

### Users Table

```sql
CREATE TABLE Users (
    Id INT PRIMARY KEY IDENTITY,
    Username NVARCHAR(50) UNIQUE NOT NULL,
    Password NVARCHAR(255) NOT NULL,
    Email NVARCHAR(100) UNIQUE,
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
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NOT NULL,
    LastLoginAt DATETIME2,
    
    CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleId) REFERENCES Roles(Id)
);

-- Indexes
CREATE UNIQUE INDEX IX_Users_Username ON Users(Username);
CREATE UNIQUE INDEX IX_Users_Email ON Users(Email) WHERE Email IS NOT NULL;
CREATE INDEX IX_Users_RoleId ON Users(RoleId);
CREATE INDEX IX_Users_IsActive ON Users(IsActive);
```

### UserAddresses Table

```sql
CREATE TABLE UserAddresses (
    Id INT PRIMARY KEY IDENTITY,
    UserId INT NOT NULL,
    RecipientName NVARCHAR(100) NOT NULL,
    PhoneNumber NVARCHAR(20) NOT NULL,
    AddressLine NVARCHAR(500) NOT NULL,
    Label NVARCHAR(50),
    IsDefault BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NOT NULL,
    
    CONSTRAINT FK_UserAddresses_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

-- Indexes
CREATE INDEX IX_UserAddresses_UserId ON UserAddresses(UserId);
CREATE INDEX IX_UserAddresses_UserId_IsDefault ON UserAddresses(UserId, IsDefault);
```

---

## 💡 Business Rules

### Profile Update Rules

```csharp
✅ Username: Cannot be changed after registration
✅ Email: Can be changed, must be unique, triggers verification
✅ FullName: Can be changed anytime
✅ PhoneNumber: Can be changed anytime
✅ Role: Cannot be changed by user (Admin only)
```

### Address Management Rules

```csharp
✅ Each user can have multiple addresses
✅ Only ONE address can be default
✅ First address is automatically default
✅ Cannot delete address if it's being used in active orders (Draft/Pending)
✅ Deleting default address auto-assigns default to next address
```

### Password Change Rules

```csharp
✅ Must provide correct old password
✅ New password must meet complexity requirements
✅ New password must differ from old password
✅ Password change does NOT invalidate existing JWT tokens (by default)
```

---

## 🔐 Security Considerations

### Password Security

```csharp
// Hashing (BCrypt with work factor 11)
string hashedPassword = BCrypt.Net.BCrypt.HashPassword(plainPassword);

// Verification
bool isValid = BCrypt.Net.BCrypt.Verify(plainPassword, hashedPassword);
```

### Email Uniqueness

```csharp
// Check before update
var emailExists = await _context.Users
    .AnyAsync(u => u.Email == newEmail && u.Id != currentUserId);

if (emailExists)
    throw new InvalidOperationException("Email đã được sử dụng");
```

### Authorization

```csharp
// User can only access their own data
if (userId != currentUserId && !User.IsInRole("ADMIN"))
    return Forbid();

// Address ownership check
var address = await _context.UserAddresses
    .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == currentUserId);

if (address == null)
    return NotFound(); // or Forbid()
```

---

## 🐛 Common Errors

### 1. Unauthorized Access
```json
{
  "success": false,
  "message": "Bạn không có quyền truy cập tài nguyên này",
  "status": 403
}
```

### 2. Email Already Exists
```json
{
  "success": false,
  "message": "Email đã được sử dụng bởi tài khoản khác",
  "status": 400
}
```

### 3. Wrong Old Password
```json
{
  "success": false,
  "message": "Mật khẩu cũ không đúng",
  "status": 400
}
```

### 4. Address Not Found
```json
{
  "success": false,
  "message": "Không tìm thấy địa chỉ",
  "status": 404
}
```

### 5. Cannot Delete Last Address
```json
{
  "success": false,
  "message": "Không thể xóa địa chỉ cuối cùng",
  "status": 400
}
```

---

## 📱 Frontend Integration Example

### React - Profile Page

```typescript
function UserProfile() {
  const [profile, setProfile] = useState(null);
  const [editing, setEditing] = useState(false);
  
  useEffect(() => {
    fetch('/api/users/profile', {
      headers: { 'Authorization': `Bearer ${token}` }
    })
    .then(res => res.json())
    .then(data => setProfile(data.data));
  }, []);
  
  const handleUpdate = async (formData) => {
    const response = await fetch('/api/users/profile', {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify(formData)
    });
    
    if (response.ok) {
      const data = await response.json();
      setProfile(data.data);
      setEditing(false);
      toast.success('Cập nhật thành công');
    }
  };
  
  return (
    <div>
      {editing ? (
        <ProfileEditForm profile={profile} onSave={handleUpdate} />
      ) : (
        <ProfileDisplay profile={profile} onEdit={() => setEditing(true)} />
      )}
    </div>
  );
}
```

### React - Address Management

```typescript
function AddressList() {
  const [addresses, setAddresses] = useState([]);
  
  const fetchAddresses = async () => {
    const response = await fetch('/api/users/addresses', {
      headers: { 'Authorization': `Bearer ${token}` }
    });
    const data = await response.json();
    setAddresses(data.data);
  };
  
  const handleSetDefault = async (addressId) => {
    await fetch(`/api/users/addresses/${addressId}/set-default`, {
      method: 'POST',
      headers: { 'Authorization': `Bearer ${token}` }
    });
    await fetchAddresses(); // Refresh
  };
  
  const handleDelete = async (addressId) => {
    if (confirm('Bạn có chắc muốn xóa địa chỉ này?')) {
      await fetch(`/api/users/addresses/${addressId}`, {
        method: 'DELETE',
        headers: { 'Authorization': `Bearer ${token}` }
      });
      await fetchAddresses(); // Refresh
    }
  };
  
  return (
    <div>
      {addresses.map(address => (
        <AddressCard 
          key={address.id}
          address={address}
          onSetDefault={handleSetDefault}
          onDelete={handleDelete}
        />
      ))}
      <AddAddressButton onClick={() => navigate('/addresses/new')} />
    </div>
  );
}
```

---

## 📖 Related Documentation

- 🔐 [Authentication Module](./AUTH_MODULE.md)
- 📋 [Order Module](./ORDER_MODULE.md)
- 🗄️ [Database Schema](./DATABASE.md)
