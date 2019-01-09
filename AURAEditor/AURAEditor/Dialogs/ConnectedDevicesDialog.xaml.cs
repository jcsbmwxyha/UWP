using AuraEditor.Models;
using AuraEditor.Pages;
using AuraEditor.UserControls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using static AuraEditor.Common.ControlHelper;
using static AuraEditor.Common.MetroEventSource;

// 內容對話方塊項目範本已記錄在 https://go.microsoft.com/fwlink/?LinkId=234238

namespace AuraEditor.Dialogs
{
    public sealed partial class ConnectedDevicesDialog : ContentDialog
    {
        static public ConnectedDevicesDialog Self;
        private ObservableCollection<SyncDevice> m_SyncDeviceList;

        bool[] TemporaryCheckArray;

        public ConnectedDevicesDialog()
        {
            Self = this;
            this.InitializeComponent();
            m_SyncDeviceList = new ObservableCollection<SyncDevice>();
            UpdateTemporaryCheckState();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            string sendToLiveService = "[SyncStatus]";
            List<DeviceModel> deviceModels = SpacePage.Self.DeviceModelCollection;

            foreach (SyncDevice sd in m_SyncDeviceList)
            {
                DeviceModel find = deviceModels.Find(d => d.Name == sd.Name);
                if (find != null)
                    find.Status = DeviceStatus.OnStage;

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
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            int arrayCount = 0;
            foreach (SyncDevice sd in ConnectedDevicesListView.Items)
            {
                sd.Sync = TemporaryCheckArray[arrayCount];
                arrayCount++;
            }
            this.Hide();
        }

        public async Task Rescan()
        {
            try
            {
                m_SyncDeviceList.Clear();
                string deviceList = await GetPluggedDevicesFromService();
                string[] deviceArray = deviceList.Split(':');

                foreach (var deviceString in deviceArray)
                {
                    if (deviceString == "")
                        continue;

                    string[] deviceData = deviceString.Split(',');

                    SyncDevice sd = new SyncDevice
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
                return result;
            }
            catch (Exception ex)
            {
                Log.Debug("[GetPluggedDevices] Get Failed : " + ex.ToString());
                return null;
            }
        }
        private void NotifyIngroupDevicesChanged()
        {
            SpacePage.Self.OnIngroupDevicesChanged();
        }
        public List<SyncDevice> GetIngroupDevices()
        {
            List<SyncDevice> result = new List<SyncDevice>();
            foreach (var d in m_SyncDeviceList)
            {
                if (d.Sync == true)
                    result.Add(d);
            }
            return result;
        }
        public List<SyncDevice> GetPluggedDevices()
        {
            return m_SyncDeviceList.ToList();
        }

        private void SelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            List<ConnectedDeviceBlock> cdbList = FindAllControl<ConnectedDeviceBlock>(ConnectedDevicesListView, typeof(ConnectedDeviceBlock));
            if (cdbList == null)
            {
                return;
            }
            if (SelectAllButton.IsChecked == true)
            {
                foreach (var cdb in cdbList)
                {
                    cdb.Update();
                    if (cdb != null)
                    {
                        List<ToggleButton> toggleButtons = FindAllControl<ToggleButton>(cdb, typeof(ToggleButton));
                        toggleButtons[0].IsChecked = true;
                    }
                }
            }
            else
            {
                foreach (var cdb in cdbList)
                {
                    if (cdb != null)
                    {
                        List<ToggleButton> toggleButtons = FindAllControl<ToggleButton>(cdb, typeof(ToggleButton));
                        toggleButtons[0].IsChecked = false;
                    }
                }
            }
        }

        public void UpdateSelectedText()
        {
            int selectedcount = 0;
            foreach (SyncDevice sd in ConnectedDevicesListView.Items)
            {
                if (sd.Sync == true)
                {
                    selectedcount += 1;
                }
            }
            if (selectedcount == 0)
            {
                SelectAllButton.IsChecked = false;
            }
            if (selectedcount == ConnectedDevicesListView.Items.Count)
            {
                SelectAllButton.IsChecked = true;
            }
            SelectedNumberText.Text = "(" + selectedcount.ToString() + ")  Selected devices";
        }

        public void UpdateTemporaryCheckState()
        {
            int arrayCount = 0;
            TemporaryCheckArray = new bool[ConnectedDevicesListView.Items.Count];
            foreach (SyncDevice sd in ConnectedDevicesListView.Items)
            {
                TemporaryCheckArray[arrayCount] = sd.Sync;
                arrayCount++;
            }
        }
    }
}
