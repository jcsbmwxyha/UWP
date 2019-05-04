using System.Collections.Generic;
using System.Text.RegularExpressions;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Controls;

// 空白頁項目範本已記錄在 https://go.microsoft.com/fwlink/?LinkId=234238

namespace AuraEditor.Dialogs
{
    public sealed partial class NamingDialog : ContentDialog
    {
        public string TheName { get; set; }
        public bool NamingCancel = false;
        public bool Result;
        private List<string> m_filenames;

        private ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView();

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
            Regex regex = new Regex(@"[\\/\:\*\?\<\>\|]");
            MatchCollection matches = regex.Matches(NamingTextBox.Text);
            if (matches.Count > 0)
            {
                StatusTextBlock.Text = resourceLoader.GetString("StatusTextBlock_SpecialChar");
            }
            else if (NamingTextBox.Text.Contains("\"") || NamingTextBox.Text.Contains(" "))
            {
                StatusTextBlock.Text = resourceLoader.GetString("StatusTextBlock_SpecialChar");
            }
            else if (m_filenames.Contains(NamingTextBox.Text))
            {
                StatusTextBlock.Text = resourceLoader.GetString("StatusTextBlock_Exist");
            }
            else
            {
                Result = true;
                NamingCancel = false;
                this.Hide();
            }
            MainPage.Self.CanShowDeviceUpdateDialog = true;
            MainPage.Self.ShowDeviceUpdateDialogOrNot();
        }
        private void CancelButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Result = false;
            NamingCancel = true;
            this.Hide();
            MainPage.Self.CanShowDeviceUpdateDialog = true;
            MainPage.Self.ShowDeviceUpdateDialogOrNot();
        }

        private void NamingTextBox_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            switch (e.Key)
            {
                case Windows.System.VirtualKey.Enter:
                    Regex regex = new Regex(@"[\\/\:\*\?\<\>\|]");
                    MatchCollection matches = regex.Matches(NamingTextBox.Text);
                    if (matches.Count > 0)
                    {
                        StatusTextBlock.Text = resourceLoader.GetString("StatusTextBlock_SpecialChar");
                    }
                    else if (NamingTextBox.Text.Contains("\"") || NamingTextBox.Text.Contains(" "))
                    {
                        StatusTextBlock.Text = resourceLoader.GetString("StatusTextBlock_SpecialChar");
                    }
                    else if (m_filenames.Contains(NamingTextBox.Text))
                    {
                        StatusTextBlock.Text = resourceLoader.GetString("StatusTextBlock_Exist");
                    }
                    else
                    {
                        Result = true;
                        this.Hide();
                    }
                    MainPage.Self.CanShowDeviceUpdateDialog = true;
                    MainPage.Self.ShowDeviceUpdateDialogOrNot();
                    break;
                case Windows.System.VirtualKey.Escape:
                    Result = false;
                    this.Hide();
                    MainPage.Self.CanShowDeviceUpdateDialog = true;
                    MainPage.Self.ShowDeviceUpdateDialogOrNot();
                    break;
            }
        }

        private void NamingDialog_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Escape)
            {
                Result = false;
                this.Hide();
                MainPage.Self.CanShowDeviceUpdateDialog = true;
                MainPage.Self.ShowDeviceUpdateDialogOrNot();
            }
        }

        private void NamingTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(NamingTextBox.Text))
            {
                OKButton.IsEnabled = true;
            }
            else
            {
                OKButton.IsEnabled = false;
            }
        }
    }
}
