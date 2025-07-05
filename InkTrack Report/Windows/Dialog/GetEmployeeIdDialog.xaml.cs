using System.Linq;
using System.Windows;

namespace InkTrack_Report.Windows.Dialog
{
    public partial class GetEmployeeIdDialog : Window
    {
        public int EmployeeId = 0;
        public GetEmployeeIdDialog()
        {
            InitializeComponent();
            Combobox_SelectEmployee.ItemsSource = App.entities.Employee.ToList();
        }
        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            EmployeeId = (int)Combobox_SelectEmployee.SelectedValue;
            DialogResult = true;
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
