
using System;
using System.Windows;


namespace Request_Refill.Windows
{
    /// <summary>
    /// Логика взаимодействия для WindowTraySelectFuntion.xaml
    /// </summary>
    public partial class WindowTraySelectFuntion : Window
    {
        private static WindowTraySelectFuntion _currentInstance;
        public WindowTraySelectFuntion()
        {
            

            InitializeComponent();



            this.SourceInitialized += (s, e) =>
            {
                var workingArea = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea;
                this.Left = workingArea.Right - this.Width + 8;
                this.Top = workingArea.Bottom - this.Height + 18;
            };
            this.Closing += (s, e) => _currentInstance = null;

        }
        public new bool? ShowDialog()
        {
            if (_currentInstance != null)
            {
                if (_currentInstance.WindowState == WindowState.Minimized)
                    _currentInstance.WindowState = WindowState.Normal;

                _currentInstance.Activate();
                return false;
            }

            _currentInstance = this;
            return base.ShowDialog();
        }


        private void Window_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e) => Close();
        private void Shutdown_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void OpenSettings_Click(object sender, RoutedEventArgs e) => new WindowSettings().ShowDialog();
        private void OpenCreateRequestRefill_Click(object sender, RoutedEventArgs e) => new WindowCreateRequestRefill().ShowDialog();

        private void OpenReplaceCartridge(object sender, RoutedEventArgs e) => new ReplaceCartridge().ShowDialog();
    }
}
