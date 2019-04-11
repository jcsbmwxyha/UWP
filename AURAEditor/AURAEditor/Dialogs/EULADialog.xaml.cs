using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using static AuraEditor.Common.ControlHelper;

// 內容對話方塊項目範本已記錄在 https://go.microsoft.com/fwlink/?LinkId=234238

namespace AuraEditor.Dialogs
{
    public sealed partial class EULADialog : ContentDialog
    {
        public EULADialog()
        {
            this.InitializeComponent();
        }

        private void AgreeBtn_Click(object sender, RoutedEventArgs e)
        {
            WindowsPage.Self.EulaAgreeOrNot = true;
            this.Hide();

            if (!WindowsPage.Self.TutorialDoneOrNot)
            {
                MainPage.Self.ShowTutorialDialogOrNot();
            }
            else
            {
                MainPage.Self.CanShowDeviceUpdateDialog = true;
                MainPage.Self.ShowDeviceUpdateDialogOrNot();
            }
        }

        private async void DisagreeBtn_Click(object sender, RoutedEventArgs e)
        {
            WindowsPage.Self.EulaAgreeOrNot = false;
            this.Hide();
            EULADisagreeDialog edcd = new EULADisagreeDialog();
            await edcd.ShowAsync();
        }
    }
}
