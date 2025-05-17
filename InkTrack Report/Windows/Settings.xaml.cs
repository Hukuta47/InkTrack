using Newtonsoft.Json;
using InkTrack_Report.Classes;
using InkTrack_Report.Database;
using System.Linq;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace InkTrack_Report.Windows
{
    public partial class Settings : Window
    {
        int SelectedCabinetID = Properties.Settings.Default.SelectedCabinetID;
        int SelectedEmployeeID = Properties.Settings.Default.SelectedEmployeeID;
        int SelectedPrinterID = Properties.Settings.Default.SelectedPrinterID;


        int CountEmployees;
        int CountPrinters;
        public Settings()
        {
            InitializeComponent();

            ComboboxCabinetSelect.ItemsSource = App.dBEntities.GetCabinetsWithPrinters().ToList();
            ComboboxFromWhoDefaultSelect.ItemsSource = App.dBEntities.GetEmployeesInCabinet(SelectedEmployeeID);
            ComboboxPrinterDefaultSelect.ItemsSource = App.dBEntities.GetPrintersInCabinet(SelectedPrinterID);


            ComboboxCabinetSelect.SelectedValue = SelectedCabinetID;
            ComboboxFromWhoDefaultSelect.SelectedValue = SelectedEmployeeID;
            ComboboxPrinterDefaultSelect.SelectedValue = SelectedPrinterID;

            ComboboxCabinetSelect.SelectionChanged += ComboboxCabinetSelect_SelectionChanged;

            
        }
        private void ComboboxCabinetSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int SelectedCabinetID = ((GetCabinetsWithPrinters_Result)ComboboxCabinetSelect.SelectedItem).CabinetID;
            CountEmployees = App.dBEntities.GetEmployeesInCabinet(SelectedCabinetID).Count();
            CountPrinters = App.dBEntities.GetPrintersInCabinet(SelectedCabinetID).Count();

            ComboboxFromWhoDefaultSelect.ItemsSource = App.dBEntities.GetEmployeesInCabinet(SelectedCabinetID);
            ComboboxFromWhoDefaultSelect.SelectedIndex = 0;
            ComboboxFromWhoDefaultSelect.IsEnabled = CountEmployees != 1;

            ComboboxPrinterDefaultSelect.ItemsSource = App.dBEntities.GetPrintersInCabinet(SelectedCabinetID);
            ComboboxPrinterDefaultSelect.SelectedIndex = 0;
            ComboboxPrinterDefaultSelect.IsEnabled = CountPrinters != 1;
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
            Properties.Settings.Default.SelectedCabinetID = (int)ComboboxCabinetSelect.SelectedValue;
            Properties.Settings.Default.SelectedEmployeeID = (int)ComboboxFromWhoDefaultSelect.SelectedValue;
            Properties.Settings.Default.SelectedPrinterID = (int)ComboboxPrinterDefaultSelect.SelectedValue;

            Properties.Settings.Default.Save();
        }
    }
}
