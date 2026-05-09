namespace QLTV_covert_2._0.Models
{
    public class User
    {
        public int MaNguoiDung { get; set; }
        public string HoTen { get; set; }
        public string DiaChi { get; set; }
        public string SoDT { get; set; }
        public string Email { get; set; }
        public string LoaiNguoiDung { get; set; } // ADMIN, NHAN_VIEN, DOC_GIA
    }

    public class Account
    {
        public string TenTaiKhoan { get; set; }
        public string MatKhau { get; set; }
        public int MaNguoiDung { get; set; }
        public User User { get; set; }
    }

    public class Reader
    {
        public int MaNguoiDung { get; set; }
        public string MaDocGia { get; set; }
        public string NgayDangKy { get; set; }
        public User User { get; set; }
    }

    public class Staff
    {
        public int MaNguoiDung { get; set; }
        public string MaNhanVien { get; set; }
        public User User { get; set; }
    }
}
