using System;
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
    public partial class LoginForm : Form
    {
        private readonly DatabaseManager _db;
        private Guna2TextBox _usernameInput;
        private Guna2TextBox _passwordInput;

        public LoginForm()
        {
            InitializeComponent();
            _db = DatabaseManager.Instance;
            SetupUI();
        }

        private void SetupUI()
        {
            Text = "Thư Viện Số — Đăng nhập";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.None;
            Size = new Size(420, 495);
            BackColor = AppTheme.Surface1;   // Form IS the card — no outer wrapper
            DoubleBuffered = true;
            Font = AppTheme.BodyMedium;

            // Rounded corners applied directly to the form
            SetRoundedRegion(AppTheme.RadiusLarge);
            Resize += (s, e) => SetRoundedRegion(AppTheme.RadiusLarge);

            // Layout
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, BackColor = Color.Transparent,
                ColumnCount = 1, RowCount = 12,
                Padding = new Padding(40, 16, 40, 20)
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));  // 0: close / drag
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));  // 1: icon
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));  // 2: title
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));  // 3: subtitle
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));  // 4: divider
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 80));  // 5: username
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 80));  // 6: password
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));  // 7: checkboxes
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 18));  // 8: spacer
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));  // 9: login
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));  // 10: register
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // 11: footer
            Controls.Add(layout);

            // ── Row 0: Drag area + Close (IconButton) ──
            var topBar = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            topBar.MouseDown += (s, e) => { if (e.Button == MouseButtons.Left) { NativeMethods.ReleaseCapture(); NativeMethods.SendMessage(Handle, 0xA1, 0x2, 0); } };
            layout.Controls.Add(topBar, 0, 0);

            var closeBtn = new IconButton
            {
                IconChar = IconChar.Xmark, IconColor = AppTheme.TextMuted, IconSize = 18,
                FlatStyle = FlatStyle.Flat, Size = new Size(32, 32),
                BackColor = Color.Transparent, Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            closeBtn.FlatAppearance.BorderSize = 0;
            closeBtn.FlatAppearance.MouseOverBackColor = AppTheme.Danger;
            closeBtn.MouseEnter += (s, e) => closeBtn.IconColor = Color.White;
            closeBtn.MouseLeave += (s, e) => closeBtn.IconColor = AppTheme.TextMuted;
            closeBtn.Click += (s, e) => Application.Exit();
            topBar.Controls.Add(closeBtn);
            closeBtn.Location = new Point(topBar.Width - 36, 4);
            topBar.Resize += (s, e) => closeBtn.Location = new Point(topBar.Width - 36, 4);

            // ── Row 1: Logo ──
            layout.Controls.Add(new IconPictureBox
            {
                IconChar = IconChar.BookOpenReader, IconColor = AppTheme.Primary,
                IconSize = 52, Dock = DockStyle.Fill, BackColor = Color.Transparent,
                SizeMode = PictureBoxSizeMode.CenterImage
            }, 0, 1);

            // ── Row 2: Title ──
            layout.Controls.Add(new Label
            {
                Text = "THƯ VIỆN SỐ", Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = AppTheme.DisplayMedium, ForeColor = AppTheme.TextPrimary,
                BackColor = Color.Transparent
            }, 0, 2);

            // ── Row 3: Subtitle ──
            layout.Controls.Add(new Label
            {
                Text = "Hệ thống Quản lý Thư viện", Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.TopCenter,
                Font = AppTheme.BodyLarge, ForeColor = AppTheme.TextMuted,
                BackColor = Color.Transparent
            }, 0, 3);

            // ── Row 4: Divider ──
            var divider = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            divider.Paint += (s, e) =>
            {
                using (var pen = new Pen(AppTheme.Border, 1))
                    e.Graphics.DrawLine(pen, 40, divider.Height / 2, divider.Width - 40, divider.Height / 2);
            };
            layout.Controls.Add(divider, 0, 4);

            // ── Row 5: Username ──
            var u = CreateInputGroup("Tên đăng nhập / Email", "Nhập tên đăng nhập hoặc Email", IconChar.User, false);
            _usernameInput = u.Item2;
            layout.Controls.Add(u.Item1, 0, 5);

            // ── Row 6: Password ──
            var p = CreateInputGroup("Mật khẩu / Mã độc giả", "Nhập mật khẩu hoặc Mã độc giả", IconChar.Lock, true);
            _passwordInput = p.Item2;
            layout.Controls.Add(p.Item1, 0, 6);

            // ── Row 7: Checkboxes ──
            var opts = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, BackColor = Color.Transparent,
                ColumnCount = 2, RowCount = 1
            };
            opts.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            opts.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            layout.Controls.Add(opts, 0, 7);

            var showPwd = new Guna2CheckBox
            {
                Text = "Hiện mật khẩu", Dock = DockStyle.Left, AutoSize = true,
                Font = AppTheme.Caption, ForeColor = AppTheme.TextSecondary,
                CheckedState = { BorderColor = AppTheme.Primary, FillColor = AppTheme.Primary }
            };
            showPwd.CheckedChanged += (s, e) => _passwordInput.PasswordChar = showPwd.Checked ? '\0' : '●';
            opts.Controls.Add(showPwd, 0, 0);

            opts.Controls.Add(new Guna2CheckBox
            {
                Text = "Ghi nhớ đăng nhập", Dock = DockStyle.Right, AutoSize = true,
                Font = AppTheme.Caption, ForeColor = AppTheme.TextSecondary,
                CheckedState = { BorderColor = AppTheme.Primary, FillColor = AppTheme.Primary }
            }, 1, 0);

            // ── Row 9: Login ──
            var loginBtn = new Guna2Button
            {
                Text = "ĐĂNG NHẬP", Dock = DockStyle.Fill,
                BorderRadius = AppTheme.RadiusSmall, FillColor = AppTheme.Primary,
                ForeColor = Color.White, Font = new Font("Segoe UI Semibold", 12F),
                Cursor = Cursors.Hand, Animated = true
            };
            loginBtn.HoverState.FillColor = AppTheme.PrimaryHover;
            loginBtn.Click += (s, e) => DoLogin();
            layout.Controls.Add(loginBtn, 0, 9);

            // ── Row 10: Register ──
            var regBtn = AppTheme.CreateSecondaryButton("ĐĂNG KÝ TÀI KHOẢN");
            regBtn.Dock = DockStyle.Fill;
            regBtn.Click += (s, e) => { using (var d = new RegisterDialog()) d.ShowDialog(this); };
            layout.Controls.Add(regBtn, 0, 10);

            // ── Row 11: Footer ──
            layout.Controls.Add(new Label
            {
                Text = "© 2026 Thư Viện Số — QLTV v2.0", Dock = DockStyle.Bottom,
                Height = 24, TextAlign = ContentAlignment.MiddleCenter,
                Font = AppTheme.Caption, ForeColor = AppTheme.TextMuted,
                BackColor = Color.Transparent
            }, 0, 11);

            // Keyboard
            KeyPreview = true;
            KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter) { DoLogin(); e.Handled = true; }
                if (e.KeyCode == Keys.Escape) Application.Exit();
            };
        }

        private Tuple<Panel, Guna2TextBox> CreateInputGroup(string label, string placeholder, IconChar icon, bool isPwd)
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            panel.Controls.Add(new Label
            {
                Text = label, Dock = DockStyle.Top, Height = 22,
                Font = AppTheme.BodySmall, ForeColor = AppTheme.TextSecondary,
                Padding = new Padding(2, 0, 0, 2)
            });
            var tb = AppTheme.CreateDarkInput(placeholder, isPwd);
            tb.Dock = DockStyle.Bottom;
            tb.Height = 46;
            tb.IconLeft = icon.ToBitmap(AppTheme.TextMuted, 16);
            tb.IconLeftSize = new Size(16, 16);
            tb.TextOffset = new Point(4, 0);
            panel.Controls.Add(tb);
            return new Tuple<Panel, Guna2TextBox>(panel, tb);
        }

        private void SetRoundedRegion(int radius)
        {
            var path = new GraphicsPath();
            var r = new Rectangle(0, 0, Width, Height);
            path.AddArc(r.X, r.Y, radius * 2, radius * 2, 180, 90);
            path.AddArc(r.Right - radius * 2, r.Y, radius * 2, radius * 2, 270, 90);
            path.AddArc(r.Right - radius * 2, r.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
            path.AddArc(r.X, r.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
            path.CloseFigure();
            Region = new Region(path);
        }

        private void DoLogin()
        {
            string username = _usernameInput.Text, password = _passwordInput.Text;
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            { MessageBox.Show("Vui lòng nhập tên đăng nhập và mật khẩu!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            if (_db.AuthenticateUser(username.Trim(), password))
            {
                User user = _db.GetUserByUsername(username.Trim());
                if (user != null)
                {
                    var mainForm = new MainForm(user);
                    mainForm.FormClosed += (s, e) =>
                    {
                        if (mainForm.IsLoggingOut)
                        {
                            _usernameInput.Text = "";
                            _passwordInput.Text = "";
                            Show();
                        }
                        else { Close(); }
                    };
                    mainForm.Show();
                    Hide();
                    return;
                }
            }
            MessageBox.Show("Tên đăng nhập hoặc mật khẩu không chính xác!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(420, 660);
            this.Name = "LoginForm";
            this.Text = "Login";
            this.ResumeLayout(false);
        }
    }

    internal static class NativeMethods
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
    }
}
