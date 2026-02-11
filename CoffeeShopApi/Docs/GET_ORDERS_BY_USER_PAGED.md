# Get My Orders (Paged) API

## ⭐ New Endpoint

**Endpoint:** `GET /api/orders/mine`

**Purpose:** Lấy danh sách đơn hàng của user hiện tại (từ JWT) với pagination, search, filter và thống kê số lượng theo trạng thái

**Authorization:** Required (Bearer token)

---

## 📡 API Specification

### Request

**Method:** GET

**URL:** `/api/orders/mine`

**Query Parameters:**
- `page` (int, optional, default=1) - Trang hiện tại
- `pageSize` (int, optional, default=10) - Số items mỗi trang
- `search` (string, optional) - Tìm kiếm theo order code, recipient name, shipping address
- `orderBy` (string, optional) - Sắp xếp (asc/desc), mặc định: CreatedAt desc (mới nhất trước)
-- `filter` (string, optional) - Filter theo status (Status=Pending, Status=Confirmed, etc.)

---

## 📋 Examples

### 1. Basic - Lấy trang đầu

```bash
curl -X GET "http://localhost:1912/api/orders/mine?page=1&pageSize=10" \
  -H "Authorization: Bearer <token>"
```

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
      "Paid": 15,
      "Cancelled": 2
    },
    "items": [
      {
        "id": 15,
        "orderCode": "20250201001",
        "status": "Pending",
        "finalAmount": 120000,
        "totalItems": 3,
        "createdAt": "2025-02-01T10:30:00Z",
        "items": [
          {
            "id": 1,
            "productId": 2,
            "productName": "Cà phê sữa",
            "quantity": 2,
            "unitPrice": 35000,
            "totalPrice": 70000
          }
        ]
      }
    ]
  }
}
```

---

### 2. Search - Tìm theo mã đơn hoặc địa chỉ

```bash
curl -X GET "http://localhost:1912/api/orders/mine?search=20250201" \
  -H "Authorization: Bearer <token>"
```

**Result:** Trả về các đơn có order code chứa "20250201"

---

### 3. Filter - Lọc theo trạng thái

```bash
curl -X GET "http://localhost:1912/api/orders/mine?filter=Status=Pending" \
  -H "Authorization: Bearer <token>"
```

**Result:** Chỉ trả về orders đang Pending

**Available Status Values:**
- `Draft` - Đơn nháp
- `Pending` - Chờ xác nhận
- `Confirmed` - Đã xác nhận
- `Paid` - Đã thanh toán
- `Cancelled` - Đã hủy

---

### 4. Sắp xếp

```bash
# Cũ nhất trước
curl -X GET "http://localhost:1912/api/orders/mine?orderBy=asc" -H "Authorization: Bearer <token>"

# Mới nhất trước (mặc định)
curl -X GET "http://localhost:1912/api/orders/mine?orderBy=desc" -H "Authorization: Bearer <token>"
```

---

### 5. Kết hợp tất cả

```bash
curl -X GET "http://localhost:1912/api/orders/mine?page=2&pageSize=5&search=Nguyen&filter=Status=Paid&orderBy=desc" \
  -H "Authorization: Bearer <token>"
```

**Result:** 
- Trang 2
- Mỗi trang 5 items
- Search "Nguyen" trong recipient name/address
- Chỉ orders đã Paid
- Sắp xếp mới nhất trước

---

## 🎨 Frontend Integration

### React/TypeScript Example

```typescript
interface GetOrdersParams {
  userId: number;
  page?: number;
  pageSize?: number;
  search?: string;
  orderBy?: 'asc' | 'desc';
  filter?: string;
}

const getOrders = async (params: GetOrdersParams) => {
  const { userId, page = 1, pageSize = 10, search, orderBy, filter } = params;
  
  const queryParams = new URLSearchParams({
    page: page.toString(),
    pageSize: pageSize.toString()
  });

  if (search) queryParams.append('search', search);
  if (orderBy) queryParams.append('orderBy', orderBy);
  if (filter) queryParams.append('filter', filter);

  const response = await fetch(
    `/api/orders/user/${userId}/paged?${queryParams}`,
    {
      headers: {
        'Authorization': `Bearer ${token}`
      }
    }
  );

  return await response.json();
};

