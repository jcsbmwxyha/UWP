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

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace AuraEditor.Dialogs
{
    public sealed partial class YesNoCancelDialog : ContentDialog
    {
        public ContentDialogResult Result;

        public string DialogTitle {
            set
            {
                TitleTextBlock.Text = value;
            }
        }
        public string DialogContent {
            set
            {
                ContentTextBlock.Text = value;
            }
        }

        public string DialogYesButtonContent
        {
            set
            {
                OKButton.Content = value;
            }
        }
        public string DialogCancelButtonContent
        {
            set
            {
                CancelButton.Content = value;
            }
        }

        public YesNoCancelDialog()
        {
            Result = ContentDialogResult.None;
            this.InitializeComponent();
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            Result = ContentDialogResult.Primary;
            this.Hide();
            MainPage.Self.ShowDeviceUpdateDialogOrNot();
        }
        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            Result = ContentDialogResult.Secondary;
            this.Hide();
            MainPage.Self.CanShowDeviceUpdateDialog = true;
            MainPage.Self.ShowDeviceUpdateDialogOrNot();
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            MainPage.Self.CanShowDeviceUpdateDialog = true;
            MainPage.Self.ShowDeviceUpdateDialogOrNot();
        }

        private void YesNoCancelDialog_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Escape)
            {
                Result = ContentDialogResult.Secondary;
                this.Hide();
                MainPage.Self.CanShowDeviceUpdateDialog = true;
                MainPage.Self.ShowDeviceUpdateDialogOrNot();
            }
        }
    }
}
