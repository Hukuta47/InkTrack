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

        public Settings()

        {
            InitializeComponent();

            Combobox_SelectCabinet.ItemsSource = App.entities.Cabinet.Where(c => c.Device.Any(d => d.DeviceTypeID == 2)).ToList();
            Combobox_SelectCabinet.SelectionChanged += ComboboxCabinetSelect_SelectionChanged;


            Combobox_SelectCabinet.SelectedValue = SelectedCabinetID;
            Combobox_SelectEmployee.SelectedValue = SelectedEmployeeID;
            Combobox_SelectPrinter.SelectedValue = SelectedPrinterID;
        }

        private void ComboboxCabinetSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Combobox_SelectEmployee.IsEnabled = (Combobox_SelectCabinet.SelectedItem as Cabinet).Employee.Count > 1 ? true : false;
            Combobox_SelectEmployee.ItemsSource = (Combobox_SelectCabinet.SelectedItem as Cabinet).Employee;
            Combobox_SelectPrinter.IsEnabled = (Combobox_SelectCabinet.SelectedItem as Cabinet).Device.Count > 1 ? true : false;
            Combobox_SelectPrinter.ItemsSource = (Combobox_SelectCabinet.SelectedItem as Cabinet).Device.Where(d => d.DeviceTypeID == 2);

            Combobox_SelectEmployee.SelectedIndex = 0;
            Combobox_SelectPrinter.SelectedIndex = 0;
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
            Properties.Settings.Default.SelectedEmployeeID = (int)Combobox_SelectEmployee.SelectedValue;
            Properties.Settings.Default.SelectedPrinterID = (int)Combobox_SelectPrinter.SelectedValue;

            Properties.Settings.Default.Save();
        }
    }
}
