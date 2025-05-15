using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Request_Refill.SetupWizardSettingsPages
{
    public class NavigationClass
    {
        private Frame _frame;
        int numPage = 0;
        List<Page> list = new List<Page>()
        {
            new SetupWizardSettings_Page1(),
            new SetupWizardSettings_Page2(),
            new SetupWizardSettings_Page3()
        };
        public NavigationClass(Frame frame, Window window)
        {
            _frame = frame;
            _frame.Navigate(list[numPage]);
        }
        public void nextPage()
        {
            numPage++;
            _frame.Navigate(list[numPage]);
        }
        public void prevPage()
        {
            numPage++;
            _frame.Navigate(list[numPage]);
        }
    }
}
