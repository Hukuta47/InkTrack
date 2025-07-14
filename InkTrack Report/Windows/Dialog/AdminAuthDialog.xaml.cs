using System;
using System.DirectoryServices.AccountManagement;
using System.Windows;
using System.Windows.Media.Animation;

namespace InkTrack.Windows.Dialog
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
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var fadeIn = new DoubleAnimation
            {
                From = 0.0,
                To = 1.0,
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut },
                Duration = new Duration(TimeSpan.FromSeconds(1))
            };
            this.BeginAnimation(OpacityProperty, fadeIn);
        }
        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            Textblock_Message.Visibility = Visibility.Hidden;
            Textblock_Message.Text = "";
            if (string.IsNullOrEmpty(Username))
            {
                Textblock_Message.Visibility = Visibility.Visible;
                Textblock_Message.Text = "Введите логин";
            }
            else
            {
                if (ValidateCredentials(Username, Password))
                {
                    DialogResult = true;
                }
                else
                {
                    Textblock_Message.Visibility = Visibility.Visible;
                    Textblock_Message.Text = "Не верные данные";
                }
            }
                
        }
        private bool ValidateCredentials(string username, string password)
        {
            try
            {
                // Если логин содержит \, значит это доменная учетка
                ContextType contextType = username.Contains("\\") ? ContextType.Domain : ContextType.Machine;

                // Удаляем домен из логина если есть
                string pureUsername = username.Contains("\\") ? username.Split('\\')[1] : username;

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
        }
    }
}