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
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// 內容對話方塊項目範本已記錄在 https://go.microsoft.com/fwlink/?LinkId=234238

namespace AuraEditor.Dialogs
{
    public sealed partial class EULADisagreeDialog : ContentDialog
    {
        public EULADisagreeDialog()
        {
            this.InitializeComponent();
        }

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            CloseApp();
        }

        private async void BackEULABtn_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            EULADialog ed = new EULADialog();
            await ed.ShowAsync();
        }

        public void CloseApp()
        {
            WindowsPage.Self.SaveEULASettings();
            Application.Current.Exit();
        }
    }
}
