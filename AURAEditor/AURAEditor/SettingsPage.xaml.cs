﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class SettingsPage : Page
    {
        static SettingsPage _instance;
        static public SettingsPage Self
        {
            get { return _instance; }
        }
        public SettingsPage()
        {
            this.InitializeComponent();
            _instance = this;

            NavigationCacheMode = NavigationCacheMode.Enabled;

            GeneralSettings.Navigate(typeof(GeneralSettingsPage));
            DeviceUpdates.Navigate(typeof(DeviceUpdatesPage));
            About.Navigate(typeof(AboutPage));
        }
    }
}
