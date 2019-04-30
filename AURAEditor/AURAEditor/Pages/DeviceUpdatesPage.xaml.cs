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
using AuraEditor.Pages;
using Windows.ApplicationModel.Resources;

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

        public ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView();
        static DeviceUpdatesPage _instance;
        static public DeviceUpdatesPage Self
        {
            get { return _instance; }
        }
        public DeviceUpdatesPage()
        {
            _instance = this;
            this.InitializeComponent();

            NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is bool)
            {
                UpdateBtnMode = (bool)e.Parameter;
                if ((bool)e.Parameter)
                {
                    UpdateButton.Content = resourceLoader.GetString("UpdateButton_Update");
                    UpdateBtnNewTab.Visibility = Visibility.Visible;
                    UpdateStateTextBlock.Foreground = new SolidColorBrush(HexToColor("#FF1046FF"));
                    UpdateStateTextBlock.Text = resourceLoader.GetString("UpdateStateTextBlock_NewVersion");

                    PowerManager.PowerSupplyStatusChanged += PowerState_ReportUpdated;
                    Battery.AggregateBattery.ReportUpdated += AggregateBattery_ReportUpdated;
                    GetPowerStateChange();
                }
                else
                {
                    UpdateButton.Content = resourceLoader.GetString("UpdateButton_Check");
                    UpdateBtnNewTab.Visibility = Visibility.Collapsed;
                    UpdateStateTextBlock.Foreground = new SolidColorBrush(HexToColor("#FF1046FF"));
                    UpdateStateTextBlock.Text = resourceLoader.GetString("UpdateStateTextBlock_UpToDate");
                }
            }
            else
            {
                return;
            }
            base.OnNavigatedTo(e);
        }

        public async void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (GetInternetConnectstate())
            {
                int UITimeOut = 0;
                if (UpdateBtnMode)
                {
                    //Update Now
                    PowerManager.PowerSupplyStatusChanged -= PowerState_ReportUpdated;
                    Battery.AggregateBattery.ReportUpdated -= AggregateBattery_ReportUpdated;
                    ProgressBar.Visibility = Visibility.Visible;
                    UpdateBtnRP.Visibility = Visibility.Collapsed;
                    UpdateStateTextBlock.Text = resourceLoader.GetString("UpdateStateTextBlock_Updating");
                    UpdateStateTextBlock.Foreground = new SolidColorBrush(Colors.White);
                    
                    await (new ServiceViewModel()).Sendupdatestatus("CreatorUpdate");
                    while (true)
                    {
                        await (new ServiceViewModel()).Sendupdatestatus(ServiceViewModel.returnnum.ToString());

                        if(ServiceViewModel.returnnum > ProgressBar.Value)
                        {
                            ProgressBar.Value = ServiceViewModel.returnnum;
                            UITimeOut = 0;
                        }
                        else
                        {
                            UITimeOut += 1;
                        }

                        if (ServiceViewModel.returnnum == 100)
                        {
                            ProgressBar.Value = ServiceViewModel.returnnum;
                            await Task.Delay(2000);
                            ProgressBar.Visibility = Visibility.Collapsed;
                            UpdateBtnNewTab.Visibility = Visibility.Collapsed;
                            UpdateButton.Content = resourceLoader.GetString("UpdateButton_Check");
                            UpdateStateTextBlock.Text = resourceLoader.GetString("UpdateStateTextBlock_UpToDate");
                            UpdateStateTextBlock.Foreground = new SolidColorBrush(HexToColor("#FF1046FF"));
                            UpdateStateTextBlock.Visibility = Visibility.Visible;
                            UpdateBtnRP.Visibility = Visibility.Visible;
                            SettingsPage.Self.PivotNewTab.Visibility = Visibility.Collapsed;
                            MainPage.Self.SettingBtnNewTab.Visibility = Visibility.Collapsed;
                            MainPage.Self.needToUpdadte = false;
                            UpdateBtnMode = false;
                            await SpacePage.Self.Rescan();
                            break;
                        }
                        if (ServiceViewModel.returnnum == 813)
                        {
                            UpdateFailState();
                            break;
                        }
                        if (UITimeOut == 60)
                        {
                            UpdateFailState();
                            break;
                        }
                        await Task.Delay(500);
                    }
                    ProgressBar.Value = 0;
                    ServiceViewModel.returnnum = 0;
                }
                else
                {
                    //Check For Update
                    UpdateStateTextBlock.Visibility = Visibility.Collapsed;
                    UpdateBtnRP.Visibility = Visibility.Collapsed;
                    CheckProgressRing.IsActive = true;
                    CheckStackPanel.Visibility = Visibility.Visible;
                    ErrorMessageStack.Visibility = Visibility.Collapsed;
                    // disable end
                    await (new ServiceViewModel()).Sendupdatestatus("CreatorCheckVersion");
                    // < 0 No checkallbyservice function

                    if (ServiceViewModel.returnnum > 0)
                    {
                        //顯示需要更新
                        PowerManager.PowerSupplyStatusChanged += PowerState_ReportUpdated;
                        Battery.AggregateBattery.ReportUpdated += AggregateBattery_ReportUpdated;
                        CheckProgressRing.IsActive = false;
                        CheckStackPanel.Visibility = Visibility.Collapsed;
                        UpdateButton.Content = resourceLoader.GetString("UpdateButton_Update");
                        UpdateBtnNewTab.Visibility = Visibility.Visible;
                        UpdateBtnRP.Visibility = Visibility.Visible;
                        SettingsPage.Self.PivotNewTab.Visibility = Visibility.Visible;
                        MainPage.Self.SettingBtnNewTab.Visibility = Visibility.Visible;
                        MainPage.Self.needToUpdadte = true;
                        NoticeImg.Visibility = Visibility.Collapsed;
                        UpdateStateTextBlock.Visibility = Visibility.Visible;
                        UpdateStateTextBlock.Text = resourceLoader.GetString("UpdateStateTextBlock_NewVersion");
                        UpdateStateTextBlock.Foreground = new SolidColorBrush(HexToColor("#FF1046FF"));
                        UpdateBtnMode = true;
                    }
                    else
                    {
                        CheckProgressRing.IsActive = false;
                        CheckStackPanel.Visibility = Visibility.Collapsed;
                        UpdateButton.Content = resourceLoader.GetString("UpdateButton_Check");
                        UpdateBtnRP.Visibility = Visibility.Visible;
                        NoticeImg.Visibility = Visibility.Collapsed;
                        UpdateStateTextBlock.Visibility = Visibility.Visible;
                        UpdateStateTextBlock.Text = resourceLoader.GetString("UpdateStateTextBlock_UpToDate");
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

        private void UpdateFailState()
        {
            ProgressBar.Visibility = Visibility.Collapsed;
            UpdateBtnNewTab.Visibility = Visibility.Collapsed;
            UpdateButton.Content = resourceLoader.GetString("UpdateButton_Check");
            UpdateStateTextBlock.Foreground = new SolidColorBrush(HexToColor("#FF1046FF"));
            UpdateStateTextBlock.Text = resourceLoader.GetString("UpdateStateTextBlock_UpToDate");
            UpdateBtnRP.Visibility = Visibility.Visible;
            SettingsPage.Self.PivotNewTab.Visibility = Visibility.Collapsed;
            MainPage.Self.SettingBtnNewTab.Visibility = Visibility.Collapsed;
            MainPage.Self.needToUpdadte = false;
            UpdateStateTextBlock.Visibility = Visibility.Collapsed;
            NoticeImg.Source = new BitmapImage(new Uri(this.BaseUri, "ms-appx:///Assets/NoticeImage/asus_ac_error_ic.png"));
            NoticeImg.Visibility = Visibility.Visible;
            ErrorMessageText.Text = resourceLoader.GetString("ErrorMessage_UpdateFail");
            ErrorMessageText.Foreground = new SolidColorBrush(Colors.Red);
            ErrorMessageStack.Visibility = Visibility.Visible;
            UpdateBtnMode = false;
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
                    ErrorMessageText.Text = resourceLoader.GetString("ErrorMessage_PowerProblem");
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
                ErrorMessageText.Text = resourceLoader.GetString("ErrorMessage_PowerProblem");
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
                ErrorMessageText.Text = resourceLoader.GetString("ErrorMessage_PowerProblem");
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
