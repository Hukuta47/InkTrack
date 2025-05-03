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
            Close();
        }

        private void CreatePrintoutData_Click(object sender, RoutedEventArgs e)
        {
            AddPrintout DialogAddPrintout = new AddPrintout();

            if (DialogAddPrintout.ShowDialog() == true)
            {
                DialogAddPrintout.printoutData.Number = listOfPrintedDocuments.Count + 1;
                listOfPrintedDocuments.Add(DialogAddPrintout.printoutData);
                DataGridListOfPrintedDocument.Items.Refresh();
            }
        }
    }
}
