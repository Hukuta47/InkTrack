using System;
using System.Windows;
using System.Windows.Threading;

namespace EduCall_Windows_app_OCW__One_Computer_Work_
{
    public partial class MainWindow : Window
    {
        DispatcherTimer timerRealTime;
        public MainWindow()
        {
            InitializeComponent();
            Textblock_RealTime.Text = DateTime.Now.ToString();
            timerRealTime = new DispatcherTimer();
            timerRealTime.Tick += ChangeTextbox_realTime;
            timerRealTime.Start();
        }
        void ChangeTextbox_realTime(object sender, EventArgs e)
        {
            string text = $"{DateTime.Now.Hour}:{DateTime.Now.Minute}:{DateTime.Now.Second}";

            Textblock_RealTime.Text = text;
        }
    }
}
