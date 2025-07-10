using InkTrack_Report.Database;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace InkTrack_Report.Windows.Dialog
{
    public partial class GetEmployeeIdDialog : Window
    {
        public int EmployeeId = 0;
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
        public GetEmployeeIdDialog()
        {
            InitializeComponent();
            ListBox_SelectEmployee.ItemsSource = App.entities.Employee.ToList();
        }
        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            EmployeeId = (int)ListBox_SelectEmployee.SelectedValue;
            DialogResult = true;
        }

        private void Textbox_Search_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            ListBox_SelectEmployee.ItemsSource = App.entities.Employee.Where(Employee => Employee.FullName.Contains(Textbox_Search.Text)).ToList();
        }
    }
}
