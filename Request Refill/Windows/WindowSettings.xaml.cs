using Request_Refill.Database;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Request_Refill.Windows
{
    public partial class WindowSettings : Window
    {
        public WindowSettings()
        {
            InitializeComponent();
            ComboboxCabinetSelect.ItemsSource = App.dBEntities.GetCabinetsWithPrinters().ToList();
        }
        private void ClickCloseWindow(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ComboboxCabinetSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int SelectedCabinetID = ((GetCabinetsWithPrinters_Result)ComboboxCabinetSelect.SelectedValue).CabinetID;
            int CountEmployees = App.dBEntities.GetEmployeesInCabinet(SelectedCabinetID).Count();
            int CountPrinters = App.dBEntities.GetPrintersInCabinet(SelectedCabinetID).Count();

            ComboboxFromWhoDefaultSelect.ItemsSource = App.dBEntities.GetEmployeesInCabinet(SelectedCabinetID);
            ComboboxFromWhoDefaultSelect.SelectedIndex = 0;
            ComboboxFromWhoDefaultSelect.IsEnabled = CountEmployees == 1 ? false : true;

            ComboboxPrinterDefaultSelect.ItemsSource = App.dBEntities.GetPrintersInCabinet(SelectedCabinetID);
            ComboboxPrinterDefaultSelect.SelectedIndex = 0;
            ComboboxPrinterDefaultSelect.IsEnabled = CountPrinters == 1 ? false : true;
        }
    }
}
