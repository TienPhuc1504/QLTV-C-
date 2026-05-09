namespace QLTV_covert_2._0.Models
{
    public class Book
    {
        public int MaSach { get; set; }
        public string TieuDe { get; set; }
        public string TacGia { get; set; }
        public string TrangThaiSach { get; set; } // CO_SAN, DA_MUON, HONG, MAT
        public int MaTheLoai { get; set; }
        public string LoaiSach { get; set; } // SACH_GIAY, SACH_ONLINE
        public Category Category { get; set; }
    }

    public class BookCopy
    {
        public int MaQuyen { get; set; }
        public int MaSach { get; set; }
        public string MaQuyenSach { get; set; }
        public string TrangThai { get; set; } // CO_SAN, DANG_MUON, HONG, MAT
        public string ViTri { get; set; }
        public string GhiChu { get; set; }
        public string NgayNhap { get; set; }
        public Book Book { get; set; }
    }

    public class Category
    {
        public int MaTheLoai { get; set; }
        public string TenTheLoai { get; set; }
    }

    public class PaperBook
    {
        public int MaSach { get; set; }
        public int SoLuong { get; set; }
        public Book Book { get; set; }
    }

    public class OnlineBook
    {
        public int MaSach { get; set; }
        public string UrlTaiLieu { get; set; }
        public string DinhDang { get; set; }
        public Book Book { get; set; }
    }
}
