using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;

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
        public NavigationClass(Frame frame)
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
