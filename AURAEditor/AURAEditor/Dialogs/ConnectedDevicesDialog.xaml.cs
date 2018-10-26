using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using static AuraEditor.Common.StorageHelper;
using static AuraEditor.Common.EffectHelper;

// 內容對話方塊項目範本已記錄在 https://go.microsoft.com/fwlink/?LinkId=234238

namespace AuraEditor.Dialogs
{
    public sealed partial class ConnectedDevicesDialog : ContentDialog
    {
        static public ConnectedDevicesDialog Self;
        private ObservableCollection<SyncDevice> m_SyncDeviceList;
        private List<XmlNode> m_IngroupDeviceXmlNode;

        public ConnectedDevicesDialog()
        {
            Self = this;
            this.InitializeComponent();
            m_SyncDeviceList = new ObservableCollection<SyncDevice>();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            List<Device> globalDevices = AuraSpaceManager.Self.GlobalDevices;
            List<Device> tempDevices = AuraSpaceManager.Self.TempDevices;
            List<Device> _tempDevices = new List<Device>(tempDevices);

            foreach (Device temp_d in _tempDevices)
            {
                foreach (SyncDevice sd in m_SyncDeviceList)
                {
                    if (sd.Name == temp_d.Name && sd.Sync == true)
                    {
                        tempDevices.Remove(temp_d);
                        globalDevices.Add(temp_d);
                        break;
                    }
                }
            }

            NotifySpaceManager();
            this.Hide();
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            NotifySpaceManager();
            this.Hide();
        }

        public async Task Rescan()
        {
            List<XmlNode> deviceNodes = await GetSortedAndFilteredDeviceList();

            m_SyncDeviceList.Clear();
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

            m_IngroupDeviceXmlNode = new List<XmlNode>(deviceNodes);
            m_IngroupDeviceXmlNode.RemoveAll(x => (x as XmlElement).GetAttribute("sync") == "false");
            NotifySpaceManager();
        }
        private void NotifySpaceManager()
        {
            AuraSpaceManager.Self.RefreshIngroupDevices(GetIngroupDevices());
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

        static private async Task<List<XmlNode>> GetSortedAndFilteredDeviceList()
        {
            XmlNodeList deviceNodes = await GetPluggedDevices();
            List<XmlNode> resultList = new List<XmlNode>();

            foreach (XmlNode node in deviceNodes)
            {
                XmlElement element = (XmlElement)node;

                if (element.GetAttribute("type") == "Aac_NBDT") // Put local at first
                    resultList.Insert(0, node);
                else
                    resultList.Add(node);
            }

            return FilterDeviceListWeNeed(resultList);
        }
        static private async Task<XmlNodeList> GetPluggedDevices()
        {
            XmlDocument xmlDoc = await GetPluggedDevicesXmlDoc();
            XmlNode ingroupdevice = xmlDoc.SelectSingleNode("ingroupdevice");
            XmlNodeList deviceNodes = ingroupdevice.SelectNodes("device");

            return deviceNodes;
        }
        static private async Task<XmlDocument> GetPluggedDevicesXmlDoc()
        {
            StorageFile sf;
            XmlDocument devicesXml = new XmlDocument(); ;

            try
            {
                sf = await StorageFile.GetFileFromPathAsync("C:\\ProgramData\\ASUS\\AURA Creator\\Devices\\ingroupdevice.xml");
                devicesXml.Load(await sf.OpenStreamForReadAsync());
                return devicesXml;
            }
            catch
            {
                StorageFolder folder = await StorageFolder.GetFolderFromPathAsync("C:\\ProgramData\\ASUS");
                folder = await EnterOrCreateFolder(folder, "AURA Creator");
                folder = await EnterOrCreateFolder(folder, "Devices");
                sf = await folder.GetFileAsync("ingroupdevice.xml");

                devicesXml.Load(await sf.OpenStreamForReadAsync());
                return devicesXml;
            }
        }
        static private List<XmlNode> FilterDeviceListWeNeed(List<XmlNode> deviceList)
        {
            List<Device> globalDevices = AuraSpaceManager.Self.GlobalDevices;
            List<Device> tempDevices = AuraSpaceManager.Self.TempDevices;
            List<Device> _tempDevices = new List<Device>(tempDevices);
            List<XmlNode> results = new List<XmlNode>();

            // 1. Keep temp device or not
            foreach (Device temp_d in _tempDevices)
            {
                XmlNode node = deviceList.Find(x => (x as XmlElement).GetAttribute("name") == temp_d.Name);

                if (node != null)
                {
                    // The temp device is plugged back, so kick other same type device
                    results.Add(node);
                    deviceList.RemoveAll(x => (x as XmlElement).GetAttribute("type") == GetTypeNameByType(temp_d.Type));
                    tempDevices.Remove(temp_d);
                    globalDevices.Add(temp_d);
                }
                else
                {
                    if (deviceList.Find(x => (x as XmlElement).GetAttribute("type") == GetTypeNameByType(temp_d.Type)) != null)
                    {
                        // Detect another same type device, so we delete temp device data
                        tempDevices.Remove(temp_d);
                        AuraLayerManager.Self.ClearDeviceData(temp_d.Type);
                    }
                }
            }

            // 2. Keep devices which are in the stage, and kick other same type device
            foreach (Device global_d in globalDevices)
            {
                XmlNode node = deviceList.Find(x => (x as XmlElement).GetAttribute("name") == global_d.Name);

                if (node != null)
                {
                    // kick other same type
                    results.Add(node);
                    deviceList.RemoveAll(x => (x as XmlElement).GetAttribute("type") == GetTypeNameByType(global_d.Type));
                }
            }

            // 3. Only retain one node for every type
            for (int i = 0; i < deviceList.Count; i++)
            {
                XmlElement elem = (XmlElement)deviceList[i];
                bool CanAddThisNode = true;

                for (int j = 0; j < i; j++)
                {
                    XmlElement elem2 = (XmlElement)deviceList[j];

                    if (elem.GetAttribute("type") == elem2.GetAttribute("type"))
                        CanAddThisNode = false;
                }

                if (CanAddThisNode)
                    results.Add(deviceList[i]);
            }

            return results;
        }
    }
}
