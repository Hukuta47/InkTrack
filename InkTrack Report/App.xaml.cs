using InkTrack_Report.Classes;
using InkTrack_Report.Database;
using InkTrack_Report.Windows;
using InkTrack_Report.Windows.Dialog;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Timers;
using System.Windows;
using System.Windows.Forms;

namespace InkTrack_Report
{
    public partial class App : System.Windows.Application
    {
        static public List<PrintoutData> printoutDatas = new List<PrintoutData>();
        static public LitDBEntities entities = new LitDBEntities();
        private ManagementEventWatcher _creationWatcher;
        private ManagementEventWatcher _modificationWatcher;
        private HashSet<int> _loggedJobIds = new HashSet<int>();

        System.Timers.Timer timerConnection = new System.Timers.Timer(20 * 1000);

        string pathApplication = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\InkTrack Report";

        bool isFirstStartup = InkTrack_Report.Properties.Settings.Default.isFirstStartup;

        public TrayIcon trayIcon;

        void DefaultNotifyIcon_MouseClick(object sender, MouseEventArgs e) => new WindowTraySelectFuntion(false).Show();
        private void SelectPrinterNotifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (new SelectPrinter().ShowDialog() == true) InitApplication();
        }
        void Log(string category, string text) => Debug.WriteLine($"{DateTime.Now.ToLongTimeString()} | {category} | {text}");
        private void TimerConnection_Elapsed(object sender, ElapsedEventArgs e) => CheckConnectionToDatabase();
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
            if (!int.TryParse(job["JobId"]?.ToString(), out int jobId))
                return;

            if (_loggedJobIds.Contains(jobId))
                return;

            _loggedJobIds.Add(jobId);

            string docName = job["Document"]?.ToString() ?? "Без названия";

            int pages = 0;
            if (job["TotalPages"] != null)
                int.TryParse(job["TotalPages"].ToString(), out pages);

            bool isPdf = docName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);
            if (pages <= 1 && isPdf)
            {
                
                Dispatcher.Invoke(() =>
                {
                    var dialog = new PageCountDialog(docName);
                    trayIcon.ChangeIcon(TrayIcon.StatusIcon.Write);
                    if (dialog.ShowDialog() == true && dialog.PageCount.HasValue)
                    {
                        pages = dialog.PageCount.Value;
                    }
                    else
                    {
                        trayIcon.ChangeIcon(TrayIcon.StatusIcon.CancelByUser, "Отказано пользователем");
                    }
                });
            }

            if (pages <= 0)
                return;

            var info = new PrintoutData
            {
                NameDocument = docName,
                CountPages = pages,
                Date = DateTime.Now
            };

            printoutDatas.Add(info);

            Dispatcher.Invoke(() =>
            {
                trayIcon.ChangeIconOnTime(TrayIcon.StatusIcon.Save, "Сохранение данных", 2000);

                Debug.WriteLine($"Найдено задание: {info.NameDocument}, страниц: {info.CountPages}");

                string jsonData = JsonConvert.SerializeObject(printoutDatas, Formatting.Indented);
                File.WriteAllText(Path.Combine(pathApplication, "printoutDatas.json"), jsonData);
            });
        }
        void InitApplication() {
            StartPrintWatchers();
            trayIcon.NotifyIcon.MouseClick += DefaultNotifyIcon_MouseClick;
        }
        bool EnabledDeviceActualityInCabinet() {
            return entities.Cabinet.First(c => c.CabinetID == InkTrack_Report.Properties.Settings.Default.SelectedCabinetID).Device.Any(d => d.DeviceID == InkTrack_Report.Properties.Settings.Default.SelectedPrinterID);
        }
        bool CheckConnectionToDatabase()
        {
            try {
                entities.Database.Connection.Close();
                Log("SQL", "Попытка подключиться к SQL базе");
                trayIcon.ChangeIcon(TrayIcon.StatusIcon.Load, "Подключение к SQL базе данным...");
                trayIcon.NotifyIcon.MouseClick -= DefaultNotifyIcon_MouseClick;
                entities.Database.Connection.Open();
                Log("SQL", "Подключение восстановлено");
                trayIcon.ChangeIcon(TrayIcon.StatusIcon.Idle);
                Log("Check", "Проверка данных на соответсвие");
                if (!EnabledDeviceActualityInCabinet()) {
                    Log("Check", "Проверка не пройдена, замена иконки и подписки метода");
                    trayIcon.ChangeIcon(TrayIcon.StatusIcon.DataError, "Ошибка данных, воспроизведите настройку заново");
                    trayIcon.NotifyIcon.MouseClick += SelectPrinterNotifyIcon_MouseClick; ;
                }
                else {
                    Log("Check", "Проверка пройдена, все нормально");
                    trayIcon.NotifyIcon.MouseClick += DefaultNotifyIcon_MouseClick;
                }
                return true;
            }
            catch (Exception) {
                Log("SQL", "Подключение к SQL базе не удачно");
                trayIcon.ChangeIcon(TrayIcon.StatusIcon.Alert, "Нет подключения к SQL базе данным.");
                return false;
            }
        }
        void CheckInitilizationData()
        {
            if (!File.Exists($"{pathApplication}\\printoutDatas.json"))
            {
                string JsonData = JsonConvert.SerializeObject(printoutDatas, Formatting.Indented);
                Directory.CreateDirectory(pathApplication);
                File.WriteAllText($"{pathApplication}\\printoutDatas.json", JsonData);
            }
        }
        void LoadData()
        {
            string FileData = File.ReadAllText($"{pathApplication}\\printoutDatas.json");
            List<PrintoutData> jsonData = JsonConvert.DeserializeObject<List<PrintoutData>>(FileData);
            printoutDatas = jsonData;
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            trayIcon = new TrayIcon();

            timerConnection.Elapsed += TimerConnection_Elapsed;
            timerConnection.Start();

            CheckInitilizationData();
            LoadData();

            if (CheckConnectionToDatabase())
            {
                Log("SQL", "Подключен к базе данным SQL");
                if (isFirstStartup) {
                    Log("Program", "Первый запуск программы");
                    if (new SettingsSetupWizard(true).ShowDialog() == true) InitApplication();
                }
                else {
                    if (EnabledDeviceActualityInCabinet() != true) {
                        if (new SettingsSetupWizard(false).ShowDialog() == true) InitApplication();
                    }
                    else InitApplication();
                }
            }
            else Log("SQL", "Ошибка поключения к SQL базе данным");
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
