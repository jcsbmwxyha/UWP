using AuraEditor.Models;
using AuraEditor.Pages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using static AuraEditor.Common.MetroEventSource;

// 內容對話方塊項目範本已記錄在 https://go.microsoft.com/fwlink/?LinkId=234238

namespace AuraEditor.Dialogs
{
    public sealed partial class ConnectedDevicesDialog : ContentDialog
    {
        static public ConnectedDevicesDialog Self;
        private ObservableCollection<SyncDeviceModel> m_SyncDeviceList;

        bool[] TemporaryCheckArray;

        public ConnectedDevicesDialog()
        {
            Self = this;
            this.InitializeComponent();
            m_SyncDeviceList = new ObservableCollection<SyncDeviceModel>();
            UpdateTemporaryCheckState();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            string sendToLiveService = "[SyncStatus]";
            List<DeviceModel> deviceModels = SpacePage.Self.DeviceModelCollection;

            foreach (SyncDeviceModel sd in m_SyncDeviceList)
            {
                sendToLiveService += sd.Name + ",";
                sendToLiveService += sd.Sync == true ? "1," : "0,";
            }

            if (MainPage.IsConnection)
            {
                MainPage.Self.SendMessageToServer(sendToLiveService);
                Log.Debug(sendToLiveService);
            }
            NotifyIngroupDevicesChanged();
            UpdateTemporaryCheckState();
            this.Hide();
            MainPage.Self.CanShowDeviceUpdateDialog = true;
            MainPage.Self.ShowDeviceUpdateDialogOrNot();
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            int arrayCount = 0;
            foreach (SyncDeviceModel sd in ConnectedDevicesListView.Items)
            {
                sd.Sync = TemporaryCheckArray[arrayCount];
                arrayCount++;
            }
            this.Hide();
            MainPage.Self.CanShowDeviceUpdateDialog = true;
            MainPage.Self.ShowDeviceUpdateDialogOrNot();
        }

        public async Task Rescan()
        {
            try
            {
                Log.Debug("[Rescan] Start !");
                m_SyncDeviceList.Clear();
                string deviceList = await GetPluggedDevicesFromService();
                string[] deviceArray = deviceList.Split(':');

                foreach (var deviceString in deviceArray)
                {
                    if (deviceString == "")
                        continue;

                    string[] deviceData = deviceString.Split(',');

                    SyncDeviceModel sd = new SyncDeviceModel
                    {
                        Name = deviceData[0],
                        Type = deviceData[1],
                        Sync = Boolean.Parse(deviceData[2])
                    };

                    if (sd.Type == "Notebook" || sd.Type == "Desktop")
                        m_SyncDeviceList.Insert(0, sd);
                    else
                        m_SyncDeviceList.Add(sd);
                }

                ConnectedDevicesListView.ItemsSource = m_SyncDeviceList;
                NotifyIngroupDevicesChanged();
                UpdateSelectedText();
                UpdateTemporaryCheckState();
            }
            catch
            {
                NotifyIngroupDevicesChanged();
                MainPage.Self.StatusTextBlock.Text = "Rescan pluggeddevices failed !";
                Log.Debug("[Rescan] Rescan pluggeddevices failed !");
            }
        }
        static private async Task<string> GetPluggedDevicesFromService()
        {
            try
            {
                string result = "";

                await (new ServiceViewModel()).AuraCreatorGetDevice("CREATORGETDEVICE");
                int listcount = Int32.Parse(ServiceViewModel.devicename);
                Log.Debug("[GetPluggedDevices] Plugged device count : " + listcount);
                for (int i = 0; i < listcount; i++)
                {
                    await (new ServiceViewModel()).AuraCreatorGetDevice(i.ToString());
                    //string format : Name,DeviceType,SyncStatus
                    string devicename = ServiceViewModel.devicename;
                    Console.WriteLine(devicename);
                    result += devicename + ":";
                }

                Log.Debug("[GetPluggedDevices] Get message : " + result);
                return "G703GI_US,Notebook,true:PUGIO,Mouse,true:NH01,MousePad,true";
                return result;
            }
            catch (Exception ex)
            {
                Log.Debug("[GetPluggedDevices] Get Failed : " + ex.ToString());
                return "G703GI_US,Notebook,true:PUGIO,Mouse,true:NH01,MousePad,true";
                return null;
            }
        }
        private void NotifyIngroupDevicesChanged()
        {
            SpacePage.Self.OnIngroupDevicesChanged();
        }
        public List<SyncDeviceModel> GetIngroupDevices()
        {
            List<SyncDeviceModel> result = new List<SyncDeviceModel>();
            foreach (var d in m_SyncDeviceList)
            {
                if (d.Sync == true)
                    result.Add(d);
            }
            return result;
        }
        public List<SyncDeviceModel> GetPluggedDevices()
        {
            return m_SyncDeviceList.ToList();
        }

        private void SelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            if (m_SyncDeviceList.Count == 0)
                return;

            if (SelectAllButton.IsChecked == true)
            {
                foreach (var sd in m_SyncDeviceList)
                    sd.Sync = true;
            }
            else
            {
                foreach (var sd in m_SyncDeviceList)
                    sd.Sync = false;
            }
        }

        public void UpdateSelectedText()
        {
            int selectedcount = 0;

            foreach (var sd in m_SyncDeviceList)
            {
                if (sd.Sync == true)
                    selectedcount++;
            }

            if (selectedcount == m_SyncDeviceList.Count)
                SelectAllButton.IsChecked = true;
            else
                SelectAllButton.IsChecked = false;

            SelectedNumberText.Text = "(" + selectedcount.ToString() + ")  synced device(s).";
        }

        public void UpdateTemporaryCheckState()
        {
            int arrayCount = 0;
            TemporaryCheckArray = new bool[ConnectedDevicesListView.Items.Count];
            foreach (SyncDeviceModel sd in ConnectedDevicesListView.Items)
            {
                TemporaryCheckArray[arrayCount] = sd.Sync;
                arrayCount++;
            }
        }
    }
}
