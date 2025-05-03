using Request_Refill.Classes;
using Request_Refill.Windows.Dialog;
using System.Collections.Generic;
using System.Windows;

namespace Request_Refill.Windows
{
    public partial class WindowCreateRequestRefill : Window
    {
        List<PrintoutData> listOfPrintedDocuments = new List<PrintoutData>();
        public WindowCreateRequestRefill()
        {
            InitializeComponent();
            DataGridListOfPrintedDocument.ItemsSource = listOfPrintedDocuments;
        }
        private void ClickCloseWindow(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void CreatePrintoutData_Click(object sender, RoutedEventArgs e)
        {
            if (new AddPrintout().ShowDialog() == true)
            {

            }
        }
    }
}
