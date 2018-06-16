using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
namespace Ch0904
{
    public sealed partial class MainPage : Page
    {
        MyColors textcolor = new MyColors();

        public MainPage()
        {
            this.InitializeComponent();

            // 設定筆刷顏色Brush1為紅色
            tb.Text = "文字顏色設為紅色 !";
            textcolor.Brush1 = new SolidColorBrush(Colors.Red);
            tb.DataContext = textcolor;
            ShowMess("文字顏色設為紅色 !");
        }
        async private void ShowMess(string res)
        {
            var messDialog = new MessageDialog(res);
            await messDialog.ShowAsync();
        }
        private void btCol_Click(object sender, RoutedEventArgs e)
        {  // 改變筆刷顏色Brush1為藍色
            tb.Text = "文字顏色改變為藍色 !";
            textcolor.Brush1 = new SolidColorBrush(Colors.Blue);
            tb.DataContext = textcolor;
        }
    }
}