# 📧 CoffeeShop API - Email Authentication Flow

## 🎯 Tổng quan

Hệ thống xác thực email cho CoffeeShop API với các tính năng:
- ✅ Đăng ký tài khoản với email verification
- ✅ Xác thực email bằng mã 6 số
- ✅ Quên mật khẩu qua email
- ✅ Reset mật khẩu bằng mã 6 số
- ✅ Rate limiting (chống spam)
- ✅ Token hashing (bảo mật)
- ✅ Dev mode (trả mã trong response để test)

---

## 📋 Flow đăng ký và đăng nhập

### 1️⃣ Đăng ký tài khoản (Register)

**Endpoint:** `POST /api/auth/register`

**Request:**
```json
{
  "username": "testuser",
  "email": "test@example.com",
  "password": "Abc@12345",
  "fullName": "Nguyễn Văn A",
  "phoneNumber": "0123456789"
}
```

**Response (Success):**
```json
{
  "success": true,
  "data": {
    "userId": 123,
    "email": "test@example.com",
    "message": "Đăng ký thành công! Vui lòng kiểm tra email để xác thực tài khoản.",
    "requiresEmailVerification": true,
    "verificationCode": "123456",  
    "expiresInMinutes": 15
  }
}
```

**Response (Email đã tồn tại):**
```json
{
  "success": true,
  "data": {
    "userId": 0,
    "email": "test@example.com",
    "message": "Email đã được sử dụng",
    "requiresEmailVerification": false
  }
}
```

**Logic:**
- UserName và Email phải unique
- Password phải đáp ứng PasswordComplexity (chữ hoa, chữ thường, số, ký tự đặc biệt)
- Tài khoản được tạo với `IsActive = false`, `IsEmailVerified = false`
- Mã xác thực 6 số được tạo và hash (SHA256) trước khi lưu DB
- Email được gửi với mã xác thực (hoặc log trong dev mode)
- Mã có hiệu lực **15 phút**

---

### 2️⃣ Xác thực email (Verify Email)

**Endpoint:** `POST /api/auth/verify-email`

**Request:**
```json
{
  "email": "test@example.com",
  "verificationCode": "123456"
}
```

**Response (Success):**
```json
{
  "success": true,
  "data": {
    "success": true,
    "message": "Xác thực email thành công! Bạn có thể đăng nhập ngay.",
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."  
  }
}
```

**Response (Mã sai hoặc hết hạn):**
```json
{
  "success": false,
  "data": {
    "success": false,
    "message": "Mã xác thực không đúng hoặc đã hết hạn."
  }
}
```

**Logic:**
- Kiểm tra mã xác thực (so sánh hash)
- Kiểm tra expiry time (15 phút)
- Khi thành công:
  - `IsEmailVerified = true`
  - `IsActive = true`
  - `EmailVerifiedAt = DateTime.UtcNow`
  - Clear mã xác thực
  - Trả JWT token để auto-login

---

### 3️⃣ Gửi lại mã xác thực (Resend Verification)

**Endpoint:** `POST /api/auth/resend-verification`

**Request:**
```json
{
  "email": "test@example.com"
}
```

**Response (Success):**
```json
{
  "success": true,
  "data": {
    "success": true,
    "message": "Mã xác thực mới đã được gửi đến email của bạn.",
    "verificationCode": "654321",  
    "expiresInMinutes": 15
  }
}
```

**Rate Limit:**
- Tối đa **5 lần/ngày** mỗi email
- Count reset vào 00:00 UTC mỗi ngày

---

### 4️⃣ Đăng nhập (Login)

**Endpoint:** `POST /api/auth/login`

**Request:**
```json
{
  "username": "testuser",
  "password": "Abc@12345"
}
```

**Response (Success):**
```json
{
  "success": true,
  "data": {
    "id": 123,
    "username": "testuser",
    "fullName": "Nguyễn Văn A",
    "phoneNumber": "0123456789",
    "role": {
      "id": 2,
      "code": "CUSTOMER",
      "name": "Khách hàng"
    },
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
  }
}
```

**Response (Email chưa verify):**
```json
{
  "success": false,
  "message": "Vui lòng xác thực email trước khi đăng nhập"
}
```

**Validation:**
- ✅ UserName & password đúng
- ✅ Email đã verified (`IsEmailVerified = true`)
- ✅ Tài khoản active (`IsActive = true`)
- ✅ Cập nhật `LastLoginAt`

---

## 🔐 Flow quên mật khẩu

### 5️⃣ Yêu cầu reset mật khẩu (Forgot Password)

**Endpoint:** `POST /api/auth/forgot-password`

**Request:**
```json
{
  "emailOrUsername": "test@example.com"
}
```

**Response (Success):**
```json
{
  "success": true,
  "data": {
    "success": true,
    "message": "Mã reset password đã được gửi đến email của bạn.",
    "resetToken": "789012", 
    "expiresInMinutes": 30
  }
}
```

**Yêu cầu:**
- Email phải đã verified (`IsEmailVerified = true`)
- Tài khoản phải active (`IsActive = true`)

**Rate Limit:**
- Tối đa **5 lần/ngày** mỗi tài khoản

**Logic:**
- Tạo mã reset 6 số
- Hash (SHA256) trước khi lưu DB
- Gửi email với mã reset
- Mã có hiệu lực **30 phút**

---

### 6️⃣ Reset mật khẩu (Reset Password)

**Endpoint:** `POST /api/auth/reset-password`

**Request:**
```json
{
  "emailOrUsername": "test@example.com",
  "resetToken": "789012",
  "newPassword": "NewPass@123",
  "confirmPassword": "NewPass@123"
}
```

**Response (Success):**
```json
{
  "success": true,
  "data": {
    "success": true,
    "message": "Đổi mật khẩu thành công. Vui lòng đăng nhập với mật khẩu mới."
  }
}
```

