using Request_Refill.Classes;
using Request_Refill.Windows.Dialog;
using System;
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

            Random rnd = new Random();
            for (int i = 0; i < 10; i++)
            {
                listOfPrintedDocuments.Add(new PrintoutData
                {
                    Number = i + 1,
                    NameDocument = $"Report_{DateTime.Today:yyyyMMdd}_{i + 1}",
                    Date = DateTime.Now.AddDays(-i), // Даты за последние 10 дней
                    CountPages = rnd.Next(1, 21)     // Случайное число от 1 до 20
                });
            }
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

        private void DeletePrintoutData_Click(object sender, RoutedEventArgs e)
        {
            listOfPrintedDocuments.Remove(DataGridListOfPrintedDocument.SelectedItem as PrintoutData);

            for (int i = 1; i < listOfPrintedDocuments.Count + 1; i++)
            {
                listOfPrintedDocuments[i - 1].Number = i;
            }
            DataGridListOfPrintedDocument.Items.Refresh();
        }

        private void ChangePrintoutData_Click(object sender, RoutedEventArgs e)
        {
            AddPrintout DialogAddPrintout = new AddPrintout(DataGridListOfPrintedDocument.SelectedItem as PrintoutData);

            if (DialogAddPrintout.ShowDialog() == true)
            {
                listOfPrintedDocuments[DataGridListOfPrintedDocument.SelectedIndex] = DialogAddPrintout.printoutData;
                DataGridListOfPrintedDocument.Items.Refresh();
            }
        }
    }
}
