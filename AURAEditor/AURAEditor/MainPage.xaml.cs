using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Core.Preview;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using AuraEditor.Dialogs;
using AuraEditor.Models;
using AuraEditor.Pages;
using AuraEditor.UserControls;
using static AuraEditor.Common.ControlHelper;
using static AuraEditor.Common.AuraEditorColorHelper;
using static AuraEditor.Common.EffectHelper;
using static AuraEditor.Common.MetroEventSource;
using static AuraEditor.Common.StorageHelper;
using static AuraEditor.Pages.SpacePage;
using Windows.Foundation;
using AuraEditor.Common;

namespace AuraEditor
{
    public sealed partial class MainPage : Page
    {
        static MainPage _instance;
        static public MainPage Self
        {
            get { return _instance; }
        }
        public SpacePage SpacePage;
        public LayerPage LayerPage;
        public ConnectedDevicesDialog ConnectedDevicesDialog;
        ApplicationDataContainer g_LocalSettings;
        public RecentColor[] g_RecentColor = new RecentColor[8];
        private Dictionary<DeviceModel, Point> oldSortingPositions;

        public TimelineEffect SelectedEffect
        {
            get
            {
                return LayerPage.CheckedEffect;
            }
            set
            {
                LayerPage.CheckedEffect = value;
            }
        }

        #region -- Key Up & Down --
        public bool g_PressShift;
        public bool g_PressCtrl;
        public bool g_CanPaste = true;

        private void CoreWindow_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            if (args.VirtualKey == Windows.System.VirtualKey.Z)
            {
                SpacePage.OnZKeyPressed();
            }
            else if (args.VirtualKey == Windows.System.VirtualKey.Shift)
            {
                g_PressShift = true;
            }
            else if (args.VirtualKey == Windows.System.VirtualKey.Control)
            {
                g_PressCtrl = true;
            }
            else if (args.VirtualKey == Windows.System.VirtualKey.X)
            {
                if (SelectedEffect == null)
                    return;

                if (g_PressCtrl == true)
                {
                    LayerPage.CopiedEffect = TimelineEffect.CloneEffect(SelectedEffect);

                    LayerModel layer = SelectedEffect.Layer;
                    layer.DeleteEffectLine(SelectedEffect);
                }
            }
            else if (args.VirtualKey == Windows.System.VirtualKey.C)
            {
                if (SelectedEffect == null)
                    return;

                if (g_PressCtrl == true)
                    LayerPage.CopiedEffect = TimelineEffect.CloneEffect(SelectedEffect);
            }
            else if (args.VirtualKey == Windows.System.VirtualKey.V)
            {
                if (LayerPage.CheckedLayer == null || g_PressCtrl == false || g_CanPaste == false || LayerPage.CopiedEffect == null)
                    return;

                g_CanPaste = false;

                var copy = TimelineEffect.CloneEffect(LayerPage.CopiedEffect);

                if (SelectedEffect != null)
                {
                    copy.Left = SelectedEffect.Right;
                    SelectedEffect.Layer.InsertTimelineEffectFitly(copy);
                }
                else
                {
                    LayerPage.CheckedLayer.InsertTimelineEffectFitly(TimelineEffect.CloneEffect(copy));
                }

                TimeSpan delay = TimeSpan.FromMilliseconds(400);
                ThreadPoolTimer DelayTimer = ThreadPoolTimer.CreateTimer(
                    (source) =>
                    {
                        Dispatcher.RunAsync(
                           CoreDispatcherPriority.High,
                           () =>
                           {
                               g_CanPaste = true;
                           });
                    }, delay);
            }
            else if (args.VirtualKey == Windows.System.VirtualKey.Delete)
            {
                if (SelectedEffect == null)
                    return;

                LayerModel layer = SelectedEffect.Layer;
                layer.DeleteEffectLine(SelectedEffect);
            }
        }

        private void CoreWindow_KeyUp(CoreWindow sender, KeyEventArgs args)
        {
            if (args.VirtualKey == Windows.System.VirtualKey.Z)
            {
                SpacePage.OnZKeyRelease();
            }
            else if (args.VirtualKey == Windows.System.VirtualKey.Shift)
            {
                g_PressShift = false;
            }
            else if (args.VirtualKey == Windows.System.VirtualKey.Control)
            {
                g_PressCtrl = false;
            }
        }
        #endregion

