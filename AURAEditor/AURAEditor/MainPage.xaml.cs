using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Input;
using Windows.UI.Xaml.Documents;
using MoonSharp.Interpreter;
using Windows.Storage.Pickers;
using AuraEditor.Common;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using Windows.Networking.Sockets;
using Windows.UI.Core;
using Windows.UI.Xaml.Input;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml.Media.Animation;
using Windows.Storage.Streams;

// 空白頁項目範本已記錄在 https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x404

namespace AuraEditor
{
    /// <summary>
    /// 可以在本身使用或巡覽至框架內的空白頁面。
    /// </summary>

    public class LedUI
    {
        public int PhyIndex;
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
        public int ZIndex;
    }
    public class DeviceContent
    {
        public string DeviceName;
        public int DeviceType;
        public int UI_Width;
        public int UI_Height;
        public List<LedUI> Leds;
        public BitmapImage Image;

        public DeviceContent()
        {
            Leds = new List<LedUI>();
        }
    }
    
    public sealed partial class MainPage : Page
    {
        static MainPage _instance;
        static public MainPage MainPageInstance
        {
            get { return _instance; }
        }
        public AuraCreatorManager _auraCreatorManager;
        public BitmapImage DragEffectIcon;

        public MainPage()
        {
            _instance = this;
            this.InitializeComponent();
            EffectListView.ItemsSource = EffectHelper.GetCommonEffectList();
            TriggerEventListView.ItemsSource = EffectHelper.GetTriggerEffectList();
            OtherTriggerEventListView.ItemsSource = EffectHelper.GetOtherTriggerEffectList();
            _auraCreatorManager = AuraCreatorManager.Instance;
            _mouseEventCtrl = IntializeMouseEventCtrl();
            SetSpaceStatus(SpaceStatus.Normal);
            InitializeDragEffectIcon();
        }
        private async void InitializeDragEffectIcon()
        {
            DragEffectIcon = new BitmapImage();

            string CountriesFile = @"Assets\effectUI.png";
            StorageFolder InstallationFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            StorageFile file = await InstallationFolder.GetFileAsync(CountriesFile);

            using (Windows.Storage.Streams.IRandomAccessStream fileStream =
                    await file.OpenAsync(Windows.Storage.FileAccessMode.Read))
            {
                DragEffectIcon.SetSource(fileStream);
            }
        }
        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeTimelineStructure();
            await GetCurrentDevicesTest();
            RefreshSpaceGrid();

            // for receive cmd form Service
            socketstart();

            // Pre add for debug
            /*
            for (int i = 0; i < 6; i++)
            {
                DeviceLayer dg = new DeviceLayer();
                dg.LayerName = "123";
                _deviceLayerManager.AddDeviceLayer(dg);
            }
            */
        }
        private async Task GetCurrentDevicesTest()
        {
            try
            {
                DeviceContent deviceContent = await GetDeviceContent("GL504");
                Device device = CreateDeviceFromContent(deviceContent, new Point(1, 1));
                _auraCreatorManager.GlobalDevices.Add(device);
            }
            catch
            {

            }
        }
        //private async Task<DeviceContent> GetDeviceContent(string modelName)
        //{
        //    try
        //    {
        //        DeviceContent deviceContent = new DeviceContent();
        //        Script script = new Script();
        //        string scriptText;
        //        Table deviceContent_table;
        //        Table leds_table;
        //        Table led_table;
        //        Table leftTop_table;
        //        Table rightBottom_table;
        //        string auraCreatorFolderPath = "C:\\ProgramData\\ASUS\\AURA Creator\\Devices\\";

        //        StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(auraCreatorFolderPath + modelName);
        //        System.Diagnostics.Debug.WriteLine("Get " + auraCreatorFolderPath + " folder successfully");
        //        StorageFile txtFile = await folder.GetFileAsync(modelName + ".txt");
        //        StorageFile pngFile = await folder.GetFileAsync(modelName + ".png");
        //        System.Diagnostics.Debug.WriteLine("Get " + modelName + " StorageFile successfully");

