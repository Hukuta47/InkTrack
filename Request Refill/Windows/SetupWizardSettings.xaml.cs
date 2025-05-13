using Request_Refill.Classes;
using Request_Refill.SetupWizardSettingsPages;
using System.Windows;
using System.Windows.Input;

namespace Request_Refill.Windows
{
    public partial class SetupWizardSettings : Window
    {
        static public int CountEmployees;
        static public int CountPrinters;

        SetupWizardSettings_Page1 PageSelectCabinet;
        SetupWizardSettings_Page2 PageSelectEmployee;
        SetupWizardSettings_Page3 PageSelectPrinter;

        ProgramData ProgramData = new ProgramData();

        public SetupWizardSettings()
        {
            InitializeComponent();

            PageSelectCabinet = new SetupWizardSettings_Page1();
            Frame_SetupWizardSettings.Navigate(PageSelectCabinet);

            
        }
        private void PanelDrag_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed) DragMove();
        }
        private void CloseWindow_Click(object sender, RoutedEventArgs e) => Close();

        public static void CheckData()
        {
            
        }
    }
}