**Logic:**
- Validate mã reset (so sánh hash)
- Kiểm tra expiry (30 phút)
- Hash mật khẩu mới (BCrypt)
- Clear reset token
- Gửi email thông báo password đã đổi

---

### 7️⃣ Kiểm tra mã reset (Validate Reset Token)

**Endpoint:** `POST /api/auth/validate-reset-token`

**Request:**
```json
{
  "emailOrUsername": "test@example.com",
  "resetToken": "789012"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "isValid": true
  },
  "message": "Mã hợp lệ"
}
```

---

## 🛡️ Bảo mật

### Token Hashing
- Tất cả mã xác thực và reset token được hash SHA256 trước khi lưu DB
- Nếu DB bị lộ, attacker không thể lấy được mã gốc

### Rate Limiting
| Action | Limit | Reset |
|--------|-------|-------|
| Email Verification | 5/ngày | 00:00 UTC |
| Password Reset | 5/ngày | 00:00 UTC |

### Validation Chain
1. ✅ Email format (EmailAddress attribute)
2. ✅ Password complexity (PasswordComplexity attribute)
3. ✅ Email verified before login
4. ✅ Account active before login
5. ✅ Code expiry check
6. ✅ Hash comparison (không compare plain text)

---

## ⚙️ Cấu hình

### appsettings.json
```json
{
  "EmailSettings": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-email@gmail.com",
    "SmtpPassword": "your-app-password",
    "FromEmail": "your-email@gmail.com",
    "FromName": "CoffeeShop",
    "EnableSsl": true
  },
  "IsDevelopment": true 
}
```

### Production Setup
1. Đổi `UseFakeEmail = false`
2. Đổi `IsDevelopment = false`
3. Cung cấp SMTP credentials thật
4. Dùng Secret Manager hoặc Environment Variables cho credentials

---

## 🧪 Testing Flow (Dev Mode)

### 1. Register
```bash
POST /api/auth/register
Body: { username, email, password, fullName }
Response: { verificationCode: "123456" }
```

### 2. Verify
```bash
POST /api/auth/verify-email
Body: { email, verificationCode: "123456" }
Response: { token: "..." }
```

### 3. Login
```bash
POST /api/auth/login
Body: { username, password }
Response: { token: "..." }
```

### 4. Forgot Password
```bash
POST /api/auth/forgot-password
Body: { emailOrUsername }
Response: { resetToken: "789012" }
```

### 5. Reset Password
```bash
POST /api/auth/reset-password
Body: { emailOrUsername, resetToken: "789012", newPassword, confirmPassword }
Response: { success: true }
```

---

## 📊 Database Schema

### User Table - New Fields
```sql
-- Email Verification
IsEmailVerified BIT DEFAULT 0
EmailVerifiedAt DATETIME2 NULL
EmailVerificationCode NVARCHAR(256) NULL  -- SHA256 hash
EmailVerificationCodeExpiry DATETIME2 NULL
EmailVerificationRequestCount INT DEFAULT 0
LastEmailVerificationRequest DATETIME2 NULL

-- Password Reset
PasswordResetToken NVARCHAR(256) NULL  -- SHA256 hash
PasswordResetTokenExpiry DATETIME2 NULL
PasswordResetRequestCount INT DEFAULT 0
LastPasswordResetRequest DATETIME2 NULL
```

---

## 🚀 Migration Command

```bash
# Tạo migration
dotnet ef migrations add AddUserEmailAndResetFields

# Áp dụng migration
dotnet ef database update
```

---

## 📝 Notes

### Dev Mode vs Production
- **Dev Mode** (`IsDevelopment = true`, `UseFakeEmail = true`):
  - Mã trả về trong response
  - Email log ra console
  - Dễ test không cần SMTP thật

- **Production** (`IsDevelopment = false`, `UseFakeEmail = false`):
  - Mã KHÔNG trả trong response
  - Email gửi thật qua SMTP
  - Response chỉ có `{ success: true, message: "..." }`

### Expiry Times
- Email Verification: **15 phút**
- Password Reset: **30 phút**
- JWT Token: **8 giờ**

### Rate Limits
- Email Verification: **5 lần/ngày**
- Password Reset: **5 lần/ngày**
- Count reset: **00:00 UTC**

### TODO (Future)
- [ ] Token Blacklist (Redis) để logout all sessions sau đổi password
- [ ] Email templates với branding đẹp hơn
- [ ] SMS verification (2FA)
- [ ] reCAPTCHA cho register/forgot-password
- [ ] Audit logging cho email events
- [ ] Admin dashboard để xem email metrics

---

## 🆘 Troubleshooting

### Lỗi: "Email đã được sử dụng"
- Kiểm tra DB: `SELECT * FROM Users WHERE Email = 'test@example.com'`
- Xóa user cũ hoặc dùng email khác

### Lỗi: "Mã xác thực không đúng hoặc đã hết hạn"
- Dev mode: copy mã từ response/console log
- Kiểm tra expiry: mã chỉ có hiệu lực 15 phút
- Dùng resend để lấy mã mới

[//]: # (### Lỗi: "Vui lòng xác thực email trước khi đăng nhập")

[//]: # (- Phải verify email trước)

[//]: # (- Check DB: `SELECT IsEmailVerified FROM Users WHERE UserName = '...'`)

### Email không nhận được (Production)
- Kiểm tra SMTP settings trong appsettings.json
- Kiểm tra spam folder
- Kiểm tra SMTP credentials (Gmail App Password, không phải password thường)
- Check logs: `grep -i "email" logs/*.log`

---

**Tác giả:** CoffeeShop Development Team  
**Version:** 1.0  
**Last Updated:** 2024
