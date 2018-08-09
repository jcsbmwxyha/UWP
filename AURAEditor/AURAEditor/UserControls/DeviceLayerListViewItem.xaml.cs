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

// 使用者控制項項目範本記載於 https://go.microsoft.com/fwlink/?LinkId=234236

namespace AuraEditor
{
    public sealed partial class DeviceLayerListViewItem : UserControl
    {
        public DeviceLayer MyDeviceLayer { get { return this.DataContext as DeviceLayer; } }
        
        public bool IsChecked {
            get {
                if (DeviceLayerRadioButton.IsChecked == true)
                    return true;
                else
                    return false;
            }
            set
            {
                DeviceLayerRadioButton.IsChecked = value;
            }
        }

        public DeviceLayerListViewItem()
        {
            this.InitializeComponent();
            this.DataContextChanged += (s, e) => Bindings.Update();
        }

        private void EyeToggleButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton tb = Common.ControlHelper.FindControl<ToggleButton>(this, typeof(ToggleButton), "EyeToggleButton");

            if (tb != null)
            {
                if (tb.IsChecked == false)
                    MyDeviceLayer.Eye = false;
                else
                    MyDeviceLayer.Eye = true;
            }
        }

        private void DeviceLayerRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            MainPage.MainPageInstance.WatchLayer(MyDeviceLayer);
        }
    }
}
