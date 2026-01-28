# Authentication Module Documentation

## 🔐 Overview

Authentication Module quản lý đăng nhập, đăng ký, xác thực email và reset password.

**Controller:** `AuthController`  
**Service:** `AuthService`, `EmailService`  
**Entities:** `User`, `Role`, `Permission`

---

## 🎯 Key Features

1. **Login** - JWT Bearer Token authentication
2. **Register** - User registration with email verification
3. **Email Verification** - 6-digit code verification
4. **Forgot Password** - Password reset via email
5. **BCrypt Password Hashing** - Secure password storage

---

## 📡 API Endpoints

### 1. Login

**Endpoint:** `POST /api/auth/login`

**Request Body:**
```json
{
  "username": "customer01",
  "password": "Customer@123"
}
```

**Response (Success):**
```json
{
  "success": true,
  "message": "Đăng nhập thành công",
  "status": 200,
  "data": {
    "id": 3,
    "username": "customer01",
    "fullName": "Nguyễn Văn A",
    "email": "customer01@example.com",
    "phoneNumber": "0912345678",
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIzIiwidXNlcklkIjoiMyIsInVzZXJuYW1lIjoiY3VzdG9tZXIwMSIsInJvbGUiOiJDVVNUT01FUiIsInBlcm1pc3Npb25zIjoib3JkZXIudmlldy5vd24sb3JkZXIuY3JlYXRlIiwiZXhwIjoxNzM4MTU3MzAwfQ.abc123...",
    "role": {
      "id": 2,
      "code": "CUSTOMER",
      "name": "Khách hàng",
      "permissions": [
        {
          "id": 9,
          "code": "order.view.own",
          "name": "Xem đơn hàng của mình",
          "module": "Order"
        },
        {
          "id": 11,
          "code": "order.create",
          "name": "Tạo đơn hàng",
          "module": "Order"
        }
      ]
    }
  }
}
```

**Response (Failed):**
```json
{
  "success": false,
  "message": "Tên đăng nhập hoặc mật khẩu không đúng",
  "status": 401
}
```

**Validation Rules:**
- ✅ Email chưa được xác thực → "Vui lòng xác thực email trước khi đăng nhập"
- ✅ User bị vô hiệu hóa → "Tài khoản đã bị vô hiệu hóa. Vui lòng liên hệ hỗ trợ."
- ✅ Sai username/password → "Sai tài khoản hoặc mật khẩu"

**cURL Example:**
```bash
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "customer01",
    "password": "Customer@123"
  }'
```

---

### 2. Register

**Endpoint:** `POST /api/auth/register`

**Request Body:**
```json
{
  "username": "newuser123",
  "password": "SecurePass@123",
  "confirmPassword": "SecurePass@123",
  "email": "newuser@example.com",
  "fullName": "Nguyễn Văn B",
  "phoneNumber": "0987654321"
}
```

**Response (Success):**
```json
{
  "success": true,
  "message": "Đăng ký thành công",
  "status": 200,
  "data": {
    "userId": 15,
    "username": "newuser123",
    "email": "newuser@example.com",
    "message": "Đăng ký thành công! Vui lòng kiểm tra email để xác thực tài khoản.",
    "verificationCode": "123456"  // ⚠️ Chỉ có trong Development mode
  }
}
```

**Response (Failed):**
```json
{
  "success": false,
  "message": "Username đã tồn tại",
  "status": 400
}
```

**Validation Rules:**
```csharp
✅ Username: 
   - Required
   - 3-50 characters
   - Unique
   - Alphanumeric + underscore

✅ Password: 
   - Required
   - Min 8 characters
   - Must contain: Uppercase, Lowercase, Digit, Special char

✅ Email: 
   - Required
   - Valid email format
   - Unique

✅ FullName: Required
✅ PhoneNumber: Valid Vietnamese phone format (10-11 digits)
✅ ConfirmPassword: Must match Password
```

**Business Logic:**
1. ✅ Validate input
2. ✅ Check username/email uniqueness
3. ✅ Hash password with BCrypt
4. ✅ Create user with role = CUSTOMER
5. ✅ Generate 6-digit verification code
6. ✅ Save code & expiry to database (15 minutes)
7. ✅ Send verification email
8. ✅ Return userId & message

