using InkTrack_Report.Database;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace InkTrack_Report.Windows
{
    public partial class SettingsSetupWizard : Window
    {
        public SettingsSetupWizard(bool isFirstSetup)
        {
            InitializeComponent();
            if (isFirstSetup)
            {
                Combobox_SelectCabinet.ItemsSource = App.entities.Cabinet.Where(c => c.Device.Any(d => d.DeviceTypeID == 2)).ToList();
                Combobox_SelectCabinet.SelectionChanged += ComboboxCabinetSelect_SelectionChanged;
            }
            else
            {
                Combobox_SelectCabinet.ItemsSource = App.entities.Cabinet.Where(c => c.Device.Any(d => d.DeviceTypeID == 2)).ToList();
                Combobox_SelectCabinet.SelectedValue = Properties.Settings.Default.SelectedCabinetID;
                Combobox_SelectEmployee.ItemsSource = (Combobox_SelectCabinet.SelectedItem as Cabinet).Employee;
                Combobox_SelectEmployee.SelectedValue = Properties.Settings.Default.SelectedEmployeeID;
                Combobox_SelectEmployee.ItemsSource = (Combobox_SelectCabinet.SelectedItem as Cabinet).Employee;
                Combobox_SelectPrinter.SelectedIndex = 0;
            }
                
        }
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
        private void ComboboxCabinetSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Combobox_SelectEmployee.IsEnabled = (Combobox_SelectCabinet.SelectedItem as Cabinet).Employee.Count > 1 ? true : false;
            Combobox_SelectEmployee.ItemsSource = (Combobox_SelectCabinet.SelectedItem as Cabinet).Employee;
            Combobox_SelectPrinter.IsEnabled = (Combobox_SelectCabinet.SelectedItem as Cabinet).Device.Count > 1 ? true : false;
            Combobox_SelectPrinter.ItemsSource = (Combobox_SelectCabinet.SelectedItem as Cabinet).Device.Where(d => d.DeviceTypeID == 2);

            Combobox_SelectEmployee.SelectedIndex = 0;
            Combobox_SelectPrinter.SelectedIndex = 0;
        }
        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.SelectedCabinetID = (int)Combobox_SelectCabinet.SelectedValue;
            Properties.Settings.Default.SelectedEmployeeID = (int)Combobox_SelectEmployee.SelectedValue;
            Properties.Settings.Default.SelectedPrinterID = (int)Combobox_SelectPrinter.SelectedValue;
            Properties.Settings.Default.isFirstStartup = false;

            Properties.Settings.Default.Save();
            DialogResult = true;
        }
        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            switch (MessageBox.Show(
                "Уверены что хотите прервать настройку? " +
                "\nПри следующем запуске программы, вам в любом случае придется провести эту процедуру", "Предупреждение", MessageBoxButton.YesNo, MessageBoxImage.Question))
            {
                case MessageBoxResult.Yes:
                    App.Current.Shutdown();
                    break;
                case MessageBoxResult.No:
                    return;
            }
        }
        private void PanelDrag_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed) DragMove();
        }
    }
}
