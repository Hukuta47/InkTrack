using InkTrack_Report.Database;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace InkTrack_Report.Windows
{
    public partial class ReplaceCartridge : Window
    {
        int SelectedPrinterID = Properties.Settings.Default.SelectedPrinterID;

        public ReplaceCartridge()
        {
            InitializeComponent();
        }
        private void PanelDrag_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed) DragMove();
        }
        private void CloseWindow_Click() => Close();
        private void CloseWindow_Click(object sender, RoutedEventArgs e) => Close();
        private void Cancel_Click(object sender, RoutedEventArgs e) => CloseWindow_Click();

        private void Save_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
