using System;
using System.Diagnostics;
using System.IO;

namespace InkTrack.Classes
{
    public struct Logger
    {
        public static void Log(string category, string text, Exception exception = null)
        {
            if (exception == null)
            {
                Debug.WriteLine($"{DateTime.Now.ToLongTimeString()} | {category} | {text}");
                File.AppendAllText(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + $"\\InkTrack Report Logs\\{DateTime.Now.ToShortDateString()} InkTrack Report_Log.txt", $"\n{DateTime.Now.ToLongTimeString()} | {category} | {text}");
            }
            else
            {
                App.trayIcon.ChangeIconOnTime(TrayIcon.StatusIcon.Alert, "Выдано исключение.", 10000);
                Debug.WriteLine($"{DateTime.Now.ToLongTimeString()} | {category} | {text}");
                File.AppendAllText(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + $"\\InkTrack Report Logs\\{DateTime.Now.ToShortDateString()} InkTrack Report_Log.txt", $"\n{DateTime.Now.ToLongTimeString()} | {category} | {text} ИСКЛЮЧЕНИЕ: ( Message: {exception.Message}, Source: {exception.Source} );");
            }
        }
        public static string LogText(string category, string text, Exception exception = null)
        {
            string Text;
            if (exception == null)
            {
                Text = $"{DateTime.Now.ToLongTimeString()} | {category} | {text}";
                return Text;
            }
            else
            {
                Text = $"{DateTime.Now.ToLongTimeString()} | {category} | {text}";
                return Text;
            }
        }
    }
}
