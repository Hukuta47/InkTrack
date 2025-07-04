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

        System.Timers.Timer timerConnection = new System.Timers.Timer(20 * 1000);

        bool isFirstStartup = InkTrack_Report.Properties.Settings.Default.isFirstStartup;

        public TrayIcon trayIcon;
        void DefaultNotifyIcon_MouseClick(object sender, MouseEventArgs e) => new WindowTraySelectFuntion(false).Show();
        private void SetNewSettingsNotifyIcon_MouseClick(object sender, MouseEventArgs e) {
            if (new SettingsSetupWizard(false).ShowDialog() == true) InitApplication();
        }
        private void TimerConnection_Elapsed(object sender, ElapsedEventArgs e) => CheckConnectionToDatabase(false);
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
        private void OnPrintJobEvent(object sender, EventArrivedEventArgs e)
        {
            var job = (ManagementBaseObject)e.NewEvent["TargetInstance"];
            HandlePrintJob(job);
        }
        private void OnPrintJobModified(object sender, EventArrivedEventArgs e)
        {
            var job = (ManagementBaseObject)e.NewEvent["TargetInstance"];
            HandlePrintJob(job);
        }
        private void HandlePrintJob(ManagementBaseObject job)
        {
            if (!int.TryParse(job["JobId"]?.ToString(), out int jobId)) return;
            if (_loggedJobIds.Contains(jobId)) return;
            _loggedJobIds.Add(jobId);

            string docName = job["Document"]?.ToString() ?? "Без названия";

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
                NameDocument = docName,
                CountPages = pages,
                Date = DateTime.Now
            };
            printoutDatas.Add(info);

            Dispatcher.Invoke(() => {
                trayIcon.ChangeIconOnTime(TrayIcon.StatusIcon.Save, "Сохранение данных", 2000);
                Debug.WriteLine($"Найдено задание: {info.NameDocument}, страниц: {info.CountPages}");
                SavePrintOutDatasToDatabase();
            });
        }
        void InitApplication() {
            StartPrintWatchers();
            trayIcon.NotifyIcon.MouseClick += DefaultNotifyIcon_MouseClick;
        }
        bool EnabledDeviceActualityInCabinet() {
            return entities.Cabinet.First(c => c.CabinetID == InkTrack_Report.Properties.Settings.Default.SelectedCabinetID).Device.Any(d => d.DeviceID == InkTrack_Report.Properties.Settings.Default.SelectedPrinterID);
        }
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
        void CheckInitilizationData() {
            Logger.Log("Check", "Проверка наличия папки отладки...");
            if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + $"\\InkTrack Report Logs")) {
                Logger.Log("Check", "Папка не найдена, создание...");
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + $"\\InkTrack Report Logs");
                Logger.Log("Check", "Папка создана");
            }
            else {
               Logger.Log("Check", "Проверка пройдена");
            }
        }
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
        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            trayIcon = new TrayIcon();

            timerConnection.Elapsed += TimerConnection_Elapsed;
            timerConnection.Start();

            CheckInitilizationData();

            if (CheckConnectionToDatabase(true)) {
                Logger.Log("SQL", "Подключен к базе данным SQL");
                if (isFirstStartup) {
                    Logger.Log("Program", "Первый запуск программы");
                    if (new SettingsSetupWizard(true).ShowDialog() == true) InitApplication();
                }
                else {
                    if (EnabledDeviceActualityInCabinet() != true) {
                        if (new SettingsSetupWizard(false).ShowDialog() == true) InitApplication();
                    }
                    else InitApplication();
                }
            }
            else Logger.Log("SQL", "Ошибка поключения к SQL базе данным");
            LoadPrintOutDataFromXmlDatabase();
        }
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
