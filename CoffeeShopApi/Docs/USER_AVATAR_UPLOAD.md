# User Avatar Upload API

## ⭐ New Features

Đã thêm 2 endpoints mới vào `UsersController` để quản lý avatar:

1. **Upload Avatar** - `POST /api/users/avatar`
2. **Delete Avatar** - `DELETE /api/users/avatar`

---

## 📡 API Endpoints

### 1. Upload Avatar

**Endpoint:** `POST /api/users/avatar`

**Authorization:** Required (Bearer Token)

**Content-Type:** `multipart/form-data`

**Request:**
```bash
curl -X POST http://localhost:1912/api/users/avatar \
  -H "Authorization: Bearer <token>" \
  -F "file=@avatar.jpg"
```

**Request (form-data):**
```
file: <file> (ảnh JPG, PNG, GIF, WEBP)
```

**Validation:**
- ✅ File types: `.jpg`, `.jpeg`, `.png`, `.gif`, `.webp`
- ✅ Max size: 5MB
- ✅ Required: Phải có file

**Response (Success):**
```json
{
  "success": true,
  "message": "Upload avatar thành công",
  "data": {
    "avatarUrl": "/uploads/avatars/user_123_20250201_abc123.jpg",
    "fileName": "user_123_20250201_abc123.jpg",
    "fileSize": 245678
  }
}
```

**Response (Error - No file):**
```json
{
  "success": false,
  "message": "Vui lòng chọn file ảnh",
  "status": 400
}
```

**Response (Error - Invalid file type):**
```json
{
  "success": false,
  "message": "File không hợp lệ. Chỉ chấp nhận: .jpg, .jpeg, .png, .gif, .webp",
  "status": 400
}
```

**Response (Error - File too large):**
```json
{
  "success": false,
  "message": "Kích thước file không được vượt quá 5MB",
  "status": 400
}
```

**Workflow:**
1. Validate file type & size
2. Get old avatar URL (để xóa sau)
3. Upload file mới → `/uploads/avatars/user_{userId}_{timestamp}_{random}.jpg`
4. Update `User.AvatarUrl` trong database
5. Xóa avatar cũ (nếu có)
6. Return new avatar URL

---

### 2. Delete Avatar

**Endpoint:** `DELETE /api/users/avatar`

**Authorization:** Required (Bearer Token)

**Request:**
```bash
curl -X DELETE http://localhost:1912/api/users/avatar \
  -H "Authorization: Bearer <token>"
```

**Response (Success):**
```json
{
  "success": true,
  "message": "Xóa avatar thành công",
  "data": null
}
```

**Response (Error - No avatar):**
```json
{
  "success": false,
  "message": "User chưa có avatar",
  "status": 400
}
```

**Workflow:**
1. Get current avatar URL
2. Set `User.AvatarUrl = null` trong database
3. Delete physical file
4. Return success

---

## 🎯 Integration với Profile

### Get Profile Response (Updated)

**Endpoint:** `GET /api/users/profile`

**Response:**
```json
{
  "success": true,
  "data": {
    "id": 123,
    "userName": "john_doe",
    "fullName": "John Doe",
    "phoneNumber": "0987654321",
    "email": "john@example.com",
    "avatarUrl": "/uploads/avatars/user_123_20250201_abc123.jpg",
    "isActive": true,
    "createdAt": "2025-01-01T00:00:00Z",
    "lastLoginAt": "2025-02-01T10:30:00Z",
    "orderStats": {
      "totalOrders": 15,
      "pendingOrders": 2,
      "completedOrders": 13,
      "totalSpent": 1250000
    }
  }
}
```

---

## 💻 Frontend Integration

### React/TypeScript Example

```typescript
// Upload avatar
const uploadAvatar = async (file: File) => {
  const formData = new FormData();
  formData.append('file', file);

  const response = await fetch('http://localhost:1912/api/users/avatar', {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`
    },
    body: formData
  });

  const result = await response.json();
  
  if (result.success) {
    console.log('Avatar uploaded:', result.data.avatarUrl);
    // Update UI với avatar mới
    setAvatarUrl(result.data.avatarUrl);
  } else {
    alert(result.message);
  }
};

// Delete avatar
const deleteAvatar = async () => {
  const response = await fetch('http://localhost:1912/api/users/avatar', {
    method: 'DELETE',
    headers: {
      'Authorization': `Bearer ${token}`
    }
  });

  const result = await response.json();
  
  if (result.success) {
    console.log('Avatar deleted');
    setAvatarUrl(null);
  } else {
    alert(result.message);
  }
};

// Component
function ProfileAvatar() {
  const [avatarUrl, setAvatarUrl] = useState<string | null>(null);

  const handleFileSelect = async (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (!file) return;

    // Validate size
    if (file.size > 5 * 1024 * 1024) {
      alert('File quá lớn. Tối đa 5MB');
      return;
    }

    // Validate type
    const validTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/webp'];
    if (!validTypes.includes(file.type)) {
      alert('File không hợp lệ. Chỉ chấp nhận ảnh');
      return;
    }

    await uploadAvatar(file);
  };

  return (
    <div className="avatar-upload">
      {avatarUrl ? (
        <div>
          <img src={`http://localhost:1912${avatarUrl}`} alt="Avatar" />
          <button onClick={deleteAvatar}>Xóa avatar</button>
        </div>
      ) : (
        <div>
          <input type="file" accept="image/*" onChange={handleFileSelect} />
        </div>
      )}
    </div>
  );
}
```

### React Native Example

```javascript
import { launchImageLibrary } from 'react-native-image-picker';

