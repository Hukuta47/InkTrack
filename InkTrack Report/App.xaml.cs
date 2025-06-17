using InkTrack_Report.Classes;
using InkTrack_Report.Database;
using InkTrack_Report.Windows;
using InkTrack_Report.Windows.Dialog;
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
        static public string pathApplication;
        static public string pathJsonSettingsFile;


        protected override void OnStartup(StartupEventArgs e)
        {

            base.OnStartup(e);

            entities.CartridgeStatus.ToList();

            if (e.Args.Length > 0)
            {
                ShutdownMode = ShutdownMode.OnLastWindowClose;
                string filePath = e.Args[0];

                if (File.Exists(filePath) && Path.GetExtension(filePath).Equals(".rr", StringComparison.OrdinalIgnoreCase))
                {
                    new CreateRequestRefill(filePath).Show();
                }
                else
                {
                    System.Windows.MessageBox.Show(
                        "Неподдерживаемый формат файла.\n" +
                        "Поддерживается только .rr файлы.",
                        "Ошибка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                    Shutdown();
                }
            }
            else
            {
                ShutdownMode = ShutdownMode.OnExplicitShutdown;

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
                }
            }
        }
        void NotifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:  new CreateRequestRefill().Show(); break;
                case MouseButtons.Right: if (new AdminAuthDialog().ShowDialog() == true) new WindowTraySelectFuntion().Show(); break;
            }
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
            var record = new PrintoutData
            {
                NameDocument = document,
                CountPages = pages,
                Date = dateTime
            };
            // сохраняем в историю
            lock (printoutDatas)
            {
                printoutDatas.Add(record);
            }

            // выводим в консоль
            Console.WriteLine("New print job detected: " + record);
        }
        Icon SelectIcon(string FileName)
        {
            Uri iconUri = new Uri($"pack://application:,,,/Resources/{FileName}", UriKind.Absolute);
            StreamResourceInfo sri = GetResourceStream(iconUri);

            return new Icon(sri.Stream);
        }

    }
}
