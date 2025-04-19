using System;
using System.Windows;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Reflection;
using Request_Refill.Windows;
using MessageBox = System.Windows.MessageBox;
using System.Windows.Resources;

namespace Request_Refill
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private NotifyIcon notifyIcon;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;


            // Создаем иконку в трее
            notifyIcon = new NotifyIcon();

            Uri iconUri = new Uri("pack://application:,,,/Resources/Printer.ico", UriKind.Absolute);
            StreamResourceInfo sri = System.Windows.Application.GetResourceStream(iconUri);
            notifyIcon.Icon = new Icon(sri.Stream);

            notifyIcon.Visible = true;

            // Создаем контекстное меню
            ContextMenuStrip contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Создание заявки на заправку", null, CreateRequestFill_Click);
            contextMenu.Items.Add("Настройки", null, Settings_Click);
            contextMenu.Items.Add("Закрыть", null, Close_Click);

            // Привязываем меню к иконке
            notifyIcon.ContextMenuStrip = contextMenu;
        }
        private void CreateRequestFill_Click(object sender, EventArgs e)
        {
            new WindowCreateRequestRefill().ShowDialog();
        }

        private void Settings_Click(object sender, EventArgs e)
        {
            new WindowSettings().ShowDialog();
        }

        private void Close_Click(object sender, EventArgs e)
        {
            Shutdown();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            notifyIcon.Dispose(); // Очищаем ресурсы
            base.OnExit(e);
        }

    }
}
