using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace InkTrack.Windows.ReplaceCartridgePages
{
    public partial class PageFullNameEnter : Page
    {
        public string FullName;
        ReplaceCartridge ParrentWindow;
        public PageFullNameEnter(ReplaceCartridge ParrentWindow)
        {
            this.ParrentWindow = ParrentWindow;
            InitializeComponent();
        }
        private void Next_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder stringBuilder = new StringBuilder();

            if (string.IsNullOrEmpty(TextBox_FirstName.Text) || string.IsNullOrWhiteSpace(TextBox_FirstName.Text))
            {
                stringBuilder.AppendLine("Поле \"Имя\" не должно быть пустым.");
            }
            if (string.IsNullOrEmpty(TextBox_LastName.Text) || string.IsNullOrWhiteSpace(TextBox_LastName.Text))
            {
                stringBuilder.AppendLine("Поле \"Фамилия\" не должно быть пустым.");
            }
            if (string.IsNullOrEmpty(TextBox_Patronymic.Text) || string.IsNullOrWhiteSpace(TextBox_Patronymic.Text))
            {
                stringBuilder.AppendLine("Поле \"Отчество\" не должно быть пустым.");
            }
            if(string.IsNullOrEmpty(stringBuilder.ToString()))
    {
                string FirstName = TextBox_FirstName.Text;
                string LastName = TextBox_LastName.Text;
                string Patronymic = TextBox_Patronymic.Text;
                FullName = $"{FirstName} {LastName} {Patronymic}";

                ParrentWindow.SetpageEnterInformationForReplaceCartridge();
            }
            else 
            {
                MessageBox.Show(stringBuilder.ToString(), "Ввод данных", MessageBoxButton.OK, MessageBoxImage.Warning);
            } 
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            string filteredText = new string(textBox.Text.Where(c => (c >= 'А' && c <= 'я') || c == 'ё' || c == 'Ё' || char.IsControl(c)).ToArray());

            if (textBox.Text != filteredText)
            {
                textBox.Text = filteredText;
                textBox.SelectionStart = textBox.Text.Length;
            }
        }
    }
}
