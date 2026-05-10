using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using QLTV_covert_2._0.Data;
using QLTV_covert_2._0.Utilities;

namespace QLTV_covert_2._0.Forms
{
    public partial class ReaderManagementForm : Form
    {
        private LibraryService _service;
        private Guna2DataGridView _grid;
        private Guna2TextBox _searchBox;
        private Guna2ComboBox _cardStatusFilter;

        private Guna2Button _btnEditReader;
        private Guna2Button _btnDeleteReader;
        private Guna2Button _btnCreateCard;
        private Guna2Button _btnRenewCard;
        private Guna2Button _btnPrintCard;
        private Guna2Button _btnHistory;
        private Guna2Button _btnResetPassword;

        public ReaderManagementForm()
        {
            InitializeComponent();
            _service = new LibraryService(DatabaseManager.Instance);
            SetupUI();
            LoadReaders();
        }

        private void SetupUI()
        {
            Dock = DockStyle.Fill;
            BackColor = AppTheme.ContentBg;
            Padding = new Padding(24, 20, 24, 16);
            Font = AppTheme.BodyMedium;

            // ═══════════════════════════════════════════════════════════
            //  ROW 1 — HEADER: Title left, "Add Reader" right
            // ═══════════════════════════════════════════════════════════
            var headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 56,
                BackColor = Color.Transparent
            };
            Controls.Add(headerPanel);

            headerPanel.Controls.Add(new Label
            {
                Text = "👥 Quản lý Độc giả",
                Font = AppTheme.HeadingLarge,
                ForeColor = AppTheme.TextDark,
                AutoSize = true,
                Location = new Point(0, 12)
            });

            var addBtn = MakeHeaderButton("➕ Thêm đọc giả", AppTheme.Primary, AddReader);
            addBtn.Width = 150;
            addBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            headerPanel.Controls.Add(addBtn);
            headerPanel.Resize += (s, e) =>
            {
                addBtn.Location = new Point(headerPanel.Width - addBtn.Width - 2, 10);
            };
            addBtn.Location = new Point(headerPanel.Width - addBtn.Width - 2, 10);