const uploadAvatar = async () => {
  // Chọn ảnh
  const result = await launchImageLibrary({
    mediaType: 'photo',
    maxWidth: 1024,
    maxHeight: 1024,
    quality: 0.8
  });

  if (result.didCancel || !result.assets?.[0]) return;

  const file = result.assets[0];

  // Validate size (5MB)
  if (file.fileSize && file.fileSize > 5 * 1024 * 1024) {
    Alert.alert('Lỗi', 'File quá lớn. Tối đa 5MB');
    return;
  }

  // Upload
  const formData = new FormData();
  formData.append('file', {
    uri: file.uri,
    type: file.type || 'image/jpeg',
    name: file.fileName || 'avatar.jpg'
  });

  try {
    const token = await AsyncStorage.getItem('@auth_token');
    
    const response = await fetch('http://10.0.2.2:1912/api/users/avatar', {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'multipart/form-data'
      },
      body: formData
    });

    const result = await response.json();
    
    if (result.success) {
      Alert.alert('Thành công', 'Upload avatar thành công');
      setAvatarUrl(result.data.avatarUrl);
    } else {
      Alert.alert('Lỗi', result.message);
    }
  } catch (error) {
    console.error('Upload error:', error);
    Alert.alert('Lỗi', 'Không thể upload avatar');
  }
};
```

---

## 🗂️ File Storage

### Directory Structure

```
wwwroot/
  uploads/
    avatars/
      user_1_20250201_abc123.jpg
      user_2_20250201_def456.png
      user_3_20250201_ghi789.webp
```

### Naming Convention

```
Pattern: user_{userId}_{timestamp}_{random}.{extension}

Examples:
- user_123_20250201_a1b2c3.jpg
- user_456_20250201_d4e5f6.png
```

### Storage Settings

**File:** `appsettings.json`

```json
{
  "FileUpload": {
    "UploadPath": "wwwroot/uploads",
    "AllowedExtensions": [".jpg", ".jpeg", ".png", ".gif", ".webp"],
    "MaxFileSize": 5242880,
    "BaseUrl": "/uploads"
  }
}
```

---

## 🔧 Service Updates

### UserService

**Added:**
- ✅ `Task<bool> UpdateAvatarAsync(int userId, string? avatarUrl)`

**Updated:**
- ✅ `MapToProfileResponse` - Include `AvatarUrl`

### FileUploadService

**Used methods:**
- ✅ `UploadFileAsync(IFormFile file, string folder, string? customFileName)`
- ✅ `DeleteFileAsync(string fileUrl)`

---

## 📊 Database

### User Table

**Field:** `AvatarUrl` (nvarchar(500), nullable)

**Already exists** - Không cần migration

---

## ✅ Checklist

- [x] Add `IFileUploadService` to `UsersController`
- [x] Add `POST /api/users/avatar` endpoint
- [x] Add `DELETE /api/users/avatar` endpoint
- [x] Add `UpdateAvatarAsync` to `IUserService`
- [x] Implement `UpdateAvatarAsync` in `UserService`
- [x] Update `MapToProfileResponse` to include `AvatarUrl`
- [x] Update `UserProfileResponse` DTO with `AvatarUrl`
- [x] Validation: File type & size
- [x] Delete old avatar when uploading new
- [x] Rollback on failure
- [x] Documentation

---

## 🧪 Testing

### Test with curl

```bash
# Upload avatar
curl -X POST http://localhost:1912/api/users/avatar \
  -H "Authorization: Bearer <token>" \
  -F "file=@test.jpg"

# Get profile (check avatar)
curl -X GET http://localhost:1912/api/users/profile \
  -H "Authorization: Bearer <token>"

# Delete avatar
curl -X DELETE http://localhost:1912/api/users/avatar \
  -H "Authorization: Bearer <token>"
```

### Test with Postman

1. **Upload Avatar**
   - Method: POST
   - URL: `http://localhost:1912/api/users/avatar`
   - Headers: `Authorization: Bearer <token>`
   - Body: form-data
     - Key: `file`
     - Type: File
     - Value: Select image file

2. **Get Profile**
   - Method: GET
   - URL: `http://localhost:1912/api/users/profile`
   - Headers: `Authorization: Bearer <token>`

3. **Delete Avatar**
   - Method: DELETE
   - URL: `http://localhost:1912/api/users/avatar`
   - Headers: `Authorization: Bearer <token>`

---

## 🚀 Status

**Implementation:** ✅ Complete

**Files Modified:**
- ✅ `CoffeeShopApi\Controllers\UsersController.cs`
- ✅ `CoffeeShopApi\Services\UserService.cs`
- ✅ `CoffeeShopApi\DTOs\UserDTO.cs`

**Ready for Testing:** ✅ Yes
