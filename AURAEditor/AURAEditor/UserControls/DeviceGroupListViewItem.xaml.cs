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
    public sealed partial class DeviceGroupListViewItem : UserControl
    {
        public DeviceGroup MyDeviceGroup { get { return this.DataContext as DeviceGroup; } }
        public bool IsSelected {
            get {
                if (GroupLayerRadioButton.IsChecked == true)
                    return true;
                else
                    return false;
            }
        }

        public DeviceGroupListViewItem()
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
                    MyDeviceGroup.Eye = false;
                else
                    MyDeviceGroup.Eye = true;
            }
        }

        private void GroupLayerRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            var frame = (Frame)Window.Current.Content;
            var page = (MainPage)frame.Content;
            page.UpdateSpaceGrid(MyDeviceGroup);
        }
    }
}
