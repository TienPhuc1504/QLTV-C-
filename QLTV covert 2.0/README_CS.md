# 📚 Hệ Thống Quản Lý Thư Viện Số (C# Windows Forms)

## 🎯 Giới thiệu

Hệ thống quản lý thư viện hiện đại được viết bằng **C# Windows Forms** với giao diện đẹp mắt sử dụng các thư viện UI nâng cao:
- **Guna.UI2** - UI Framework hiện đại
- **FontAwesome.Sharp** - Các biểu tượng chuyên nghiệp
- **SQLite** - Cơ sở dữ liệu nhẹ nhàng nhưng mạnh mẽ

## 🛠️ Công Nghệ Sử Dụng

- **Framework**: .NET Framework 4.7.2
- **Language**: C# 7.3
- **Database**: SQLite
- **UI Libraries**: Guna.UI2, FontAwesome.Sharp, MaterialSkin
- **IDE**: Visual Studio Community 2026

## 📋 Tính Năng Chính

### 🔐 Hệ Thống Xác Thực
- ✅ Đăng nhập với 3 vai trò: Admin, Nhân viên, Độc giả
- ✅ Mã hóa mật khẩu SHA256
- ✅ Phân quyền theo vai trò

### 📖 Quản Lý Sách (Nhân viên + Admin)
- ✅ Thêm/Sửa/Xóa sách
- ✅ Tìm kiếm sách nâng cao
- ✅ Phân loại sách theo thể loại
- ✅ Quản lý sách giấy và sách online
- ✅ Theo dõi trạng thái sách

### 👥 Quản Lý Độc Giả (Nhân viên + Admin)
- ✅ Thêm/Sửa/Xóa độc giả
- ✅ Quản lý thẻ độc giả
- ✅ Gia hạn thẻ đọc
- ✅ Xem lịch sử mượn
- ✅ In thẻ đọc giả (PDF)

### 📋 Quản Lý Mượn/Trả (Nhân viên + Admin)
- ✅ Tạo phiếu mượn sách
- ✅ Trả sách
- ✅ Theo dõi sách quá hạn
- ✅ Tính phí phạt
- ✅ Báo cáo mượn

### 📊 Thống Kê/Báo Cáo (Admin)
- ✅ Bảng điều khiển tổng quan
- ✅ Thống kê lượng sách
- ✅ Thống kê độc giả
- ✅ Báo cáo mượn/trả
- ✅ Xuất báo cáo PDF

## 🚀 Hướng Dẫn Cài Đặt

### Yêu Cầu Hệ Thống
- Windows 7 trở lên
- .NET Framework 4.7.2

### Các Bước Cài Đặt

1. **Clone/Tải dự án**
   ```
   D:\C#\QLTV covert 2.0\
   ```

2. **Mở project trong Visual Studio**
   - File > Open > Project
   - Chọn file `QLTV covert 2.0.csproj`

3. **Cài đặt NuGet Packages**
   ```
   Tools > NuGet Package Manager > Manage NuGet Packages for Solution
   ```

   Các package cần thiết (đã có sẵn):
   - Guna.UI2.WinForms
   - FontAwesome.Sharp
   - System.Data.SQLite

4. **Biên dịch & Chạy**
   ```
   Build > Build Solution (Ctrl + Shift + B)
   Debug > Start Debugging (F5)
   ```

## 👤 Tài Khoản Demo

| Vai Trò | Username | Password | Quyền |
|---------|----------|----------|--------|
| Admin | admin | admin123 | Toàn quyền |
| Nhân viên | nhanvien | nv123 | Quản lý sách, độc giả, mượn/trả |
| Độc giả | docgia | dg123 | Xem sách, yêu cầu mượn |

## 📁 Cấu Trúc Dự Án

```
QLTV covert 2.0/
├── Models/
│   ├── User.cs          - Model người dùng
│   ├── Book.cs          - Model sách
│   └── Borrow.cs        - Model mượn/trả
├── Data/
│   ├── DatabaseManager.cs      - Quản lý kết nối DB
│   ├── BookRepository.cs       - Repository sách
│   ├── ReaderRepository.cs     - Repository độc giả
│   └── BorrowRepository.cs     - Repository mượn/trả
├── Forms/
│   ├── LoginForm.cs            - Form đăng nhập
│   ├── MainForm.cs             - Form chính
│   ├── BookManagementForm.cs   - Form quản lý sách
│   ├── ReaderManagementForm.cs - Form quản lý độc giả
│   └── BorrowManagementForm.cs - Form quản lý mượn/trả
├── Utilities/
│   ├── AppConfig.cs    - Cấu hình ứng dụng
│   └── FormHelper.cs   - Các hàm trợ giúp
├── Program.cs          - Điểm vào ứng dụng
└── library.db          - Cơ sở dữ liệu SQLite
```

