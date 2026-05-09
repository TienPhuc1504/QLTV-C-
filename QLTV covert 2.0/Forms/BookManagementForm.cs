using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;
using FontAwesome.Sharp;
using Guna.UI2.WinForms;
using QLTV_covert_2._0.Data;
using QLTV_covert_2._0.Utilities;

namespace QLTV_covert_2._0.Forms
{
    public partial class BookManagementForm : Form
    {
        private DatabaseManager _db;
        private LibraryService _service;
        private Guna2DataGridView _grid;
        private Guna2TextBox _searchBox;
        private Guna2ComboBox _categoryFilter, _typeFilter, _statusFilter;
        private Guna2Button _editBtn, _deleteBtn, _viewBtn, _manageCopiesBtn;

        // Vietnamese display mapping for raw DB status values
        private static readonly Dictionary<string, string> StatusDisplayMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "CO_SAN", "Có sẵn" },
            { "DA_MUON", "Đã mượn" },
            { "HONG", "Hỏng" },
            { "MAT", "Mất" },
            { "HET_SACH", "Hết sách" },
            { "DANG_MUON", "Đang mượn" },
            { "SACH_GIAY", "Sách giấy" },
            { "SACH_ONLINE", "Sách online" },
            { "KHONG_CO_SAN", "Không có sẵn" }
        };

        public BookManagementForm()
        {
            InitializeComponent();
            _db = DatabaseManager.Instance;
            _service = new LibraryService(_db);
            SetupUI();
            LoadCategories();
            LoadBooks();
        }

        private void SetupUI()
        {
            Dock = DockStyle.Fill;
            BackColor = AppTheme.ContentBg;
            Padding = new Padding(24, 20, 24, 16);
            Font = AppTheme.BodyMedium;

            // ═══════════════════════════════════════════════════════════
            //  HEADER ROW — Title left, action buttons right
            // ═══════════════════════════════════════════════════════════
            var headerPanel = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = Color.Transparent };
            Controls.Add(headerPanel);

            var titleLabel = new Label
            {
                Text = "📖 Quản lý Sách",
                Font = AppTheme.HeadingLarge,
                ForeColor = AppTheme.TextDark,
                AutoSize = true,
                Location = new Point(0, 12)
            };
            headerPanel.Controls.Add(titleLabel);

            // "Add Book" button — rightmost
            var addBookBtn = MakeHeaderButton("➕ Thêm sách", AppTheme.Primary, AddBook);
            addBookBtn.Width = 136;
            addBookBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            headerPanel.Controls.Add(addBookBtn);

            // "Add Category" button — next to Add Book
            var addCategoryBtn = MakeHeaderButton("📁 Thêm thể loại", Color.FromArgb(108, 117, 125), AddCategory);
            addCategoryBtn.Width = 150;
            addCategoryBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            headerPanel.Controls.Add(addCategoryBtn);

            // Position the buttons on the right, accounting for padding
            headerPanel.Resize += (s, e) =>
            {
                addBookBtn.Location = new Point(headerPanel.Width - addBookBtn.Width - 2, 10);
                addCategoryBtn.Location = new Point(addBookBtn.Left - addCategoryBtn.Width - 10, 10);
            };
            // Fire once to set initial positions
            addBookBtn.Location = new Point(headerPanel.Width - addBookBtn.Width - 2, 10);
            addCategoryBtn.Location = new Point(addBookBtn.Left - addCategoryBtn.Width - 10, 10);

            // ═══════════════════════════════════════════════════════════
            //  FILTER ROW — Search + evenly spaced dropdowns + refresh
            // ═══════════════════════════════════════════════════════════
            var filterPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 52,
                ColumnCount = 8,
                RowCount = 1,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 6, 0, 6)
            };
            // Column proportions: Search(wider) | Label | Combo | Label | Combo | Label | Combo | Refresh
            filterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220)); // search
            filterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 72));  // "Thể loại:"
            filterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));   // category combo
            filterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 52));  // "Loại:"
            filterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));   // type combo
            filterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));  // "Trạng thái:"
            filterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));   // status combo
            filterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 44));  // refresh
            Controls.Add(filterPanel);
            filterPanel.BringToFront(); // Fix docking z-order so header stays on top

            _searchBox = new Guna2TextBox
            {
                Dock = DockStyle.Fill,
                Height = 36,
                PlaceholderText = "🔍 Tìm kiếm theo tên, tác giả...",
                BorderRadius = 6,
                BorderColor = AppTheme.BorderLight,
                FocusedState = { BorderColor = AppTheme.Primary }
            };
            _searchBox.TextChanged += (s, e) => LoadBooks();
            filterPanel.Controls.Add(_searchBox, 0, 0);

            filterPanel.Controls.Add(MakeFilterLabel("Thể loại:"), 1, 0);
            _categoryFilter = MakeCombo();
            _categoryFilter.SelectedIndexChanged += (s, e) => LoadBooks();
            filterPanel.Controls.Add(_categoryFilter, 2, 0);

            filterPanel.Controls.Add(MakeFilterLabel("Loại:"), 3, 0);
            _typeFilter = MakeCombo();
            _typeFilter.Items.AddRange(new object[] { "Tất cả", "Sách giấy", "Sách online" });
            _typeFilter.SelectedIndex = 0;
            _typeFilter.SelectedIndexChanged += (s, e) => LoadBooks();
            filterPanel.Controls.Add(_typeFilter, 4, 0);

            filterPanel.Controls.Add(MakeFilterLabel("Trạng thái:"), 5, 0);
            _statusFilter = MakeCombo();
            _statusFilter.Items.AddRange(new object[] { "Tất cả", "Có sẵn", "Hết sách", "Hỏng", "Mất" });
            _statusFilter.SelectedIndex = 0;
            _statusFilter.SelectedIndexChanged += (s, e) => LoadBooks();
            filterPanel.Controls.Add(_statusFilter, 6, 0);

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
            refreshBtn.Click += (s, e) => { _searchBox.Text = ""; _categoryFilter.SelectedIndex = 0; _typeFilter.SelectedIndex = 0; _statusFilter.SelectedIndex = 0; LoadBooks(); };
            filterPanel.Controls.Add(refreshBtn, 7, 0);

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

            _editBtn = MakeButton("✏️ Sửa", AppTheme.Info, EditBook);
            _deleteBtn = MakeButton("🗑️ Xóa", AppTheme.Danger, DeleteBook);
            _viewBtn = MakeButton("👁️ Chi tiết", Color.FromArgb(108, 117, 125), ViewDetail);
            _manageCopiesBtn = MakeButton("📦 Quản lý quyển", Color.FromArgb(23, 162, 184), ManageCopies);
            _manageCopiesBtn.Width = 160;

            actionPanel.Controls.Add(_editBtn);
            actionPanel.Controls.Add(_deleteBtn);
            actionPanel.Controls.Add(_viewBtn);
            actionPanel.Controls.Add(_manageCopiesBtn);

            // ═══════════════════════════════════════════════════════════
            //  DATA GRID — fills remaining space
            // ═══════════════════════════════════════════════════════════
            _grid = new Guna2DataGridView { Dock = DockStyle.Fill };
            AppTheme.StyleGrid(_grid);
            _grid.CellFormatting += Grid_CellFormatting;
            _grid.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) EditBook(); };
            _grid.SelectionChanged += Grid_SelectionChanged;
            Controls.Add(_grid);
            _grid.BringToFront();
            
            // Set initial disabled state
            Grid_SelectionChanged(null, EventArgs.Empty);
        }

        private void Grid_SelectionChanged(object sender, EventArgs e)
        {
            bool hasSelection = _grid.SelectedRows.Count > 0;
            _editBtn.Enabled = hasSelection;
            _deleteBtn.Enabled = hasSelection;
            _viewBtn.Enabled = hasSelection;

            if (hasSelection)
            {
                var typeCell = _grid.SelectedRows[0].Cells["Loại sách"];
                if (typeCell != null && typeCell.Value != null)
                {
                    _manageCopiesBtn.Enabled = typeCell.Value.ToString() != "Sách online";
                }
                else
                {
                    _manageCopiesBtn.Enabled = true;
                }
            }
            else
            {
                _manageCopiesBtn.Enabled = false;
            }
        }

        // ═══════════════════════════════════════════════════════════════
        //  CELL FORMATTING — replace DB underscores with readable text
        // ═══════════════════════════════════════════════════════════════
        private void Grid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.Value == null) return;
            string raw = e.Value.ToString();

            // Check if the value matches a known DB status key
            if (StatusDisplayMap.TryGetValue(raw, out string display))
            {
                e.Value = display;
                e.FormattingApplied = true;
            }
            // Generic fallback: if value contains underscores and looks like a DB enum
            else if (raw.Contains("_") && raw == raw.ToUpper())
            {
                // Convert "SOME_STATUS" → "Some status"
                string formatted = raw.Replace("_", " ");
                formatted = char.ToUpper(formatted[0]) + formatted.Substring(1).ToLower();
                e.Value = formatted;
                e.FormattingApplied = true;
            }
        }

        private void LoadCategories()
        {
            _categoryFilter.Items.Clear();
            _categoryFilter.Items.Add("Tất cả");
            foreach (DataRow row in _service.Query("SELECT ten_the_loai FROM THE_LOAI_SACH ORDER BY ten_the_loai").Rows)
                _categoryFilter.Items.Add(row["ten_the_loai"].ToString());
            _categoryFilter.SelectedIndex = 0;
        }

        private void LoadBooks()
        {
            string search = _searchBox?.Text?.Trim() ?? "";
            string typeFilter = _typeFilter?.SelectedItem?.ToString() ?? "Tất cả";
            string statusFilter = _statusFilter?.SelectedItem?.ToString() ?? "Tất cả";
            string categoryFilter = _categoryFilter?.SelectedItem?.ToString() ?? "Tất cả";

            string sql = @"
                SELECT s.ma_sach AS 'Mã sách', s.tieu_de AS 'Tiêu đề', s.tac_gia AS 'Tác giả',
                       COALESCE(tl.ten_the_loai, 'N/A') AS 'Thể loại',
                       CASE s.loai_sach WHEN 'SACH_GIAY' THEN 'Sách giấy' ELSE 'Sách online' END AS 'Loại sách',
                       CASE s.loai_sach
                           WHEN 'SACH_GIAY' THEN
                               (SELECT COUNT(*) FROM QUYEN_SACH q WHERE q.ma_sach=s.ma_sach AND q.trang_thai='CO_SAN')
                               || '/' ||
                               (SELECT COUNT(*) FROM QUYEN_SACH q WHERE q.ma_sach=s.ma_sach)
                           ELSE '∞'
                       END AS 'Số lượng',
                       CASE
                           WHEN s.loai_sach = 'SACH_ONLINE' THEN 'Có sẵn'
                           WHEN (SELECT COUNT(*) FROM QUYEN_SACH q WHERE q.ma_sach=s.ma_sach AND q.trang_thai='CO_SAN') > 0 THEN 'Có sẵn'
                           ELSE 'Hết sách'
                       END AS 'Trạng thái'
                FROM SACH s
                LEFT JOIN THE_LOAI_SACH tl ON s.ma_the_loai = tl.ma_the_loai
                WHERE (s.tieu_de LIKE @s OR s.tac_gia LIKE @s)";

            if (typeFilter == "Sách giấy") sql += " AND s.loai_sach = 'SACH_GIAY'";
            else if (typeFilter == "Sách online") sql += " AND s.loai_sach = 'SACH_ONLINE'";

            if (categoryFilter != "Tất cả")
                sql += " AND tl.ten_the_loai = @cat";

            sql += " ORDER BY s.ma_sach DESC";

            DataTable table = _service.Query(sql,
                new SQLiteParameter("@s", "%" + search + "%"),
                new SQLiteParameter("@cat", categoryFilter));

            // Filter by status client-side (computed column)
            if (statusFilter != "Tất cả")
            {
                for (int i = table.Rows.Count - 1; i >= 0; i--)
                {
                    string status = table.Rows[i]["Trạng thái"].ToString();
                    if (status != statusFilter)
                        table.Rows.RemoveAt(i);
                }
            }

            _grid.DataSource = table;
        }

        // ═══════════════════════════════════════════════════════════════
        //  CRUD ACTIONS
        // ═══════════════════════════════════════════════════════════════

        private void AddBook()
        {
            using (var dialog = new BookEditDialog(_service))
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    LoadBooks();
                    LoadCategories();
                }
        }

        private void EditBook()
        {
            int id = GetSelectedId("Mã sách");
            if (id == 0) return;
            using (var dialog = new BookEditDialog(_service, id))
                if (dialog.ShowDialog(this) == DialogResult.OK) LoadBooks();
        }

        private void DeleteBook()
        {
            int id = GetSelectedId("Mã sách");
            if (id == 0) return;
            if (MessageBox.Show("Bạn có chắc chắn muốn xóa sách này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    _service.DeleteBook(id);
                    MessageBox.Show("Xóa sách thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadBooks();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ViewDetail()
        {
            int id = GetSelectedId("Mã sách");
            if (id == 0) return;

            DataTable table = _service.Query(@"
                SELECT s.*, tl.ten_the_loai,
                       COALESCE(sg.so_luong, 0) AS so_luong,
                       so.url_tai_lieu,
                       (SELECT COUNT(*) FROM QUYEN_SACH q WHERE q.ma_sach=s.ma_sach AND q.trang_thai='CO_SAN') AS co_san,
                       (SELECT COUNT(*) FROM QUYEN_SACH q WHERE q.ma_sach=s.ma_sach) AS tong
                FROM SACH s
                LEFT JOIN THE_LOAI_SACH tl ON s.ma_the_loai = tl.ma_the_loai
                LEFT JOIN SACH_GIAY sg ON s.ma_sach = sg.ma_sach
                LEFT JOIN SACH_ONLINE so ON s.ma_sach = so.ma_sach
                WHERE s.ma_sach = @id", new SQLiteParameter("@id", id));

            if (table.Rows.Count == 0) return;
            DataRow row = table.Rows[0];

            string info = $"Mã sách: {row["ma_sach"]}\n" +
                           $"Tiêu đề: {row["tieu_de"]}\n" +
                           $"Tác giả: {row["tac_gia"]}\n" +
                           $"Thể loại: {row["ten_the_loai"]}\n" +
                           $"Loại sách: {(row["loai_sach"].ToString() == "SACH_GIAY" ? "Sách giấy" : "Sách online")}\n";

            if (row["loai_sach"].ToString() == "SACH_GIAY")
                info += $"Có sẵn: {row["co_san"]}/{row["tong"]}";
            else
                info += $"URL: {row["url_tai_lieu"]}";

            MessageBox.Show(info, "Chi tiết sách", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ManageCopies()
        {
            int id = GetSelectedId("Mã sách");
            if (id == 0) return;

            // Check if it's a paper book
            string bookType = _service.Scalar("SELECT loai_sach FROM SACH WHERE ma_sach = @id",
                new SQLiteParameter("@id", id))?.ToString() ?? "";
            if (bookType == "SACH_ONLINE")
            {
                MessageBox.Show("Sách online không cần quản lý quyển.", "Thông báo");
                return;
            }

            using (var dialog = new CopyManagementDialog(_service, id))
                dialog.ShowDialog(this);
            LoadBooks();
        }

        private void AddCategory()
        {
            string name = PromptDialog.Ask("Thêm thể loại", "Nhập tên thể loại mới:");
            if (string.IsNullOrWhiteSpace(name)) return;
            try
            {
                _service.Execute("INSERT INTO THE_LOAI_SACH (ten_the_loai) VALUES (@name)", new SQLiteParameter("@name", name.Trim()));
                MessageBox.Show("Thêm thể loại thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadCategories();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int GetSelectedId(string column)
        {
            if (_grid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn một dòng.");
                return 0;
            }
            return Convert.ToInt32(_grid.SelectedRows[0].Cells[column].Value);
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

        private Label MakeFilterLabel(string text)
        {
            return new Label
            {
                Text = text,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                Font = AppTheme.BodySmall,
                ForeColor = AppTheme.TextDarkSecondary,
                Padding = new Padding(0, 0, 4, 0)
            };
        }

        private Guna2ComboBox MakeCombo()
        {
            return new Guna2ComboBox
            {
                Dock = DockStyle.Fill,
                Height = 36,
                BorderRadius = 6,
                BorderColor = AppTheme.BorderLight,
                FocusedState = { BorderColor = AppTheme.Primary }
            };
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(800, 450);
            this.Name = "BookManagementForm";
            this.Text = "Quản lý Sách";
            this.ResumeLayout(false);
        }
    }
}
