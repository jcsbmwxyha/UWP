using System.Collections.Generic;
using Windows.UI.Xaml.Controls;

// 空白頁項目範本已記錄在 https://go.microsoft.com/fwlink/?LinkId=234238

namespace AuraEditor.Dialogs
{
    public sealed partial class NamingDialog : ContentDialog
    {
        public string TheName { get; set; }
        public bool Result;
        private List<string> m_filenames;

        public NamingDialog(List<string> filenames)
        {
            this.InitializeComponent();
            m_filenames = filenames;
            Result = false;
            TheName = "";
        }
        public NamingDialog(string defaultName, List<string> filenames)
        {
            this.InitializeComponent();
            m_filenames = filenames;
            Result = false;
            TheName = defaultName;
        }

        private void OKButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (m_filenames.Contains(NamingTextBox.Text))
            {
                StatusTextBlock.Text = "Error: File name";
            }
            Result = true;
            this.Hide();
            MainPage.Self.CanShowDeviceUpdateDialog = true;
            MainPage.Self.ShowDeviceUpdateDialogOrNot();
        }
        private void CancelButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Result = false;
            this.Hide();
            MainPage.Self.CanShowDeviceUpdateDialog = true;
            MainPage.Self.ShowDeviceUpdateDialogOrNot();
        }

        private void NamingTextBox_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                Result = true;
                this.Hide();
            }
        }
    }
}