## 🎨 Giao Diện

### Đặc Điểm Thiết Kế
- **Modern UI** - Sử dụng Guna.UI2 với hiệu ứng mượt mà
- **Responsive Design** - Tự động điều chỉnh theo kích thước cửa sổ
- **Consistent Colors** - Bảng màu thống nhất (Xanh - Xám - Trắng)
- **Professional Layout** - Sidebar navigation + Main content area

### Màu Sắc Chính
- 🔵 Xanh Primary: #2980B9
- 🟢 Xanh Thành công: #2ECC71
- 🟡 Vàng Cảnh báo: #F1C40F
- 🔴 Đỏ Lỗi: #E74C3C
- 🟣 Tím: #9B59B6

## 💾 Cơ Sở Dữ Liệu

### Các Bảng Chính
- **NGUOI_DUNG** - Thông tin người dùng
- **TAI_KHOAN** - Tài khoản đăng nhập
- **SACH** - Thông tin sách
- **QUYEN_SACH** - Quyển sách (từng bản sao)
- **PHIEU_MUON_TRA** - Phiếu mượn/trả
- **THE_DOC_GIA** - Thẻ độc giả
- **THE_LOAI_SACH** - Thể loại sách

### Vị Trí Database
```
%APPDATA%\QLTV_CoVert_2.0\library.db
```

Hoặc trong thư mục QLTV Python:
```
QLTV covert 2.0\QLTV\library.db
```

## 🔐 Bảo Mật

- ✅ Mật khẩu mã hóa SHA256
- ✅ Kiểm tra quyền trên từng chức năng
- ✅ Kiểm tra SQL Injection (Sử dụng Parameterized Queries)
- ✅ Phiên làm việc an toàn

## 📝 Hướng Dẫn Sử Dụng

### 1. Đăng Nhập
1. Nhập tên đăng nhập
2. Nhập mật khẩu
3. Nhấn "ĐĂNG NHẬP" hoặc Enter
4. Chuyển hướng đến trang chủ tương ứng

### 2. Quản Lý Sách (Nhân viên)
1. Nhấn "📖 Quản lý sách" từ menu
2. Tìm kiếm sách hoặc lọc theo thể loại
3. Nhấn "➕ Thêm sách" để thêm mới
4. Nhấn "✏️ Sửa" để chỉnh sửa
5. Nhấn "❌ Xóa" để xóa

### 3. Quản Lý Độc Giả (Nhân viên)
1. Nhấn "👥 Quản lý độc giả"
2. Xem danh sách độc giả và thẻ
3. Thêm/Sửa/Xóa độc giả

### 4. Quản Lý Mượn/Trả (Nhân viên)
1. Nhấn "📋 Mượn/Trả sách"
2. Tạo phiếu mượn mới
3. Ghi nhận sách trả
4. Xem sách quá hạn

## 🐛 Troubleshooting

### Lỗi: Database không được tìm thấy
- Đảm bảo folder `%APPDATA%\QLTV_CoVert_2.0\` tồn tại
- Kiểm tra quyền đọc/ghi thư mục
- Database sẽ tự động tạo nếu không tồn tại

### Lỗi: Không kết nối được SQLite
- Kiểm tra package `System.Data.SQLite` đã cài
- Biên dịch lại solution
- Xóa folder bin/Debug và build lại

### Lỗi: Giao diện không hiển thị đúng
- Kiểm tra phiên bản Guna.UI2
- Cập nhật Visual Studio lên version mới nhất

## 📚 Tài Liệu Tham Khảo

- [Guna.UI2 Documentation](https://blog.bunifu.co/guna-ui-framework/)
- [SQLite Documentation](https://www.sqlite.org/docs.html)
- [C# Windows Forms](https://docs.microsoft.com/en-us/dotnet/desktop/winforms/)

## 🚀 Phát Triển Tiếp Theo

- ⏳ Hoàn thiện tất cả CRUD operations
- ⏳ Thêm form dialog cho Add/Edit
- ⏳ Xuất báo cáo PDF
- ⏳ In thẻ độc giả
- ⏳ Thông báo sách quá hạn
- ⏳ Đồng bộ dữ liệu với Python version

## 📞 Liên Hệ & Hỗ Trợ

Nếu gặp vấn đề hoặc có câu hỏi, vui lòng:
- Kiểm tra file này
- Xem logs trong Output window
- Liên hệ nhóm phát triển

## 📄 Giấy Phép

Dự án này được phát triển cho mục đích giáo dục và sử dụng nội bộ.

---

**Phiên bản**: 2.0  
**Cập nhật**: 2026  
**Trạng thái**: ✅ Đang phát triển
