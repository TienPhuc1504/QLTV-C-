using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Guna.UI2.WinForms;

namespace QLTV_covert_2._0.Utilities
{
    /// <summary>
    /// Centralized design system for the entire application.
    /// Provides a cohesive, modern dark-themed palette with consistent typography,
    /// spacing, and component styling across all forms.
    /// </summary>
    public static class AppTheme
    {
        // ═══════════════════════════════════════════════════════════════
        //  COLOR PALETTE — Deep Blue / Slate / Teal Accent
        // ═══════════════════════════════════════════════════════════════

        // Surface hierarchy (dark → light)
        public static readonly Color Surface0 = Color.FromArgb(15, 23, 42);       // Deepest — sidebar, login bg
        public static readonly Color Surface1 = Color.FromArgb(22, 33, 62);       // Card backgrounds
        public static readonly Color Surface2 = Color.FromArgb(30, 41, 74);       // Elevated cards / inputs
        public static readonly Color Surface3 = Color.FromArgb(39, 52, 88);       // Hover / subtle raised
        public static readonly Color SurfaceOverlay = Color.FromArgb(50, 255, 255, 255); // Frosted glass effect

        // Content panel — lighter for contrast
        public static readonly Color ContentBg = Color.FromArgb(241, 245, 249);   // Main content area
        public static readonly Color CardWhite = Color.FromArgb(255, 255, 255);   // White cards on light bg

        // Primary — Vibrant Teal / Cyan
        public static readonly Color Primary = Color.FromArgb(6, 182, 212);       // Main accent
        public static readonly Color PrimaryHover = Color.FromArgb(8, 145, 178);  // Hover state
        public static readonly Color PrimaryLight = Color.FromArgb(34, 211, 238); // Light variant
        public static readonly Color PrimarySubtle = Color.FromArgb(20, 50, 80);  // Subtle bg tint

        // Secondary — Indigo / Violet
        public static readonly Color Secondary = Color.FromArgb(99, 102, 241);    // Secondary actions
        public static readonly Color SecondaryHover = Color.FromArgb(79, 70, 229);

        // Semantic
        public static readonly Color Success = Color.FromArgb(16, 185, 129);      // Green
        public static readonly Color SuccessLight = Color.FromArgb(20, 83, 60);
        public static readonly Color Warning = Color.FromArgb(245, 158, 11);      // Amber
        public static readonly Color WarningLight = Color.FromArgb(92, 60, 15);
        public static readonly Color Danger = Color.FromArgb(239, 68, 68);        // Red
        public static readonly Color DangerHover = Color.FromArgb(220, 38, 38);
        public static readonly Color DangerLight = Color.FromArgb(80, 30, 30);
        public static readonly Color Info = Color.FromArgb(59, 130, 246);         // Blue

        // Text
        public static readonly Color TextPrimary = Color.FromArgb(248, 250, 252); // Brightest white text
        public static readonly Color TextSecondary = Color.FromArgb(148, 163, 184); // Muted / secondary
        public static readonly Color TextMuted = Color.FromArgb(100, 116, 139);   // Very muted / hints
        public static readonly Color TextDark = Color.FromArgb(15, 23, 42);       // Dark text on light bg
        public static readonly Color TextDarkSecondary = Color.FromArgb(71, 85, 105); // Muted on light bg

        // Borders / Dividers
        public static readonly Color Border = Color.FromArgb(51, 65, 85);         // Subtle dividers
        public static readonly Color BorderLight = Color.FromArgb(226, 232, 240);  // Light-bg borders
        public static readonly Color FocusBorder = Primary;

        // ═══════════════════════════════════════════════════════════════
        //  TYPOGRAPHY
        // ═══════════════════════════════════════════════════════════════

