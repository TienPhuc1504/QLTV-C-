using System;
using System.Collections.Generic;
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
    internal class ComboItem
    {
        public string Text { get; }
        public int Value { get; }

        public ComboItem(string text, int value)
        {
            Text = text;
            Value = value;
        }

        public override string ToString() => Text;
    }

    internal class BookEditDialog : Form
    {
        private readonly LibraryService _service;
        private readonly int _bookId;
        private Guna2TextBox _title;
        private Guna2TextBox _author;
        private Guna2TextBox _quantity;
        private Guna2TextBox _url;
        private Guna2ComboBox _category;
        private Guna2ComboBox _type;
        private Guna2ComboBox _status;

        // Panels that toggle visibility based on book type
        private Panel _quantityPanel;
        private Panel _urlPanel;
        private Guna2Button _copyBtn;

        public BookEditDialog(LibraryService service, int bookId = 0)
        {
            _service = service;
            _bookId = bookId;
            Build();
            LoadCategories();
            if (_bookId > 0)
                LoadBook();
            else
                UpdateFieldVisibility(); // Set initial visibility for "Add" mode
        }

        private void Build()
        {
            Text = _bookId > 0 ? "✏️ Sửa sách" : "➕ Thêm sách mới";
            Size = new Size(500, 650);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Padding = new Padding(24, 20, 24, 16);
            Font = new Font("Segoe UI", 10F);
            BackColor = Color.FromArgb(248, 250, 252);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 9,
                BackColor = Color.Transparent
            };
            Controls.Add(layout);

            // Row heights: 7 input rows + 1 flexible + 1 button row
            for (int i = 0; i < 7; i++) layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // spacer
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50)); // buttons

            // Row 0: Title
            _title = AddInput(layout, "Tiêu đề *");

            // Row 1: Author
            _author = AddInput(layout, "Tác giả");

            // Row 2: Category
            _category = AddCombo(layout, "Thể loại");

            // Row 3: Book Type — with SelectedIndexChanged handler
            _type = AddCombo(layout, "Loại sách *");
            _type.Items.AddRange(new object[]
            {
                new ComboItem("Sách giấy", 0),
                new ComboItem("Sách online", 1),
                new ComboItem("Cả hai (giấy + online)", 2)
            });
            _type.SelectedIndex = 0;
            _type.SelectedIndexChanged += (s, e) => UpdateFieldVisibility();

            // Row 4: Status (only for editing)
            _status = AddCombo(layout, "Trạng thái");
            _status.Items.AddRange(new object[]
            {
                new ComboItem("Có sẵn", 0),
                new ComboItem("Hỏng", 1),
                new ComboItem("Mất", 2)
            });
            _status.SelectedIndex = 0;
            // Hide status row when adding a new book (auto-set to CO_SAN)
            if (_bookId == 0) _status.Parent.Visible = false;

            // Row 5: Quantity (conditional — shown for physical)
            _quantityPanel = new Panel { Dock = DockStyle.Fill };
            _quantityPanel.Controls.Add(new Label
            {
                Text = "Số lượng bản giấy",
                Dock = DockStyle.Top,
                Height = 22,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(71, 85, 105)
            });
            _quantity = new Guna2TextBox
            {
                Dock = DockStyle.Bottom,
                Height = 32,
                BorderRadius = 5,
                Text = "1",
                BorderColor = Color.FromArgb(226, 232, 240),
                FocusedState = { BorderColor = Color.FromArgb(6, 182, 212) }
            };
            _quantityPanel.Controls.Add(_quantity);
            layout.Controls.Add(_quantityPanel);

            // Row 6: URL (conditional — shown for online)
            _urlPanel = new Panel { Dock = DockStyle.Fill };
            _urlPanel.Controls.Add(new Label
            {
                Text = "URL tài liệu online",
                Dock = DockStyle.Top,
                Height = 22,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(71, 85, 105)
            });
            _url = new Guna2TextBox
            {
                Dock = DockStyle.Bottom,
                Height = 32,
                BorderRadius = 5,
                PlaceholderText = "https://example.com/document.pdf",
                BorderColor = Color.FromArgb(226, 232, 240),
                FocusedState = { BorderColor = Color.FromArgb(6, 182, 212) }
            };
            _urlPanel.Controls.Add(_url);
            layout.Controls.Add(_urlPanel);

            // Row 7: Spacer (flexible height)
            layout.Controls.Add(new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent });

            // Row 8: Buttons
            var buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                Padding = new Padding(0, 4, 0, 0)
            };
            Controls.Add(buttons);
            buttons.BringToFront();
            buttons.Controls.Add(MakeDialogButton("💾 Lưu", Color.FromArgb(6, 182, 212), Save));
            buttons.Controls.Add(MakeDialogButton("✖ Hủy", Color.FromArgb(239, 68, 68), () => DialogResult = DialogResult.Cancel));
            if (_bookId > 0)
            {
                _copyBtn = MakeDialogButton("📦 Quản lý quyển", Color.FromArgb(23, 162, 184), () => 
                {
                    using (var dlg = new CopyManagementDialog(_service, _bookId))
                    {
                        dlg.ShowDialog(this);
                    }
                });
                _copyBtn.Width = 160;
                buttons.Controls.Add(_copyBtn);
            }
        }

        /// <summary>
        /// Dynamically show/hide Quantity and URL fields based on selected book type.
        /// </summary>
        private void UpdateFieldVisibility()
        {
            if (_type.SelectedItem is ComboItem selected)
            {
                if (_copyBtn != null)
                {
                    _copyBtn.Enabled = selected.Value != 1; // 1 = Sách online, 0 = Giấy, 2 = Cả hai
                }

                switch (selected.Value)
                {
                    case 0: // Physical book
                        _quantityPanel.Visible = true;
                        _urlPanel.Visible = false;
                        break;
                    case 1: // Online book
                        _quantityPanel.Visible = false;
                        _urlPanel.Visible = true;
                        break;
                    case 2: // Both
                        _quantityPanel.Visible = true;
                        _urlPanel.Visible = true;
                        break;
                }
            }
        }

        private Guna2TextBox AddInput(TableLayoutPanel layout, string label)
        {
            var panel = new Panel { Dock = DockStyle.Fill };
            panel.Controls.Add(new Label
            {
                Text = label,
                Dock = DockStyle.Top,
                Height = 22,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(71, 85, 105)
            });
            var input = new Guna2TextBox
            {
                Dock = DockStyle.Bottom,
                Height = 32,
                BorderRadius = 5,
                BorderColor = Color.FromArgb(226, 232, 240),
                FocusedState = { BorderColor = Color.FromArgb(6, 182, 212) }
            };
            panel.Controls.Add(input);
            layout.Controls.Add(panel);
            return input;
        }

        private Guna2ComboBox AddCombo(TableLayoutPanel layout, string label)
        {
            var panel = new Panel { Dock = DockStyle.Fill };
            panel.Controls.Add(new Label
            {
                Text = label,
                Dock = DockStyle.Top,
                Height = 22,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(71, 85, 105)
            });
            var combo = new Guna2ComboBox
            {
                Dock = DockStyle.Bottom,
                Height = 32,
                BorderRadius = 5,
                BorderColor = Color.FromArgb(226, 232, 240),
                FocusedState = { BorderColor = Color.FromArgb(6, 182, 212) }
            };
            panel.Controls.Add(combo);
            layout.Controls.Add(panel);
            return combo;
        }

        private Guna2Button MakeDialogButton(string text, Color color, Action action)
        {
            var button = new Guna2Button
            {
                Text = text,
                Width = 100,
                Height = 38,
                BorderRadius = 6,
                FillColor = color,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 9.5F),
                Margin = new Padding(8, 0, 0, 0),
                Cursor = Cursors.Hand,
                Animated = true
            };
            button.HoverState.FillColor = ControlPaint.Light(color, 0.15f);
            button.Click += (s, e) => action();
            return button;
        }

        private void LoadCategories()
        {
            _category.Items.Add(new ComboItem("— Không chọn —", 0));
            foreach (DataRow row in _service.Query("SELECT ma_the_loai, ten_the_loai FROM THE_LOAI_SACH ORDER BY ten_the_loai").Rows)
                _category.Items.Add(new ComboItem(row["ten_the_loai"].ToString(), Convert.ToInt32(row["ma_the_loai"])));
            _category.SelectedIndex = 0;
        }

        private void LoadBook()
        {
            DataTable table = _service.Query("SELECT * FROM SACH WHERE ma_sach = @id", new SQLiteParameter("@id", _bookId));
            if (table.Rows.Count == 0)
                return;
            DataRow row = table.Rows[0];
            _title.Text = row["tieu_de"].ToString();
            _author.Text = row["tac_gia"].ToString();
            SelectComboValue(_category, row["ma_the_loai"] == DBNull.Value ? 0 : Convert.ToInt32(row["ma_the_loai"]));

            string bookType = row["loai_sach"].ToString();
            if (bookType == "SACH_ONLINE") _type.SelectedIndex = 1;
            else _type.SelectedIndex = 0;

            // Map DB status to combo index
            string statusVal = row["trang_thai_sach"]?.ToString() ?? "CO_SAN";
            if (statusVal == "HONG") _status.SelectedIndex = 1;
            else if (statusVal == "MAT") _status.SelectedIndex = 2;
            else _status.SelectedIndex = 0;

            // Disable quantity for existing books (use "Quản lý quyển" instead)
            _quantity.Enabled = false;
            _quantity.Text = "—";

            // Load existing URL for online books
            DataTable urlTable = _service.Query("SELECT url_tai_lieu FROM SACH_ONLINE WHERE ma_sach = @id", new SQLiteParameter("@id", _bookId));
            if (urlTable.Rows.Count > 0)
                _url.Text = urlTable.Rows[0]["url_tai_lieu"]?.ToString() ?? "";

            UpdateFieldVisibility();
        }

        private void SelectComboValue(Guna2ComboBox combo, int value)
        {
            for (int i = 0; i < combo.Items.Count; i++)
                if (combo.Items[i] is ComboItem item && item.Value == value)
                    combo.SelectedIndex = i;
        }

        private void Save()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_title.Text))
                {
                    MessageBox.Show("Vui lòng nhập tiêu đề.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int categoryId = _category.SelectedItem is ComboItem category ? category.Value : 0;
                int typeIndex = _type.SelectedItem is ComboItem typeItem ? typeItem.Value : 0;

                // Map combo selection to DB type string
                string type;
                switch (typeIndex)
                {
                    case 1: type = "SACH_ONLINE"; break;
                    case 2: type = "SACH_GIAY"; break; // "Both" stores as SACH_GIAY with online record too
                    default: type = "SACH_GIAY"; break;
                }

                int quantity = int.TryParse(_quantity.Text, out int q) ? Math.Max(1, q) : 1;

                // Map status combo to DB value
                string statusStr = "CO_SAN";
                if (_status.SelectedItem is ComboItem statusItem)
                {
                    switch (statusItem.Value)
                    {
                        case 1: statusStr = "HONG"; break;
                        case 2: statusStr = "MAT"; break;
                        default: statusStr = "CO_SAN"; break;
                    }
                }

                if (_bookId > 0)
                {
                    _service.UpdateBook(_bookId, _title.Text.Trim(), _author.Text.Trim(), categoryId, type, statusStr);

                    // If user changed type to include online, ensure SACH_ONLINE record exists
                    if (typeIndex == 1 || typeIndex == 2)
                    {
                        int hasOnline = _service.ScalarInt("SELECT COUNT(*) FROM SACH_ONLINE WHERE ma_sach = @id", new SQLiteParameter("@id", _bookId));
                        if (hasOnline == 0)
                            _service.Execute("INSERT INTO SACH_ONLINE (ma_sach, url_tai_lieu, dinh_dang) VALUES (@id, @url, 'PDF')",
                                new SQLiteParameter("@id", _bookId), new SQLiteParameter("@url", _url.Text.Trim()));
                        else
                            _service.Execute("UPDATE SACH_ONLINE SET url_tai_lieu = @url WHERE ma_sach = @id",
                                new SQLiteParameter("@url", _url.Text.Trim()), new SQLiteParameter("@id", _bookId));
                    }
                }
                else
                {
                    _service.AddBook(_title.Text.Trim(), _author.Text.Trim(), categoryId, type, quantity, _url.Text.Trim());

                    // For "Both" type: also create the online record
                    if (typeIndex == 2)
                    {
                        int newBookId = _service.ScalarInt("SELECT MAX(ma_sach) FROM SACH");
                        int hasOnline = _service.ScalarInt("SELECT COUNT(*) FROM SACH_ONLINE WHERE ma_sach = @id", new SQLiteParameter("@id", newBookId));
                        if (hasOnline == 0)
                            _service.Execute("INSERT INTO SACH_ONLINE (ma_sach, url_tai_lieu, dinh_dang) VALUES (@id, @url, 'PDF')",
                                new SQLiteParameter("@id", newBookId), new SQLiteParameter("@url", _url.Text.Trim()));
                    }
                }

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    internal class ReaderEditDialog : Form
    {
        private readonly LibraryService _service;
        private readonly int _readerId;
        private Guna2TextBox _name, _address, _phone, _email, _code;

        public ReaderEditDialog(LibraryService service, int readerId = 0)
        {
            _service = service;
            _readerId = readerId;
            Build();
            if (_readerId > 0)
                LoadReader();
        }

        private void Build()
        {
            Text = _readerId > 0 ? "Sửa độc giả" : "Thêm độc giả";
            Size = new Size(430, 450);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Padding = new Padding(20);
            Font = new Font("Segoe UI", 10F);
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1 };
            Controls.Add(layout);
            _name = AddInput(layout, "Họ tên");
            _address = AddInput(layout, "Địa chỉ");
            _phone = AddInput(layout, "SĐT");
            _email = AddInput(layout, "Email");
            _code = AddInput(layout, "Mã độc giả (khi thêm)");
            _code.Enabled = _readerId == 0;
            var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 48, FlowDirection = FlowDirection.RightToLeft };
            Controls.Add(buttons);
            buttons.BringToFront();
            buttons.Controls.Add(Button("Lưu", Color.FromArgb(46, 204, 113), Save));
            buttons.Controls.Add(Button("Hủy", Color.FromArgb(231, 76, 60), () => DialogResult = DialogResult.Cancel));
        }

        private Guna2TextBox AddInput(TableLayoutPanel layout, string label)
        {
            var panel = new Panel { Height = 58, Dock = DockStyle.Top };
            panel.Controls.Add(new Label { Text = label, Dock = DockStyle.Top, Height = 20 });
            var input = new Guna2TextBox { Dock = DockStyle.Bottom, Height = 32, BorderRadius = 5 };
            panel.Controls.Add(input);
            layout.Controls.Add(panel);
            return input;
        }

        private Guna2Button Button(string text, Color color, Action action)
        {
            var button = new Guna2Button { Text = text, Width = 90, Height = 36, BorderRadius = 5, FillColor = color, ForeColor = Color.White, Margin = new Padding(8, 4, 0, 0) };
            button.Click += (s, e) => action();
            return button;
        }

        private void LoadReader()
        {
            DataTable table = _service.Query(@"
                SELECT nd.*, dg.ma_doc_gia
                FROM NGUOI_DUNG nd JOIN DOC_GIA dg ON nd.ma_nd = dg.ma_nd
                WHERE nd.ma_nd = @id", new SQLiteParameter("@id", _readerId));
            if (table.Rows.Count == 0) return;
            DataRow row = table.Rows[0];
            _name.Text = row["ho_ten"].ToString();
            _address.Text = row["dia_chi"].ToString();
            _phone.Text = row["so_dt"].ToString();
            _email.Text = row["email"].ToString();
            _code.Text = row["ma_doc_gia"].ToString();
        }

        private void Save()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_name.Text))
                {
                    MessageBox.Show("Vui lòng nhập họ tên.");
                    return;
                }
                if (!string.IsNullOrWhiteSpace(_phone.Text) && !System.Text.RegularExpressions.Regex.IsMatch(_phone.Text.Trim(), @"^\d{10,11}$"))
                {
                    MessageBox.Show("Số điện thoại phải có 10-11 chữ số.");
                    return;
                }
                if (!string.IsNullOrWhiteSpace(_email.Text) && !System.Text.RegularExpressions.Regex.IsMatch(_email.Text.Trim(), @"^[\w\.-]+@[\w\.-]+\.\w+$"))
                {
                    MessageBox.Show("Email không hợp lệ.");
                    return;
                }

                if (_readerId > 0)
                {
                    _service.Execute(@"
                        UPDATE NGUOI_DUNG SET ho_ten = @name, dia_chi = @address, so_dt = @phone, email = @email
                        WHERE ma_nd = @id",
                        new SQLiteParameter("@name", _name.Text.Trim()),
                        new SQLiteParameter("@address", _address.Text.Trim()),
                        new SQLiteParameter("@phone", _phone.Text.Trim()),
                        new SQLiteParameter("@email", _email.Text.Trim()),
                        new SQLiteParameter("@id", _readerId));
                }
                else
                {
                    string code = _code.Text.Trim();
                    int userId = Convert.ToInt32(_service.Scalar(@"
                        INSERT INTO NGUOI_DUNG (ho_ten, dia_chi, so_dt, email, loai_nguoi_dung)
                        VALUES (@name, @address, @phone, @email, 'DOC_GIA');
                        SELECT last_insert_rowid();",
                        new SQLiteParameter("@name", _name.Text.Trim()),
                        new SQLiteParameter("@address", _address.Text.Trim()),
                        new SQLiteParameter("@phone", _phone.Text.Trim()),
                        new SQLiteParameter("@email", _email.Text.Trim())));

                    if (string.IsNullOrEmpty(code)) code = $"DG{userId:D4}";
                    _service.Execute(@"
                        INSERT INTO DOC_GIA (ma_nd, ma_doc_gia, ngay_dk) VALUES (@id, @code, date('now'));
                        INSERT INTO TAI_KHOAN (ten_tk, mat_khau, ma_nd) VALUES (@account, @password, @id)",
                        new SQLiteParameter("@id", userId),
                        new SQLiteParameter("@code", code),
                        new SQLiteParameter("@account", code.ToLower()),
                        new SQLiteParameter("@password", DatabaseManager.HashPassword(code.ToLower())));
                }

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    internal class BorrowEditDialog : Form
    {
        private readonly LibraryService _service;
        private readonly User _user;
        private Guna2ComboBox _reader, _book;
        private Guna2TextBox _days;

        public BorrowEditDialog(LibraryService service, User user)
        {
            _service = service;
            _user = user;
            Build();
            LoadData();
        }

        private void Build()
        {
            Text = "Tạo phiếu mượn";
            Size = new Size(480, 320);
            StartPosition = FormStartPosition.CenterParent;
            Padding = new Padding(20);
            Font = new Font("Segoe UI", 10F);
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1 };
            Controls.Add(layout);
            _reader = AddCombo(layout, "Độc giả");
            _book = AddCombo(layout, "Sách có sẵn");
            _days = AddInput(layout, "Số ngày mượn");
            _days.Text = "14";
            var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 48, FlowDirection = FlowDirection.RightToLeft };
            Controls.Add(buttons);
            buttons.BringToFront();
            buttons.Controls.Add(Button("Tạo", Color.FromArgb(46, 204, 113), Save));
            buttons.Controls.Add(Button("Hủy", Color.FromArgb(231, 76, 60), () => DialogResult = DialogResult.Cancel));
        }

        private Guna2ComboBox AddCombo(TableLayoutPanel layout, string label)
        {
            var panel = new Panel { Height = 62, Dock = DockStyle.Top };
            panel.Controls.Add(new Label { Text = label, Dock = DockStyle.Top, Height = 22 });
            var combo = new Guna2ComboBox { Dock = DockStyle.Bottom, Height = 34, BorderRadius = 5 };
            panel.Controls.Add(combo);
            layout.Controls.Add(panel);
            return combo;
        }

        private Guna2TextBox AddInput(TableLayoutPanel layout, string label)
        {
            var panel = new Panel { Height = 62, Dock = DockStyle.Top };
            panel.Controls.Add(new Label { Text = label, Dock = DockStyle.Top, Height = 22 });
            var input = new Guna2TextBox { Dock = DockStyle.Bottom, Height = 34, BorderRadius = 5 };
            panel.Controls.Add(input);
            layout.Controls.Add(panel);
            return input;
        }

        private Guna2Button Button(string text, Color color, Action action)
        {
            var button = new Guna2Button { Text = text, Width = 90, Height = 36, BorderRadius = 5, FillColor = color, ForeColor = Color.White, Margin = new Padding(8, 4, 0, 0) };
            button.Click += (s, e) => action();
            return button;
        }

        private void LoadData()
        {
            foreach (DataRow row in _service.Query("SELECT dg.ma_nd, nd.ho_ten, dg.ma_doc_gia FROM DOC_GIA dg JOIN NGUOI_DUNG nd ON dg.ma_nd = nd.ma_nd ORDER BY nd.ho_ten").Rows)
                _reader.Items.Add(new ComboItem($"{row["ho_ten"]} ({row["ma_doc_gia"]})", Convert.ToInt32(row["ma_nd"])));
            foreach (DataRow row in _service.Query(@"
                SELECT s.ma_sach, s.tieu_de, COUNT(q.ma_quyen) AS con_lai
                FROM SACH s JOIN QUYEN_SACH q ON s.ma_sach = q.ma_sach
                WHERE q.trang_thai = 'CO_SAN'
                GROUP BY s.ma_sach, s.tieu_de
                ORDER BY s.tieu_de").Rows)
                _book.Items.Add(new ComboItem($"{row["tieu_de"]} ({row["con_lai"]} bản)", Convert.ToInt32(row["ma_sach"])));
            if (_reader.Items.Count > 0) _reader.SelectedIndex = 0;
            if (_book.Items.Count > 0) _book.SelectedIndex = 0;
        }

        private void Save()
        {
            try
            {
                if (!(_reader.SelectedItem is ComboItem reader) || !(_book.SelectedItem is ComboItem book))
                {
                    MessageBox.Show("Vui lòng chọn độc giả và sách.");
                    return;
                }
                int staffId = _user != null && _user.LoaiNguoiDung == "NHAN_VIEN" || _user != null && _user.LoaiNguoiDung == "ADMIN"
                    ? _user.MaNguoiDung
                    : _service.ScalarInt("SELECT ma_nd FROM NHAN_VIEN ORDER BY ma_nd LIMIT 1");
                int days = int.TryParse(_days.Text, out int d) ? Math.Max(1, d) : 14;
                _service.CreateBorrowRequest(reader.Value, book.Value, days, "");
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    public class StaffManagementForm : SimpleGridForm
    {
        public StaffManagementForm() : base("👔 Quản lý Nhân viên")
        {
        }

        protected override void LoadData()
        {
            Grid.DataSource = Service.Query(@"
                SELECT nv.ma_nd AS 'Mã', nv.ma_nhan_vien AS 'Mã NV', nd.ho_ten AS 'Họ tên',
                       nd.so_dt AS 'SĐT', nd.email AS 'Email', nd.dia_chi AS 'Địa chỉ'
                FROM NHAN_VIEN nv JOIN NGUOI_DUNG nd ON nv.ma_nd = nd.ma_nd
                WHERE nd.ho_ten LIKE @s OR nv.ma_nhan_vien LIKE @s OR nd.so_dt LIKE @s
                ORDER BY nv.ma_nd DESC", new SQLiteParameter("@s", "%" + SearchText + "%"));
        }

        protected override void Add()
        {
            using (var dialog = new StaffEditDialog(Service))
                if (dialog.ShowDialog(this) == DialogResult.OK) LoadData();
        }

        protected override void Edit()
        {
            int id = SelectedId("Mã");
            if (id == 0) return;
            using (var dialog = new StaffEditDialog(Service, id))
                if (dialog.ShowDialog(this) == DialogResult.OK) LoadData();
        }

        protected override void Delete()
        {
            int id = SelectedId("Mã");
            if (id == 0) return;
            if (MessageBox.Show("Xóa nhân viên này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Service.Execute("DELETE FROM NGUOI_DUNG WHERE ma_nd = @id", new SQLiteParameter("@id", id));
                MessageBox.Show("Đã xóa nhân viên.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadData();
            }
        }
    }

    internal class StaffEditDialog : Form
    {
        private readonly LibraryService _service;
        private readonly int _staffId;
        private Guna2TextBox _name, _address, _phone, _email, _code;

        public StaffEditDialog(LibraryService service, int id = 0)
        {
            _service = service;
            _staffId = id;
            Text = id > 0 ? "Sửa nhân viên" : "Thêm nhân viên";
            Build();
            if (_staffId > 0)
                LoadStaff();
        }

        private void Build()
        {
            Size = new Size(430, 430);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Padding = new Padding(20);
            Font = new Font("Segoe UI", 10F);
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1 };
            Controls.Add(layout);
            _name = AddInput(layout, "Họ tên");
            _address = AddInput(layout, "Địa chỉ");
            _phone = AddInput(layout, "SĐT");
            _email = AddInput(layout, "Email");
            _code = AddInput(layout, "Mã nhân viên");
            _code.Enabled = _staffId == 0;
            var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 48, FlowDirection = FlowDirection.RightToLeft };
            Controls.Add(buttons);
            buttons.BringToFront();
            buttons.Controls.Add(Button("Lưu", Color.FromArgb(46, 204, 113), Save));
            buttons.Controls.Add(Button("Hủy", Color.FromArgb(231, 76, 60), () => DialogResult = DialogResult.Cancel));
        }

        private Guna2TextBox AddInput(TableLayoutPanel layout, string label)
        {
            var panel = new Panel { Height = 58, Dock = DockStyle.Top };
            panel.Controls.Add(new Label { Text = label, Dock = DockStyle.Top, Height = 20 });
            var input = new Guna2TextBox { Dock = DockStyle.Bottom, Height = 32, BorderRadius = 5 };
            panel.Controls.Add(input);
            layout.Controls.Add(panel);
            return input;
        }

        private Guna2Button Button(string text, Color color, Action action)
        {
            var button = new Guna2Button { Text = text, Width = 90, Height = 36, BorderRadius = 5, FillColor = color, ForeColor = Color.White, Margin = new Padding(8, 4, 0, 0) };
            button.Click += (s, e) => action();
            return button;
        }

        private void LoadStaff()
        {
            DataTable table = _service.Query(@"
                SELECT nd.*, nv.ma_nhan_vien
                FROM NGUOI_DUNG nd JOIN NHAN_VIEN nv ON nd.ma_nd = nv.ma_nd
                WHERE nd.ma_nd = @id", new SQLiteParameter("@id", _staffId));
            if (table.Rows.Count == 0) return;
            DataRow row = table.Rows[0];
            _name.Text = row["ho_ten"].ToString();
            _address.Text = row["dia_chi"].ToString();
            _phone.Text = row["so_dt"].ToString();
            _email.Text = row["email"].ToString();
            _code.Text = row["ma_nhan_vien"].ToString();
        }

        private void Save()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_name.Text))
                {
                    MessageBox.Show("Vui lòng nhập họ tên.");
                    return;
                }
                if (!string.IsNullOrWhiteSpace(_phone.Text) && !System.Text.RegularExpressions.Regex.IsMatch(_phone.Text.Trim(), @"^\d{10,11}$"))
                {
                    MessageBox.Show("Số điện thoại phải có 10-11 chữ số.");
                    return;
                }
                if (!string.IsNullOrWhiteSpace(_email.Text) && !System.Text.RegularExpressions.Regex.IsMatch(_email.Text.Trim(), @"^[\w\.-]+@[\w\.-]+\.\w+$"))
                {
                    MessageBox.Show("Email không hợp lệ.");
                    return;
                }

                if (_staffId > 0)
                {
                    _service.Execute(@"
                        UPDATE NGUOI_DUNG SET ho_ten = @name, dia_chi = @address, so_dt = @phone, email = @email
                        WHERE ma_nd = @id",
                        new SQLiteParameter("@name", _name.Text.Trim()),
                        new SQLiteParameter("@address", _address.Text.Trim()),
                        new SQLiteParameter("@phone", _phone.Text.Trim()),
                        new SQLiteParameter("@email", _email.Text.Trim()),
                        new SQLiteParameter("@id", _staffId));
                }
                else
                {
                    string code = _code.Text.Trim();
                    int userId = Convert.ToInt32(_service.Scalar(@"
                        INSERT INTO NGUOI_DUNG (ho_ten, dia_chi, so_dt, email, loai_nguoi_dung)
                        VALUES (@name, @address, @phone, @email, 'NHAN_VIEN');
                        SELECT last_insert_rowid();",
                        new SQLiteParameter("@name", _name.Text.Trim()),
                        new SQLiteParameter("@address", _address.Text.Trim()),
                        new SQLiteParameter("@phone", _phone.Text.Trim()),
                        new SQLiteParameter("@email", _email.Text.Trim())));

                    if (string.IsNullOrEmpty(code)) code = $"NV{userId:D4}";
                    _service.Execute(@"
                        INSERT INTO NHAN_VIEN (ma_nd, ma_nhan_vien) VALUES (@id, @code);
                        INSERT INTO TAI_KHOAN (ten_tk, mat_khau, ma_nd) VALUES (@account, @password, @id)",
                        new SQLiteParameter("@id", userId),
                        new SQLiteParameter("@code", code),
                        new SQLiteParameter("@account", code.ToLower()),
                        new SQLiteParameter("@password", DatabaseManager.HashPassword(code.ToLower())));
                }

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    public class BorrowRequestsForm : SimpleGridForm
    {
        private readonly User _user;
        private Guna2ComboBox _statusFilter;
        private Guna2Button _btnApprove;
        private Guna2Button _btnPickup;
        private Guna2Button _btnReject;
        private Guna2Button _btnDetails;

        public BorrowRequestsForm(User user) : base("📋 Yêu cầu mượn sách")
        {
            _user = user;

            ClearBottomButtons();

            _btnApprove = AddExtraButton("✅ Duyệt", AppTheme.Success, () => Approve());
            _btnPickup = AddExtraButton("📦 Xác nhận lấy", AppTheme.Info, () => Pickup());
            _btnReject = AddExtraButton("❌ Từ chối", AppTheme.Danger, () => Reject());
            _btnDetails = AddExtraButton("ℹ️ Chi tiết", Color.FromArgb(108, 117, 125), () => ShowDetails());

            // Filter row: status filter
            _statusFilter = AddFilterCombo("Trạng thái:", 80,
                new[] { "Tất cả", "Chờ duyệt", "Chờ lấy sách", "Đã lấy", "Từ chối", "Đã hủy", "Đang làm việc", "Đã in" });
            _statusFilter.SelectedIndex = 0;

            Grid.SelectionChanged += Grid_SelectionChanged;
        }

        protected override void LoadData()
        {
            // Auto update cards 7 days after requested (Optimized by avoiding function calls on the column)
            Service.Execute("UPDATE YEU_CAU_THE SET trang_thai = 'DA_IN' WHERE trang_thai = 'DANG_XU_LY' AND ngay_xu_ly <= datetime('now', '-7 days')");

            string statusVal = _statusFilter?.SelectedItem?.ToString() ?? "Tất cả";
            string statusSqlYc = "";
            string statusSqlYt = "";
            switch (statusVal)
            {
                case "Chờ duyệt": statusSqlYc = " AND yc.trang_thai = 'CHO_DUYET'"; statusSqlYt = " AND 1=0"; break;
                case "Chờ lấy sách": statusSqlYc = " AND yc.trang_thai = 'CHO_LAY_SACH'"; statusSqlYt = " AND 1=0"; break;
                case "Đã lấy": statusSqlYc = " AND yc.trang_thai = 'DA_LAY'"; statusSqlYt = " AND 1=0"; break;
                case "Từ chối": statusSqlYc = " AND yc.trang_thai = 'TU_CHOI'"; statusSqlYt = " AND 1=0"; break;
                case "Đã hủy": statusSqlYc = " AND yc.trang_thai = 'DA_HUY'"; statusSqlYt = " AND 1=0"; break;
                case "Đang làm việc": statusSqlYc = " AND 1=0"; statusSqlYt = " AND yt.trang_thai = 'DANG_XU_LY'"; break;
                case "Đã in": statusSqlYc = " AND 1=0"; statusSqlYt = " AND yt.trang_thai = 'DA_IN'"; break;
            }

            Grid.DataSource = Service.Query($@"
                SELECT yc.ma_yeu_cau AS 'Mã', nd.ho_ten AS 'Độc giả', s.tieu_de AS 'Sách',
                       yc.so_ngay_muon_de_xuat AS 'Ngày đề xuất',
                       CASE yc.trang_thai
                           WHEN 'CHO_DUYET' THEN '⏳ Chờ duyệt'
                           WHEN 'CHO_LAY_SACH' THEN '📦 Chờ lấy sách'
                           WHEN 'DA_LAY' THEN '✅ Đã lấy'
                           WHEN 'TU_CHOI' THEN '❌ Từ chối'
                           WHEN 'DA_HUY' THEN '🚫 Đã hủy'
                           ELSE yc.trang_thai
                       END AS 'Trạng thái',
                       strftime('%d/%m/%Y', yc.ngay_yeu_cau) AS 'Ngày yêu cầu', yc.ghi_chu AS 'Ghi chú',
                       'MUON' AS 'Loại',
                       yc.ngay_yeu_cau AS 'RawDate'
                FROM YEU_CAU_MUON yc
                JOIN NGUOI_DUNG nd ON yc.ma_nd_doc_gia = nd.ma_nd
                JOIN SACH s ON yc.ma_sach = s.ma_sach
                WHERE (nd.ho_ten LIKE @s OR s.tieu_de LIKE @s){statusSqlYc}
                
                UNION ALL
                
                SELECT yt.ma_yeu_cau AS 'Mã', nd.ho_ten AS 'Độc giả', 'In Thẻ Độc Giả' AS 'Sách',
                       0 AS 'Ngày đề xuất',
                       CASE yt.trang_thai
                           WHEN 'DANG_XU_LY' THEN '⚙️ Đang làm việc'
                           WHEN 'DA_IN' THEN '🖨️ Đã in'
                           ELSE yt.trang_thai
                       END AS 'Trạng thái',
                       strftime('%d/%m/%Y', yt.ngay_yeu_cau) AS 'Ngày yêu cầu', '' AS 'Ghi chú',
                       'THE' AS 'Loại',
                       yt.ngay_yeu_cau AS 'RawDate'
                FROM YEU_CAU_THE yt
                JOIN NGUOI_DUNG nd ON yt.ma_nd_doc_gia = nd.ma_nd
                WHERE (nd.ho_ten LIKE @s){statusSqlYt}
                
                ORDER BY RawDate DESC, ""Mã"" DESC", new SQLiteParameter("@s", "%" + SearchText + "%"));
                
            if (Grid.Columns.Contains("Loại")) Grid.Columns["Loại"].Visible = false;
            if (Grid.Columns.Contains("RawDate")) Grid.Columns["RawDate"].Visible = false;

            UpdateButtonStates();
        }

        private void Grid_SelectionChanged(object sender, EventArgs e)
        {
            UpdateButtonStates();
        }

        private void UpdateButtonStates()
        {
            if (Grid.SelectedRows.Count == 0)
            {
                if (_btnApprove != null) _btnApprove.Enabled = false;
                if (_btnPickup != null) _btnPickup.Enabled = false;
                if (_btnReject != null) _btnReject.Enabled = false;
                if (_btnDetails != null) _btnDetails.Enabled = false;
                return;
            }

            if (_btnDetails != null) _btnDetails.Enabled = true;

            var row = Grid.SelectedRows[0];
            string status = row.Cells["Trạng thái"].Value?.ToString() ?? "";
            string loai = row.Cells["Loại"].Value?.ToString() ?? "";

            if (_btnApprove != null) _btnApprove.Enabled = status.Contains("Chờ duyệt") && loai == "MUON";
            if (_btnPickup != null) _btnPickup.Enabled = status.Contains("Chờ lấy sách") && loai == "MUON";
            if (_btnReject != null) _btnReject.Enabled = status.Contains("Chờ duyệt") && loai == "MUON";
        }

        protected override void Add() { }
        protected override void Edit() => ShowDetails();
        protected override void Delete() { }

        private bool Approve()
        {
            int id = SelectedId("Mã");
            if (id == 0) return false;
            string daysStr = PromptDialog.Ask("Duyệt yêu cầu", "Nhập số ngày mượn chính thức:");
            if (string.IsNullOrEmpty(daysStr)) return false;
            int days = int.TryParse(daysStr, out int d) && d > 0 ? d : 14;
            Service.ApproveBorrowRequest(id, _user.MaNguoiDung, days);
            MessageBox.Show("Đã duyệt yêu cầu!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadData();
            return true;
        }

        private bool Pickup()
        {
            int id = SelectedId("Mã");
            if (id == 0) return false;
            Service.ConfirmBorrowPickup(id, _user.MaNguoiDung);
            MessageBox.Show("Đã xác nhận lấy sách!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadData();
            return true;
        }

        private bool Reject()
        {
            int id = SelectedId("Mã");
            if (id == 0) return false;
            string reason = PromptDialog.Ask("Lý do từ chối", "Nhập lý do:");
            if (string.IsNullOrWhiteSpace(reason)) return false;
            Service.RejectBorrowRequest(id, _user.MaNguoiDung, reason);
            MessageBox.Show("Đã từ chối yêu cầu.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadData();
            return true;
        }

        private void ShowDetails()
        {
            if (Grid.SelectedRows.Count == 0) return;
            var row = Grid.SelectedRows[0];
            
            using (var dialog = new BorrowRequestDetailsDialog(row, Approve, Pickup, Reject))
            {
                dialog.ShowDialog(this);
            }
        }
    }

    public class BorrowRequestDetailsDialog : Form
    {
        private readonly DataGridViewRow _row;
        private readonly Func<bool> _onApprove;
        private readonly Func<bool> _onPickup;
        private readonly Func<bool> _onReject;

        public BorrowRequestDetailsDialog(DataGridViewRow row, Func<bool> onApprove, Func<bool> onPickup, Func<bool> onReject)
        {
            _row = row;
            _onApprove = onApprove;
            _onPickup = onPickup;
            _onReject = onReject;
            BuildUI();
        }

        private void BuildUI()
        {
            Text = "Chi tiết yêu cầu";
            Size = new Size(500, 500);
            StartPosition = FormStartPosition.CenterParent;
            Padding = new Padding(24);
            Font = new Font("Segoe UI", 10F);
            BackColor = AppTheme.ContentBg;
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = false;
            MinimumSize = new Size(400, 400);

            var title = new Label
            {
                Text = "Chi tiết yêu cầu",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 40,
                ForeColor = AppTheme.Primary
            };
            Controls.Add(title);

            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 2,
                RowCount = 8,
                Padding = new Padding(0, 10, 0, 10)
            };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));

            void AddRow(string labelText, string valueText, int rowIndex)
            {
                var lbl = new Label { Text = labelText, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, ForeColor = AppTheme.TextDarkSecondary };
                var val = new Label { Text = valueText, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = AppTheme.TextDark };
                panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
                panel.Controls.Add(lbl, 0, rowIndex);
                panel.Controls.Add(val, 1, rowIndex);
            }

            AddRow("Mã yêu cầu:", _row.Cells["Mã"].Value?.ToString(), 0);
            AddRow("Độc giả:", _row.Cells["Độc giả"].Value?.ToString(), 1);
            AddRow("Sách:", _row.Cells["Sách"].Value?.ToString(), 2);
            string days = _row.Cells["Ngày đề xuất"].Value?.ToString();
            AddRow("Ngày đề xuất:", days != "0" ? days + " ngày" : "-", 3);
            AddRow("Trạng thái:", _row.Cells["Trạng thái"].Value?.ToString(), 4);
            AddRow("Ngày yêu cầu:", _row.Cells["Ngày yêu cầu"].Value?.ToString(), 5);
            AddRow("Loại:", _row.Cells["Loại"].Value?.ToString(), 6);
            AddRow("Ghi chú:", _row.Cells["Ghi chú"].Value?.ToString(), 7);

            Controls.Add(panel);
            panel.BringToFront();

            var buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 48,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(0, 8, 0, 0)
            };
            Controls.Add(buttons);
            buttons.BringToFront();

            var btnClose = MkBtn("Đóng", Color.FromArgb(108, 117, 125), () => Close());
            buttons.Controls.Add(btnClose);

            string status = _row.Cells["Trạng thái"].Value?.ToString() ?? "";
            string loai = _row.Cells["Loại"].Value?.ToString() ?? "";

            if (loai == "MUON")
            {
                if (status.Contains("Chờ duyệt"))
                {
                    var btnReject = MkBtn("❌ Từ chối", AppTheme.Danger, () => { if (_onReject()) Close(); });
                    var btnApprove = MkBtn("✅ Duyệt", AppTheme.Success, () => { if (_onApprove()) Close(); });
                    buttons.Controls.Add(btnReject);
                    buttons.Controls.Add(btnApprove);
                }
                else if (status.Contains("Chờ lấy sách"))
                {
                    var btnPickup = MkBtn("📦 Xác nhận lấy", AppTheme.Info, () => { if (_onPickup()) Close(); });
                    buttons.Controls.Add(btnPickup);
                }
            }
        }

        private Guna2Button MkBtn(string text, Color color, Action action)
        {
            var button = new Guna2Button
            {
                Text = text,
                Width = 130,
                Height = 36,
                BorderRadius = 5,
                FillColor = color,
                ForeColor = Color.White,
                Margin = new Padding(8, 0, 0, 0),
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            button.Click += (s, e) => action();
            return button;
        }


    }

    public class SearchBooksForm : SimpleGridForm
    {
        private readonly User _user;
        public SearchBooksForm(User user) : base("Tìm kiếm Sách")
        {
            _user = user;
            AddExtraButton("Gửi yêu cầu mượn", Color.FromArgb(46, 204, 113), RequestBorrow);
        }

        protected override void LoadData()
        {
            Grid.DataSource = Service.Query(@"
                SELECT s.ma_sach AS 'Mã', s.tieu_de AS 'Tiêu đề', s.tac_gia AS 'Tác giả',
                       tl.ten_the_loai AS 'Thể loại', s.loai_sach AS 'Loại',
                       (SELECT COUNT(*) FROM QUYEN_SACH q WHERE q.ma_sach=s.ma_sach AND q.trang_thai='CO_SAN') AS 'Có sẵn'
                FROM SACH s LEFT JOIN THE_LOAI_SACH tl ON s.ma_the_loai = tl.ma_the_loai
                WHERE s.tieu_de LIKE @s OR s.tac_gia LIKE @s OR tl.ten_the_loai LIKE @s
                ORDER BY s.tieu_de", new SQLiteParameter("@s", "%" + SearchText + "%"));
        }

        protected override void Add() { }
        protected override void Edit() { }
        protected override void Delete() { }

        private void RequestBorrow()
        {
            int id = SelectedId("Mã");
            if (id == 0) return;
            Service.CreateBorrowRequest(_user.MaNguoiDung, id, 14, "");
            MessageBox.Show("Đã gửi yêu cầu mượn sách.");
        }
    }

    public class MyBorrowsForm : SimpleGridForm
    {
        private readonly User _user;
        public MyBorrowsForm(User user) : base("Sách của tôi")
        {
            _user = user;
        }

        protected override void LoadData()
        {
            Grid.DataSource = Service.Query(@"
                SELECT p.ma_phieu AS 'Mã phiếu', s.tieu_de AS 'Sách', strftime('%d/%m/%Y', p.ngay_muon) AS 'Ngày mượn',
                       strftime('%d/%m/%Y', p.ngay_hen_tra) AS 'Hạn trả', COALESCE(strftime('%d/%m/%Y', p.ngay_tra_thuc), '-') AS 'Ngày trả', p.trang_thai_phieu AS 'Trạng thái'
                FROM PHIEU_MUON_TRA p JOIN SACH s ON p.ma_sach = s.ma_sach
                WHERE p.ma_nd_doc_gia = @id AND s.tieu_de LIKE @s
                ORDER BY p.ma_phieu DESC", new SQLiteParameter("@id", _user.MaNguoiDung), new SQLiteParameter("@s", "%" + SearchText + "%"));
        }
        protected override void Add() { }
        protected override void Edit() { }
        protected override void Delete() { }
    }

    public class MyRequestsForm : SimpleGridForm
    {
        private readonly User _user;
        public MyRequestsForm(User user) : base("Yêu cầu của tôi")
        {
            _user = user;
            AddExtraButton("Hủy yêu cầu", Color.FromArgb(231, 76, 60), Cancel);
        }

        protected override void LoadData()
        {
            Grid.DataSource = Service.Query(@"
                SELECT yc.ma_yeu_cau AS 'Mã', s.tieu_de AS 'Sách', strftime('%d/%m/%Y', yc.ngay_yeu_cau) AS 'Ngày yêu cầu',
                       yc.so_ngay_muon_de_xuat AS 'Số ngày', yc.trang_thai AS 'Trạng thái', yc.ghi_chu AS 'Ghi chú',
                       yc.ngay_yeu_cau AS 'RawDate'
                FROM YEU_CAU_MUON yc JOIN SACH s ON yc.ma_sach = s.ma_sach
                WHERE yc.ma_nd_doc_gia = @id AND s.tieu_de LIKE @s
                ORDER BY RawDate DESC, yc.ma_yeu_cau DESC", new SQLiteParameter("@id", _user.MaNguoiDung), new SQLiteParameter("@s", "%" + SearchText + "%"));
            
            if (Grid.Columns.Contains("RawDate")) Grid.Columns["RawDate"].Visible = false;
        }
        protected override void Add() { }
        protected override void Edit() { }
        protected override void Delete() { }
        private void Cancel()
        {
            int id = SelectedId("Mã");
            if (id == 0) return;
            Service.CancelBorrowRequest(id, _user.MaNguoiDung);
            LoadData();
        }
    }

    public class StatisticsForm : Form
    {
        private readonly LibraryService _service;
        private Guna2DataGridView _detailGrid;

        public StatisticsForm()
        {
            _service = new LibraryService(DatabaseManager.Instance);
            Dock = DockStyle.Fill;
            BackColor = Color.FromArgb(245, 245, 245);
            Padding = new Padding(20);
            Font = new Font("Segoe UI", 10F);
            BuildUI();
        }

        private void BuildUI()
        {
            // Header
            var header = new Panel { Dock = DockStyle.Top, Height = 50 };
            Controls.Add(header);
            header.Controls.Add(new Label
            {
                Text = "📊 Thống kê Thư viện", Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                AutoSize = true, Location = new Point(0, 8)
            });

            // Stat cards row
            int totalBooks = _service.ScalarInt("SELECT COUNT(*) FROM SACH");
            int borrowed = _service.ScalarInt("SELECT COUNT(*) FROM PHIEU_MUON_TRA WHERE trang_thai_phieu='DANG_MUON'");
            int totalReaders = _service.ScalarInt("SELECT COUNT(*) FROM DOC_GIA");
            int overdue = _service.ScalarInt("SELECT COUNT(*) FROM PHIEU_MUON_TRA WHERE trang_thai_phieu='DANG_MUON' AND date(ngay_hen_tra)<date('now')");
            decimal fines = _service.ScalarDecimal("SELECT COALESCE(SUM(tien_phat),0) FROM PHIEU_MUON_TRA");

            var cardsPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Top, Height = 130, ColumnCount = 5, RowCount = 1,
                Padding = new Padding(0, 8, 0, 16)
            };
            for (int i = 0; i < 5; i++) cardsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
            Controls.Add(cardsPanel);
            cardsPanel.BringToFront();

            cardsPanel.Controls.Add(AppTheme.CreateStatCard(totalBooks.ToString(), "Tổng sách", AppTheme.Info), 0, 0);
            cardsPanel.Controls.Add(AppTheme.CreateStatCard(borrowed.ToString(), "Đang mượn", AppTheme.Success), 1, 0);
            cardsPanel.Controls.Add(AppTheme.CreateStatCard(totalReaders.ToString(), "Tổng độc giả", AppTheme.Secondary), 2, 0);
            cardsPanel.Controls.Add(AppTheme.CreateStatCard(overdue.ToString(), "Quá hạn", AppTheme.Danger), 3, 0);
            cardsPanel.Controls.Add(AppTheme.CreateStatCard(fines.ToString("N0") + "đ", "Tiền phạt", AppTheme.Warning), 4, 0);

            // View switching + export buttons
            var exportPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom, Height = 50, WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(0, 7, 0, 7)
            };
            Controls.Add(exportPanel);

            var viewInventory = new Guna2Button { Text = "📦 Kho sách", Width = 130, Height = 36, BorderRadius = 5, FillColor = Color.FromArgb(52, 152, 219), ForeColor = Color.White, Font = new Font("Segoe UI", 9F), Margin = new Padding(0, 0, 8, 0) };
            viewInventory.Click += (s, e) => LoadInventory();
            exportPanel.Controls.Add(viewInventory);

            var viewHistory = new Guna2Button { Text = "📋 Lịch sử", Width = 130, Height = 36, BorderRadius = 5, FillColor = Color.FromArgb(108, 117, 125), ForeColor = Color.White, Font = new Font("Segoe UI", 9F), Margin = new Padding(0, 0, 8, 0) };
            viewHistory.Click += (s, e) => LoadHistory();
            exportPanel.Controls.Add(viewHistory);

            var viewTop = new Guna2Button { Text = "🏆 Top 10", Width = 130, Height = 36, BorderRadius = 5, FillColor = Color.FromArgb(245, 158, 11), ForeColor = Color.White, Font = new Font("Segoe UI", 9F), Margin = new Padding(0, 0, 8, 0) };
            viewTop.Click += (s, e) => LoadTopBorrowed();
            exportPanel.Controls.Add(viewTop);

            var exportBtn = new Guna2Button { Text = "📥 Xuất CSV", Width = 130, Height = 36, BorderRadius = 5, FillColor = Color.FromArgb(16, 185, 129), ForeColor = Color.White, Font = new Font("Segoe UI", 9F), Margin = new Padding(0, 0, 8, 0) };
            exportBtn.Click += (s, e) => AppTheme.ExportGridToCsv(_detailGrid, "ThongKe");
            exportPanel.Controls.Add(exportBtn);

            // Detail grid
            _detailGrid = new Guna2DataGridView { Dock = DockStyle.Fill };
            AppTheme.StyleGrid(_detailGrid);
            Controls.Add(_detailGrid);
            _detailGrid.BringToFront();

            LoadInventory();
        }

        private void LoadInventory()
        {
            _detailGrid.DataSource = _service.Query(@"
                SELECT s.ma_sach AS 'Mã', s.tieu_de AS 'Tiêu đề', s.tac_gia AS 'Tác giả',
                       COALESCE(tl.ten_the_loai,'N/A') AS 'Thể loại',
                       CASE s.loai_sach WHEN 'SACH_GIAY' THEN 'Giấy' ELSE 'Online' END AS 'Loại',
                       (SELECT COUNT(*) FROM QUYEN_SACH q WHERE q.ma_sach=s.ma_sach AND q.trang_thai='CO_SAN') AS 'Có sẵn',
                       (SELECT COUNT(*) FROM QUYEN_SACH q WHERE q.ma_sach=s.ma_sach) AS 'Tổng quyển'
                FROM SACH s LEFT JOIN THE_LOAI_SACH tl ON s.ma_the_loai=tl.ma_the_loai ORDER BY s.ma_sach");
        }

        private void LoadHistory()
        {
            _detailGrid.DataSource = _service.Query(@"
                SELECT p.ma_phieu AS 'Mã', nd.ho_ten AS 'Đọc giả', s.tieu_de AS 'Sách',
                       p.ngay_muon AS 'Ngày mượn', p.ngay_hen_tra AS 'Hạn trả',
                       COALESCE(p.ngay_tra_thuc,'-') AS 'Ngày trả',
                       CASE p.trang_thai_phieu
                           WHEN 'DA_TRA' THEN '✅ Đã trả'
                           WHEN 'DANG_MUON' THEN CASE WHEN date(p.ngay_hen_tra) < date('now') THEN '⚠️ Quá hạn' ELSE '📖 Đang mượn' END
                           ELSE p.trang_thai_phieu
                       END AS 'Trạng thái',
                       COALESCE(p.tien_phat,0) AS 'Tiền phạt'
                FROM PHIEU_MUON_TRA p
                JOIN NGUOI_DUNG nd ON p.ma_nd_doc_gia=nd.ma_nd
                JOIN SACH s ON p.ma_sach=s.ma_sach ORDER BY p.ma_phieu DESC");
        }

        private void LoadTopBorrowed()
        {
            _detailGrid.DataSource = _service.Query(@"
                SELECT s.tieu_de AS 'Tiêu đề', s.tac_gia AS 'Tác giả',
                       COALESCE(tl.ten_the_loai, 'N/A') AS 'Thể loại',
                       COUNT(p.ma_phieu) AS 'Lượt mượn'
                FROM PHIEU_MUON_TRA p
                JOIN SACH s ON p.ma_sach = s.ma_sach
                LEFT JOIN THE_LOAI_SACH tl ON s.ma_the_loai = tl.ma_the_loai
                GROUP BY s.ma_sach, s.tieu_de, s.tac_gia, tl.ten_the_loai
                ORDER BY COUNT(p.ma_phieu) DESC
                LIMIT 10");
        }
    }

    public class AccountForm : Form
    {
        public AccountForm(User user)
        {
            var service = new LibraryService(DatabaseManager.Instance);
            Text = "Thông tin tài khoản";
            Dock = DockStyle.Fill;
            Padding = new Padding(24);
            Font = new Font("Segoe UI", 10F);
            var info = new Label
            {
                Dock = DockStyle.Top,
                Height = 150,
                Font = new Font("Segoe UI", 12F),
                Text = $"Họ tên: {user.HoTen}\nVai trò: {user.LoaiNguoiDung}\nEmail: {user.Email}\nSĐT: {user.SoDT}\nĐịa chỉ: {user.DiaChi}"
            };
            Controls.Add(info);
            var oldPass = new Guna2TextBox { PlaceholderText = "Mật khẩu cũ", PasswordChar = '●', Dock = DockStyle.Top, Height = 38 };
            var newPass = new Guna2TextBox { PlaceholderText = "Mật khẩu mới", PasswordChar = '●', Dock = DockStyle.Top, Height = 38 };
            var button = new Guna2Button { Text = "Đổi mật khẩu", Dock = DockStyle.Top, Height = 38, FillColor = Color.FromArgb(52, 152, 219), ForeColor = Color.White };
            button.Click += (s, e) =>
            {
                if (service.ChangePassword(user.MaNguoiDung, oldPass.Text, newPass.Text))
                    MessageBox.Show("Đổi mật khẩu thành công.");
                else
                    MessageBox.Show("Mật khẩu cũ không đúng.");
            };
            Controls.Add(button);
            Controls.Add(newPass);
            Controls.Add(oldPass);
        }
    }

    public class NotificationsForm : SimpleGridForm
    {
        private readonly User _user;
        public NotificationsForm(User user) : base("Thông báo")
        {
            _user = user;
            AddExtraButton("Đánh dấu đã đọc", Color.FromArgb(52, 152, 219), MarkRead);
            AddExtraButton("Xóa", Color.FromArgb(231, 76, 60), DeleteNotification);
        }
        protected override void LoadData()
        {
            Grid.DataSource = Service.Query(@"
                SELECT ma_thong_bao AS 'Mã', tieu_de AS 'Tiêu đề', noi_dung AS 'Nội dung', loai_thong_bao AS 'Loại',
                       CASE da_doc WHEN 1 THEN '✅ Đã đọc' ELSE '🔔 Chưa đọc' END AS 'Trạng thái',
                       ngay_tao AS 'Ngày tạo'
                FROM THONG_BAO WHERE (tieu_de LIKE @s OR noi_dung LIKE @s)
                ORDER BY ma_thong_bao DESC", new SQLiteParameter("@s", "%" + SearchText + "%"));
        }
        protected override void Add() { }
        protected override void Edit() { }
        protected override void Delete() { }
        private void MarkRead()
        {
            int id = SelectedId("Mã");
            if (id == 0) return;
            Service.Execute("UPDATE THONG_BAO SET da_doc = 1 WHERE ma_thong_bao = @id", new SQLiteParameter("@id", id));
            LoadData();
        }
        private void DeleteNotification()
        {
            int id = SelectedId("Mã");
            if (id == 0) return;
            Service.Execute("DELETE FROM THONG_BAO WHERE ma_thong_bao = @id", new SQLiteParameter("@id", id));
            LoadData();
        }
    }

    public abstract class SimpleGridForm : Form
    {
        protected readonly LibraryService Service;
        protected Guna2DataGridView Grid;
        private Guna2TextBox _search;
        private FlowLayoutPanel _bottomBar;
        private Panel _headerPanel;
        private TableLayoutPanel _filterPanel;
        private List<Guna2Button> _headerButtons = new List<Guna2Button>();
        protected string SearchText => _search?.Text?.Trim() ?? "";

        protected SimpleGridForm(string title)
        {
            Service = new LibraryService(DatabaseManager.Instance);
            Dock = DockStyle.Fill;
            BackColor = AppTheme.ContentBg;
            Padding = new Padding(24, 20, 24, 16);
            Font = AppTheme.BodyMedium;

            // ═══════════════════════════════════════════════════════════
            //  ROW 1 — HEADER: Title left, action buttons right
            // ═══════════════════════════════════════════════════════════
            _headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 56,
                BackColor = Color.Transparent,
                Name = "headerPanel"
            };
            Controls.Add(_headerPanel);

            _headerPanel.Controls.Add(new Label
            {
                Text = title,
                Font = AppTheme.HeadingLarge,
                ForeColor = AppTheme.TextDark,
                AutoSize = true,
                Location = new Point(0, 12)
            });

            // ═══════════════════════════════════════════════════════════
            //  ROW 2 — FILTER: Search + optional combos + refresh
            // ═══════════════════════════════════════════════════════════
            _filterPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 52,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 6, 0, 6),
                Name = "filterPanel"
            };
            _filterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); // search
            _filterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 48)); // refresh
            Controls.Add(_filterPanel);
            _filterPanel.BringToFront(); // Fix docking z-order so header stays on top

            _search = new Guna2TextBox
            {
                Dock = DockStyle.Fill,
                Height = 36,
                PlaceholderText = "🔍 Tìm kiếm...",
                BorderRadius = 6,
                BorderColor = AppTheme.BorderLight,
                FocusedState = { BorderColor = AppTheme.Primary }
            };
            _search.TextChanged += (s, e) => LoadData();
            _filterPanel.Controls.Add(_search, 0, 0);

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
            refreshBtn.Click += (s, e) => { _search.Text = ""; LoadData(); };
            _filterPanel.Controls.Add(refreshBtn, 1, 0);

            // ═══════════════════════════════════════════════════════════
            //  BOTTOM ACTION BAR
            // ═══════════════════════════════════════════════════════════
            _bottomBar = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 52,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0, 8, 0, 8),
                BackColor = Color.Transparent,
                Name = "bottomBar"
            };
            Controls.Add(_bottomBar);

            // Standard CRUD buttons in bottom bar
            _bottomBar.Controls.Add(MkBtn("➕ Thêm", AppTheme.Success, Add));
            _bottomBar.Controls.Add(MkBtn("✏️ Sửa", AppTheme.Info, Edit));
            _bottomBar.Controls.Add(MkBtn("🗑️ Xóa", AppTheme.Danger, Delete));
            _bottomBar.Controls.Add(MkBtn("📥 Xuất CSV", Color.FromArgb(108, 117, 125), () => AppTheme.ExportGridToCsv(Grid, title.Replace(" ", "_"))));

            // ═══════════════════════════════════════════════════════════
            //  DATA GRID — fills remaining space
            // ═══════════════════════════════════════════════════════════
            Grid = new Guna2DataGridView { Dock = DockStyle.Fill };
            AppTheme.StyleGrid(Grid);
            Grid.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) Edit(); };
            Controls.Add(Grid);
            Grid.BringToFront();

            Load += (s, e) => LoadData();
        }

        /// <summary>
        /// Adds a button to the HEADER ROW (right-aligned). Call in subclass constructor.
        /// </summary>
        protected void AddHeaderButton(string text, Color color, Action action)
        {
            var btn = MkBtn(text, color, action);
            btn.Width = Math.Max(130, TextRenderer.MeasureText(text, btn.Font).Width + 32);
            btn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _headerButtons.Add(btn);
            _headerPanel.Controls.Add(btn);
            RepositionHeaderButtons();
            _headerPanel.Resize += (s, e) => RepositionHeaderButtons();
        }

        private void RepositionHeaderButtons()
        {
            int right = _headerPanel.Width - 2;
            for (int i = 0; i < _headerButtons.Count; i++)
            {
                var btn = _headerButtons[i];
                right -= btn.Width;
                btn.Location = new Point(right, 10);
                right -= 10; // gap between buttons
            }
        }

        /// <summary>
        /// Adds a labelled filter ComboBox to the FILTER ROW. Call in subclass constructor.
        /// Returns the combo for external event binding.
        /// </summary>
        protected Guna2ComboBox AddFilterCombo(string label, int labelWidth = 80, string[] items = null)
        {
            // Rebuild column styles to insert label + combo before the refresh button
            int refreshCol = _filterPanel.ColumnCount - 1;
            _filterPanel.ColumnCount += 2; // add label col + combo col

            // Reconfigure: Search(flex) | ...existing filter pairs... | NewLabel | NewCombo | Refresh(48px)
            _filterPanel.ColumnStyles.Clear();
            // Column 0: search (flexible)
            _filterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));
            // Calculate how many filter pairs we have
            int pairCount = (_filterPanel.ColumnCount - 2) / 2; // minus search and refresh
            for (int p = 0; p < pairCount; p++)
            {
                _filterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, labelWidth));
                _filterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / pairCount));
            }
            // Last column: refresh
            _filterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 48));

            // Move refresh button to last column
            _filterPanel.SetColumn(_filterPanel.GetControlFromPosition(refreshCol, 0), _filterPanel.ColumnCount - 1);

            var lbl = new Label
            {
                Text = label,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                Font = AppTheme.BodySmall,
                ForeColor = AppTheme.TextDarkSecondary,
                Padding = new Padding(0, 0, 4, 0)
            };
            _filterPanel.Controls.Add(lbl, _filterPanel.ColumnCount - 3, 0);

            var combo = new Guna2ComboBox
            {
                Dock = DockStyle.Fill,
                Height = 36,
                BorderRadius = 6,
                BorderColor = AppTheme.BorderLight,
                FocusedState = { BorderColor = AppTheme.Primary }
            };
            if (items != null) combo.Items.AddRange(items);
            combo.SelectedIndexChanged += (s, e) => LoadData();
            _filterPanel.Controls.Add(combo, _filterPanel.ColumnCount - 2, 0);

            return combo;
        }

        protected void ClearBottomButtons()
        {
            _bottomBar?.Controls.Clear();
        }

        /// <summary>
        /// Adds an extra button to the BOTTOM action bar.
        /// </summary>
        protected Guna2Button AddExtraButton(string text, Color color, Action action)
        {
            var btn = MkBtn(text, color, action);
            _bottomBar?.Controls.Add(btn);
            return btn;
        }

        protected Guna2Button MkBtn(string text, Color color, Action action)
        {
            var button = new Guna2Button
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
            button.HoverState.FillColor = ControlPaint.Light(color, 0.15f);
            button.Click += (s, e) =>
            {
                try { action(); }
                catch (Exception ex) { MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            };
            return button;
        }

        // Keep backward compat
        protected Guna2Button Button(string text, Color color, Action action) => MkBtn(text, color, action);

        protected int SelectedId(string column)
        {
            if (Grid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn một dòng.");
                return 0;
            }
            return Convert.ToInt32(Grid.SelectedRows[0].Cells[column].Value);
        }

        protected abstract void LoadData();
        protected abstract void Add();
        protected abstract void Edit();
        protected abstract void Delete();
    }

    internal class PromptDialog : Form
    {
        private Guna2TextBox _input;
        public string Value => _input.Text;

        public static string Ask(string title, string prompt)
        {
            using (var dialog = new PromptDialog(title, prompt))
                return dialog.ShowDialog() == DialogResult.OK ? dialog.Value : "";
        }

        private PromptDialog(string title, string prompt)
        {
            Text = title;
            Size = new Size(380, 180);
            StartPosition = FormStartPosition.CenterParent;
            Padding = new Padding(16);
            Controls.Add(new Label { Text = prompt, Dock = DockStyle.Top, Height = 28 });
            _input = new Guna2TextBox { Dock = DockStyle.Top, Height = 36, BorderRadius = 5 };
            Controls.Add(_input);
            var ok = new Guna2Button { Text = "OK", Dock = DockStyle.Bottom, Height = 36, FillColor = Color.FromArgb(52, 152, 219), ForeColor = Color.White };
            ok.Click += (s, e) => DialogResult = DialogResult.OK;
            Controls.Add(ok);
        }
    }

    public class RegisterDialog : Form
    {
        private readonly LibraryService _service = new LibraryService(DatabaseManager.Instance);
        private Guna2TextBox _username, _password, _name, _address, _phone, _email;

        public RegisterDialog()
        {
            Text = "Đăng ký tài khoản độc giả";
            Size = new Size(430, 470);
            StartPosition = FormStartPosition.CenterParent;
            Padding = new Padding(20);
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1 };
            Controls.Add(layout);
            _name = AddInput(layout, "Họ tên");
            _username = AddInput(layout, "Tên đăng nhập");
            _password = AddInput(layout, "Mật khẩu");
            _password.PasswordChar = '●';
            _address = AddInput(layout, "Địa chỉ");
            _phone = AddInput(layout, "SĐT");
            _email = AddInput(layout, "Email");
            var save = new Guna2Button { Text = "Đăng ký", Dock = DockStyle.Bottom, Height = 38, FillColor = Color.FromArgb(46, 204, 113), ForeColor = Color.White };
            save.Click += (s, e) => Save();
            layout.Controls.Add(save);
        }

        private Guna2TextBox AddInput(TableLayoutPanel layout, string label)
        {
            var panel = new Panel { Height = 56, Dock = DockStyle.Top };
            panel.Controls.Add(new Label { Text = label, Dock = DockStyle.Top, Height = 20 });
            var input = new Guna2TextBox { Dock = DockStyle.Bottom, Height = 32, BorderRadius = 5 };
            panel.Controls.Add(input);
            layout.Controls.Add(panel);
            return input;
        }

        private void Save()
        {
            try
            {
                _service.RegisterUser(_username.Text.Trim(), _password.Text, _name.Text.Trim(), _address.Text.Trim(), _phone.Text.Trim(), _email.Text.Trim());
                MessageBox.Show("Đăng ký thành công. Bạn có thể đăng nhập ngay.");
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    /// <summary>
    /// Dialog for managing individual book copies (QUYEN_SACH).
    /// </summary>
    internal class CopyManagementDialog : Form
    {
        private readonly LibraryService _service;
        private readonly int _bookId;
        private Guna2DataGridView _grid;

        public CopyManagementDialog(LibraryService service, int bookId)
        {
            _service = service;
            _bookId = bookId;
            Build();
            LoadCopies();
        }

        private void Build()
        {
            Text = "Quản lý quyển sách";
            Size = new Size(700, 450);
            StartPosition = FormStartPosition.CenterParent;
            Padding = new Padding(16);
            Font = new Font("Segoe UI", 10F);

            // Header
            var header = new Panel { Dock = DockStyle.Top, Height = 45 };
            Controls.Add(header);

            string title = _service.Scalar("SELECT tieu_de FROM SACH WHERE ma_sach = @id",
                new SQLiteParameter("@id", _bookId))?.ToString() ?? "";
            header.Controls.Add(new Label
            {
                Text = $"📦 Quyển sách — {title}",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                AutoSize = true, Location = new Point(0, 8)
            });

            // Bottom actions
            var actions = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom, Height = 48,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0, 6, 0, 6)
            };
            Controls.Add(actions);

            var addBtn = Btn("➕ Thêm quyển", Color.FromArgb(46, 204, 113), AddCopy);
            addBtn.Width = 130;
            actions.Controls.Add(addBtn);

            var statusBtn = Btn("🔄 Đổi trạng thái", Color.FromArgb(52, 152, 219), ChangeStatus);
            statusBtn.Width = 140;
            actions.Controls.Add(statusBtn);

            var deleteBtn = Btn("🗑️ Xóa quyển", Color.FromArgb(220, 53, 69), DeleteCopy);
            deleteBtn.Width = 120;
            actions.Controls.Add(deleteBtn);

            // Grid
            _grid = new Guna2DataGridView { Dock = DockStyle.Fill };
            AppTheme.StyleGrid(_grid);
            _grid.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) ChangeStatus(); };
            Controls.Add(_grid);
            _grid.BringToFront();
        }

        private void LoadCopies()
        {
            _grid.DataSource = _service.Query(@"
                SELECT ma_quyen AS 'Mã quyển', ma_quyen_sach AS 'Mã quyển sách',
                       CASE trang_thai
                           WHEN 'CO_SAN' THEN '✅ Có sẵn'
                           WHEN 'DANG_MUON' THEN '📖 Đang mượn'
                           WHEN 'KHONG_CO_SAN' THEN '🔒 Đã đặt trước'
                           WHEN 'HONG' THEN '⚠️ Hỏng'
                           WHEN 'MAT' THEN '❌ Mất'
                           ELSE trang_thai
                       END AS 'Trạng thái',
                       ngay_nhap AS 'Ngày nhập'
                FROM QUYEN_SACH WHERE ma_sach = @id
                ORDER BY ma_quyen", new SQLiteParameter("@id", _bookId));
        }

        private void AddCopy()
        {
            string qtyStr = PromptDialog.Ask("Thêm quyển sách", "Nhập số lượng quyển cần thêm:");
            if (string.IsNullOrWhiteSpace(qtyStr)) return;
            if (!int.TryParse(qtyStr, out int qty) || qty <= 0 || qty > 100)
            {
                MessageBox.Show("Số lượng không hợp lệ (1-100).", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Get current total count of copies to generate the next sequential code
                int existingCount = _service.ScalarInt(
                    "SELECT COUNT(*) FROM QUYEN_SACH WHERE ma_sach = @id",
                    new SQLiteParameter("@id", _bookId));

                for (int i = 1; i <= qty; i++)
                {
                    int nextNum = existingCount + i;
                    string copyCode = $"S{_bookId:0000}-Q{nextNum:000}";

                    // Ensure the code is unique (handle edge cases from previous deletions)
                    int exists = _service.ScalarInt(
                        "SELECT COUNT(*) FROM QUYEN_SACH WHERE ma_quyen_sach = @code",
                        new SQLiteParameter("@code", copyCode));
                    while (exists > 0)
                    {
                        nextNum++;
                        copyCode = $"S{_bookId:0000}-Q{nextNum:000}";
                        exists = _service.ScalarInt(
                            "SELECT COUNT(*) FROM QUYEN_SACH WHERE ma_quyen_sach = @code",
                            new SQLiteParameter("@code", copyCode));
                    }

                    _service.Execute(@"
                        INSERT INTO QUYEN_SACH (ma_sach, ma_quyen_sach, trang_thai, ngay_nhap)
                        VALUES (@id, @code, 'CO_SAN', date('now'))",
                        new SQLiteParameter("@id", _bookId),
                        new SQLiteParameter("@code", copyCode));
                }

                // Update the SACH_GIAY quantity record to match actual count
                int newTotal = _service.ScalarInt(
                    "SELECT COUNT(*) FROM QUYEN_SACH WHERE ma_sach = @id",
                    new SQLiteParameter("@id", _bookId));
                _service.Execute(
                    "UPDATE SACH_GIAY SET so_luong = @qty WHERE ma_sach = @id",
                    new SQLiteParameter("@qty", newTotal),
                    new SQLiteParameter("@id", _bookId));

                MessageBox.Show($"Đã thêm {qty} quyển thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadCopies();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi thêm quyển: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ChangeStatus()
        {
            if (_grid.SelectedRows.Count == 0) { MessageBox.Show("Vui lòng chọn một quyển."); return; }
            int copyId = Convert.ToInt32(_grid.SelectedRows[0].Cells["Mã quyển"].Value);

            string currentStatus = _service.Scalar("SELECT trang_thai FROM QUYEN_SACH WHERE ma_quyen = @id",
                new SQLiteParameter("@id", copyId))?.ToString() ?? "";

            if (currentStatus == "DANG_MUON" || currentStatus == "KHONG_CO_SAN")
            {
                MessageBox.Show("Không thể đổi thông tin quyển đang cho mượn hoặc đã đặt trước.", "Thông báo");
                return;
            }

            using (var dialog = new CopyEditDialog(_service, copyId))
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    LoadCopies();
                }
            }
        }

        private void DeleteCopy()
        {
            if (_grid.SelectedRows.Count == 0) { MessageBox.Show("Vui lòng chọn một quyển."); return; }
            int copyId = Convert.ToInt32(_grid.SelectedRows[0].Cells["Mã quyển"].Value);

            string status = _service.Scalar("SELECT trang_thai FROM QUYEN_SACH WHERE ma_quyen = @id",
                new SQLiteParameter("@id", copyId))?.ToString() ?? "";
            if (status == "DANG_MUON")
            {
                MessageBox.Show("Không thể xóa quyển đang cho mượn.", "Thông báo");
                return;
            }

            if (MessageBox.Show("Xóa quyển sách này?", "Xác nhận", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                _service.Execute("DELETE FROM QUYEN_SACH WHERE ma_quyen = @id", new SQLiteParameter("@id", copyId));
                LoadCopies();
            }
        }

        private Guna2Button Btn(string text, Color color, Action action)
        {
            var btn = new Guna2Button
            {
                Text = text, Width = 112, Height = 36, BorderRadius = 5,
                FillColor = color, ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F),
                Margin = new Padding(0, 0, 8, 0), Cursor = Cursors.Hand
            };
            btn.HoverState.FillColor = ControlPaint.Light(color);
            btn.Click += (s, e) =>
            {
                try { action(); }
                catch (Exception ex) { MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            };
            return btn;
        }
    }

    internal class CopyEditDialog : Form
    {
        private readonly LibraryService _service;
        private readonly int _copyId;
        private Guna2ComboBox _statusCombo;
        private Guna2TextBox _locationTxt;
        private Guna2TextBox _notesTxt;

        public CopyEditDialog(LibraryService service, int copyId)
        {
            _service = service;
            _copyId = copyId;
            Build();
            LoadData();
        }

        private void Build()
        {
            Text = "Cập nhật quyển sách";
            Size = new Size(400, 420);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Padding = new Padding(20);
            Font = new Font("Segoe UI", 10F);
            
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1 };
            Controls.Add(layout);

            _statusCombo = AddCombo(layout, "Trạng thái");
            _statusCombo.Items.Add(new ComboItem("Có sẵn", 0));
            _statusCombo.Items.Add(new ComboItem("Hỏng", 1));
            _statusCombo.Items.Add(new ComboItem("Mất", 2));

            _locationTxt = AddInput(layout, "Vị trí (vd: Kệ A1, Tầng 2)");
            
            var notesPanel = new Panel { Dock = DockStyle.Top, Height = 100 };
            notesPanel.Controls.Add(new Label { Text = "Ghi chú", Dock = DockStyle.Top, Height = 22, ForeColor = Color.FromArgb(71, 85, 105) });
            _notesTxt = new Guna2TextBox { Dock = DockStyle.Fill, Multiline = true, BorderRadius = 5 };
            notesPanel.Controls.Add(_notesTxt);
            layout.Controls.Add(notesPanel);

            var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 48, FlowDirection = FlowDirection.RightToLeft };
            Controls.Add(buttons);
            buttons.BringToFront();
            
            var saveBtn = new Guna2Button { Text = "Lưu", Width = 90, Height = 36, BorderRadius = 5, FillColor = Color.FromArgb(46, 204, 113), ForeColor = Color.White, Margin = new Padding(8, 4, 0, 0) };
            saveBtn.Click += (s, e) => Save();
            buttons.Controls.Add(saveBtn);
            
            var cancelBtn = new Guna2Button { Text = "Hủy", Width = 90, Height = 36, BorderRadius = 5, FillColor = Color.FromArgb(231, 76, 60), ForeColor = Color.White, Margin = new Padding(8, 4, 0, 0) };
            cancelBtn.Click += (s, e) => DialogResult = DialogResult.Cancel;
            buttons.Controls.Add(cancelBtn);
        }

        private Guna2TextBox AddInput(TableLayoutPanel layout, string label)
        {
            var panel = new Panel { Dock = DockStyle.Top, Height = 65 };
            panel.Controls.Add(new Label { Text = label, Dock = DockStyle.Top, Height = 22, ForeColor = Color.FromArgb(71, 85, 105) });
            var input = new Guna2TextBox { Dock = DockStyle.Bottom, Height = 34, BorderRadius = 5 };
            panel.Controls.Add(input);
            layout.Controls.Add(panel);
            return input;
        }

        private Guna2ComboBox AddCombo(TableLayoutPanel layout, string label)
        {
            var panel = new Panel { Dock = DockStyle.Top, Height = 65 };
            panel.Controls.Add(new Label { Text = label, Dock = DockStyle.Top, Height = 22, ForeColor = Color.FromArgb(71, 85, 105) });
            var combo = new Guna2ComboBox { Dock = DockStyle.Bottom, Height = 34, BorderRadius = 5 };
            panel.Controls.Add(combo);
            layout.Controls.Add(panel);
            return combo;
        }

        private void LoadData()
        {
            var dt = _service.Query("SELECT trang_thai, vi_tri, ghi_chu FROM QUYEN_SACH WHERE ma_quyen = @id", new System.Data.SQLite.SQLiteParameter("@id", _copyId));
            if (dt.Rows.Count > 0)
            {
                var row = dt.Rows[0];
                string status = row["trang_thai"]?.ToString() ?? "";
                if (status == "HONG") _statusCombo.SelectedIndex = 1;
                else if (status == "MAT") _statusCombo.SelectedIndex = 2;
                else _statusCombo.SelectedIndex = 0; // CO_SAN

                _locationTxt.Text = row["vi_tri"]?.ToString() ?? "";
                _notesTxt.Text = row["ghi_chu"]?.ToString() ?? "";
            }
        }

        private void Save()
        {
            string status = "CO_SAN";
            if (_statusCombo.SelectedItem is ComboItem selected)
            {
                if (selected.Value == 1) status = "HONG";
                else if (selected.Value == 2) status = "MAT";
            }

            _service.Execute("UPDATE QUYEN_SACH SET trang_thai = @status, vi_tri = @loc, ghi_chu = @note WHERE ma_quyen = @id",
                new System.Data.SQLite.SQLiteParameter("@status", status),
                new System.Data.SQLite.SQLiteParameter("@loc", _locationTxt.Text.Trim()),
                new System.Data.SQLite.SQLiteParameter("@note", _notesTxt.Text.Trim()),
                new System.Data.SQLite.SQLiteParameter("@id", _copyId));
            DialogResult = DialogResult.OK;
        }
    }
}
