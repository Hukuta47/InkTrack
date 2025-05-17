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

namespace InkTrack_Report.Windows
{
    /// <summary>
    /// Логика взаимодействия для Error.xaml
    /// </summary>
    public partial class Error : Window
    {
        public string ErrorMessage { get; }
        public Error(Exception ex)
        {
            InitializeComponent();
            DataContext = this;
            ErrorMessage = $"{ex.Message}\n\n{ex.StackTrace}";
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
