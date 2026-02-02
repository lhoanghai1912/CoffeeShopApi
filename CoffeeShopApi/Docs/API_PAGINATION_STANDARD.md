# 📏 API Pagination Standardization Guide

## 🎯 Overview

This document defines the **standard pagination parameters** used across all API endpoints in CoffeeShopApi.

---

## ✅ Standard Pagination Parameters

All paginated endpoints **MUST** use these exact parameters:

```csharp
[HttpGet("Paged")]
public async Task<IActionResult> GetPaged(
    [FromQuery] int page = 1,          // ✅ Required
    [FromQuery] int pageSize = 10,     // ✅ Required
    [FromQuery] string? search = null, // ✅ Optional
    [FromQuery] string? orderBy = null,// ✅ Optional
    [FromQuery] string? filter = null  // ✅ Optional
)
```

### Parameter Details

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `page` | int | Yes | 1 | Page number (1-based) |
| `pageSize` | int | Yes | 10 | Items per page (max: 100) |
| `search` | string? | No | null | Full-text search keyword |
| `orderBy` | string? | No | null | Sort expression (Gridify) |
| `filter` | string? | No | null | Filter expression (Gridify) |

---

## 📊 Standard Response Format

All paginated responses **MUST** use `PaginatedResponse<T>`:

```json
{
  "success": true,
  "data": {
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 5,
    "totalCount": 47,
    "items": [...]
  }
}
```

### Response Properties

| Property | Type | Description |
|----------|------|-------------|
| `pageNumber` | int | Current page (same as request `page`) |
| `pageSize` | int | Items per page (same as request) |
| `totalPages` | int | Total number of pages |
| `totalCount` | int | Total items matching filters |
| `items` | T[] | Array of items on current page |

---

## 🔍 Filter Syntax (Gridify)

### Basic Filters

```bash
# Equality
filter=IsActive=true
filter=CategoryId=1

# Comparison
filter=BasePrice>10000
filter=DiscountValue>=5000
filter=EndDate<2025-12-31

# String operations
filter=Code^=SALE        # StartsWith
filter=Code$=2025        # EndsWith
filter=Description@=gift # Contains
```

### Logical Operators

```bash
# AND (comma)
filter=IsActive=true,IsPublic=true
filter=BasePrice>10000,BasePrice<50000

# OR (pipe)
filter=CategoryId=1|CategoryId=2
filter=Status=Pending|Status=Confirmed
```

### Complex Filters

```bash
# Combine AND + OR (use parentheses)
filter=(CategoryId=1|CategoryId=2),IsActive=true

# Multiple conditions
filter=IsActive=true,IsPublic=false,DiscountValue>10000
```

---

## 🔀 OrderBy Syntax (Gridify)

### Single Field

```bash
# Ascending
orderBy=Name
orderBy=Name asc

# Descending
orderBy=CreatedAt desc
orderBy=BasePrice desc
```

### Multiple Fields

```bash
# Multiple sorts (comma-separated)
orderBy=IsActive desc, CreatedAt desc
orderBy=CategoryId asc, BasePrice desc
```

---

## 🔍 Search Implementation

### Backend Pattern

```csharp
// Apply search across multiple fields
if (!string.IsNullOrWhiteSpace(search))
{
    query = query.Where(x => 
        x.Code.Contains(search) ||
        x.Name.Contains(search) ||
        (x.Description != null && x.Description.Contains(search)));
}
```

### Search is case-sensitive by default
- ✅ Use `.Contains()` for partial match
- ✅ Search across multiple relevant fields
- ❌ Don't search ID fields
- ❌ Don't search date/numeric fields

---

## 📐 Implementation Pattern

### Controller Pattern

```csharp
[HttpGet("Paged")]
public async Task<IActionResult> GetPaged(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] string? search = null,
    [FromQuery] string? orderBy = null,
    [FromQuery] string? filter = null)
{
    var result = await _service.GetPagedAsync(page, pageSize, search, orderBy, filter);
    return Ok(ApiResponse<object>.Ok(result));
}
```

### Service Pattern

```csharp
public async Task<PaginatedResponse<TResponse>> GetPagedAsync(
    int page, 
    int pageSize, 
    string? search, 
    string? orderBy, 
    string? filter)
{
    var query = _context.Entities.AsQueryable();

    // 1. Apply Gridify filter
    if (!string.IsNullOrWhiteSpace(filter))
    {
        query = query.ApplyFiltering(filter);
    }

    // 2. Apply search
    if (!string.IsNullOrWhiteSpace(search))
    {
        query = query.Where(e => 
            e.Name.Contains(search) || 
            (e.Description != null && e.Description.Contains(search)));
    }

    // 3. Apply ordering (with default)
    if (!string.IsNullOrWhiteSpace(orderBy))
    {
        query = query.ApplyOrdering(orderBy);
    }
    else
    {
        query = query.OrderByDescending(e => e.CreatedAt);
    }

    // 4. Get total count
    var totalCount = await query.CountAsync();

    // 5. Apply pagination
    var items = await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    // 6. Map to response
    var mapped = items.Select(MapToResponse).ToList();

    return new PaginatedResponse<TResponse>(mapped, totalCount, page, pageSize);
}
```

