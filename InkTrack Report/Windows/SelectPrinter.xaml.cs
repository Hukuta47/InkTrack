using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace InkTrack_Report.Windows
{
    public partial class SelectPrinter : Window
    {
        public SelectPrinter()
        {
            InitializeComponent();
            Combobox_SelectPrinter.ItemsSource = App.entities.Cabinet.First(c => c.CabinetID == Properties.Settings.Default.SelectedCabinetID).Device.Where(d => d.DeviceTypeID == 2);
        }
        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.SelectedPrinterID = (int)Combobox_SelectPrinter.SelectedValue;

            Properties.Settings.Default.Save();
            DialogResult = true;
        }
        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            switch (MessageBox.Show(
                "Уверены что хотите прервать настройку? " +
                "\nПри следующем запуске программы, вам в любом случае придется провести эту процедуру", "Предупреждение", MessageBoxButton.YesNo, MessageBoxImage.Question))
            {
                case MessageBoxResult.Yes:
                    App.Current.Shutdown();
                    break;
                case MessageBoxResult.No:
                    return;
            }
        }
        private void PanelDrag_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed) DragMove();
        }

    }
}
