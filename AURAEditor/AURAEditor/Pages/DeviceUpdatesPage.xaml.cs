using System;
using Windows.Networking.Connectivity;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using AuraEditor.Dialogs;
using Windows.System.Power;
using Windows.UI.Core;
using Windows.ApplicationModel.Core;
using Windows.Devices.Power;
using Windows.UI;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;
using static AuraEditor.Common.AuraEditorColorHelper;

// 空白頁項目範本已記錄在 https://go.microsoft.com/fwlink/?LinkId=234238

namespace AuraEditor
{
    /// <summary>
    /// 可以在本身使用或巡覽至框架內的空白頁面。
    /// </summary>
    public sealed partial class DeviceUpdatesPage : Page
    {
        private bool UpdateBtnMode;
        private bool IsCharge = false;

        public ConnectedDevicesDialog ConnectedDevicesDialog;
        public DeviceUpdatesPage()
        {
            this.InitializeComponent();

            ConnectedDevicesDialog = new ConnectedDevicesDialog();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is bool)
            {
                UpdateBtnMode = (bool)e.Parameter;
                if ((bool)e.Parameter)
                {
                    UpdateButton.Content = "Update now";
                    UpdateBtnNewTab.Visibility = Visibility.Visible;
                    UpdateStateTextBlock.Text = "A new version is available.";
                    InfoTextBlock.Text = "When updating , make sure your computer is plugged to a power source and do not turn it off until the update process is complete.";

                    PowerManager.PowerSupplyStatusChanged += PowerState_ReportUpdated;
                    Battery.AggregateBattery.ReportUpdated += AggregateBattery_ReportUpdated;
                    GetPowerStateChange();
                }
                else
                {
                    UpdateButton.Content = "Check for updates";
                    UpdateBtnNewTab.Visibility = Visibility.Collapsed;
                    UpdateStateTextBlock.Text = "Your version is up to date.";
                    InfoTextBlock.Text = "Update for the latest AURA functions and firmware";
                }
            }
            else
            {
                return;
            }
            base.OnNavigatedTo(e);
        }

