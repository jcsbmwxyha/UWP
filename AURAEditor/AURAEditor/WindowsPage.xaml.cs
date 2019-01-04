using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Core.Preview;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// 空白頁項目範本已記錄在 https://go.microsoft.com/fwlink/?LinkId=234238

namespace AuraEditor
{
    /// <summary>
    /// 可以在本身使用或巡覽至框架內的空白頁面。
    /// </summary>
    public sealed partial class WindowsPage : Page
    {
        public bool needToUpdadte = false;

        ApplicationDataContainer g_EULASettings;
        public bool EulaAgreeOrNot = false;

        static WindowsPage _instance;
        static public WindowsPage Self
        {
            get { return _instance; }
        }
        public WindowsPage()
        {
            
            this.InitializeComponent();

            _instance = this;

            WindowsFrame.Navigated += WindowsFrame_Navigated;
            SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += this.OnCloseRequest;
            g_EULASettings = Windows.Storage.ApplicationData.Current.LocalSettings;
        }

        private async void WindowsPage_Loaded(object sender, RoutedEventArgs e)
        {
            #region App Title Bar
            // Hide default title bar.
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            UpdateTitleBarLayout(coreTitleBar);

            // Set XAML element as a draggable region.
            Window.Current.SetTitleBar(AppTitleBar);

            // Register a handler for when the size of the overlaid caption control changes.
            // For example, when the app moves to a screen with a different DPI.
            coreTitleBar.LayoutMetricsChanged += CoreTitleBar_LayoutMetricsChanged;

            // Register a handler for when the title bar visibility changes.
            // For example, when the title bar is invoked in full screen mode.
            coreTitleBar.IsVisibleChanged += CoreTitleBar_IsVisibleChanged;
            #endregion

            LoadingRing.IsActive = true;
            LoadingFrame.Visibility = Visibility.Visible;
            //Disable settings button until check finish
            SettingsRadioButton.IsEnabled = false;
            SettingsRadioButton.Opacity = 0.5;
            // disable end

            WindowsGrid.Visibility = Visibility.Visible;
            WindowsGrid1.Visibility = Visibility.Collapsed;
            WindowsFrame.Navigate(typeof(MainPage), null, new SuppressNavigationTransitionInfo());
            LoadEULASettings();

            await Task.Delay(4000);
            LoadingFrame.Visibility = Visibility.Collapsed;
            LoadingRing.IsActive = false;

            WindowsGrid.Visibility = Visibility.Collapsed;
            WindowsGrid1.Visibility = Visibility.Collapsed;

            #region Show EULA page
            await (new ServiceViewModel()).Sendupdatestatus("ASUSSYS");
            if (ServiceViewModel.returnnum == 1)//ASUS SYS
            {
                WindowsGrid.Visibility = Visibility.Visible;
                WindowsGrid1.Visibility = Visibility.Collapsed;
            }
            else//Other SYS show EULA page
            {
                if (EulaAgreeOrNot)
                {
                    WindowsGrid.Visibility = Visibility.Visible;
                    WindowsGrid1.Visibility = Visibility.Collapsed;
                }
                else
                {
                    //Hide Setting Button
                    SettingRelativePanel.Visibility = Visibility.Collapsed;
                    WindowsGrid.Visibility = Visibility.Collapsed;
                    WindowsGrid1.Visibility = Visibility.Visible;
                    WindowsFrame1.Navigate(typeof(EULAPage), null, new SuppressNavigationTransitionInfo());
                }
            }
            #endregion

            #region Check for Update and show icon
            await (new ServiceViewModel()).Sendupdatestatus("CreatorCheckVersion");
            // < 0 No checkallbyservice function
            if (ServiceViewModel.returnnum > 0)
            {
                //顯示需要更新
                SettingBtnNewTab.Visibility = Visibility.Visible;
                needToUpdadte = true;
            }
            else
            {
                SettingBtnNewTab.Visibility = Visibility.Collapsed;
                needToUpdadte = false;
            }
            //Enable settings button until check finish
            SettingsRadioButton.IsEnabled = true;
            SettingsRadioButton.Opacity = 1;
            //Enable end
            #endregion
        }

        private void WindowsFrame_Navigated(object sender, NavigationEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            foreach (var item in rootFrame.BackStack.ToList())
                rootFrame.BackStack.Remove(item);
        }

        private void CoreTitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            UpdateTitleBarLayout(sender);
        }

        private void UpdateTitleBarLayout(CoreApplicationViewTitleBar coreTitleBar)
        {
            // Get the size of the caption controls area and back button 
            // (returned in logical pixels), and move your content around as necessary.
            LeftPaddingColumn.Width = new GridLength(coreTitleBar.SystemOverlayLeftInset);
            RightPaddingColumn.Width = new GridLength(coreTitleBar.SystemOverlayRightInset);
            SettingRelativePanel.Margin = new Thickness(0, 0, coreTitleBar.SystemOverlayRightInset, 0);

            // Update title bar control size as needed to account for system size changes.
            AppTitleBar.Height = coreTitleBar.Height;
        }

        private void CoreTitleBar_IsVisibleChanged(CoreApplicationViewTitleBar sender, object args)
        {
            if (sender.IsVisible)
            {
                AppTitleBar.Visibility = Visibility.Visible;
            }
            else
            {
                AppTitleBar.Visibility = Visibility.Collapsed;
            }
        }

        private void SettingsRadioButton_Click(object sender, RoutedEventArgs e)
        {
            // Register a handler for BackRequested events and set the
            // visibility of the Back button
            SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            WindowsGrid.Visibility = Visibility.Collapsed;
            WindowsGrid1.Visibility = Visibility.Visible;
            WindowsFrame1.Navigate(typeof(SettingsPage), needToUpdadte, new SuppressNavigationTransitionInfo());
        }

        public void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            if (WindowsFrame1.Content is SettingsPage)
            {
                WindowsGrid.Visibility = Visibility.Visible;
                WindowsGrid1.Visibility = Visibility.Collapsed;
                SettingsRadioButton.IsChecked = false;
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
                SystemNavigationManager.GetForCurrentView().BackRequested -= OnBackRequested;
            }
            else
            {
                if (WindowsFrame1.CanGoBack)
                {
                    e.Handled = true;
                    WindowsFrame1.GoBack();
                }
            }
        }

        private void OnCloseRequest(object sender, SystemNavigationCloseRequestedPreviewEventArgs e)
        {
            SaveEULASettings();
        }

        private void SaveEULASettings()
        {
            g_EULASettings.Values["EULAAgree"] = EulaAgreeOrNot.ToString();
        }

        private void LoadEULASettings()
        {
            bool successful = bool.TryParse(g_EULASettings.Values["EULAAgree"] as string, out bool agree);
            if (successful)
                EulaAgreeOrNot = agree;
            else
            {
                EulaAgreeOrNot = false;
            }
        }
    }
}
