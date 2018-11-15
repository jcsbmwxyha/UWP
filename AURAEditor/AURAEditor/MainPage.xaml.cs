using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Networking.Sockets;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Core.Preview;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using AuraEditor.Dialogs;
using AuraEditor.UserControls;
using static AuraEditor.AuraSpaceManager;
using static AuraEditor.Common.ControlHelper;
using static AuraEditor.Common.EffectHelper;
using Windows.Networking;

namespace AuraEditor
{
    public sealed partial class MainPage : Page
    {
        // for Socket TCP use
        private string cm;
        private string port;
        private string ip;
        BackgroundWorker bgwSocketServerRecv;
        StreamSocket socket = new StreamSocket();

        static MainPage _instance;
        static public MainPage Self
        {
            get { return _instance; }
        }
        public AuraSpaceManager SpaceManager;
        public AuraLayerManager LayerManager;
        public ConnectedDevicesDialog ConnectedDevicesDialog;
        public BitmapImage DragEffectIcon;

        ApplicationDataContainer g_LocalSettings;

        bool _angleImgPressing;
        public double _preAngle;
        Point AngleImgCenter;

        #region Key Up & Down
        public bool g_PressShift;
        public bool g_PressCtrl;
        public bool g_PressX;
        public bool g_PressC;
        public bool g_PressV;
        private void CoreWindow_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            if (args.VirtualKey == Windows.System.VirtualKey.Shift)
            {
                g_PressShift = true;
            }
            else if (args.VirtualKey == Windows.System.VirtualKey.Control)
            {
                g_PressCtrl = true;
            }
            else if (args.VirtualKey == Windows.System.VirtualKey.X)
            {
                g_PressX = true;
            }
            else if (args.VirtualKey == Windows.System.VirtualKey.C)
            {
                g_PressC = true;
            }
            else if (args.VirtualKey == Windows.System.VirtualKey.V)
            {
                g_PressV = true;
            }
            else if (args.VirtualKey == Windows.System.VirtualKey.Delete)
            {
                if (SelectedEffectLine == null)
                    return;

                Layer layer = SelectedEffectLine.Layer;
                layer.DeleteEffectLine(SelectedEffectLine.UI);
            }
        }
        private void CoreWindow_KeyUp(CoreWindow sender, KeyEventArgs args)
        {
            if (args.VirtualKey == Windows.System.VirtualKey.Shift)
            {
                g_PressShift = false;
            }
            else if (args.VirtualKey == Windows.System.VirtualKey.Control)
            {
                g_PressCtrl = false;
            }
            else if (args.VirtualKey == Windows.System.VirtualKey.X)
            {
                g_PressX = false;
            }
            else if (args.VirtualKey == Windows.System.VirtualKey.C)
            {
                g_PressC = false;
            }
            else if (args.VirtualKey == Windows.System.VirtualKey.V)
            {
                g_PressV = false;
            }
        }
        #endregion

        public MainPage()
        {
            _instance = this;
            this.InitializeComponent();
            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
            Window.Current.CoreWindow.KeyUp += CoreWindow_KeyUp;
            g_PressShift = false;
            g_PressCtrl = false;
            SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += this.OnCloseRequest;
            g_LocalSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            EffectBlockListView.ItemsSource = GetCommonEffectBlocks();
            SetDefaultPatternList();
        }
        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            await IntializeFileOperations();
            ConnectedDevicesDialog = new ConnectedDevicesDialog();
            SpaceManager = new AuraSpaceManager();
            LayerManager = new AuraLayerManager();
            InitializeDragEffectIcon();
            InitializePlayerStructure();
            SetDefaultPattern();
            await ConnectedDevicesDialog.Rescan();
            //Start SocketClient
            startclient();

            AngleImgCenter = new Point(40, 40);
            _preAngle = 0;
            AngleTextBox.Text = "0";

