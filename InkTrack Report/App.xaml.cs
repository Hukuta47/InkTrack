using InkTrack_Report.Classes;
using InkTrack_Report.Helpers;
using InkTrack_Report.Database;
using InkTrack_Report.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Timers;
using System.Windows;
using System.Windows.Forms;

namespace InkTrack_Report
{
    public partial class App : System.Windows.Application
    {
        static public LitEntities entities = new LitEntities();

        private ManagementEventWatcher _creationWatcher;
        private ManagementEventWatcher _modificationWatcher;
        
        private HashSet<int> _loggedJobIds = new HashSet<int>();
        static public Employee LoginedEmployee;

        System.Timers.Timer timerConnection = new System.Timers.Timer(20 * 1000);

        public static TrayIcon trayIcon;
        /// <summary>
        /// Метод который выполняется при нажатии любой клавишой миши по иконке в трее
        /// </summary>
        static public void DefaultNotifyIcon_MouseClick(object sender, MouseEventArgs e) => new WindowTraySelectFuntion(false).Show();
                
        /// <summary>
        /// Выполняемый метод при запуске программы
        /// </summary>
        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                base.OnStartup(e);
                ShutdownMode = ShutdownMode.OnExplicitShutdown;
                trayIcon = new TrayIcon();
                CheckInitilizationData();
                timerConnection.Elapsed += TimerConnection_Elapsed;

                if (CheckConnectionToDatabase(true))
                {
                    Logger.Log("SQL", "Подключен к базе данным SQL");
                    InitApplication();
                    trayIcon.ChangeIcon(TrayIcon.StatusIcon.Idle);
                    timerConnection.Start();
                }
                else
                {
                    Logger.Log("SQL", "Ошибка поключения к SQL базе данным");
                    timerConnection.Start();
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Error", "Ошибочка...", ex);
            }
        }

        /// <summary>
        /// Метод который выполняется когда таймер "timerConnection" заканчивается
        /// </summary>
        private void TimerConnection_Elapsed(object sender, ElapsedEventArgs e) => CheckConnectionToDatabase(false);
        
        /// <summary>
        /// Метод для инициализации программы
        /// </summary>
        void InitApplication()
        {
            try
            {
                trayIcon.NotifyIcon.MouseClick -= DefaultNotifyIcon_MouseClick;
                int UserId = UserHelper.GetID();
                switch (UserId)
                {
                    case -1: Shutdown(); break;
                }
                
                LoginedEmployee = entities.Employee.FirstOrDefault(Employee => Employee.Id == UserId);

                new PrinterHelper().StartPrintWatchers();
                trayIcon.NotifyIcon.MouseClick += DefaultNotifyIcon_MouseClick;
            }
            catch (Exception ex)
            {
                Logger.Log("Error", "Получено исключение", ex);
            }
            
        }        

        /// <summary>
        /// Проверка на подключение к базе данным
        /// </summary>
        /// <param name="isFirst">Парамет для самого первого запуска для предотвращения исключения при нехвате данных</param>
        /// <returns>Если подключение удачно, возвращает true, инчае false</returns>
        bool CheckConnectionToDatabase(bool isFirst)
        {
            try {
                entities.Database.Connection.Close();
                Logger.Log("SQL", "Попытка подключиться к SQL базе");
                trayIcon.ChangeIcon(TrayIcon.StatusIcon.Load, "Подключение к SQL базе данным...");
                trayIcon.NotifyIcon.MouseClick -= DefaultNotifyIcon_MouseClick;
                entities.Database.Connection.Open();
                Logger.Log("SQL", "Подключение восстановлено");
                trayIcon.ChangeIcon(TrayIcon.StatusIcon.Idle);
                trayIcon.NotifyIcon.MouseClick += DefaultNotifyIcon_MouseClick;
                return true;
            }
            catch (Exception e) {
                Logger.Log("SQL", $"Подключение к SQL базе не удачно", e);
                trayIcon.ChangeIcon(TrayIcon.StatusIcon.Alert, "Нет подключения к SQL базе данным.");
                return false;
            }
        }

        /// <summary>
        /// Метод который проверяет наличие папок в системе, если таковых нет, то создает
        /// </summary>
        void CheckInitilizationData() {
            if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + $"\\InkTrack Report Logs")) {
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + $"\\InkTrack Report Logs");
            }
        }

        /// <summary>
        /// Метод выполняемый при завершении работы программы
        /// </summary>
        protected override void OnExit(ExitEventArgs e)
        {
            _creationWatcher?.Stop();
            _creationWatcher?.Dispose();
            _modificationWatcher?.Stop();
            _modificationWatcher?.Dispose();
            base.OnExit(e);
        }
    }
}
