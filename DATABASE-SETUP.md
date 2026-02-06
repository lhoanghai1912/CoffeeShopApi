# 🛠️ HƯỚNG DẪN SETUP DATABASE

## ⚠️ VẤN ĐỀ: Data bị reset mỗi lần chạy API

**Nguyên nhân:**
- Migration `RefactorOptionGroupsToTemplates` thay đổi schema lớn (drop columns `ProductId`, `FatherId`)
- Khi drop columns, data cũ bị xóa do cascade constraints
- `context.Database.Migrate()` trong `Program.cs` chạy mỗi lần start app → re-apply migration → mất data

**Giải pháp đã áp dụng:**
✅ **Đã TẮT auto-migrate trong `Program.cs`**
- Migration chỉ chạy khi bạn CHẠY THỦ CÔNG
- Data sẽ KHÔNG bị reset mỗi lần start app

---

## 🚀 SETUP LẦN ĐẦU (hoặc khi cần reset)

### Cách 1: Dùng script PowerShell (Khuyên dùng)

```powershell
# Chạy trong PowerShell tại thư mục gốc của project
.\setup-database.ps1
```

Script sẽ tự động:
1. Xóa database cũ
2. Apply tất cả migrations
3. Tạo database mới với schema mới

### Cách 2: Chạy thủ công

```bash
cd CoffeeShopApi

# Xóa database cũ
dotnet ef database drop --force

# Apply migrations
dotnet ef database update
```

---

## 🏃 CHẠY API VÀ SEED DATA

Sau khi setup database:

```bash
cd CoffeeShopApi
dotnet run
```

**Lần chạy đầu tiên:**
- API sẽ tự động seed data (ProductSeeder, OrderSeeder, VoucherSeeder...)
- Kiểm tra console log để thấy quá trình seed

**Lần chạy tiếp theo:**
- ✅ Data **KHÔNG bị reset** (vì đã tắt auto-migrate)
- Seeder có check `if (await context.Products.AnyAsync())` nên không tạo duplicate

---

## 📊 KẾT QUẢ SAU REFACTOR

### Trước (schema cũ):
```
- ~102 OptionGroups (duplicate cho mỗi product)
- ~450 OptionItems (duplicate cho mỗi product)
```

### Sau (schema mới - Template-based):
```
- 4 OptionGroups (templates tái sử dụng)
- 18 OptionItems
- ~102 ProductOptionGroups (mappings)
```

→ **Giảm 96% data duplication!**

---

## 🔄 KHI NÀO CẦN CHẠY LẠI SETUP?

Chỉ chạy lại `setup-database.ps1` khi:
- ❌ Database bị lỗi không sửa được
- ❌ Muốn reset toàn bộ data về trạng thái ban đầu
- ❌ Có migration mới thay đổi schema lớn

**KHÔNG** cần chạy lại khi:
- ✅ Chỉ restart API bình thường
- ✅ Đang develop và test API
- ✅ Update code logic (không đổi database schema)

---

## 🆘 XỬ LÝ LỖI

### Lỗi: "Database already exists"
```bash
# Xóa database thủ công
dotnet ef database drop --force
dotnet ef database update
```

### Lỗi: "Migration not found"
```bash
# Xem danh sách migrations
dotnet ef migrations list

# Xóa migration cuối cùng (nếu cần)
dotnet ef migrations remove
```

### Lỗi: "Cannot drop database because it is currently in use"
```bash
# Dừng tất cả process đang dùng database (Visual Studio, API running...)
# Sau đó chạy lại setup-database.ps1
```

---

## 📝 NOTES CHO DEVELOPER

### Khi tạo Migration mới:
```bash
dotnet ef migrations add YourMigrationName
```

### Khi cần rollback migration:
```bash
# Rollback về migration trước đó
dotnet ef database update PreviousMigrationName

# Xóa migration chưa apply
dotnet ef migrations remove
```

### Production deployment:
```bash
# KHÔNG dùng auto-migrate trong production
# Apply migrations thủ công với backup trước:
dotnet ef database update --connection "ProductionConnectionString"
```

---

## ✅ CHECKLIST SAU KHI SETUP

- [ ] Database được tạo thành công
- [ ] Có 4 OptionGroups trong DB
- [ ] Có 18 OptionItems
- [ ] Có 30 Products
- [ ] Có ~102 ProductOptionGroups mappings
- [ ] API chạy không lỗi
- [ ] Restart API → Data **KHÔNG** bị reset

---

**Tạo bởi:** Refactor OptionGroup to Template-based System
**Ngày:** 2025-01-06
