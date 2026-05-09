using System.Drawing;

namespace QLTV_covert_2._0.Utilities
{
    public static class AppColors
    {
        // Primary Colors
        public static readonly Color PrimaryBlue = Color.FromArgb(41, 128, 185);
        public static readonly Color PrimaryBlueDark = Color.FromArgb(30, 100, 160);
        public static readonly Color PrimaryBlueLite = Color.FromArgb(52, 152, 219);

        // Status Colors
        public static readonly Color SuccessGreen = Color.FromArgb(46, 204, 113);
        public static readonly Color WarningYellow = Color.FromArgb(241, 196, 15);
        public static readonly Color ErrorRed = Color.FromArgb(231, 76, 60);
        public static readonly Color PurpleViolet = Color.FromArgb(155, 89, 182);

        // Background Colors
        public static readonly Color BackgroundLightGray = Color.FromArgb(245, 245, 245);
        public static readonly Color BackgroundWhite = Color.White;

        // Text Colors
        public static readonly Color TextDark = Color.FromArgb(60, 60, 60);
        public static readonly Color TextMedium = Color.FromArgb(80, 80, 80);
        public static readonly Color TextLight = Color.Gray;
    }

    public static class AppConfig
    {
        public const string AppTitle = "📚 Quản Lý Thư Viện Số";
        public const string AppVersion = "2.0";

        // Default window sizes
        public const int LoginFormWidth = 1200;
        public const int LoginFormHeight = 650;

        public const int MainFormWidth = 1400;
        public const int MainFormHeight = 800;

        // Database
        public const string DatabaseName = "library.db";

        // Borrow settings
        public const int DefaultBorrowDays = 7;
        public const int MaxBorrowDays = 30;
    }
}
