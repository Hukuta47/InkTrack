using System.DirectoryServices.AccountManagement;
using System.Windows;

namespace InkTrack_Report.Windows.Dialog
{
    public partial class AdminAuthDialog : Window
    {
        public string Username => Textbox_Login.Text;
        public string Password => Passwordbox_Password.Password;
        public AdminAuthDialog()
        {
            InitializeComponent();
            Textbox_Login.Focus();
        }
        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
            {
                MessageBox.Show("Введите логин и пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            DialogResult = ValidateCredentials(Username, Password);
            Close();
        }
        private bool ValidateCredentials(string username, string password)
        {
            try
            {
                // Если логин содержит \, значит это доменная учетка
                ContextType contextType = username.Contains("\\") ?
                    ContextType.Domain : ContextType.Machine;

                // Удаляем домен из логина если есть
                string pureUsername = username.Contains("\\") ?
                    username.Split('\\')[1] : username;

                using (var context = new PrincipalContext(contextType))
                {
                    return context.ValidateCredentials(pureUsername, password);
                }
            }
            catch
            {
                return false;
            }
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}