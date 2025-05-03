using Request_Refill.Database;
using Request_Refill.Classes;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;
using System.IO;

namespace Request_Refill.Windows
{
    public partial class WindowSettings : Window
    {
        int CountEmployees;
        int CountPrinters;


        public WindowSettings()
        {
            InitializeComponent();
            

            if (App.programData != null)
            {
                ComboboxCabinetSelect.ItemsSource = App.dBEntities.GetCabinetsWithPrinters().ToList();
                ComboboxFromWhoDefaultSelect.ItemsSource = App.dBEntities.GetEmployeesInCabinet(App.programData.SelectedCabinetID);
                ComboboxPrinterDefaultSelect.ItemsSource = App.dBEntities.GetPrintersInCabinet(App.programData.SelectedCabinetID);

                ComboboxCabinetSelect.SelectedValue = App.programData.SelectedCabinetID;
                ComboboxFromWhoDefaultSelect.SelectedValue = App.programData.SelectedEmployeeID;
                ComboboxPrinterDefaultSelect.SelectedValue = App.programData.SelectedPrinterID;
            }
            else
            {
                ComboboxCabinetSelect.ItemsSource = App.dBEntities.GetCabinetsWithPrinters().ToList();
            }



            ComboboxCabinetSelect.SelectionChanged += ComboboxCabinetSelect_SelectionChanged;
        }
        private void ClickCloseWindow(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ComboboxCabinetSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int SelectedCabinetID = ((GetCabinetsWithPrinters_Result)ComboboxCabinetSelect.SelectedItem).CabinetID;
            CountEmployees = App.dBEntities.GetEmployeesInCabinet(SelectedCabinetID).Count();
            CountPrinters = App.dBEntities.GetPrintersInCabinet(SelectedCabinetID).Count();

            ComboboxFromWhoDefaultSelect.ItemsSource = App.dBEntities.GetEmployeesInCabinet(SelectedCabinetID);
            ComboboxFromWhoDefaultSelect.SelectedIndex = 0;
            ComboboxFromWhoDefaultSelect.IsEnabled = CountEmployees == 1 ? false : true;

            ComboboxPrinterDefaultSelect.ItemsSource = App.dBEntities.GetPrintersInCabinet(SelectedCabinetID);
            ComboboxPrinterDefaultSelect.SelectedIndex = 0;
            ComboboxPrinterDefaultSelect.IsEnabled = CountPrinters == 1 ? false : true;
        }

        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {

            App.programData = new ProgramData()
            {
                SelectedCabinetID = (ComboboxCabinetSelect.SelectedItem as GetCabinetsWithPrinters_Result).CabinetID,
                SelectedEmployeeID = (ComboboxFromWhoDefaultSelect.SelectedItem as GetEmployeesInCabinet_Result).EmployeeID,
                SelectedPrinterID = (ComboboxPrinterDefaultSelect.SelectedItem as GetPrintersInCabinet_Result).PrinterID,
            };

            string JsonData = JsonConvert.SerializeObject(App.programData, Formatting.Indented);

            File.WriteAllText(App.pathJsonSettingsFile, JsonData);
        }
    }
}
