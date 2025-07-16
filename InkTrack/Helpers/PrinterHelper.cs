using InkTrack.Classes;
using InkTrack.Database;
using InkTrack.Windows.Dialog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Windows;

namespace InkTrack.Helpers
{
    public class PrinterHelper
    {
        private ManagementEventWatcher _creationWatcher;
        private ManagementEventWatcher _modificationWatcher;
        private HashSet<int> _loggedJobIds = new HashSet<int>();

        /// <summary>
        /// Метод запускаемый прослушивание очереди печати
        /// </summary>
        public void StartPrintWatchers()
        {
            TimeSpan interval = TimeSpan.FromSeconds(1);

            // 1. Creation
            var creationQuery = new WqlEventQuery(
                "__InstanceCreationEvent",
                interval,
                "TargetInstance ISA 'Win32_PrintJob'"
            );
            _creationWatcher = new ManagementEventWatcher(creationQuery);
            _creationWatcher.EventArrived += OnPrintJobEvent;
            _creationWatcher.Start();

            // 2. Modification (TotalPages > 0)
            var modificationQuery = new WqlEventQuery(
                "__InstanceModificationEvent",
                interval,
                "TargetInstance ISA 'Win32_PrintJob' AND TargetInstance.TotalPages > 0"
            );
            _modificationWatcher = new ManagementEventWatcher(modificationQuery);
            _modificationWatcher.EventArrived += OnPrintJobEvent;
            _modificationWatcher.Start();
        }

        /// <summary>
        /// Триггер когда появляется задача на печать
        /// </summary>
        private void OnPrintJobEvent(object sender, EventArrivedEventArgs e)
        {
            if (App.ProgramWork)
            {
                var job = (ManagementBaseObject)e.NewEvent["TargetInstance"];
                HandlePrintJob(job);
            }
            
        }

        /// <summary>
        /// Триггер когда изменяеся задача на печать
        /// </summary>
        private void OnPrintJobModified(object sender, EventArrivedEventArgs e)
        {
            if (App.ProgramWork)
            {
                var job = (ManagementBaseObject)e.NewEvent["TargetInstance"];
                HandlePrintJob(job);
            }
        }

        /// <summary>
        /// Метод который сохраняет данные о задаче печати и сохраняет в базу информацию
        /// </summary>
        private void HandlePrintJob(ManagementBaseObject job)
        {
            if (!int.TryParse(job["JobId"]?.ToString(), out int jobId)) return;
            if (_loggedJobIds.Contains(jobId)) return;
            _loggedJobIds.Add(jobId);

            string docName = job["Document"]?.ToString() ?? "Без названия";

            string printerFullName = job["Name"]?.ToString() ?? "Неизвестный принтер";
            string printerName = printerFullName.Split(',')[0].Trim();

            int index = printerName.IndexOf("#") + 1;
            string printerInventoryNumber = printerName.Substring(index);

            int colonIndex = printerFullName.LastIndexOf(':');
            if (colonIndex > 0)
            {
                printerName = printerFullName.Substring(0, colonIndex).Trim();
            }

            int pages = 0;
            if (job["TotalPages"] != null) int.TryParse(job["TotalPages"].ToString(), out pages);

            bool isPdf = docName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);
            if (pages <= 1 && isPdf)
            {
                Application.Current.Dispatcher.Invoke(() => {
                    var dialog = new PageCountDialog(docName);
                    App.trayIcon.ChangeIcon(TrayIcon.StatusIcon.Write);
                    if (dialog.ShowDialog() == true && dialog.PageCount.HasValue)
                    {
                        pages = dialog.PageCount.Value;
                    }
                    else
                    {
                        App.trayIcon.ChangeIcon(TrayIcon.StatusIcon.CancelByUser, "Отказано пользователем");
                    }
                });
            }

            if (pages <= 0) return;

            var info = new PrintoutData
            {
                FIOWhoPrinting = App.LoginedEmployee.FullName,
                NameDocument = docName,
                CountPages = pages,
                Date = DateTime.Now
            };

            Application.Current.Dispatcher.Invoke(() => {
                App.trayIcon.ChangeIconOnTime(TrayIcon.StatusIcon.Save, "Сохранение данных", 2000);
                Logger.Log("Printed", $"Найдено задание: {info.NameDocument}, страниц: {info.CountPages}, Принтер:{printerInventoryNumber}");

                Printer printer = App.entities.Device.FirstOrDefault(d => d.InventoryNumber == printerInventoryNumber).Printer;

                DatabaseHelper.SavePrintDataToDatabase(info, printer);
            });
        }

    }
}
