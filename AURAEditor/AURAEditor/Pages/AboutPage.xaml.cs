using Windows.ApplicationModel;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;

// 空白頁項目範本已記錄在 https://go.microsoft.com/fwlink/?LinkId=234238

namespace AuraEditor
{
    /// <summary>
    /// 可以在本身使用或巡覽至框架內的空白頁面。
    /// </summary>
    public sealed partial class AboutPage : Page
    {
        public AboutPage()
        {
            this.InitializeComponent();
            AppVersionTextBlock.Text = GetAppVersion();
        }

        private void EULATextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            WindowsPage.Self.WindowsFrame1.Navigate(typeof(EULAPage), null, new SuppressNavigationTransitionInfo());
        }

        public static string GetAppVersion()
        {

            Package package = Package.Current;
            PackageId packageId = package.Id;
            PackageVersion version = packageId.Version;

            return string.Format("Ver {0}.{1}.{2}", version.Major, version.Minor, version.Build);

        }
    }
}
