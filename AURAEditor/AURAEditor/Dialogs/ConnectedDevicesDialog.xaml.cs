﻿using AuraEditor.Models;
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

        public ConnectedDevicesDialog()
        {
            Self = this;
            this.InitializeComponent();
            m_SyncDeviceList = new ObservableCollection<SyncDevice>();
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
            this.Hide();
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            NotifyIngroupDevicesChanged();
            this.Hide();
        }

        public async Task Rescan()
        {
            try
            {
                List<XmlNode> deviceNodes = await GetSortedAndFilteredDeviceList();

                m_SyncDeviceList.Clear();
                ConnectedDevicesListView.ItemsSource = null;
                foreach (var node in deviceNodes)
                {
                    SyncDevice sd = new SyncDevice
                    {
                        Name = (node as XmlElement).GetAttribute("name"),
                        Type = (node as XmlElement).GetAttribute("type"),
                        Sync = Boolean.Parse((node as XmlElement).GetAttribute("sync"))
                    };
                    m_SyncDeviceList.Add(sd);
                }
                ConnectedDevicesListView.ItemsSource = m_SyncDeviceList;

                NotifyIngroupDevicesChanged();
                UpdateSelectedText();
            }
            catch
            {
                MainPage.Self.StatusTextBlock.Text = "Rescan pluggeddevices.xml failed !";
                Log.Debug("[Rescan] Rescan pluggeddevices.xml failed !");
            }
        }
        private void NotifyIngroupDevicesChanged()
        {
            SpacePage.Self.OnIngroupDevicesChanged();
            MainPage.Self.OnIngroupDevicesChanged();
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

        static private async Task<List<XmlNode>> GetSortedAndFilteredDeviceList()
        {
            XmlNodeList deviceNodes = await GetPluggedDeviceXmlNodes();
            List<XmlNode> result = new List<XmlNode>();

            foreach (XmlNode n in deviceNodes)
            {
                XmlElement element = (XmlElement)n;
                result.Add(n);
            }

            result = FilterNodes(result);

            XmlNode node = result.Find(x => (x as XmlElement).GetAttribute("type") == "Notebook" ||
                                            (x as XmlElement).GetAttribute("type") == "Desktop");
            if (node != null)
            {
                result.Remove(node);
                result.Insert(0, node);
            }

            return result;
        }
        static private async Task<XmlNodeList> GetPluggedDeviceXmlNodes()
        {
            XmlDocument xmlDoc = await GetPluggedDevicesXmlDoc();
            if (xmlDoc == null)
                return null;

            XmlNode devicesNode = xmlDoc.SelectSingleNode("devices");
            XmlNodeList deviceNodes = devicesNode.SelectNodes("device");

            return deviceNodes;
        }
        static private async Task<XmlDocument> GetPluggedDevicesXmlDoc()
        {
            StorageFile sf;
            XmlDocument devicesXml = new XmlDocument(); ;

            try
            {
                sf = await StorageFile.GetFileFromPathAsync("C:\\ProgramData\\ASUS\\AURA Creator\\Devices\\pluggeddevices.xml");
                devicesXml.Load(await sf.OpenStreamForReadAsync());
                return devicesXml;
            }
            catch
            {
                MainPage.Self.StatusTextBlock.Text = "pluggeddevices.xml doesn't exist!";
                return null;
            }
        }
        static private List<XmlNode> FilterNodes(List<XmlNode> deviceNodes)
        {
            List<XmlNode> results = new List<XmlNode>();
            List<DeviceModel> deviceModels = SpacePage.Self.DeviceModelCollection;
            List<DeviceModel> existedDevices = deviceModels.FindAll(d => d.Status == DeviceStatus.OnStage || d.Status == DeviceStatus.Temp);

            while (deviceNodes.Count != 0)
            {
                XmlNode firstNode = deviceNodes[0];
                string firstNodeType = (firstNode as XmlElement).GetAttribute("type");
                List<XmlNode> sameTypeList = deviceNodes.FindAll(n => (n as XmlElement).GetAttribute("type") == firstNodeType);

                XmlNode findTheExistedNode = sameTypeList.Find(
                    n => existedDevices.Find(
                        d => d.Name == (n as XmlElement).GetAttribute("name")
                    ) != null
                );

                if (findTheExistedNode != null)
                    results.Add(findTheExistedNode);
                else
                    results.Add(firstNode);

                deviceNodes.RemoveAll(n => (n as XmlElement).GetAttribute("type") == firstNodeType);
            }

            return results;
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
    }
}
