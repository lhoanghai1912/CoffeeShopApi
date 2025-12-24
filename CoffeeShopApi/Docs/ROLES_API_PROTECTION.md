# B?O V? ROLES API - CH? ADMIN

## ? ?Ã TH?C HI?N

### 1. C?p nh?t RolesController

**File:** `CoffeeShopApi/Controllers/RolesController.cs`

```csharp
[Route("api/[controller]")]
[ApiController]
[Authorize] // ? B?t bu?c ph?i login
public class RolesController : ControllerBase
{
    [HttpGet]
    [RequirePermission("role.manage")] // ? Ch? ADMIN
    public async Task<IActionResult> GetAll() { }

    [HttpGet("{id}")]
    [RequirePermission("role.manage")] // ? Ch? ADMIN
    public async Task<IActionResult> GetById(int id) { }

    [HttpPost]
    [RequirePermission("role.manage")] // ? Ch? ADMIN
    public async Task<IActionResult> Create(CreateRoleRequest request) { }

    [HttpPut("{id}")]
    [RequirePermission("role.manage")] // ? Ch? ADMIN
    public async Task<IActionResult> Update(int id, UpdateRoleRequest request) { }

    [HttpDelete("{id}")]
    [RequirePermission("role.manage")] // ? Ch? ADMIN
    public async Task<IActionResult> Delete(int id) { }
}
```

### 2. Permission Matrix

| Endpoint | ADMIN | STAFF | CUSTOMER | GUEST |
|----------|-------|-------|----------|-------|
| GET /api/roles | ? | ? | ? | ? |
| GET /api/roles/{id} | ? | ? | ? | ? |
| POST /api/roles | ? | ? | ? | ? |
| PUT /api/roles/{id} | ? | ? | ? | ? |
| DELETE /api/roles/{id} | ? | ? | ? | ? |

### 3. Permission ?ã Có

Permission `role.manage` (ID: 21) ?ã ???c seed trong `001_AddPermissionSystem.sql` và ch? ADMIN (RoleId = 1) có quy?n này.

## ?? KI?M TRA

### Test 1: Guest (Không có token) - FAIL
```bash
POST http://localhost:5000/api/roles
Content-Type: application/json

{
  "code": "NEWROLE",
  "name": "New Role"
}

# ? Response: 401 Unauthorized
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401
}
```

### Test 2: Customer Token - FAIL
```bash
# 1. Login as customer
POST http://localhost:5000/api/auth/login
{
  "username": "customer1",
  "password": "pass123"
}

# 2. Try to create role
POST http://localhost:5000/api/roles
Authorization: Bearer <customer_token>
Content-Type: application/json

{
  "code": "NEWROLE",
  "name": "New Role"
}

# ? Response: 403 Forbidden
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",
  "title": "Forbidden",
  "status": 403
}
```

### Test 3: Staff Token - FAIL
```bash
# 1. Login as staff
POST http://localhost:5000/api/auth/login
{
  "username": "staff1",
  "password": "staff123"
}

# 2. Try to create role
POST http://localhost:5000/api/roles
Authorization: Bearer <staff_token>
Content-Type: application/json

{
  "code": "NEWROLE",
  "name": "New Role"
}

# ? Response: 403 Forbidden
```

### Test 4: Admin Token - SUCCESS ?
```bash
# 1. Login as admin
POST http://localhost:5000/api/auth/login
{
  "username": "admin",
  "password": "admin123"
}

# Response:
{
  "success": true,
  "data": {
    "id": 1,
    "username": "admin",
    "role": {
      "code": "ADMIN",
      "name": "Admin"
    },
    "token": "eyJhbGciOi..."
  }
}

# 2. Create role with admin token
POST http://localhost:5000/api/roles
Authorization: Bearer <admin_token>
Content-Type: application/json

{
  "code": "MANAGER",
  "name": "Manager"
}

# ? Response: 200 OK
{
  "success": true,
  "message": "Success",
  "data": {
    "id": 4,
    "code": "MANAGER",
    "name": "Manager"
  }
}
```

## ?? B?O M?T ?Ã ???C ??M B?O

### Các L?p B?o V?:
1. **Authentication Layer** - `[Authorize]`
   - B?t bu?c ph?i có JWT token h?p l?
   - Không có token ? 401 Unauthorized

2. **Authorization Layer** - `[RequirePermission("role.manage")]`
   - Ki?m tra user có permission `role.manage` trong JWT claims
   - Không có permission ? 403 Forbidden

3. **Database Layer**
   - Ch? ADMIN role có `role.manage` permission trong b?ng `RolePermissions`

### Lu?ng Ki?m Tra:
```
Request
  ?
1. JWT Middleware (Authentication)
   - Ki?m tra token có h?p l? không?
   - Parse claims t? token
   ?
2. Authorization Middleware
   - Ki?m tra claim "permissions" có ch?a "role.manage"?
   ?
3. Controller Action
   - Th?c thi logic business
   ?
Response
```

## ?? CHECKLIST

- [x] Thêm `[Authorize]` vào RolesController
- [x] Thêm `[RequirePermission("role.manage")]` cho t?t c? actions
- [x] Permission `role.manage` ?ã ???c seed
- [x] Ch? ADMIN có permission `role.manage`
- [x] Build thành công (code không l?i syntax)

## ?? TI?P THEO

**Restart application ?? áp d?ng thay ??i, sau ?ó test theo các scenarios ? trên.**

?? test nhanh b?ng Swagger/Scalar:
1. M? http://localhost:5000/swagger
2. Click "Authorize" và nh?p token admin
3. Th? POST /api/roles ? S? thành công
4. Logout và th? l?i không có token ? S? fail 401
5. Login v?i customer token và th? ? S? fail 403
