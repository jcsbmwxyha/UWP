using System;
using System.Collections.Generic;
using System.IO;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using MoonSharp.Interpreter;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Windows.Networking.Sockets;
using Windows.UI.Core;
using Windows.UI.Xaml.Input;
using Windows.ApplicationModel.Core;
using Windows.Storage.Streams;
using System.Xml;
using System.ComponentModel;
using System.Linq;

using AuraEditor.Common;
using AuraEditor.UserControls;
using static AuraEditor.Common.ControlHelper;
using static AuraEditor.Common.EffectHelper;
using static AuraEditor.Common.LuaHelper;
using System.Collections.ObjectModel;

namespace AuraEditor
{
    public sealed partial class MainPage : Page
    {
        BackgroundWorker bgwSocketServer;

        static MainPage _instance;
        static public MainPage MainPageInstance
        {
            get { return _instance; }
        }
        public AuraCreatorManager _auraCreatorManager;
        public BitmapImage DragEffectIcon;
        
        private StorageFile m_RecentFiles_SF;
        private StorageFile m_CurrentScript_SF;
        public ObservableRecentList RecentFileList;
        
        public double TimelineScrollHorOffset
        {
            get { return (double)GetValue(ScrollHorOffseProperty); }
            set { SetValue(ScrollHorOffseProperty, (double)value); }
        }
        public static readonly DependencyProperty ScrollHorOffseProperty =
            DependencyProperty.Register("TimelineScrollHorOffset", typeof(double), typeof(MainPage),
                new PropertyMetadata(0, ScrollTimeLinePropertyChangedCallback));
        static private void ScrollTimeLinePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as MainPage).TrackScrollViewer.ChangeView((double)e.NewValue, null, null, true);
            (d as MainPage).ScaleScrollViewer.ChangeView((double)e.NewValue, null, null, true);
        }

        bool _angleImgPressing;
        public double _preAngle;
        Point AngleImgCenter;

        public MainPage()
        {
            _instance = this;
            this.InitializeComponent();
            EffectBlockListView.ItemsSource = GetCommonEffectBlocks();
            RecentFileList = new ObservableRecentList();
            _auraCreatorManager = AuraCreatorManager.Instance;

            //BackgroundWorker for Socket Server
            bgwSocketServer = new BackgroundWorker();
            bgwSocketServer.DoWork += SocketServer_DoWork;
            bgwSocketServer.RunWorkerAsync();
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            await FileOperations();
            IntializeSpaceGrid();
            InitializeDragEffectIcon();
            InitializeTimelineStructure();

            AngleImgCenter = new Point(AngleGrid.ActualWidth / 2, AngleGrid.ActualHeight / 2);
            _preAngle = 0;
            AngleTextBox.Text = "0";
        }

        private async Task FileOperations()
        {
            m_RecentFiles_SF = await TryAndGetRecentFilesSF();
            XmlDocument recentFilesXmlDoc = await GetRecentFilesXmlDoc(m_RecentFiles_SF);
            GetSortedRecentFiles(recentFilesXmlDoc);
        }
        private List<string> GetSortedRecentFiles(XmlDocument xml)
        {
            XmlNode recentfilesNode = xml.SelectSingleNode("recentfiles");
            XmlNodeList fileNodes = recentfilesNode.SelectNodes("file");
            List<string> list = new List<string>();

            for (int i = 0; i < fileNodes.Count; i++)
            {
                XmlElement element = (XmlElement)fileNodes[i];
                RecentFileList.Add(element.GetAttribute("path"));
            }

            return list;
        }
        private async Task<XmlDocument> GetRecentFilesXmlDoc(StorageFile sf)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(await sf.OpenStreamForReadAsync());

            return xml;
        }
        private async Task<StorageFile> TryAndGetRecentFilesSF()
        {
            StorageFolder folder = await StorageFolder.GetFolderFromPathAsync("C:\\ProgramData\\ASUS");
            folder = await EnterOrCreateFolder(folder, "AURA Creator");
            folder = await EnterOrCreateFolder(folder, "script");

            try
            {
                return await folder.GetFileAsync("recentfiles.xml");
            }
            catch
            {
                return await folder.CreateFileAsync("recentfiles.xml");
            }
        }
        private void FileListComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = FileListComboBox.SelectedIndex;

            // Calling InsertHead or setting SelectedIndex will call SelectionChanged again.
            // We should ingore it.
            if (index == -1 || index == 0)
                return;

            string item = FileListComboBox.Items[index] as string;
            RecentFileList.InsertHead(item);
            FileListComboBox.SelectedIndex = 0;
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

        #region Framework Element
        private void EffectRadioButton_Click(object sender, RoutedEventArgs e)
        {
            if (EffectBlockListView.Visibility == Visibility.Visible)
                EffectBlockListView.Visibility = Visibility.Collapsed;
            else
                EffectBlockListView.Visibility = Visibility.Visible;
        }
        private void AdjustEffectBlockGrid_Click(object sender, RoutedEventArgs e)
        {
            int columnSpans = Grid.GetColumnSpan(SpaceGrid);

            if (MainGrid.Children.Contains(EffectBlockScrollViewer))
            {
                MainGrid.Children.Remove(EffectBlockScrollViewer);

                Grid.SetColumn(SpaceGrid, 0);
                Grid.SetColumnSpan(SpaceGrid, columnSpans + 1);
            }
            else
            {
                Grid.SetColumn(EffectBlockScrollViewer, 0);
                MainGrid.Children.Add(EffectBlockScrollViewer);

                Grid.SetColumn(SpaceGrid, 1);
                Grid.SetColumnSpan(SpaceGrid, columnSpans - 1);
            }
        }
        private void AdjustEffectInfoGrid_Click(object sender, RoutedEventArgs e)
        {
            int columnSpans = Grid.GetColumnSpan(SpaceGrid);

            if (MainGrid.Children.Contains(EffectInfoScrollViewer))
            {
                MainGrid.Children.Remove(EffectInfoScrollViewer);

                Grid.SetColumnSpan(SpaceGrid, columnSpans + 1);
            }
            else
            {
                Grid.SetColumn(EffectInfoScrollViewer, 2);
                MainGrid.Children.Add(EffectInfoScrollViewer);

                Grid.SetColumnSpan(SpaceGrid, columnSpans - 1);
            }
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
            List<DeviceLayerItem> items =
                FindAllControl<DeviceLayerItem>(LayerListView, typeof(DeviceLayerItem));

            foreach (var item in items)
            {
                if (item.IsChecked == true)
                {
                    DeviceLayer layer = item.DataContext as DeviceLayer;

                    if (layer.TimelineEffects.Contains(_selectedEffectLine))
                        SelectedEffectLine = null;

                    _auraCreatorManager.RemoveDeviceLayer(layer);
                }
            }
        }
        private void GoLeftButton_Click(object sender, RoutedEventArgs e)
        {
            double source = ScaleScrollViewer.HorizontalOffset;
            double target = 0;
            AnimationStart(this, "TimelineScrollHorOffset", 200, source, target);
        }
        private void GoRightButton_Click(object sender, RoutedEventArgs e)
        {
            double source = ScaleScrollViewer.HorizontalOffset;
            double target = _auraCreatorManager.RightmostPosition;
            AnimationStart(this, "TimelineScrollHorOffset", 200, source, target);
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
            
            RecentFileList.InsertHead(inputFile.Path);
            FileListComboBox.SelectedIndex = 0;
            m_CurrentScript_SF = inputFile;

            // Loading script is not ready.
            return;

            string luaScript = await Windows.Storage.FileIO.ReadTextAsync(inputFile);
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
        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (m_CurrentScript_SF == null)
            {
                SaveAsButton_Click(sender, e);
                return;
            }
            else
            {
                // Prevent updates to the remote version of the file until
                // we finish making changes and call CompleteUpdatesAsync.
                Windows.Storage.CachedFileManager.DeferUpdates(m_CurrentScript_SF);
                // write to file
                await Windows.Storage.FileIO.WriteTextAsync(m_CurrentScript_SF, _auraCreatorManager.PrintLuaScript());
                // Let Windows know that we're finished changing the file so
                // the other app can update the remote version of the file.
                // Completing updates may require Windows to ask for user input.
                Windows.Storage.Provider.FileUpdateStatus status =
                    await Windows.Storage.CachedFileManager.CompleteUpdatesAsync(m_CurrentScript_SF);
            }

            //StorageFolder folder = await StorageFolder.GetFolderFromPathAsync("C:\\ProgramData\\ASUS");
            //folder = await EnterOrCreateFolder(folder, "AURA Creator");
            //folder = await EnterOrCreateFolder(folder, "script");
            //StorageFile sf =
            //    await folder.CreateFileAsync("script.lua", Windows.Storage.CreationCollisionOption.ReplaceExisting);

            //await Windows.Storage.FileIO.WriteTextAsync(sf, _auraCreatorManager.PrintLuaScript());
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

                RecentFileList.InsertHead(saveFile.Path);
                FileListComboBox.SelectedIndex = 0;
                m_CurrentScript_SF = saveFile; 
            }
        }
        private async Task<StorageFolder> EnterOrCreateFolder(StorageFolder sf, string folderName)
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
        #endregion

        async void socketstart()
        {
            Windows.Networking.Sockets.DatagramSocket socket = new Windows.Networking.Sockets.DatagramSocket();
            while (true)
            {
                socket.Dispose();
                try
                {
                    socket = new Windows.Networking.Sockets.DatagramSocket();
                    socket.MessageReceived += Socket_MessageReceived;

                    //You can use any port that is not currently in use already on the machine. We will be using two separate and random 
                    //ports for the client and server because both the will be running on the same machine.
                    string serverPort = "6667";
                    string clientPort = "8002";

                    //Because we will be running the client and server on the same machine, we will use localhost as the hostname.
                    Windows.Networking.HostName serverHost = new Windows.Networking.HostName("127.0.0.1");

                    //Bind the socket to the clientPort so that we can start listening for UDP messages from the UDP echo server.
                    await socket.BindServiceNameAsync(clientPort);
                    await socket.ConnectAsync(serverHost, serverPort);
                    //Write a message to the UDP echo server.
                    Stream streamOut = (await socket.GetOutputStreamAsync(serverHost, serverPort)).AsStreamForWrite();
                    StreamWriter writer = new StreamWriter(streamOut);
                    string message = "I'm the message from client!";
                    await writer.WriteLineAsync(message);
                    await writer.FlushAsync();
                }
                catch (Exception ex)
                {
                    Windows.Networking.Sockets.SocketErrorStatus webErrorStatus = Windows.Networking.Sockets.SocketError.GetStatus(ex.GetBaseException().HResult);
                }

                await Task.Delay(10000);
            }
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

                RescanIngroupDevices();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
        }

        private void SocketServer_DoWork(object sender, DoWorkEventArgs e)
        {
            socketstart();
        }

        private void MoreButton_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}

