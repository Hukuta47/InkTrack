using Request_Refill.Database;
using Request_Refill.Classes;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;

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
                ComboboxFromWhoDefaultSelect.ItemsSource = App.dBEntities.GetEmployeesInCabinet(App.programData.idSelectedCabinet);
                ComboboxPrinterDefaultSelect.ItemsSource = App.dBEntities.GetPrintersInCabinet(App.programData.idSelectedCabinet);

                ComboboxCabinetSelect.SelectedIndex = App.programData.idSelectedCabinet;
                ComboboxFromWhoDefaultSelect.SelectedIndex = App.programData.idFromWhoDefaultSelect;
                ComboboxPrinterDefaultSelect.SelectedIndex = App.programData.idPrinterDefaultSelect;
            }
            else
            {
                ComboboxCabinetSelect.ItemsSource = App.dBEntities.GetCabinetsWithPrinters().ToList();
            }


        }
        private void ClickCloseWindow(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ComboboxCabinetSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int SelectedCabinetID = ((GetCabinetsWithPrinters_Result)ComboboxCabinetSelect.SelectedValue).CabinetID;
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
                idSelectedCabinet = ComboboxCabinetSelect.SelectedIndex,
                idFromWhoDefaultSelect = ComboboxFromWhoDefaultSelect.SelectedIndex,
                idPrinterDefaultSelect = ComboboxPrinterDefaultSelect.SelectedIndex,
                CountEmployeesInCabinet = CountEmployees,
                CountPrintersInCabinet = CountPrinters
            };

            string JsonData = JsonConvert.SerializeObject(App.programData, Formatting.Indented);

            File.WriteAllText(App.pathJsonSettingsFile, JsonData);
        }
    }
}
