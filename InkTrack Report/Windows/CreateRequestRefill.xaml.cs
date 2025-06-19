using InkTrack_Report.Classes;
using InkTrack_Report.Windows.Dialog;
using System;
using System.Collections.Generic;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System.Windows;
using System.Linq;
using InkTrack_Report.Database;
using System.Windows.Input;
using Newtonsoft.Json;
using System.Text;
using System.Diagnostics;

namespace InkTrack_Report.Windows
{
    public partial class CreateRequestRefill : Window
    {
        


        List<PrintoutData> listOfPrintedDocuments = new List<PrintoutData>();
        
        public CreateRequestRefill()
        {
            InitializeComponent();
            
            //DataGrid_ListOfPrintedDocument.ItemsSource = App.printoutDatas;

            ////Random rnd = new Random();
            ////for (int i = 0; i < 20; i++)
            ////{
            ////    listOfPrintedDocuments.Add(new PrintoutData
            ////    {
            ////        Number = i + 1,
            ////        NameDocument = $"Report_{DateTime.Today:yyyyMMdd}_{i + 1}",
            ////        Date = DateTime.Now.AddDays(-i), // Даты за последние 10 дней
            ////        CountPages = rnd.Next(1, 21)     // Случайное число от 1 до 20
            ////    });
            ////}

            //SumPagesPrintouts = 0;
            //foreach (PrintoutData printoutData in listOfPrintedDocuments)
            //{
            //    SumPagesPrintouts += printoutData.CountPages;
            //}
            //Textblock_SumPagesPrintouts.Text = SumPagesPrintouts.ToString();


            //if (listOfPrintedDocuments.Count > 0)
            //{
            //    Button_Accept.IsEnabled = true;
            //}
            //else
            //{
            //    Button_Accept.IsEnabled = false;
            //}
        }
        public CreateRequestRefill(string pathFile)
        {
            //string readData = File.ReadAllText(pathFile);
            
            //byte[] bytes = readData.Split('-').Select(h => Convert.ToByte(h, 16)).ToArray();

            //string decodedJson = Encoding.UTF8.GetString(bytes);

            //InitializeComponent();
            //listOfPrintedDocuments = JsonConvert.DeserializeObject<List<PrintoutData>>(decodedJson);
            //DataGrid_ListOfPrintedDocument.ItemsSource = listOfPrintedDocuments;

            //SumPagesPrintouts = 0;
            //foreach (PrintoutData printoutData in listOfPrintedDocuments)
            //{
            //    SumPagesPrintouts += printoutData.CountPages;
            //}
            //Textblock_SumPagesPrintouts.Text = SumPagesPrintouts.ToString();

        }
        private void CloseWindow_Click(object sender, RoutedEventArgs e) => Close();
        private void PanelDrag_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed) DragMove();
        }

        private void CreatePrintoutData_Click(object sender, RoutedEventArgs e)
        {

            //PrintoutInfo DialogAddPrintout = new PrintoutInfo();

            //if (DialogAddPrintout.ShowDialog() == true)
            //{
            //    DialogAddPrintout.printoutData.Number = listOfPrintedDocuments.Count + 1;
            //    listOfPrintedDocuments.Add(DialogAddPrintout.printoutData);
            //    DataGrid_ListOfPrintedDocument.Items.Refresh();
            //}

            //SumPagesPrintouts = 0;
            //foreach (PrintoutData printoutData in listOfPrintedDocuments)
            //{
            //    SumPagesPrintouts += printoutData.CountPages;
            //}
            //Textblock_SumPagesPrintouts.Text = SumPagesPrintouts.ToString();

            //Button_Accept.IsEnabled = true;
        }

        private void DeletePrintoutData_Click(object sender, RoutedEventArgs e)
        {
            //if (DataGrid_ListOfPrintedDocument.SelectedItems.Count == 1)
            //{
            //    listOfPrintedDocuments.Remove(DataGrid_ListOfPrintedDocument.SelectedItem as PrintoutData);
            //}
            //else
            //{
            //    foreach (PrintoutData data in DataGrid_ListOfPrintedDocument.SelectedItems)
            //    {
            //        listOfPrintedDocuments.Remove(data);
            //    }
            //}

            //for (int i = 1; i < listOfPrintedDocuments.Count + 1; i++)
            //{
            //    listOfPrintedDocuments[i - 1].Number = i;
            //}
            //DataGrid_ListOfPrintedDocument.Items.Refresh();


            //SumPagesPrintouts = 0;
            //foreach (PrintoutData printoutData in listOfPrintedDocuments)
            //{
            //    SumPagesPrintouts += printoutData.CountPages;
            //}
            //Textblock_SumPagesPrintouts.Text = SumPagesPrintouts.ToString();

            //if (listOfPrintedDocuments.Count > 0)
            //{
            //    Button_Accept.IsEnabled = true;
            //}
            //else
            //{
            //    Button_Accept.IsEnabled = false;
            //}
        }
        private void ChangePrintoutData_Click(object sender, RoutedEventArgs e)
        {
            //PrintoutInfo DialogAddPrintout = new PrintoutInfo(DataGrid_ListOfPrintedDocument.SelectedItem as PrintoutData);

            //if (DialogAddPrintout.ShowDialog() == true)
            //{
            //    listOfPrintedDocuments[DataGrid_ListOfPrintedDocument.SelectedIndex] = DialogAddPrintout.printoutData;
            //    DataGrid_ListOfPrintedDocument.Items.Refresh();
            //}
            //SumPagesPrintouts = 0;
            //foreach (PrintoutData printoutData in listOfPrintedDocuments)
            //{
            //    SumPagesPrintouts += printoutData.CountPages;
            //}
            //Textblock_SumPagesPrintouts.Text = SumPagesPrintouts.ToString();
        }

        private void SaveRequestRefill_Click(object sender, RoutedEventArgs e)
        {
            //GenerateFiles(listOfPrintedDocuments);
            
        }
        private void DataGridListOfPrintedDocument_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (DataGrid_ListOfPrintedDocument.SelectedItems.Count == 1)
            {
                Button_Change.Visibility = Visibility.Visible;
                Button_Delete.Visibility = Visibility.Visible;
            }
            else if (DataGrid_ListOfPrintedDocument.SelectedItems.Count > 1)
            {
                Button_Change.Visibility = Visibility.Collapsed;
                Button_Delete.Visibility = Visibility.Visible;
            }
            else
            {
                Button_Change.Visibility = Visibility.Collapsed;
                Button_Delete.Visibility = Visibility.Collapsed;
            }
        }
        
    }
}
