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
        public PrintoutData printoutData;
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