        public static readonly Font DisplayLarge = new Font("Segoe UI", 28F, FontStyle.Bold);
        public static readonly Font DisplayMedium = new Font("Segoe UI", 22F, FontStyle.Bold);
        public static readonly Font HeadingLarge = new Font("Segoe UI", 18F, FontStyle.Bold);
        public static readonly Font HeadingMedium = new Font("Segoe UI", 15F, FontStyle.Bold);
        public static readonly Font HeadingSmall = new Font("Segoe UI", 13F, FontStyle.Bold);
        public static readonly Font BodyLarge = new Font("Segoe UI", 11F);
        public static readonly Font BodyMedium = new Font("Segoe UI", 10F);
        public static readonly Font BodySmall = new Font("Segoe UI", 9F);
        public static readonly Font Caption = new Font("Segoe UI", 8.5F);
        public static readonly Font ButtonFont = new Font("Segoe UI Semibold", 10F);
        public static readonly Font ButtonSmall = new Font("Segoe UI Semibold", 9F);
        public static readonly Font MenuFont = new Font("Segoe UI", 10F);
        public static readonly Font MonoFont = new Font("Cascadia Code", 10F);

        // ═══════════════════════════════════════════════════════════════
        //  SPACING & SIZING
        // ═══════════════════════════════════════════════════════════════

        public const int RadiusSmall = 6;
        public const int RadiusMedium = 10;
        public const int RadiusLarge = 14;
        public const int RadiusXL = 20;

        public const int SidebarWidth = 260;
        public const int SidebarCollapsed = 70;

        public const int CardPadding = 20;
        public const int SectionSpacing = 24;
        public const int ItemSpacing = 12;

        // ═══════════════════════════════════════════════════════════════
        //  COMPONENT FACTORIES
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Creates a primary action button with consistent styling.
        /// </summary>
        public static Guna2Button CreatePrimaryButton(string text, int width = 140, int height = 42)
        {
            var btn = new Guna2Button
            {
                Text = text,
                Width = width,
                Height = height,
                BorderRadius = RadiusSmall,
                FillColor = Primary,
                ForeColor = Color.White,
                Font = ButtonFont,
                Cursor = Cursors.Hand,
                Animated = true
            };
            btn.HoverState.FillColor = PrimaryHover;
            btn.HoverState.ForeColor = Color.White;
            return btn;
        }

        /// <summary>
        /// Creates a secondary / outline button.
        /// </summary>
        public static Guna2Button CreateSecondaryButton(string text, int width = 140, int height = 42)
        {
            var btn = new Guna2Button
            {
                Text = text,
                Width = width,
                Height = height,
                BorderRadius = RadiusSmall,
                BorderThickness = 1,
                BorderColor = Border,
                FillColor = Color.Transparent,
                ForeColor = TextSecondary,
                Font = ButtonFont,
                Cursor = Cursors.Hand,
                Animated = true
            };
            btn.HoverState.FillColor = Surface3;
            btn.HoverState.ForeColor = TextPrimary;
            btn.HoverState.BorderColor = Primary;
            return btn;
        }

        /// <summary>
        /// Creates a colored action button (for toolbars).
        /// </summary>
        public static Guna2Button CreateActionButton(string text, Color color, int width = 120, int height = 36)
        {
            var btn = new Guna2Button
            {
                Text = text,
                Width = width,
                Height = height,
                BorderRadius = RadiusSmall,
                FillColor = color,
                ForeColor = Color.White,
                Font = ButtonSmall,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 8, 0),
                Animated = true
            };
            btn.HoverState.FillColor = ControlPaint.Light(color, 0.15f);
            return btn;
        }

        /// <summary>
        /// Creates a ghost / text-only button for subtle actions.
        /// </summary>
        public static Guna2Button CreateGhostButton(string text, int width = 120, int height = 36)
        {
            var btn = new Guna2Button
            {
                Text = text,
                Width = width,
                Height = height,
                BorderRadius = RadiusSmall,
                FillColor = Color.Transparent,
                ForeColor = TextSecondary,
                Font = ButtonSmall,
                Cursor = Cursors.Hand,
                Animated = true
            };
            btn.HoverState.FillColor = Surface2;
            btn.HoverState.ForeColor = Primary;
            return btn;
        }

        /// <summary>
        /// Creates a styled text input for dark backgrounds.
        /// </summary>
        public static Guna2TextBox CreateDarkInput(string placeholder, bool isPassword = false)
        {
            var input = new Guna2TextBox
            {
                PlaceholderText = placeholder,
                Height = 48,
                BorderRadius = RadiusSmall,
                BorderThickness = 1,
                BorderColor = Border,
                FillColor = Surface2,
                Font = BodyMedium,
                ForeColor = TextPrimary,
                PlaceholderForeColor = TextMuted,
                PasswordChar = isPassword ? '●' : '\0',
                Cursor = Cursors.IBeam,
                FocusedState = { BorderColor = Primary }
            };
            return input;
        }

