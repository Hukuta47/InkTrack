using InkTrack_Report.Classes;
using InkTrack_Report.Database;
using InkTrack_Report.Windows;
using InkTrack_Report.Windows.Dialog;
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
        private HashSet<string> _loggedJobs = new HashSet<string>();
        private HashSet<int> _loggedJobIds = new HashSet<int>();



        System.Timers.Timer timerConnection = new System.Timers.Timer(20 * 1000);
        System.Timers.Timer timerIconChange = new System.Timers.Timer(2000);

        string pathApplication = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\InkTrack Report";
        bool isFirstStartup = InkTrack_Report.Properties.Settings.Default.isFirstStartup;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            timerIconChange.Elapsed += TimerIconChange_Elapsed;
            timerConnection.AutoReset = true;

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
                    notifyIcon.Icon = ThemeDetector.GetWindowsTheme() == AppTheme.Light ? InkTrack_Report.Properties.Resources.Write_B : InkTrack_Report.Properties.Resources.Write_W;
                    if (dialog.ShowDialog() == true && dialog.PageCount.HasValue)
                    {
                        pages = dialog.PageCount.Value;
                    }
                    else
                    {
                        ChangeIcon(ThemeDetector.GetWindowsTheme() == AppTheme.Light ? InkTrack_Report.Properties.Resources.CancelByUser_B : InkTrack_Report.Properties.Resources.CancelByUser_W);
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
                ChangeIcon(ThemeDetector.GetWindowsTheme() == AppTheme.Light
                    ? InkTrack_Report.Properties.Resources.Save_B
                    : InkTrack_Report.Properties.Resources.Save_W);

                Debug.WriteLine($"Найдено задание: {info.NameDocument}, страниц: {info.CountPages}");

                string jsonData = JsonConvert.SerializeObject(printoutDatas, Formatting.Indented);
                File.WriteAllText(Path.Combine(pathApplication, "printoutDatas.json"), jsonData);
            });
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
                notifyIcon.Icon = ThemeDetector.GetWindowsTheme() == AppTheme.Light
                    ? InkTrack_Report.Properties.Resources.Load_B
                    : InkTrack_Report.Properties.Resources.Load_W;

                entities.Database.ExecuteSqlCommand("SELECT 1");

                notifyIcon.Icon = ThemeDetector.GetWindowsTheme() == AppTheme.Light
                    ? InkTrack_Report.Properties.Resources.Printer_B
                    : InkTrack_Report.Properties.Resources.Printer_W;
            }
            catch (Exception)
            {
                notifyIcon.Icon = ThemeDetector.GetWindowsTheme() == AppTheme.Light
                    ? InkTrack_Report.Properties.Resources.Alert_B
                    : InkTrack_Report.Properties.Resources.Alert_W;
            }
        }
        bool EnabledDeviceActualityInCabinet()
        {
            return entities.Cabinet.First(c => c.CabinetID == InkTrack_Report.Properties.Settings.Default.SelectedCabinetID).Device.Any(d => d.DeviceID == InkTrack_Report.Properties.Settings.Default.SelectedPrinterID);
        }
    }
}
