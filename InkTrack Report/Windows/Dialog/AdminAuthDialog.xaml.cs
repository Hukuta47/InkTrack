using System;
using System.DirectoryServices.AccountManagement;
using System.Security.Principal;
using System.Windows;

namespace InkTrack_Report.Windows.Dialog
{
    public partial class AdminAuthDialog : Window
    {
        public AdminAuthDialog()
        {
            InitializeComponent();
            Textbox_Login.Focus();

        }
        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = IsUserAdministrator(Textbox_Login.Text, Passwordbox_Password.Password);
            Close();
        }
        public bool IsUserAdministrator(string username, string password)
        {
            try
            {
                // Создаем контекст для локальной машины
                using (PrincipalContext context = new PrincipalContext(ContextType.Machine))
                {
                    // Проверяем учетные данные
                    if (context.ValidateCredentials(username, password))
                    {
                        // Находим пользователя
                        using (UserPrincipal user = UserPrincipal.FindByIdentity(context, username))
                        {
                            if (user != null)
                            {
                                // Получаем группы, в которые входит пользователь
                                var groups = user.GetAuthorizationGroups();
                                foreach (var group in groups)
                                {
                                    // Проверяем SID группы администраторов
                                    if (group.Sid == new SecurityIdentifier("S-1-5-32-544"))
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Обработка исключений
                Console.WriteLine(ex.Message);
            }
            return false;
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}