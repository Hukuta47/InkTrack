using System;
using System.Windows;
using LIT_Organizer.Windows.StartWindow;
using LIT_Organizer.Database;

namespace LIT_Organizer
{
    public partial class App : Application
    {
        public static LitDBEntities dbEntites = new LitDBEntities();
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Current.ShutdownMode = ShutdownMode.OnLastWindowClose;

            new StartWindow().Show();

        }
    }
}
