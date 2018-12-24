using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

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
            WindowsPage.Self.MaskFrame.Visibility = Visibility.Collapsed;
            WindowsPage.Self.WindowsGrid.Opacity = 1;
            WindowsPage.Self.WindowsGrid1.Visibility = Visibility.Collapsed;
            WindowsPage.Self.SettingRelativePanel.Visibility = Visibility.Visible;
            WindowsPage.Self.SettingsRadioButton.IsChecked = false;
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            SystemNavigationManager.GetForCurrentView().BackRequested -= WindowsPage.Self.OnBackRequested;
        }

        private void DisagreeBtn_Click(object sender, RoutedEventArgs e)
        {
            WindowsPage.Self.EulaAgreeOrNot = false;
            WindowsPage.Self.WindowsGrid.Visibility = Visibility.Visible;
            WindowsPage.Self.MaskFrame.Visibility = Visibility.Visible;
            WindowsPage.Self.WindowsGrid.Opacity = 0.5;
            WindowsPage.Self.WindowsGrid1.Visibility = Visibility.Collapsed;
            WindowsPage.Self.SettingRelativePanel.Visibility = Visibility.Visible;
            WindowsPage.Self.SettingsRadioButton.IsChecked = false;
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            SystemNavigationManager.GetForCurrentView().BackRequested -= WindowsPage.Self.OnBackRequested;
        }
    }
}