        private async void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (GetInternetConnectstate())
            {
                if (UpdateBtnMode)
                {
                    //Update Now
                    PowerManager.PowerSupplyStatusChanged -= PowerState_ReportUpdated;
                    Battery.AggregateBattery.ReportUpdated -= AggregateBattery_ReportUpdated;
                    InfoTextBlock.Visibility = Visibility.Collapsed;
                    InfoTextStack.Visibility = Visibility.Visible;
                    ProgressBar.Visibility = Visibility.Visible;
                    UpdateBtnRP.Visibility = Visibility.Collapsed;
                    UpdateStateTextBlock.Text = "Updating...";
                    UpdateStateTextBlock.Foreground = new SolidColorBrush(Colors.White);

                    int old_value = 0;
                    await (new ServiceViewModel()).Sendupdatestatus("CreatorUpdate");
                    while (true)
                    {
                        await (new ServiceViewModel()).Sendupdatestatus(ServiceViewModel.returnnum.ToString());
                        if (old_value < ServiceViewModel.returnnum)
                        {
                            ProgressBar.Value = ServiceViewModel.returnnum;
                            old_value = ServiceViewModel.returnnum;
                        }
                        else
                        {
                            old_value = ServiceViewModel.returnnum;
                        }
                        if (ServiceViewModel.returnnum == 100)
                        {
                            await Task.Delay(2000);
                            ProgressBar.Visibility = Visibility.Collapsed;
                            UpdateBtnNewTab.Visibility = Visibility.Collapsed;
                            UpdateButton.Content = "Check for updates";
                            UpdateBtnRP.Visibility = Visibility.Visible;
                            SettingsPage.Self.PivotNewTab.Visibility = Visibility.Collapsed;
                            WindowsPage.Self.SettingBtnNewTab.Visibility = Visibility.Collapsed;
                            WindowsPage.Self.needToUpdadte = false;
                            UpdateStateTextBlock.Visibility = Visibility.Collapsed;
                            NoticeImg.Source = new BitmapImage(new Uri(this.BaseUri, "ms-appx:///Assets/NoticeImage/asus_ac_correct_ic.png"));
                            NoticeImg.Visibility = Visibility.Visible;
                            ErrorMessageText.Text = "AURA update is completed.";
                            ErrorMessageText.Foreground = new SolidColorBrush(Colors.White);
                            ErrorMessageStack.Visibility = Visibility.Visible;
                            InfoTextStack.Visibility = Visibility.Collapsed;
                            InfoTextBlock.Text = "Update for the latest AURA functions and firmware";
                            InfoTextBlock.Visibility = Visibility.Visible;
                            UpdateBtnMode = false;
                            await ConnectedDevicesDialog.Rescan();
                            break;
                        }
                        if (ServiceViewModel.returnnum == 813)
                        {
                            await Task.Delay(2000);
                            ProgressBar.Visibility = Visibility.Collapsed;
                            UpdateBtnNewTab.Visibility = Visibility.Collapsed;
                            UpdateButton.Content = "Check for updates";
                            UpdateBtnRP.Visibility = Visibility.Visible;
                            SettingsPage.Self.PivotNewTab.Visibility = Visibility.Collapsed;
                            WindowsPage.Self.SettingBtnNewTab.Visibility = Visibility.Collapsed;
                            WindowsPage.Self.needToUpdadte = false;
                            UpdateStateTextBlock.Visibility = Visibility.Collapsed;
                            NoticeImg.Source = new BitmapImage(new Uri(this.BaseUri, "ms-appx:///Assets/NoticeImage/asus_ac_error_ic.png"));
                            NoticeImg.Visibility = Visibility.Visible;
                            ErrorMessageText.Text = "AURA update failed! Please try again.";
                            ErrorMessageText.Foreground = new SolidColorBrush(Colors.Red);
                            ErrorMessageStack.Visibility = Visibility.Visible;
                            InfoTextStack.Visibility = Visibility.Collapsed;
                            InfoTextBlock.Text = "Update for the latest AURA functions and firmware";
                            InfoTextBlock.Visibility = Visibility.Visible;
                            UpdateBtnMode = false;
                            break;
                        }
                        await Task.Delay(4000);
                    }
                    ProgressBar.Value = 0;
                    ServiceViewModel.returnnum = 0;
                }
                else
                {
                    //Check For Update
                    UpdateStateTextBlock.Visibility = Visibility.Collapsed;
                    ProgressRing.Visibility = Visibility.Visible;
                    ProgressRing.IsActive = true;
                    NoticeImg.Visibility = Visibility.Collapsed;
                    ErrorMessageText.Text = "Checking...";
                    ErrorMessageText.Foreground = new SolidColorBrush(Colors.White);
                    ErrorMessageStack.Visibility = Visibility.Visible;
                    UpdateButton.IsEnabled = false;
                    UpdateButton.Opacity = 0.5;
                    // disable end
                    await (new ServiceViewModel()).Sendupdatestatus("CreatorCheckVersion");
                    // < 0 No checkallbyservice function
                    if (ServiceViewModel.returnnum > 0)
                    {
                        //顯示需要更新
                        PowerManager.PowerSupplyStatusChanged += PowerState_ReportUpdated;
                        Battery.AggregateBattery.ReportUpdated += AggregateBattery_ReportUpdated;
                        UpdateButton.IsEnabled = true;
                        UpdateButton.Opacity = 1;
                        UpdateButton.Content = "Update now";
                        UpdateBtnNewTab.Visibility = Visibility.Visible;
                        SettingsPage.Self.PivotNewTab.Visibility = Visibility.Visible;
                        WindowsPage.Self.SettingBtnNewTab.Visibility = Visibility.Visible;
                        WindowsPage.Self.needToUpdadte = true;
                        ErrorMessageStack.Visibility = Visibility.Collapsed;
                        ProgressRing.Visibility = Visibility.Collapsed;
                        ProgressRing.IsActive = false;
                        NoticeImg.Visibility = Visibility.Collapsed;
                        UpdateStateTextBlock.Visibility = Visibility.Visible;
                        UpdateStateTextBlock.Text = "A new version is available.";
                        UpdateStateTextBlock.Foreground = new SolidColorBrush(HexToColor("#FF1046FF"));
                        InfoTextBlock.Text = "When updating , make sure your computer is plugged to a power source and do not turn it off until the update process is complete.";
                        UpdateBtnMode = true;
                    }
                    else
                    {
                        UpdateButton.IsEnabled = true;
                        UpdateButton.Opacity = 1;
                        ErrorMessageStack.Visibility = Visibility.Collapsed;
                        ProgressRing.Visibility = Visibility.Collapsed;
                        ProgressRing.IsActive = false;
                        NoticeImg.Visibility = Visibility.Collapsed;
                        UpdateStateTextBlock.Visibility = Visibility.Visible;
                        UpdateStateTextBlock.Text = "Your version is up to date.";
                        UpdateStateTextBlock.Foreground = new SolidColorBrush(HexToColor("#FF1046FF"));
                        UpdateBtnMode = false;
                    }
                }
            }
            else
            {
                ContentDialog nonetworkdialog = new NoNetworkDialog();
                await nonetworkdialog.ShowAsync();
            }
        }

        private bool GetInternetConnectstate()
        {
            bool isConnected = false;
            ConnectionProfile profile = NetworkInformation.GetInternetConnectionProfile();
            if (profile == null)
            {
                return false;
            }
            else
            {
                NetworkConnectivityLevel cl = profile.GetNetworkConnectivityLevel();
                isConnected = !((cl == NetworkConnectivityLevel.None) || (cl == NetworkConnectivityLevel.LocalAccess));
            }

            if (!isConnected)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private async void PowerState_ReportUpdated(object sender, object args)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                GetPowerStateChange();
            });
        }

        private async void GetPowerStateChange()
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (PowerManager.PowerSupplyStatus == 0)
                {
                    ErrorMessageText.Text = "Ensure that your computer is plugged to a power source.";
                    ErrorMessageText.Foreground = new SolidColorBrush(Colors.Red);
                    NoticeImg.Source = new BitmapImage(new Uri(this.BaseUri, "ms-appx:///Assets/NoticeImage/asus_ac_error_ic.png"));
                    NoticeImg.Visibility = Visibility.Visible;
                    ErrorMessageStack.Visibility = Visibility.Visible;
                    UpdateStateTextBlock.Visibility = Visibility.Collapsed;
                    UpdateButton.IsEnabled = false;
                    UpdateButton.Opacity = 0.5;
                    IsCharge = false;
                }
                else
                {
                    UpdateButton.IsEnabled = true;
                    UpdateButton.Opacity = 1;
                    IsCharge = true;
                    RequestAggregateBatteryReport();
                }
            });
        }

        async private void AggregateBattery_ReportUpdated(Battery sender, object args)
        {
            if (IsCharge)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    RequestAggregateBatteryReport();
                });
            }
        }

        private void RequestAggregateBatteryReport()
        {
            // Create aggregate battery object
            var aggBattery = Battery.AggregateBattery;

            // Get report
            var report = aggBattery.GetReport();

            var pcMaximum = Convert.ToDouble(report.FullChargeCapacityInMilliwattHours);
            var pcValue = Convert.ToDouble(report.RemainingCapacityInMilliwattHours);
            var BatteryPercentage = (pcValue / pcMaximum) * 100;
            //Desktop, No Battery information
            if (double.IsNaN(BatteryPercentage))
            {
                return;
            }
            //Laptop
            if (BatteryPercentage > 20)
            {
                ErrorMessageText.Text = "Ensure that your computer is plugged to a power source.";
                ErrorMessageText.Foreground = new SolidColorBrush(Colors.Red);
                NoticeImg.Source = new BitmapImage(new Uri(this.BaseUri, "ms-appx:///Assets/NoticeImage/asus_ac_error_ic.png"));
                NoticeImg.Visibility = Visibility.Visible;
                ErrorMessageStack.Visibility = Visibility.Collapsed;
                UpdateStateTextBlock.Visibility = Visibility.Visible;
                UpdateButton.IsEnabled = true;
                UpdateButton.Opacity = 1;
            }
            else
            {
                ErrorMessageText.Text = "Remaining battery power is less than 20%.";
                ErrorMessageText.Foreground = new SolidColorBrush(Colors.Red);
                NoticeImg.Source = new BitmapImage(new Uri(this.BaseUri, "ms-appx:///Assets/NoticeImage/asus_ac_error_ic.png"));
                NoticeImg.Visibility = Visibility.Visible;
                ErrorMessageStack.Visibility = Visibility.Visible;
                UpdateStateTextBlock.Visibility = Visibility.Collapsed;
                UpdateButton.IsEnabled = false;
                UpdateButton.Opacity = 0.5;
            }
        }
    }
}
