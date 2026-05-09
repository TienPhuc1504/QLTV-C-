using System;
using System.Data;
using System.Data.SQLite;
using System.Collections.Generic;
using System.Diagnostics;
using QLTV_covert_2._0.Models;

namespace QLTV_covert_2._0.Data
{
    public class BookRepository
    {
        private readonly DatabaseManager _db;

        public BookRepository(DatabaseManager db)
        {
            _db = db;
        }

        public List<Book> GetAllBooks()
        {
            List<Book> books = new List<Book>();
            try
            {
                using (var connection = _db.GetConnection())
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                            SELECT s.ma_sach, s.tieu_de, s.tac_gia, s.trang_thai_sach, 
                                   s.ma_the_loai, s.loai_sach, tl.ten_the_loai
                            FROM SACH s
                            LEFT JOIN THE_LOAI_SACH tl ON s.ma_the_loai = tl.ma_the_loai
                            ORDER BY s.tieu_de";

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                books.Add(new Book
                                {
                                    MaSach = Convert.ToInt32(reader["ma_sach"]),
                                    TieuDe = reader["tieu_de"].ToString(),
                                    TacGia = reader["tac_gia"]?.ToString() ?? "",
                                    TrangThaiSach = reader["trang_thai_sach"].ToString(),
                                    MaTheLoai = reader["ma_the_loai"] == DBNull.Value ? 0 : Convert.ToInt32(reader["ma_the_loai"]),
                                    LoaiSach = reader["loai_sach"].ToString(),
                                    Category = new Category
                                    {
                                        TenTheLoai = reader["ten_the_loai"]?.ToString() ?? "Không xác định"
                                    }
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[QLTV] GetAllBooks error: {ex.Message}");
            }

            return books;
        }

        public Book GetBookById(int maSach)
        {
            try
            {
                using (var connection = _db.GetConnection())
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                            SELECT s.ma_sach, s.tieu_de, s.tac_gia, s.trang_thai_sach, 
                                   s.ma_the_loai, s.loai_sach
                            FROM SACH s
                            WHERE s.ma_sach = @maSach";
                        command.Parameters.AddWithValue("@maSach", maSach);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new Book
                                {
                                    MaSach = Convert.ToInt32(reader["ma_sach"]),
                                    TieuDe = reader["tieu_de"].ToString(),
                                    TacGia = reader["tac_gia"]?.ToString() ?? "",
                                    TrangThaiSach = reader["trang_thai_sach"].ToString(),
                                    MaTheLoai = reader["ma_the_loai"] == DBNull.Value ? 0 : Convert.ToInt32(reader["ma_the_loai"]),
                                    LoaiSach = reader["loai_sach"].ToString()
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[QLTV] GetBookById error: {ex.Message}");
            }

            return null;
        }

        public bool AddBook(Book book)
        {
            try
            {
                using (var connection = _db.GetConnection())
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                            INSERT INTO SACH (tieu_de, tac_gia, trang_thai_sach, ma_the_loai, loai_sach)
                            VALUES (@tieuDe, @tacGia, @trangThaiSach, @maTheLoai, @loaiSach)";

                        command.Parameters.AddWithValue("@tieuDe", book.TieuDe);
                        command.Parameters.AddWithValue("@tacGia", book.TacGia ?? "");
                        command.Parameters.AddWithValue("@trangThaiSach", book.TrangThaiSach);
                        object maTheLoaiValue = (book.MaTheLoai > 0) ? (object)book.MaTheLoai : DBNull.Value;
                        command.Parameters.AddWithValue("@maTheLoai", maTheLoaiValue);
                        command.Parameters.AddWithValue("@loaiSach", book.LoaiSach);

                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        public bool UpdateBook(Book book)
        {
            try
            {
                using (var connection = _db.GetConnection())
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                            UPDATE SACH 
                            SET tieu_de = @tieuDe, tac_gia = @tacGia, trang_thai_sach = @trangThaiSach, 
                                ma_the_loai = @maTheLoai, loai_sach = @loaiSach
                            WHERE ma_sach = @maSach";

                        command.Parameters.AddWithValue("@tieuDe", book.TieuDe);
                        command.Parameters.AddWithValue("@tacGia", book.TacGia ?? "");
                        command.Parameters.AddWithValue("@trangThaiSach", book.TrangThaiSach);
                        object maTheLoaiValue = (book.MaTheLoai > 0) ? (object)book.MaTheLoai : DBNull.Value;
                        command.Parameters.AddWithValue("@maTheLoai", maTheLoaiValue);
                        command.Parameters.AddWithValue("@loaiSach", book.LoaiSach);
                        command.Parameters.AddWithValue("@maSach", book.MaSach);

                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        public bool DeleteBook(int maSach)
        {
            try
            {
                using (var connection = _db.GetConnection())
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "DELETE FROM SACH WHERE ma_sach = @maSach";
                        command.Parameters.AddWithValue("@maSach", maSach);

                        return command.ExecuteNonQuery() > 0;
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
