
using InkTrack_Report.Classes;
using InkTrack_Report.Database;
using InkTrack_Report.Windows.Dialog;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;


namespace InkTrack_Report.Windows
{
    public partial class WindowTraySelectFuntion : Window
    {
        int SelectedCabinetID = Properties.Settings.Default.SelectedCabinetID;
        int SelectedEmployeeID = Properties.Settings.Default.SelectedEmployeeID;
        int SelectedPrinterID = Properties.Settings.Default.SelectedPrinterID;
        int SumPagesPrintouts;
        public WindowTraySelectFuntion(bool ServiceOn)
        {
            InitializeComponent();
            SumPagesPrintouts = 0;
            foreach (PrintoutData printoutData in App.printoutDatas)
            {
                SumPagesPrintouts += printoutData.CountPages;
            }
            this.SourceInitialized += (s, e) =>
            {
                var workingArea = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea;
                this.Left = workingArea.Right - this.Width + 8;
                this.Top = workingArea.Bottom - this.Height + 18;
            };
            if (ServiceOn)
            {
                Button_Service.Visibility = Visibility.Collapsed;
                ServiceButtons.Visibility = Visibility.Visible;
            }
        }
        private void Shutdown_Click(object sender, RoutedEventArgs e)
        {
            Deactivated -= Window_Deactivated;
            Application.Current.Shutdown();
        }
        private void OpenSettings_Click(object sender, RoutedEventArgs e) => new Settings().Show();
        private void OpenReplaceCartridge(object sender, RoutedEventArgs e) => new ReplaceCartridge().Show();
        private void Window_Deactivated(object sender, EventArgs e)
        {
            Close();
        }
        private void OpenServiceButtons_Click(object sender, EventArgs e)
        {
            if (new AdminAuthDialog().ShowDialog() == true)
            {
                new WindowTraySelectFuntion(true).Show();
            }
        }

        private void GeneratePrintOutList(object sender, RoutedEventArgs e)
        {
            GenerateFiles(App.printoutDatas);
            Cartridge cartridge = App.entities.Printer.First(printer => printer.PrinterID == SelectedPrinterID).Cartridge;
            cartridge.Capacity = SumPagesPrintouts;
            App.entities.SaveChanges();
        }
        public void GenerateFiles(List<PrintoutData> listOfPrintedDocuments)
        {
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
            Phrase headerPhrase = new Phrase($"Директору ГАПОУ\n«Забайкальский горный колледж имени И.М. Агошкова»\nН.В. Зыкову\nот {App.entities.Employee.First(employee => employee.EmployeeID == SelectedEmployeeID).FIO}", regularFont);
            headerPhrase.Leading = 1; // Высота строки = размер шрифта (12)
            header.Add(headerPhrase);
            document.Add(header);

            Paragraph title = new Paragraph("Заявка", titleFont);
            title.Alignment = Element.ALIGN_CENTER;
            title.SpacingAfter = 10f;
            document.Add(title);



            int CartridgeIDInstalled = App.entities.Printer.First(printer => printer.PrinterID == SelectedPrinterID).Cartridge.CartridgeID;
            string NumberCartridge = App.entities.Cartridge.First(cartridge => cartridge.CartridgeID == CartridgeIDInstalled).CartridgeNumber;
            string PrinterName = App.entities.GetPrintersInCabinet(SelectedCabinetID).First(printer => printer.PrinterID == SelectedPrinterID).PrinterInfo;
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
                table.AddCell(new PdfPCell(new Phrase(item.Number.ToString(), regularFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
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
    }
}
