using InkTrack_Report.Database;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.Generic;

namespace InkTrack_Report.Windows
{
    public partial class Settings : Window
    {
        int SelectedCabinetID = Properties.Settings.Default.SelectedCabinetID;
        int SelectedEmployeeID = Properties.Settings.Default.SelectedEmployeeID;
        int SelectedPrinterID = Properties.Settings.Default.SelectedPrinterID;

        int CountEmployees;
        int CountPrinters;

        List<Cabinet> cabinetsWithPrinters;
        List<Employee> employeeInCabinet;
        List<Printer> printersInCabinet;
        public Settings()
        {
            InitializeComponent();
            UpdateData();

            ComboboxCabinetSelect.SelectedValue = SelectedCabinetID;
            ComboboxFromWhoDefaultSelect.SelectedValue = SelectedEmployeeID;
            ComboboxPrinterDefaultSelect.SelectedValue = SelectedPrinterID;

            ComboboxCabinetSelect.SelectionChanged += ComboboxCabinetSelect_SelectionChanged;

            
        }
        private void ComboboxCabinetSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateData();

            int SelectedCabinetID = ((Cabinet)ComboboxCabinetSelect.SelectedItem).CabinetID;
            CountEmployees = employeeInCabinet.Count();
            CountPrinters = printersInCabinet.Count();

            ComboboxFromWhoDefaultSelect.ItemsSource = employeeInCabinet;
            ComboboxFromWhoDefaultSelect.SelectedIndex = 0;
            ComboboxFromWhoDefaultSelect.IsEnabled = CountEmployees != 1;

            ComboboxPrinterDefaultSelect.ItemsSource = printersInCabinet;
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
        void UpdateData()
        {
            List<Cabinet> CabinetHaveDevice = App.entities.Cabinet.Where(c => c.Device != null).ToList();
            foreach (var cabinet in CabinetHaveDevice)
            {
                List<Device> PrintersList = cabinet.Device.Where(d => d.DeviceTypeID == 2).ToList();
                if (PrintersList != null)
                {
                    cabinetsWithPrinters.Add(cabinet);
                }
            }

            ComboboxCabinetSelect.ItemsSource = cabinetsWithPrinters;
            foreach (var employee in (ComboboxCabinetSelect.SelectedItem as Cabinet).Employee)
            {
                employeeInCabinet.Add(employee);
            }
            foreach (var printer in (ComboboxCabinetSelect.SelectedItem as Cabinet).Device.Where(d => d.DeviceTypeID == 2))
            {
                printersInCabinet.Add(printer.Printer);
            }

            ComboboxFromWhoDefaultSelect.ItemsSource = employeeInCabinet;
            ComboboxPrinterDefaultSelect.ItemsSource = printersInCabinet;
        }
    }
}
