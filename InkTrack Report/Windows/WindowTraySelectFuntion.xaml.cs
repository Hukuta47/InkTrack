using InkTrack_Report.Classes;
using InkTrack_Report.Windows.Dialog;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;


namespace InkTrack_Report.Windows
{

    public partial class WindowTraySelectFuntion : Window
    {
        int SumPagesPrintouts;
        public WindowTraySelectFuntion(bool ServiceOn)
        {
            InitializeComponent();

            this.SourceInitialized += (s, e) =>
            {
                // Получаем DPI окна
                var dpi = VisualTreeHelper.GetDpi(this);

                // Рабочая область экрана в WPF-единицах
                Rect workArea = SystemParameters.WorkArea;

                // Конвертируем физические отступы в WPF-единицы
                double offsetX = dpi.DpiScaleX;
                double offsetY = dpi.DpiScaleY;

                // Позиционируем окно с учётом масштабирования
                this.Left = workArea.Right - this.ActualWidth;
                this.Top = workArea.Bottom - this.ActualHeight;
            };
            if (ServiceOn)
            {
                Button_Service.Visibility = Visibility.Collapsed;
                ServiceButtons.Visibility = Visibility.Visible;
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var fadeIn = new DoubleAnimation
            {
                From = 0.0,
                To = 1.0,
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut },
                Duration = new Duration(TimeSpan.FromSeconds(1)) // Можно изменить время
            };
            this.BeginAnimation(OpacityProperty, fadeIn);
        }
        private void Shutdown_Click(object sender, RoutedEventArgs e)
        {
            Deactivated -= Window_Deactivated;
            Application.Current.Shutdown();
        }
        private void OpenReplaceCartridge(object sender, RoutedEventArgs e) => new ReplaceCartridge().Show();
        private void Window_Deactivated(object sender, EventArgs e)
        {
            Close();
        }
        private void OpenServiceButtons_Click(object sender, EventArgs e)
        {
            if (new AdminAuthDialog().ShowDialog() == true)
            {
                new WindowTraySelectFuntion(true).Show();
            }
        }
    }
}
