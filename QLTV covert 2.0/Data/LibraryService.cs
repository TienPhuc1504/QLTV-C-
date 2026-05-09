using System;
using System.Data;
using System.Data.SQLite;

namespace QLTV_covert_2._0.Data
{
    public class LibraryService
    {
        private readonly DatabaseManager _db;

        public LibraryService(DatabaseManager db)
        {
            _db = db;
        }

        public DataTable Query(string sql, params SQLiteParameter[] parameters)
        {
            using (var connection = _db.GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = sql;
                    if (parameters != null)
                        command.Parameters.AddRange(parameters);

                    using (var adapter = new SQLiteDataAdapter(command))
                    {
                        var table = new DataTable();
                        adapter.Fill(table);
                        return table;
                    }
                }
            }
        }

        public int Execute(string sql, params SQLiteParameter[] parameters)
        {
            using (var connection = _db.GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = sql;
                    if (parameters != null)
                        command.Parameters.AddRange(parameters);

                    return command.ExecuteNonQuery();
                }
            }
        }

        public object Scalar(string sql, params SQLiteParameter[] parameters)
        {
            using (var connection = _db.GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = sql;
                    if (parameters != null)
                        command.Parameters.AddRange(parameters);

                    return command.ExecuteScalar();
                }
            }
        }

        public int ScalarInt(string sql, params SQLiteParameter[] parameters)
        {
            object value = Scalar(sql, parameters);
            return value == null || value == DBNull.Value ? 0 : Convert.ToInt32(value);
        }

        public decimal ScalarDecimal(string sql, params SQLiteParameter[] parameters)
        {
            object value = Scalar(sql, parameters);
            return value == null || value == DBNull.Value ? 0 : Convert.ToDecimal(value);
        }

        public bool RegisterUser(string username, string password, string fullName, string address, string phone, string email)
        {
            using (var connection = _db.GetConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (var command = connection.CreateCommand())
                        {
                            command.Transaction = transaction;
                            command.CommandText = "SELECT COUNT(*) FROM TAI_KHOAN WHERE ten_tk = @username";
                            command.Parameters.AddWithValue("@username", username);
                            if (Convert.ToInt32(command.ExecuteScalar()) > 0)
                                throw new InvalidOperationException("Tên đăng nhập đã tồn tại.");
                        }

                        int userId;
                        using (var command = connection.CreateCommand())
                        {
                            command.Transaction = transaction;
                            command.CommandText = @"
                                INSERT INTO NGUOI_DUNG (ho_ten, dia_chi, so_dt, email, loai_nguoi_dung)
                                VALUES (@name, @address, @phone, @email, 'DOC_GIA');
                                SELECT last_insert_rowid();";
                            command.Parameters.AddWithValue("@name", fullName);
                            command.Parameters.AddWithValue("@address", address ?? "");
                            command.Parameters.AddWithValue("@phone", phone ?? "");
                            command.Parameters.AddWithValue("@email", email ?? "");
                            userId = Convert.ToInt32(command.ExecuteScalar());
                        }

                        string readerCode = $"DG{userId:0000}";
                        using (var command = connection.CreateCommand())
                        {
                            command.Transaction = transaction;
                            command.CommandText = @"
                                INSERT INTO DOC_GIA (ma_nd, ma_doc_gia, ngay_dk) VALUES (@id, @code, date('now'));
                                INSERT INTO THE_DOC_GIA (ma_nd_doc_gia, ngay_cap, ngay_het_han, trang_thai_the)
                                VALUES (@id, date('now'), date('now', '+1 year'), 'HOAT_DONG');
                                INSERT INTO TAI_KHOAN (ten_tk, mat_khau, ma_nd) VALUES (@username, @password, @id);";
                            command.Parameters.AddWithValue("@id", userId);
                            command.Parameters.AddWithValue("@code", readerCode);
                            command.Parameters.AddWithValue("@username", username);
                            command.Parameters.AddWithValue("@password", DatabaseManager.HashPassword(password));
                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public bool AddBook(string title, string author, int categoryId, string type, int quantity, string onlineUrl)
        {
            using (var connection = _db.GetConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        int bookId;
                        using (var command = connection.CreateCommand())
                        {
                            command.Transaction = transaction;
                            command.CommandText = @"
                                INSERT INTO SACH (tieu_de, tac_gia, trang_thai_sach, ma_the_loai, loai_sach)
                                VALUES (@title, @author, 'CO_SAN', @category, @type);
                                SELECT last_insert_rowid();";
                            command.Parameters.AddWithValue("@title", title);
                            command.Parameters.AddWithValue("@author", author ?? "");
                            command.Parameters.AddWithValue("@category", categoryId > 0 ? (object)categoryId : DBNull.Value);
                            command.Parameters.AddWithValue("@type", type);
                            bookId = Convert.ToInt32(command.ExecuteScalar());
                        }

                        if (type == "SACH_GIAY")
                        {
                            using (var command = connection.CreateCommand())
                            {
                                command.Transaction = transaction;
                                command.CommandText = "INSERT INTO SACH_GIAY (ma_sach, so_luong) VALUES (@id, @quantity)";
                                command.Parameters.AddWithValue("@id", bookId);
                                command.Parameters.AddWithValue("@quantity", quantity);
                                command.ExecuteNonQuery();
                            }

                            for (int i = 1; i <= Math.Max(1, quantity); i++)
                            {
                                using (var command = connection.CreateCommand())
                                {
                                    command.Transaction = transaction;
                                    command.CommandText = @"
                                        INSERT INTO QUYEN_SACH (ma_sach, ma_quyen_sach, trang_thai, ngay_nhap)
                                        VALUES (@id, @code, 'CO_SAN', date('now'))";
                                    command.Parameters.AddWithValue("@id", bookId);
                                    command.Parameters.AddWithValue("@code", $"S{bookId:0000}-Q{i:000}");
                                    command.ExecuteNonQuery();
                                }
                            }
                        }
                        else
                        {
                            using (var command = connection.CreateCommand())
                            {
                                command.Transaction = transaction;
                                command.CommandText = "INSERT INTO SACH_ONLINE (ma_sach, url_tai_lieu, dinh_dang) VALUES (@id, @url, 'PDF')";
                                command.Parameters.AddWithValue("@id", bookId);
                                command.Parameters.AddWithValue("@url", onlineUrl ?? "");
                                command.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public bool UpdateBook(int bookId, string title, string author, int categoryId, string type, string status)
        {
            return Execute(@"
                UPDATE SACH
                SET tieu_de = @title, tac_gia = @author, ma_the_loai = @category, loai_sach = @type, trang_thai_sach = @status
                WHERE ma_sach = @id",
                new SQLiteParameter("@title", title),
                new SQLiteParameter("@author", author ?? ""),
                new SQLiteParameter("@category", categoryId > 0 ? (object)categoryId : DBNull.Value),
                new SQLiteParameter("@type", type),
                new SQLiteParameter("@status", status),
                new SQLiteParameter("@id", bookId)) > 0;
        }

        public bool DeleteBook(int bookId)
        {
            // Kiểm tra quyển sách đang được mượn
            int activeBorrows = ScalarInt(
                "SELECT COUNT(*) FROM QUYEN_SACH WHERE ma_sach = @id AND trang_thai = 'DANG_MUON'",
                new SQLiteParameter("@id", bookId));
            if (activeBorrows > 0)
                throw new InvalidOperationException("Không thể xóa sách đang được mượn.");

            // Kiểm tra yêu cầu mượn đang chờ
            int pendingRequests = ScalarInt(
                "SELECT COUNT(*) FROM YEU_CAU_MUON WHERE ma_sach = @id AND trang_thai IN ('CHO_DUYET', 'CHO_LAY_SACH')",
                new SQLiteParameter("@id", bookId));
            if (pendingRequests > 0)
                throw new InvalidOperationException("Không thể xóa sách có yêu cầu mượn đang chờ xử lý.");

            return Execute("DELETE FROM SACH WHERE ma_sach = @id", new SQLiteParameter("@id", bookId)) > 0;
        }

        public bool CreateBorrow(int readerId, int staffId, int bookId, int days)
        {
            using (var connection = _db.GetConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        int copyId = 0;
                        using (var command = connection.CreateCommand())
                        {
                            command.Transaction = transaction;
                            command.CommandText = "SELECT ma_quyen FROM QUYEN_SACH WHERE ma_sach = @book AND trang_thai = 'CO_SAN' ORDER BY ma_quyen LIMIT 1";
                            command.Parameters.AddWithValue("@book", bookId);
                            object value = command.ExecuteScalar();
                            if (value == null || value == DBNull.Value)
                                throw new InvalidOperationException("Sách đã hết bản có sẵn.");
                            copyId = Convert.ToInt32(value);
                        }

                        using (var command = connection.CreateCommand())
                        {
                            command.Transaction = transaction;
                            command.CommandText = @"
                                INSERT INTO PHIEU_MUON_TRA (ma_nd_doc_gia, ma_nd_nhan_vien, ma_sach, ma_quyen, ngay_muon, ngay_hen_tra, trang_thai_phieu)
                                VALUES (@reader, @staff, @book, @copy, date('now'), date('now', '+' || @days || ' days'), 'DANG_MUON')";
                            command.Parameters.AddWithValue("@reader", readerId);
                            command.Parameters.AddWithValue("@staff", staffId);
                            command.Parameters.AddWithValue("@book", bookId);
                            command.Parameters.AddWithValue("@copy", copyId);
                            command.Parameters.AddWithValue("@days", days);
                            command.ExecuteNonQuery();
                        }

                        using (var command = connection.CreateCommand())
                        {
                            command.Transaction = transaction;
                            command.CommandText = "UPDATE QUYEN_SACH SET trang_thai = 'DANG_MUON' WHERE ma_quyen = @copy";
                            command.Parameters.AddWithValue("@copy", copyId);
                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public bool ReturnBook(int borrowId, decimal fine)
        {
            using (var connection = _db.GetConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        int copyId = 0;
                        using (var command = connection.CreateCommand())
                        {
                            command.Transaction = transaction;
                            command.CommandText = "SELECT ma_quyen FROM PHIEU_MUON_TRA WHERE ma_phieu = @id";
                            command.Parameters.AddWithValue("@id", borrowId);
                            object value = command.ExecuteScalar();
                            copyId = value == null || value == DBNull.Value ? 0 : Convert.ToInt32(value);
                        }

                        using (var command = connection.CreateCommand())
                        {
                            command.Transaction = transaction;
                            command.CommandText = @"
                                UPDATE PHIEU_MUON_TRA
                                SET ngay_tra_thuc = date('now'), trang_thai_phieu = 'DA_TRA', tien_phat = @fine
                                WHERE ma_phieu = @id";
                            command.Parameters.AddWithValue("@id", borrowId);
                            command.Parameters.AddWithValue("@fine", fine);
                            command.ExecuteNonQuery();
                        }

                        if (copyId > 0)
                        {
                            using (var command = connection.CreateCommand())
                            {
                                command.Transaction = transaction;
                                command.CommandText = "UPDATE QUYEN_SACH SET trang_thai = 'CO_SAN' WHERE ma_quyen = @copy";
                                command.Parameters.AddWithValue("@copy", copyId);
                                command.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        private const int MAX_ACTIVE_BORROWS = 5;
        private const decimal FINE_PER_DAY = 5000m;

        /// <summary>
        /// Calculates the overdue fine for a borrow record (5000₫/day).
        /// </summary>
        public decimal CalculateFine(int borrowId)
        {
            return ScalarDecimal(@"
                SELECT CASE WHEN julianday('now') > julianday(ngay_hen_tra)
                    THEN (julianday('now') - julianday(ngay_hen_tra)) * @rate ELSE 0 END
                FROM PHIEU_MUON_TRA WHERE ma_phieu = @id",
                new SQLiteParameter("@id", borrowId),
                new SQLiteParameter("@rate", FINE_PER_DAY));
        }

        /// <summary>
        /// Creates a borrow request with full validation matching the Python implementation.
        /// </summary>
        public void CreateBorrowRequest(int readerId, int bookId, int days, string note)
        {
            if (days <= 0 || days > 30)
                throw new InvalidOperationException("Số ngày mượn phải từ 1-30 ngày!");

            using (var connection = _db.GetConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 1. Check active borrow count
                        int activeBorrows = ScalarIntTx(connection, transaction,
                            "SELECT COUNT(*) FROM PHIEU_MUON_TRA WHERE ma_nd_doc_gia = @id AND trang_thai_phieu = 'DANG_MUON'",
                            new SQLiteParameter("@id", readerId));

                        // 2. Check pending request count
                        int pendingRequests = ScalarIntTx(connection, transaction,
                            "SELECT COUNT(*) FROM YEU_CAU_MUON WHERE ma_nd_doc_gia = @id AND trang_thai IN ('CHO_DUYET', 'CHO_LAY_SACH')",
                            new SQLiteParameter("@id", readerId));

                        if (activeBorrows + pendingRequests >= MAX_ACTIVE_BORROWS)
                            throw new InvalidOperationException(
                                $"Bạn chỉ có thể mượn tối đa {MAX_ACTIVE_BORROWS} sách cùng lúc!\n" +
                                $"Hiện tại: {activeBorrows} đang mượn, {pendingRequests} đang chờ xử lý.");

                        // 3. Check if already borrowing this book
                        int alreadyBorrowing = ScalarIntTx(connection, transaction,
                            "SELECT COUNT(*) FROM PHIEU_MUON_TRA WHERE ma_nd_doc_gia = @id AND ma_sach = @book AND trang_thai_phieu = 'DANG_MUON'",
                            new SQLiteParameter("@id", readerId),
                            new SQLiteParameter("@book", bookId));
                        if (alreadyBorrowing > 0)
                            throw new InvalidOperationException("Bạn đang mượn sách này! Vui lòng trả sách trước khi mượn lại.");

                        // 4. Check if already has pending request for this book
                        int existingRequest = ScalarIntTx(connection, transaction,
                            "SELECT COUNT(*) FROM YEU_CAU_MUON WHERE ma_nd_doc_gia = @id AND ma_sach = @book AND trang_thai IN ('CHO_DUYET', 'CHO_LAY_SACH')",
                            new SQLiteParameter("@id", readerId),
                            new SQLiteParameter("@book", bookId));
                        if (existingRequest > 0)
                            throw new InvalidOperationException("Bạn đã có yêu cầu mượn sách này đang chờ xử lý!");

                        // 5. Check book availability
                        DataTable bookInfo = QueryTx(connection, transaction, @"
                            SELECT s.trang_thai_sach, s.loai_sach,
                                   (SELECT COUNT(*) FROM QUYEN_SACH q WHERE q.ma_sach = s.ma_sach AND q.trang_thai = 'CO_SAN') AS so_quyen_co_san
                            FROM SACH s WHERE s.ma_sach = @book",
                            new SQLiteParameter("@book", bookId));

                        if (bookInfo.Rows.Count == 0)
                            throw new InvalidOperationException("Không tìm thấy sách!");

                        DataRow book = bookInfo.Rows[0];
                        string status = book["trang_thai_sach"].ToString();
                        string bookType = book["loai_sach"].ToString();

                        if (status == "HONG" || status == "MAT")
                            throw new InvalidOperationException("Sách không khả dụng (hỏng hoặc mất)!");

                        if (bookType == "SACH_GIAY")
                        {
                            int available = book["so_quyen_co_san"] == DBNull.Value ? 0 : Convert.ToInt32(book["so_quyen_co_san"]);
                            if (available <= 0)
                                throw new InvalidOperationException("Sách đã hết! Vui lòng chọn sách khác.");
                        }

                        // 6. Insert the request
                        ExecuteTx(connection, transaction, @"
                            INSERT INTO YEU_CAU_MUON (ma_nd_doc_gia, ma_sach, ngay_yeu_cau, so_ngay_muon_de_xuat, ghi_chu, trang_thai)
                            VALUES (@reader, @book, date('now'), @days, @note, 'CHO_DUYET')",
                            new SQLiteParameter("@reader", readerId),
                            new SQLiteParameter("@book", bookId),
                            new SQLiteParameter("@days", days),
                            new SQLiteParameter("@note", note ?? ""));

                        int requestId = ScalarIntTx(connection, transaction, "SELECT last_insert_rowid()");

                        // 7. Reserve a copy for paper books
                        if (bookType == "SACH_GIAY")
                        {
                            object copyVal = ScalarTx(connection, transaction,
                                "SELECT ma_quyen FROM QUYEN_SACH WHERE ma_sach = @book AND trang_thai = 'CO_SAN' ORDER BY ma_quyen LIMIT 1",
                                new SQLiteParameter("@book", bookId));
                            if (copyVal != null && copyVal != DBNull.Value)
                            {
                                int copyId = Convert.ToInt32(copyVal);
                                ExecuteTx(connection, transaction,
                                    "UPDATE QUYEN_SACH SET trang_thai = 'KHONG_CO_SAN' WHERE ma_quyen = @copy",
                                    new SQLiteParameter("@copy", copyId));
                            }
                        }

                        // 8. Create notifications for all staff
                        string bookTitle = ScalarTx(connection, transaction,
                            "SELECT tieu_de FROM SACH WHERE ma_sach = @id",
                            new SQLiteParameter("@id", bookId))?.ToString() ?? "Sách";
                        string readerName = ScalarTx(connection, transaction,
                            "SELECT ho_ten FROM NGUOI_DUNG WHERE ma_nd = @id",
                            new SQLiteParameter("@id", readerId))?.ToString() ?? "Đọc giả";

                        DataTable staffList = QueryTx(connection, transaction,
                            "SELECT ma_nd FROM NGUOI_DUNG WHERE loai_nguoi_dung IN ('NHAN_VIEN', 'ADMIN')");
                        foreach (DataRow staff in staffList.Rows)
                        {
                            ExecuteTx(connection, transaction, @"
                                INSERT INTO THONG_BAO (ma_nd, tieu_de, noi_dung, loai_thong_bao, ngay_tao, link_lien_quan)
                                VALUES (@staffId, @title, @content, 'YEU_CAU_MOI', datetime('now'), @link)",
                                new SQLiteParameter("@staffId", staff["ma_nd"]),
                                new SQLiteParameter("@title", "📬 Yêu cầu mượn sách mới"),
                                new SQLiteParameter("@content", $"Đọc giả {readerName} yêu cầu mượn sách \"{bookTitle}\" trong {days} ngày."),
                                new SQLiteParameter("@link", $"yeu_cau:{requestId}"));
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public void ApproveBorrowRequest(int requestId, int staffId, int days)
        {
            if (days <= 0 || days > 30)
                throw new InvalidOperationException("Số ngày mượn phải từ 1-30 ngày!");

            using (var connection = _db.GetConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Verify request exists and is pending
                        DataTable reqInfo = QueryTx(connection, transaction,
                            "SELECT yc.*, s.loai_sach FROM YEU_CAU_MUON yc JOIN SACH s ON yc.ma_sach = s.ma_sach WHERE yc.ma_yeu_cau = @id",
                            new SQLiteParameter("@id", requestId));
                        if (reqInfo.Rows.Count == 0)
                            throw new InvalidOperationException("Không tìm thấy yêu cầu!");
                        DataRow req = reqInfo.Rows[0];
                        if (req["trang_thai"].ToString() != "CHO_DUYET")
                            throw new InvalidOperationException("Yêu cầu này đã được xử lý!");

                        // Update request status
                        ExecuteTx(connection, transaction, @"
                            UPDATE YEU_CAU_MUON
                            SET trang_thai = 'CHO_LAY_SACH', ma_nd_xu_ly = @staff, ngay_xu_ly = date('now'),
                                so_ngay_muon_chinh_thuc = @days, han_lay_sach = datetime('now', '+3 days')
                            WHERE ma_yeu_cau = @id",
                            new SQLiteParameter("@staff", staffId),
                            new SQLiteParameter("@days", days),
                            new SQLiteParameter("@id", requestId));

                        // Clean up old notifications
                        ExecuteTx(connection, transaction,
                            "DELETE FROM THONG_BAO WHERE link_lien_quan = @link AND loai_thong_bao = 'YEU_CAU_MOI'",
                            new SQLiteParameter("@link", $"yeu_cau:{requestId}"));

                        // Notify reader
                        string bookTitle = ScalarTx(connection, transaction,
                            "SELECT tieu_de FROM SACH WHERE ma_sach = @id",
                            new SQLiteParameter("@id", req["ma_sach"]))?.ToString() ?? "Sách";

                        ExecuteTx(connection, transaction, @"
                            INSERT INTO THONG_BAO (ma_nd, tieu_de, noi_dung, loai_thong_bao, ngay_tao, link_lien_quan)
                            VALUES (@readerId, @title, @content, 'YEU_CAU_DUYET', datetime('now'), @link)",
                            new SQLiteParameter("@readerId", req["ma_nd_doc_gia"]),
                            new SQLiteParameter("@title", "✅ Yêu cầu mượn sách đã được duyệt!"),
                            new SQLiteParameter("@content", $"Yêu cầu mượn sách \"{bookTitle}\" đã được duyệt với thời hạn {days} ngày.\n⏰ Vui lòng đến thư viện lấy sách trong 3 ngày."),
                            new SQLiteParameter("@link", $"yeu_cau:{requestId}"));

                        transaction.Commit();
                    }
                    catch { transaction.Rollback(); throw; }
                }
            }
        }

        public void ConfirmBorrowPickup(int requestId, int staffId)
        {
            DataTable table = Query("SELECT yc.*, s.loai_sach FROM YEU_CAU_MUON yc JOIN SACH s ON yc.ma_sach = s.ma_sach WHERE yc.ma_yeu_cau = @id",
                new SQLiteParameter("@id", requestId));
            if (table.Rows.Count == 0)
                throw new InvalidOperationException("Không tìm thấy yêu cầu.");
            DataRow row = table.Rows[0];
            if (row["trang_thai"].ToString() != "CHO_LAY_SACH")
                throw new InvalidOperationException("Yêu cầu này chưa được duyệt hoặc đã lấy sách!");

            int readerId = Convert.ToInt32(row["ma_nd_doc_gia"]);
            int bookId = Convert.ToInt32(row["ma_sach"]);
            int borrowDays = row["so_ngay_muon_chinh_thuc"] != DBNull.Value
                ? Convert.ToInt32(row["so_ngay_muon_chinh_thuc"])
                : Convert.ToInt32(row["so_ngay_muon_de_xuat"]);

            CreateBorrow(readerId, staffId, bookId, borrowDays);
            Execute("UPDATE YEU_CAU_MUON SET trang_thai = 'DA_LAY' WHERE ma_yeu_cau = @id",
                new SQLiteParameter("@id", requestId));

            // Clean up approval notifications
            Execute("DELETE FROM THONG_BAO WHERE link_lien_quan = @link AND loai_thong_bao IN ('YEU_CAU_DUYET', 'CHO_LAY_SACH')",
                new SQLiteParameter("@link", $"yeu_cau:{requestId}"));

            // Notify reader about successful pickup
            string bookTitle = Scalar("SELECT tieu_de FROM SACH WHERE ma_sach = @id",
                new SQLiteParameter("@id", bookId))?.ToString() ?? "Sách";
            string dueDate = DateTime.Now.AddDays(borrowDays).ToString("dd/MM/yyyy");

            Execute(@"
                INSERT INTO THONG_BAO (ma_nd, tieu_de, noi_dung, loai_thong_bao, ngay_tao, link_lien_quan)
                VALUES (@readerId, @title, @content, 'HE_THONG', datetime('now'), 'phieu_muon:new')",
                new SQLiteParameter("@readerId", readerId),
                new SQLiteParameter("@title", "📚 Bạn đã lấy sách thành công!"),
                new SQLiteParameter("@content", $"Bạn đã mượn sách \"{bookTitle}\". Hạn trả: {dueDate}. Vui lòng trả sách đúng hạn!"));
        }

        public void RejectBorrowRequest(int requestId, int staffId, string reason)
        {
            using (var connection = _db.GetConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        DataTable reqInfo = QueryTx(connection, transaction,
                            "SELECT trang_thai, ma_nd_doc_gia, ma_sach FROM YEU_CAU_MUON WHERE ma_yeu_cau = @id",
                            new SQLiteParameter("@id", requestId));
                        if (reqInfo.Rows.Count == 0)
                            throw new InvalidOperationException("Không tìm thấy yêu cầu!");
                        DataRow req = reqInfo.Rows[0];
                        if (req["trang_thai"].ToString() != "CHO_DUYET")
                            throw new InvalidOperationException("Yêu cầu này đã được xử lý!");

                        // Update status
                        ExecuteTx(connection, transaction, @"
                            UPDATE YEU_CAU_MUON
                            SET trang_thai = 'TU_CHOI', ma_nd_xu_ly = @staff, ngay_xu_ly = date('now'), ly_do_tu_choi = @reason
                            WHERE ma_yeu_cau = @id",
                            new SQLiteParameter("@staff", staffId),
                            new SQLiteParameter("@reason", reason ?? ""),
                            new SQLiteParameter("@id", requestId));

                        // Release reserved copy back to CO_SAN
                        ExecuteTx(connection, transaction, @"
                            UPDATE QUYEN_SACH SET trang_thai = 'CO_SAN'
                            WHERE ma_quyen IN (
                                SELECT q.ma_quyen FROM QUYEN_SACH q
                                JOIN SACH s ON q.ma_sach = s.ma_sach
                                WHERE s.ma_sach = @bookId AND q.trang_thai = 'KHONG_CO_SAN'
                                LIMIT 1
                            )",
                            new SQLiteParameter("@bookId", req["ma_sach"]));

                        // Clean up old notifications
                        ExecuteTx(connection, transaction,
                            "DELETE FROM THONG_BAO WHERE link_lien_quan = @link AND loai_thong_bao = 'YEU_CAU_MOI'",
                            new SQLiteParameter("@link", $"yeu_cau:{requestId}"));

                        // Notify reader
                        string bookTitle = ScalarTx(connection, transaction,
                            "SELECT tieu_de FROM SACH WHERE ma_sach = @id",
                            new SQLiteParameter("@id", req["ma_sach"]))?.ToString() ?? "Sách";
                        string content = $"Yêu cầu mượn sách \"{bookTitle}\" của bạn đã bị từ chối.";
                        if (!string.IsNullOrWhiteSpace(reason)) content += $" Lý do: {reason}";

                        ExecuteTx(connection, transaction, @"
                            INSERT INTO THONG_BAO (ma_nd, tieu_de, noi_dung, loai_thong_bao, ngay_tao, link_lien_quan)
                            VALUES (@readerId, @title, @content, 'YEU_CAU_TU_CHOI', datetime('now'), @link)",
                            new SQLiteParameter("@readerId", req["ma_nd_doc_gia"]),
                            new SQLiteParameter("@title", "❌ Yêu cầu mượn sách bị từ chối"),
                            new SQLiteParameter("@content", content),
                            new SQLiteParameter("@link", $"yeu_cau:{requestId}"));

                        transaction.Commit();
                    }
                    catch { transaction.Rollback(); throw; }
                }
            }
        }

        public void CancelBorrowRequest(int requestId, int readerId)
        {
            // Release any reserved copy
            DataTable reqInfo = Query("SELECT ma_sach, trang_thai FROM YEU_CAU_MUON WHERE ma_yeu_cau = @id AND ma_nd_doc_gia = @reader",
                new SQLiteParameter("@id", requestId),
                new SQLiteParameter("@reader", readerId));

            if (reqInfo.Rows.Count > 0)
            {
                string status = reqInfo.Rows[0]["trang_thai"].ToString();
                if (status != "CHO_DUYET" && status != "CHO_LAY_SACH")
                    throw new InvalidOperationException("Không thể hủy yêu cầu đã xử lý!");
            }

            Execute("UPDATE YEU_CAU_MUON SET trang_thai = 'DA_HUY' WHERE ma_yeu_cau = @id AND ma_nd_doc_gia = @reader AND trang_thai IN ('CHO_DUYET', 'CHO_LAY_SACH')",
                new SQLiteParameter("@id", requestId),
                new SQLiteParameter("@reader", readerId));

            // Clean up notifications
            Execute("DELETE FROM THONG_BAO WHERE link_lien_quan = @link",
                new SQLiteParameter("@link", $"yeu_cau:{requestId}"));
        }

        public bool ChangePassword(int userId, string oldPassword, string newPassword)
        {
            if (string.IsNullOrEmpty(newPassword) || newPassword.Length < 4)
                throw new InvalidOperationException("Mật khẩu mới phải có ít nhất 4 ký tự.");

            string oldHash = DatabaseManager.HashPassword(oldPassword);
            int count = ScalarInt("SELECT COUNT(*) FROM TAI_KHOAN WHERE ma_nd = @id AND mat_khau = @old",
                new SQLiteParameter("@id", userId),
                new SQLiteParameter("@old", oldHash));
            if (count == 0)
                return false;

            Execute("UPDATE TAI_KHOAN SET mat_khau = @new WHERE ma_nd = @id",
                new SQLiteParameter("@new", DatabaseManager.HashPassword(newPassword)),
                new SQLiteParameter("@id", userId));
            return true;
        }

        // ═══════════════════════════════════════════════════════════════
        //  TRANSACTION HELPER METHODS
        // ═══════════════════════════════════════════════════════════════

        private int ScalarIntTx(SQLiteConnection conn, SQLiteTransaction tx, string sql, params SQLiteParameter[] parameters)
        {
            object value = ScalarTx(conn, tx, sql, parameters);
            return value == null || value == DBNull.Value ? 0 : Convert.ToInt32(value);
        }

        private object ScalarTx(SQLiteConnection conn, SQLiteTransaction tx, string sql, params SQLiteParameter[] parameters)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = sql;
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                return cmd.ExecuteScalar();
            }
        }

        private int ExecuteTx(SQLiteConnection conn, SQLiteTransaction tx, string sql, params SQLiteParameter[] parameters)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = sql;
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                return cmd.ExecuteNonQuery();
            }
        }

        private DataTable QueryTx(SQLiteConnection conn, SQLiteTransaction tx, string sql, params SQLiteParameter[] parameters)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = sql;
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                using (var adapter = new SQLiteDataAdapter(cmd))
                {
                    var table = new DataTable();
                    adapter.Fill(table);
                    return table;
                }
            }
        }
    }
}
