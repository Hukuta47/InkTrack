using InkTrack.Classes;
using InkTrack.Database;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace InkTrack.Windows.ReplaceCartridgePages
{
    public partial class PageEnterInformationForReplaceCartridge : Page
    {
        private readonly ReplaceCartridge _parentWindow;
        private List<Cartridge> _cartridgesForReplace;

        public Device SelectedPrinter { get; private set; }
        public int WhoReplacedId { get; private set; }
        public Cartridge SelectedCartridgeToReplace { get; private set; }
        public int ReasonToReplaceId { get; private set; }

        public PageEnterInformationForReplaceCartridge(bool onePage, ReplaceCartridge parentWindow)
        {
            InitializeComponent();
            _parentWindow = parentWindow;

            try
            {
                LoadInitialData(onePage);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Ошибка загрузки окна\n{ex.Message},\n{ex.Source}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

                Logger.Log("Error", "Ошибка загрузки окна", ex);
            }
        }

        private void LoadInitialData(bool onePage)
        {
            Combobox_CauseReplaceCartridge.ItemsSource = App.entities.ReasonForRelpacement.ToList();

            Combobox_WhoReplaced.ItemsSource = App.entities.Employee
                .Where(emp => emp.EmployeePosition.Any(pos =>
                    pos.Name == "ГПХ" ||
                    pos.Name == "Лаборант ЛИТ" ||
                    pos.Name == "Техник ЛИТ" ||
                    pos.Name == "Начальник ЛИТ"))
                .ToList();

            if (onePage)
            {
                Button_Return.Visibility = Visibility.Collapsed;
                Button_ReplaceCartridge.SetValue(Grid.ColumnProperty, 1);
            }

            var printers = GetAvailablePrinters();

            if (printers.Count == 1)
            {
                SelectedPrinter = printers[0];
                Combobox_SelectPrinter.Visibility = Visibility.Collapsed;
                TextBlock_PrinterName.Text = SelectedPrinter.DeviceNameWithId;
                TextBlock_PrinterName.Visibility = Visibility.Visible;
                LoadCartridgesForPrinter(SelectedPrinter);
            }
            else
            {
                Combobox_SelectPrinter.ItemsSource = printers;
            }
        }

        private List<Device> GetAvailablePrinters()
        {
            var printers = new List<Device>();

            foreach (string printerName in PrinterSettings.InstalledPrinters)
            {
                if (printerName.Contains("#"))
                {
                    int index = printerName.IndexOf("#") + 1;
                    string inventoryNumber = printerName.Substring(index);

                    var device = App.entities.Device.FirstOrDefault(d => d.InventoryNumber == inventoryNumber);
                    if (device != null)
                    {
                        printers.Add(device);
                    }
                }
            }

            return printers;
        }

        private void LoadCartridgesForPrinter(Device printer)
        {
            _cartridgesForReplace = App.entities.Cartridge
                .Where(Cartridge => Cartridge.CartridgeModel
                .Any(Model => Model.DeviceModel
                .Any(DModel => DModel.Id == printer.DeviceModelId))).ToList();

            if (_cartridgesForReplace.Count == 0)
            {
                Combobox_CartridgeOnReplace.ItemsSource = new List<Cartridge>
                {
                    new Cartridge { Number = "Картриджей на замену нет..." }
                };
                Combobox_CartridgeOnReplace.SelectedIndex = 0;
                Button_ReplaceCartridge.IsEnabled = false;
            }
            else
            {
                Combobox_CartridgeOnReplace.ItemsSource = _cartridgesForReplace;
                Button_ReplaceCartridge.IsEnabled = true;
            }
        }

        private void Combobox_SelectPrinter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedDevice = Combobox_SelectPrinter.SelectedItem as Device;
            if (selectedDevice == null) return;

            SelectedPrinter = selectedDevice;
            LoadCartridgesForPrinter(SelectedPrinter);
        }

        private void Return_Click(object sender, RoutedEventArgs e)
        {
            _parentWindow.SetpageFullName();
        }

        private void Replace_Click(object sender, RoutedEventArgs e)
        {
            var errors = ValidateInputs();

            if (errors.Length == 0)
            {
                WhoReplacedId = (int)Combobox_WhoReplaced.SelectedValue;
                SelectedCartridgeToReplace = (Cartridge)Combobox_CartridgeOnReplace.SelectedItem;
                ReasonToReplaceId = (int)Combobox_CauseReplaceCartridge.SelectedValue;
                Button_ReplaceCartridge.Content = "Думаю...";

                _parentWindow.Replace_Click();
            }
            else
            {
                System.Windows.MessageBox.Show(errors.ToString(), "Ввод данных", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private StringBuilder ValidateInputs()
        {
            var sb = new StringBuilder();

            if (SelectedPrinter == null)
                sb.AppendLine("Принтер не указан.");

            if (Combobox_CauseReplaceCartridge.SelectedItem == null)
                sb.AppendLine("Причина замены не указана.");

            if (Combobox_CartridgeOnReplace.SelectedItem == null ||
                Combobox_CartridgeOnReplace.SelectedItem is Cartridge c && c.Number == "Картриджей на замену нет...")
                sb.AppendLine("Картридж на замену не выбран.");

            if (Combobox_WhoReplaced.SelectedItem == null)
                sb.AppendLine("Не указано, кто заменил картридж.");

            return sb;
        }
    }
}
