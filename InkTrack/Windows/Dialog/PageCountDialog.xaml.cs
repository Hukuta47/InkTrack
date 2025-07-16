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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace InkTrack.Windows.Dialog
{
    public partial class PageCountDialog : Window
    {
        public int? PageCount { get; private set; }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var fadeIn = new DoubleAnimation
            {
                From = 0.0,
                To = 1.0,
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut },
                Duration = new Duration(TimeSpan.FromSeconds(1)) // Можно изменить время
            };
            this.BeginAnimation(OpacityProperty, fadeIn);
        }
        private void PanelDrag_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed) DragMove();
        }
        public PageCountDialog(string documentName)
        {
            InitializeComponent();
            txtMessage.Text = $"Не удалось определить количество печатаемых страниц\nв документе \"{documentName}\".\nВведите количество вручную:";
        }
        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(TextBox_PageCount.Text.Trim(), out int result) && result > 0)
            {
                PageCount = result;
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Введите корректное положительное число.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
