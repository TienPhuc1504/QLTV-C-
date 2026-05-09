namespace QLTV_covert_2._0.Models
{
    public class Borrow
    {
        public int MaPhieu { get; set; }
        public int MaNguoiDung { get; set; }      // ma_nd_doc_gia
        public int MaNhanVien { get; set; }        // ma_nd_nhan_vien
        public int MaSach { get; set; }            // ma_sach
        public int MaQuyen { get; set; }           // ma_quyen
        public string NgayMuon { get; set; }
        public string NgayHenTra { get; set; }
        public string NgayTraThuc { get; set; }
        public string TrangThaiPhieu { get; set; } // DANG_MUON, DA_TRA, QUA_HAN
        public decimal TienPhat { get; set; }      // tien_phat
    }

    public class ReaderCard
    {
        public int MaThe { get; set; }
        public int MaNguoiDungDocGia { get; set; }
        public string NgayCap { get; set; }
        public string NgayHetHan { get; set; }
        public string TrangThaiThe { get; set; } // HOAT_DONG, HET_HAN, KHOA
    }

    public class BorrowRequest
    {
        public int MaYeuCau { get; set; }
        public int MaNguoiDung { get; set; }       // ma_nd_doc_gia
        public int MaSach { get; set; }
        public string NgayYeuCau { get; set; }
        public int SoNgayMuonDeXuat { get; set; }  // so_ngay_muon_de_xuat
        public string GhiChu { get; set; }
        public string TrangThai { get; set; }      // CHO_DUYET, CHO_LAY_SACH, DA_LAY, TU_CHOI, DA_HUY
        public int MaNguoiXuLy { get; set; }       // ma_nd_xu_ly
        public string NgayXuLy { get; set; }
        public string LyDoTuChoi { get; set; }
        public int SoNgayMuonChinhThuc { get; set; }
        public string HanLaySach { get; set; }
    }
}
