using System;
using System.Windows;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using Request_Refill.Windows;
using System.Windows.Resources;
using System.Windows.Media.Imaging;
using Request_Refill.Database;
using Request_Refill.Classes;
using System.Threading;
using Newtonsoft.Json;

namespace Request_Refill
{
    public partial class App : System.Windows.Application
    {
        private NotifyIcon notifyIcon;
        static public LitDBEntities dBEntities = new LitDBEntities();
        static string pathApplication;
        static public ProgramData programData;


        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            string pathToAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            pathApplication = Path.Combine(pathToAppData, "Request Refill");
            
            if (Directory.Exists(pathApplication))
            {
                string ConfigFilePath = Path.Combine(pathApplication, "Config.json");
                string JsonImportData = File.ReadAllText(ConfigFilePath);
                programData = JsonConvert.DeserializeObject<ProgramData>(JsonImportData);
            }
            else
            {
                Directory.CreateDirectory(pathApplication);
                string JsonData = JsonConvert.SerializeObject(new ProgramData(), Formatting.Indented);
                string CreateConfigFilePath = Path.Combine(pathApplication, "Config.json");
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

            // Создаем контекстное меню
            ContextMenuStrip contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Создание заявки на заправку", SelectImage("16/File B.png"), CreateRequestFill_Click);
            contextMenu.Items.Add("Настройки", SelectImage("16/Settings B.png"), Settings_Click);
            contextMenu.Items.Add("Закрыть", SelectImage("16/Shutdown B.png"), Close_Click);

            // Привязываем меню к иконке
            notifyIcon.ContextMenuStrip = contextMenu;
        }


        void NotifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            notifyIcon.ContextMenuStrip.Show(Cursor.Position);
        }
        private void CreateRequestFill_Click(object sender, EventArgs e) => new WindowCreateRequestRefill().ShowDialog();
        private void Settings_Click(object sender, EventArgs e) => new WindowSettings().ShowDialog();
        private void Close_Click(object sender, EventArgs e) => Shutdown();

        protected override void OnExit(ExitEventArgs e)
        {
            notifyIcon.Dispose(); // Очищаем ресурсы
            base.OnExit(e);
        }


        Icon SelectIcon(string FileName)
        {
            Uri iconUri = new Uri($"pack://application:,,,/Resources/{FileName}", UriKind.Absolute);
            StreamResourceInfo sri = System.Windows.Application.GetResourceStream(iconUri);

            return new Icon(sri.Stream);
        }

        Image SelectImage(string fileName)
        {
            Uri uri = new Uri($"pack://application:,,,/Resources/{fileName}", UriKind.Absolute);
            StreamResourceInfo sri = System.Windows.Application.GetResourceStream(uri);

            if (sri == null)
                throw new FileNotFoundException($"Image resource not found: {fileName}");

            using (var bmpStream = sri.Stream)
            {
                BitmapImage bmpImage = new BitmapImage();
                bmpImage.BeginInit();
                bmpImage.StreamSource = bmpStream;
                bmpImage.CacheOption = BitmapCacheOption.OnLoad;
                bmpImage.EndInit();
                bmpImage.Freeze();

                using (var ms = new MemoryStream())
                {
                    BitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bmpImage));
                    encoder.Save(ms);

                    return Image.FromStream(new MemoryStream(ms.ToArray()));
                }
            }
        }

    }
}