        /// <summary>
        /// Creates a styled text input for light backgrounds.
        /// </summary>
        public static Guna2TextBox CreateLightInput(string placeholder, bool isPassword = false)
        {
            var input = new Guna2TextBox
            {
                PlaceholderText = placeholder,
                Height = 44,
                BorderRadius = RadiusSmall,
                BorderThickness = 1,
                BorderColor = BorderLight,
                FillColor = CardWhite,
                Font = BodyMedium,
                ForeColor = TextDark,
                PlaceholderForeColor = TextDarkSecondary,
                PasswordChar = isPassword ? '●' : '\0',
                Cursor = Cursors.IBeam,
                FocusedState = { BorderColor = Primary }
            };
            return input;
        }

        /// <summary>
        /// Applies premium DataGridView theming for light backgrounds.
        /// </summary>
        public static void StyleGrid(Guna2DataGridView grid)
        {
            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AllowUserToResizeRows = false;                              // ← Disable row resize
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing; // ← Fixed header height
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;
            grid.RowHeadersVisible = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.BorderStyle = BorderStyle.None;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.BackgroundColor = CardWhite;
            grid.GridColor = Color.FromArgb(241, 245, 249);
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersHeight = 44;
            grid.RowTemplate.Height = 40;

            // Header — disable active/highlighted state on column headers
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = TextDarkSecondary;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9F);
            grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(12, 0, 12, 0);
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(248, 250, 252);  // Same as normal = no highlight
            grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = TextDarkSecondary;

            // Rows
            grid.DefaultCellStyle.BackColor = CardWhite;
            grid.DefaultCellStyle.ForeColor = TextDark;
            grid.DefaultCellStyle.Font = BodySmall;
            grid.DefaultCellStyle.Padding = new Padding(12, 0, 12, 0);
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(224, 242, 254);
            grid.DefaultCellStyle.SelectionForeColor = TextDark;

