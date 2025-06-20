using InkTrack_Report.Classes;
using InkTrack_Report.Database;
using InkTrack_Report.Windows;
using InkTrack_Report.Windows.Dialog;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Resources;

namespace InkTrack_Report
{
    public partial class App : System.Windows.Application
    {
        private NotifyIcon notifyIcon;
        ManagementEventWatcher watcherPrinting;
        static public List<PrintoutData> printoutDatas = new List<PrintoutData>();
        static public LitDBEntities entities = new LitDBEntities();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            entities.CartridgeStatus.ToList();

            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            string pathApplication = "C:\\ProgramData\\InkTrackReport";

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
                        notifyIcon.Icon = SelectIcon("16/IconB.ico");
                    }
                    else
                    {
                        notifyIcon.Icon = SelectIcon("16/IconW.ico");
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
                    notifyIcon.Icon = SelectIcon("16/IconB.ico");
                }
                else
                {
                    notifyIcon.Icon = SelectIcon("16/IconW.ico");
                }
                notifyIcon.Visible = true;

                watcherPrinting = new ManagementEventWatcher(query);
                watcherPrinting.EventArrived += OnPrintJobCreated;
                watcherPrinting.Start();
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
                string pathApplication = "C:\\ProgramData\\InkTrackReport";
                File.WriteAllText($"{pathApplication}\\printoutDatas.json", JsonData);
            }
        }
        Icon SelectIcon(string FileName)
        {
            Uri iconUri = new Uri($"pack://application:,,,/Resources/{FileName}", UriKind.Absolute);
            StreamResourceInfo sri = GetResourceStream(iconUri);

            return new Icon(sri.Stream);
        }

    }
}
