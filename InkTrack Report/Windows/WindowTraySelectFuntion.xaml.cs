
using InkTrack_Report.Windows.Dialog;
using System;
using System.Windows;


namespace InkTrack_Report.Windows
{
    public partial class WindowTraySelectFuntion : Window
    {
        public WindowTraySelectFuntion()
        {
            InitializeComponent();

            this.SourceInitialized += (s, e) =>
            {
                var workingArea = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea;
                this.Left = workingArea.Right - this.Width + 8;
                this.Top = workingArea.Bottom - this.Height + 18;
            };
        }
        private void Shutdown_Click(object sender, RoutedEventArgs e)
        {

            if (new AdminAuthDialog().ShowDialog() == true)
            {
                Deactivated -= Window_Deactivated;
                Application.Current.Shutdown();
            }
        }
        private void OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            if (new AdminAuthDialog().ShowDialog() == true)
            {
                new Settings().Show();
            }
        }
        private void OpenCreateRequestRefill_Click(object sender, RoutedEventArgs e) => new CreateRequestRefill().Show();

        private void OpenReplaceCartridge(object sender, RoutedEventArgs e)
        {
            if (new AdminAuthDialog().ShowDialog() == true)
            {
                new ReplaceCartridge().Show();
            }
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            Close();
        }
    }
}