            // Alternating rows
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            grid.AlternatingRowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(224, 242, 254);
        }

        /// <summary>
        /// Exports a DataGridView's content to a beautifully formatted Excel file using COM late binding.
        /// </summary>
        public static void ExportGridToExcel(DataGridView grid, string defaultName = "export")
        {
            if (grid.Rows.Count == 0) { MessageBox.Show("Không có dữ liệu để xuất.", "Thông báo"); return; }

            try
            {
                // Use late binding to avoid adding COM references
                System.Type excelType = System.Type.GetTypeFromProgID("Excel.Application");
                if (excelType == null)
                {
                    MessageBox.Show("Không tìm thấy Microsoft Excel trên máy tính này. Vui lòng cài đặt Excel để sử dụng chức năng xuất trực quan.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                dynamic excelApp = System.Activator.CreateInstance(excelType);
                excelApp.Visible = false;
                excelApp.DisplayAlerts = false;
                dynamic workbook = excelApp.Workbooks.Add();
                dynamic worksheet = workbook.Worksheets[1];
                worksheet.Name = "Dữ liệu xuất";

                // Title row
                worksheet.Cells[1, 1] = "BÁO CÁO THỐNG KÊ";
                worksheet.Cells[1, 1].Font.Size = 16;
                worksheet.Cells[1, 1].Font.Bold = true;
                worksheet.Cells[1, 1].Font.Color = System.Drawing.ColorTranslator.ToOle(Color.Black);

                int colIndex = 1;
                // Headers
                foreach (DataGridViewColumn col in grid.Columns)
                {
                    if (col.Visible)
                    {
                        var cell = worksheet.Cells[3, colIndex];
                        cell.Value = col.HeaderText;
                        // Format header
                        cell.Interior.Color = System.Drawing.ColorTranslator.ToOle(Color.LightGray); // Light Gray background for contrast
                        cell.Font.Color = System.Drawing.ColorTranslator.ToOle(Color.Black);
                        cell.Font.Bold = true;
                        cell.Borders.LineStyle = 1; // xlContinuous
                        colIndex++;
                    }
                }

                // Data Rows
                int rowIndex = 4;
                foreach (DataGridViewRow row in grid.Rows)
                {
                    colIndex = 1;
                    foreach (DataGridViewColumn col in grid.Columns)
                    {
                        if (col.Visible)
                        {
                            var cell = worksheet.Cells[rowIndex, colIndex];
                            // Prefix string to prevent scientific notation/date conversion issues for text
                            var cellValue = row.Cells[col.Index].Value?.ToString();
                            if (cellValue != null && (cellValue.StartsWith("0") || cellValue.Contains("/")))
                            {
                                cell.NumberFormat = "@"; // Text format
                            }
                            cell.Value = cellValue;
                            cell.Font.Color = System.Drawing.ColorTranslator.ToOle(Color.Black);
                            cell.Borders.LineStyle = 1; // xlContinuous
                            
                            // Alternate row color
                            if (rowIndex % 2 == 1)
                            {
                                cell.Interior.Color = System.Drawing.ColorTranslator.ToOle(Color.FromArgb(248, 250, 252));
                            }
                            
                            colIndex++;
                        }
                    }
                    rowIndex++;
                }

                // AutoFit columns
                worksheet.Columns.AutoFit();

                using (var sfd = new SaveFileDialog())
                {
                    sfd.Filter = "Excel Workbook (*.xlsx)|*.xlsx|Excel 97-2003 Workbook (*.xls)|*.xls";
                    sfd.FileName = defaultName + "_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx";
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        // 51 is the enumeration value for xlOpenXMLWorkbook (.xlsx)
                        // 56 is xlExcel8 (.xls)
                        int fileFormat = sfd.FileName.EndsWith(".xlsx") ? 51 : 56;
                        workbook.SaveAs(sfd.FileName, fileFormat);
                        MessageBox.Show("Xuất dữ liệu thành công!\n" + sfd.FileName, "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        
                        // Ask user if they want to open the file
                        if (MessageBox.Show("Bạn có muốn mở file vừa xuất không?", "Mở file", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            // Thay vì Process.Start (gây xung đột COM), ta cho hiển thị luôn Excel đang chạy ngầm
                            excelApp.Visible = true;
                            
                            // Chỉ giải phóng object, KHÔNG đóng workbook hay quit ứng dụng để người dùng sử dụng
                            if (worksheet != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(worksheet);
                            if (workbook != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(workbook);
                            if (excelApp != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(excelApp);
                            return;
                        }
                    }
                }

                // Nếu người dùng chọn "No" hoặc Hủy lưu file
                workbook.Close(false);
                excelApp.Quit();
                if (worksheet != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(worksheet);
                if (workbook != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(workbook);
                if (excelApp != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(excelApp);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Lỗi khi xuất dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Creates a metric/stat card for dashboard with gradient background.
        /// </summary>
        public static Guna2Panel CreateStatCard(string value, string label, Color accentColor, int width = 0)
        {
            var card = new Guna2Panel
            {
                Dock = width > 0 ? DockStyle.None : DockStyle.Fill,
                Width = width > 0 ? width : 0,
                Height = 110,
                Margin = new Padding(0, 0, 16, 0),
                BorderRadius = RadiusMedium,
                FillColor = CardWhite,
                BorderThickness = 0,
                ShadowDecoration = { Enabled = true, Shadow = new Padding(0, 2, 0, 4), Color = Color.FromArgb(20, 0, 0, 0) }
            };

            // Accent bar at top
            var accentBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 4,
                BackColor = accentColor
            };
            card.Controls.Add(accentBar);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(20, 14, 20, 14)
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 55));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 45));
            card.Controls.Add(layout);
            layout.BringToFront();

            var valueLabel = new Label
            {
                Text = value,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.BottomLeft,
                Font = new Font("Segoe UI", 24F, FontStyle.Bold),
                ForeColor = TextDark,
                BackColor = Color.Transparent
            };
            layout.Controls.Add(valueLabel, 0, 0);

            var captionLabel = new Label
            {
                Text = label,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.TopLeft,
                Font = new Font("Segoe UI", 10F),
                ForeColor = TextDarkSecondary,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 4, 0, 0)
            };
            layout.Controls.Add(captionLabel, 0, 1);

            return card;
        }
    }
}
