using InkTrack_Report.Database;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace InkTrack_Report.Windows
{
    public partial class ReplaceCartridge : Window
    {
        Printer SelectedPrinter = App.entities.Printer.First(p => p.PrinterID == Properties.Settings.Default.SelectedPrinterID);

        List<Cartridge> ListCartritgesForReplace;
        public ReplaceCartridge()
        {
            ListCartritgesForReplace = App.entities.Cartridge.Where(c => c.CartridgeModel.Printer.Any(p => p.PrinterID == SelectedPrinter.PrinterID) && c.StatusID == 2).ToList();

            InitializeComponent();

            Listbox_CauseReplaceCartridge.ItemsSource = App.entities.ReasonForReplacement.ToList();
            Combobox_CartridgeOnReplace.ItemsSource = ListCartritgesForReplace;
            Combobox_WhoReplaced.ItemsSource = App.entities.EmployeeLIT.ToList();
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
            Cartridge SelectedCartridge = Combobox_CartridgeOnReplace.SelectedItem as Cartridge;

            CartridgeReplacement_Log cartridgeReplacement_Log = new CartridgeReplacement_Log()
            {
                OldCartridgeID = SelectedPrinter.Cartridge.CartridgeID,
                NewCartridgeID = (int)Combobox_CartridgeOnReplace.SelectedValue,
                PrinterID = SelectedPrinter.PrinterID,
                EmployeeLitID = (int)Combobox_WhoReplaced.SelectedValue,
                ReasonID = (int)Listbox_CauseReplaceCartridge.SelectedValue,
                Comment = Textbox_Description.Text
            };
            App.entities.CartridgeReplacement_Log.Add(cartridgeReplacement_Log);
            SelectedPrinter.Cartridge = SelectedCartridge;
            SelectedCartridge.StatusID = 3;

            App.entities.SaveChanges();

        }
    }
}