        //        deviceContent.DeviceName = modelName;
        //        if (modelName == "GLADIUS II")
        //            deviceContent.DeviceType = 1;
        //        else
        //            deviceContent.DeviceType = 0;

        //        scriptText = await Windows.Storage.FileIO.ReadTextAsync(txtFile);
        //        deviceContent_table = script.DoString(scriptText).Table;

        //        deviceContent.UI_Width = (int)deviceContent_table.Get("width").Number;
        //        deviceContent.UI_Height = (int)deviceContent_table.Get("height").Number;
        //        leds_table = deviceContent_table.Get("leds").Table;

        //        foreach (var deviceKey in leds_table.Keys)
        //        {
        //            led_table = leds_table.Get(deviceKey.Number).Table;

        //            leftTop_table = led_table.Get("leftTop").Table;
        //            rightBottom_table = led_table.Get("rightBottom").Table;

        //            deviceContent.Leds.Add(
        //                new LedUI()
        //                {
        //                    PhyIndex = (int)deviceKey.Number,
        //                    Left = (int)leftTop_table.Get("x").Number,
        //                    Top = (int)leftTop_table.Get("y").Number,
        //                    Right = (int)rightBottom_table.Get("x").Number,
        //                    Bottom = (int)rightBottom_table.Get("y").Number,
        //                }
        //            );
        //        }

        //        if (pngFile != null)
        //        {
        //            // Open a stream for the selected file.
        //            // The 'using' block ensures the stream is disposed
        //            // after the image is loaded.
        //            using (Windows.Storage.Streams.IRandomAccessStream fileStream =
        //                await pngFile.OpenAsync(Windows.Storage.FileAccessMode.Read))
        //            {
        //                // Set the image source to the selected bitmap.
        //                Windows.UI.Xaml.Media.Imaging.BitmapImage bitmapImage =
        //                    new Windows.UI.Xaml.Media.Imaging.BitmapImage();

        //                bitmapImage.SetSource(fileStream);
        //                deviceContent.Image = bitmapImage;
        //            }
        //        }

