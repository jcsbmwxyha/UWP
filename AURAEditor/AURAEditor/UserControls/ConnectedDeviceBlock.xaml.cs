using System;
using System.ComponentModel;
using System.IO;
using Windows.Networking.Sockets;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using static AuraEditor.Common.ControlHelper;
using AuraEditor.Dialogs;

// 使用者控制項項目範本記載於 https://go.microsoft.com/fwlink/?LinkId=234236

namespace AuraEditor.UserControls
{
    public sealed partial class ConnectedDeviceBlock : UserControl
    {
        public SyncDevice MyDevice { get { return this.DataContext as SyncDevice; } }
        public ConnectedDeviceBlock()
        {
            this.InitializeComponent();
            this.DataContextChanged += (s, e) => Bindings.Update();
        }

        public void Update()
        {
            Bindings.Update();
            ConnectedDevicesDialog.Self.UpdateSelectedText();
        }

        private void DeviceToggleButton_Checked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            MyDevice.Sync = true;
            Update();
        }

        private void DeviceToggleButton_Unchecked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            MyDevice.Sync = false;
            Update();
        }
    }
}
