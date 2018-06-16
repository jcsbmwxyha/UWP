using System;
using Windows.UI.Xaml.Controls;

// 空白頁項目範本已記錄在 https://go.microsoft.com/fwlink/?LinkId=234238

namespace ColorPickerTest
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void ShowDialog_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            MyColorPicker ColorPickerDialog = new MyColorPicker(Windows.UI.Colors.Yellow);
            await ColorPickerDialog.ShowAsync();
        }
    }
}
