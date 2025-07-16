using InkTrack.Classes;
using InkTrack.Database;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InkTrack.Helpers
{
    struct PdfHelper
    {
        public void GenerateResultPrintingFiles(List<PrintoutData> listOfPrintedDocuments, Device SelectedPrinter)
        {
            // Create document with A4 size and margins (approximately 2-3cm)
            Document document = new Document(PageSize.A4);

            int CartridgeIDInstalled = SelectedPrinter.Printer.Cartridge.Id;
            string NumberCartridge = App.entities.Cartridge.First(cartridge => cartridge.Id == CartridgeIDInstalled).Number;

            string pathToDesktop = "\\\\zabgc-rabota\\LIT\\8. doc\\Отчеты картриджей";
            string pathToSavePdf = Path.Combine(pathToDesktop, $"Отчет картриджа №{NumberCartridge} от {DateTime.Now.ToShortDateString()}.pdf");


            try
            {
                PdfWriter.GetInstance(document, new FileStream(pathToSavePdf, FileMode.Create));
            }
            catch (Exception ex)
            {
                pathToDesktop = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                pathToSavePdf = Path.Combine(pathToDesktop, $"Отчет картриджа №{NumberCartridge} от {DateTime.Now.ToShortDateString()}.pdf");

                PdfWriter.GetInstance(document, new FileStream(pathToSavePdf, FileMode.Create));
            }

            document.Open();

            // Define font with Cyrillic support
            BaseFont baseFont = BaseFont.CreateFont("C:\\Windows\\Fonts\\times.ttf", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            Font regularFont = new Font(baseFont, 12);
            Font boldFont = new Font(baseFont, 12, Font.BOLD);
            Font titleFont = new Font(baseFont, 14, Font.BOLD);


            Paragraph tableName = new Paragraph(
                $"Отчет работы картриджа №{NumberCartridge}",
                regularFont
            );
            tableName.Alignment = Element.ALIGN_CENTER;
            tableName.SpacingAfter = 10f;
            document.Add(tableName);


            int totalRows = listOfPrintedDocuments.Count;
            int rowIndex = 0;

            while (rowIndex < totalRows)
            {
                // Добавляем новую страницу (но не перед самой первой)
                if (rowIndex != 0)
                {
                    document.NewPage();
                }

                // Создание новой таблицы
                PdfPTable table = new PdfPTable(3);
                table.WidthPercentage = 80;
                table.SetWidths(new float[] { 50, 10, 20 }); // Ширины колонок

                // Заголовок таблицы
                string[] headers = { "Наименование документов", "Дата", "Количество страниц" };
                foreach (string headerText in headers)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(headerText, regularFont));
                    cell.HorizontalAlignment = headerText == "Наименование документов" ? Element.ALIGN_LEFT : Element.ALIGN_CENTER;
                    table.AddCell(cell);
                }

                // Определяем, сколько строк выводить на этой странице
                int rowsOnThisPage = (rowIndex == 0) ? 40 : 45;
                int rowsToPrint = Math.Min(rowsOnThisPage, totalRows - rowIndex);

                // Добавляем строки
                for (int i = 0; i < rowsToPrint; i++)
                {
                    var doc = listOfPrintedDocuments[rowIndex];

                    table.AddCell(new PdfPCell(new Phrase(doc.NameDocument, regularFont)) { HorizontalAlignment = Element.ALIGN_LEFT });
                    table.AddCell(new PdfPCell(new Phrase(doc.Date.ToString("dd.MM.yy"), regularFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                    table.AddCell(new PdfPCell(new Phrase(doc.CountPages.ToString(), regularFont)) { HorizontalAlignment = Element.ALIGN_CENTER });

                    rowIndex++;
                }

                // Добавляем таблицу на текущую страницу
                document.Add(table);
            }

            // Финальная строка "Итого:"
            PdfPTable totalTable = new PdfPTable(3);
            totalTable.WidthPercentage = 80;
            totalTable.SetWidths(new float[] { 50, 10, 20 });

            PdfPCell totalLabel = new PdfPCell(new Phrase("Итого:", regularFont));
            totalLabel.Colspan = 2;
            totalLabel.HorizontalAlignment = Element.ALIGN_LEFT;
            totalTable.AddCell(totalLabel);

            int totalPages = listOfPrintedDocuments.Sum(x => x.CountPages);
            PdfPCell totalValue = new PdfPCell(new Phrase(totalPages.ToString(), regularFont));
            totalValue.HorizontalAlignment = Element.ALIGN_CENTER;
            totalTable.AddCell(totalValue);

            document.Add(totalTable);


            document.Close();
        }

    }
}
