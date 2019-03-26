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
        private string DeviceName;
        public DeviceUpdatePromptDialog(string deviceName)
        {
            this.InitializeComponent();
            DeviceName = deviceName;
            InfoTextBlock.Text = "[" + DeviceName + "] Need to Update!";
            WindowsPage.Self.needToUpdadte = true;
        }

        private void OKBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            
            SystemNavigationManager.GetForCurrentView().BackRequested += WindowsPage.Self.OnBackRequested;
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            WindowsPage.Self.WindowsGrid.Visibility = Visibility.Collapsed;
            WindowsPage.Self.SettingBtnNewTab.Visibility = Visibility.Visible;
            WindowsPage.Self.WindowsFrame1.Navigate(typeof(SettingsPage), true, new SuppressNavigationTransitionInfo());
            SettingsPage.Self.rootPivot.SelectedIndex = 0;
            WindowsPage.Self.WindowsGrid1.Visibility = Visibility.Visible;
            MainPage.Self.g_ContentDialog = null;
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            MainPage.Self.g_ContentDialog = null;
            WindowsPage.Self.SettingBtnNewTab.Visibility = Visibility.Visible;
        }
    }
}