        public MainPage()
        {
            _instance = this;
            this.InitializeComponent();
            for (int i = 0; i < 8; i++)
                g_RecentColor[i] = new RecentColor();
            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
            Window.Current.CoreWindow.KeyUp += CoreWindow_KeyUp;
            g_PressShift = false;
            g_PressCtrl = false;
            SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += this.OnCloseRequest;
            g_LocalSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            EffectBlockListView.ItemsSource = GetCommonEffectBlocks();
            oldSortingPositions = new Dictionary<DeviceModel, Point>();
        }
        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            Log.Debug("[MainPage_Loaded] Intialize ...");

            await IntializeFileOperations();
            ConnectedDevicesDialog = new ConnectedDevicesDialog();
            SpaceFrame.Navigate(typeof(SpacePage));
            SpacePage = SpacePage.Self;

            LayerFrame.Navigate(typeof(LayerPage));
            LayerPage = LayerPage.Self;

            await ConnectedDevicesDialog.Rescan();

            //Start SocketClient
            startclient();

            LoadSettings();
        }
        private void LoadSettings()
        {
            bool successful = float.TryParse(g_LocalSettings.Values["SpaceZooming"] as string, out float percent);
            if (successful)
                SpacePage.SetSpaceZoomPercent(percent);
            else
            {
                SpacePage.SetSpaceZoomPercent(50);
                //SpaceZoomButton.Content = "50 %";
            }

            successful = int.TryParse(g_LocalSettings.Values["LayerLevel"] as string, out int level);
            if (successful)
                LayerPage.LayerZoomSlider.Value = level;
            else
            {
                LayerPage.LayerZoomSlider.Value = 2;
            }

            #region Recent Color
            successful = string.IsNullOrEmpty(g_LocalSettings.Values["RecentColor1"] as string);
            if (successful)
                g_RecentColor[0].HexColor = "#00000000";
            else
            {
                g_RecentColor[0].HexColor = g_LocalSettings.Values["RecentColor1"] as string;
            }

            successful = string.IsNullOrEmpty(g_LocalSettings.Values["RecentColor2"] as string);
            if (successful)
                g_RecentColor[1].HexColor = "#00000000";
            else
            {
                g_RecentColor[1].HexColor = g_LocalSettings.Values["RecentColor2"] as string;
            }

            successful = string.IsNullOrEmpty(g_LocalSettings.Values["RecentColor3"] as string);
            if (successful)
                g_RecentColor[2].HexColor = "#00000000";
            else
            {
                g_RecentColor[2].HexColor = g_LocalSettings.Values["RecentColor3"] as string;
            }

            successful = string.IsNullOrEmpty(g_LocalSettings.Values["RecentColor4"] as string);
            if (successful)
                g_RecentColor[3].HexColor = "#00000000";
            else
            {
                g_RecentColor[3].HexColor = g_LocalSettings.Values["RecentColor4"] as string;
            }

            successful = string.IsNullOrEmpty(g_LocalSettings.Values["RecentColor5"] as string);
            if (successful)
                g_RecentColor[4].HexColor = "#00000000";
            else
            {
                g_RecentColor[4].HexColor = g_LocalSettings.Values["RecentColor5"] as string;
            }

            successful = string.IsNullOrEmpty(g_LocalSettings.Values["RecentColor6"] as string);
            if (successful)
                g_RecentColor[5].HexColor = "#00000000";
            else
            {
                g_RecentColor[5].HexColor = g_LocalSettings.Values["RecentColor6"] as string;
            }

            successful = string.IsNullOrEmpty(g_LocalSettings.Values["RecentColor7"] as string);
            if (successful)
                g_RecentColor[6].HexColor = "#00000000";
            else
            {
                g_RecentColor[6].HexColor = g_LocalSettings.Values["RecentColor7"] as string;
            }

            successful = string.IsNullOrEmpty(g_LocalSettings.Values["RecentColor8"] as string);
            if (successful)
                g_RecentColor[7].HexColor = "#00000000";
            else
            {
                g_RecentColor[7].HexColor = g_LocalSettings.Values["RecentColor8"] as string;
            }

            #endregion
        }

        #region -- Event --
        private async void ConnectedDevicesButton_Click(object sender, RoutedEventArgs e)
        {
            await ConnectedDevicesDialog.ShowAsync();
        }

        private void EffectBlockListView_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            string effName = e.Items[0] as string;
            e.Data.Properties.Add("EffectName", effName);

            SpacePage.Self.SetSpaceStatus(SpaceStatus.DraggingEffectBlock);

            // Workaround for keeping EffectBlock in Pressed state
            var ebList = FindAllControl<EffectBlock>(EffectBlockListView, typeof(EffectBlock));
            var index = EffectBlockListView.Items.IndexOf(effName);
            var eb = ebList[index];
            eb.Dragging = true;
            VisualStateManager.GoToState(eb, "Pressed", false);
        }
        private void EffectBlockListView_DragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
        {
            SpacePage.Self.SetSpaceStatus(SpaceStatus.WatchingLayer);

            // Workaround for preventing EffctBlock from keeping another status after completing
            var ebList = FindAllControl<EffectBlock>(EffectBlockListView, typeof(EffectBlock));
            foreach (var eb in ebList)
            {
                eb.Dragging = false;
                VisualStateManager.GoToState(eb, "Normal", false);
            }
        }

        private void SetLayerButton_Click(object sender, RoutedEventArgs e)
        {
            LayerModel layer = new LayerModel();
            List<int> selectedIndex;

            layer.Name = "Layer " + (LayerPage.GetLayerCount() + 1);

            foreach (DeviceModel dm in SpacePage.DeviceModelCollection)
            {
                selectedIndex = new List<int>();

                foreach (var zone in dm.AllZones)
                {
                    if (zone.Selected == true)
                    {
                        selectedIndex.Add(zone.Index);
                    }
                }

                if (selectedIndex.Count != 0)
                    layer.AddDeviceZones(dm.Type, selectedIndex.ToArray());
            }

            LayerPage.AddLayer(layer);
            Log.Debug("[SetLayerButton] Add the layer : " + layer.Name);
            LayerPage.CheckedLayer = layer;
            NeedSave = true;
        }
        private void SortDeviceButton_Click(object sender, RoutedEventArgs e)
        {
            EditDoneButton.IsEnabled = true;
            ShowMask("Device Sorting");
            SpacePage.SetSpaceStatus(SpaceStatus.DraggingDevice);

            oldSortingPositions.Clear();
            foreach (var dm in SpacePage.DeviceModelCollection)
            {
                oldSortingPositions.Add(dm, new Point(dm.PixelLeft, dm.PixelTop));
            }
        }
        public void OnLeftSidePanelButtonClick()
        {
            int columnSpans = Grid.GetColumnSpan(SpaceFrame);

            if (MainGrid.Children.Contains(EffectBlockScrollViewer))
            {
                MainGrid.Children.Remove(EffectBlockScrollViewer);
                MainGrid.Children.Remove(MaskEffectblockGrid);

                Grid.SetColumn(SpaceFrame, 0);
                Grid.SetColumnSpan(SpaceFrame, columnSpans + 1);
            }
            else
            {
                Grid.SetColumn(EffectBlockScrollViewer, 0);
                Grid.SetColumn(MaskEffectblockGrid, 0);

                MainGrid.Children.Add(EffectBlockScrollViewer);
                MainGrid.Children.Add(MaskEffectblockGrid);

                Grid.SetColumn(SpaceFrame, 1);
                Grid.SetColumnSpan(SpaceFrame, columnSpans - 1);
            }
        }
        public void OnRightSidePanelButtonClick()
        {
            int columnSpans = Grid.GetColumnSpan(SpaceFrame);

            if (MainGrid.Children.Contains(EffectInfoFrame))
            {
                MainGrid.Children.Remove(EffectInfoFrame);
                MainGrid.Children.Remove(MaskEffectInfoGrid);
                Grid.SetColumnSpan(SpaceFrame, columnSpans + 1);
            }
            else
            {
                Grid.SetColumn(EffectInfoFrame, 2);
                Grid.SetColumn(MaskEffectInfoGrid, 2);

                MainGrid.Children.Add(EffectInfoFrame);
                MainGrid.Children.Add(MaskEffectInfoGrid);

                Grid.SetColumnSpan(SpaceFrame, columnSpans - 1);
            }
        }

        public void OnIngroupDevicesChanged()
        {
            if (SpacePage.DeviceModelCollection.Count == 0)
                NoSupportedDeviceGrid.Visibility = Visibility.Visible;
            else
                NoSupportedDeviceGrid.Visibility = Visibility.Collapsed;
        }

        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            ReUndoManager.GetInstance().Undo();
        }
        private void RedoButton_Click(object sender, RoutedEventArgs e)
        {
            ReUndoManager.GetInstance().Redo();
        }
        #endregion

        #region -- Mask --
        private void EditDoneButton_Click(object sender, RoutedEventArgs e)
        {
            if (SpacePage.GetSpaceStatus() == SpaceStatus.ReEditing)
            {
                LayerModel layer = LayerPage.CheckedLayer;
                List<int> selectedIndex;

                foreach (DeviceModel dm in SpacePage.DeviceModelCollection)
                {
                    selectedIndex = new List<int>();

                    foreach (var zone in dm.AllZones)
                    {
                        if (zone.Selected == true)
                        {
                            selectedIndex.Add(zone.Index);
                        }
                    }

                    layer.SetDeviceZones(dm.Type, selectedIndex.ToArray());
                }

                NeedSave = true;
                SpacePage.WatchLayer(layer);
            }
            else // Sorting
            {
                SpacePage.SetSpaceStatus(SpaceStatus.Clean);
            }

            HideMask();
        }
        private void EditCancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (SpacePage.GetSpaceStatus() == SpaceStatus.ReEditing)
            {
                LayerModel layer = LayerPage.CheckedLayer;
                SpacePage.WatchLayer(layer);
            }
            else // Sorting
            {
                SpacePage.SetSpaceStatus(SpaceStatus.Clean);

                foreach (var pair in oldSortingPositions)
                {
                    pair.Key.PixelLeft = pair.Value.X;
                    pair.Key.PixelTop = pair.Value.Y;
                }
            }

            SpacePage.OnDeviceMoveCompleted();
            HideMask();
        }
        public void ShowReEditMask(LayerModel layer)
        {
            ShowMask("Edit layer : " + layer.Name);
        }
        private void ShowMask(string descriptionText)
        {
            EditBarTextBlock.Text = descriptionText;

            EditBarRelativePanel.Visibility = Visibility.Visible;
            ActionBarRelativePanel.Visibility = Visibility.Collapsed;
            MaskConntectedDeviceGrid.Visibility = Visibility.Visible;
            MaskFileCombobox.Visibility = Visibility.Visible;
            MaskEffectblockGrid.Visibility = Visibility.Visible;
            MaskEffectInfoGrid.Visibility = Visibility.Visible;
            MaskLayerPage.Visibility = Visibility.Visible;
        }
        private void HideMask()
        {
            EditBarRelativePanel.Visibility = Visibility.Collapsed;
            ActionBarRelativePanel.Visibility = Visibility.Visible;
            MaskConntectedDeviceGrid.Visibility = Visibility.Collapsed;
            MaskFileCombobox.Visibility = Visibility.Collapsed;
            MaskEffectblockGrid.Visibility = Visibility.Collapsed;
            MaskEffectInfoGrid.Visibility = Visibility.Collapsed;
            MaskLayerPage.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region -- Socket --
        StreamSocket socket = new StreamSocket();

        private void SocketServerRecv_DoWork(object sender, DoWorkEventArgs e)
        {
            receivedata();
        }

        //Use senddata(string) can send string to server
        public async void SendMessageToServer(string request)
        {
            Stream streamOut = socket.OutputStream.AsStreamForWrite();
            StreamWriter writer = new StreamWriter(streamOut);
            await writer.WriteLineAsync(request);
            await writer.FlushAsync();
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                //StatusTextBlock.Text = "Send successful";
            });
        }

        public static bool IsConnection = false;

        async void receivedata()
        {
            IsConnection = false;
            string cm;
            string port;
            string ip;

            do
            {
                try
                {
                    port = "6667";
                    ip = "127.0.0.1";
                    cm = "I'm the message from client";
                    HostName serverHost = new HostName("127.0.0.1");
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

                    if (response.Contains("[SYNCSTATUS_CHANGE]"))
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                        {
                            //from Service message
                            StatusTextBlock.Text = "Service : " + response;
                            Log.Debug("[ReceiveData] Rescan ...");
                            await ConnectedDevicesDialog.Rescan();
                        });
                }
                catch (Exception ex)
                {
                    IsConnection = false;
                }
            }
        }

        void startclient()
        {
            BackgroundWorker bgwSocketServerRecv;

            //BackgroundWorker for Socket Server
            bgwSocketServerRecv = new BackgroundWorker();
            bgwSocketServerRecv.DoWork += SocketServerRecv_DoWork;
            bgwSocketServerRecv.RunWorkerAsync();
        }
        #endregion

        #region -- UI initial get device from service --


        #endregion

        private async void DebugButton_Click(object sender, RoutedEventArgs e)
        {
            //ConnectedDevicesDialog.Rescan();
            SpacePage.DeviceModelCollection[0].PixelLeft = 100;
        }
    }
}

