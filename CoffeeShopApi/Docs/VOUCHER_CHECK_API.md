# Voucher Check API - Quick Guide

## ⭐ New Endpoint: Check Voucher by ID

**Endpoint:** `POST /api/vouchers/check`

**Authorization:** Required (Bearer Token)

**Purpose:** Kiểm tra voucher khi người dùng chọn từ danh sách (có sẵn voucherId)

---

## 📋 Request

```json
{
  "voucherId": 2,
  "orderSubTotal": 150000
}
```

### Parameters

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `voucherId` | int | Yes | ID của voucher cần kiểm tra |
| `orderSubTotal` | decimal | Yes | Tổng giá trị đơn hàng (trước khi giảm) |

---

## ✅ Response (Valid Voucher)

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

### Response Fields

| Field | Type | Description |
|-------|------|-------------|
| `isValid` | bool | Voucher có khả dụng không |
| `voucherId` | int | ID voucher |
| `voucherCode` | string | Mã voucher (để hiển thị) |
| `voucherDescription` | string | Mô tả voucher |
| `discountType` | string | "FixedAmount" hoặc "Percentage" |
| `discountValue` | decimal | Giá trị giảm (số tiền hoặc %) |
| `minOrderValue` | decimal? | Giá trị đơn hàng tối thiểu |
| `maxDiscountAmount` | decimal? | Giảm tối đa (cho Percentage) |
| `orderSubTotal` | decimal | Tổng đơn hàng (input) |
| `discountAmount` | decimal | Số tiền được giảm |
| `finalAmount` | decimal | Tổng sau khi giảm |
| `savedAmount` | decimal | Số tiền tiết kiệm được |
| `percentageSaved` | decimal | % tiết kiệm |

---

## ❌ Response (Invalid Voucher)

### Case 1: MinOrderValue không đạt

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

### Case 2: Voucher đã hết hạn

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

### Case 3: Voucher không tồn tại

```json
{
  "success": false,
  "message": "Voucher không tồn tại",
  "status": 404
}
```

### Case 4: User không có quyền dùng (Private Voucher)

```json
{
  "success": true,
  "message": "Bạn không có quyền sử dụng voucher này",
  "data": {
    "isValid": false,
    "errorMessage": "Bạn không có quyền sử dụng voucher này",
    "voucherId": 8,
    "orderSubTotal": 150000,
    "discountAmount": 0,
    "finalAmount": 150000
  }
}
```

---

## 🔍 Validation Rules

Endpoint này sẽ kiểm tra:

1. ✅ **Voucher tồn tại** - HTTP 404 nếu không tìm thấy
2. ✅ **IsActive = true** - Voucher phải đang hoạt động
3. ✅ **Còn hạn** - StartDate <= now <= EndDate
4. ✅ **Còn lượt sử dụng** - UsageLimit chưa đạt
5. ✅ **User chưa vượt quá limit** - UsageLimitPerUser
6. ✅ **Đơn hàng đạt MinOrderValue**
7. ✅ **Private Voucher** - User phải được assign và chưa dùng

---

## 💻 Frontend Integration

### React/TypeScript Example

```typescript
interface CheckVoucherRequest {
  voucherId: number;
  orderSubTotal: number;
}

interface CheckVoucherResponse {
  isValid: boolean;
  errorMessage?: string;
  voucherId: number;
  voucherCode?: string;
  voucherDescription?: string;
  discountType?: string;
  discountValue?: number;
  orderSubTotal: number;
  discountAmount: number;
  finalAmount: number;
  savedAmount?: number;
  percentageSaved?: number;
}

const checkVoucher = async (
  voucherId: number, 
  cartTotal: number
): Promise<CheckVoucherResponse> => {
  const response = await fetch('/api/vouchers/check', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${localStorage.getItem('token')}`
    },
    body: JSON.stringify({
      voucherId,
      orderSubTotal: cartTotal
    })
  });

  const result = await response.json();
  return result.data;
};