            // ═══════════════════════════════════════════════════════════
            //  ROW 2 — FILTER: Search + Card Status + Refresh
            // ═══════════════════════════════════════════════════════════
            var filterPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 52,
                ColumnCount = 4,
                RowCount = 1,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 6, 0, 6)
            };
            filterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 300)); // search
            filterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));  // label
            filterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));  // combo
            filterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 48));  // refresh
            Controls.Add(filterPanel);
            filterPanel.BringToFront(); // Fix docking z-order so header stays on top

            _searchBox = new Guna2TextBox
            {
                Dock = DockStyle.Fill,
                Height = 36,
                PlaceholderText = "🔍 Tìm theo tên, mã đọc giả, SĐT...",
                BorderRadius = 6,
                BorderColor = AppTheme.BorderLight,
                FocusedState = { BorderColor = AppTheme.Primary }
            };
            _searchBox.TextChanged += (s, e) => LoadReaders();
            filterPanel.Controls.Add(_searchBox, 0, 0);

            filterPanel.Controls.Add(new Label
            {
                Text = "Trạng thái thẻ:",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                Font = AppTheme.BodySmall,
                ForeColor = AppTheme.TextDarkSecondary,
                Padding = new Padding(0, 0, 4, 0)
            }, 1, 0);

            _cardStatusFilter = new Guna2ComboBox
            {
                Dock = DockStyle.Fill,
                Height = 36,
                BorderRadius = 6,
                BorderColor = AppTheme.BorderLight,
                FocusedState = { BorderColor = AppTheme.Primary }
            };
            _cardStatusFilter.Items.AddRange(new object[] { "Tất cả", "Hoạt động", "Hết hạn", "Khóa" });
            _cardStatusFilter.SelectedIndex = 0;
            _cardStatusFilter.SelectedIndexChanged += (s, e) => LoadReaders();
            filterPanel.Controls.Add(_cardStatusFilter, 2, 0);

            var refreshBtn = new Guna2Button
            {
                Text = "🔄",
                Dock = DockStyle.Fill,
                Height = 36,
                BorderRadius = 6,
                FillColor = AppTheme.Info,
                ForeColor = Color.White,
                Font = AppTheme.BodyMedium,
                Cursor = Cursors.Hand
            };
            refreshBtn.HoverState.FillColor = ControlPaint.Light(AppTheme.Info);
            refreshBtn.Click += (s, e) => { _searchBox.Text = ""; _cardStatusFilter.SelectedIndex = 0; LoadReaders(); };
            filterPanel.Controls.Add(refreshBtn, 3, 0);

            // ═══════════════════════════════════════════════════════════
            //  BOTTOM ACTION BAR
            // ═══════════════════════════════════════════════════════════
            var actionPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 52,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0, 8, 0, 8),
                BackColor = Color.Transparent
            };
            Controls.Add(actionPanel);

            _btnEditReader = MakeButton("✏️ Sửa", AppTheme.Info, EditReader);
            _btnDeleteReader = MakeButton("🗑️ Xóa", AppTheme.Danger, DeleteReader);
            _btnCreateCard = MakeButton("💳 Lập thẻ", AppTheme.Success, CreateCard);
            _btnRenewCard = MakeButton("🪪 Gia hạn", Color.FromArgb(23, 162, 184), UpdateCard);
            _btnPrintCard = MakeButton("🖨️ In thẻ", Color.MediumPurple, PrintCard);
            _btnHistory = MakeButton("📜 Lịch sử mượn trả", Color.FromArgb(108, 117, 125), ShowHistory);
            _btnResetPassword = MakeButton("🔑 Đặt lại MK", Color.DarkOrange, ResetPassword);

            actionPanel.Controls.Add(_btnEditReader);
            actionPanel.Controls.Add(_btnDeleteReader);
            actionPanel.Controls.Add(_btnCreateCard);
            actionPanel.Controls.Add(_btnRenewCard);
            actionPanel.Controls.Add(_btnPrintCard);
            actionPanel.Controls.Add(_btnHistory);
            actionPanel.Controls.Add(_btnResetPassword);

            // ═══════════════════════════════════════════════════════════
            //  DATA GRID — fills remaining space
            // ═══════════════════════════════════════════════════════════
            _grid = new Guna2DataGridView { Dock = DockStyle.Fill };
            AppTheme.StyleGrid(_grid);
            _grid.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) EditReader(); };
            _grid.SelectionChanged += Grid_SelectionChanged;
            Controls.Add(_grid);
            _grid.BringToFront();
        }

        private void LoadReaders()
        {
            string search = _searchBox?.Text?.Trim() ?? "";
            string cardFilter = _cardStatusFilter?.SelectedItem?.ToString() ?? "Tất cả";

            string cardSql = "";
            switch (cardFilter)
            {
                case "Hoạt động": cardSql = " AND COALESCE(t.trang_thai_the, 'KHONG_CO') = 'HOAT_DONG'"; break;
                case "Hết hạn": cardSql = " AND COALESCE(t.trang_thai_the, 'KHONG_CO') = 'HET_HAN'"; break;
                case "Khóa": cardSql = " AND COALESCE(t.trang_thai_the, 'KHONG_CO') = 'KHOA'"; break;
            }

            _grid.DataSource = _service.Query(@"
                SELECT dg.ma_doc_gia AS 'Mã ĐG', nd.ho_ten AS 'Họ tên', nd.so_dt AS 'SĐT', nd.email AS 'Email',
                       strftime('%d/%m/%Y', dg.ngay_dk) AS 'Ngày ĐK',
                       CASE COALESCE(t.trang_thai_the, 'KHONG_CO')
                           WHEN 'HOAT_DONG' THEN '✅ Hoạt động'
                           WHEN 'HET_HAN' THEN '⚠️ Hết hạn'
                           WHEN 'KHOA' THEN '🔒 Khóa'
                           ELSE 'N/A'
                       END AS 'Trạng thái thẻ',
                       nd.ma_nd AS 'MaNd'
                FROM DOC_GIA dg
                JOIN NGUOI_DUNG nd ON dg.ma_nd = nd.ma_nd
                LEFT JOIN THE_DOC_GIA t ON dg.ma_nd = t.ma_nd_doc_gia
                WHERE (nd.ho_ten LIKE @s OR dg.ma_doc_gia LIKE @s OR nd.so_dt LIKE @s)" + cardSql + @"
                ORDER BY dg.ma_nd DESC", new SQLiteParameter("@s", "%" + search + "%"));

            if (_grid.Columns.Contains("MaNd"))
                _grid.Columns["MaNd"].Visible = false;

            Grid_SelectionChanged(null, null);
        }

        private void Grid_SelectionChanged(object sender, EventArgs e)
        {
            if (_grid.SelectedRows.Count == 0) return;
            var row = _grid.SelectedRows[0];
            string status = row.Cells["Trạng thái thẻ"].Value?.ToString() ?? "";

            if (status == "N/A" || status.Contains("N/A"))
            {
                _btnCreateCard.Enabled = true;
                _btnRenewCard.Enabled = false;
                _btnPrintCard.Enabled = false;
            }
            else
            {
                _btnCreateCard.Enabled = false;
                _btnRenewCard.Enabled = true;
                _btnPrintCard.Enabled = status.Contains("Hoạt động");
            }
        }

        private int GetSelectedUserId()
        {
            if (_grid.SelectedRows.Count == 0) { MessageBox.Show("Vui lòng chọn một dòng."); return 0; }
            return Convert.ToInt32(_grid.SelectedRows[0].Cells["MaNd"].Value);
        }

        private void AddReader()
        {
            using (var dialog = new ReaderEditDialog(_service))
                if (dialog.ShowDialog(this) == DialogResult.OK) LoadReaders();
        }

        private void EditReader()
        {
            int id = GetSelectedUserId();
            if (id == 0) return;
            using (var dialog = new ReaderEditDialog(_service, id))
                if (dialog.ShowDialog(this) == DialogResult.OK) LoadReaders();
        }

        private void DeleteReader()
        {
            int id = GetSelectedUserId();
            if (id == 0) return;
            if (MessageBox.Show("Bạn có chắc chắn muốn xóa đọc giả này?\nMọi dữ liệu liên quan sẽ bị xóa!", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    _service.Execute("DELETE FROM NGUOI_DUNG WHERE ma_nd = @id", new SQLiteParameter("@id", id));
                    MessageBox.Show("Xóa đọc giả thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadReaders();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }

        private void CreateCard()
        {
            int id = GetSelectedUserId();
            if (id == 0) return;
            
            if (MessageBox.Show("Xác nhận lập thẻ cho độc giả này với thời hạn 6 tháng? (Giá: xxxVNĐ)", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    _service.Execute(@"
                        INSERT INTO THE_DOC_GIA (ma_nd_doc_gia, ngay_cap, ngay_het_han, trang_thai_the) 
                        VALUES (@id, date('now'), date('now', '+6 months'), 'HOAT_DONG')",
                        new SQLiteParameter("@id", id));
                    MessageBox.Show("Lập thẻ thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadReaders();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }

        private void PrintCard()
        {
            if (_grid.SelectedRows.Count == 0) return;
            var row = _grid.SelectedRows[0];
            string status = row.Cells["Trạng thái thẻ"].Value?.ToString() ?? "";
            
            if (status.Contains("Hoạt động"))
            {
                int userId = Convert.ToInt32(row.Cells["MaNd"].Value);
                _service.Execute(@"
                    INSERT INTO YEU_CAU_THE (ma_nd_doc_gia, ngay_yeu_cau, trang_thai)
                    VALUES (@id, datetime('now'), 'DANG_XU_LY')", 
                    new SQLiteParameter("@id", userId));
                MessageBox.Show("Đã thêm yêu cầu in thẻ vào hàng chờ xử lý. Thời gian in dự kiến là 7 ngày.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadReaders();
            }
        }

        private void ResetPassword()
        {
            if (_grid.SelectedRows.Count == 0) return;
            var row = _grid.SelectedRows[0];
            int userId = Convert.ToInt32(row.Cells["MaNd"].Value);
            string readerCode = row.Cells["Mã ĐG"].Value?.ToString() ?? "";

            if (MessageBox.Show($"Bạn có chắc chắn muốn đặt lại mật khẩu cho độc giả {readerCode} về mặc định không?\n(Mật khẩu mặc định: {readerCode.ToLower()})", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                string newPassHash = DatabaseManager.HashPassword(readerCode.ToLower());
                _service.Execute("UPDATE TAI_KHOAN SET mat_khau = @pass WHERE ma_nd = @id",
                    new SQLiteParameter("@pass", newPassHash),
                    new SQLiteParameter("@id", userId));
                MessageBox.Show("Đã đặt lại mật khẩu thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void UpdateCard()
        {
            int id = GetSelectedUserId();
            if (id == 0) return;

            if (MessageBox.Show("Gia hạn thẻ cho độc giả thêm 6 tháng? Phí gia hạn: xxxVNĐ", "Xác nhận gia hạn", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    _service.Execute(@"
                        UPDATE THE_DOC_GIA 
                        SET ngay_het_han = date(ngay_het_han, '+6 months'), trang_thai_the = 'HOAT_DONG'
                        WHERE ma_nd_doc_gia = @id",
                        new SQLiteParameter("@id", id));
                    MessageBox.Show("Gia hạn thẻ thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadReaders();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }

        private void ShowHistory()
        {
            int id = GetSelectedUserId();
            if (id == 0) return;

            using (var historyForm = new Form())
            {
                historyForm.Text = "Lịch sử mượn sách";
                historyForm.Size = new Size(700, 400);
                historyForm.StartPosition = FormStartPosition.CenterParent;

                var grid = new Guna2DataGridView { Dock = DockStyle.Fill };
                AppTheme.StyleGrid(grid);
                historyForm.Controls.Add(grid);

                grid.DataSource = _service.Query(@"
                    SELECT p.ma_phieu AS 'Mã phiếu', s.tieu_de AS 'Tên sách',
                           strftime('%d/%m/%Y', p.ngay_muon) AS 'Ngày mượn', strftime('%d/%m/%Y', p.ngay_hen_tra) AS 'Hạn trả',
                           COALESCE(strftime('%d/%m/%Y', p.ngay_tra_thuc), 'Chưa trả') AS 'Ngày trả',
                           CASE p.trang_thai_phieu
                               WHEN 'DA_TRA' THEN '✅ Đã trả'
                               WHEN 'DANG_MUON' THEN CASE WHEN date(p.ngay_hen_tra) < date('now') THEN '⚠️ Quá hạn' ELSE '📖 Đang mượn' END
                               ELSE p.trang_thai_phieu
                           END AS 'Trạng thái'
                    FROM PHIEU_MUON_TRA p
                    JOIN SACH s ON p.ma_sach = s.ma_sach
                    WHERE p.ma_nd_doc_gia = @id
                    ORDER BY p.ma_phieu DESC", new SQLiteParameter("@id", id));

                grid.CellDoubleClick += (s, e) =>
                {
                    if (e.RowIndex >= 0)
                    {
                        int phieuId = Convert.ToInt32(grid.Rows[e.RowIndex].Cells["Mã phiếu"].Value);
                        ViewBorrowDetail(phieuId);
                    }
                };

                historyForm.ShowDialog(this);
            }
        }

        private void ViewBorrowDetail(int id)
        {
            DataTable table = _service.Query(@"
                SELECT p.*, nd.ho_ten, dg.ma_doc_gia, s.tieu_de, s.tac_gia,
                       tl.ten_the_loai, nv_nd.ho_ten AS ten_nhan_vien, nv.ma_nhan_vien,
                       CASE 
                           WHEN p.trang_thai_phieu = 'DA_TRA' THEN COALESCE(p.tien_phat, 0)
                           WHEN julianday('now') > julianday(p.ngay_hen_tra) THEN CAST((julianday('now') - julianday(p.ngay_hen_tra)) * 5000 AS INTEGER)
                           ELSE 0
                       END AS calculated_fine
                FROM PHIEU_MUON_TRA p
                JOIN NGUOI_DUNG nd ON p.ma_nd_doc_gia = nd.ma_nd
                JOIN DOC_GIA dg ON p.ma_nd_doc_gia = dg.ma_nd
                JOIN SACH s ON p.ma_sach = s.ma_sach
                LEFT JOIN THE_LOAI_SACH tl ON s.ma_the_loai = tl.ma_the_loai
                LEFT JOIN NHAN_VIEN nv ON p.ma_nd_nhan_vien = nv.ma_nd
                LEFT JOIN NGUOI_DUNG nv_nd ON nv.ma_nd = nv_nd.ma_nd
                WHERE p.ma_phieu = @id", new SQLiteParameter("@id", id));

            if (table.Rows.Count == 0) return;
            DataRow row = table.Rows[0];

            string info = $"═══ PHIẾU MƯỢN #{row["ma_phieu"]} ═══\n\n" +
                           $"📚 Đọc giả: {row["ho_ten"]} ({row["ma_doc_gia"]})\n" +
                           $"📖 Sách: {row["tieu_de"]} — {row["tac_gia"]}\n" +
                           $"📁 Thể loại: {row["ten_the_loai"]}\n\n" +
                           $"📅 Ngày mượn: {(row["ngay_muon"] != DBNull.Value ? Convert.ToDateTime(row["ngay_muon"]).ToString("dd/MM/yyyy") : "")}\n" +
                           $"📅 Hạn trả: {(row["ngay_hen_tra"] != DBNull.Value ? Convert.ToDateTime(row["ngay_hen_tra"]).ToString("dd/MM/yyyy") : "")}\n" +
                           $"📅 Ngày trả: {(row["ngay_tra_thuc"] == DBNull.Value ? "Chưa trả" : Convert.ToDateTime(row["ngay_tra_thuc"]).ToString("dd/MM/yyyy"))}\n" +
                           $"💰 Tiền phạt tạm tính: {(row["calculated_fine"] == DBNull.Value ? "0đ" : Convert.ToDecimal(row["calculated_fine"]).ToString("N0", new System.Globalization.CultureInfo("vi-VN")) + "đ")}\n\n" +
                           $"👤 NV xử lý: {row["ten_nhan_vien"]} ({row["ma_nhan_vien"]})";

            MessageBox.Show(info, "Chi tiết phiếu mượn", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ═══════════════════════════════════════════════════════════════
        //  UI HELPER FACTORIES
        // ═══════════════════════════════════════════════════════════════

        private Guna2Button MakeHeaderButton(string text, Color color, Action action)
        {
            var btn = new Guna2Button
            {
                Text = text,
                Height = 36,
                BorderRadius = 6,
                FillColor = color,
                ForeColor = Color.White,
                Font = AppTheme.ButtonSmall,
                Cursor = Cursors.Hand,
                Animated = true,
                Padding = new Padding(10, 0, 10, 0)
            };
            btn.HoverState.FillColor = ControlPaint.Light(color, 0.15f);
            btn.Click += (s, e) =>
            {
                try { action(); }
                catch (Exception ex) { MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            };
            return btn;
        }

        private Guna2Button MakeButton(string text, Color color, Action action)
        {
            var btn = new Guna2Button
            {
                Text = text,
                Width = 120,
                Height = 36,
                BorderRadius = 6,
                FillColor = color,
                ForeColor = Color.White,
                Font = AppTheme.ButtonSmall,
                Margin = new Padding(0, 0, 8, 0),
                Cursor = Cursors.Hand,
                Animated = true
            };
            btn.HoverState.FillColor = ControlPaint.Light(color, 0.15f);
            btn.Click += (s, e) =>
            {
                try { action(); }
                catch (Exception ex) { MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            };
            return btn;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(800, 450);
            this.Name = "ReaderManagementForm";
            this.ResumeLayout(false);
        }
    }
}
