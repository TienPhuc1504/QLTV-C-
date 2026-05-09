using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using QLTV_covert_2._0.Data;
using QLTV_covert_2._0.Models;
using QLTV_covert_2._0.Utilities;

namespace QLTV_covert_2._0.Forms
{
    public partial class BorrowManagementForm : Form
    {
        private readonly User _user;
        private LibraryService _service;
        private Guna2DataGridView _grid;
        private Guna2TextBox _searchBox;
        private Guna2ComboBox _statusFilter;

        public BorrowManagementForm(User user)
        {
            InitializeComponent();
            _user = user;
            _service = new LibraryService(DatabaseManager.Instance);
            SetupUI();
            LoadBorrows();
        }

        private void SetupUI()
        {
            Dock = DockStyle.Fill;
            BackColor = AppTheme.ContentBg;
            Padding = new Padding(24, 20, 24, 16);
            Font = AppTheme.BodyMedium;

            // ═══════════════════════════════════════════════════════════
            //  ROW 1 — HEADER: Title left, "Create Borrow" right
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
                Text = "📋 Quản lý Mượn/Trả Sách",
                Font = AppTheme.HeadingLarge,
                ForeColor = AppTheme.TextDark,
                AutoSize = true,
                Location = new Point(0, 12)
            });

            var addBtn = MakeHeaderButton("➕ Tạo phiếu mượn", AppTheme.Primary, CreateBorrow);
            addBtn.Width = 160;
            addBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            headerPanel.Controls.Add(addBtn);
            headerPanel.Resize += (s, e) =>
            {
                addBtn.Location = new Point(headerPanel.Width - addBtn.Width - 2, 10);
            };
            addBtn.Location = new Point(headerPanel.Width - addBtn.Width - 2, 10);

