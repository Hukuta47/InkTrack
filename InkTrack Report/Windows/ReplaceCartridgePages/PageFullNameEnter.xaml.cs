using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static InkTrack.Windows.ReplaceCartridge;

namespace InkTrack.Windows.ReplaceCartridgePages
{
    /// <summary>
    /// Логика взаимодействия для PageFullNameEnter.xaml
    /// </summary>
    public partial class PageFullNameEnter : Page
    {
        public string FullName;
        public PageFullNameEnter()
        {
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
            if (string.IsNullOrEmpty(stringBuilder.ToString()))
            {
                string FirstName = TextBox_FirstName.Text;
                string LastName = TextBox_LastName.Text;
                string Patronymic = TextBox_Patronymic.Text;

                FullName = $"{FirstName} {LastName} {Patronymic}";

                ReplaceCartridge.SetpageEnterInformationForReplaceCartridge();
            }
            else 
            {
                MessageBox.Show(stringBuilder.ToString());
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