---

### 3. Verify Email (Commented Out - Future)

**Endpoint:** `POST /api/auth/verify-email`

**Request Body:**
```json
{
  "emailOrUsername": "newuser123",
  "verificationCode": "123456"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Xác thực email thành công",
  "data": {
    "success": true,
    "message": "Email đã được xác thực",
    "token": "eyJhbGc...",  // Auto-login token
    "userId": 15
  }
}
```

**Business Logic:**
1. ✅ Find user by email/username
2. ✅ Check code expiry (15 minutes)
3. ✅ Verify code matches
4. ✅ Set `IsEmailVerified = true`
5. ✅ Generate JWT token for auto-login
6. ✅ Delete verification code

---

### 4. Forgot Password

**Endpoint:** `POST /api/auth/forgot-password`

**Request Body:**
```json
{
  "emailOrUsername": "customer01"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Mã reset password đã được gửi đến email của bạn",
  "data": {
    "success": true,
    "message": "Mã reset password đã được gửi đến email của bạn",
    "resetToken": "123456"  // ⚠️ Chỉ có trong Development mode
  }
}
```

**Business Logic:**
1. ✅ Find user by email/username
2. ✅ Generate 6-digit reset token
3. ✅ Save token & expiry (15 minutes) to database
4. ✅ Send email with reset token
5. ✅ Rate limit: Max 5 requests/day per account

**Email Template:**
```
Subject: Reset Password - CoffeeShop

Xin chào {FullName},

Bạn đã yêu cầu reset mật khẩu. Mã xác thực của bạn là:

    123456

Mã này có hiệu lực trong 15 phút.

Nếu bạn không yêu cầu reset password, vui lòng bỏ qua email này.

Trân trọng,
CoffeeShop Team
```

---

### 5. Reset Password

**Endpoint:** `POST /api/auth/reset-password`

**Request Body:**
```json
{
  "emailOrUsername": "customer01",
  "resetToken": "123456",
  "newPassword": "NewSecurePass@123",
  "confirmNewPassword": "NewSecurePass@123"
}
```

**Response (Success):**
```json
{
  "success": true,
  "message": "Đặt lại mật khẩu thành công",
  "data": {
    "success": true,
    "message": "Đặt lại mật khẩu thành công. Bạn có thể đăng nhập với mật khẩu mới."
  }
}
```

**Response (Failed):**
```json
{
  "success": false,
  "message": "Mã xác thực không hợp lệ hoặc đã hết hạn",
  "status": 400
}
```

**Business Logic:**
1. ✅ Validate reset token
2. ✅ Check token expiry (15 minutes)
3. ✅ Validate new password format
4. ✅ Hash new password with BCrypt
5. ✅ Update user password
6. ✅ Delete reset token
7. ✅ Invalidate all existing JWT tokens (optional)

---

## 🔑 JWT Token Structure

### Token Payload (Claims)

```json
{
  "sub": "3",                    // User ID (standard claim)
  "userId": "3",                 // Custom claim
  "username": "customer01",
  "role": "CUSTOMER",
  "permissions": "order.view.own,order.create,user.view.own",
  "exp": 1738157300,             // Expiry timestamp
  "iss": "http://localhost:5000", // Issuer
  "aud": "http://localhost:5000"  // Audience
}
```

### Token Generation

```csharp
private string GenerateJwtToken(User user, List<string> permissions)
{
    var jwtSettings = _configuration.GetSection("Jwt");
    var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);
    var issuer = jwtSettings["Issuer"];
    var audience = jwtSettings["Audience"];
    
    var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new Claim("userId", user.Id.ToString()),
        new Claim("username", user.Username),
        new Claim("role", user.Role.Code),
        new Claim("permissions", string.Join(",", permissions))
    };
    
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(claims),
        Expires = DateTime.UtcNow.AddDays(7), // 7 days
        Issuer = issuer,
        Audience = audience,
        SigningCredentials = new SigningCredentials(
            new SymmetricSecurityKey(key),
            SecurityAlgorithms.HmacSha256Signature
        )
    };
    
    var tokenHandler = new JwtSecurityTokenHandler();
    var token = tokenHandler.CreateToken(tokenDescriptor);
    return tokenHandler.WriteToken(token);
}
```

