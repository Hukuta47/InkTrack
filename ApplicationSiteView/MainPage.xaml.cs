using System;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace ApplicationSiteView
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a <see cref="Frame">.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        // Блокировка сторонних доменов
        private void WebView_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            if (!IsValidUri(args.Uri))
            {
                args.Cancel = true; // Отмена навигации
            }
        }

        // Блокировка новых окон (поп-апы и ссылки с target="_blank")
        private void WebView_NewWindowRequested(WebView sender, WebViewNewWindowRequestedEventArgs args)
        {
            args.Handled = true; // Запрет открытия
        }

        // Проверка URL на соответствие домену и протоколу
        private bool IsValidUri(Uri uri)
        {
            return uri.Host.Equals("bbb.zabgc.ru", StringComparison.OrdinalIgnoreCase)
                && uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase);
        }
    }
}
