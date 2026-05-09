using System;
using System.Windows.Forms;

namespace QLTV_covert_2._0.Utilities
{
    public static class FormHelper
    {
        public static void CenterFormOnScreen(Form form)
        {
            form.StartPosition = FormStartPosition.CenterScreen;
        }

        public static void MaximizeForm(Form form)
        {
            form.WindowState = FormWindowState.Maximized;
        }

        public static void ShowSuccessMessage(string message, string title = "Thành công")
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void ShowErrorMessage(string message, string title = "Lỗi")
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static void ShowWarningMessage(string message, string title = "Cảnh báo")
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static DialogResult ShowConfirmDialog(string message, string title = "Xác nhận")
        {
            return MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        }
    }

    public static class DateTimeHelper
    {
        public static string FormatDate(string date)
        {
            if (DateTime.TryParse(date, out DateTime dt))
            {
                return dt.ToString("dd/MM/yyyy");
            }
            return date;
        }

        public static bool IsOverdue(string dueDate)
        {
            if (DateTime.TryParse(dueDate, out DateTime dt))
            {
                return DateTime.Today > dt;
            }
            return false;
        }

        public static int GetDaysOverdue(string dueDate)
        {
            if (DateTime.TryParse(dueDate, out DateTime dt))
            {
                TimeSpan span = DateTime.Today - dt;
                return span.Days;
            }
            return 0;
        }
    }
}
