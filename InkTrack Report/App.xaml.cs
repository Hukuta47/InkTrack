using InkTrack_Report.Classes;
using InkTrack_Report.Database;
using InkTrack_Report.Windows;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Printing;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Forms;

namespace InkTrack_Report
{
    public partial class App : System.Windows.Application
    {
        private NotifyIcon notifyIcon;
        static public List<PrintoutData> printoutDatas = new List<PrintoutData>();
        static public LitDBEntities entities = new LitDBEntities();




        private ManagementEventWatcher _creationWatcher;
        private ManagementEventWatcher _modificationWatcher;
        private ManagementEventWatcher _deletionWatcher;
        private HashSet<string> _loggedJobs = new HashSet<string>();



        System.Timers.Timer timerConnection = new System.Timers.Timer(20 * 1000);
        System.Timers.Timer timerIconChange = new System.Timers.Timer(2000);

        string pathApplication = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\InkTrack Report";
        bool isFirstStartup = InkTrack_Report.Properties.Settings.Default.isFirstStartup;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            timerIconChange.Elapsed += TimerIconChange_Elapsed;

            if (isFirstStartup)
            {
                ShutdownMode = ShutdownMode.OnLastWindowClose;

                if (new SettingsSetupWizard().ShowDialog() == true) FirstInitApplication();
            }
            else
            {
                ShutdownMode = ShutdownMode.OnExplicitShutdown;

                InitApplication();
            }
        }

        private void Connection_StateChange(object sender, System.Data.StateChangeEventArgs e)
        {
            switch (e.CurrentState)
            {
                case System.Data.ConnectionState.Open:
                    notifyIcon.Icon = ThemeDetector.GetWindowsTheme() == AppTheme.Light ? InkTrack_Report.Properties.Resources.Printer_B : InkTrack_Report.Properties.Resources.Printer_W;
                break;
            }
        }
        private void TimerConnection_Elapsed(object sender, ElapsedEventArgs e)
        {
            CheckConnectionToDatabase();
        }
        private void TimerIconChange_Elapsed(object sender, ElapsedEventArgs e)
        {
            notifyIcon.Icon = ThemeDetector.GetWindowsTheme() == AppTheme.Light ? InkTrack_Report.Properties.Resources.Printer_B : InkTrack_Report.Properties.Resources.Printer_W;
            timerIconChange.Stop();
        }
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