        //        return deviceContent;
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}
        private async Task<DeviceContent> GetDeviceContent(string modelName)
        {
            try
            {
                DeviceContent deviceContent = new DeviceContent();
                string auraCreatorFolderPath = "C:\\ProgramData\\ASUS\\AURA Creator\\Devices\\";

                StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(auraCreatorFolderPath + modelName);
                StorageFile csvFile = await folder.GetFileAsync(modelName + ".csv");
                StorageFile pngFile = await folder.GetFileAsync(modelName + ".png");

                deviceContent.DeviceName = modelName;
                if (modelName == "GLADIUS II")
                    deviceContent.DeviceType = 1;
                else
                    deviceContent.DeviceType = 0;


                if (csvFile != null)
                {
                    using (CsvFileReader csvReader = new CsvFileReader(await csvFile.OpenStreamForReadAsync()))
                    {
                        CsvRow row = new CsvRow();
                        while (csvReader.ReadRow(row))
                        {
                            if (row[0] == "Width")
                                deviceContent.UI_Width = Int32.Parse(row[1]);

                            else if (row[0] == "Height")
                                deviceContent.UI_Height = Int32.Parse(row[1]);

                            else if (row[0].Contains("LED"))
                            {
                                deviceContent.Leds.Add(
                                    new LedUI(){
                                        PhyIndex = Int32.Parse(row[1]),
                                        Left = Int32.Parse(row[4]),
                                        Top = Int32.Parse(row[5]),
                                        Right = Int32.Parse(row[6]),
                                        Bottom = Int32.Parse(row[7]),
                                        ZIndex = Int32.Parse(row[8]),
                                    });
                            }
                        }
                    }
                }

                if (pngFile != null)
                {
                    using (IRandomAccessStream fileStream = await pngFile.OpenAsync(FileAccessMode.Read))
                    {
                        BitmapImage bitmapImage = new BitmapImage();

                        bitmapImage.SetSource(fileStream);
                        deviceContent.Image = bitmapImage;
                    }
                }

                return deviceContent;
            }
            catch
            {
                return null;
            }
        }
        private Device CreateDeviceFromContent(DeviceContent deviceContent, Point devicePosition)
        {
            Device device;
            CompositeTransform ct;
            Image img;
            List<LightZone> zones = new List<LightZone>();

            ct = new CompositeTransform
            {
                TranslateX = Constants.GridLength * devicePosition.X,
                TranslateY = Constants.GridLength * devicePosition.Y
            };

            img = new Image
            {
                RenderTransform = ct,
                Width = Constants.GridLength * deviceContent.UI_Width,
                Height = Constants.GridLength * deviceContent.UI_Height,
                Source = deviceContent.Image,
                ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY,
                Stretch = Stretch.Fill,
            };

            device = new Device(img)
            {
                DeviceName = deviceContent.DeviceName,
                DeviceType = deviceContent.DeviceType,
                X = devicePosition.X,
                Y = devicePosition.Y,
                W = deviceContent.UI_Width,
                H = deviceContent.UI_Height,
                DeviceImg = img,
            };

            for (int idx = 0; idx < deviceContent.Leds.Count; idx++)
            {
                LedUI led = deviceContent.Leds[idx];
                zones.Add(
                    new LightZone(idx, devicePosition, led)
                    );
            }

            device.LightZones = zones.ToArray();
            return device;
        }

        #region Framework Element
        private void EffectRadioButton_Click(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            FrameworkElement fe;

            if (rb.Name == "EffectRadioButton")
                fe = EffectListView;
            else if (rb.Name == "EventRadioButton")
                fe = EventStackPanel;
            else if (rb.Name == "TriggerEventRadioButton")
                fe = TriggerEventListView;
            else // OtherTriggerEventToggleButton
                fe = OtherTriggerEventListView;

            if (fe == null)
                return;
            else if (fe.Visibility == Visibility.Visible)
                fe.Visibility = Visibility.Collapsed;
            else
                fe.Visibility = Visibility.Visible;
        }
        //private void EffectListView_DragStarting(object sender, DragItemsStartingEventArgs e)
        //{
        //    UpdateSpaceGridOperations(SpaceStatus.DragingEffectListItem);
        //    var item = e.Items[0] as string;
        //    e.Data.SetText(item);
        //    e.Data.RequestedOperation = DataPackageOperation.Copy;
        //
        //    if (EffectHelper.IsTriggerEffects(item))
        //        _auraCreatorManager.ShowTriggerDeviceLayer();
        //}
        //private void EffectListView_DragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
        //{
        //    UpdateSpaceGridOperations(SpaceStatus.Normal);
        //    _auraCreatorManager.HideTriggerDeviceLayer();
        //}
        private void SpaceZoomComboxBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string content = e.AddedItems[0].ToString();
            switch (content)
            {
                case "0 %":
                    SpaceAreaScrollViewer.ChangeView(SpaceAreaScrollViewer.HorizontalOffset,
                        SpaceAreaScrollViewer.VerticalOffset, 1, true);
                    break;
                case "50 %":
                    SpaceAreaScrollViewer.ChangeView(SpaceAreaScrollViewer.HorizontalOffset,
                        SpaceAreaScrollViewer.VerticalOffset, 1.5f, true);
                    break;
                case "100 %":
                    SpaceAreaScrollViewer.ChangeView(SpaceAreaScrollViewer.HorizontalOffset,
                        SpaceAreaScrollViewer.VerticalOffset, 2, true);
                    break;
            }

        }
        private void HideEffectList_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindowRow1.ColumnDefinitions[0].ActualWidth < 100)
                MainWindowRow1.ColumnDefinitions[0].Width = new GridLength(300);
            else
                MainWindowRow1.ColumnDefinitions[0].Width = new GridLength(10);
        }
        private void HideEffectInfo_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindowRow1.ColumnDefinitions[2].ActualWidth < 100)
                MainWindowRow1.ColumnDefinitions[2].Width = new GridLength(200);
            else
                MainWindowRow1.ColumnDefinitions[2].Width = new GridLength(10);
        }
        private void DeleteItem_DragEnter(object sender, DragEventArgs e)
        {
            // Trash only accepts text
            e.AcceptedOperation = DataPackageOperation.Move;
            // We don't want to show the Move icon
            e.DragUIOverride.IsGlyphVisible = false;
            e.DragUIOverride.Caption = "Drop item here to remove it from selection";
        }
        private void DeleteItem_Drop(object sender, DragEventArgs e)
        {
            //if (e.DataView.Contains(StandardDataFormats.Text))
            //{
            //    canDelete = true;
            //}
        }
        private void TrashCanButton_Click(object sender, RoutedEventArgs e)
        {
            List<DeviceLayerListViewItem> items =
                Common.ControlHelper.FindAllControl<DeviceLayerListViewItem>(LayerListView, typeof(DeviceLayerListViewItem));

            foreach (var item in items)
            {
                if (item.IsChecked == true)
                {
                    DeviceLayer layer = item.DataContext as DeviceLayer;

                    if (layer is TriggerDeviceLayer)
                        continue;

                    if (layer.Effects.Contains(_selectedEffectLine))
                        SelectedEffectLine = null;

                    _auraCreatorManager.RemoveDeviceLayer(layer);
                }
            }
        }
        #endregion

        #region Lua script
        private async void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker fileOpenPicker = new FileOpenPicker();
            fileOpenPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            fileOpenPicker.FileTypeFilter.Add(".lua");
            fileOpenPicker.ViewMode = PickerViewMode.Thumbnail;

            var inputFile = await fileOpenPicker.PickSingleFileAsync();

            if (inputFile == null)
            {
                // The user cancelled the picking operation
                return;
            }

            string luaScript = await Windows.Storage.FileIO.ReadTextAsync(inputFile);
            luaScript = luaScript.Replace("require(\"script//global\")", "");

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
            _auraCreatorManager.SetGlobalDevices(globaldevices);

            // Step 2 : Get all device layers from EventProvider table
            List<DeviceLayer> deviceLayers = ParsingEventProviderTable(eventprovider_table);
            if (deviceLayers.Count == 0)
                return;

            // Step 3 : According to device layer name, get all device zones from Viewport table
            foreach (var layer in deviceLayers)
            {
                Dictionary<int, int[]> dictionary = GetDeviceZonesFromViewportTable(viewport_table, layer.LayerName);
                layer.AddDeviceZones(dictionary);

                foreach (var effect in layer.Effects)
                {
                    EffectInfo ei = GetEffectInfoFromEventTable(event_table, effect.EffectLuaName);
                    effect.Info = ei;
                }
                _auraCreatorManager.AddDeviceLayer(layer);
            }

            RefreshSpaceGrid();
            ClearEffectInfoGrid();
            LayerListView.SelectedIndex = 0;
        }
        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            StorageFolder folder = await StorageFolder.GetFolderFromPathAsync("C:\\ProgramData\\ASUS");
            folder = await CheckOrCreateFolder(folder, "AURA Creator");
            folder = await CheckOrCreateFolder(folder, "script");
            StorageFile sf =
                await folder.CreateFileAsync("script.lua", Windows.Storage.CreationCollisionOption.ReplaceExisting);

            await Windows.Storage.FileIO.WriteTextAsync(sf, _auraCreatorManager.PrintLuaScript());
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
                // Prevent updates to the remote version of the file until
                // we finish making changes and call CompleteUpdatesAsync.
                Windows.Storage.CachedFileManager.DeferUpdates(saveFile);
                // write to file
                await Windows.Storage.FileIO.WriteTextAsync(saveFile, _auraCreatorManager.PrintLuaScript());
                // Let Windows know that we're finished changing the file so
                // the other app can update the remote version of the file.
                // Completing updates may require Windows to ask for user input.
                Windows.Storage.Provider.FileUpdateStatus status =
                    await Windows.Storage.CachedFileManager.CompleteUpdatesAsync(saveFile);
            }
        }
        private async Task<StorageFolder> CheckOrCreateFolder(StorageFolder sf, string folderName)
        {
            IReadOnlyList<StorageFolder> folderList = await sf.GetFoldersAsync();

            foreach (StorageFolder folder in folderList)
            {
                if (folder.Name == folderName)
                    return folder;
            }

            return await sf.CreateFolderAsync(folderName);
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

                DeviceLayer layer = layers.Find(x => x.LayerName == layerName);

                if (layer == null)
                {
                    layer = new DeviceLayer(layerName);
                    layers.Add(layer);
                }

                string effectLuaName = t.Get("Effect").String;
                double startTime = t.Get("Delay").Number;
                double durationTime = t.Get("Duration").Number;
                int type = EffectHelper.GetEffectIndex(effectLuaName);

                Effect effect = new Effect(layer, type)
                {
                    EffectLuaName = effectLuaName,
                    StartTime = startTime,
                    DurationTime = durationTime
                };
                layer.AddEffect(effect);
            }

            return layers;
        }
        private EffectInfo GetEffectInfoFromEventTable(Table eventTable, string effectLuaName)
        {
            Table effectTable = eventTable.Get(effectLuaName).Table;
            Table colorTable = effectTable.Get("initColor").Table;
            Table waveTable = effectTable.Get("wave").Table;

            EffectInfo ei = new EffectInfo();

            if (!effectLuaName.Contains("Rainbow") && !effectLuaName.Contains("Smart") && !effectLuaName.Contains("ColorCycle"))
            {
                ei.Color = AuraEditorColorHelper.HslToRgb(
                    colorTable.Get("alpha").Number,
                    colorTable.Get("hue").Number,
                    colorTable.Get("saturation").Number,
                    colorTable.Get("lightness").Number
                    );
            }

            switch (waveTable.Get("waveType").String)
            {
                case "SineWave": ei.WaveType = 0; break;
                case "HalfSineWave": ei.WaveType = 1; break;
                case "QuarterSineWave": ei.WaveType = 2; break;
                case "SquareWave": ei.WaveType = 3; break;
                case "TriangleWave": ei.WaveType = 4; break;
                case "SawToothleWave": ei.WaveType = 5; break;
            }

            ei.Min = waveTable.Get("min").Number;
            ei.Max = waveTable.Get("max").Number;
            ei.WaveLength = waveTable.Get("waveLength").Number;
            ei.Freq = waveTable.Get("freq").Number;
            ei.Phase = waveTable.Get("phase").Number;
            ei.Start = waveTable.Get("start").Number;
            ei.Velocity = waveTable.Get("velocity").Number;

            return ei;
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
        #endregion
        
        async void socketstart()
        {
            Windows.Networking.Sockets.DatagramSocket socket = new Windows.Networking.Sockets.DatagramSocket();
            socket.MessageReceived += Socket_MessageReceived;
            string serverPort = "6667";
            string clientPort = "8002";
            Windows.Networking.HostName serverHost = new Windows.Networking.HostName("127.0.0.1");

            await socket.BindServiceNameAsync(clientPort);
            await socket.ConnectAsync(serverHost, serverPort);
            Stream streamOut = (await socket.GetOutputStreamAsync(serverHost, serverPort)).AsStreamForWrite();
            StreamWriter writer = new StreamWriter(streamOut);
            string message = "client";
            await writer.WriteLineAsync(message);
            await writer.FlushAsync();
        }
        private async void Socket_MessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
            try
            {
                Stream streamIn = args.GetDataStream().AsStreamForRead();
                StreamReader reader = new StreamReader(streamIn);
                string message = await reader.ReadLineAsync();

                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    //from Service message
                    txtresult.Text = "Service : " + message;
                });

                if (message == "1 : GLADIUS II")
                {
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        DeviceContent deviceContent = await GetDeviceContent("GLADIUS II");
                        Device device = CreateDeviceFromContent(deviceContent, new Point(25, 3));

                        _auraCreatorManager.GlobalDevices.Add(device);
                        RefreshSpaceGrid();
                    });
                }
                else if (message == "0 : GLADIUS II")
                {
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        Device device = _auraCreatorManager.GlobalDevices.Find(x => x.DeviceType == 1);

                        if (device != null)
                        {
                            _auraCreatorManager.GlobalDevices.Remove(device);
                            RefreshSpaceGrid();
                        }
                    });
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
        }
    }
}

