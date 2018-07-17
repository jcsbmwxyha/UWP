using AuraEditor.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace AuraEditor
{
    public sealed partial class AddDeviceDialog : ContentDialog
    {
        public ObservableCollection<DeviceItem> DeviceList { get; set; }

        static AddDeviceDialog myInstance;
        public static AddDeviceDialog GetInstance()
        {
            return myInstance;
        }

        public AddDeviceDialog(ObservableCollection<DeviceItem> list)
        {
            this.InitializeComponent();
            myInstance = this;
            DeviceList = list;
        }

        private void AddDeviceDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var frame = (Frame)Window.Current.Content;
            var page = (MainPage)frame.Content;
            this.Closed += page.AddGroupFinished;
        }

        private void AddDeviceDialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        { }
    }
}
