
using InkTrack_Report.Classes;
using InkTrack_Report.Database;
using InkTrack_Report.Windows.Dialog;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;


namespace InkTrack_Report.Windows
{

    public partial class WindowTraySelectFuntion : Window
    {
        int SumPagesPrintouts;
        public WindowTraySelectFuntion(bool ServiceOn)
        {
            InitializeComponent();
            SumPagesPrintouts = 0;
            foreach (PrintoutData printoutData in App.printoutDatas)
            {
                SumPagesPrintouts += printoutData.CountPages;
            }
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
            TextBlock_CounterPages.Text = $"Страниц напечатано: {SumPagesPrintouts}";
        }
        private void Shutdown_Click(object sender, RoutedEventArgs e)
        {
            Deactivated -= Window_Deactivated;
            Application.Current.Shutdown();
        }
        private void OpenSettings_Click(object sender, RoutedEventArgs e) => new Settings().Show();
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
