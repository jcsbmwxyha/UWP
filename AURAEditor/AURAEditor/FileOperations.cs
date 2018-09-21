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
using static AuraEditor.Common.EffectHelper;
using static AuraEditor.Common.LuaHelper;
using static AuraEditor.Common.StorageHelper;
using System.Collections.ObjectModel;

namespace AuraEditor
{
    public sealed partial class MainPage : Page
    {
        public class RecentFileList
        {
            public ObservableCollection<string> List;
            public int MaxCount;
            private StorageFile m_XmlSF;

            public RecentFileList()
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

                string s =  doc.OuterXml;
                await SaveFile(m_XmlSF, doc.OuterXml);
            }
        }
        private RecentFileList recentFileList;
        public string CurrentScriptPath
        {
            get
            {
                if (FileListComboBox.SelectedIndex == -1)
                    return null;

                return recentFileList.List[0];
            }
            set
            {
                recentFileList.InsertHead(value);
                FileListComboBox.SelectedIndex = 0;
            }
        }

        private async Task IntializeFileOperations()
        {
            recentFileList = new RecentFileList();
            await recentFileList.SetXmlAndLoadRecentFiles("C:\\ProgramData\\ASUS\\AURA Creator\\script\\recentfiles.xml");
            Bindings.Update();
        }

        private async void FileListComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = FileListComboBox.SelectedIndex;

            // Calling InsertHead or setting SelectedIndex will call SelectionChanged again.
            // We should ingore it.
            if (index == -1 || index == 0)
                return;

            string item = FileListComboBox.Items[index] as string;
            string content = await LoadFile(item);
            CurrentScriptPath = item;
            //recentFileList.InsertHead(item);
            FileListComboBox.SelectedIndex = 0;
        }
        private void NewButton_Click(object sender, RoutedEventArgs e)
        {
            _auraCreatorManager.Reset();
        }
        private async void LoadFileButton_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker fileOpenPicker = new FileOpenPicker();
            fileOpenPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            fileOpenPicker.FileTypeFilter.Add(".lua");
            fileOpenPicker.ViewMode = PickerViewMode.Thumbnail;

            var inputFile = await fileOpenPicker.PickSingleFileAsync();

            if (inputFile != null)
            {
                CurrentScriptPath = inputFile.Path;
                LoadContent(await LoadFile(inputFile));
            }
        }
        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentScriptPath == null)
                SaveAsButton_Click(sender, e);
            else
                await SaveFile(CurrentScriptPath, _auraCreatorManager.PrintLuaScript());
        }
        private async void SaveAsButton_Click(object sender, RoutedEventArgs e)
        {
            var savePicker = new FileSavePicker();
            savePicker.SuggestedStartLocation =
                Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            // Dropdown of file types the user can save the file as
            savePicker.FileTypeChoices.Add("Plain Text", new List<string>() { ".lua" });
            // Default file name if the user does not type one in or select a file to replace
            savePicker.SuggestedFileName = "MyLuaScript";

            StorageFile saveFile = await savePicker.PickSaveFileAsync();

            if (saveFile != null)
            {
                await SaveFile(saveFile, _auraCreatorManager.PrintLuaScript());
                CurrentScriptPath = saveFile.Path;
            }
        }
        private void MoreButton_Click(object sender, RoutedEventArgs e)
        {
        }
        
        private void LoadContent(string luaScript)
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

            _auraCreatorManager.ClearAllLayer();

            // Step 1 : Get global devices from GlobalSpace table and set to deviceLayerManager
            List<Device> globaldevices = GetDeviceLocationFromGlobalSpaceTable(globalspace_table);
            _auraCreatorManager.GlobalDevices = globaldevices;

            // Step 2 : Get all device layers from EventProvider table
            List<DeviceLayer> deviceLayers = ParsingEventProviderTable(eventprovider_table);
            if (deviceLayers.Count == 0)
                return;

            // Step 3 : According to device layer name, get all device zones from Viewport table
            foreach (var layer in deviceLayers)
            {
                Dictionary<int, int[]> dictionary = GetDeviceZonesFromViewportTable(viewport_table, layer.Name);
                layer.AddDeviceZones(dictionary);

                foreach (var effect in layer.TimelineEffects)
                {
                    EffectInfo ei = GetEffectInfoFromEventTable(event_table, effect.LuaName);
                    effect.Info = ei;
                }
                _auraCreatorManager.AddDeviceLayer(layer);
            }

            RefreshSpaceGrid();
            ClearEffectInfoGrid();
            LayerListView.SelectedIndex = 0;
        }
        private Dictionary<int, int[]> GetDeviceZonesFromViewportTable(Table viewport_table, string layerName)
        {
            Dictionary<int, int[]> zoneDictionary = new Dictionary<int, int[]>();
            List<Device> devices = new List<Device>();
            Table layerTable = viewport_table.Get(layerName).Table;

            foreach (var deviceKey in layerTable.Keys)
            {
                Table deviceTable = layerTable.Get(deviceKey.String).Table;
                //string deviceName = deviceTable.Get("name").String;
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

                TimelineEffect effect = new TimelineEffect(layer, type)
                {
                    LuaName = effectLuaName,
                    StartTime = startTime,
                    DurationTime = durationTime
                };
                layer.AddTimelineEffect(effect);
            }

            return layers;
        }
        private EffectInfo GetEffectInfoFromEventTable(Table eventTable, string effectLuaName)
        {
            Table effectTable = eventTable.Get(effectLuaName).Table;
            Table colorTable = effectTable.Get("initColor").Table;
            Table waveTable = effectTable.Get("wave").Table;

            return null;
        }
        private List<Device> GetDeviceLocationFromGlobalSpaceTable(Table globalspaceTable)
        {
            List<Device> devices = new List<Device>();

            foreach (var deviceKey in globalspaceTable.Keys)
            {
                Table deviceTable = globalspaceTable.Get(deviceKey.String).Table;

                int type = 0;
                switch (deviceTable.Get("DeviceType").String)
                {
                    case "Notebook": type = 0; break;
                    case "Mouse": type = 1; break;
                    case "Keyboard": type = 2; break;
                    case "Headset": type = 3; break;
                }

                Table locationTable = deviceTable.Get("location").Table;
                int x = (int)locationTable.Get("x").Number;
                int y = (int)locationTable.Get("y").Number;

                //Device d = new Device(deviceKey.String, type, x, y);
                //devices.Add(d);
            }

            return devices;
        }
    }
}

