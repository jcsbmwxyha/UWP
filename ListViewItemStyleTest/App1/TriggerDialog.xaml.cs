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
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// 內容對話方塊項目範本已記錄在 https://go.microsoft.com/fwlink/?LinkId=234238

namespace App1
{
    public sealed partial class TriggerDialog : ContentDialog
    {
        ObservableCollection<string> collection;

        public TriggerDialog()
        {
            this.InitializeComponent();
            collection = new ObservableCollection<string>();
            collection.Add("123");
            collection.Add("456");
            collection.Add("789");
            TriggerEffectListView.ItemsSource = collection;
        }
    }
}
