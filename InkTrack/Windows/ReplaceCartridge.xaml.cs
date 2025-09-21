using InkTrack.Classes;
using InkTrack.Database;
using InkTrack.Helpers;
using InkTrack.Windows.ReplaceCartridgePages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Navigation;

namespace InkTrack.Windows
{
    public partial class ReplaceCartridge : System.Windows.Window
    {

        private PageFullNameEnter _pageFullName;
        private PageEnterInformationForReplaceCartridge _pageEIFRC;

        Device SelectedPrinter;
        private bool isNavigating;

        List<Cartridge> ListCartritgesForReplace;
        int SumPagesPrintouts;



        public ReplaceCartridge(bool userKnown)
        {
            InitializeComponent();

            Frame_PagesReplaceCartridge.Navigating += Frame_Navigating;
            Frame_PagesReplaceCartridge.Navigated += Frame_Navigated;

            switch (userKnown)
            {
                case true:
                    _pageEIFRC = new PageEnterInformationForReplaceCartridge(true, this);

                    Frame_PagesReplaceCartridge.Navigate(_pageEIFRC);
                    
                    break;
                case false:
                    _pageFullName = new PageFullNameEnter(this);
                    _pageEIFRC = new PageEnterInformationForReplaceCartridge(false, this);

                    Frame_PagesReplaceCartridge.Navigate(_pageFullName);
                    
                    break;
            }
        }
        public void SetpageEnterInformationForReplaceCartridge()
        {
            Frame_PagesReplaceCartridge.Navigate(_pageEIFRC);

        }
        public void SetpageFullName()
        {
            Frame_PagesReplaceCartridge.Navigate(_pageFullName);
        }




