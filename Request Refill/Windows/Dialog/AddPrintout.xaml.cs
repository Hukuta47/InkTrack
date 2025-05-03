using Request_Refill.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Request_Refill.Windows.Dialog
{
    public partial class AddPrintout : Window
    {
        public AddPrintout()
        {
            InitializeComponent();
            Button_AddPrintoutData.Content = "Добавить";
        }
        public AddPrintout(PrintoutData printoutData)
        {
            InitializeComponent();
            Button_AddPrintoutData.Content = "Сохранить";
        }
        private void AddPrintoutData_Click(object sender, RoutedEventArgs e)
        {
            PrintoutData printoutData = new PrintoutData()
            {
                NameDocument = Textbox_NameDocument.Text,
                CountPages = int.Parse(Textbox_CountPages.Text),
                date = DateTime.Parse(DatePicker_date.Text),
            };


            StringBuilder createtStr = new StringBuilder();
            createtStr.AppendLine();

            createtStr.AppendLine($"Имя документа: {printoutData.NameDocument}");
            createtStr.AppendLine($"Дата: {printoutData.CountPages}");
            createtStr.AppendLine($"Количесвто страниц: {printoutData.date}");



            MessageBox.Show(createtStr.ToString());
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
