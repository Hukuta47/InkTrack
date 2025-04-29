using Request_Refill.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Request_Refill.Windows
{
    /// <summary>
    /// Логика взаимодействия для WindowSettings.xaml
    /// </summary>
    public partial class WindowSettings : Window
    {
        public WindowSettings()
        {
            InitializeComponent();
            ComboboxCabinetSelect.ItemsSource = App.dBEntities.vCabinetPrinters.ToList();
        }
        private void ClickCloseWindow(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ComboboxCabinetSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            MessageBox.Show(ComboboxCabinetSelect.SelectedValuePath);
            //ComboboxFromWhoDefaultSelect.ItemsSource = App.dBEntities.GetEmployeesByCabinet(ComboboxCabinetSelect.SelectedItem);
        }
    }
}
