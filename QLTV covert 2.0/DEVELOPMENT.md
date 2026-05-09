// QLTV - Quản Lý Thư Viện Số
// C# Windows Forms Version
// .NET Framework 4.7.2

## 🎯 MỤC TIÊU DỰ ÁN

✅ Chuyển đổi từ Python CustomTkinter sang C# Windows Forms
✅ Nâng cấp giao diện với Guna.UI2 & FontAwesome.Sharp
✅ Giữ nguyên cơ sở dữ liệu SQLite
✅ Nâng cao hiệu suất và tính chuyên nghiệp
✅ Duy trì tương thích với dữ liệu Python version

## 📦 PACKAGES ĐÃ CÀI ĐẶT

1. **Guna.UI2.WinForms** (v2.0.4.7)
   - UI Framework hiện đại
   - Guna2Button, Guna2TextBox, Guna2DataGridView
   - Hỗ trợ themes và styling

2. **FontAwesome.Sharp** (v6.6.0)
   - Biểu tượng chuyên nghiệp
   - Hỗ trợ 1000+ icons
   - Tích hợp dễ dàng

3. **System.Data.SQLite** (v2.0.3)
   - Kết nối SQLite
   - Hỗ trợ đầy đủ cho .NET Framework

4. **MaterialSkin** (v2.3.1)
   - Material Design styling
   - Modern look and feel

## 📁 CẤU TRÚC FILE

QLTV covert 2.0/
├── 📂 Models/
│   ├── User.cs              ← Models người dùng
│   ├── Book.cs              ← Models sách
│   └── Borrow.cs            ← Models mượn/trả
│
├── 📂 Data/
│   ├── DatabaseManager.cs   ← Quản lý DB & Auth
│   ├── BookRepository.cs    ← CRUD sách
│   ├── ReaderRepository.cs  ← CRUD độc giả
│   └── BorrowRepository.cs  ← CRUD mượn/trả
│
├── 📂 Forms/
│   ├── LoginForm.cs             ← Form đăng nhập
│   ├── MainForm.cs              ← Form chính + Dashboard
│   ├── BookManagementForm.cs    ← Quản lý sách
│   ├── ReaderManagementForm.cs  ← Quản lý độc giả
│   └── BorrowManagementForm.cs  ← Quản lý mượn/trả
│
├── 📂 Utilities/
│   ├── AppConfig.cs         ← Cấu hình toàn cục
│   └── FormHelper.cs        ← Helper functions
│
├── Program.cs               ← Entry point
├── packages.config          ← NuGet packages
└── library.db              ← SQLite Database

## 🔑 CÁC TÍNH NĂNG CHÍNH ĐÃ TRIỂN KHAI

### ✅ Hoàn thành
- [x] Cấu trúc dự án cơ bản
- [x] Database Manager
- [x] Form Đăng nhập
- [x] Form Chính + Dashboard
- [x] Form Quản lý sách (giao diện)
- [x] Form Quản lý độc giả (giao diện)
- [x] Form Quản lý mượn/trả (giao diện)
- [x] Models & Repositories
- [x] Utilities & Config
- [x] Authentication & Authorization

### ⏳ Cần hoàn thành
- [ ] CRUD hoàn chỉnh cho sách (Add/Edit dialogs)
- [ ] CRUD hoàn chỉnh cho độc giả (Add/Edit dialogs)
- [ ] CRUD hoàn chỉnh cho mượn/trả
- [ ] Tìm kiếm & lọc nâng cao
- [ ] In báo cáo PDF
- [ ] In thẻ độc giả
- [ ] Dashboard thống kê đầy đủ
- [ ] Form Quản lý nhân viên
- [ ] Form Thống kê/Báo cáo
- [ ] Xử lý lỗi & validation
- [ ] Unit tests
- [ ] Installer/Setup

## 🎨 GHI CHÚ VỀ THIẾT KẾ

### Màu Sắc Sử Dụng
```csharp
PrimaryBlue       = #2980B9 (41, 128, 185)
PrimaryBlueDark   = #1E64A0 (30, 100, 160)
PrimaryBlueLite   = #3498DB (52, 152, 219)
SuccessGreen      = #2ECC71 (46, 204, 113)
WarningYellow     = #F1C40F (241, 196, 15)
ErrorRed          = #E74C3C (231, 76, 60)
BackgroundGray    = #F5F5F5 (245, 245, 245)
```

### Layout
- Sidebar Navigation (250px width) - Xanh đậm
- Main Content Area - Xám nhạt background
- Top Bar - Trắng

### Fonts
- Title: Arial Bold 22-26px
- Button/Label: Arial 11-14px
- Small text: Arial 9-10px

## 🔗 TƯƠNG THÍCH DỮ LIỆU

Database được duy trì tương thích 100% với Python version:
- Cùng schema SQLite
- Cùng tên bảng & cột
- Cùng kiểu dữ liệu
- Cùng dữ liệu mẫu ban đầu

## 🧪 KIỂM THỬ

### Tài khoản demo
```
Admin:       admin / admin123
Nhân viên:   nhanvien / nv123
Độc giả:    docgia / dg123
```

### Test scenarios
1. Đăng nhập với 3 vai trò khác nhau ✓
2. Hiển thị menu theo quyền ✓
3. Xem danh sách sách ✓
4. Xem danh sách độc giả ✓
5. Xem danh sách mượn/trả ✓

## 💡 LƯỚI ỲNổ VỀ CÀI ĐẶT

- .NET Framework 4.7.2 yêu cầu Windows 7 SP1 trở lên
- SQLite embedded, không cần cài riêng
- Packages tự động restore từ NuGet

## 📌 NOTES KỸ THUẬT

### C# Language Features
- Chủ yếu dùng C# 7.3 (tương thích .NET Framework 4.7.2)
- Tránh dùng C# 8+ features (pattern matching, switch expressions)
- Dùng ternary conditional thay vì switch expressions

### Database Pattern
- Repository pattern cho mỗi entity
- Using statements cho connections
- Parameterized queries để tránh SQL Injection
- Error handling cho tất cả DB operations

### UI Pattern
- Programmatic UI creation (không dùng Designer)
- Consistent color usage qua AppColors class
- Responsive layout với Dock properties
- Event handlers cho user interactions

## 🚀 HƯỚNG DẪN TIẾP TỤC

Để hoàn thành dự án:

1. **Implement Add/Edit Dialogs**
   - Tạo các form dialog cho Add/Edit
   - Validate input
   - Update repositories

2. **Complete CRUD Operations**
   - Implement button handlers
   - Add database transactions
   - Error handling & messages

3. **Advanced Features**
   - Tìm kiếm/lọc nâng cao
   - Export PDF
   - Print functionality
   - Statistics/Reports

4. **Testing & Polish**
   - Unit tests
   - User acceptance testing
   - Performance optimization
   - Bug fixes

## 📞 LIÊN HỆ HỖNH TỚI

Nếu cần hỗ trợ hoặc có câu hỏi về setup/architecture, 
vui lòng kiểm tra:
- README_CS.md - Hướng dẫn sử dụng chi tiết
- Code comments - Giải thích từng phần

---
**Last Updated**: 2026
**Status**: Active Development ✅
