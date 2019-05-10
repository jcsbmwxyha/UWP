using System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

// 內容對話方塊項目範本已記錄在 https://go.microsoft.com/fwlink/?LinkId=234238

namespace AuraEditor.Dialogs
{
    public sealed partial class DeviceUpdatePromptDialog : ContentDialog
    {
        public DeviceUpdatePromptDialog()
        {
            this.InitializeComponent();
        }

        private void OKBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            
            SystemNavigationManager.GetForCurrentView().BackRequested += MainPage.Self.OnBackRequested;
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            WindowsPage.Self.WindowsGrid.Visibility = Visibility.Collapsed;
            MainPage.Self.SettingBtnNewTab.Visibility = Visibility.Visible;
            WindowsPage.Self.WindowsFrame1.Navigate(typeof(SettingsPage), MainPage.Self.needToUpdate, new SuppressNavigationTransitionInfo());
            SettingsPage.Self.rootPivot.SelectedIndex = 0;
            WindowsPage.Self.WindowsGrid1.Visibility = Visibility.Visible;
            MainPage.Self.g_ContentDialog = null;
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            MainPage.Self.g_ContentDialog = null;
            MainPage.Self.SettingBtnNewTab.Visibility = Visibility.Visible;
        }
    }
}