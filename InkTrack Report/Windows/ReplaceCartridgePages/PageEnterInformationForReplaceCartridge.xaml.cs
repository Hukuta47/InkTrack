using InkTrack.Classes;
using InkTrack.Database;
using InkTrack.Helpers;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.fonts.cmaps;
using NPetrovich;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace InkTrack.Windows.ReplaceCartridgePages
{
    /// <summary>
    /// Логика взаимодействия для PageEnterInformationForReplaceCartridge.xaml
    /// </summary>
    public partial class PageEnterInformationForReplaceCartridge : Page
    {
        List<Cartridge> ListCartritgesForReplace;

        
        public Device SelectedPrinter;
        public int WhoReplacedId;
        public Cartridge SelectedCartridgeToReplace;
        public int ReasonToReplaceId;


        public PageEnterInformationForReplaceCartridge(bool onePage)
        {
            try
            {
                InitializeComponent();

                Combobox_CauseReplaceCartridge.ItemsSource = App.entities.ReasonForRelpacement.ToList();
                Combobox_WhoReplaced.ItemsSource = App.entities.Employee.Where(Employee => Employee.EmployeePosition.Any(Po => Po.Name == "ГПХ" || Po.Name == "Лаборант ЛИТ" || Po.Name == "Техник ЛИТ" || Po.Name == "Начальник ЛИТ")).ToList();



                if (onePage)
                {
                    Button_Return.Visibility = Visibility.Collapsed;
                    Button_ReplaceCartridge.SetValue(Grid.ColumnProperty, 1);
                }


                List<Device> printers = new List<Device>();
                foreach (string Printer in PrinterSettings.InstalledPrinters.Cast<string>().ToArray())
                {
                    if (Printer.Contains("#"))
                    {
                        int index = Printer.IndexOf("#") + 1;
                        string printerInventoryNumber = Printer.Substring(index);
                        printers.Add(App.entities.Device.First(Device => Device.InventoryNumber == printerInventoryNumber));
                    }
                }
                if (printers.Count == 1)
                {
                    SelectedPrinter = printers[0];
                    StackPanel_SectionSelectionPrinter.Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    Combobox_SelectPrinter.ItemsSource = printers;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка загрузки окна\n{ex.Message},\n{ex.Source}", "Ошибка", (MessageBoxButton)MessageBoxButtons.OK, (MessageBoxImage)MessageBoxIcon.Error);
                Logger.Log("Error", "Ошибка загрузки окна", ex);
            }
        }

        private void Return_Click(object sender, RoutedEventArgs e)
        {
            ReplaceCartridge.SetpageFullName();
        }
        private void Combobox_SelectPrinter_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            SelectedPrinter = Combobox_SelectPrinter.SelectedItem as Device;
            ListCartritgesForReplace = new List<Cartridge>();

            var compatibleModels = App.entities.CartridgeModel.Where(Model => Model.DeviceModel.Any(DM => DM.Device.Any(D => D.Id == SelectedPrinter.Id)));

            foreach (var model in compatibleModels)
            {
                foreach (var cart in model.Cartridge.Where(Cartridge => Cartridge.StatusId == 2))
                {
                    ListCartritgesForReplace.Add(cart);
                }
            }
            Combobox_CartridgeOnReplace.ItemsSource = ListCartritgesForReplace;
        }

        private void Replace_Click(object sender, RoutedEventArgs e)
        {
            WhoReplacedId = (int)Combobox_WhoReplaced.SelectedValue;
            SelectedCartridgeToReplace = (Cartridge)Combobox_CartridgeOnReplace.SelectedItem;
            ReasonToReplaceId = (int)Combobox_CauseReplaceCartridge.SelectedValue;


            Button_ReplaceCartridge.Content = "Думаю...";
            ReplaceCartridge.static_Replace_Click();
        }
    }
}
