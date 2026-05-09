using System;
using System.Data.SQLite;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using QLTV_covert_2._0.Models;

namespace QLTV_covert_2._0.Data
{
    public class DatabaseManager
    {
        private static DatabaseManager _instance;
        private static readonly object _lock = new object();

        private readonly string _connectionString;
        private readonly string _databasePath;

        /// <summary>
        /// Singleton instance — tránh tạo nhiều DatabaseManager.
        /// </summary>
        public static DatabaseManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                            _instance = new DatabaseManager();
                    }
                }
                return _instance;
            }
        }

        public DatabaseManager()
        {
            // Đặt database trong folder của ứng dụng
            _databasePath = FindDatabasePath();
            _connectionString = $"Data Source={_databasePath};Version=3;Foreign Keys=True;";

            InitializeDatabase();
        }

        public string ConnectionString => _connectionString;

        public SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(_connectionString);
        }

        private static string FindDatabasePath()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            DirectoryInfo directory = new DirectoryInfo(baseDirectory);

            while (directory != null)
            {
                string candidate = Path.Combine(directory.FullName, "Data", "library.db");
                if (File.Exists(candidate))
                    return candidate;

                candidate = Path.Combine(directory.FullName, "QLTV covert 2.0", "Data", "library.db");
                if (File.Exists(candidate))
                    return candidate;

                directory = directory.Parent;
            }

            return Path.Combine(baseDirectory, "Data", "library.db");
        }

        private void InitializeDatabase()
        {
            if (File.Exists(_databasePath))
                return;

            // Tạo thư mục nếu chưa tồn tại
            string dir = Path.GetDirectoryName(_databasePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = GetInitializationScript();
                    command.ExecuteNonQuery();
                }
                InsertSampleData(connection);
            }
        }

        private string GetInitializationScript()
        {
            return @"
            -- Bảng NGUOI_DUNG
            CREATE TABLE NGUOI_DUNG (
                ma_nd INTEGER PRIMARY KEY AUTOINCREMENT,
                ho_ten VARCHAR(100) NOT NULL,
                dia_chi VARCHAR(255),
                so_dt VARCHAR(15),
                email VARCHAR(100),
                loai_nguoi_dung VARCHAR(20) NOT NULL CHECK(loai_nguoi_dung IN ('ADMIN', 'NHAN_VIEN', 'DOC_GIA'))
            );

            -- Bảng TAI_KHOAN
            CREATE TABLE TAI_KHOAN (
                ten_tk VARCHAR(50) PRIMARY KEY,
                mat_khau VARCHAR(255) NOT NULL,
                ma_nd INTEGER UNIQUE NOT NULL,
                FOREIGN KEY (ma_nd) REFERENCES NGUOI_DUNG(ma_nd) ON DELETE CASCADE
            );

            -- Bảng ADMIN
            CREATE TABLE ADMIN (
                ma_nd INTEGER PRIMARY KEY,
                FOREIGN KEY (ma_nd) REFERENCES NGUOI_DUNG(ma_nd) ON DELETE CASCADE
            );

            -- Bảng DOC_GIA
            CREATE TABLE DOC_GIA (
                ma_nd INTEGER PRIMARY KEY,
                ma_doc_gia VARCHAR(20) UNIQUE NOT NULL,
                ngay_dk DATE NOT NULL,
                FOREIGN KEY (ma_nd) REFERENCES NGUOI_DUNG(ma_nd) ON DELETE CASCADE
            );

            -- Bảng NHAN_VIEN
            CREATE TABLE NHAN_VIEN (
                ma_nd INTEGER PRIMARY KEY,
                ma_nhan_vien VARCHAR(20) UNIQUE NOT NULL,
                FOREIGN KEY (ma_nd) REFERENCES NGUOI_DUNG(ma_nd) ON DELETE CASCADE
            );

            -- Bảng THE_DOC_GIA
            CREATE TABLE THE_DOC_GIA (
                ma_the INTEGER PRIMARY KEY AUTOINCREMENT,
                ma_nd_doc_gia INTEGER UNIQUE NOT NULL,
                ngay_cap DATE NOT NULL,
                ngay_het_han DATE NOT NULL,
                trang_thai_the VARCHAR(20) NOT NULL CHECK(trang_thai_the IN ('HOAT_DONG', 'HET_HAN', 'KHOA')),
                FOREIGN KEY (ma_nd_doc_gia) REFERENCES DOC_GIA(ma_nd) ON DELETE CASCADE
            );

            -- Bảng THE_LOAI_SACH
            CREATE TABLE THE_LOAI_SACH (
                ma_the_loai INTEGER PRIMARY KEY AUTOINCREMENT,
                ten_the_loai VARCHAR(100) NOT NULL UNIQUE
            );

            -- Bảng SACH
            CREATE TABLE SACH (
                ma_sach INTEGER PRIMARY KEY AUTOINCREMENT,
                tieu_de VARCHAR(255) NOT NULL,
                tac_gia VARCHAR(100),
                trang_thai_sach VARCHAR(20) NOT NULL CHECK(trang_thai_sach IN ('CO_SAN', 'DA_MUON', 'HONG', 'MAT')),
                ma_the_loai INTEGER,
                loai_sach VARCHAR(20) NOT NULL CHECK(loai_sach IN ('SACH_GIAY', 'SACH_ONLINE')),
                FOREIGN KEY (ma_the_loai) REFERENCES THE_LOAI_SACH(ma_the_loai)
            );

            -- Bảng QUYEN_SACH
            CREATE TABLE QUYEN_SACH (
                ma_quyen INTEGER PRIMARY KEY AUTOINCREMENT,
                ma_sach INTEGER NOT NULL,
                ma_quyen_sach VARCHAR(50) UNIQUE NOT NULL,
                trang_thai VARCHAR(20) NOT NULL DEFAULT 'CO_SAN' CHECK(trang_thai IN ('CO_SAN', 'DANG_MUON', 'HONG', 'MAT', 'KHONG_CO_SAN')),
                vi_tri VARCHAR(100),
                ghi_chu TEXT,
                ngay_nhap DATE NOT NULL,
                FOREIGN KEY (ma_sach) REFERENCES SACH(ma_sach) ON DELETE CASCADE
            );

            -- Bảng SACH_GIAY
            CREATE TABLE SACH_GIAY (
                ma_sach INTEGER PRIMARY KEY,
                so_luong INTEGER NOT NULL DEFAULT 1,
                FOREIGN KEY (ma_sach) REFERENCES SACH(ma_sach) ON DELETE CASCADE
            );

            -- Bảng SACH_ONLINE
            CREATE TABLE SACH_ONLINE (
                ma_sach INTEGER PRIMARY KEY,
                url_tai_lieu VARCHAR(500),
                dinh_dang VARCHAR(50),
                FOREIGN KEY (ma_sach) REFERENCES SACH(ma_sach) ON DELETE CASCADE
            );

            -- Bảng PHIEU_MUON_TRA (khớp schema Python)
            CREATE TABLE PHIEU_MUON_TRA (
                ma_phieu INTEGER PRIMARY KEY AUTOINCREMENT,
                ma_nd_doc_gia INTEGER NOT NULL,
                ma_nd_nhan_vien INTEGER NOT NULL,
                ma_sach INTEGER NOT NULL,
                ngay_muon DATE NOT NULL,
                ngay_hen_tra DATE NOT NULL,
                ngay_tra_thuc DATE,
                trang_thai_phieu VARCHAR(20) NOT NULL CHECK(trang_thai_phieu IN ('DANG_MUON', 'DA_TRA', 'QUA_HAN')),
                tien_phat REAL DEFAULT 0,
                ma_quyen INTEGER,
                FOREIGN KEY (ma_nd_doc_gia) REFERENCES DOC_GIA(ma_nd),
                FOREIGN KEY (ma_nd_nhan_vien) REFERENCES NHAN_VIEN(ma_nd),
                FOREIGN KEY (ma_sach) REFERENCES SACH(ma_sach),
                FOREIGN KEY (ma_quyen) REFERENCES QUYEN_SACH(ma_quyen)
            );

            -- Bảng YEU_CAU_MUON (khớp schema Python)
            CREATE TABLE YEU_CAU_MUON (
                ma_yeu_cau INTEGER PRIMARY KEY AUTOINCREMENT,
                ma_nd_doc_gia INTEGER NOT NULL,
                ma_sach INTEGER NOT NULL,
                ngay_yeu_cau DATE NOT NULL,
                so_ngay_muon_de_xuat INTEGER NOT NULL,
                ghi_chu TEXT,
                trang_thai VARCHAR(20) NOT NULL CHECK(trang_thai IN ('CHO_DUYET', 'CHO_LAY_SACH', 'DA_LAY', 'TU_CHOI', 'DA_HUY')),
                ma_nd_xu_ly INTEGER,
                ngay_xu_ly DATE,
                ly_do_tu_choi TEXT,
                so_ngay_muon_chinh_thuc INTEGER,
                han_lay_sach DATETIME,
                ma_quyen INTEGER,
                FOREIGN KEY (ma_nd_doc_gia) REFERENCES DOC_GIA(ma_nd),
                FOREIGN KEY (ma_sach) REFERENCES SACH(ma_sach),
                FOREIGN KEY (ma_nd_xu_ly) REFERENCES NGUOI_DUNG(ma_nd),
                FOREIGN KEY (ma_quyen) REFERENCES QUYEN_SACH(ma_quyen)
            );

            -- Bảng THONG_BAO (khớp schema Python)
            CREATE TABLE THONG_BAO (
                ma_thong_bao INTEGER PRIMARY KEY AUTOINCREMENT,
                ma_nd INTEGER NOT NULL,
                tieu_de VARCHAR(200) NOT NULL,
                noi_dung TEXT NOT NULL,
                loai_thong_bao VARCHAR(30) NOT NULL CHECK(loai_thong_bao IN (
                    'YEU_CAU_DUYET', 'YEU_CAU_TU_CHOI', 'SAP_HET_HAN', 'QUA_HAN',
                    'SACH_CO_SAN', 'HE_THONG', 'YEU_CAU_MOI', 'CHO_LAY_SACH'
                )),
                da_doc INTEGER DEFAULT 0,
                ngay_tao DATETIME NOT NULL,
                link_lien_quan TEXT,
                FOREIGN KEY (ma_nd) REFERENCES NGUOI_DUNG(ma_nd) ON DELETE CASCADE
            );

            -- Bảng YEU_CAU_THE (khớp schema Python)
            CREATE TABLE YEU_CAU_THE (
                ma_yeu_cau INTEGER PRIMARY KEY AUTOINCREMENT,
                ma_nd_doc_gia INTEGER NOT NULL,
                ngay_yeu_cau DATETIME NOT NULL,
                trang_thai VARCHAR(20) NOT NULL CHECK(trang_thai IN ('CHO_DUYET', 'DANG_XU_LY', 'DA_IN', 'TU_CHOI', 'DA_HUY')),
                ma_nd_xu_ly INTEGER,
                ngay_xu_ly DATETIME,
                ly_do_tu_choi TEXT,
                da_nhan INTEGER DEFAULT 0,
                FOREIGN KEY (ma_nd_doc_gia) REFERENCES DOC_GIA(ma_nd),
                FOREIGN KEY (ma_nd_xu_ly) REFERENCES NGUOI_DUNG(ma_nd)
            );
            ";
        }

        private void InsertSampleData(SQLiteConnection connection)
        {
            using (var command = connection.CreateCommand())
            {
                // Thêm dữ liệu mẫu
                command.CommandText = @"
                -- Thêm người dùng
                INSERT INTO NGUOI_DUNG (ho_ten, dia_chi, so_dt, email, loai_nguoi_dung) VALUES 
                    ('Quản Trị Viên', 'Hà Nội', '0901234567', 'admin@library.vn', 'ADMIN'),
                    ('Nguyễn Văn A', 'Hà Nội', '0912345678', 'nva@library.vn', 'NHAN_VIEN'),
                    ('Trần Thị B', 'Hồ Chí Minh', '0923456789', 'ttb@email.com', 'DOC_GIA');

                -- Thêm tài khoản
                INSERT INTO TAI_KHOAN (ten_tk, mat_khau, ma_nd) VALUES 
                    ('admin', '" + HashPassword("admin123") + @"', 1),
                    ('nhanvien', '" + HashPassword("nv123") + @"', 2),
                    ('docgia', '" + HashPassword("dg123") + @"', 3);

                -- Thêm admin
                INSERT INTO ADMIN (ma_nd) VALUES (1);

                -- Thêm nhân viên
                INSERT INTO NHAN_VIEN (ma_nd, ma_nhan_vien) VALUES (2, 'NV001');

                -- Thêm độc giả
                INSERT INTO DOC_GIA (ma_nd, ma_doc_gia, ngay_dk) VALUES (3, 'DG001', date('now'));

                -- Thêm thẻ độc giả
                INSERT INTO THE_DOC_GIA (ma_nd_doc_gia, ngay_cap, ngay_het_han, trang_thai_the) VALUES 
                    (3, date('now'), date('now', '+1 year'), 'HOAT_DONG');

                -- Thêm thể loại sách
                INSERT INTO THE_LOAI_SACH (ten_the_loai) VALUES 
                    ('Văn học'),
                    ('Khoa học'),
                    ('Công nghệ'),
                    ('Kinh tế'),
                    ('Lịch sử'),
                    ('Thiếu nhi'),
                    ('Tâm lý');

                -- Thêm sách mẫu
                INSERT INTO SACH (tieu_de, tac_gia, trang_thai_sach, ma_the_loai, loai_sach) VALUES 
                    ('Truyện Kiều', 'Nguyễn Du', 'CO_SAN', 1, 'SACH_GIAY'),
                    ('Lập trình C#', 'Microsoft', 'CO_SAN', 3, 'SACH_GIAY'),
                    ('Lịch sử Việt Nam', 'Nhiều tác giả', 'CO_SAN', 5, 'SACH_GIAY');

                -- Thêm quyển sách
                INSERT INTO QUYEN_SACH (ma_sach, ma_quyen_sach, trang_thai, vi_tri, ngay_nhap) VALUES 
                    (1, 'S0001-Q001', 'CO_SAN', 'Kệ A1', date('now')),
                    (1, 'S0001-Q002', 'CO_SAN', 'Kệ A1', date('now')),
                    (2, 'S0002-Q001', 'CO_SAN', 'Kệ B2', date('now')),
                    (2, 'S0002-Q002', 'CO_SAN', 'Kệ B2', date('now')),
                    (2, 'S0002-Q003', 'CO_SAN', 'Kệ B2', date('now')),
                    (3, 'S0003-Q001', 'CO_SAN', 'Kệ C3', date('now'));

                -- Thêm sách giấy
                INSERT INTO SACH_GIAY (ma_sach, so_luong) VALUES (1, 2), (2, 3), (3, 1);
                ";

                try
                {
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[QLTV] InsertSampleData error: {ex.Message}");
                }
            }
        }

        public static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        public bool AuthenticateUser(string username, string password)
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        // Kiểm tra tài khoản truyền thống
                        command.CommandText = "SELECT mat_khau FROM TAI_KHOAN WHERE ten_tk = @username";
                        command.Parameters.AddWithValue("@username", username);

                        var result = command.ExecuteScalar();
                        if (result != null)
                        {
                            string storedHash = result.ToString();
                            string providedHash = HashPassword(password);
                            if (storedHash == providedHash)
                                return true;
                        }

                        // Nếu không tìm thấy hoặc sai mật khẩu, kiểm tra đăng nhập bằng Email và Mã Độc Giả
                        command.CommandText = @"
                            SELECT dg.ma_doc_gia 
                            FROM NGUOI_DUNG nd
                            JOIN DOC_GIA dg ON nd.ma_nd = dg.ma_nd
                            WHERE nd.email = @username AND nd.loai_nguoi_dung = 'DOC_GIA'";
                        
                        var readerResult = command.ExecuteScalar();
                        if (readerResult != null)
                        {
                            string maDocGia = readerResult.ToString();
                            if (maDocGia == password)
                                return true;
                        }

                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[QLTV] AuthenticateUser error: {ex.Message}");
                return false;
            }
        }

        public User GetUserByUsername(string username)
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        // Lấy người dùng theo Tên Đăng Nhập
                        command.CommandText = @"
                            SELECT nd.ma_nd, nd.ho_ten, nd.dia_chi, nd.so_dt, nd.email, nd.loai_nguoi_dung
                            FROM NGUOI_DUNG nd
                            JOIN TAI_KHOAN tk ON nd.ma_nd = tk.ma_nd
                            WHERE tk.ten_tk = @username";
                        command.Parameters.AddWithValue("@username", username);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new Models.User
                                {
                                    MaNguoiDung = Convert.ToInt32(reader["ma_nd"]),
                                    HoTen = reader["ho_ten"].ToString(),
                                    DiaChi = reader["dia_chi"]?.ToString() ?? "",
                                    SoDT = reader["so_dt"]?.ToString() ?? "",
                                    Email = reader["email"]?.ToString() ?? "",
                                    LoaiNguoiDung = reader["loai_nguoi_dung"].ToString()
                                };
                            }
                        }

                        // Lấy người dùng Độc Giả theo Email
                        command.CommandText = @"
                            SELECT nd.ma_nd, nd.ho_ten, nd.dia_chi, nd.so_dt, nd.email, nd.loai_nguoi_dung
                            FROM NGUOI_DUNG nd
                            JOIN DOC_GIA dg ON nd.ma_nd = dg.ma_nd
                            WHERE nd.email = @username AND nd.loai_nguoi_dung = 'DOC_GIA'";
                        
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new Models.User
                                {
                                    MaNguoiDung = Convert.ToInt32(reader["ma_nd"]),
                                    HoTen = reader["ho_ten"].ToString(),
                                    DiaChi = reader["dia_chi"]?.ToString() ?? "",
                                    SoDT = reader["so_dt"]?.ToString() ?? "",
                                    Email = reader["email"]?.ToString() ?? "",
                                    LoaiNguoiDung = reader["loai_nguoi_dung"].ToString()
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[QLTV] GetUserByUsername error: {ex.Message}");
            }

            return null;
        }
    }
}
