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
using static AuraEditor.Common.LuaHelper;
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
                        await SaveFile(CurrentScriptPath, PrintLuaScript());
                    }
                    else
                    {
                        StorageFile saveFile = await ShowFileSavePickerAsync();

                        if (saveFile != null)
                        {
                            await SaveFile(saveFile, PrintLuaScript());
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
                        await SaveFile(CurrentScriptPath, PrintLuaScript());
                    }
                    else
                    {
                        StorageFile saveFile = await ShowFileSavePickerAsync();

                        if (saveFile != null)
                        {
                            await SaveFile(saveFile, PrintLuaScript());
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
                        await SaveFile(CurrentScriptPath, PrintLuaScript());
                    }
                    else
                    {
                        StorageFile saveFile = await ShowFileSavePickerAsync();

                        if (saveFile != null)
                        {
                            await SaveFile(saveFile, PrintLuaScript());
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
                await SaveFile(CurrentScriptPath, PrintLuaScript());
        }
        private async void SaveAsButton_Click(object sender, RoutedEventArgs e)
        {
            StorageFile saveFile = await ShowFileSavePickerAsync();

            if (saveFile != null)
            {
                await SaveFile(saveFile, PrintLuaScript());
                CurrentScriptPath = saveFile.Path;
            }
        }
        private void MoreButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private async Task LoadContent(string luaScript)
        {
            luaScript = luaScript.Replace(RequireLine, "");

            Script script = new Script();

            DynValue script_dv;
            Table eventprovider_table;
            Table viewport_table;
            Table event_table;
            Table globalspace_table;

            script_dv = script.DoString(luaScript + "\nreturn EventProvider");
            eventprovider_table = script_dv.Table;
            script_dv = script.DoString(luaScript + "\nreturn Viewport");
            viewport_table = script_dv.Table;
            script_dv = script.DoString(luaScript + "\nreturn Event");
            event_table = script_dv.Table;
            script_dv = script.DoString(luaScript + "\nreturn GlobalSpace");
            globalspace_table = script_dv.Table;

            // Step 1 : Convert GlobalSpace table to SpaceManager.GlobalDevices
            SpaceManager.GlobalDevices = await GetDeviceLocationFromGlobalSpaceTable(globalspace_table);

            // Step 2 : Convert EventProvider table to devicelayers
            List<DeviceLayer> deviceLayers = ParsingEventProviderTable(eventprovider_table);
            if (deviceLayers.Count == 0)
                return;

            // Step 3 : According to device layer name, convert Viewport table to m_ZoneDictionary
            foreach (var layer in deviceLayers)
            {
                Dictionary<int, int[]> dictionary = GetDeviceZonesFromViewportTable(viewport_table, layer.Name);
                layer.AddDeviceZones(dictionary);

                // Step 4 : According to effect.LuaName, convert EventTable table to EffectInfo
                foreach (var effect in layer.TimelineEffects)
                {
                    Table effectTable = event_table.Get(effect.LuaName).Table;
                    int type = GetEffectIndex(effect.LuaName);
                    EffectInfo ei = GetInfoFromEffectTable(type, effectTable);
                    effect.Info = ei;
                }
                foreach (var effect in layer.TriggerEffects)
                {
                    Table effectTable = event_table.Get(effect.LuaName).Table;
                    int type = GetEffectIndex(effect.LuaName);
                    EffectInfo ei = GetInfoFromEffectTable(type, effectTable);
                    effect.Info = ei;
                }

                LayerManager.AddDeviceLayer(layer);
            }
        }
        private Dictionary<int, int[]> GetDeviceZonesFromViewportTable(Table viewport_table, string layerName)
        {
            Dictionary<int, int[]> zoneDictionary = new Dictionary<int, int[]>();
            List<Device> devices = new List<Device>();
            Table layerTable = viewport_table.Get(layerName).Table;

            foreach (var deviceKey in layerTable.Keys)
            {
                Table deviceTable = layerTable.Get(deviceKey.String).Table;
                int type = 0;
                List<int> zones = new List<int>();

                switch (deviceTable.Get("DeviceType").String)
                {
                    case "Notebook": type = 0; break;
                    case "Mouse": type = 1; break;
                    case "Keyboard": type = 2; break;
                    case "Headset": type = 3; break;
                }

                Table usageTable = deviceTable.Get("usage").Table;
                foreach (var index in usageTable.Keys)
                {
                    int physicalIndex = (int)usageTable.Get(index.Number).Number;

                    zones.Add(physicalIndex);
                }

                zoneDictionary.Add(type, zones.ToArray());
            }

            return zoneDictionary;
        }
        private List<DeviceLayer> ParsingEventProviderTable(Table eventProviderTable)
        {
            List<DeviceLayer> layers = new List<DeviceLayer>();
            Table queueTable = eventProviderTable.Get("queue").Table;

            // TODO : Simplify here
            for (int queueIndex = 1; queueIndex <= queueTable.Length; queueIndex++)
            {
                Table t = queueTable.Get(queueIndex).Table;
                string layerName = t.Get("Viewport").String;

                DeviceLayer layer = layers.Find(x => x.Name == layerName);

                if (layer == null)
                {
                    layer = new DeviceLayer(layerName);
                    layers.Add(layer);
                }

                string effectLuaName = t.Get("Effect").String;
                double startTime = t.Get("Delay").Number;
                double durationTime = t.Get("Duration").Number;
                int type = GetEffectIndex(effectLuaName);

                if (IsTriggerEffect(type))
                {
                    TriggerEffect effect = new TriggerEffect(layer, type)
                    {
                        LuaName = effectLuaName,
                    };
                    layer.AddTriggerEffect(effect);
                }
                else
                {
                    TimelineEffect effect = new TimelineEffect(layer, type)
                    {
                        LuaName = effectLuaName,
                        StartTime = startTime,
                        DurationTime = durationTime
                    };
                    layer.AddTimelineEffect(effect);
                }
            }

            return layers;
        }
        private EffectInfo GetInfoFromEffectTable(int type, Table effectTable)
        {
            Table waveTable = effectTable.Get("wave").Table;
            Table waveTable_1 = waveTable.Get(1).Table;
            WaveInfo wInfo = new WaveInfo(type)
            {
                WaveType = WaveInfo.StringToWaveType(waveTable_1.Get("WaveType").String),
                Min = waveTable_1.Get("min").Number,
                Max = waveTable_1.Get("max").Number,
                WaveLength = waveTable_1.Get("waveLength").Number,
                Freq = waveTable_1.Get("freq").Number,
                Phase = waveTable_1.Get("phase").Number,
                Start = waveTable_1.Get("start").Number,
                Velocity = waveTable_1.Get("velocity").Number,
            };

            Table colorTable = effectTable.Get("initColor").Table;
            Color c = HSLToRGB(
                colorTable.Get("alpha").Number,
                colorTable.Get("hue").Number,
                colorTable.Get("saturation").Number,
                colorTable.Get("lightness").Number
                );

            EffectInfo ei = new EffectInfo(type);
            ei.InitColor = c;
            ei.Waves = new List<WaveInfo> { wInfo };

            return ei;
        }
        private async Task<List<Device>> GetDeviceLocationFromGlobalSpaceTable(Table globalspaceTable)
        {
            List<Device> devices = new List<Device>();

            foreach (var deviceKey in globalspaceTable.Keys)
            {
                Table deviceTable = globalspaceTable.Get(deviceKey.String).Table;

                string deviceName = deviceTable.Get("name").String;
                DeviceContent deviceContent = await DeviceContent.GetDeviceContent(deviceName);

                Table locationTable = deviceTable.Get("location").Table;
                int x = (int)locationTable.Get("x").Number;
                int y = (int)locationTable.Get("y").Number;

                Device d = deviceContent.ToDevice(new Point(x, y));
                devices.Add(d);
            }

            return devices;
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