// Usage
const handleVoucherSelect = async (voucherId: number) => {
  const cartTotal = calculateCartTotal();
  const result = await checkVoucher(voucherId, cartTotal);

  if (result.isValid) {
    // Hiển thị thông tin giảm giá
    setDiscountAmount(result.discountAmount);
    setFinalAmount(result.finalAmount);
    
    showNotification({
      type: 'success',
      title: 'Voucher áp dụng thành công',
      message: `Bạn tiết kiệm ${result.savedAmount?.toLocaleString()}đ (${result.percentageSaved}%)`
    });
  } else {
    // Hiển thị lỗi
    showNotification({
      type: 'error',
      title: 'Không thể áp dụng voucher',
      message: result.errorMessage
    });
  }
};
```

### React Native Example

```javascript
const checkVoucher = async (voucherId, cartTotal) => {
  try {
    const token = await AsyncStorage.getItem('@auth_token');
    
    const response = await fetch('http://10.0.2.2:1912/api/vouchers/check', {
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
      // Voucher hợp lệ
      Alert.alert(
        'Áp dụng voucher thành công',
        `Tiết kiệm: ${result.data.savedAmount.toLocaleString()}đ\n` +
        `Giảm: ${result.data.percentageSaved}%\n` +
        `Tổng cuối: ${result.data.finalAmount.toLocaleString()}đ`
      );
      return result.data;
    } else {
      // Voucher không hợp lệ
      Alert.alert('Lỗi', result.data.errorMessage || result.message);
      return null;
    }
  } catch (error) {
    console.error('Check voucher error:', error);
    Alert.alert('Lỗi', 'Không thể kiểm tra voucher');
    return null;
  }
};
```

---

## 🎨 UI/UX Recommendations

### Hiển thị khi chọn voucher

```
┌─────────────────────────────────────────┐
│ ✅ Voucher áp dụng thành công           │
├─────────────────────────────────────────┤
│ Mã: SALE20                              │
│ Giảm 20% tối đa 50,000đ                 │
│                                         │
│ Tổng đơn hàng:     150,000đ            │
│ Giảm giá:          -30,000đ (20%)      │
│ ──────────────────────────────────────  │
│ Tổng thanh toán:   120,000đ            │
│                                         │
│ 🎉 Bạn tiết kiệm được 30,000đ!         │
└─────────────────────────────────────────┘
```

### Hiển thị khi voucher không hợp lệ

```
┌─────────────────────────────────────────┐
│ ❌ Không thể áp dụng voucher            │
├─────────────────────────────────────────┤
│ Mã: SALE20                              │
│                                         │
│ Đơn hàng phải từ 100,000đ              │
│ để sử dụng voucher này                  │
│                                         │
│ Hiện tại: 80,000đ                       │
│ Cần thêm: 20,000đ                       │
└─────────────────────────────────────────┘
```

---

## 🔄 Workflow Recommendation

### Cách 1: Chọn voucher từ danh sách

```
1. User mở danh sách vouchers
   ↓ GET /api/vouchers/my-vouchers
   
2. User click chọn voucher
   ↓ POST /api/vouchers/check (voucherId, cartTotal)
   
3. Nếu valid → Hiển thị discount, update UI
   Nếu invalid → Hiển thị lỗi, suggest cách fix
```

### Cách 2: Nhập mã voucher

```
1. User nhập code "SALE20"
   ↓
   
2. Validate input
   ↓ POST /api/vouchers/validate (voucherCode, cartTotal)
   
3. Nếu valid → Apply voucher
   Nếu invalid → Hiển thị lỗi
```

---

## 📊 Comparison: Check vs Validate

| Feature | `/check` (by ID) | `/validate` (by Code) |
|---------|------------------|----------------------|
| Input | `voucherId` | `voucherCode` |
| Use Case | Chọn từ danh sách | Nhập mã thủ công |
| Response trên lỗi | HTTP 200 | HTTP 400 |
| Extra Info | `percentageSaved` | - |
| Recommended | ✅ Yes | Legacy |

---

## 🧪 Testing với curl

### Test valid voucher

```bash
curl -X POST http://localhost:1912/api/vouchers/check \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{
    "voucherId": 2,
    "orderSubTotal": 150000
  }'
```

### Test voucher với đơn hàng nhỏ (không đạt MinOrderValue)

```bash
curl -X POST http://localhost:1912/api/vouchers/check \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{
    "voucherId": 2,
    "orderSubTotal": 50000
  }'
```

### Test voucher không tồn tại

```bash
curl -X POST http://localhost:1912/api/vouchers/check \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{
    "voucherId": 9999,
    "orderSubTotal": 100000
  }'
```

---

## 📖 Related Documentation

- [VOUCHER_MODULE.md](./VOUCHER_MODULE.md) - Full documentation
- [ORDER_MODULE.md](./ORDER_MODULE.md) - Order integration
