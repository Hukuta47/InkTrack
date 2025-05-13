using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Request_Refill.Windows;


namespace Request_Refill.SetupWizardSettingsPages
{
    /// <summary>
    /// Логика взаимодействия для FirstSetupSettings_Page2.xaml
    /// </summary>
    public partial class SetupWizardSettings_Page2 : Page
    {
        public int SelectedEmployeeID;
        public SetupWizardSettings_Page2()
        {
            InitializeComponent();
        }
        private void Combobox_SelectEmployee_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
