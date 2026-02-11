# ⚠️ Database Reset Issue - Nguyên nhân và Giải pháp

## 🔍 Vấn đề

**Hiện tượng:** Mỗi khi run lại app, database bị reset hoặc có lỗi duplicate data.

**Nguyên nhân:**

### 1. `DbInitializer.InitializeAsync()` chạy mỗi lần start app

**File:** `Program.cs` (Line 208)

```csharp
await DbInitializer.InitializeAsync(context);
```

→ Code này chạy **MỌI LẦN** app khởi động.

### 2. `ProductSeeder` kiểm tra không đầy đủ

**File:** `ProductSeeder.cs` (Line 12-15)

```csharp
if (await context.Products.AnyAsync())
{
    return; // ← CHỈ CHECK PRODUCTS
}
```

**Vấn đề:**
- ✅ Nếu DB có Products → Skip seeding (OK)
- ❌ Nếu Products bị xóa nhưng OptionGroups/OptionItems vẫn còn → Cố tạo lại OptionGroups → **Lỗi duplicate** hoặc **conflict**

### 3. Identity Seed Reset

**File:** `DbInitializer.cs` (Line 36-44)

```csharp
var sql = $@"
    DECLARE @maxId INT;
    SELECT @maxId = ISNULL(MAX(Id), 0) FROM {table};
    IF @maxId > 0
        DBCC CHECKIDENT ('{table}', RESEED, @maxId);
    ELSE
        DBCC CHECKIDENT ('{table}', RESEED, 1);
";
context.Database.ExecuteSqlRaw(sql);
```

→ Code này **RESET IDENTITY** của tất cả bảng mỗi lần start app.

**Vấn đề:**
- Nếu có data trong DB, identity sẽ bị reseed → **Có thể gây conflict**
- Không cần thiết phải reset identity mỗi lần start

---

## 🛠️ Giải pháp

### Giải pháp 1: Tắt DbInitializer (Recommended cho Production)

**File:** `Program.cs`

```csharp
// ❌ Tắt seeding trong production
// using (var scope = app.Services.CreateScope())
// {
//     var services = scope.ServiceProvider;
//     var context = services.GetRequiredService<AppDbContext>();
//     await DbInitializer.InitializeAsync(context);
// }

// ✅ Chỉ chạy khi Development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    
    // Chỉ seed nếu DB rỗng
    if (!await context.Products.AnyAsync())
    {
        await DbInitializer.InitializeAsync(context);
    }
}
```

### Giải pháp 2: Fix ProductSeeder để kiểm tra đầy đủ

**File:** `ProductSeeder.cs`

```csharp
public static async Task SeedProductsWithOptions(AppDbContext context)
{
    // ✅ Kiểm tra cả Products VÀ OptionGroups
    if (await context.Products.AnyAsync() || await context.OptionGroups.AnyAsync())
    {
        Console.WriteLine("⏭️  Database already seeded. Skipping...");
        return;
    }

    Console.WriteLine("🌱 Seeding database...");
    // ... rest of code
}
```

### Giải pháp 3: Xóa DBCC CHECKIDENT (Recommended)

**File:** `DbInitializer.cs`

```csharp
public static async Task InitializeAsync(AppDbContext context)
{
    // ❌ XÓA PHẦN NÀY - Không cần reset identity
    // try 
    // {
    //     var tables = new[] { "Users", "Roles", ... };
    //     foreach (var table in tables)
    //     {
    //         context.Database.ExecuteSqlRaw($"DBCC CHECKIDENT ...");
    //     }
    // }

    // ✅ CHỈ GIỮ PHẦN SEEDING
    if (!await context.Products.AnyAsync())
    {
        await ProductSeeder.SeedProductsWithOptions(context);
    }

    if (!await context.Orders.AnyAsync())
    {
        await OrderSeeder.SeedSampleOrders(context);
    }

    // ... rest
}
```

### Giải pháp 4: Sử dụng Migration Seed thay vì Runtime Seed

**Tạo migration riêng cho seed data:**

```bash
dotnet ef migrations add SeedInitialData
```

