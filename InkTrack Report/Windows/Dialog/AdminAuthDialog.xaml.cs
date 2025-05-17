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
            UsernameTextBox.Focus();

        }
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = IsUserAdministrator(UsernameTextBox.Text, PasswordBox.Password);
            Close();
        }

        public bool IsUserAdministrator(string username, string password)
        {
            if (username.Contains("\\") || username.Contains("@"))
            {
                try
                {
                    PrincipalContext context;
                    string user;

                    // Проверяем, указано ли имя в формате domain\user
                    if (username.Contains("\\"))
                    {
                        var parts = username.Split('\\');
                        string domain = parts[0]; // Имя домена
                        user = parts[1];         // Имя пользователя
                                                 // Создаем контекст для домена
                        context = new PrincipalContext(ContextType.Domain, domain);
                    }
                    else
                    {
                        user = username;         // Считаем локальным пользователем
                                                 // Создаем контекст для локальной машины
                        context = new PrincipalContext(ContextType.Machine);
                    }

                    // Проверяем учетные данные
                    return context.ValidateCredentials(user, password);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка: {ex.Message}");
                    return false;
                }
            }
            else
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
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}