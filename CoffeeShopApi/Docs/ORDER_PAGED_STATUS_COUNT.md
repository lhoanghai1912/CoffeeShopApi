# ✅ Order Paged Endpoints - Status Count Implementation

## 📋 Summary

Đã cập nhật **TẤT CẢ** paged order endpoints để trả về thêm object `count` chứa số lượng đơn hàng theo từng trạng thái (`OrderStatus`).

---

## 🎯 Affected Endpoints

### 1. `GET /api/orders/mine` (Customer - My Orders)

**Authorization:** Required (Bearer Token)

**Response:**
```json
{
  "success": true,
  "data": {
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 3,
    "totalCount": 25,
    "count": {
      "Pending": 3,
      "Confirmed": 5,
      "Paid": 15,
      "Cancelled": 2
    },
    "items": [...]
  }
}
```

**Implementation:**
- Service: `OrderService.GetByUserIdPagedAsync()`
- Filters by: `UserId` (từ JWT token)
- Count logic: Tính cho tất cả orders của user, áp dụng search nhưng KHÔNG áp dụng status filter

---

### 2. `GET /api/orders/paged` (Admin - All Orders)

**Authorization:** Admin/Staff (recommended)

**Response:**
```json
{
  "success": true,
  "data": {
    "pageNumber": 1,
    "pageSize": 20,
    "totalPages": 10,
    "totalCount": 195,
    "count": {
      "Draft": 5,
      "Pending": 25,
      "Confirmed": 30,
      "Delivering": 15,
      "Paid": 100,
      "Completed": 10,
      "Cancelled": 10
    },
    "items": [...]
  }
}
```

**Implementation:**
- Service: `OrderService.GetPagedAsync()`
- Filters by: Tất cả orders trong hệ thống
- Count logic: Tính cho tất cả orders, áp dụng search nhưng KHÔNG áp dụng status filter

---

## 🔧 Implementation Details

### Count Logic Behavior

**Điều kiện áp dụng cho Count:**
- ✅ **Áp dụng:** Search query (order code, user name, phone number, shipping address)
- ❌ **KHÔNG áp dụng:** Status filter (để hiển thị đầy đủ phân bố)

**Example:**
```bash
# Request
GET /api/orders/mine?search=Nguyen&filter=Status=Pending

# Response Count sẽ tính:
# - Tất cả orders của user có search="Nguyen"
# - KHÔNG quan tâm filter=Status=Pending

# count: {
#   "Pending": 3,      <- bao gồm cả orders không phải Pending
#   "Paid": 10,        <- nhưng có search="Nguyen"
#   "Cancelled": 1
# }

# items: [...] <- chỉ chứa Pending orders
```

**Rationale:** 
- UI có thể hiển thị tabs/badges cho từng status
- User nhìn thấy tổng quan phân bố đơn hàng
- Không phụ thuộc vào filter hiện tại

---

## 📊 Use Cases

### 1. Customer - Order History Screen

```typescript
// Hiển thị tabs với count
<Tabs>
  <Tab label={`Chờ xử lý (${count.Pending || 0})`} />
  <Tab label={`Đã thanh toán (${count.Paid || 0})`} />
  <Tab label={`Đã hủy (${count.Cancelled || 0})`} />
</Tabs>
```

### 2. Admin - Order Management Dashboard

```typescript
// Stats cards
<StatsCard title="Đơn mới" count={count.Pending} />
<StatsCard title="Đang giao" count={count.Delivering} />
<StatsCard title="Hoàn thành" count={count.Paid} />
<StatsCard title="Đã hủy" count={count.Cancelled} />
```

---

## 🎨 Frontend Integration

### React Example

```typescript
interface OrderListResponse {
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  totalCount: number;
  count?: Record<string, number>; // Status counts
  items: OrderSummary[];
}

const MyOrdersPage = () => {
  const [orders, setOrders] = useState<OrderListResponse | null>(null);
  const [activeTab, setActiveTab] = useState<string>('all');

  const loadOrders = async (status?: string) => {
    const params = new URLSearchParams({ page: '1', pageSize: '20' });
    if (status && status !== 'all') {
      params.append('filter', `Status=${status}`);
    }

    const response = await fetch(`/api/orders/mine?${params}`, {
      headers: { Authorization: `Bearer ${token}` }
    });
    const result = await response.json();
    setOrders(result.data);
  };

  return (
    <div>
      {/* Tabs with counts */}
      <div className="tabs">
        <button onClick={() => { setActiveTab('all'); loadOrders(); }}>
          Tất cả ({orders?.totalCount || 0})
        </button>
        <button onClick={() => { setActiveTab('Pending'); loadOrders('Pending'); }}>
          Chờ xử lý ({orders?.count?.['Pending'] || 0})
        </button>
        <button onClick={() => { setActiveTab('Paid'); loadOrders('Paid'); }}>
          Đã thanh toán ({orders?.count?.['Paid'] || 0})
        </button>
        <button onClick={() => { setActiveTab('Cancelled'); loadOrders('Cancelled'); }}>
          Đã hủy ({orders?.count?.['Cancelled'] || 0})
        </button>
      </div>

      {/* Order list */}
      <div className="orders">
        {orders?.items.map(order => (
          <OrderCard key={order.id} order={order} />
        ))}
      </div>
    </div>
  );
};
```

