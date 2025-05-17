using Newtonsoft.Json;
using InkTrack_Report.Classes;
using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;

namespace InkTrack_Report.Windows.Dialog
{
    public partial class PrintoutInfo : Window
    {
        int SelectedCabinetID = Properties.Settings.Default.SelectedCabinetID;
        int SelectedEmployeeID = Properties.Settings.Default.SelectedEmployeeID;
        int SelectedPrinterID = Properties.Settings.Default.SelectedPrinterID;



        public PrintoutData printoutData;

        DateTime MaxDateSelect = DateTime.Now.Date;
        DateTime? MinDateSelect = App.dBEntities.Printer.First(p => p.PrinterID == Properties.Settings.Default.SelectedPrinterID).CartridgeReplacementDate;
        bool isChange;

        public PrintoutInfo()
        {
            InitializeComponent();  
            Button_Accept.Content = "Добавить";

        }
        public PrintoutInfo(PrintoutData printoutData)
        {
            InitializeComponent();
            this.printoutData = printoutData;
            isChange = true;

            Button_Accept.Content = "Сохранить";
            Textbox_NameDocument.Text = printoutData.NameDocument;
            Textbox_CountPages.Text = printoutData.CountPages.ToString();
            DatePicker_date.SelectedDate = printoutData.Date;
        }

        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            switch (isChange)
            {
                case true:
                    if (ValidateInput())
                    {
                        printoutData.NameDocument = Textbox_NameDocument.Text;
                        printoutData.CountPages = int.Parse(Textbox_CountPages.Text);
                        printoutData.Date = DateTime.Parse(DatePicker_date.Text);

                        DialogResult = true;
                    }
                    break;

                case false:

                    if (ValidateInput())
                    {
                        printoutData = new PrintoutData()
                        {
                            NameDocument = Textbox_NameDocument.Text,
                            CountPages = int.Parse(Textbox_CountPages.Text),
                            Date = DateTime.Parse(DatePicker_date.Text),
                        };
                        DialogResult = true;
                    }

                    break;
            }
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public bool ValidateInput()
        {
            StringBuilder errors = new StringBuilder();

            string name = Textbox_NameDocument.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                errors.AppendLine("Название документа не может быть пустым");
            }
            else if (name.Length > 255)
            {
                errors.AppendLine("Название документа не может превышать 255 символов");
            }

            string countPages = Textbox_CountPages.Text.Trim();
            if (string.IsNullOrWhiteSpace(countPages))
            {
                errors.AppendLine("Количество страниц не может быть пустым");
            }
            else if (!int.TryParse(countPages, out int pages) || pages < 1 || pages > 100)
            {
                errors.AppendLine("Количество страниц должно быть целым числом от 1 до 100");
            }

            if (!DatePicker_date.SelectedDate.HasValue)
            {
                errors.AppendLine("Дата должна быть указана");
            }
            else
            {
                DateTime selectedDate = DatePicker_date.SelectedDate.Value;

                if (MinDateSelect > selectedDate || selectedDate > MaxDateSelect)
                {
                    errors.AppendLine($"Дата должна быть в диапазоне с {MinDateSelect.Value.ToShortDateString()} по {MaxDateSelect.ToShortDateString()}");
                }
            }

            if (errors.Length > 0)
            {
                System.Windows.Forms.MessageBox.Show(errors.ToString(), "Ошибки ввода", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }
    }
}
