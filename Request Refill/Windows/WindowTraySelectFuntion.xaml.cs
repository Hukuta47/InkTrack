
using System;
using System.Windows;


namespace Request_Refill.Windows
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
            Deactivated -= Window_Deactivated;
            Application.Current.Shutdown();
        }
        private void OpenSettings_Click(object sender, RoutedEventArgs e) => new Settings().Show();
        private void OpenCreateRequestRefill_Click(object sender, RoutedEventArgs e) => new CreateRequestRefill().Show();

        private void OpenReplaceCartridge(object sender, RoutedEventArgs e) => new ReplaceCartridge().Show();


        private void Window_Deactivated(object sender, EventArgs e)
        {
            
            Close();
        }
    }
}
