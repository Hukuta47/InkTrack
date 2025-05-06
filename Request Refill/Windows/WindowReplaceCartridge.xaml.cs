using Request_Refill.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Request_Refill.Windows
{
    /// <summary>
    /// Логика взаимодействия для WindowReplaceCartridge.xaml
    /// </summary>
    public partial class WindowReplaceCartridge : Window
    {
        public WindowReplaceCartridge()
        {
            InitializeComponent();
            int SelectedPrinter = App.programData.SelectedPrinterID;
            Listbox_CauseReplaceCartridge.ItemsSource = App.dBEntities.ReasonForReplacement.ToList();
            Combobox_CartridgeNumber.ItemsSource = App.dBEntities.GetCompatibleCartridgesForPrinter(SelectedPrinter);
            Combobox_WhoReplaced.ItemsSource = App.dBEntities.EmployeeLIT.ToList();



            



        }

        private void AddCartridgeReplacement(object sender, RoutedEventArgs e)
        {
            int NewCartridgeID = (Combobox_CartridgeNumber.SelectedItem as GetCompatibleCartridgesForPrinter_Result).CartridgeID;
            int ReasonID = (Listbox_CauseReplaceCartridge.SelectedItem as ReasonForReplacement).ReasonID;
            int EmployeeLITID = (Combobox_WhoReplaced.SelectedItem as EmployeeLIT).EmployeeID;

            App.dBEntities.InstallCartridgeToPrinter(NewCartridgeID, ReasonID, EmployeeLITID);
        }
    }
}
