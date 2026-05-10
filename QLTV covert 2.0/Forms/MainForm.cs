using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using FontAwesome.Sharp;
using Guna.UI2.WinForms;
using QLTV_covert_2._0.Data;
using QLTV_covert_2._0.Models;
using QLTV_covert_2._0.Utilities;

namespace QLTV_covert_2._0.Forms
{
    public partial class MainForm : Form
    {
        private readonly User _currentUser;
        private readonly DatabaseManager _db;
        private Panel _sidebarPanel;
        private Panel _contentPanel;
        private Guna2Button _activeMenuButton;
        private readonly Dictionary<string, Guna2Button> _menuButtons = new Dictionary<string, Guna2Button>();

        /// <summary>True when user clicked Logout (vs closing the window).</summary>
        public bool IsLoggingOut { get; private set; }

        public MainForm(User user)
        {
            InitializeComponent();
            _currentUser = user;
            _db = DatabaseManager.Instance;
            SetupUI();
            ShowDashboard();
        }

        private void SetupUI()
        {
            Text = $"Thư Viện Số — {GetRoleDisplayName(_currentUser.LoaiNguoiDung)}";
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            BackColor = AppTheme.ContentBg;
            DoubleBuffered = true;
            Font = AppTheme.BodyMedium;
            MinimumSize = new Size(1280, 760);

            var root = new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.ContentBg };
            Controls.Add(root);

            _sidebarPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = AppTheme.SidebarWidth,
                BackColor = AppTheme.Surface0,
                Padding = new Padding(0)
            };
            root.Controls.Add(_sidebarPanel);