**File migration:**

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // Seed OptionGroups
    migrationBuilder.InsertData(
        table: "OptionGroups",
        columns: new[] { "Name", "Description", "IsRequired", ... },
        values: new object[] { "Kích cỡ", "Kích cỡ sản phẩm", true, ... }
    );

    // Seed OptionItems
    // Seed Products
    // ...
}
```

**Ưu điểm:**
- ✅ Chỉ chạy 1 lần khi migrate
- ✅ Không reset mỗi lần start app
- ✅ Version control cho seed data

---

## 🎯 Recommended Solution (Quick Fix)

### Bước 1: Sửa Program.cs

```csharp
// Chỉ seed khi Development VÀ DB rỗng
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    
    try
    {
        // CHỈ CHẠY NẾU DB RỖNG
        if (!await context.Products.AnyAsync() && !await context.OptionGroups.AnyAsync())
        {
            Console.WriteLine("🌱 Seeding database for the first time...");
            await DbInitializer.InitializeAsync(context);
        }
        else
        {
            Console.WriteLine("✓ Database already contains data. Skipping seed.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error initializing database: {ex.Message}");
    }
}
```

### Bước 2: Sửa DbInitializer.cs

```csharp
public static async Task InitializeAsync(AppDbContext context)
{
    // ❌ XÓA TOÀN BỘ PHẦN DBCC CHECKIDENT
    // Không cần reset identity

    // ✅ CHỈ GIỮ PHẦN SEED DATA
    Console.WriteLine("🔧 Seeding OptionGroups...");
    await ProductSeeder.SeedProductsWithOptions(context);

    Console.WriteLine("📦 Seeding Orders...");
    await OrderSeeder.SeedSampleOrders(context);

    Console.WriteLine("📍 Seeding UserAddresses...");
    await UserAddressSeeder.SeedSampleAddresses(context);

    Console.WriteLine("🎟️ Seeding Vouchers...");
    await VoucherSeeder.SeedSampleVouchers(context);

    Console.WriteLine("✅ Database seeding completed!");
}
```

### Bước 3: Sửa ProductSeeder.cs

```csharp
public static async Task SeedProductsWithOptions(AppDbContext context)
{
    // ✅ Kiểm tra kỹ hơn
    if (await context.Products.AnyAsync() || await context.OptionGroups.AnyAsync())
    {
        Console.WriteLine("⏭️  Data already exists. Skipping ProductSeeder.");
        return;
    }

    Console.WriteLine("🔧 Creating OptionGroup templates...");
    // ... rest of code
}
```

---

## 🧹 Clean Database (Nếu bị lỗi)

Nếu database đã bị lỗi do duplicate data, chạy script sau:

```sql
-- Xóa tất cả data (giữ lại schema)
DELETE FROM OrderItemOptions;
DELETE FROM OrderItems;
DELETE FROM Orders;
DELETE FROM ProductOptionGroups;
DELETE FROM OptionItems;
DELETE FROM OptionGroups;
DELETE FROM Products;
DELETE FROM Categories;
DELETE FROM UserVouchers;
DELETE FROM VoucherUsages;
DELETE FROM Vouchers;
DELETE FROM UserAddresses;

-- Reset identity
DBCC CHECKIDENT ('OptionGroups', RESEED, 0);
DBCC CHECKIDENT ('OptionItems', RESEED, 0);
DBCC CHECKIDENT ('Products', RESEED, 0);
DBCC CHECKIDENT ('Orders', RESEED, 0);
DBCC CHECKIDENT ('OrderItems', RESEED, 0);
DBCC CHECKIDENT ('Vouchers', RESEED, 0);

-- Sau đó run lại app để seed từ đầu
```

---

## 📊 Workflow lý tưởng

```
1. Development:
   App start → Check if DB empty → Seed nếu rỗng → Skip nếu có data

2. Staging/Production:
   App start → KHÔNG SEED → Sử dụng data thật

3. Seed data mới:
   Tạo migration → dotnet ef database update → Data được seed 1 lần duy nhất
```

---

## ✅ Checklist

- [ ] Tắt DBCC CHECKIDENT trong DbInitializer
- [ ] Chỉ seed khi Development
- [ ] Kiểm tra DB rỗng trước khi seed
- [ ] Sử dụng Migration cho seed data (long-term)
- [ ] Xóa `DbInitializer.InitializeAsync()` trong Program.cs (Production)

---

## 🚀 Next Steps

1. ✅ Apply fix ngay (Giải pháp 1 + 2 + 3)
2. ✅ Test lại app
3. ✅ Chuyển sang Migration seed cho stable data (Optional)
4. ✅ Disable seeding hoàn toàn khi deploy Production
