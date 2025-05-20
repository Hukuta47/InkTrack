using InkTrack_Report.Classes;
using InkTrack_Report.Windows.Dialog;
using System;
using System.Collections.Generic;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System.Windows;
using System.Linq;
using InkTrack_Report.Database;
using System.Windows.Input;
using Newtonsoft.Json;
using System.Text;
using System.Diagnostics;

namespace InkTrack_Report.Windows
{ 
    public partial class CreateRequestRefill : Window
    {
        int SelectedCabinetID = Properties.Settings.Default.SelectedCabinetID;
        int SelectedEmployeeID = Properties.Settings.Default.SelectedEmployeeID;
        int SelectedPrinterID = Properties.Settings.Default.SelectedPrinterID;



        List<PrintoutData> listOfPrintedDocuments = new List<PrintoutData>();
        int SumPagesPrintouts;
        public CreateRequestRefill()
        {
            InitializeComponent();
            
            DataGrid_ListOfPrintedDocument.ItemsSource = listOfPrintedDocuments;


            Random rnd = new Random();
            for (int i = 0; i < 20; i++)
            {
                listOfPrintedDocuments.Add(new PrintoutData
                {
                    Number = i + 1,
                    NameDocument = $"Report_{DateTime.Today:yyyyMMdd}_{i + 1}",
                    Date = DateTime.Now.AddDays(-i), // Даты за последние 10 дней
                    CountPages = rnd.Next(1, 21)     // Случайное число от 1 до 20
                });
            }

            SumPagesPrintouts = 0;
            foreach (PrintoutData printoutData in listOfPrintedDocuments)
            {
                SumPagesPrintouts += printoutData.CountPages;
            }
            Textblock_SumPagesPrintouts.Text = SumPagesPrintouts.ToString();


            if (listOfPrintedDocuments.Count > 0)
            {
                Button_Accept.IsEnabled = true;
            }
            else
            {
                Button_Accept.IsEnabled = false;
            }
        }
        public CreateRequestRefill(string pathFile)
        {
            string readData = File.ReadAllText(pathFile);
            
            byte[] bytes = readData.Split('-').Select(h => Convert.ToByte(h, 16)).ToArray();

            string decodedJson = Encoding.UTF8.GetString(bytes);

            InitializeComponent();
            listOfPrintedDocuments = JsonConvert.DeserializeObject<List<PrintoutData>>(decodedJson);
            DataGrid_ListOfPrintedDocument.ItemsSource = listOfPrintedDocuments;

            SumPagesPrintouts = 0;
            foreach (PrintoutData printoutData in listOfPrintedDocuments)
            {
                SumPagesPrintouts += printoutData.CountPages;
            }
            Textblock_SumPagesPrintouts.Text = SumPagesPrintouts.ToString();

        }
        private void CloseWindow_Click(object sender, RoutedEventArgs e) => Close();
        private void PanelDrag_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed) DragMove();
        }

        private void CreatePrintoutData_Click(object sender, RoutedEventArgs e)
        {

            PrintoutInfo DialogAddPrintout = new PrintoutInfo();

            if (DialogAddPrintout.ShowDialog() == true)
            {
                DialogAddPrintout.printoutData.Number = listOfPrintedDocuments.Count + 1;
                listOfPrintedDocuments.Add(DialogAddPrintout.printoutData);
                DataGrid_ListOfPrintedDocument.Items.Refresh();
            }

            SumPagesPrintouts = 0;
            foreach (PrintoutData printoutData in listOfPrintedDocuments)
            {
                SumPagesPrintouts += printoutData.CountPages;
            }
            Textblock_SumPagesPrintouts.Text = SumPagesPrintouts.ToString();

            Button_Accept.IsEnabled = true;
        }

        private void DeletePrintoutData_Click(object sender, RoutedEventArgs e)
        {
            if (DataGrid_ListOfPrintedDocument.SelectedItems.Count == 1)
            {
                listOfPrintedDocuments.Remove(DataGrid_ListOfPrintedDocument.SelectedItem as PrintoutData);
            }
            else
            {
                foreach (PrintoutData data in DataGrid_ListOfPrintedDocument.SelectedItems)
                {
                    listOfPrintedDocuments.Remove(data);
                }
            }

            for (int i = 1; i < listOfPrintedDocuments.Count + 1; i++)
            {
                listOfPrintedDocuments[i - 1].Number = i;
            }
            DataGrid_ListOfPrintedDocument.Items.Refresh();


            SumPagesPrintouts = 0;
            foreach (PrintoutData printoutData in listOfPrintedDocuments)
            {
                SumPagesPrintouts += printoutData.CountPages;
            }
            Textblock_SumPagesPrintouts.Text = SumPagesPrintouts.ToString();

            if (listOfPrintedDocuments.Count > 0)
            {
                Button_Accept.IsEnabled = true;
            }
            else
            {
                Button_Accept.IsEnabled = false;
            }
        }

        private void ChangePrintoutData_Click(object sender, RoutedEventArgs e)
        {
            PrintoutInfo DialogAddPrintout = new PrintoutInfo(DataGrid_ListOfPrintedDocument.SelectedItem as PrintoutData);

            if (DialogAddPrintout.ShowDialog() == true)
            {
                listOfPrintedDocuments[DataGrid_ListOfPrintedDocument.SelectedIndex] = DialogAddPrintout.printoutData;
                DataGrid_ListOfPrintedDocument.Items.Refresh();
            }
            SumPagesPrintouts = 0;
            foreach (PrintoutData printoutData in listOfPrintedDocuments)
            {
                SumPagesPrintouts += printoutData.CountPages;
            }
            Textblock_SumPagesPrintouts.Text = SumPagesPrintouts.ToString();
        }

        private void SaveRequestRefill_Click(object sender, RoutedEventArgs e)
        {
            GenerateFiles(listOfPrintedDocuments);
            Cartridge cartridge = App.dBEntities.Printer.First(printer => printer.PrinterID == SelectedPrinterID).Cartridge;
            cartridge.Capacity = SumPagesPrintouts;
            App.dBEntities.SaveChanges();
        }

        private void DataGridListOfPrintedDocument_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (DataGrid_ListOfPrintedDocument.SelectedItems.Count == 1)
            {
                Button_Change.Visibility = Visibility.Visible;
                Button_Delete.Visibility = Visibility.Visible;
            }
            else if (DataGrid_ListOfPrintedDocument.SelectedItems.Count > 1)
            {
                Button_Change.Visibility = Visibility.Collapsed;
                Button_Delete.Visibility = Visibility.Visible;
            }
            else
            {
                Button_Change.Visibility = Visibility.Collapsed;
                Button_Delete.Visibility = Visibility.Collapsed;
            }
        }
        public void GenerateFiles(List<PrintoutData> listOfPrintedDocuments)
        {
            // Create document with A4 size and margins (approximately 2-3cm)
            Document document = new Document(PageSize.A4);


            string pathToDesktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string pathToCreateFolder = Path.Combine(pathToDesktop, $"Заявка на заправку картриджа от {DateTime.Now.ToShortDateString()}");
            Directory.CreateDirectory(pathToCreateFolder);
            string pathToSavePdf = Path.Combine(pathToCreateFolder, $"Заявка на заправку картриджа.pdf");


            string pathToSaveRr = Path.Combine(pathToCreateFolder, $"Заявка на заправку картриджа.rr");
            string JsonData = JsonConvert.SerializeObject(listOfPrintedDocuments, Formatting.Indented);
            string binaryData = BitConverter.ToString(Encoding.UTF8.GetBytes(JsonData));
            File.WriteAllText(pathToSaveRr, binaryData);






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
            Phrase headerPhrase = new Phrase($"Директору ГАПОУ\n«Забайкальский горный колледж имени И.М. Агошкова»\nН.В. Зыкову\nот {App.dBEntities.Employee.First(employee => employee.EmployeeID == SelectedEmployeeID).FIO}", regularFont);
            headerPhrase.Leading = 1; // Высота строки = размер шрифта (12)
            header.Add(headerPhrase);
            document.Add(header);

            Paragraph title = new Paragraph("Заявка", titleFont);
            title.Alignment = Element.ALIGN_CENTER;
            title.SpacingAfter = 10f;
            document.Add(title);



            int CartridgeIDInstalled = App.dBEntities.Printer.First(printer => printer.PrinterID == SelectedPrinterID).Cartridge.CartridgeID;
            string NumberCartridge = App.dBEntities.Cartridge.First(cartridge => cartridge.CartridgeID == CartridgeIDInstalled).CartridgeNumber;
            string PrinterName = App.dBEntities.GetPrintersInCabinet(SelectedCabinetID).First(printer => printer.PrinterID == SelectedPrinterID).PrinterInfo;
            string CabinetName = App.dBEntities.Cabinet.First(cabinet => cabinet.CabinetID == SelectedCabinetID).CabinetName;


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
