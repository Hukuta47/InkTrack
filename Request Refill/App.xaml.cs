using System;
using System.Windows;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using Request_Refill.Windows;
using System.Windows.Resources;
using Request_Refill.Database;
using Request_Refill.Classes;
using Newtonsoft.Json;
using System.Windows.Threading;
using System.Threading.Tasks;

namespace Request_Refill
{
    public partial class App : System.Windows.Application
    {
        private NotifyIcon notifyIcon;
        static public LitDBEntities dBEntities = new LitDBEntities();
        static public string pathApplication;
        static public string pathJsonSettingsFile;
        static public ProgramData programData;


        protected override void OnStartup(StartupEventArgs e)
        {
            // Обработка исключений в UI-потоке WPF
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;

            // Обработка исключений в других потоках
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            // Обработка исключений в задачах async/await
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;




            base.OnStartup(e);


            if (e.Args.Length > 0)
            {
                ShutdownMode = ShutdownMode.OnLastWindowClose;
                string filePath = e.Args[0];

                if (File.Exists(filePath) && Path.GetExtension(filePath).Equals(".rr", StringComparison.OrdinalIgnoreCase))
                {
                    string pathToAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    pathApplication = Path.Combine(pathToAppData, "Request Refill");

                    if (Directory.Exists(pathApplication))
                    {
                        pathJsonSettingsFile = Path.Combine(pathApplication, "Config.json");
                        string JsonImportData = File.ReadAllText(pathJsonSettingsFile);
                        programData = JsonConvert.DeserializeObject<ProgramData>(JsonImportData);
                    }
                    else
                    {
                        new SetupWizardSettings().Show();

                        Directory.CreateDirectory(pathApplication);
                        string JsonData = JsonConvert.SerializeObject(new ProgramData(), Formatting.Indented);
                        string CreateConfigFilePath = Path.Combine(pathApplication, "Config.json");
                        pathJsonSettingsFile = CreateConfigFilePath;
                        File.WriteAllText(CreateConfigFilePath, JsonData);
                    }
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
                string pathToAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                pathApplication = Path.Combine(pathToAppData, "Request Refill");

                if (Directory.Exists(pathApplication))
                {
                    pathJsonSettingsFile = Path.Combine(pathApplication, "Config.json");
                    string JsonImportData = File.ReadAllText(pathJsonSettingsFile);
                    programData = JsonConvert.DeserializeObject<ProgramData>(JsonImportData);
                }
                else
                {
                    Directory.CreateDirectory(pathApplication);
                    string JsonData = JsonConvert.SerializeObject(new ProgramData(), Formatting.Indented);
                    string CreateConfigFilePath = Path.Combine(pathApplication, "Config.json");
                    pathJsonSettingsFile = CreateConfigFilePath;
                    File.WriteAllText(CreateConfigFilePath, JsonData);
                }
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


        void NotifyIcon_MouseClick(object sender, MouseEventArgs e) => new WindowTraySelectFuntion().Show();

        protected override void OnExit(ExitEventArgs e)
        {
            notifyIcon?.Dispose();

            if (programData == null && pathApplication != null)
            {
                Directory.Delete(pathApplication, true);
            }
            base.OnExit(e);
        }


        Icon SelectIcon(string FileName)
        {
            Uri iconUri = new Uri($"pack://application:,,,/Resources/{FileName}", UriKind.Absolute);
            StreamResourceInfo sri = GetResourceStream(iconUri);

            return new Icon(sri.Stream);
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // Предотвращаем аварийное завершение
            e.Handled = true;

            ShowError(e.Exception, "Произошла непредвиденная ошибка в UI-потоке");

            // При желании можно завершить приложение:
            // Environment.Exit(1);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // Здесь нет возможности предотвратить завершение
            var ex = e.ExceptionObject as Exception;
            ShowError(ex, "Произошла ошибка вне UI-потока");
        }
        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            // Предотвращаем «подавление» приложения .NET
            e.SetObserved();

            ShowError(e.Exception, "Произошла ошибка в задаче");
        }
        private void ShowError(Exception ex, string title)
        {
            // Простейший вариант: стандартное окно
            System.Windows.MessageBox.Show(
                ex?.ToString() ?? "Неизвестная ошибка",
                title,
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            // Или открыть своё WPF-окно:
            // var win = new ErrorWindow();
            // win.Owner = Current?.MainWindow;
            // win.ShowDialog();
        }
    }
}