---

## 📋 Checklist for New Paginated Endpoints

- [ ] **Route:** Named `"Paged"` (e.g., `/api/products/Paged`)
- [ ] **Parameters:** Use standard 5 parameters (page, pageSize, search, orderBy, filter)
- [ ] **Response:** Return `PaginatedResponse<T>`
- [ ] **Gridify:** Import `using Gridify;`
- [ ] **Filter:** Apply with `query.ApplyFiltering(filter)`
- [ ] **OrderBy:** Apply with `query.ApplyOrdering(orderBy)` + default sort
- [ ] **Search:** Implement across relevant string fields
- [ ] **Count:** Get `totalCount` before pagination
- [ ] **Pagination:** Use `Skip()` and `Take()`
- [ ] **Documentation:** Update module docs with examples

---

## 🎯 Standardized Endpoints

| Endpoint | Route | Status |
|----------|-------|--------|
| **Products** | `GET /api/products/Paged` | ✅ Standardized |
| **Orders** | `GET /api/orders/Paged` | ✅ Standardized |
| **Vouchers** | `GET /api/vouchers/Paged` | ✅ Standardized |
| **Categories** | N/A (too few items) | - |
| **Users** | Future | 🔜 TODO |

---

## 📖 API Examples

### Products

```bash
# Filter by category and price range
GET /api/products/Paged?filter=CategoryId=1,BasePrice>10000&orderBy=BasePrice desc

# Search with sorting
GET /api/products/Paged?search=coffee&orderBy=Name asc
```

### Orders

```bash
# Filter by status and date
GET /api/orders/Paged?filter=Status=Pending,CreatedAt>2025-01-01

# Filter by user
GET /api/orders/Paged?filter=UserId=5&orderBy=CreatedAt desc
```

### Vouchers

```bash
# Filter active public vouchers
GET /api/vouchers/Paged?filter=IsActive=true,IsPublic=true

# High value private vouchers
GET /api/vouchers/Paged?filter=IsPublic=false,DiscountValue>50000&orderBy=DiscountValue desc
```

---

## 🚫 Anti-Patterns (Don't Do This)

### ❌ Using specific filter parameters

```csharp
// ❌ BAD: Specific parameters
public async Task<IActionResult> GetPaged(
    int page,
    int pageSize,
    bool? isActive,    // ← Don't do this
    bool? isPublic,    // ← Don't do this
    string? categoryName // ← Don't do this
)

// ✅ GOOD: Use filter parameter
public async Task<IActionResult> GetPaged(
    int page,
    int pageSize,
    string? filter     // ← Use this: "IsActive=true,IsPublic=false"
)
```

### ❌ Non-standard parameter names

```csharp
// ❌ BAD
pageNumber, size, pageIndex, limit

// ✅ GOOD
page, pageSize
```

### ❌ Non-standard response format

```csharp
// ❌ BAD
{
  "data": [...],
  "total": 100,
  "currentPage": 1
}

// ✅ GOOD
{
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 10,
  "totalCount": 100,
  "items": [...]
}
```

---

## 💡 Best Practices

### 1. Always provide default sorting

```csharp
// ✅ Good: Default sort when orderBy is null
if (!string.IsNullOrWhiteSpace(orderBy))
{
    query = query.ApplyOrdering(orderBy);
}
else
{
    query = query.OrderByDescending(e => e.CreatedAt); // Default
}
```

### 2. Validate pageSize limits

```csharp
// ✅ Good: Prevent excessive page sizes
pageSize = Math.Min(pageSize, 100); // Max 100 items per page
```

### 3. Handle invalid filters gracefully

```csharp
try
{
    query = query.ApplyFiltering(filter);
}
catch (GridifyFilteringException ex)
{
    return BadRequest(ApiResponse<object>.Fail($"Invalid filter: {ex.Message}"));
}
```

### 4. Document filter fields

```markdown
**Filterable Fields:**
- `IsActive` (bool)
- `CategoryId` (int)
- `BasePrice` (decimal)
- `CreatedAt` (DateTime)
```

---

## 📚 References

- **Gridify Documentation:** https://alirezanet.github.io/Gridify/
- **PaginatedResponse Class:** `CoffeeShopApi/DTOs/Paginated.cs`
- **Example Implementation:** `ProductsController.cs`, `OrdersController.cs`

---

## 🔄 Migration Guide

### For existing non-standard endpoints:

1. **Change parameters:**
   ```csharp
   // Before
   bool? isActive, bool? isPublic
   
   // After
   string? filter  // Use: "IsActive=true,IsPublic=false"
   ```

2. **Update service signature**
3. **Replace manual filters with Gridify**
4. **Update documentation**
5. **Notify frontend team of changes**

---

**Last Updated:** January 28, 2025  
**Version:** 1.0.0