            LoadSettings();
        }
        private async void InitializeDragEffectIcon()
        {
            DragEffectIcon = new BitmapImage();

            string CountriesFile = @"Assets\EffectBlock\drag_effect_ic.png";
            StorageFolder InstallationFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            StorageFile file = await InstallationFolder.GetFileAsync(CountriesFile);

            using (Windows.Storage.Streams.IRandomAccessStream fileStream =
                    await file.OpenAsync(Windows.Storage.FileAccessMode.Read))
            {
                DragEffectIcon.SetSource(fileStream);
            }
        }
        private void LoadSettings()
        {
            bool successful = float.TryParse(g_LocalSettings.Values["SpaceZooming"] as string, out float percent);
            if (successful)
                SpaceManager.SetSpaceZoomPercent(percent);
            else
            {
                SpaceManager.SetSpaceZoomPercent(50);
                SpaceZoomButton.Content = "50 %";
            }

            successful = int.TryParse(g_LocalSettings.Values["LayerLevel"] as string, out int level);
            if (successful)
                LayerZoomSlider.Value = level;
            else
            {
                LayerZoomSlider.Value = 2;
            }
        }

        #region Framework Element
        private async void ConnectedDevicesButton_Click(object sender, RoutedEventArgs e)
        {
            await ConnectedDevicesDialog.ShowAsync();
        }
        private void SetLayerButton_Click(object sender, RoutedEventArgs e)
        {
            Layer layer = new Layer();
            List<int> selectedIndex;

            layer.Name = "Layer " + (LayerManager.GetLayerCount() + 1);

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

            LayerManager.AddLayer(layer);
            SpaceManager.UnselectAllZones();
            SetLayerButton.IsEnabled = false;
            NeedSave = true;
        }
        private void SortDeviceButton_Checked(object sender, RoutedEventArgs e)
        {
            EditOKButton.IsEnabled = true;
            ShowMask("Device Sorting", "Save", "Cancel");
            SpaceManager.SetSpaceStatus(SpaceStatus.DragingDevice);
        }

        private void LeftSidePanelButton_Click(object sender, RoutedEventArgs e)
        {
            int columnSpans = Grid.GetColumnSpan(SpaceGrid);

            if (MainGrid.Children.Contains(EffectBlockScrollViewer))
            {
                MainGrid.Children.Remove(EffectBlockScrollViewer);
                MainGrid.Children.Remove(MaskGrid1);
                Grid.SetColumn(SpaceGrid, 0);
                Grid.SetColumnSpan(SpaceGrid, columnSpans + 1);

                LeftSideOpenedButton.Visibility = Visibility.Collapsed;
                LeftSideClosedButton.Visibility = Visibility.Visible;
            }
            else
            {
                Grid.SetColumn(EffectBlockScrollViewer, 0);
                MainGrid.Children.Add(EffectBlockScrollViewer);
                Grid.SetColumn(MaskGrid1, 0);
                MainGrid.Children.Add(MaskGrid1);
                Grid.SetColumn(SpaceGrid, 1);
                Grid.SetColumnSpan(SpaceGrid, columnSpans - 1);

                LeftSideOpenedButton.Visibility = Visibility.Visible;
                LeftSideClosedButton.Visibility = Visibility.Collapsed;
            }
        }
        private void RightSidePanelButton_Click(object sender, RoutedEventArgs e)
        {
            int columnSpans = Grid.GetColumnSpan(SpaceGrid);

            if (MainGrid.Children.Contains(EffectInfoScrollViewer))
            {
                MainGrid.Children.Remove(EffectInfoScrollViewer);
                MainGrid.Children.Remove(MaskGrid2);
                Grid.SetColumnSpan(SpaceGrid, columnSpans + 1);

                RightSideOpenedButton.Visibility = Visibility.Collapsed;
                RightSideClosedButton.Visibility = Visibility.Visible;
            }
            else
            {
                Grid.SetColumn(EffectInfoScrollViewer, 2);
                MainGrid.Children.Add(EffectInfoScrollViewer);
                Grid.SetColumn(MaskGrid2, 2);
                MainGrid.Children.Add(MaskGrid2);
                Grid.SetColumnSpan(SpaceGrid, columnSpans - 1);

                RightSideOpenedButton.Visibility = Visibility.Visible;
                RightSideClosedButton.Visibility = Visibility.Collapsed;
            }
        }
        private void SpaceZoom_Click(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuFlyoutItem;
            string itemText = item.Text;

            if (itemText == SpaceZoomButton.Content as string)
                return;

            float percent = float.Parse(itemText.Replace(" %", ""));
            SpaceManager.SetSpaceZoomPercent(percent);
        }

        private void LayerListView_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            Layer layer = e.Items[0] as Layer;
            e.Data.Properties.Add("layer", layer);

            if (layer != null)
            {

            }
        }
        private void TrashCanButton_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data == null)
                return;

            e.DragUIOverride.IsCaptionVisible = false;
            e.DragUIOverride.IsGlyphVisible = false;

            var pair = e.Data.Properties.FirstOrDefault();
            Layer layer = pair.Value as Layer;
            if (layer != null)
                e.AcceptedOperation = DataPackageOperation.Copy;
        }
        private void TrashCanButton_Drop(object sender, DragEventArgs e)
        {
            var pair = e.Data.Properties.FirstOrDefault();
            Layer layer = pair.Value as Layer;
            LayerManager.RemoveLayer(layer);
            SpaceManager.SetSpaceStatus(SpaceStatus.Normal);
            SelectedEffectLine = null;
            NeedSave = true;
        }
        private void TrashCanButton_Click(object sender, RoutedEventArgs e)
        {
            Layer layer = LayerManager.GetCheckedLayer();
            if (layer != null)
            {
                LayerManager.RemoveLayer(layer);
                SpaceManager.SetSpaceStatus(SpaceStatus.Normal);
                SelectedEffectLine = null;
                NeedSave = true;
            }
        }
        #endregion

        #region Mask
        public void ShowReEditMask(Layer layer)
        {
            ShowMask("Edit layer: " + layer.Name, "Save", "Cancel");
        }
        private void EditOKButton_Click(object sender, RoutedEventArgs e)
        {
            if (SpaceManager.GetSpaceStatus() == SpaceStatus.ReEditing)
            {
                Layer layer = LayerManager.GetCheckedLayer();
                List<int> selectedIndex;

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

                    layer.SetDeviceZones(d.Type, selectedIndex.ToArray());
                }

                NeedSave = true;
                SpaceManager.SetSpaceStatus(SpaceStatus.WatchingLayer);
            }
            else
                SpaceManager.SetSpaceStatus(SpaceStatus.Normal);
            HideMask();
        }
        private void EditCancelButton_Click(object sender, RoutedEventArgs e)
        {
            SpaceManager.SetSpaceStatus(SpaceStatus.Normal);
            HideMask();
        }
        private void ShowMask(string descriptionText, string okText, string cancelText)
        {
            EditBarTextBlock.Text = descriptionText;
            EditOKButton.Content = okText;
            //EditCancelButton.Content = cancelText;

            EditBar.Visibility = Visibility.Visible;
            MaskGrid1.Visibility = Visibility.Visible;
            MaskGrid2.Visibility = Visibility.Visible;
            MaskGrid3.Visibility = Visibility.Visible;
        }
        private void HideMask()
        {
            EditBar.Visibility = Visibility.Collapsed;
            MaskGrid1.Visibility = Visibility.Collapsed;
            MaskGrid2.Visibility = Visibility.Collapsed;
            MaskGrid3.Visibility = Visibility.Collapsed;
        }
        #endregion

        private void SocketServerRecv_DoWork(object sender, DoWorkEventArgs e)
        {
            receivedata();
        }

        //Use senddata(string) can send string to server
        async void senddata(string request)
        {
            Stream streamOut = socket.OutputStream.AsStreamForWrite();
            StreamWriter writer = new StreamWriter(streamOut);
            await writer.WriteLineAsync(request + "\n");
            await writer.FlushAsync();
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                StatusTextBlock.Text = "Send successful";
            });
        }

        async void receivedata()
        {
            bool IsConnection = false;
            do
            {
                try
                {
                    port = "6667";
                    ip = "127.0.0.1";
                    cm = "I'm the message from client";
                    HostName serverHost = new HostName("localhost");
                    string serverPort = port;
                    await socket.ConnectAsync(serverHost, serverPort);
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        StatusTextBlock.Text = "Connect...\n";
                    });
                    IsConnection = true;
                }
                catch (Exception ex)
                {
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        StatusTextBlock.Text = "Server not start!!";
                    });
                    await Task.Delay(2000);
                }
            } while (!IsConnection);

            while (true)
            {
                try
                {
                    string response;
                    Stream inputStream = socket.InputStream.AsStreamForRead();
                    StreamReader streamReader = new StreamReader(inputStream);
                    response = await streamReader.ReadLineAsync();
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        //from Service message
                        StatusTextBlock.Text = "Service : " + response;
                        ConnectedDevicesDialog.Rescan();
                    });
                }
                catch (Exception ex)
                {

                }
            }
        }

        void startclient()
        {
            //BackgroundWorker for Socket Server
            bgwSocketServerRecv = new BackgroundWorker();
            bgwSocketServerRecv.DoWork += SocketServerRecv_DoWork;
            bgwSocketServerRecv.RunWorkerAsync();
        }

        private void DebugButton_Click(object sender, RoutedEventArgs e)
        {
            ConnectedDevicesDialog.Rescan();
        }
    }
}

