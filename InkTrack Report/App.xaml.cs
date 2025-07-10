using InkTrack_Report.Classes;
using InkTrack_Report.Database;
using InkTrack_Report.Windows;
using InkTrack_Report.Windows.Dialog;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Timers;
using System.Windows;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace InkTrack_Report
{
    public partial class App : System.Windows.Application
    {
        static public LitEntities entities = new LitEntities();

        private ManagementEventWatcher _creationWatcher;
        private ManagementEventWatcher _modificationWatcher;
        
        private HashSet<int> _loggedJobIds = new HashSet<int>();
        static public Employee LoginedEmployee;

        System.Timers.Timer timerConnection = new System.Timers.Timer(20 * 1000);

        public static TrayIcon trayIcon;
        /// <summary>
        /// Метод который выполняется при нажатии любой клавишой миши по иконке в трее
        /// </summary>
        void DefaultNotifyIcon_MouseClick(object sender, MouseEventArgs e) => new WindowTraySelectFuntion(false).Show();
                
        /// <summary>
        /// Выполняемый метод при запуске программы
        /// </summary>
        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                base.OnStartup(e);
                ShutdownMode = ShutdownMode.OnExplicitShutdown;
                CheckInitilizationData();
                trayIcon = new TrayIcon();
                timerConnection.Elapsed += TimerConnection_Elapsed;

                if (CheckConnectionToDatabase(true))
                {
                    Logger.Log("SQL", "Подключен к базе данным SQL");
                    InitApplication();
                    trayIcon.ChangeIcon(TrayIcon.StatusIcon.Idle);
                    timerConnection.Start();
                }
                else
                {
                    Logger.Log("SQL", "Ошибка поключения к SQL базе данным");
                    timerConnection.Start();
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Error", "Ошибочка...", ex);
            }
        }
        
        /// <summary>
        /// Метод который выполняется когда таймер "timerConnection" заканчивается
        /// </summary>
        private void TimerConnection_Elapsed(object sender, ElapsedEventArgs e) => CheckConnectionToDatabase(false);
        
        /// <summary>
        /// Метод запускаемый прослушивание очереди печати
        /// </summary>
        private void StartPrintWatchers()
        {
            TimeSpan interval = TimeSpan.FromSeconds(1);

            // 1. Creation
            var creationQuery = new WqlEventQuery(
                "__InstanceCreationEvent",
                interval,
                "TargetInstance ISA 'Win32_PrintJob'"
            );
            _creationWatcher = new ManagementEventWatcher(creationQuery);
            _creationWatcher.EventArrived += OnPrintJobEvent;
            _creationWatcher.Start();

            // 2. Modification (TotalPages > 0)
            var modificationQuery = new WqlEventQuery(
                "__InstanceModificationEvent",
                interval,
                "TargetInstance ISA 'Win32_PrintJob' AND TargetInstance.TotalPages > 0"
            );
            _modificationWatcher = new ManagementEventWatcher(modificationQuery);
            _modificationWatcher.EventArrived += OnPrintJobEvent;
            _modificationWatcher.Start();
        }
        
        /// <summary>
        /// Триггер когда появляется задача на печать
        /// </summary>
        private void OnPrintJobEvent(object sender, EventArrivedEventArgs e)
        {
            var job = (ManagementBaseObject)e.NewEvent["TargetInstance"];
            HandlePrintJob(job);
        }
        
        /// <summary>
        /// Триггер когда изменяеся задача на печать
        /// </summary>
        private void OnPrintJobModified(object sender, EventArrivedEventArgs e)
        {
            var job = (ManagementBaseObject)e.NewEvent["TargetInstance"];
            HandlePrintJob(job);
        }
        
        /// <summary>
        /// Метод который сохраняет данные о задаче печати и сохраняет в базу информацию
        /// </summary>
        private void HandlePrintJob(ManagementBaseObject job)
        {
            if (!int.TryParse(job["JobId"]?.ToString(), out int jobId)) return;
            if (_loggedJobIds.Contains(jobId)) return;
            _loggedJobIds.Add(jobId);

            string docName = job["Document"]?.ToString() ?? "Без названия";

            string printerFullName = job["Name"]?.ToString() ?? "Неизвестный принтер";
            string printerName = printerFullName.Split(',')[0].Trim();
            
            int index = printerName.IndexOf("#") + 1;
            string printerInventoryNumber = printerName.Substring(index);

            int colonIndex = printerFullName.LastIndexOf(':');
            if (colonIndex > 0)
            {
                printerName = printerFullName.Substring(0, colonIndex).Trim();
            }

            int pages = 0;
            if (job["TotalPages"] != null) int.TryParse(job["TotalPages"].ToString(), out pages);

            bool isPdf = docName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);
            if (pages <= 1 && isPdf) {
                Dispatcher.Invoke(() => {
                    var dialog = new PageCountDialog(docName);
                    trayIcon.ChangeIcon(TrayIcon.StatusIcon.Write);
                    if (dialog.ShowDialog() == true && dialog.PageCount.HasValue) {
                        pages = dialog.PageCount.Value;
                    }
                    else {
                        trayIcon.ChangeIcon(TrayIcon.StatusIcon.CancelByUser, "Отказано пользователем");
                    }
                });
            }

            if (pages <= 0) return;

            var info = new PrintoutData {
                FIOWhoPrinting = LoginedEmployee.FullName,
                NameDocument = docName,
                CountPages = pages,
                Date = DateTime.Now
            };

            Dispatcher.Invoke(() => {
                trayIcon.ChangeIconOnTime(TrayIcon.StatusIcon.Save, "Сохранение данных", 2000);
                Logger.Log("Printed", $"Найдено задание: {info.NameDocument}, страниц: {info.CountPages}, Принтер:{printerInventoryNumber}");

                Printer printer = entities.Device.FirstOrDefault(d => d.InventoryNumber == printerInventoryNumber).Printer;

                SavePrintDataToDatabase(info, printer);
            });
        }
        
        /// <summary>
        /// Метод для инициализации программы
        /// </summary>
        void InitApplication()
        {
            try
            {
                int UserId = User.GetID();
                switch (UserId)
                {
                    case -1: Shutdown(); break;
                }
                LoginedEmployee = entities.Employee.FirstOrDefault(Employee => Employee.Id == UserId);
                StartPrintWatchers();
                trayIcon.NotifyIcon.MouseClick += DefaultNotifyIcon_MouseClick;
            }
            catch (Exception ex)
            {
                Logger.Log("Error", "Получено исключение", ex);
            }
            
        }        
        /// <summary>
        /// Проверка на подключение к базе данным
        /// </summary>
        /// <param name="isFirst">Парамет для самого первого запуска для предотвращения исключения при нехвате данных</param>
        /// <returns>Если подключение удачно, возвращает true, инчае false</returns>
        bool CheckConnectionToDatabase(bool isFirst)
        {
            try {
                entities.Database.Connection.Close();
                Logger.Log("SQL", "Попытка подключиться к SQL базе");
                trayIcon.ChangeIcon(TrayIcon.StatusIcon.Load, "Подключение к SQL базе данным...");
                trayIcon.NotifyIcon.MouseClick -= DefaultNotifyIcon_MouseClick;
                entities.Database.Connection.Open();
                Logger.Log("SQL", "Подключение восстановлено");
                trayIcon.ChangeIcon(TrayIcon.StatusIcon.Idle);
                trayIcon.NotifyIcon.MouseClick += DefaultNotifyIcon_MouseClick;
                return true;
            }
            catch (Exception e) {
                Logger.Log("SQL", $"Подключение к SQL базе не удачно", e);
                trayIcon.ChangeIcon(TrayIcon.StatusIcon.Alert, "Нет подключения к SQL базе данным.");
                return false;
            }
        }
        /// <summary>
        /// Метод который проверяет наличие папок в системе, если таковых нет, то создает
        /// </summary>
        void CheckInitilizationData() {
            if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + $"\\InkTrack Report Logs")) {
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + $"\\InkTrack Report Logs");
            }
        }
        public static void ResetPrintoutDataHistory(Printer printer)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<PrintoutData>));
            using (var stringWriter = new StringWriter())
            {
                serializer.Serialize(stringWriter, new List<PrintoutData>());
                printer.PrinterDocumentsList = stringWriter.ToString();
                entities.SaveChanges();
            }
        }
        public static List<PrintoutData> GetPrintOutDataList(Printer printer)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<PrintoutData>));
            if (string.IsNullOrWhiteSpace(printer.PrinterDocumentsList))
            {
                return new List<PrintoutData>();
            }
            else
            {
                using (var stringReader = new StringReader(printer.PrinterDocumentsList))
                {
                    return (List<PrintoutData>)serializer.Deserialize(stringReader);
                }
            }
        }
        public static void SavePrintDataToDatabase(PrintoutData printoutData, Printer printer)
        {
            List<PrintoutData> printoutDatas;

            XmlSerializer serializer = new XmlSerializer(typeof(List<PrintoutData>));
            if (string.IsNullOrWhiteSpace(printer.PrinterDocumentsList))
            {
                printoutDatas = new List<PrintoutData>();
            }
            else
            {
                using (var stringReader = new StringReader(printer.PrinterDocumentsList))
                {
                    printoutDatas = (List<PrintoutData>)serializer.Deserialize(stringReader);
                }
            }

            printoutDatas.Add(printoutData);


            serializer = new XmlSerializer(typeof(List<PrintoutData>));
            using (var stringWriter = new StringWriter())
            {
                serializer.Serialize(stringWriter, printoutDatas);
                printer.PrinterDocumentsList = stringWriter.ToString();
                entities.SaveChanges();
            }

        }
        
        /// <summary>
        /// Метод выполняемый при завершении работы программы
        /// </summary>
        /// <param name="e"></param>
        protected override void OnExit(ExitEventArgs e)
        {
            _creationWatcher?.Stop();
            _creationWatcher?.Dispose();
            _modificationWatcher?.Stop();
            _modificationWatcher?.Dispose();
            base.OnExit(e);
        }

        public void GenerateResultPrintingFiles(List<PrintoutData> listOfPrintedDocuments)
        {
            // Create document with A4 size and margins (approximately 2-3cm)
            Document document = new Document(PageSize.A4);

            int CartridgeIDInstalled = entities.Device.First(d => d.Id == 15).Printer.Cartridge.Id;
            string NumberCartridge = entities.Cartridge.First(cartridge => cartridge.Id == CartridgeIDInstalled).Number;

            string pathToDesktop = "\\\\zabgc-rabota\\LIT\\8. doc\\Отчеты картриджей";
            string pathToSavePdf = Path.Combine(pathToDesktop, $"Отчет картриджа №{NumberCartridge} от {DateTime.Now.ToShortDateString()}.pdf");



            PdfWriter.GetInstance(document, new FileStream(pathToSavePdf, FileMode.Create));
            document.Open();

            // Define font with Cyrillic support
            BaseFont baseFont = BaseFont.CreateFont("C:\\Windows\\Fonts\\times.ttf", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            Font regularFont = new Font(baseFont, 12);
            Font titleFont = new Font(baseFont, 14, Font.NORMAL);




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
