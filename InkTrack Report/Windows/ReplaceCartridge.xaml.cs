using InkTrack.Classes;
using InkTrack.Database;
using InkTrack.Helpers;
using InkTrack.Windows.ReplaceCartridgePages;
using iTextSharp.text;
using iTextSharp.text.pdf;
using NPetrovich;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Navigation;

namespace InkTrack.Windows
{
    public partial class ReplaceCartridge : System.Windows.Window
    {
        private static ReplaceCartridge _instance;
        public static ReplaceCartridge Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ReplaceCartridge(App.userKnown);
                }
                return _instance;
            }
        }




        static PageFullNameEnter pageFullName;
        static PageEnterInformationForReplaceCartridge PageEIFRC;
        static Frame static_Frame;
        static Border static_Border;
        static Window static_Window;
        static Border static_Border_SuccsesReplace;

        Device SelectedPrinter;
        private bool isNavigating;

        List<Cartridge> ListCartritgesForReplace;
        int SumPagesPrintouts;




        public void ShowReplaceCartridge()
        {
            if (_instance == null || !_instance.IsLoaded)
            {
                Instance.Show();
            }
            else
            {
                Instance.Activate(); 
            }
        }


        public ReplaceCartridge(bool userKnown)
        {
            InitializeComponent();

            static_Border = Border_Window;
            static_Frame = Frame_PagesReplaceCartridge;
            static_Window = this;
            static_Border_SuccsesReplace = Border_SuccsesReplace;


            static_Frame.Navigating += Frame_Navigating;
            static_Frame.Navigated += Frame_Navigated;

            switch (userKnown)
            {
                case true:
                    PageEIFRC = new PageEnterInformationForReplaceCartridge(true);
                    
                    static_Frame.Navigate(PageEIFRC);
                    
                    break;
                case false:
                    pageFullName = new PageFullNameEnter();
                    PageEIFRC = new PageEnterInformationForReplaceCartridge(false);

                    static_Frame.Navigate(pageFullName);
                    
                    break;
            }


        }
        static public void SetpageEnterInformationForReplaceCartridge()
        {
            static_Frame.Navigate(PageEIFRC);

        }
        static public void SetpageFullName()
        {
            static_Frame.Navigate(pageFullName);

        }




        static public void BeginAnimationReplace()
        {


            var fadeSplash = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300)) { EasingFunction = new CubicEase()};
            var fadeWidth = new DoubleAnimation(static_Border.ActualWidth, 302, TimeSpan.FromMilliseconds(300)) { EasingFunction = new CubicEase() };
            var fadeHeight = new DoubleAnimation(static_Border.ActualHeight, 64, TimeSpan.FromMilliseconds(300)) { EasingFunction = new CubicEase() };
            System.Timers.Timer timer = new System.Timers.Timer(3000) { AutoReset = false };
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(300)) { EasingFunction = new CubicEase() };
            fadeOut.Completed += (s, _) =>
            {
                static_Window.Dispatcher.Invoke(() =>
                {
                    static_Window.Close();
                });
            };
            timer.Elapsed += (s, _) =>
            {
                static_Border.Dispatcher.Invoke(() =>
                {
                    static_Border.BeginAnimation(OpacityProperty, fadeOut);
                });
                
            };
            


            static_Border.BeginAnimation(WidthProperty, fadeWidth);
            static_Border.BeginAnimation(HeightProperty, fadeHeight);
            static_Border_SuccsesReplace.Visibility = Visibility.Visible;
            static_Border_SuccsesReplace.BeginAnimation(OpacityProperty, fadeSplash);
            timer.Start();
        }


        public static void static_Replace_Click()
        {
            if (static_Window is ReplaceCartridge instance)
            {
                instance.Replace_Click();
            }
        }

        public void Replace_Click()
        {
            try
            {
                GenerateRequestPDF(DatabaseHelper.GetPrintOutDataList(PageEIFRC.SelectedPrinter.Printer));
                GenerateResultPrintingFiles(DatabaseHelper.GetPrintOutDataList(PageEIFRC.SelectedPrinter.Printer));

                Cartridge cartridge = PageEIFRC.SelectedPrinter.Printer.Cartridge;
                cartridge.Capacity = cartridge.Capacity <= SumPagesPrintouts ? SumPagesPrintouts : cartridge.Capacity;


                cartridge.StatusId = 3;
                PageEIFRC.SelectedPrinter.Printer.CartridgeReplacementDate = DateTime.Now;

                Cartridge SelectedCartridge = PageEIFRC.SelectedCartridgeToReplace;

                CartridgeReplacement_Log cartridgeReplacement_Log = new CartridgeReplacement_Log()
                {
                    OldCartridgeId = PageEIFRC.SelectedPrinter.Printer.Cartridge.Id,
                    NewCartridgeId = SelectedCartridge.Id,
                    PrinterId = PageEIFRC.SelectedPrinter.Id,
                    EmployeeLitId = PageEIFRC.WhoReplacedId,
                    ReasonId = PageEIFRC.ReasonToReplaceId
                };
                App.entities.CartridgeReplacement_Log.Add(cartridgeReplacement_Log);
                PageEIFRC.SelectedPrinter.Printer.Cartridge = SelectedCartridge;
                SelectedCartridge.StatusId = 1;

                DatabaseHelper.ResetPrintoutDataHistory(PageEIFRC.SelectedPrinter.Printer);


                BeginAnimationReplace();
            }
            catch (Exception ex)
            {
                Logger.Log("Error", "Получено исключение", ex);
            }
        }


        private void Frame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (isNavigating)
                return;

            e.Cancel = true;
            isNavigating = true;

            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(300));
            fadeOut.Completed += (s, _) =>
            {
                static_Frame.Navigate(e.Content);
            };

            static_Frame.BeginAnimation(OpacityProperty, fadeOut);
        }
        private void Frame_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300));
            fadeIn.Completed += (s, _) => isNavigating = false;
            static_Frame.BeginAnimation(OpacityProperty, fadeIn);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var fadeIn = new DoubleAnimation
            {
                From = 0.0,
                To = 1.0,
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut },
                Duration = new Duration(TimeSpan.FromSeconds(1))
            };
            this.BeginAnimation(OpacityProperty, fadeIn);
        }
        private void PanelDrag_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed) DragMove();
        }
        private void CloseWindow_Click(object sender, RoutedEventArgs e) => Close();

        public void GenerateResultPrintingFiles(List<PrintoutData> listOfPrintedDocuments)
        {
            // Create document with A4 size and margins (approximately 2-3cm)
            Document document = new Document(PageSize.A4);

            int CartridgeIDInstalled = PageEIFRC.SelectedPrinter.Printer.Cartridge.Id;
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
        public void GenerateRequestPDF(List<PrintoutData> listOfPrintedDocuments)
        {





            string dbFIO = "";

            if (App.LoginedEmployee != null)
            {
                dbFIO = App.LoginedEmployee.FullName;
            }
            else
            {
                dbFIO = pageFullName.FullName;
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

            int CartridgeIDInstalled = PageEIFRC.SelectedPrinter.Printer.Cartridge.Id;
            string NumberCartridge = App.entities.Cartridge.First(cartridge => cartridge.Id == CartridgeIDInstalled).Number;
            string PrinterName = PageEIFRC.SelectedPrinter.DeviceName;
            string CabinetName = PageEIFRC.SelectedPrinter.Room.Name;
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
