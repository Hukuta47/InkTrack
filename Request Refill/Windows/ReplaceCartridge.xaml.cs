using Request_Refill.Database;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Request_Refill.Windows
{
    public partial class ReplaceCartridge : Window
    {
        public ReplaceCartridge()
        {
            InitializeComponent();

            int SelectedPrinter = App.programData.SelectedPrinterID;
            Listbox_CauseReplaceCartridge.ItemsSource = App.dBEntities.ReasonForReplacement.ToList();
            Combobox_CartridgeNumber.ItemsSource = App.dBEntities.GetCompatibleCartridgesForPrinter(SelectedPrinter);
            Combobox_WhoReplaced.ItemsSource = App.dBEntities.EmployeeLIT.ToList();
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
            int NewCartridgeID = (Combobox_CartridgeNumber.SelectedItem as GetCompatibleCartridgesForPrinter_Result).CartridgeID;
            int ReasonID = (Listbox_CauseReplaceCartridge.SelectedItem as ReasonForReplacement).ReasonID;
            int EmployeeLITID = (Combobox_WhoReplaced.SelectedItem as EmployeeLIT).EmployeeID;

            App.dBEntities.InstallCartridgeToPrinter(NewCartridgeID, ReasonID, EmployeeLITID);
        }
    }
}
