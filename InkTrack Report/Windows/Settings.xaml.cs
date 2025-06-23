using InkTrack_Report.Database;
using System.Collections.Generic;
using System.Linq;
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

        List<Cabinet> cabinetsWithPrinters = new List<Cabinet>();
        List<Employee> employeeInCabinet = new List<Employee>();
        List<Printer> printersInCabinet = new List<Printer>();
        public Settings()
        {
            InitializeComponent();
            InitData();
        }
        private void ComboboxCabinetSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          

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
            Properties.Settings.Default.SelectedCabinetID = (int)Combobox_SelectCabinet.SelectedValue;
            Properties.Settings.Default.SelectedEmployeeID = (int)ComboboxFromWhoDefaultSelect.SelectedValue;
            Properties.Settings.Default.SelectedPrinterID = (int)ComboboxPrinterDefaultSelect.SelectedValue;

            Properties.Settings.Default.Save();
        }

        void InitData()
        {
            UpdateData();
            Combobox_SelectCabinet.ItemsSource = cabinetsWithPrinters;
        }
        void UpdateData()
        {
            List<Cabinet> CabinetHaveDevice = new List<Cabinet>();
            foreach (var cabinet in App.entities.Cabinet.Where(c => c.Device.Contains<Device>(new Device().DeviceTypeID = 1)))
            {
                
            }
        }
    }
}
