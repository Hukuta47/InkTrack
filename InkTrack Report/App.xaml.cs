using InkTrack_Report.Classes;
using InkTrack_Report.Database;
using InkTrack_Report.Windows;
using InkTrack_Report.Windows.Dialog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        static public List<PrintoutData> printoutDatas;
        static public LitDBEntities entities = new LitDBEntities();

        private ManagementEventWatcher _creationWatcher;
        private ManagementEventWatcher _modificationWatcher;
        
        private HashSet<int> _loggedJobIds = new HashSet<int>();
        static public Employee LoginedEmployee;

        System.Timers.Timer timerConnection = new System.Timers.Timer(20 * 1000);
        bool isFirstStartup = InkTrack_Report.Properties.Settings.Default.isFirstStartup;

        public TrayIcon trayIcon;
        /// <summary>
        /// Метод который выполняется при нажатии любой клавишой миши по иконке в трее
        /// </summary>
        void DefaultNotifyIcon_MouseClick(object sender, MouseEventArgs e) => new WindowTraySelectFuntion(false).Show();
        
        /// <summary>
        /// Метод который выполняется при нажатии любой клавишой миши по иконке в трее
        /// </summary>
        private void SetNewSettingsNotifyIcon_MouseClick(object sender, MouseEventArgs e) {
            if (new SettingsSetupWizard(false).ShowDialog() == true) InitApplication();
        }
        
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
                    if (isFirstStartup)
                    {
                        Logger.Log("Program", "Первый запуск программы");
                        if (new SettingsSetupWizard(true).ShowDialog() == true)
                        {
                            InitApplication();
                            timerConnection.Start();
                            LoadPrintOutDataFromXmlDatabase();
                        }
                    }
                    else
                    {
                        if (EnabledDeviceActualityInCabinet() != true)
                        {
                            if (new SettingsSetupWizard(false).ShowDialog() == true)
                            {
                                InitApplication();
                                timerConnection.Start();
                                LoadPrintOutDataFromXmlDatabase();
                            }
                        }
                        else
                        {
                            InitApplication();
                            timerConnection.Start();
                            LoadPrintOutDataFromXmlDatabase();
                        }
                    }
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
                FIOWhoPrinting = LoginedEmployee.FIO,
                NameDocument = docName,
                CountPages = pages,
                Date = DateTime.Now
            };
            printoutDatas.Add(info);

            Dispatcher.Invoke(() => {
                trayIcon.ChangeIconOnTime(TrayIcon.StatusIcon.Save, "Сохранение данных", 2000);
                Logger.Log("Printed", $"Найдено задание: {info.NameDocument}, страниц: {info.CountPages}, Принтер:{printerName}");
                SavePrintOutDatasToDatabase();
            });
        }
        
        /// <summary>
        /// Метод для инициализации программы
        /// </summary>
        void InitApplication() {
            int EmployeeId = User.GetID();
            LoginedEmployee = entities.Employee.FirstOrDefault(e => e.EmployeeID == EmployeeId);
            Classes.Settings.Init();
            StartPrintWatchers();
            trayIcon.NotifyIcon.MouseClick += DefaultNotifyIcon_MouseClick;
        }
        
        /// <summary>
        /// Метод который проверяет есть ли изменение в базе данных, для исключения момента кода принтер не в кабинете, а программа понимает иначе
        /// </summary>
        /// <returns>Когда есть изменение данных true, иначе false</returns>
        bool EnabledDeviceActualityInCabinet() {
            return entities.Cabinet.First(c => c.CabinetID == InkTrack_Report.Properties.Settings.Default.SelectedCabinetID).Device.Any(d => d.DeviceID == InkTrack_Report.Properties.Settings.Default.SelectedPrinterID);
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
                Logger.Log("Check", "Проверка данных на соответсвие");
                if (!isFirst) {
                    if (!EnabledDeviceActualityInCabinet()) {
                        Logger.Log("Check", "Проверка не пройдена, замена иконки и подписки метода");
                        trayIcon.ChangeIcon(TrayIcon.StatusIcon.DataError, "Ошибка данных, воспроизведите настройку заново");
                        trayIcon.NotifyIcon.MouseClick += SetNewSettingsNotifyIcon_MouseClick; ;
                    }
                    else {
                        Logger.Log("Check", "Проверка пройдена, все нормально");
                        trayIcon.NotifyIcon.MouseClick += DefaultNotifyIcon_MouseClick;
                        SyncPrintOutData();
                    }
                }
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
        
        /// <summary>
        /// Синхронизация данных о печатаемых документах между устройством и базы данных
        /// </summary>
        void SyncPrintOutData()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<PrintoutData>));
            Printer printer = entities.Printer.First(p => p.PrinterID == InkTrack_Report.Properties.Settings.Default.SelectedPrinterID);
            if (string.IsNullOrWhiteSpace(printer.PrintedDocumentsList))
            {
                printoutDatas = new List<PrintoutData>();
                return;
            }
            try
            {
                using (var stringReader = new StringReader(printer.PrintedDocumentsList))
                {
                    printoutDatas = (List<PrintoutData>)serializer.Deserialize(stringReader);
                }
            }
            catch
            {
                printoutDatas = new List<PrintoutData>();
            }



            int SumDatasInDatabase = 0;
            int SumDatasOnMachine = 0;

            List<PrintoutData> DatasInDatabase;
            List<PrintoutData> DatasOnMachine;

            DatasOnMachine = printoutDatas;
            if (string.IsNullOrWhiteSpace(printer.PrintedDocumentsList))
            {
                DatasInDatabase = new List<PrintoutData>();
                return;
            }
            try
            {
                using (var stringReader = new StringReader(printer.PrintedDocumentsList))
                {
                    DatasInDatabase = (List<PrintoutData>)serializer.Deserialize(stringReader);
                }
            }
            catch
            {
                DatasInDatabase = new List<PrintoutData>();
            }




            foreach (PrintoutData printoutData in DatasOnMachine)
            {
                SumDatasOnMachine += printoutData.CountPages;
            }
            foreach (PrintoutData printoutData in DatasInDatabase)
            {
                SumDatasInDatabase += printoutData.CountPages;
            }



            if (SumDatasOnMachine > SumDatasInDatabase)
            {
                SavePrintOutDatasToDatabase();
            }
            else
            {
                LoadPrintOutDataFromXmlDatabase();
            }
        }
        
        /// <summary>
        /// Метод который сохраняет данные о печати в базу данных в формате XML
        /// </summary>
        public static void SavePrintOutDatasToDatabase() {
            Logger.Log("SQL", "Сохранение списка документов");
            XmlSerializer serializer = new XmlSerializer(typeof(List<PrintoutData>));
            using (var stringWriter = new StringWriter()) {
                serializer.Serialize(stringWriter, printoutDatas);
                Printer printer = entities.Printer.First(p => p.PrinterID == InkTrack_Report.Properties.Settings.Default.SelectedPrinterID);
                printer.PrintedDocumentsList = stringWriter.ToString();
                entities.SaveChanges();
            }
        }
        
        /// <summary>
        /// Метод который загружает данные о печати в переменую из формата XML в список printoutDatas
        /// </summary>
        public static void LoadPrintOutDataFromXmlDatabase() {
            XmlSerializer serializer = new XmlSerializer(typeof(List<PrintoutData>));
            Printer printer = entities.Printer.First(p => p.PrinterID == InkTrack_Report.Properties.Settings.Default.SelectedPrinterID);
            if (string.IsNullOrWhiteSpace(printer.PrintedDocumentsList)) {
                printoutDatas = new List<PrintoutData>();
                return;
            }
            try {
                using (var stringReader = new StringReader(printer.PrintedDocumentsList)) {
                    printoutDatas = (List<PrintoutData>)serializer.Deserialize(stringReader);
                }
            }
            catch {
                printoutDatas = new List<PrintoutData>();
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
    }
}
