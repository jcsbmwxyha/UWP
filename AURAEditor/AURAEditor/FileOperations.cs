using System;
using System.Collections.Generic;
using System.IO;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using MoonSharp.Interpreter;
using Windows.Storage.Pickers;
using Windows.Storage;
using System.Threading.Tasks;
using System.Xml;
using System.Collections.ObjectModel;
using AuraEditor.Dialogs;
using Windows.Foundation;
using static AuraEditor.Common.EffectHelper;
using static AuraEditor.Common.XmlHelper;
using static AuraEditor.Common.StorageHelper;
using static AuraEditor.Common.AuraEditorColorHelper;
using Windows.UI;

namespace AuraEditor
{
    public sealed partial class MainPage : Page
    {
        public class RecentList
        {
            public ObservableCollection<string> List;
            public int MaxCount;
            private StorageFile m_XmlSF;

            public RecentList()
            {
                List = new ObservableCollection<string>();
                MaxCount = 5;
            }
            public async Task SetXmlAndLoadRecentFiles(string path)
            {
                try
                {
                    m_XmlSF = await StorageFile.GetFileFromPathAsync(path);
                    List = new ObservableCollection<string>(await GetRecentFilePaths(m_XmlSF));
                }
                catch
                {
                    StorageFolder folder = await StorageFolder.GetFolderFromPathAsync("C:\\ProgramData\\ASUS");
                    folder = await EnterOrCreateFolder(folder, "AURA Creator");
                    folder = await EnterOrCreateFolder(folder, "script");
                    m_XmlSF = await folder.CreateFileAsync("recentfiles.xml", Windows.Storage.CreationCollisionOption.ReplaceExisting);

                    XmlDocument doc = new XmlDocument();
                    XmlElement recentfilesElement = doc.CreateElement(string.Empty, "recentfiles", string.Empty);
                    doc.AppendChild(recentfilesElement);

                    await Windows.Storage.FileIO.WriteTextAsync(m_XmlSF, doc.OuterXml);
                }
            }
            private async Task<List<string>> GetRecentFilePaths(StorageFile recentFileSF)
            {
                List<string> list = new List<string>();
                XmlDocument xml = new XmlDocument();
                xml.Load(await recentFileSF.OpenStreamForReadAsync());
                XmlNode recentfilesNode = xml.SelectSingleNode("recentfiles");
                XmlNodeList fileNodes = recentfilesNode.SelectNodes("file");

                for (int i = 0; i < fileNodes.Count; i++)
                {
                    XmlElement element = (XmlElement)fileNodes[i];
                    list.Add(element.GetAttribute("path"));
                }

                return list;
            }

            public void InsertHead(string item)
            {
                for (int i = 0; i < List.Count; i++)
                {
                    if (List[i] == item)
                    {
                        List.RemoveAt(i);
                        break;
                    }
                }

                List.Insert(0, item);

                if (List.Count > MaxCount)
                {
                    List.RemoveAt(MaxCount);
                }
                UpdateXml();
            }
            public void InsertTail(string item)
            {
                if (List.Count == MaxCount)
                {
                    return;
                }

                List.Add(item);
                UpdateXml();
            }
            private async void UpdateXml()
            {
                XmlDocument doc = new XmlDocument();
                XmlElement recentfilesElement = doc.CreateElement(string.Empty, "recentfiles", string.Empty);
                doc.AppendChild(recentfilesElement);

                foreach (var filepath in List)
                {
                    XmlElement fileElement = doc.CreateElement(string.Empty, "file", string.Empty);
                    fileElement.SetAttribute("path", filepath);
                    recentfilesElement.AppendChild(fileElement);
                }

                await SaveFile(m_XmlSF, doc.OuterXml);
            }
        }
        private RecentList recentFileList;
        private string _currentScriptPath;
        public string CurrentScriptPath
        {
            get
            {
                return _currentScriptPath;
            }
            set
            {
                if (value != _currentScriptPath)
                {
                    _currentScriptPath = value;

                    if (value == null)
                    {
                        FileListComboBox.SelectedIndex = -1;
                    }
                    else
                    {
                        FileListComboBox.SelectedIndex = -1;
                        recentFileList.InsertHead(value);
                        FileListComboBox.SelectedIndex = 0;
                    }
                }
            }
        }