            // ═══════════════════════════════════════════════════════════
            //  ROW 2 — FILTER: Search + Status + Refresh
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
            filterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 320)); // search
            filterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));  // label
            filterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));  // combo
            filterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 48));  // refresh
            Controls.Add(filterPanel);
            filterPanel.BringToFront(); // Fix docking z-order so header stays on top

            _searchBox = new Guna2TextBox
            {
                Dock = DockStyle.Fill,
                Height = 36,
                PlaceholderText = "🔍 Tìm theo tên đọc giả, mã ĐG, tên sách...",
                BorderRadius = 6,
                BorderColor = AppTheme.BorderLight,
                FocusedState = { BorderColor = AppTheme.Primary }
            };
            _searchBox.TextChanged += (s, e) => LoadBorrows();
            filterPanel.Controls.Add(_searchBox, 0, 0);

            filterPanel.Controls.Add(new Label
            {
                Text = "Trạng thái:",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                Font = AppTheme.BodySmall,
                ForeColor = AppTheme.TextDarkSecondary,
                Padding = new Padding(0, 0, 4, 0)
            }, 1, 0);

            _statusFilter = new Guna2ComboBox
            {
                Dock = DockStyle.Fill,
                Height = 36,
                BorderRadius = 6,
                BorderColor = AppTheme.BorderLight,
                FocusedState = { BorderColor = AppTheme.Primary }
            };
            _statusFilter.Items.AddRange(new object[] { "Tất cả", "Đang mượn", "Đã trả", "Quá hạn" });
            _statusFilter.SelectedIndex = 0;
            _statusFilter.SelectedIndexChanged += (s, e) => LoadBorrows();
            filterPanel.Controls.Add(_statusFilter, 2, 0);

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
            refreshBtn.Click += (s, e) => { _searchBox.Text = ""; _statusFilter.SelectedIndex = 0; LoadBorrows(); };
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

            actionPanel.Controls.Add(MakeButton("📥 Trả sách", AppTheme.Success, ReturnBook));
            actionPanel.Controls.Add(MakeButton("👁️ Chi tiết", AppTheme.Info, ViewDetail));

            // ═══════════════════════════════════════════════════════════
            //  DATA GRID — fills remaining space
            // ═══════════════════════════════════════════════════════════
            _grid = new Guna2DataGridView { Dock = DockStyle.Fill };
            AppTheme.StyleGrid(_grid);
            _grid.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) ViewDetail(); };
            Controls.Add(_grid);
            _grid.BringToFront();
        }

        private void LoadBorrows()
        {
            string search = _searchBox?.Text?.Trim() ?? "";
            string statusName = _statusFilter?.SelectedItem?.ToString() ?? "Tất cả";

            string statusFilter = "";
            switch (statusName)
            {
                case "Đang mượn": statusFilter = " AND p.trang_thai_phieu = 'DANG_MUON'"; break;
                case "Đã trả": statusFilter = " AND p.trang_thai_phieu = 'DA_TRA'"; break;
                case "Quá hạn": statusFilter = " AND p.trang_thai_phieu IN ('DANG_MUON', 'QUA_HAN') AND date(p.ngay_hen_tra) < date('now')"; break;
            }

            _grid.DataSource = _service.Query(@"
                SELECT p.ma_phieu AS 'Mã phiếu', dg.ma_doc_gia AS 'Mã ĐG',
                       nd.ho_ten AS 'Tên đọc giả', s.tieu_de AS 'Tên sách',
                       strftime('%d/%m/%Y', p.ngay_muon) AS 'Ngày mượn', strftime('%d/%m/%Y', p.ngay_hen_tra) AS 'Hạn trả',
                       COALESCE(strftime('%d/%m/%Y', p.ngay_tra_thuc), '-') AS 'Ngày trả',
                       CASE
                           WHEN p.trang_thai_phieu = 'DA_TRA' THEN '✅ Đã trả'
                           WHEN date(p.ngay_hen_tra) < date('now') THEN '⚠️ Quá hạn'
                           ELSE '📖 Đang mượn'
                       END AS 'Trạng thái',
                       COALESCE(p.tien_phat, 0) AS 'Tiền phạt'
                FROM PHIEU_MUON_TRA p
                JOIN NGUOI_DUNG nd ON p.ma_nd_doc_gia = nd.ma_nd
                JOIN DOC_GIA dg ON p.ma_nd_doc_gia = dg.ma_nd
                JOIN SACH s ON p.ma_sach = s.ma_sach
                WHERE (nd.ho_ten LIKE @s OR dg.ma_doc_gia LIKE @s OR s.tieu_de LIKE @s)" + statusFilter + @"
                ORDER BY p.ma_phieu DESC", new SQLiteParameter("@s", "%" + search + "%"));
        }

        private int GetSelectedId()
        {
            if (_grid.SelectedRows.Count == 0) { MessageBox.Show("Vui lòng chọn một dòng."); return 0; }
            return Convert.ToInt32(_grid.SelectedRows[0].Cells["Mã phiếu"].Value);
        }

        private void CreateBorrow()
        {
            using (var dialog = new BorrowEditDialog(_service, _user))
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    MessageBox.Show("Đã tạo yêu cầu mượn sách! Vui lòng sang tab 'Yêu cầu mượn sách' để duyệt và xác nhận lấy sách.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadBorrows();
                }
            }
        }

        private void ReturnBook()
        {
            int id = GetSelectedId();
            if (id == 0) return;

            string status = _grid.SelectedRows[0].Cells["Trạng thái"].Value.ToString();
            if (status.Contains("Đã trả"))
            {
                MessageBox.Show("Phiếu này đã được trả.", "Thông báo");
                return;
            }

            // Tính tiền phạt
            decimal fine = _service.ScalarDecimal(@"
                SELECT CASE WHEN julianday('now') > julianday(ngay_hen_tra)
                    THEN (julianday('now') - julianday(ngay_hen_tra)) * 5000 ELSE 0 END
                FROM PHIEU_MUON_TRA WHERE ma_phieu = @id", new SQLiteParameter("@id", id));

            string msg = $"Xác nhận trả sách — Mã phiếu #{id}";
            if (fine > 0)
                msg += $"\n⚠️ TIỀN PHẠT QUÁ HẠN: {fine:N0} VNĐ";
            else
                msg += "\n✅ Trả đúng hạn — Không phạt";

            if (MessageBox.Show(msg, "Xác nhận trả sách", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    _service.ReturnBook(id, fine);
                    MessageBox.Show("Trả sách thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadBorrows();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }

        private void ViewDetail()
        {
            int id = GetSelectedId();
            if (id == 0) return;

            DataTable table = _service.Query(@"
                SELECT p.*, nd.ho_ten, dg.ma_doc_gia, s.tieu_de, s.tac_gia,
                       tl.ten_the_loai, nv_nd.ho_ten AS ten_nhan_vien, nv.ma_nhan_vien
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
                           $"💰 Tiền phạt: {(row["tien_phat"] == DBNull.Value ? 0 : Convert.ToDecimal(row["tien_phat"])):N0} VNĐ\n\n" +
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
            this.Name = "BorrowManagementForm";
            this.ResumeLayout(false);
        }
    }
}
