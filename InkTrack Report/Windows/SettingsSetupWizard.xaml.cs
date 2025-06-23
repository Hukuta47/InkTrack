using InkTrack_Report.Database;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace InkTrack_Report.Windows
{
    public partial class SettingsSetupWizard : Window
    {
        List<Cabinet> cabinetsWithPrinters;
        List<Employee> employeeInCabinet;
        List<Printer> printersInCabinet;
        public SettingsSetupWizard()
        {
            InitializeComponent();
            UpdateData();

            Combobox_SelectCabinet.ItemsSource = cabinetsWithPrinters;
            Combobox_SelectEmployee.ItemsSource = employeeInCabinet;
            Combobox_SelectPrinter.ItemsSource = printersInCabinet;

            Combobox_SelectEmployee.SelectedIndex = 0; 
            Combobox_SelectPrinter.SelectedIndex = 0; 


        }
        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.SelectedCabinetID = (int)Combobox_SelectCabinet.SelectedValue;
            Properties.Settings.Default.SelectedEmployeeID = (int)Combobox_SelectEmployee.SelectedValue;
            Properties.Settings.Default.SelectedPrinterID = (int)Combobox_SelectPrinter.SelectedValue;
            Properties.Settings.Default.isFirstStartup = false;

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
                    DialogResult = false;
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

        private void Combobox_SelectCabinet_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            Combobox_SelectEmployee.ItemsSource = employeeInCabinet;
            Combobox_SelectPrinter.ItemsSource = printersInCabinet;

            Combobox_SelectEmployee.SelectedIndex = 0;
            Combobox_SelectPrinter.SelectedIndex = 0;
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

            Combobox_SelectCabinet.ItemsSource = cabinetsWithPrinters;
            foreach (var employee in (Combobox_SelectCabinet.SelectedItem as Cabinet).Employee)
            {
                employeeInCabinet.Add(employee);
            }
            foreach (var printer in (Combobox_SelectCabinet.SelectedItem as Cabinet).Device.Where(d => d.DeviceTypeID == 2))
            {
                printersInCabinet.Add(printer.Printer);
            }

            Combobox_SelectEmployee.ItemsSource = employeeInCabinet;
            Combobox_SelectPrinter.ItemsSource = printersInCabinet;
        }
    }
}
