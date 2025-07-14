using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using InkTrack.Classes;
using InkTrack.Windows.ReplaceCartridgePages;
using NPetrovich;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace InkTrack.Helpers
{
    public struct WordHelper
    {
        public void GenerateRequestWord(List<PrintoutData> listOfPrintedDocuments, PageFullNameEnter _pageFullName, PageEnterInformationForReplaceCartridge _pageEIFRC)
        {
            string dbFIO = App.LoginedEmployee?.FullName ?? _pageFullName.FullName;

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
            string GenetiveFIO = string.IsNullOrWhiteSpace(inflectedFIO.MiddleName)
                ? $"{inflectedFIO.LastName} {inflectedFIO.FirstName}"
                : $"{inflectedFIO.LastName} {inflectedFIO.FirstName} {inflectedFIO.MiddleName}";

            string pathToDesktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string pathToSaveDocx = Path.Combine(pathToDesktop, $"Заявка на заправку картриджа от {DateTime.Now:dd.MM.yyyy}.docx");

            using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(pathToSaveDocx, DocumentFormat.OpenXml.WordprocessingDocumentType.Document))
            {
                MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
                mainPart.Document = new Document();
                Body body = new Body();

                // Заголовок
                Paragraph paragraph = new Paragraph(
                    new ParagraphProperties()
                    {
                        
                    });

                body.Append(CreateParagraph($"Директору ГАПОУ\n«Забайкальский горный колледж имени И.М. Агошкова»\nН.В. Зыкову\nот {GenetiveFIO}", JustificationValues.Right));

                // Название
                body.Append(CreateParagraph("Заявка", JustificationValues.Center));

                // Данные картриджа
                int cartridgeID = _pageEIFRC.SelectedPrinter.Printer.Cartridge.Id;
                string cartridgeNumber = App.entities.Cartridge.First(c => c.Id == cartridgeID).Number;
                string printerName = _pageEIFRC.SelectedPrinter.DeviceName;
                string cabinetName = _pageEIFRC.SelectedPrinter.Room.Name;
                int totalPages = listOfPrintedDocuments.Sum(x => x.CountPages);

                string pageText = totalPages == 1
                    ? "страница"
                    : (totalPages < 5 ? "страницы" : "страниц");

                string requestText = $"Прошу произвести заправку картриджа №{cartridgeNumber} для принтера {printerName} в кабинете {cabinetName}. " +
                                     $"На картридже №{cartridgeNumber} было распечатано {totalPages} {pageText}.";

                body.Append(CreateParagraph(requestText, JustificationValues.Both));

                // Подпись и дата
                string footer = $"{DateTime.Now.ToLongDateString()}\n________________\nподпись";
                body.Append(CreateParagraph(footer, JustificationValues.Right));

                mainPart.Document.Append(body);
                mainPart.Document.Save();
            }

            Process.Start(new ProcessStartInfo(pathToSaveDocx) { UseShellExecute = true });
        }

        // Вспомогательная функция для абзаца
        private Paragraph CreateParagraph(string text, JustificationValues alignment, bool bold = false)
        {
            var runProperties = new RunProperties();
            if (bold)
                runProperties.Append(new Bold());

            return new Paragraph(
                new ParagraphProperties(
                    new Justification() { Val = alignment },
                    new SpacingBetweenLines() { After = "200" }),
                new Run(runProperties, new Text(text) { Space = SpaceProcessingModeValues.Preserve })
            );
        }

    }
}
