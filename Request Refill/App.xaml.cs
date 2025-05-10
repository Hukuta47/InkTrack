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
using Newtonsoft.Json;

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
            base.OnStartup(e);
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


        void NotifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            new WindowTraySelectFuntion().Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            notifyIcon.Dispose(); // Очищаем ресурсы

            if (programData == null)
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
