using System;
using System.Data;
using System.Data.SQLite;
using System.Collections.Generic;
using QLTV_covert_2._0.Models;

namespace QLTV_covert_2._0.Data
{
    public class ReaderRepository
    {
        private readonly DatabaseManager _db;

        public ReaderRepository(DatabaseManager db)
        {
            _db = db;
        }

        public List<Reader> GetAllReaders()
        {
            List<Reader> readers = new List<Reader>();
            try
            {
                using (var connection = _db.GetConnection())
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                            SELECT dg.ma_nd, dg.ma_doc_gia, dg.ngay_dk, 
                                   nd.ho_ten, nd.dia_chi, nd.so_dt, nd.email
                            FROM DOC_GIA dg
                            JOIN NGUOI_DUNG nd ON dg.ma_nd = nd.ma_nd
                            ORDER BY nd.ho_ten";

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                readers.Add(new Reader
                                {
                                    MaNguoiDung = Convert.ToInt32(reader["ma_nd"]),
                                    MaDocGia = reader["ma_doc_gia"].ToString(),
                                    NgayDangKy = reader["ngay_dk"].ToString(),
                                    User = new User
                                    {
                                        MaNguoiDung = Convert.ToInt32(reader["ma_nd"]),
                                        HoTen = reader["ho_ten"].ToString(),
                                        DiaChi = reader["dia_chi"]?.ToString() ?? "",
                                        SoDT = reader["so_dt"]?.ToString() ?? "",
                                        Email = reader["email"]?.ToString() ?? ""
                                    }
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[QLTV] GetAllReaders error: {ex.Message}");
            }

            return readers;
        }

        public Reader GetReaderById(int maNguoiDung)
        {
            try
            {
                using (var connection = _db.GetConnection())
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                            SELECT dg.ma_nd, dg.ma_doc_gia, dg.ngay_dk, 
                                   nd.ho_ten, nd.dia_chi, nd.so_dt, nd.email
                            FROM DOC_GIA dg
                            JOIN NGUOI_DUNG nd ON dg.ma_nd = nd.ma_nd
                            WHERE dg.ma_nd = @maNguoiDung";
                        command.Parameters.AddWithValue("@maNguoiDung", maNguoiDung);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new Reader
                                {
                                    MaNguoiDung = Convert.ToInt32(reader["ma_nd"]),
                                    MaDocGia = reader["ma_doc_gia"].ToString(),
                                    NgayDangKy = reader["ngay_dk"].ToString(),
                                    User = new User
                                    {
                                        MaNguoiDung = Convert.ToInt32(reader["ma_nd"]),
                                        HoTen = reader["ho_ten"].ToString(),
                                        DiaChi = reader["dia_chi"]?.ToString() ?? "",
                                        SoDT = reader["so_dt"]?.ToString() ?? "",
                                        Email = reader["email"]?.ToString() ?? ""
                                    }
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[QLTV] GetReaderById error: {ex.Message}");
            }

            return null;
        }

        public bool AddReader(User user, string maDocGia)
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
                            // Thêm vào NGUOI_DUNG
                            using (var command = connection.CreateCommand())
                            {
                                command.CommandText = @"
                                    INSERT INTO NGUOI_DUNG (ho_ten, dia_chi, so_dt, email, loai_nguoi_dung)
                                    VALUES (@hoTen, @diaChi, @soDT, @email, 'DOC_GIA')";

                                command.Parameters.AddWithValue("@hoTen", user.HoTen);
                                command.Parameters.AddWithValue("@diaChi", user.DiaChi ?? "");
                                command.Parameters.AddWithValue("@soDT", user.SoDT ?? "");
                                command.Parameters.AddWithValue("@email", user.Email ?? "");

                                command.ExecuteNonQuery();
                            }

                            // Lấy ID vừa tạo
                            int newUserId;
                            using (var command = connection.CreateCommand())
                            {
                                command.CommandText = "SELECT last_insert_rowid()";
                                newUserId = Convert.ToInt32(command.ExecuteScalar());
                            }

                            // Thêm vào DOC_GIA
                            using (var command = connection.CreateCommand())
                            {
                                command.CommandText = @"
                                    INSERT INTO DOC_GIA (ma_nd, ma_doc_gia, ngay_dk)
                                    VALUES (@maNguoiDung, @maDocGia, date('now'))";

                                command.Parameters.AddWithValue("@maNguoiDung", newUserId);
                                command.Parameters.AddWithValue("@maDocGia", maDocGia);

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
            catch
            {
                return false;
            }
        }
    }
}
