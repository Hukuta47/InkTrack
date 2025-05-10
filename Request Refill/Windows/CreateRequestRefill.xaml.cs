using Request_Refill.Classes;
using Request_Refill.Windows.Dialog;
using System;
using System.Collections.Generic;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System.Windows;
using System.Linq;
using Request_Refill.Database;
using System.Windows.Input;

namespace Request_Refill.Windows
{
    /// <summary>
    /// Логика взаимодействия для CreateRequestRefill.xaml
    /// </summary>
    

    public partial class CreateRequestRefill : Window
    {
        List<PrintoutData> listOfPrintedDocuments = new List<PrintoutData>();
        int SumPagesPrintouts;
        public CreateRequestRefill()
        {
            InitializeComponent();

            DataGridListOfPrintedDocument.ItemsSource = listOfPrintedDocuments;


            Random rnd = new Random();
            for (int i = 0; i < 5; i++)
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
        }
        private void CloseWindow_Click(object sender, RoutedEventArgs e) => Close();
        private void PanelDrag_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed) DragMove();
        }

        private void CreatePrintoutData_Click(object sender, RoutedEventArgs e)
        {

            DataWindowPrintout DialogAddPrintout = new DataWindowPrintout();

            if (DialogAddPrintout.ShowDialog() == true)
            {
                DialogAddPrintout.printoutData.Number = listOfPrintedDocuments.Count + 1;
                listOfPrintedDocuments.Add(DialogAddPrintout.printoutData);
                DataGridListOfPrintedDocument.Items.Refresh();
            }

            SumPagesPrintouts = 0;
            foreach (PrintoutData printoutData in listOfPrintedDocuments)
            {
                SumPagesPrintouts += printoutData.CountPages;
            }
            Textblock_SumPagesPrintouts.Text = SumPagesPrintouts.ToString();
        }

        private void DeletePrintoutData_Click(object sender, RoutedEventArgs e)
        {
            listOfPrintedDocuments.Remove(DataGridListOfPrintedDocument.SelectedItem as PrintoutData);

            for (int i = 1; i < listOfPrintedDocuments.Count + 1; i++)
            {
                listOfPrintedDocuments[i - 1].Number = i;
            }
            DataGridListOfPrintedDocument.Items.Refresh();


            SumPagesPrintouts = 0;
            foreach (PrintoutData printoutData in listOfPrintedDocuments)
            {
                SumPagesPrintouts += printoutData.CountPages;
            }
            Textblock_SumPagesPrintouts.Text = SumPagesPrintouts.ToString();
        }

        private void ChangePrintoutData_Click(object sender, RoutedEventArgs e)
        {
            DataWindowPrintout DialogAddPrintout = new DataWindowPrintout(DataGridListOfPrintedDocument.SelectedItem as PrintoutData);

            if (DialogAddPrintout.ShowDialog() == true)
            {
                listOfPrintedDocuments[DataGridListOfPrintedDocument.SelectedIndex] = DialogAddPrintout.printoutData;
                DataGridListOfPrintedDocument.Items.Refresh();
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
            GenerateTimesheet(listOfPrintedDocuments);
            Cartridge cartridge = App.dBEntities.Printer.First(printer => printer.PrinterID == App.programData.SelectedPrinterID).Cartridge;
            cartridge.Capacity = SumPagesPrintouts;
            App.dBEntities.SaveChanges();
        }
        public static void GenerateTimesheet(List<PrintoutData> listOfPrintedDocuments)
        {
            // Create document with A4 size and margins (approximately 2-3cm)
            Document document = new Document(PageSize.A4, 56, 56, 36, 36);
            PdfWriter.GetInstance(document, new FileStream("timesheet.pdf", FileMode.Create));
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
            header.IndentationLeft = 300;
            Phrase headerPhrase = new Phrase($"Директору ГАПОУ\n«Забайкальский горный колледж имени И.М. Агошкова»\nН.В. Зыкову\nот {App.dBEntities.Employee.First(employee => employee.EmployeeID == App.programData.SelectedEmployeeID).FIO}", regularFont);
            headerPhrase.Leading = 1; // Высота строки = размер шрифта (12)
            header.Add(headerPhrase);
            document.Add(header);

            Paragraph title = new Paragraph("Заявка", titleFont);
            title.Alignment = Element.ALIGN_CENTER;
            title.SpacingAfter = 10f;
            document.Add(title);

            Paragraph request = new Paragraph(
                "Прошу произвести заправку картриджа №0054 для принтера HP LaserJet PRO M132nw в кабинете 204",
                regularFont
            );
            request.Alignment = Element.ALIGN_CENTER;
            request.SpacingAfter = 20f;
            document.Add(request);

            // Table Section
            PdfPTable table = new PdfPTable(4);
            table.WidthPercentage = 100;
            table.SetWidths(new float[] { 10, 50, 20, 20 }); // Proportional column widths

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
            PdfPCell totalLabel = new PdfPCell(new Phrase("Итого:", boldFont));
            totalLabel.Colspan = 3;
            totalLabel.HorizontalAlignment = Element.ALIGN_RIGHT;
            totalLabel.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(totalLabel);

            PdfPCell totalValue = new PdfPCell(new Phrase(totalPages.ToString(), boldFont));
            totalValue.HorizontalAlignment = Element.ALIGN_CENTER;
            totalValue.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(totalValue);

            document.Add(table);

            // Footer Section
            Paragraph footer = new Paragraph("<30> Апрель 2025г.\n\nподпись", regularFont);
            footer.Alignment = Element.ALIGN_LEFT;
            footer.SpacingBefore = 20f;
            document.Add(footer);

            document.Close();
        }
    }
}