        private async Task IntializeFileOperations()
        {
            recentFileList = new RecentList();
            await recentFileList.SetXmlAndLoadRecentFiles("C:\\ProgramData\\ASUS\\AURA Creator\\script\\recentfiles.xml");
        }

        private async void FileListComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = FileListComboBox.SelectedIndex;

            // Remove selected item will cause selected index changing to -1
            if (index == -1)
                return;

            if (index == 0)
            {
                if (CurrentScriptPath != null)
                    return;
            }

            ContentDialog dialog = new YesNoCancelDialog();
            ContentDialogResult result = await dialog.ShowAsync();

            if (result == ContentDialogResult.None)
            {
                return;
            }
            else
            {
                if (result == ContentDialogResult.Primary)
                {
                    if (CurrentScriptPath != null)
                    {
                        await SaveFile(CurrentScriptPath, GetUserData());
                    }
                    else
                    {
                        StorageFile saveFile = await ShowFileSavePickerAsync();

                        if (saveFile != null)
                        {
                            await SaveFile(saveFile, GetUserData());
                            CurrentScriptPath = saveFile.Path;
                        }
                        else
                        {
                            return;
                        }
                    }
                }

                // load file
                string path = FileListComboBox.Items[index] as string;
                CurrentScriptPath = path;
                Reset();
                await LoadContent(await LoadFile(path));
                SpaceManager.RefreshSpaceGrid();
            }
        }
        private async void NewButton_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new YesNoCancelDialog();
            ContentDialogResult result = await dialog.ShowAsync();

