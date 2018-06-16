using System;
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
using AuraEditor.Common;

// 空白頁項目範本已記錄在 https://go.microsoft.com/fwlink/?LinkId=234238

namespace AuraEditor
{
    /// <summary>
    /// 可以在本身使用或巡覽至框架內的空白頁面。
    /// </summary>
    public sealed partial class NamedDialog : ContentDialog
    {
        public string CustomizeName { get; set; }
        //ZoneItem customizingZone;
        DeviceListViewItem customizingItem;

        public NamedDialog(DeviceListViewItem dlvi)
        {
            this.InitializeComponent();
            //customizingZone = z;
            customizingItem = dlvi;
            CustomizeName = "";
        }

        private void NamedDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            //this.Closed += ShowAddDeviceDialog;
        }

        private async void ShowAddDeviceDialog(ContentDialog sender, ContentDialogClosedEventArgs args)
        {
            AddDeviceDialog addDeviceDialog = AddDeviceDialog.GetInstance();
            //customizingZone.Name = CustomizeName;
            //customizingItem.MyDevice.Zones.Add(customizingZone);
            var result = await addDeviceDialog.ShowAsync();
        }

        private void NamedDialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            //this.Closed += ShowAddDeviceDialogWithCancel;
        }

        private async void ShowAddDeviceDialogWithCancel(ContentDialog sender, ContentDialogClosedEventArgs args)
        {
            AddDeviceDialog addDeviceDialog = AddDeviceDialog.GetInstance();
            var result = await addDeviceDialog.ShowAsync();
        }
    }
}
