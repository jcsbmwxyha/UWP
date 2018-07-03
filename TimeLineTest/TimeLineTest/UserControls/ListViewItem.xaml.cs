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

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace TimeLineTest.UserControls
{
    public sealed partial class ListViewItem : UserControl
    {
        public DeviceGroup MyDeviceGroup { get { return this.DataContext as DeviceGroup; } }

        public ListViewItem()
        {
            this.InitializeComponent();
        }
        public void UpdateContent()
        {
            Bindings.Update();
        }

        private void Test(object sender, RoutedEventArgs e)
        {
            Bindings.Update();
            Button b = sender as Button;
            b.IsEnabled = !b.IsEnabled;
        }
    }
}