            if (result == ContentDialogResult.None)
            {
                return;
            }
            else
            {
                if (result == ContentDialogResult.Primary)
                {
                    if (CurrentScriptPath != null)
                    {
                        await SaveFile(CurrentScriptPath, GetUserData());
                    }
                    else
                    {
                        StorageFile saveFile = await ShowFileSavePickerAsync();

                        if (saveFile != null)
                        {
                            await SaveFile(saveFile, GetUserData());
                            CurrentScriptPath = saveFile.Path;
                        }
                        else
                        {
                            return;
                        }
                    }
                }

                CurrentScriptPath = null;
                Reset();
                SpaceManager.FillWithIngroupDevices();
            }
        }
        private async void LoadFileButton_Click(object sender, RoutedEventArgs e)
        {
            var inputFile = await ShowFileOpenPickerAsync();

            if (inputFile == null)
            {
                return;
            }

            ContentDialog dialog = new YesNoCancelDialog();
            ContentDialogResult result = await dialog.ShowAsync();

            if (result == ContentDialogResult.None)
            {
                return;
            }
            else
            {
                if (result == ContentDialogResult.Primary)
                {
                    if (CurrentScriptPath != null)
                    {
                        await SaveFile(CurrentScriptPath, GetUserData());
                    }
                    else
                    {
                        StorageFile saveFile = await ShowFileSavePickerAsync();

                        if (saveFile != null)
                        {
                            await SaveFile(saveFile, GetUserData());
                            CurrentScriptPath = saveFile.Path;
                        }
                        else
                        {
                            return;
                        }
                    }
                }

                // load file
                CurrentScriptPath = inputFile.Path;
                Reset();
                await LoadContent(await LoadFile(inputFile));
                SpaceManager.RefreshSpaceGrid();
            }
        }
        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentScriptPath == null)
                SaveAsButton_Click(sender, e);
            else
                await SaveFile(CurrentScriptPath, GetUserData());
        }
        private async void SaveAsButton_Click(object sender, RoutedEventArgs e)
        {
            StorageFile saveFile = await ShowFileSavePickerAsync();

            if (saveFile != null)
            {
                await SaveFile(saveFile, GetUserData());
                CurrentScriptPath = saveFile.Path;
            }
        }
        private void MoreButton_Click(object sender, RoutedEventArgs e)
        {
        }

        public string GetUserData()
        {
            XmlNode root = CreateXmlNodeOfFile("root");

            root.AppendChild(SpaceManager.ToXmlNodeForUserData());
            root.AppendChild(LayerManager.ToXmlNodeForUserData());

            return root.OuterXml;
        }
        private async Task LoadContent(string xmlContent)
        {
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(xmlContent);

            XmlNode spaceNode = xml.SelectSingleNode("/root/space");
            XmlNode layersNode = xml.SelectSingleNode("/root/layers");

            XmlNodeList deviceNodes = spaceNode.SelectNodes("device");
            XmlNodeList layerNodes = layersNode.SelectNodes("layer");

            await ParsingGlobalDevices(deviceNodes);
            ParsingLayers(layerNodes);
        }
        private async Task ParsingGlobalDevices(XmlNodeList deviceNodes)
        {
            List<Device> devices = new List<Device>();

            foreach (XmlNode node in deviceNodes)
            {
                XmlElement element = (XmlElement)node;
                string deviceName = element.GetAttribute("name");
                int x = Int32.Parse(element.SelectSingleNode("x").InnerText);
                int y = Int32.Parse(element.SelectSingleNode("y").InnerText);

                DeviceContent deviceContent = await DeviceContent.GetDeviceContent(deviceName);
                Device d = deviceContent.ToDevice(new Point(x, y));

                devices.Add(d);
            }
            SpaceManager.GlobalDevices = devices;
        }
        private void ParsingLayers(XmlNodeList layerNodes)
        {
            List<DeviceLayer> layers = new List<DeviceLayer>();

            foreach (XmlNode node in layerNodes)
            {
                XmlElement element = (XmlElement)node;
                string layerName = element.GetAttribute("name");
                DeviceLayer layer = new DeviceLayer(layerName);

                // parsing effects
                XmlNode effectsNode = element.SelectSingleNode("effects");
                foreach (XmlNode effectNode in effectsNode.ChildNodes)
                {
                    XmlElement element2 = (XmlElement)effectNode;
                    int type = Int32.Parse(element2.SelectSingleNode("type").InnerText);

                    EffectInfo info = new EffectInfo()
                    {
                        InitColor = new Color
                        {
                            A = Byte.Parse(element2.SelectSingleNode("a").InnerText),
                            R = Byte.Parse(element2.SelectSingleNode("r").InnerText),
                            G = Byte.Parse(element2.SelectSingleNode("g").InnerText),
                            B = Byte.Parse(element2.SelectSingleNode("b").InnerText),
                        },
                        Type = type,
                        Direction = Int32.Parse(element2.SelectSingleNode("direction").InnerText),
                        Speed = Int32.Parse(element2.SelectSingleNode("speed").InnerText),
                        Angle = Int32.Parse(element2.SelectSingleNode("angle").InnerText),
                        Random = bool.Parse(element2.SelectSingleNode("random").InnerText),
                        High = Int32.Parse(element2.SelectSingleNode("high").InnerText),
                        Low = Int32.Parse(element2.SelectSingleNode("low").InnerText),
                    };

                    if (!IsTriggerEffect(type))
                    {
                        TimelineEffect eff = new TimelineEffect(layer, type);
                        eff.StartTime = Int32.Parse(element2.SelectSingleNode("start").InnerText);
                        eff.DurationTime = Int32.Parse(element2.SelectSingleNode("duration").InnerText);
                        eff.Info = info;
                        layer.AddTimelineEffect(eff);
                    }
                    else
                    {
                        TriggerEffect eff = new TriggerEffect(layer, type);
                        eff.StartTime = Int32.Parse(element2.SelectSingleNode("start").InnerText);
                        eff.DurationTime = Int32.Parse(element2.SelectSingleNode("duration").InnerText);
                        eff.Info = info;
                        layer.AddTriggerEffect(eff);
                    }
                }

                // parsing zones
                XmlNode devicesNode = element.SelectSingleNode("devices");
                foreach (XmlNode deviceNode in devicesNode.ChildNodes)
                {
                    Dictionary<int, int[]> zoneDictionary = new Dictionary<int, int[]>();
                    List<int> zones = new List<int>();
                    XmlElement element2 = (XmlElement)deviceNode;
                    string name = element2.GetAttribute("name");
                    int type = GetDeviceTypeByDeviceName(name);

                    XmlNodeList indexNodes = element2.ChildNodes;
                    foreach (XmlNode index in indexNodes)
                    {
                        zones.Add(Int32.Parse(index.InnerText));
                    }

                    zoneDictionary.Add(type, zones.ToArray());
                    layer.AddDeviceZones(zoneDictionary);
                }

                layers.Add(layer);
                LayerManager.AddDeviceLayer(layer);
            }
        }
        private void Reset()
        {
            DragDevImgToggleButton.IsChecked = false;
            SetLayerButton.IsEnabled = true;
            SelectedEffectLine = null;
            LayerManager.Reset();
            SpaceManager.Reset();
            TimelineZoomLevel = 2;
        }
    }
}

