using System;
using System.Collections.Generic;
using System.IO;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.UI.Core;
using System.ComponentModel;
using AuraEditor.UserControls;
using static AuraEditor.Common.ControlHelper;
using static AuraEditor.Common.EffectHelper;
using static AuraEditor.AuraSpaceManager;
using Windows.UI.Core.Preview;

namespace AuraEditor
{
    public sealed partial class MainPage : Page
    {
        BackgroundWorker bgwSocketServer;

        static MainPage _instance;
        static public MainPage Self
        {
            get { return _instance; }
        }
        public AuraSpaceManager SpaceManager;
        public AuraLayerManager LayerManager;
        public BitmapImage DragEffectIcon;

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
            SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += this.OnCloseRequest;

            EffectBlockListView.ItemsSource = GetCommonEffectBlocks();

            //BackgroundWorker for Socket Server
            bgwSocketServer = new BackgroundWorker();
            bgwSocketServer.DoWork += SocketServer_DoWork;
            bgwSocketServer.RunWorkerAsync();

            SetDefaultPatternList();
        }
        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            await IntializeFileOperations();
            SpaceManager = new AuraSpaceManager();
            LayerManager = new AuraLayerManager();
            InitializeDragEffectIcon();
            InitializePlayerStructure();
            SetDefaultPattern();

            AngleImgCenter = new Point(40, 40);
            _preAngle = 0;
            AngleTextBox.Text = "0";
            
            Bindings.Update();
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
        private void SetLayerButton_Click(object sender, RoutedEventArgs e)
        {
            DeviceLayer layer = new DeviceLayer();
            List<int> selectedIndex;

            layer.Name = "Layer " + (LayerManager.GetLayerCount());

            foreach (Device d in SpaceManager.GlobalDevices)
            {
                selectedIndex = new List<int>();

                foreach (var zone in d.LightZones)
                {
                    if (zone.Selected == true)
                    {
                        selectedIndex.Add(zone.Index);
                    }
                }

                if (selectedIndex.Count != 0)
                    layer.AddDeviceZones(d.Type, selectedIndex.ToArray());
            }

            LayerManager.AddDeviceLayer(layer);
            SpaceManager.UnselectAllZones();
            NeedSave = true;
        }
        private void DragDevImgToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            SpaceManager.SetSpaceStatus(SpaceStatus.DragingDevice);
        }
        private void DragDevImgToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            SpaceManager.SetSpaceStatus(SpaceStatus.Normal);
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

                    LayerManager.RemoveDeviceLayer(layer);
                    SpaceManager.SetSpaceStatus(SpaceStatus.Normal);
                    NeedSave = true;
                }
            }
        }
        private void PlusButton_Click(object sender, RoutedEventArgs e)
        {
            ZoomSlider.Value += 1;
        }
        private void MinusButton_Click(object sender, RoutedEventArgs e)
        {
            ZoomSlider.Value -= 1;
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
                    StatusTextBlock.Text = "Service : " + message;
                });

                SpaceManager.RescanIngroupDevices();
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

        private void SpaceZoom_Click(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuFlyoutItem;
            string itemName = item.Name;

            switch (itemName)
            {
                case "Zoom_0":
                    SpaceManager.SpaceZoomChanged("0 %"); SpaceZoomButton.Content = "0 %"; break;
                case "Zoom_50":
                    SpaceManager.SpaceZoomChanged("50 %"); SpaceZoomButton.Content = "50 %"; break;
                case "Zoom_100":
                    SpaceManager.SpaceZoomChanged("100 %"); SpaceZoomButton.Content = "100 %"; break;
            }
        }

        private void DebugButton_Click(object sender, RoutedEventArgs e)
        {
            SpaceManager.RescanIngroupDevices();
        }
    }
}

