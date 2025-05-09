using Newtonsoft.Json;
using Request_Refill.Classes;
using Request_Refill.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Request_Refill.Windows
{
    /// <summary>
    /// Логика взаимодействия для Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        int CountEmployees;
        int CountPrinters;
        public Settings()
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



        private void PanelDrag_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed) DragMove();
        }
        private void CloseWindow_Click() => Close();
        private void CloseWindow_Click(object sender, RoutedEventArgs e) => Close();
        private void Cancel_Click(object sender, RoutedEventArgs e) => CloseWindow_Click();

        private void Save_Click(object sender, RoutedEventArgs e)
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
