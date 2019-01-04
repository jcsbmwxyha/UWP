using System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using AuraEditor.Dialogs;

// 空白頁項目範本已記錄在 https://go.microsoft.com/fwlink/?LinkId=234238

namespace AuraEditor
{
    /// <summary>
    /// 可以在本身使用或巡覽至框架內的空白頁面。
    /// </summary>
    public sealed partial class EULAPage : Page
    {
        public EULAPage()
        {
            this.InitializeComponent();
        }

        private void AgreeBtn_Click(object sender, RoutedEventArgs e)
        {
            WindowsPage.Self.EulaAgreeOrNot = true;
            WindowsPage.Self.WindowsGrid.Visibility = Visibility.Visible;
            WindowsPage.Self.WindowsGrid1.Visibility = Visibility.Collapsed;
            WindowsPage.Self.SettingRelativePanel.Visibility = Visibility.Visible;
            WindowsPage.Self.SettingsRadioButton.IsChecked = false;
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            SystemNavigationManager.GetForCurrentView().BackRequested -= WindowsPage.Self.OnBackRequested;
        }

        private async void DisagreeBtn_Click(object sender, RoutedEventArgs e)
        {
            WindowsPage.Self.EulaAgreeOrNot = false;
            WindowsPage.Self.WindowsGrid.Visibility = Visibility.Visible;
            WindowsPage.Self.WindowsGrid1.Visibility = Visibility.Collapsed;
            WindowsPage.Self.SettingRelativePanel.Visibility = Visibility.Visible;
            WindowsPage.Self.SettingsRadioButton.IsChecked = false;
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            SystemNavigationManager.GetForCurrentView().BackRequested -= WindowsPage.Self.OnBackRequested;
            EULADisagreeDialog edcd = new EULADisagreeDialog();
            await edcd.ShowAsync();
        }
    }
}
