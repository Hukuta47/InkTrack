using Request_Refill.Classes;
using Request_Refill.SetupWizardSettingsPages;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Request_Refill.Windows
{
    public partial class SetupWizardSettings : Window
    {
        static public int CountEmployees;
        static public int CountPrinters;
        static public int SelectedCabinetID;

        static public NavigationClass navigationClass;

        static public int numPage = 0;

        public SetupWizardSettings()
        {
            InitializeComponent();
            navigationClass = new NavigationClass(Frame_SetupWizardSettings, this);
        }
        private void PanelDrag_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed) DragMove();
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
        public static ProgramData GetResult()
        {
            int selectedEmployeeID = App.dBEntities.GetEmployeesInCabinet(SelectedCabinetID).ToList()[0].EmployeeID;
            int selectedPrinterID = App.dBEntities.GetPrintersInCabinet(SelectedCabinetID).ToList()[0].PrinterID;

            return new ProgramData() { SelectedCabinetID = SelectedCabinetID, SelectedEmployeeID = selectedEmployeeID, SelectedPrinterID = SelectedCabinetID};
        }
    }
}