            // 3. Deletion (опционально, просто для отладки)
            var deletionQuery = new WqlEventQuery(
                "__InstanceDeletionEvent",
                interval,
                "TargetInstance ISA 'Win32_PrintJob'"
            );
            _deletionWatcher = new ManagementEventWatcher(deletionQuery);
            _deletionWatcher.EventArrived += OnPrintJobDeleted;
            _deletionWatcher.Start();
        }




        private void OnPrintJobEvent(object sender, EventArrivedEventArgs e)
        {
            var job = (ManagementBaseObject)e.NewEvent["TargetInstance"];
            string jobName = job["Name"]?.ToString() ?? "";

            if (string.IsNullOrWhiteSpace(jobName) || _loggedJobs.Contains(jobName))
                return;

            int pages = 0;
            if (job["TotalPages"] != null)
                int.TryParse(job["TotalPages"].ToString(), out pages);

            if (pages <= 0)
                return;

            _loggedJobs.Add(jobName);

            var info = new PrintoutData
            {
                NameDocument = job["Document"]?.ToString() ?? "Без названия",
                CountPages = pages,
                Date = DateTime.Now
            };

            printoutDatas.Add(info);

            Dispatcher.Invoke(() =>
            {
                ChangeIcon(ThemeDetector.GetWindowsTheme() == AppTheme.Light
                    ? InkTrack_Report.Properties.Resources.Save_B
                    : InkTrack_Report.Properties.Resources.Save_W);
                Debug.WriteLine($"Найдено задание: {info.NameDocument}, страниц: {info.CountPages}");

                // Запись в JSON
                string jsonData = JsonConvert.SerializeObject(printoutDatas, Formatting.Indented);
                File.WriteAllText(Path.Combine(pathApplication, "printoutDatas.json"), jsonData);
            });
        }

        private void OnPrintJobDeleted(object sender, EventArrivedEventArgs e)
        {
            var job = (ManagementBaseObject)e.NewEvent["TargetInstance"];
            string jobName = job["Name"]?.ToString() ?? "";
            Debug.WriteLine($"Удалено задание: {jobName}");
        }






        private void OnPrintJobModified(object sender, EventArrivedEventArgs e)
        {
            var job = (ManagementBaseObject)e.NewEvent["TargetInstance"];
            string jobName = job["Name"].ToString(); // содержит "PrinterName, JobID"

            // Убедимся, что ещё не записывали это задание
            if (_loggedJobs.Contains(jobName))
                return;
            _loggedJobs.Add(jobName);

            int pages = Convert.ToInt32(job["TotalPages"]);

            if (pages <= 0)
                return; // Игнорируем задания с нулевым количеством страниц

            // Логируем (код)
            var info = new PrintoutData
            {
                NameDocument = job["Document"].ToString(),
                CountPages = pages,
                Date = DateTime.Now
            };

            printoutDatas.Add(info);
            ChangeIcon(ThemeDetector.GetWindowsTheme() == AppTheme.Light ? InkTrack_Report.Properties.Resources.Save_B : InkTrack_Report.Properties.Resources.Save_W);
            Debug.WriteLine($"Нашел печать {info.NameDocument}, страниц: {info.CountPages}");

            string JsonData = JsonConvert.SerializeObject(printoutDatas, Formatting.Indented);
            File.WriteAllText($"{pathApplication}\\printoutDatas.json", JsonData);
        }
        void ChangeIcon(Icon changeIcon)
        {
            notifyIcon.Icon = changeIcon;
            timerIconChange.Start();
        }
        void NotifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            new WindowTraySelectFuntion(false).Show();
        }
        protected override void OnExit(ExitEventArgs e)
        {
            notifyIcon?.Dispose();
            _creationWatcher?.Stop();
            _creationWatcher?.Dispose();
            _modificationWatcher?.Stop();
            _modificationWatcher?.Dispose();
            _deletionWatcher?.Stop();
            _deletionWatcher?.Dispose();
            base.OnExit(e);
        }
        void InitApplication()
        {
            StartPrintWatchers();
            timerConnection.Elapsed += TimerConnection_Elapsed;

            if (!File.Exists($"{pathApplication}\\printoutDatas.json"))
            {
                string JsonData = JsonConvert.SerializeObject(printoutDatas, Formatting.Indented);
                Directory.CreateDirectory(pathApplication);
                File.WriteAllText($"{pathApplication}\\printoutDatas.json", JsonData);
            }

            string FileData = File.ReadAllText($"{pathApplication}\\printoutDatas.json");
            List<PrintoutData> jsonData = JsonConvert.DeserializeObject<List<PrintoutData>>(FileData);
            printoutDatas = jsonData;

            notifyIcon = new NotifyIcon();
            notifyIcon.MouseClick += NotifyIcon_MouseClick;
            notifyIcon.Icon = ThemeDetector.GetWindowsTheme() == AppTheme.Light ? InkTrack_Report.Properties.Resources.Load_B : InkTrack_Report.Properties.Resources.Load_W;
            notifyIcon.Visible = true;

            entities.Database.Connection.StateChange += Connection_StateChange;
            CheckConnectionToDatabase();

            timerConnection.Start();
        }
        void FirstInitApplication()
        {
            StartPrintWatchers();
            timerConnection.Elapsed += TimerConnection_Elapsed;


            if (!File.Exists($"{pathApplication}\\printoutDatas.json"))
            {
                string JsonData = JsonConvert.SerializeObject(printoutDatas, Formatting.Indented);
                Directory.CreateDirectory(pathApplication);
                File.WriteAllText($"{pathApplication}\\printoutDatas.json", JsonData);
            }

            string FileData = File.ReadAllText($"{pathApplication}\\printoutDatas.json");
            List<PrintoutData> jsonData = JsonConvert.DeserializeObject<List<PrintoutData>>(FileData);
            printoutDatas = jsonData;


            notifyIcon = new NotifyIcon();
            notifyIcon.MouseClick += NotifyIcon_MouseClick;
            notifyIcon.Icon = ThemeDetector.GetWindowsTheme() == AppTheme.Light ? InkTrack_Report.Properties.Resources.Load_B : InkTrack_Report.Properties.Resources.Load_W;
            notifyIcon.Visible = true;

            entities.Database.Connection.StateChange += Connection_StateChange;
            try
            {
                entities.Database.Connection.Open();
            }
            catch (Exception ex)
            {
                entities.Database.Connection.Close();
                notifyIcon.Icon = ThemeDetector.GetWindowsTheme() == AppTheme.Light ? InkTrack_Report.Properties.Resources.Alert_B : InkTrack_Report.Properties.Resources.Alert_W;
            }
            timerConnection.Start();
        }
        void CheckConnectionToDatabase()
        {
            try
            {
                if (entities.Database.Connection.State != System.Data.ConnectionState.Closed) entities.Database.Connection.Close();
                notifyIcon.Icon = ThemeDetector.GetWindowsTheme() == AppTheme.Light ? InkTrack_Report.Properties.Resources.Load_B : InkTrack_Report.Properties.Resources.Load_W;
                if (entities.Database.Connection.State != System.Data.ConnectionState.Open) entities.Database.Connection.Open();
            }
            catch (Exception ex)
            {
                entities.Database.Connection.Close();
                notifyIcon.Icon = ThemeDetector.GetWindowsTheme() == AppTheme.Light ? InkTrack_Report.Properties.Resources.Alert_B : InkTrack_Report.Properties.Resources.Alert_W;
            }
        }
        bool EnabledDeviceActualityInCabinet()
        {
            return entities.Cabinet.First(c => c.CabinetID == InkTrack_Report.Properties.Settings.Default.SelectedCabinetID).Device.Any(d => d.DeviceID == InkTrack_Report.Properties.Settings.Default.SelectedPrinterID);
        }
    }
}
