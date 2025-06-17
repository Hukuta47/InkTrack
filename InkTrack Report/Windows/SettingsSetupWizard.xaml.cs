using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace InkTrack_Report.Windows
{
    public partial class SettingsSetupWizard : Window
    {
        public SettingsSetupWizard()
        {
            InitializeComponent();

            Combobox_SelectCabinet.ItemsSource = App.entities.GetCabinetsWithPrinters().ToList();
            Combobox_SelectEmployee.ItemsSource = App.entities.GetEmployeesInCabinet((int)Combobox_SelectCabinet.SelectedValue).ToList();
            Combobox_SelectPrinter.ItemsSource = App.entities.GetPrintersInCabinet((int)Combobox_SelectCabinet.SelectedValue).ToList();

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
            Combobox_SelectEmployee.ItemsSource = App.entities.GetEmployeesInCabinet((int)Combobox_SelectCabinet.SelectedValue).ToList();
            Combobox_SelectPrinter.ItemsSource = App.entities.GetPrintersInCabinet((int)Combobox_SelectCabinet.SelectedValue).ToList();

            Combobox_SelectEmployee.SelectedIndex = 0;
            Combobox_SelectPrinter.SelectedIndex = 0;
        }
    }
}