### React Native Example

```javascript
const MyOrdersScreen = () => {
  const [counts, setCounts] = useState({});
  const [activeStatus, setActiveStatus] = useState('all');

  const tabs = [
    { key: 'all', label: 'Tất cả', count: counts.total },
    { key: 'Pending', label: 'Chờ xử lý', count: counts['Pending'] },
    { key: 'Paid', label: 'Đã thanh toán', count: counts['Paid'] },
    { key: 'Cancelled', label: 'Đã hủy', count: counts['Cancelled'] }
  ];

  const loadOrders = async (status) => {
    const params = new URLSearchParams({ page: 1, pageSize: 20 });
    if (status !== 'all') {
      params.append('filter', `Status=${status}`);
    }

    const response = await fetch(`${API_URL}/orders/mine?${params}`, {
      headers: { Authorization: `Bearer ${token}` }
    });
    const result = await response.json();
    
    if (result.success) {
      setCounts({ 
        total: result.data.totalCount,
        ...result.data.count 
      });
      setOrders(result.data.items);
    }
  };

  return (
    <View>
      {/* Status tabs */}
      <ScrollView horizontal>
        {tabs.map(tab => (
          <TouchableOpacity
            key={tab.key}
            onPress={() => {
              setActiveStatus(tab.key);
              loadOrders(tab.key);
            }}
          >
            <Text>{tab.label} ({tab.count || 0})</Text>
          </TouchableOpacity>
        ))}
      </ScrollView>

      {/* Order list */}
      <FlatList
        data={orders}
        renderItem={({ item }) => <OrderItem order={item} />}
      />
    </View>
  );
};
```

---

## 🔍 Response Structure

```typescript
interface PaginatedOrderResponse {
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  totalCount: number;
  
  // ⭐ NEW: Status counts
  count?: {
    [status: string]: number;
    // Possible keys:
    // - "Draft"
    // - "Pending"
    // - "Confirmed"
    // - "Delivering"
    // - "Paid"
    // - "Completed"
    // - "Cancelled"
  };
  
  items: OrderSummary[];
}
```

**Notes:**
- `count` là optional (`?`) vì backward compatibility
- Chỉ chứa status có ít nhất 1 order (không có keys với count = 0)
- Keys là string (enum name), values là số lượng

---

## 📝 Files Changed

### 1. `CoffeeShopApi\DTOs\Paginated.cs`
```csharp
public class PaginatedResponse<T>
{
    // ... existing properties
    
    /// <summary>
    /// Count per status or custom groups
    /// </summary>
    public Dictionary<string, int>? Count { get; set; }
}
```

### 2. `CoffeeShopApi\Services\OrderService.cs`

**GetByUserIdPagedAsync (Customer endpoint):**
```csharp
// Compute counts per status (respecting search, ignoring filter)
var statusCounts = await countQuery
    .GroupBy(o => o.Status)
    .Select(g => new { Status = g.Key, Count = g.Count() })
    .ToListAsync();

var countDict = statusCounts.ToDictionary(
    k => k.Status.ToString(), 
    v => v.Count);

var response = new PaginatedResponse<OrderSummaryResponse>(...)
{
    Count = countDict
};
```

**GetPagedAsync (Admin endpoint):**
```csharp
// Same logic as above but for all orders
```

---

## ✅ Checklist

- [x] Update `PaginatedResponse<T>` with `Count` property
- [x] Implement count logic in `GetByUserIdPagedAsync` (customer)
- [x] Implement count logic in `GetPagedAsync` (admin)
- [x] Count respects search but ignores status filter
- [x] Test compilation
- [x] Update documentation
- [x] Add frontend examples

---

## 🚀 Testing

### Test Count với Search

```bash
# Test 1: No filter - should return all status counts
curl -X GET "http://localhost:1912/api/orders/mine?page=1&pageSize=10" \
  -H "Authorization: Bearer <token>"

# Expected: count: { "Pending": 3, "Paid": 10, "Cancelled": 1 }

# Test 2: With status filter - count should still show all statuses
curl -X GET "http://localhost:1912/api/orders/mine?filter=Status=Pending" \
  -H "Authorization: Bearer <token>"

# Expected: 
# count: { "Pending": 3, "Paid": 10, "Cancelled": 1 }  <- all statuses
# items: [...only Pending orders...]                    <- filtered items

# Test 3: With search - count should respect search
curl -X GET "http://localhost:1912/api/orders/mine?search=Nguyen" \
  -H "Authorization: Bearer <token>"

# Expected: count only for orders matching "Nguyen"
```

---

## 🎯 Benefits

1. **Better UX:** Users nhìn thấy tổng quan phân bố đơn hàng
2. **Reduce API Calls:** Không cần gọi riêng API để lấy count cho từng tab
3. **Consistent:** Tất cả paged endpoints đều có count
4. **Flexible:** Count logic có thể mở rộng cho các grouping khác (by date, by total amount, etc.)

---

**Status:** ✅ Production Ready
