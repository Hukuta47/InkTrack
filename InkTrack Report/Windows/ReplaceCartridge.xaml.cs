using InkTrack_Report.Classes;
using InkTrack_Report.Database;
using iTextSharp.text;
using iTextSharp.text.pdf;
using NPetrovich;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace InkTrack_Report.Windows
{
    public partial class ReplaceCartridge : Window
    {
        Device SelectedPrinter;
        List<Cartridge> ListCartritgesForReplace;

        int SumPagesPrintouts;

        public ReplaceCartridge()
        {
            List<Device> printers = new List<Device>();
            foreach (string Printer in PrinterSettings.InstalledPrinters.Cast<string>().ToArray())
            {
                int index = Printer.IndexOf("#") + 1;
                string printerInventoryNumber = Printer.Substring(index);
                printers.Add(App.entities.Device.First(Device => Device.InventoryNumber == printerInventoryNumber));
            }
            
            InitializeComponent();
            Combobox_SelectPrinter.ItemsSource = printers;

            Listbox_CauseReplaceCartridge.ItemsSource = App.entities.ReasonForRelpacement.ToList();
            Combobox_CartridgeOnReplace.ItemsSource = ListCartritgesForReplace;
            Combobox_WhoReplaced.ItemsSource = App.entities.Employee.Where(Employee => Employee.EmployeePosition.Any(Po => Po.Name == "ГПХ" || Po.Name == "Лаборант ЛИТ" || Po.Name == "Техник ЛИТ" || Po.Name == "Начальник ЛИТ")).ToList();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var fadeIn = new DoubleAnimation
            {
                From = 0.0,
                To = 1.0,
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut },
                Duration = new Duration(TimeSpan.FromSeconds(1)) // Можно изменить время
            };
            this.BeginAnimation(OpacityProperty, fadeIn);
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
            GenerateFiles(App.GetPrintOutDataList(SelectedPrinter.Printer));

            Cartridge cartridge = SelectedPrinter.Printer.Cartridge;
            cartridge.Capacity = cartridge.Capacity <= SumPagesPrintouts ? SumPagesPrintouts : cartridge.Capacity;

            cartridge.StatusId = 3;

            Cartridge SelectedCartridge = Combobox_CartridgeOnReplace.SelectedItem as Cartridge;

            CartridgeReplacement_Log cartridgeReplacement_Log = new CartridgeReplacement_Log()
            {
                OldCartridgeId = SelectedPrinter.Printer.Cartridge.Id,
                NewCartridgeId = (int)Combobox_CartridgeOnReplace.SelectedValue,
                PrinterId = SelectedPrinter.Id,
                EmployeeLitId = (int)Combobox_WhoReplaced.SelectedValue,
                ReasonId = (int)Listbox_CauseReplaceCartridge.SelectedValue,
                Comment = Textbox_Description.Text
            };
            App.entities.CartridgeReplacement_Log.Add(cartridgeReplacement_Log);
            SelectedPrinter.Printer.Cartridge = SelectedCartridge;
            SelectedCartridge.StatusId = 1;

            App.ResetPrintoutDataHistory(SelectedPrinter.Printer);

            MessageBox.Show("Картридж заменен.");
        }
        public void GenerateFiles(List<PrintoutData> listOfPrintedDocuments)
        {
            string dbFIO = App.LoginedEmployee.FullName;
            var fioParts = dbFIO.Split(' ');

            string lastName = fioParts.Length > 0 ? fioParts[0] : "";
            string firstName = fioParts.Length > 1 ? fioParts[1] : "";
            string middleName = fioParts.Length > 2 ? fioParts[2] : "";

            var petrovich = new Petrovich()
            {
                LastName = lastName,
                FirstName = firstName,
                MiddleName = middleName,
                AutoDetectGender = true
            };

            var inflectedFIO = petrovich.InflectTo(Case.Genitive);

            // Если отчества нет, просто не включаем его в строку
            string GenetiveFIO = string.IsNullOrWhiteSpace(inflectedFIO.MiddleName)
                ? $"{inflectedFIO.LastName} {inflectedFIO.FirstName}"
                : $"{inflectedFIO.LastName} {inflectedFIO.FirstName} {inflectedFIO.MiddleName}";

            // Create document with A4 size and margins (approximately 2-3cm)
            Document document = new Document(PageSize.A4);


            string pathToDesktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string pathToSavePdf = Path.Combine(pathToDesktop, $"Заявка на заправку картриджа от {DateTime.Now.ToShortDateString()}.pdf");



            PdfWriter.GetInstance(document, new FileStream(pathToSavePdf, FileMode.Create));
            document.Open();

            // Define font with Cyrillic support
            BaseFont baseFont = BaseFont.CreateFont("C:\\Windows\\Fonts\\times.ttf", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            Font regularFont = new Font(baseFont, 12);
            Font boldFont = new Font(baseFont, 12, Font.BOLD);
            Font titleFont = new Font(baseFont, 14, Font.BOLD);


            // Header Section
            Paragraph header = new Paragraph();
            header.Alignment = Element.ALIGN_LEFT;
            header.SpacingAfter = 10f;
            header.IndentationLeft = 325;
            Phrase headerPhrase = new Phrase($"Директору ГАПОУ\n«Забайкальский горный колледж имени И.М. Агошкова»\nН.В. Зыкову\nот {GenetiveFIO}", regularFont);
            headerPhrase.Leading = 1; // Высота строки = размер шрифта (12)
            header.Add(headerPhrase);
            document.Add(header);

            Paragraph title = new Paragraph("Заявка", titleFont);
            title.Alignment = Element.ALIGN_CENTER;
            title.SpacingAfter = 10f;
            document.Add(title);

            int CartridgeIDInstalled = SelectedPrinter.Printer.Cartridge.Id;
            string NumberCartridge = App.entities.Cartridge.First(cartridge => cartridge.Id == CartridgeIDInstalled).Number;
            string PrinterName = GetDeviceName(SelectedPrinter.Id);
            string CabinetName = SelectedPrinter.Room.Name;


            Paragraph request = new Paragraph(
                $"Прошу произвести заправку картриджа №{NumberCartridge} для принтера {PrinterName} в кабинете {CabinetName}",
                regularFont
            );
            request.Alignment = Element.ALIGN_CENTER;
            request.SpacingAfter = 20f;
            document.Add(request);

            // Table Section
            PdfPTable table = new PdfPTable(4);
            table.WidthPercentage = 100;
            table.SetWidths(new float[] { 4, 50, 10, 20 }); // Proportional column widths

            // Header Row
            string[] headers = { "№", "Наименование документов", "Дата", "Количество страниц" };
            foreach (string headerText in headers)
            {
                PdfPCell cell = new PdfPCell(new Phrase(headerText, boldFont));
                cell.HorizontalAlignment = headerText == "Наименование документов" ? Element.ALIGN_LEFT : Element.ALIGN_CENTER;
                table.AddCell(cell);
            }

            // Data Rows from listOfPrintedDocuments
            foreach (var item in listOfPrintedDocuments)
            {
                table.AddCell(new PdfPCell(new Phrase(item.NameDocument, regularFont)) { HorizontalAlignment = Element.ALIGN_LEFT });
                table.AddCell(new PdfPCell(new Phrase(item.Date.ToString("dd.MM.yy"), regularFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                table.AddCell(new PdfPCell(new Phrase(item.CountPages.ToString(), regularFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
            }

            // Total Row
            int totalPages = listOfPrintedDocuments.Sum(x => x.CountPages);
            PdfPCell totalLabel = new PdfPCell(new Phrase("Итого:", regularFont));
            totalLabel.Colspan = 3;
            totalLabel.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(totalLabel);

            PdfPCell totalValue = new PdfPCell(new Phrase(totalPages.ToString(), regularFont));
            totalValue.HorizontalAlignment = Element.ALIGN_CENTER;
            table.AddCell(totalValue);

            document.Add(table);

            // Footer Section

            Paragraph footer = new Paragraph($"{DateTime.Now.ToLongDateString()}\n________________\nподпись", regularFont);
            footer.IndentationLeft = 400;
            footer.Alignment = Element.ALIGN_CENTER;
            footer.SpacingBefore = 20f;
            document.Add(footer);


            document.Close();

            Process.Start(new ProcessStartInfo(pathToSavePdf) { UseShellExecute = true });
        }
        string GetDeviceName(int DeviceId)
        {
            Device device = App.entities.Device.FirstOrDefault(d => d.Id == DeviceId);

            return $"{device.DeviceModel.Manufacturer} {device.DeviceModel.Model}";
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
    }
}
