using InkTrack_Report.Classes;
using InkTrack_Report.Database;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Newtonsoft.Json;
using NPetrovich;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace InkTrack_Report.Windows
{
    public partial class ReplaceCartridge : Window
    {
        Printer SelectedPrinter = App.entities.Printer.First(p => p.PrinterID == Properties.Settings.Default.SelectedPrinterID);
        List<Cartridge> ListCartritgesForReplace;

        int SelectedCabinetID = Properties.Settings.Default.SelectedCabinetID;
        int SelectedPrinterID = Properties.Settings.Default.SelectedPrinterID;
        int SumPagesPrintouts;


        public ReplaceCartridge()
        {
            ListCartritgesForReplace = App.entities.Cartridge.Where(c => c.CartridgeModel.Printer.Any(p => p.PrinterID == SelectedPrinter.PrinterID) && c.StatusID == 2).ToList();
            SumPagesPrintouts = 0;
            foreach (PrintoutData printoutData in App.printoutDatas)
            {
                SumPagesPrintouts += printoutData.CountPages;
            }

            InitializeComponent();

            Listbox_CauseReplaceCartridge.ItemsSource = App.entities.ReasonForReplacement.ToList();
            Combobox_CartridgeOnReplace.ItemsSource = ListCartritgesForReplace;
            Combobox_WhoReplaced.ItemsSource = App.entities.EmployeeLIT.ToList();
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
            GenerateFiles(App.printoutDatas);
            App.printoutDatas = new List<PrintoutData>();
            App.SavePrintOutDatasToDatabase();
            Cartridge cartridge = App.entities.Printer.First(printer => printer.PrinterID == SelectedPrinterID).Cartridge;
            if (cartridge.Capacity <= SumPagesPrintouts)
            {
                cartridge.Capacity = SumPagesPrintouts;
            }
            cartridge.StatusID = 3;

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
            SelectedCartridge.StatusID = 1;

            App.entities.SaveChanges();
            string JsonData = JsonConvert.SerializeObject(App.printoutDatas, Formatting.Indented);
            string pathApplication = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\InkTrack Report";
            File.WriteAllText($"{pathApplication}\\printoutDatas.json", JsonData);

            MessageBox.Show("Картридж заменен.");
        }
        public void GenerateFiles(List<PrintoutData> listOfPrintedDocuments)
        {
            string dbFIO = App.LoginedEmployee.FIO;

            var petrovich = new Petrovich()
            {
                LastName = dbFIO.Split(' ')[0],
                FirstName = dbFIO.Split(' ')[1],
                MiddleName = dbFIO.Split(' ')[2],
                AutoDetectGender = true
            };
            var inflectedFIO = petrovich.InflectTo(Case.Genitive);

            string GenetiveFIO = $"{inflectedFIO.LastName} {inflectedFIO.FirstName} {inflectedFIO.MiddleName}";




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

            int CartridgeIDInstalled = App.entities.Printer.First(printer => printer.PrinterID == SelectedPrinterID).Cartridge.CartridgeID;
            string NumberCartridge = App.entities.Cartridge.First(cartridge => cartridge.CartridgeID == CartridgeIDInstalled).CartridgeNumber;
            string PrinterName = GetDeviceName(SelectedPrinterID);
            string CabinetName = App.entities.Cabinet.First(cabinet => cabinet.CabinetID == SelectedCabinetID).CabinetName;


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
            Device device = App.entities.Device.FirstOrDefault(d => d.DeviceID == DeviceId);

            return $"{device.Manufacturer} {device.Model}";
        }
    }
}
