using System;
using System.Data;
using System.Data.SQLite;
using System.Collections.Generic;
using QLTV_covert_2._0.Models;

namespace QLTV_covert_2._0.Data
{
    public class BorrowRepository
    {
        private readonly DatabaseManager _db;

        public BorrowRepository(DatabaseManager db)
        {
            _db = db;
        }

        public List<Borrow> GetAllBorrows()
        {
            List<Borrow> borrows = new List<Borrow>();
            try
            {
                using (var connection = _db.GetConnection())
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                            SELECT ma_phieu, ma_nd_doc_gia, ma_nd_nhan_vien, ma_sach,
                                   ma_quyen, ngay_muon, ngay_hen_tra,
                                   ngay_tra_thuc, trang_thai_phieu, tien_phat
                            FROM PHIEU_MUON_TRA
                            ORDER BY ngay_muon DESC";

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                borrows.Add(new Borrow
                                {
                                    MaPhieu = Convert.ToInt32(reader["ma_phieu"]),
                                    MaNguoiDung = Convert.ToInt32(reader["ma_nd_doc_gia"]),
                                    MaNhanVien = Convert.ToInt32(reader["ma_nd_nhan_vien"]),
                                    MaSach = Convert.ToInt32(reader["ma_sach"]),
                                    MaQuyen = reader["ma_quyen"] == DBNull.Value ? 0 : Convert.ToInt32(reader["ma_quyen"]),
                                    NgayMuon = reader["ngay_muon"].ToString(),
                                    NgayHenTra = reader["ngay_hen_tra"].ToString(),
                                    NgayTraThuc = reader["ngay_tra_thuc"]?.ToString() ?? "",
                                    TrangThaiPhieu = reader["trang_thai_phieu"].ToString(),
                                    TienPhat = reader["tien_phat"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tien_phat"])
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[QLTV] GetAllBorrows error: {ex.Message}");
            }

            return borrows;
        }

        public bool AddBorrow(int maNguoiDung, int maQuyen, int soNgayMuon)
        {
            try
            {
                using (var connection = _db.GetConnection())
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                            INSERT INTO PHIEU_MUON_TRA (
                                ma_nd_doc_gia, ma_nd_nhan_vien, ma_sach, ma_quyen,
                                ngay_muon, ngay_hen_tra, trang_thai_phieu
                            )
                            SELECT
                                @maNguoiDung,
                                COALESCE((SELECT ma_nd FROM NHAN_VIEN ORDER BY ma_nd LIMIT 1), @maNguoiDung),
                                ma_sach,
                                @maQuyen,
                                date('now'),
                                date('now', '+' || @soNgay || ' days'),
                                'DANG_MUON'
                            FROM QUYEN_SACH
                            WHERE ma_quyen = @maQuyen";

                        command.Parameters.AddWithValue("@maNguoiDung", maNguoiDung);
                        command.Parameters.AddWithValue("@maQuyen", maQuyen);
                        command.Parameters.AddWithValue("@soNgay", soNgayMuon);

                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[QLTV] AddBorrow error: {ex.Message}");
                return false;
            }
        }

        public bool ReturnBook(int maPhieu)
        {
            try
            {
                using (var connection = _db.GetConnection())
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Cập nhật phiếu mượn
                            using (var command = connection.CreateCommand())
                            {
                                command.CommandText = @"
                                    UPDATE PHIEU_MUON_TRA 
                                    SET ngay_tra_thuc = date('now'), trang_thai_phieu = 'DA_TRA'
                                    WHERE ma_phieu = @maPhieu";

                                command.Parameters.AddWithValue("@maPhieu", maPhieu);
                                command.ExecuteNonQuery();
                            }

                            // Cập nhật trạng thái quyển sách
                            using (var command = connection.CreateCommand())
                            {
                                command.CommandText = @"
                                    UPDATE QUYEN_SACH 
                                    SET trang_thai = 'CO_SAN'
                                    WHERE ma_quyen = (SELECT ma_quyen FROM PHIEU_MUON_TRA WHERE ma_phieu = @maPhieu)";

                                command.Parameters.AddWithValue("@maPhieu", maPhieu);
                                command.ExecuteNonQuery();
                            }

                            transaction.Commit();
                            return true;
                        }
                        catch
                        {
                            transaction.Rollback();
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[QLTV] ReturnBook error: {ex.Message}");
                return false;
            }
        }

        public List<Borrow> GetOverdueBorrows()
        {
            List<Borrow> borrows = new List<Borrow>();
            try
            {
                using (var connection = _db.GetConnection())
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                            SELECT ma_phieu, ma_nd_doc_gia, ma_nd_nhan_vien, ma_sach,
                                   ma_quyen, ngay_muon, ngay_hen_tra,
                                   ngay_tra_thuc, trang_thai_phieu, tien_phat
                            FROM PHIEU_MUON_TRA
                            WHERE ngay_hen_tra < date('now') AND ngay_tra_thuc IS NULL
                            ORDER BY ngay_hen_tra ASC";

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                borrows.Add(new Borrow
                                {
                                    MaPhieu = Convert.ToInt32(reader["ma_phieu"]),
                                    MaNguoiDung = Convert.ToInt32(reader["ma_nd_doc_gia"]),
                                    MaNhanVien = Convert.ToInt32(reader["ma_nd_nhan_vien"]),
                                    MaSach = Convert.ToInt32(reader["ma_sach"]),
                                    MaQuyen = reader["ma_quyen"] == DBNull.Value ? 0 : Convert.ToInt32(reader["ma_quyen"]),
                                    NgayMuon = reader["ngay_muon"].ToString(),
                                    NgayHenTra = reader["ngay_hen_tra"].ToString(),
                                    NgayTraThuc = reader["ngay_tra_thuc"]?.ToString() ?? "",
                                    TrangThaiPhieu = reader["trang_thai_phieu"].ToString(),
                                    TienPhat = reader["tien_phat"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tien_phat"])
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[QLTV] GetOverdueBorrows error: {ex.Message}");
            }

            return borrows;
        }
    }
}
