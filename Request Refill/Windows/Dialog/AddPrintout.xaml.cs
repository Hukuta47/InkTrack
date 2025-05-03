using Request_Refill.Classes;
using System;
using System.Linq;
using System.Windows;

namespace Request_Refill.Windows.Dialog
{
    public partial class AddPrintout : Window
    {
        public PrintoutData printoutData;

        DateTime MaxDateSelect = DateTime.Now.Date;
        DateTime? MinDateSelect = App.dBEntities.Printer.First(p => p.PrinterID == App.programData.SelectedPrinterID).CartridgeReplacementDate;


        public AddPrintout(bool EnabledSomePrinters)
        {
            InitializeComponent();
            Button_AddPrintoutData.Content = "Добавить";
        }
        public AddPrintout(bool EnabledSomePrinters, PrintoutData printoutData)
        {
            InitializeComponent();
            Button_AddPrintoutData.Content = "Сохранить";
        }
        private void AddPrintoutData_Click(object sender, RoutedEventArgs e)
        {

            if (DateTime.TryParse(DatePicker_date.Text, out DateTime DateSelected))
            {
                if (DateSelected > MaxDateSelect)
                {
                    
                }
                else if (MinDateSelect > DateSelected)
                {
                    
                }
            }
            else
            {
                //
            }




            printoutData = new PrintoutData()
                {
                    NameDocument = Textbox_NameDocument.Text,
                    CountPages = int.Parse(Textbox_CountPages.Text),
                    Date = DateTime.Parse(DatePicker_date.Text),
                };
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