// Usage
const MyOrdersPage = ({ userId }) => {
  const [orders, setOrders] = useState([]);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(0);
  const [search, setSearch] = useState('');
  const [status, setStatus] = useState('');

  useEffect(() => {
    loadOrders();
  }, [page, search, status]);

  const loadOrders = async () => {
  const result = await fetch(`/api/orders/mine?page=${page}&pageSize=10${search ? `&search=${encodeURIComponent(search)}` : ''}${status ? `&filter=Status=${status}` : ''}`, {
    headers: { Authorization: `Bearer ${token}` }
  }).then(r => r.json());

    if (result.success) {
      setOrders(result.data.items);
      setTotalPages(result.data.totalPages);
    }
  };

  return (
    <div>
      {/* Search */}
      <input 
        type="text" 
        placeholder="Tìm đơn hàng..." 
        value={search}
        onChange={(e) => setSearch(e.target.value)}
      />

      {/* Filter */}
      <select value={status} onChange={(e) => setStatus(e.target.value)}>
        <option value="">Tất cả</option>
        <option value="Pending">Chờ xác nhận</option>
        <option value="Paid">Đã thanh toán</option>
        <option value="Cancelled">Đã hủy</option>
      </select>

      {/* Orders list */}
      {orders.map(order => (
        <OrderCard key={order.id} order={order} />
      ))}

      {/* Pagination */}
      <Pagination 
        current={page} 
        total={totalPages} 
        onChange={setPage} 
      />
    </div>
  );
};
```

---

### React Native Example

```javascript
const MyOrdersScreen = ({ route }) => {
  const { userId } = route.params;
  const [orders, setOrders] = useState([]);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(false);
  const [refreshing, setRefreshing] = useState(false);
  const [hasMore, setHasMore] = useState(true);

  const loadOrders = async (pageNumber = 1, append = false) => {
    try {
      setLoading(true);
      const token = await AsyncStorage.getItem('@auth_token');

      const response = await fetch(
        `http://10.0.2.2:1912/api/orders/user/${userId}/paged?page=${pageNumber}&pageSize=20`,
        {
          headers: {
            'Authorization': `Bearer ${token}`
          }
        }
      );

      const result = await response.json();

      if (result.success) {
        if (append) {
          setOrders([...orders, ...result.data.items]);
        } else {
          setOrders(result.data.items);
        }
        setHasMore(pageNumber < result.data.totalPages);
      }
    } catch (error) {
      console.error('Load orders error:', error);
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  };

  useEffect(() => {
    loadOrders(1);
  }, []);

  const onRefresh = () => {
    setRefreshing(true);
    setPage(1);
    loadOrders(1);
  };

  const loadMore = () => {
    if (!loading && hasMore) {
      const nextPage = page + 1;
      setPage(nextPage);
      loadOrders(nextPage, true);
    }
  };

  return (
    <FlatList
      data={orders}
      keyExtractor={(item) => item.id.toString()}
      renderItem={({ item }) => <OrderItem order={item} />}
      onRefresh={onRefresh}
      refreshing={refreshing}
      onEndReached={loadMore}
      onEndReachedThreshold={0.5}
      ListFooterComponent={loading && <ActivityIndicator />}
    />
  );
};
```

---

## 🔍 Search Behavior

**Searches in:**
- `OrderCode` - Mã đơn hàng
- `RecipientName` - Tên người nhận
- `ShippingAddress` - Địa chỉ giao hàng

**Example:**
```bash
# Tìm tất cả orders có địa chỉ chứa "Nguyen Hue"
GET /api/orders/user/2/paged?search=Nguyen%20Hue

# Tìm order code chứa "20250201"
GET /api/orders/user/2/paged?search=20250201
```

---

## 📊 Response Structure

```typescript
interface PaginatedOrdersResponse {
  success: boolean;
  data: {
    pageNumber: number;
    pageSize: number;
    totalPages: number;
    totalCount: number;
    items: OrderSummary[];
  };
}

interface OrderSummary {
  id: number;
  orderCode: string;
  status: OrderStatus;
  finalAmount: number;
  totalItems: number;
  createdAt: string;
  items: OrderItemResponse[];
}
```

---

## 🎯 Use Cases

### 1. Customer - Xem lịch sử đơn hàng

```bash
GET /api/orders/user/2/paged?page=1&pageSize=20
```

### 2. Customer - Xem đơn đang chờ

```bash
GET /api/orders/user/2/paged?filter=Status=Pending
```

### 3. Customer - Tìm đơn hàng cũ

```bash
GET /api/orders/user/2/paged?search=20250115
```

### 4. Admin - Kiểm tra orders của user

```bash
GET /api/orders/user/123/paged?page=1&pageSize=50
```

---

## 🔒 Security Recommendations

**Hiện tại:** API không có authorization

**Nên thêm:**

```csharp
[HttpGet("user/{userId:int}/paged")]
[Authorize]
public async Task<IActionResult> GetByUserIdPaged(int userId, ...)
{
    // Check ownership
    var currentUserId = GetCurrentUserId();
    if (currentUserId != userId && !IsAdmin())
    {
        return Forbid();
    }
    
    // ... rest of code
}
```

---

## 📄 Related Endpoints

| Endpoint | Purpose | Paged? |
|----------|---------|--------|
| `GET /api/orders/user/{userId}` | Get all orders (no paging) | ❌ |
| `GET /api/orders/user/{userId}/paged` | Get orders with paging ⭐ | ✅ |
| `GET /api/orders/paged` | Get all orders (admin) | ✅ |
| `GET /api/orders/{id}` | Get single order | - |

---

## ✅ Implementation Summary

**Files changed:**
1. ✅ `Services\OrderService.cs` - Added `GetByUserIdPagedAsync`
2. ✅ `Controllers\OrdersController.cs` - Added endpoint

**Features:**
- ✅ Pagination
- ✅ Search (order code, recipient, address)
- ✅ Filter by status
- ✅ Sort by created date (asc/desc)
- ✅ Clean, performant query

**Status:** Production Ready ✅
