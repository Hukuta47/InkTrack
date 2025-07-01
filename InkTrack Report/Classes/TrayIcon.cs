using System.Drawing;
using System.Windows.Forms;

namespace InkTrack_Report.Classes
{
    public class TrayIcon
    {
        public NotifyIcon NotifyIcon { get; private set; }
        StatusIcon statusIcon = StatusIcon.Idle;

        public enum StatusIcon { Idle, Alert, CancelByUser, DataError, Load, Save, Write }
        public TrayIcon()
        {
            NotifyIcon = new NotifyIcon
            {
                Visible = true,
            };
            ChangeIcon(statusIcon);
        }
        ~TrayIcon()
        {
            NotifyIcon.Dispose();
        }
        private System.Timers.Timer _timer;

        public void ChangeIconOnTime(StatusIcon statusIcon, string text, int milliseconds)
        {
            Icon storedIcon = NotifyIcon.Icon;
            string storedText = NotifyIcon.Text;

            ChangeIcon(statusIcon, text);

            _timer = new System.Timers.Timer(milliseconds);
            _timer.Elapsed += (sender, e) =>
            {
                NotifyIcon.Icon = storedIcon;
                NotifyIcon.Text = storedText;

                _timer.Stop();
                _timer.Dispose();
            };
            _timer.AutoReset = false;
            _timer.Start();
        }
        public void ChangeIcon(StatusIcon statusIcon, string text = "Ink Track Report")
        {
            bool isLightTheme = ThemeDetector.GetWindowsTheme() == AppTheme.Light;
            string suffix = isLightTheme ? "_B" : "_W";
            string resourceName = statusIcon.ToString() + suffix;

            var icon = (System.Drawing.Icon)Properties.Resources.ResourceManager.GetObject(resourceName);
            if (icon != null)
            {
                NotifyIcon.Icon = icon;
                NotifyIcon.Text = text;

            }
        }
    }
}