        public void BeginAnimationReplace()
        {


            var fadeSplash = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300)) { EasingFunction = new CubicEase()};
            var fadeWidth = new DoubleAnimation(Frame_PagesReplaceCartridge.ActualWidth, 302, TimeSpan.FromMilliseconds(300)) { EasingFunction = new CubicEase() };
            var fadeHeight = new DoubleAnimation(Frame_PagesReplaceCartridge.ActualHeight, 64, TimeSpan.FromMilliseconds(300)) { EasingFunction = new CubicEase() };
            System.Timers.Timer timer = new System.Timers.Timer(3000) { AutoReset = false };
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(300)) { EasingFunction = new CubicEase() };
            fadeOut.Completed += (s, _) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    this.Close();
                });
            };
            timer.Elapsed += (s, _) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    Border_Window.BeginAnimation(OpacityProperty, fadeOut);
                });
                
            };
            


            Border_Window.BeginAnimation(WidthProperty, fadeWidth);
            Border_Window.BeginAnimation(HeightProperty, fadeHeight);
            Border_SuccsesReplace.Visibility = Visibility.Visible;
            Border_SuccsesReplace.BeginAnimation(OpacityProperty, fadeSplash);
            timer.Start();
        }


        public void Replace_Click()
        {
            try
            {
                

                Cartridge cartridge = _pageEIFRC.SelectedPrinter.Printer.Cartridge;

                if (cartridge != null)
                {
                    string _FullName = App.LoginedEmployee?.FullName ?? _pageFullName.FullName;


                    List<PrintoutData> printoutDatas = DatabaseHelper.GetPrintOutDataList(_pageEIFRC.SelectedPrinter.Printer);
                    
                    string FullName = FullNameHelper.GetGenetiveFullName(_FullName);
                    string CartridgeNumber = _pageEIFRC.SelectedPrinter.Printer.Cartridge.Number;
                    string DeviceName = _pageEIFRC.SelectedPrinter.DeviceName;
                    string RoomName = _pageEIFRC.SelectedPrinter.Room.Name;
                    int sumPages = printoutDatas.Sum(s => s.CountPages);
                    string Suggection = string.Empty;

                    if (sumPages == 1) { Suggection = $"На картридже №{CartridgeNumber} была распечатана 1 страница"; }
                    else if (sumPages > 1) { Suggection = $"На картридже №{CartridgeNumber} было распечатано {sumPages} страниц"; }
                    else if (sumPages > 4) { Suggection = $"На картридже №{CartridgeNumber} было распечатано {sumPages} страниц"; }



                    new PdfHelper().GenerateRequestPDF(printoutDatas, _pageEIFRC.SelectedPrinter, _FullName);
                    new PdfHelper().GenerateResultPrintingFiles(DatabaseHelper.GetPrintOutDataList(_pageEIFRC.SelectedPrinter.Printer), _pageEIFRC.SelectedPrinter);

                    cartridge.Capacity = cartridge.Capacity <= SumPagesPrintouts ? SumPagesPrintouts : cartridge.Capacity;
                    cartridge.StatusId = 3;
                }

                _pageEIFRC.SelectedPrinter.Printer.CartridgeReplacementDate = DateTime.Now;

                Cartridge SelectedCartridge = _pageEIFRC.SelectedCartridgeToReplace;

                CartridgeReplacement_Log cartridgeReplacement_Log;
                if (_pageEIFRC.SelectedPrinter.Printer.Cartridge == null)
                {
                    cartridgeReplacement_Log = new CartridgeReplacement_Log()
                    {
                        OldCartridgeId = null,
                        NewCartridgeId = SelectedCartridge.Id,
                        PrinterId = _pageEIFRC.SelectedPrinter.Id,
                        EmployeeLitId = _pageEIFRC.WhoReplacedId,
                        ReasonId = _pageEIFRC.ReasonToReplaceId
                    };
                }
                else
                {
                    cartridgeReplacement_Log = new CartridgeReplacement_Log()
                    {
                        OldCartridgeId = _pageEIFRC.SelectedPrinter.Printer.Cartridge.Id,
                        NewCartridgeId = SelectedCartridge.Id,
                        PrinterId = _pageEIFRC.SelectedPrinter.Id,
                        EmployeeLitId = _pageEIFRC.WhoReplacedId,
                        ReasonId = _pageEIFRC.ReasonToReplaceId
                    };
                }
                App.entities.CartridgeReplacement_Log.Add(cartridgeReplacement_Log);
                _pageEIFRC.SelectedPrinter.Printer.Cartridge = SelectedCartridge;
                SelectedCartridge.StatusId = 1;

                DatabaseHelper.ResetPrintoutDataHistory(_pageEIFRC.SelectedPrinter.Printer);


                BeginAnimationReplace();
            }
            catch (Exception ex)
            {
                Logger.Log("Error", "Получено исключение", ex);
            }
        }


        private void Frame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (isNavigating)
                return;

            e.Cancel = true;
            isNavigating = true;

            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(300));
            fadeOut.Completed += (s, _) =>
            {
                Frame_PagesReplaceCartridge.Navigate(e.Content);
            };

            Frame_PagesReplaceCartridge.BeginAnimation(OpacityProperty, fadeOut);
        }
        private void Frame_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300));
            fadeIn.Completed += (s, _) => isNavigating = false;
            Frame_PagesReplaceCartridge.BeginAnimation(OpacityProperty, fadeIn);
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
        private void PanelDrag_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed) DragMove();
        }
        private void CloseWindow_Click(object sender, RoutedEventArgs e) => Close();

        private void FAQ_Click(object sender, RoutedEventArgs e)
        {
            switch (((Page)Frame_PagesReplaceCartridge.Content).Title)
            {
                case "pageFullName":
                    System.Windows.MessageBox.Show("Данная страница предназначена для заполнения ФИО, которое будет отображаться в заявке.", "Справка", (MessageBoxButton)MessageBoxButtons.OK, MessageBoxImage.Information);
                    break;
                case "PageEIFRC":
                    System.Windows.MessageBox.Show("Данная страница предназначена для заполнения данных замены картриджа.", "Справка", (MessageBoxButton)MessageBoxButtons.OK, MessageBoxImage.Information);
                    break;
            }
        }
    }
}
