using InkTrack_Report.Classes;
using InkTrack_Report.Database;
using InkTrack_Report.Windows;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
using System.Timers;
using System.Windows;
using System.Windows.Forms;

namespace InkTrack_Report
{
    public partial class App : System.Windows.Application
    {
        private NotifyIcon notifyIcon;
        ManagementEventWatcher watcherPrinting;
        static public List<PrintoutData> printoutDatas = new List<PrintoutData>();
        static public LitDBEntities entities = new LitDBEntities();
        System.Timers.Timer timerConnection = new System.Timers.Timer(20000);

        bool isConnectedDB = false;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            timerConnection.Elapsed += TimerConnection_Elapsed;

            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            string pathApplication = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\InkTrack Report";

            if (!File.Exists($"{pathApplication}\\printoutDatas.json"))
            {
                string JsonData = JsonConvert.SerializeObject(printoutDatas, Formatting.Indented);
                Directory.CreateDirectory(pathApplication);
                File.WriteAllText($"{pathApplication}\\printoutDatas.json", JsonData);
            }

            var query = new WqlEventQuery(
                "SELECT * FROM __InstanceCreationEvent " +
                "WITHIN 1 WHERE TargetInstance ISA 'Win32_PrintJob'"
            );

            if (InkTrack_Report.Properties.Settings.Default.isFirstStartup)
            {
                if (new SettingsSetupWizard().ShowDialog() == true)
                {
                    // Создаем иконку в трее
                    notifyIcon = new NotifyIcon();
                    notifyIcon.MouseClick += NotifyIcon_MouseClick;
                    if (ThemeDetector.GetWindowsTheme() == AppTheme.Light)
                    {
                        notifyIcon.Icon = InkTrack_Report.Properties.Resources.Load_B;
                    }
                    else
                    {
                        notifyIcon.Icon = InkTrack_Report.Properties.Resources.Load_W;
                    }

                    notifyIcon.Visible = true;

                    watcherPrinting = new ManagementEventWatcher(query);
                    watcherPrinting.EventArrived += OnPrintJobCreated;
                    watcherPrinting.Start();
                }
            }
            else
            {
                string FileData = File.ReadAllText($"{pathApplication}\\printoutDatas.json");
                List<PrintoutData> jsonData = JsonConvert.DeserializeObject<List<PrintoutData>>(FileData);
                printoutDatas = jsonData;


                notifyIcon = new NotifyIcon(); 
                notifyIcon.MouseClick += NotifyIcon_MouseClick;
                if (ThemeDetector.GetWindowsTheme() == AppTheme.Light)
                {
                    notifyIcon.Icon = InkTrack_Report.Properties.Resources.Load_B;
                }
                else
                {
                    notifyIcon.Icon = InkTrack_Report.Properties.Resources.Load_W;
                }
                notifyIcon.Visible = true;

                watcherPrinting = new ManagementEventWatcher(query);
                watcherPrinting.EventArrived += OnPrintJobCreated;
                watcherPrinting.Start();

                entities.Database.Connection.StateChange += Connection_StateChange;
                try
                {
                    entities.Database.Connection.Open();
                }
                catch (Exception ex)
                {
                    entities.Database.Connection.Close();
                    if (ThemeDetector.GetWindowsTheme() == AppTheme.Light)
                    {
                        notifyIcon.Icon = InkTrack_Report.Properties.Resources.Alert_B;
                    }
                    else
                    {
                        notifyIcon.Icon = InkTrack_Report.Properties.Resources.Alert_W;
                    }
                }
            }
            timerConnection.Start();
        }
        private void Connection_StateChange(object sender, System.Data.StateChangeEventArgs e)
        {
            switch (e.CurrentState)
            {
                case System.Data.ConnectionState.Open:
                    if (ThemeDetector.GetWindowsTheme() == AppTheme.Light)
                    {
                        notifyIcon.Icon = InkTrack_Report.Properties.Resources.Printer_B;
                    }
                    else
                    {
                        notifyIcon.Icon = InkTrack_Report.Properties.Resources.Printer_W;
                    }
                    break;
            }
        }
        private void TimerConnection_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (ThemeDetector.GetWindowsTheme() == AppTheme.Light)
                {
                    notifyIcon.Icon = InkTrack_Report.Properties.Resources.Load_B;
                }
                else
                {
                    notifyIcon.Icon = InkTrack_Report.Properties.Resources.Load_W;
                }
                entities.Database.Connection.Open();
            }
            catch (Exception ex)
            {
                entities.Database.Connection.Close();
                if (ThemeDetector.GetWindowsTheme() == AppTheme.Light)
                {
                    notifyIcon.Icon = InkTrack_Report.Properties.Resources.Alert_B;
                }
                else
                {
                    notifyIcon.Icon = InkTrack_Report.Properties.Resources.Alert_W;
                }
            }
        }
        void NotifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            new WindowTraySelectFuntion(false).Show();
        }
        protected override void OnExit(ExitEventArgs e)
        {
            notifyIcon?.Dispose();
            base.OnExit(e);
        }
        private static void OnPrintJobCreated(object sender, EventArrivedEventArgs e)
        {
            var printJob = (ManagementBaseObject)e.NewEvent["TargetInstance"];
            string document = (string)printJob["Document"];
            DateTime dateTime = DateTime.Now;
            int pages = 0;
            try
            {
                var totalPagesValue = printJob.Properties["TotalPages"].Value;
                if (totalPagesValue != null)
                {
                    pages = Convert.ToInt32(totalPagesValue);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Не удалось получить количество страниц: {ex.Message}");
            }
            if (pages != 0)
            {
                var record = new PrintoutData
                {
                    NameDocument = document,
                    CountPages = pages,
                    Date = dateTime
                };
                lock (printoutDatas)
                {
                    printoutDatas.Add(record);
                }
                string JsonData = JsonConvert.SerializeObject(printoutDatas, Formatting.Indented);
                string pathApplication = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\InkTrack Report";
                File.WriteAllText($"{pathApplication}\\printoutDatas.json", JsonData);
            }
        }

    }
}
