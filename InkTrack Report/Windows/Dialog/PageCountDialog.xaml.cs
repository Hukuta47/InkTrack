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

namespace InkTrack_Report.Windows.Dialog
{
    public partial class PageCountDialog : Window
    {
        public int? PageCount { get; private set; }

        public PageCountDialog(string documentName)
        {
            InitializeComponent();
            txtMessage.Text = $"Не удалось определить количество страниц в документе \"{documentName}\".\nВведите количество вручную:";
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(txtPageCount.Text.Trim(), out int result) && result > 0)
            {
                PageCount = result;
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Введите корректное положительное число.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
