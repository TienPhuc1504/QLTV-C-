# 📚 QLTV - Hệ Thống Quản Lý Thư Viện v2.0

Một ứng dụng quản lý thư viện số toàn diện được xây dựng bằng C# Windows Forms với giao diện hiện đại (Modern UI), hỗ trợ đầy đủ các nghiệp vụ thư viện từ quản lý sách, độc giả đến mượn trả và thống kê.

---

## 🌟 Tính Năng Nổi Bật

Hệ thống được thiết kế với mô hình phân quyền chặt chẽ gồm 3 vai trò:

### 👑 Quản trị viên (Admin)
- Toàn quyền hệ thống.
- Quản lý tài khoản và thông tin nhân viên thư viện.
- Khôi phục/đặt lại mật khẩu cho nhân viên.
- Xem bảng thống kê tổng quan (Dashboard), báo cáo mượn trả và doanh thu tiền phạt.

### 👔 Nhân viên Thư viện
- **Quản lý Sách:** Thêm, sửa, xóa, tìm kiếm sách và thể loại sách. Hỗ trợ cả sách giấy và sách online (PDF).
- **Quản lý Độc giả:** Cấp thẻ, gia hạn thẻ, khóa thẻ, yêu cầu in thẻ và xem lịch sử mượn trả của độc giả.
- **Mượn/Trả Sách:** Xử lý yêu cầu mượn sách, lập phiếu mượn, xác nhận trả sách và tính tiền phạt quá hạn tự động.
- Khôi phục/đặt lại mật khẩu cho độc giả về mặc định.

### 🎓 Độc giả
- **Tra cứu:** Tìm kiếm sách trực tuyến trong thư viện với giao diện thân thiện.
- **Yêu cầu mượn sách:** Gửi yêu cầu mượn sách trực tuyến đến nhân viên và theo dõi trạng thái yêu cầu.
- **Tủ sách cá nhân:** Xem lịch sử các cuốn sách đang mượn, đã trả, hoặc quá hạn.
- **Tài khoản:** Quản lý thông tin cá nhân và thay đổi mật khẩu.

---

## 🚀 Hướng Dẫn Cài Đặt & Chạy

### 1. Yêu cầu hệ thống
- Visual Studio 2022 (hoặc phiên bản mới hơn)
- .NET Framework 4.7.2 (hoặc tương thích)
- Thư viện UI: `Guna.UI2.WinForms` và thư viện Database: `System.Data.SQLite.Core` (đã được cấu hình sẵn qua NuGet Packages).

### 2. Các bước chạy ứng dụng
1. Mở file solution hoặc project (`QLTV covert 2.0.csproj`) bằng Visual Studio.
2. Nhấn **Ctrl + Shift + B** để Build project (hệ thống sẽ tự động khôi phục các gói NuGet).
3. Nhấn **F5** để khởi chạy ứng dụng.

---

## 👤 Tài Khoản Demo (Mặc định)

Ngay trong lần đầu tiên chạy ứng dụng, hệ thống sẽ tự động tạo cơ sở dữ liệu và nạp sẵn các tài khoản mẫu sau để bạn dễ dàng trải nghiệm:

| Vai trò | Tên đăng nhập | Mật khẩu |
| :--- | :--- | :--- |
| **Admin** | `admin` | `admin123` |
| **Nhân viên** | `nhanvien` | `nv123` |
| **Độc giả** | `docgia` | `dg123` |

*(Lưu ý: Bạn có thể đăng nhập, sau đó vào tab Tài khoản để đổi mật khẩu)*

---

## 💾 Cơ Sở Dữ Liệu

- **Hệ quản trị CSDL:** SQLite (Portable, gọn nhẹ)
- **Đường dẫn lưu trữ DB:** Hệ thống sẽ tự động khởi tạo cơ sở dữ liệu tại thư mục `%APPDATA%\QLTV_CoVert_2.0\library.db`. Bạn không cần cài đặt SQL Server hay cấu hình chuỗi kết nối phức tạp.
- **Cấu trúc:** Gồm các bảng chặt chẽ như: Người dùng, Tài khoản, Nhân viên, Độc giả, Thẻ độc giả, Sách, Thể loại, Quyển sách (bản vật lý), Phiếu mượn, Yêu cầu mượn, Thông báo hệ thống và Sách Online.

---

## 🛠 Công Nghệ Sử Dụng

- **Ngôn ngữ:** C# 7.3+
- **Nền tảng:** .NET Framework Windows Forms
- **UI Framework:** [Guna.UI2](https://gunaframework.com/) cho giao diện mượt mà, hỗ trợ bo góc (border radius), shadow và các hiệu ứng động (animations) cao cấp.
- **Database:** ADO.NET kết nối với SQLite.
- **Bảo mật:** Mật khẩu được mã hóa băm SHA-256 trước khi lưu xuống Database. Sử dụng tham số hóa (Parameterized Queries) trên toàn bộ truy vấn để chống SQL Injection.
- **Kiến trúc:** Phân tách logic quản lý dữ liệu (`LibraryService`, `DatabaseManager`) và UI (các file trong thư mục `Forms` và bộ màu `AppTheme`), giúp code dễ đọc, dễ bảo trì và mở rộng.

---

*Dự án được phát triển với mục tiêu cung cấp trải nghiệm hiện đại (Modern UX/UI) cho bài toán Quản lý Thư viện truyền thống trên môi trường Windows.*
