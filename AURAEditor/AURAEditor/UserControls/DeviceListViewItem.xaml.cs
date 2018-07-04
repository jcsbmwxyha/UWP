using AuraEditor;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using AuraEditor.Common;

// 使用者控制項項目範本記載於 https://go.microsoft.com/fwlink/?LinkId=234236

namespace AuraEditor
{
    public sealed partial class DeviceListViewItem : UserControl
    {
        public DeviceItem MyDevice { get { return this.DataContext as DeviceItem; } }
        AddDeviceDialog addDeviceDialog;

        public DeviceListViewItem()
        {
            this.InitializeComponent();
            this.DataContextChanged += (s, e) => Bindings.Update();
            addDeviceDialog = AddDeviceDialog.GetInstance();
        }
        
        private void CustomizeButton_Click(object sender, RoutedEventArgs e)
        {
            addDeviceDialog.Closed += ShowZoneCustomizedDialog;
            addDeviceDialog.Hide();
        }

        private async void ShowZoneCustomizedDialog(ContentDialog sender, ContentDialogClosedEventArgs args)
        {
            addDeviceDialog.Closed -= ShowZoneCustomizedDialog;
            ZoneCustomizedDialog zoneCustomizedDialog =
                new ZoneCustomizedDialog(MyDevice);
            var result = await zoneCustomizedDialog.ShowAsync();
        }

        public string GetCheckedResult()
        {
            if (AllRadioButton.IsChecked == true)
            {
                return MyDevice.DeviceName;
            }
            else if (CustomizeRadioButton.IsChecked == true)
            {
                return MyDevice.DeviceName;
            }
            
            return null;
        }

        private void DeviceItemCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            // Initialization has not been completed
            if (MyDevice == null)
                return;

            RadioButton rb = sender as RadioButton;

            switch (rb.Name)
            {
                case "NoneRadioButton": MyDevice.Mode = 0; break;
                case "AllRadioButton": MyDevice.Mode = 1; break;
                case "CustomizeRadioButton": MyDevice.Mode = 2; break;
                default: MyDevice.Mode = 0; break;
            }
        }
    }
}