            _contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppTheme.ContentBg,
                Padding = new Padding(32, 28, 32, 24)
            };
            root.Controls.Add(_contentPanel);
            _contentPanel.BringToFront();

            BuildSidebar();
        }

        private void BuildSidebar()
        {
            // Brand header
            var brandPanel = new Panel { Dock = DockStyle.Top, Height = 72, BackColor = AppTheme.Surface0, Padding = new Padding(20, 0, 0, 0) };
            _sidebarPanel.Controls.Add(brandPanel);

            var brandIcon = new IconPictureBox
            {
                IconChar = IconChar.BookOpenReader, IconColor = AppTheme.Primary, IconSize = 24,
                BackColor = Color.Transparent, Location = new Point(20, 24), Size = new Size(28, 28)
            };
            brandPanel.Controls.Add(brandIcon);

            brandPanel.Controls.Add(new Label
            {
                Text = "THƯ VIỆN SỐ", Location = new Point(52, 20), AutoSize = true,
                Font = AppTheme.HeadingMedium, ForeColor = AppTheme.TextPrimary, BackColor = Color.Transparent
            });

            // Divider
            var div1 = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = AppTheme.Border };
            _sidebarPanel.Controls.Add(div1);

            // User card
            var userPanel = new Panel { Dock = DockStyle.Top, Height = 90, BackColor = AppTheme.Surface1, Padding = new Padding(20, 14, 20, 14) };
            _sidebarPanel.Controls.Add(userPanel);

            var avatar = new IconPictureBox
            {
                IconChar = IconChar.CircleUser, IconColor = AppTheme.Primary, IconSize = 36,
                BackColor = Color.Transparent, Location = new Point(20, 20), Size = new Size(40, 40)
            };
            userPanel.Controls.Add(avatar);

            userPanel.Controls.Add(new Label
            {
                Text = _currentUser.HoTen, Location = new Point(68, 16), Width = 170, Height = 24,
                Font = AppTheme.HeadingSmall, ForeColor = AppTheme.TextPrimary, BackColor = Color.Transparent
            });
            userPanel.Controls.Add(new Label
            {
                Text = GetRoleDisplayName(_currentUser.LoaiNguoiDung), Location = new Point(68, 42), Width = 170, Height = 20,
                Font = AppTheme.Caption, ForeColor = AppTheme.TextMuted, BackColor = Color.Transparent
            });

            // Logout button at bottom — styled to match sidebar
            var logoutButton = new Guna2Button
            {
                Text = "  Đăng xuất", TextAlign = HorizontalAlignment.Left,
                ImageAlign = HorizontalAlignment.Left,
                Image = IconChar.RightFromBracket.ToBitmap(Color.White, 16),
                ImageSize = new Size(16, 16),
                Dock = DockStyle.Bottom, Height = 44,
                Margin = new Padding(12, 0, 12, 16),
                BorderRadius = AppTheme.RadiusSmall, BorderThickness = 1,
                BorderColor = AppTheme.Danger,
                FillColor = Color.Transparent,
                ForeColor = AppTheme.Danger,
                Font = AppTheme.ButtonFont,
                Padding = new Padding(14, 0, 8, 0),
                Cursor = Cursors.Hand, Animated = true
            };
            logoutButton.HoverState.FillColor = AppTheme.Danger;
            logoutButton.HoverState.ForeColor = Color.White;
            logoutButton.Click += LogoutButton_Click;
            _sidebarPanel.Controls.Add(logoutButton);

            // Menu area
            var menuPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown,
                WrapContents = false, AutoScroll = false,
                BackColor = AppTheme.Surface0, Padding = new Padding(12, 16, 12, 0)
            };
            _sidebarPanel.Controls.Add(menuPanel);
            menuPanel.BringToFront();

            // Section label
            menuPanel.Controls.Add(CreateSectionLabel("TỔNG QUAN"));
            AddMenu(menuPanel, "Tổng quan", IconChar.ChartSimple, ShowDashboard, true);

            if (_currentUser.LoaiNguoiDung == "NHAN_VIEN" || _currentUser.LoaiNguoiDung == "ADMIN")
            {
                menuPanel.Controls.Add(CreateSectionLabel("QUẢN LÝ"));
                AddMenu(menuPanel, "Quản lý Sách", IconChar.Book, () => OpenChildForm(new BookManagementForm()), false);
                AddMenu(menuPanel, "Quản lý Độc giả", IconChar.Users, () => OpenChildForm(new ReaderManagementForm()), false);
                AddMenu(menuPanel, "Mượn/Trả Sách", IconChar.ClipboardList, () => OpenChildForm(new BorrowManagementForm(_currentUser)), false);
                AddMenu(menuPanel, "Yêu cầu mượn sách", IconChar.FileCircleQuestion, () => OpenChildForm(new BorrowRequestsForm(_currentUser)), false);
            }

            if (_currentUser.LoaiNguoiDung == "ADMIN")
            {
                menuPanel.Controls.Add(CreateSectionLabel("HỆ THỐNG"));
                AddMenu(menuPanel, "Quản lý Nhân viên", IconChar.IdBadge, () => OpenChildForm(new StaffManagementForm()), false);
                AddMenu(menuPanel, "Thống kê", IconChar.TableCells, () => OpenChildForm(new StatisticsForm()), false);
            }

            if (_currentUser.LoaiNguoiDung == "DOC_GIA")
            {
                menuPanel.Controls.Add(CreateSectionLabel("DỊCH VỤ"));
                AddMenu(menuPanel, "Tìm kiếm sách", IconChar.Search, () => OpenChildForm(new SearchBooksForm(_currentUser)), false);
                AddMenu(menuPanel, "Sách của tôi", IconChar.BookBookmark, () => OpenChildForm(new MyBorrowsForm(_currentUser)), false);
                AddMenu(menuPanel, "Yêu cầu của tôi", IconChar.ListCheck, () => OpenChildForm(new MyRequestsForm(_currentUser)), false);
            }

            menuPanel.Controls.Add(CreateSectionLabel("CÁ NHÂN"));
            AddMenu(menuPanel, "Tài khoản", IconChar.User, () => OpenChildForm(new AccountForm(_currentUser)), false);
        }

        private Label CreateSectionLabel(string text)
        {
            return new Label
            {
                Text = text, Width = 220, Height = 28, Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = AppTheme.TextMuted, BackColor = Color.Transparent,
                Padding = new Padding(8, 8, 0, 0), Margin = new Padding(0, 4, 0, 0)
            };
        }

        private void AddMenu(FlowLayoutPanel parent, string text, IconChar icon, Action onClick, bool active)
        {
            var button = CreateSidebarButton(text, icon, Color.Transparent);
            button.ForeColor = AppTheme.TextSecondary;
            button.Width = 232;
            button.Height = 40;
            button.Margin = new Padding(0, 0, 0, 2);
            button.Click += (s, e) => { SetActiveMenu(button); onClick?.Invoke(); };
            parent.Controls.Add(button);
            _menuButtons[text] = button;
            if (active) SetActiveMenu(button);
        }

        private Guna2Button CreateSidebarButton(string text, IconChar icon, Color fillColor)
        {
            var fc = fillColor == Color.Transparent ? AppTheme.TextSecondary : Color.White;
            var button = new Guna2Button
            {
                Text = text, TextAlign = HorizontalAlignment.Left, ImageAlign = HorizontalAlignment.Left,
                Image = icon.ToBitmap(fc, 16), ImageSize = new Size(16, 16),
                BorderRadius = AppTheme.RadiusSmall, BorderThickness = 0,
                FillColor = fillColor, ForeColor = fc,
                Font = AppTheme.MenuFont, Padding = new Padding(14, 0, 8, 0),
                TextOffset = new Point(8, 0), Cursor = Cursors.Hand, Animated = true
            };
            button.Tag = icon;
            button.HoverState.FillColor = fillColor == Color.Transparent ? AppTheme.Surface2 : ControlPaint.Light(fillColor);
            button.HoverState.ForeColor = fillColor == Color.Transparent ? AppTheme.TextPrimary : Color.White;
            return button;
        }

        private void SetActiveMenu(Guna2Button button)
        {
            if (_activeMenuButton != null)
            {
                _activeMenuButton.FillColor = Color.Transparent;
                _activeMenuButton.ForeColor = AppTheme.TextSecondary;
                _activeMenuButton.Image = ((IconChar)_activeMenuButton.Tag).ToBitmap(AppTheme.TextSecondary, 16);
                _activeMenuButton.HoverState.FillColor = AppTheme.Surface2;
            }
            _activeMenuButton = button;
            _activeMenuButton.FillColor = AppTheme.PrimarySubtle;
            _activeMenuButton.ForeColor = AppTheme.Primary;
            _activeMenuButton.Image = ((IconChar)_activeMenuButton.Tag).ToBitmap(AppTheme.Primary, 16);
            _activeMenuButton.HoverState.FillColor = AppTheme.PrimarySubtle;
        }

        private void ActivateMenuContaining(string text)
        {
            foreach (var pair in _menuButtons)
                if (pair.Key.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0)
                { SetActiveMenu(pair.Value); return; }
        }

        private void ShowDashboard()
        {
            _contentPanel.Controls.Clear();
            DashboardStats stats = LoadDashboardStats();

            // Header
            var header = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = AppTheme.ContentBg };
            _contentPanel.Controls.Add(header);
            header.Controls.Add(new Label
            {
                Text = $"Xin chào, {_currentUser.HoTen}!", AutoSize = true, Location = new Point(0, 0),
                Font = AppTheme.HeadingLarge, ForeColor = AppTheme.TextDark
            });
            header.Controls.Add(new Label
            {
                Text = $"{GetRoleDisplayName(_currentUser.LoaiNguoiDung)} • {DateTime.Now:dddd, dd/MM/yyyy}",
                AutoSize = true, Location = new Point(2, 32),
                Font = AppTheme.BodySmall, ForeColor = AppTheme.TextDarkSecondary
            });

            // Stats row
            var statsPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Top, Height = 130, ColumnCount = 4, RowCount = 1,
                BackColor = AppTheme.ContentBg, Padding = new Padding(0, 8, 0, 16)
            };
            for (int i = 0; i < 4; i++) statsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            _contentPanel.Controls.Add(statsPanel);
            statsPanel.BringToFront();

            statsPanel.Controls.Add(AppTheme.CreateStatCard(stats.TotalBooks.ToString(), "Tổng số sách", AppTheme.Info), 0, 0);
            statsPanel.Controls.Add(AppTheme.CreateStatCard(stats.Borrowing.ToString(), "Đang mượn", AppTheme.Success), 1, 0);
            statsPanel.Controls.Add(AppTheme.CreateStatCard(stats.Overdue.ToString(), "Quá hạn", AppTheme.Danger), 2, 0);
            statsPanel.Controls.Add(AppTheme.CreateStatCard(FormatCurrency(stats.Fines), "Tiền phạt", AppTheme.Warning), 3, 0);

            // Body: quick actions + notifications
            var body = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1,
                BackColor = AppTheme.ContentBg, Padding = new Padding(0, 4, 0, 0)
            };
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            _contentPanel.Controls.Add(body);
            body.BringToFront();

            body.Controls.Add(CreateQuickActionsCard(), 0, 0);
            body.Controls.Add(CreateNotificationCard(stats.UnreadNotifications), 1, 0);
        }

        private Guna2Panel CreateQuickActionsCard()
        {
            var card = new Guna2Panel
            {
                Dock = DockStyle.Fill, Margin = new Padding(0, 0, 12, 0),
                BorderRadius = AppTheme.RadiusMedium, FillColor = AppTheme.CardWhite,
                Padding = new Padding(24, 20, 24, 20)
            };

            card.Controls.Add(new Label
            {
                Text = "Thao tác nhanh", Dock = DockStyle.Top, Height = 36,
                Font = AppTheme.HeadingSmall, ForeColor = AppTheme.TextDark, BackColor = Color.Transparent
            });

            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Top, Height = 120, FlowDirection = FlowDirection.TopDown,
                WrapContents = false, BackColor = Color.Transparent, Padding = new Padding(0, 8, 0, 0)
            };
            card.Controls.Add(flow);
            flow.BringToFront();

            if (_currentUser.LoaiNguoiDung != "DOC_GIA")
            {
                var b1 = AppTheme.CreateActionButton("Xử lý yêu cầu mượn", AppTheme.Primary, 220, 38);
                b1.Click += (s, e) => { ActivateMenuContaining("Yêu cầu"); OpenChildForm(new BorrowRequestsForm(_currentUser)); };
                flow.Controls.Add(b1);

                var b2 = AppTheme.CreateActionButton("Quản lý sách", AppTheme.Secondary, 220, 38);
                b2.Click += (s, e) => { ActivateMenuContaining("Sách"); OpenChildForm(new BookManagementForm()); };
                flow.Controls.Add(b2);
            }
            else
            {
                var b1 = AppTheme.CreateActionButton("Tìm kiếm sách", AppTheme.Primary, 220, 38);
                b1.Click += (s, e) => { ActivateMenuContaining("Tìm"); OpenChildForm(new SearchBooksForm(_currentUser)); };
                flow.Controls.Add(b1);

                var b2 = AppTheme.CreateActionButton("Sách của tôi", AppTheme.Secondary, 220, 38);
                b2.Click += (s, e) => { ActivateMenuContaining("Sách của"); OpenChildForm(new MyBorrowsForm(_currentUser)); };
                flow.Controls.Add(b2);
            }

            return card;
        }

        private Guna2Panel CreateNotificationCard(int unreadCount)
        {
            var card = new Guna2Panel
            {
                Dock = DockStyle.Fill, Margin = new Padding(4, 0, 0, 0),
                BorderRadius = AppTheme.RadiusMedium, FillColor = AppTheme.CardWhite,
                Padding = new Padding(24, 20, 24, 12)
            };

            // Header with actions
            var hdr = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = Color.Transparent };
            card.Controls.Add(hdr);

            hdr.Controls.Add(new Label
            {
                Text = $"Thông báo ({unreadCount} chưa đọc)", AutoSize = true, Location = new Point(0, 6),
                Font = AppTheme.HeadingSmall, ForeColor = AppTheme.TextDark
            });

            var clearBtn = AppTheme.CreateActionButton("Xóa tất cả", AppTheme.Danger, 100, 30);
            clearBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            clearBtn.Location = new Point(hdr.Width - 160, 4);
            clearBtn.Click += (s, e) =>
            {
                var svc = new LibraryService(_db);
                svc.Execute("DELETE FROM THONG_BAO WHERE ma_nd = @id", new SQLiteParameter("@id", _currentUser.MaNguoiDung));
                ShowDashboard();
            };
            hdr.Controls.Add(clearBtn);

            var refreshBtn = AppTheme.CreateActionButton("⟳", AppTheme.Info, 36, 30);
            refreshBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            refreshBtn.Location = new Point(hdr.Width - 52, 4);
            refreshBtn.Click += (s, e) => ShowDashboard();
            hdr.Controls.Add(refreshBtn);
            hdr.Resize += (s, e) => { clearBtn.Left = hdr.Width - 160; refreshBtn.Left = hdr.Width - 52; };

            // Notification list
            var list = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown,
                WrapContents = false, AutoScroll = true, BackColor = Color.Transparent,
                Padding = new Padding(0, 8, 0, 0)
            };
            card.Controls.Add(list);
            list.BringToFront();

            LoadNotifications(list);
            return card;
        }

        private void LoadNotifications(FlowLayoutPanel list)
        {
            try
            {
                using (var connection = _db.GetConnection())
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"SELECT ma_thong_bao, tieu_de, noi_dung, ngay_tao, link_lien_quan, loai_thong_bao
                            FROM THONG_BAO WHERE ma_nd = @userId AND da_doc = 0 ORDER BY datetime(ngay_tao) DESC";
                        command.Parameters.AddWithValue("@userId", _currentUser.MaNguoiDung);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                                list.Controls.Add(CreateNotificationItem(
                                    Convert.ToInt32(reader["ma_thong_bao"]), reader["tieu_de"].ToString(),
                                    reader["noi_dung"].ToString(), FormatDate(reader["ngay_tao"].ToString()),
                                    reader["link_lien_quan"]?.ToString() ?? "", reader["loai_thong_bao"]?.ToString() ?? ""));
                        }
                    }
                }
            }
            catch { }

            if (list.Controls.Count == 0)
                list.Controls.Add(CreateNotificationItem(0, "Chưa có thông báo", "Các yêu cầu mới sẽ xuất hiện tại đây.", "", "", ""));
        }

        private Guna2Panel CreateNotificationItem(int notificationId, string title, string content, string date, string link, string type)
        {
            var item = new Guna2Panel
            {
                Width = 700, Height = 90, BorderRadius = AppTheme.RadiusSmall,
                FillColor = Color.FromArgb(248, 250, 252), Margin = new Padding(0, 0, 0, 6),
                Padding = new Padding(16, 10, 16, 10), BorderThickness = 1,
                BorderColor = AppTheme.BorderLight
            };

            item.Controls.Add(new Label
            {
                Text = "●", ForeColor = notificationId > 0 ? AppTheme.Success : AppTheme.TextMuted,
                Location = new Point(14, 14), AutoSize = true, BackColor = Color.Transparent, Font = AppTheme.Caption
            });
            item.Controls.Add(new Label
            {
                Text = title, Location = new Point(32, 12), Width = 460, Height = 22,
                Font = new Font("Segoe UI Semibold", 9.5F), ForeColor = AppTheme.TextDark, BackColor = Color.Transparent
            });
            item.Controls.Add(new Label
            {
                Text = date, Location = new Point(530, 12), Width = 140, Height = 20,
                TextAlign = ContentAlignment.MiddleRight, Font = AppTheme.Caption,
                ForeColor = AppTheme.TextDarkSecondary, BackColor = Color.Transparent
            });
            item.Controls.Add(new Label
            {
                Text = content, Location = new Point(32, 36), Width = 620, Height = 20,
                Font = AppTheme.Caption, ForeColor = AppTheme.TextDarkSecondary, BackColor = Color.Transparent
            });

            if (notificationId > 0)
            {
                var delBtn = AppTheme.CreateActionButton("Xóa", AppTheme.Danger, 52, 24);
                delBtn.Font = AppTheme.Caption;
                delBtn.Location = new Point(32, 60);
                delBtn.Click += (s, e) =>
                {
                    var svc = new LibraryService(_db);
                    svc.Execute("DELETE FROM THONG_BAO WHERE ma_thong_bao = @id", new SQLiteParameter("@id", notificationId));
                    ShowDashboard();
                };
                item.Controls.Add(delBtn);

                var handleBtn = AppTheme.CreateActionButton("Xử lý", AppTheme.Primary, 70, 24);
                handleBtn.Font = AppTheme.Caption;
                handleBtn.Location = new Point(92, 60);
                handleBtn.Click += (s, e) =>
                {
                    var svc = new LibraryService(_db);
                    svc.Execute("UPDATE THONG_BAO SET da_doc = 1 WHERE ma_thong_bao = @id", new SQLiteParameter("@id", notificationId));
                    if (type == "YEU_CAU_MOI" || link.StartsWith("yeu_cau"))
                    { 
                        if (_currentUser.LoaiNguoiDung == "DOC_GIA")
                        {
                            ActivateMenuContaining("Yêu cầu của tôi"); 
                            OpenChildForm(new MyRequestsForm(_currentUser));
                        }
                        else
                        {
                            ActivateMenuContaining("Yêu cầu"); 
                            OpenChildForm(new BorrowRequestsForm(_currentUser)); 
                        }
                    }
                    else if (link.StartsWith("phieu_muon"))
                    { 
                        if (_currentUser.LoaiNguoiDung == "DOC_GIA")
                        {
                            ActivateMenuContaining("Sách của tôi"); 
                            OpenChildForm(new MyBorrowsForm(_currentUser));
                        }
                        else
                        {
                            ActivateMenuContaining("Mượn"); 
                            OpenChildForm(new BorrowManagementForm(_currentUser)); 
                        }
                    }
                    else
                    { ActivateMenuContaining("Thông báo"); OpenChildForm(new NotificationsForm(_currentUser)); }
                };
                item.Controls.Add(handleBtn);
            }

            return item;
        }

        private DashboardStats LoadDashboardStats()
        {
            var stats = new DashboardStats();
            try
            {
                using (var c = _db.GetConnection())
                {
                    c.Open();
                    stats.TotalBooks = ExecInt(c, "SELECT COUNT(*) FROM SACH");
                    stats.Borrowing = ExecInt(c, "SELECT COUNT(*) FROM PHIEU_MUON_TRA WHERE trang_thai_phieu = 'DANG_MUON'");
                    stats.Overdue = ExecInt(c, "SELECT COUNT(*) FROM PHIEU_MUON_TRA WHERE trang_thai_phieu IN ('DANG_MUON', 'QUA_HAN') AND date(ngay_hen_tra) < date('now')");
                    stats.Fines = ExecDec(c, @"
                        SELECT 
                            COALESCE(SUM(tien_phat), 0) + 
                            COALESCE(SUM(CASE WHEN trang_thai_phieu != 'DA_TRA' AND julianday('now') > julianday(ngay_hen_tra) 
                                              THEN CAST((julianday('now') - julianday(ngay_hen_tra)) * 5000 AS INTEGER) 
                                              ELSE 0 END), 0) 
                        FROM PHIEU_MUON_TRA");
                    stats.UnreadNotifications = ExecInt(c, "SELECT COUNT(*) FROM THONG_BAO WHERE da_doc = 0 AND ma_nd = @userId", new SQLiteParameter("@userId", _currentUser.MaNguoiDung));
                }
            }
            catch { }
            return stats;
        }

        private int ExecInt(SQLiteConnection c, string q, params SQLiteParameter[] parameters)
        {
            using (var cmd = c.CreateCommand()) { 
                cmd.CommandText = q; 
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                var v = cmd.ExecuteScalar(); 
                return v == null || v == DBNull.Value ? 0 : Convert.ToInt32(v); 
            }
        }

        private decimal ExecDec(SQLiteConnection c, string q)
        {
            using (var cmd = c.CreateCommand()) { cmd.CommandText = q; var v = cmd.ExecuteScalar(); return v == null || v == DBNull.Value ? 0 : Convert.ToDecimal(v); }
        }

        private string FormatCurrency(decimal v) => v == 0 ? "0đ" : v.ToString("N0", new System.Globalization.CultureInfo("vi-VN")) + "đ";
        private string FormatDate(string v) { DateTime d; return DateTime.TryParse(v, out d) ? d.ToString("dd/MM/yyyy") : v; }

        private void OpenChildForm(Form childForm)
        {
            _contentPanel.Controls.Clear();
            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;
            _contentPanel.Controls.Add(childForm);
            childForm.Show();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Skip confirmation when logging out
            if (IsLoggingOut)
            {
                base.OnFormClosing(e);
                return;
            }

            if (e.CloseReason == CloseReason.UserClosing)
            {
                DialogResult result = MessageBox.Show("Bạn có muốn đóng ứng dụng không?", "Xác nhận thoát", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.No) { e.Cancel = true; return; }
            }
            base.OnFormClosing(e);
            Application.Exit();
        }

        private void LogoutButton_Click(object sender, EventArgs e)
        {
            IsLoggingOut = true;
            Close();  // Triggers FormClosed → LoginForm handler re-shows login
        }

        private string GetRoleDisplayName(string role)
        {
            if (role == "ADMIN") return "Quản trị viên";
            if (role == "NHAN_VIEN") return "Nhân viên";
            if (role == "DOC_GIA") return "Độc giả";
            return "Người dùng";
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            AutoScaleDimensions = new SizeF(8F, 16F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1400, 800);
            Name = "MainForm";
            Text = "Main";
            ResumeLayout(false);
        }

        private class DashboardStats
        {
            public int TotalBooks { get; set; }
            public int Borrowing { get; set; }
            public int Overdue { get; set; }
            public int UnreadNotifications { get; set; }
            public decimal Fines { get; set; }
        }
    }
}