### Using Token in Subsequent Requests

```bash
# Add to Authorization header
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...

# Example
curl -X GET https://localhost:5001/api/users/profile \
  -H "Authorization: Bearer eyJhbGc..."
```

---

## 🔒 Password Security

### BCrypt Hashing

```csharp
// Hash password when registering
string hashedPassword = BCrypt.Net.BCrypt.HashPassword(plainPassword);

// Verify password when logging in
bool isValid = BCrypt.Net.BCrypt.Verify(plainPassword, hashedPassword);
```

**Benefits:**
- ✅ **Salt automatically included** - mỗi hash khác nhau
- ✅ **Slow by design** - chống brute-force
- ✅ **One-way function** - không thể reverse

**Example Hashed Password:**
```
$2a$11$vZ5Q5XjH1k.8xZ9aJ7X1h.xY8aH1k.8xZ9aJ7X1h.xY8aH1k.8x
```

---

## 📧 Email Service

### SMTP Configuration

```json
// appsettings.json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "noreply@coffeeshop.com",
    "SmtpPassword": "app-password",
    "FromEmail": "noreply@coffeeshop.com",
    "FromName": "CoffeeShop"
  }
}
```

### Email Templates

**1. Email Verification:**
```
Subject: Xác thực email - CoffeeShop

Xin chào {FullName},

Cảm ơn bạn đã đăng ký tài khoản tại CoffeeShop!

Mã xác thực của bạn là:

    123456

Mã này có hiệu lực trong 15 phút.

Trân trọng,
CoffeeShop Team
```

**2. Password Reset:**
```
Subject: Reset Password - CoffeeShop

Xin chào {FullName},

Bạn đã yêu cầu reset mật khẩu. Mã xác thực của bạn là:

    123456

Mã này có hiệu lực trong 15 phút.

Trân trọng,
CoffeeShop Team
```

---

## 🏗️ Database Schema

### Users Table

```sql
CREATE TABLE Users (
    Id INT PRIMARY KEY IDENTITY,
    Username NVARCHAR(50) UNIQUE NOT NULL,
    Password NVARCHAR(255) NOT NULL,  -- BCrypt hash
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
CREATE INDEX IX_Users_IsActive ON Users(IsActive);
```

---

## 🔐 Authorization

### Permission-Based Access Control

**Roles:**
```csharp
public enum RoleCode
{
    ADMIN,    // Full access
    STAFF,    // Product/Order management
    CUSTOMER  // Own orders/profile only
}
```

**Permission Format:** `{module}.{action}[.scope]`

**Examples:**
- `product.view` - View products (public)
- `order.view.own` - View own orders
- `order.view.all` - View all orders (STAFF/ADMIN)
- `user.update.own` - Update own profile
- `user.update.all` - Update any user (ADMIN)

**Usage in Controllers:**
```csharp
[Authorize] // Require login
[RequirePermission("order.view.all")] // Require specific permission
public async Task<IActionResult> GetAllOrders()
{
    // Only STAFF/ADMIN can access
}
```

---

## 🐛 Common Errors

### 1. Login Failed - Wrong Credentials
```json
{
  "success": false,
  "message": "Sai tài khoản hoặc mật khẩu",
  "status": 401
}
```

### 2. Login Failed - Email Not Verified
```json
{
  "success": false,
  "message": "Vui lòng xác thực email trước khi đăng nhập",
  "status": 401
}
```

### 3. Login Failed - Account Deactivated
```json
{
  "success": false,
  "message": "Tài khoản đã bị vô hiệu hóa. Vui lòng liên hệ hỗ trợ.",
  "status": 401
}
```

### 4. Register Failed - Username Exists
```json
{
  "success": false,
  "message": "Username đã tồn tại",
  "status": 400
}
```

### 5. Reset Token Invalid/Expired
```json
{
  "success": false,
  "message": "Mã xác thực không hợp lệ hoặc đã hết hạn",
  "status": 400
}
```

---

## 📖 Related Documentation

- 👤 [User Module](./USER_MODULE.md)
- 🏗️ [Architecture](./ARCHITECTURE.md)
