using Request_Refill.Database;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Request_Refill.Windows;


namespace Request_Refill.SetupWizardSettingsPages
{
    public partial class SetupWizardSettings_Page1 : Page
    {
        public int CountEmployees;
        public int CountPrinters;
        public SetupWizardSettings_Page1()
        {
            InitializeComponent();
            Combobox_SelectCabinet.ItemsSource = App.dBEntities.GetCabinetsWithPrinters().ToList();
        }

        private void VaribleButton_Click(object sender, RoutedEventArgs e)
        {
            if (CountEmployees == 1 && CountPrinters == 1)
            {
                SetupWizardSettings.CountEmployees = 1;
                SetupWizardSettings.CountPrinters = 1;
            }
            else
            {
                SetupWizardSettings.navigationClass.nextPage();
            }
        }

        private void Combobox_SelectCabinet_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int SelectedCabinetID = (int)Combobox_SelectCabinet.SelectedValue;

            CountEmployees = App.dBEntities.GetEmployeesInCabinet(SelectedCabinetID).Count();
            CountPrinters = App.dBEntities.GetPrintersInCabinet(SelectedCabinetID).Count();

            if (CountEmployees == 1 && CountPrinters == 1)
            {

            }
            else
            {

            }
        }
    }
}
