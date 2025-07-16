using InkTrack.Classes;
using InkTrack.Database;
using iTextSharp.text;
using iTextSharp.text.pdf;
using NPetrovich;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public void GenerateRequestPDF(List<PrintoutData> listOfPrintedDocuments, Device SelectedPrinter, string FullName)
        {





            string dbFIO = "";

            if (App.LoginedEmployee != null)
            {
                dbFIO = App.LoginedEmployee.FullName;
            }
            else
            {
                dbFIO = FullName;
            }


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
            Font titleFont = new Font(baseFont, 14, Font.NORMAL);


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
            string PrinterName = SelectedPrinter.DeviceName;
            string CabinetName = SelectedPrinter.Room.Name;
            int totalPages = listOfPrintedDocuments.Sum(x => x.CountPages);


            Paragraph request;

            if (totalPages == 1) { request = new Paragraph($"Прошу произвести заправку картриджа №{NumberCartridge} для принтера {PrinterName} в кабинете {CabinetName}. На картридже №{NumberCartridge} было распечатано {totalPages} страница.", regularFont); }
            else if (totalPages > 1 && totalPages < 5) { request = new Paragraph($"Прошу произвести заправку картриджа №{NumberCartridge} для принтера {PrinterName} в кабинете {CabinetName}. На картридже №{NumberCartridge} было распечатано {totalPages} страницы.", regularFont); }
            else { request = new Paragraph($"Прошу произвести заправку картриджа №{NumberCartridge} для принтера {PrinterName} в кабинете {CabinetName}. На картридже №{NumberCartridge} было распечатано {totalPages} страниц.", regularFont); }

            request.IndentationLeft = 20;
            request.IndentationRight = 20;
            request.Alignment = Element.ALIGN_JUSTIFIED;
            request.SpacingAfter = 20f;
            document.Add(request);


            // Footer Section

            Paragraph footer = new Paragraph($"{DateTime.Now.ToLongDateString()}\n\n________________\nподпись", regularFont);
            footer.IndentationLeft = 400;
            footer.Alignment = Element.ALIGN_CENTER;
            footer.SpacingBefore = 20f;
            document.Add(footer);


            document.Close();

            Process.Start(new ProcessStartInfo(pathToSavePdf) { UseShellExecute = true });
        }
    }
}
