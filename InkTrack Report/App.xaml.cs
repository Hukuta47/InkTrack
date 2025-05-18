using InkTrack_Report.Classes;
using InkTrack_Report.Database;
using InkTrack_Report.Windows;
using System;
using System.Drawing;
using System.IO;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Resources;
using System.Windows.Threading;

namespace InkTrack_Report
{
    public partial class App : System.Windows.Application
    {
        private NotifyIcon notifyIcon;
        static public LitDBEntities dBEntities = new LitDBEntities();
        static public string pathApplication;
        static public string pathJsonSettingsFile;


        protected override void OnStartup(StartupEventArgs e)
        {

            base.OnStartup(e);

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
        void NotifyIcon_MouseClick(object sender, MouseEventArgs e) => new WindowTraySelectFuntion().Show();

        protected override void OnExit(ExitEventArgs e)
        {
            notifyIcon?.Dispose();
            base.OnExit(e);
        }


        Icon SelectIcon(string FileName)
        {
            Uri iconUri = new Uri($"pack://application:,,,/Resources/{FileName}", UriKind.Absolute);
            StreamResourceInfo sri = GetResourceStream(iconUri);

            return new Icon(sri.Stream);
        }

    }
}
