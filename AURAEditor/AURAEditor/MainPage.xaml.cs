﻿using AuraEditor.Common;
using AuraEditor.Dialogs;
using AuraEditor.Models;
using AuraEditor.Pages;
using AuraEditor.UserControls;
using AuraEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Core.Preview;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using static AuraEditor.Common.AuraEditorColorHelper;
using static AuraEditor.Common.ControlHelper;
using static AuraEditor.Common.EffectHelper;
using static AuraEditor.Common.Math2;
using static AuraEditor.Common.MetroEventSource;
using static AuraEditor.Common.StorageHelper;
using static AuraEditor.Pages.SpacePage;
using DevicZonesPair = System.Tuple<int, int[]>;
using DeviceZonesPairList = System.Collections.Generic.List<System.Tuple<int, int[]>>;
using Windows.UI.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.ApplicationModel.Resources;
using Windows.UI.ViewManagement;

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
        public ContentDialog g_ContentDialog;
        static private DeviceUpdatePromptDialog dupd;
        public bool CanShowDeviceUpdateDialog = true;
        ApplicationDataContainer g_LocalSettings;
        public RecentColor[] g_RecentColor = new RecentColor[8];
        private Dictionary<DeviceModel, Point> oldSortingPositions;
        private ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView();

        public bool needToUpdate = false;

        private int WindowsSizeFlag = 0;

        public EffectLineViewModel SelectedEffect
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
        public bool g_isFirstTimePressZ = true;


        private void CoreWindow_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            switch (args.VirtualKey)
            {
                //TODO modify
                case Windows.System.VirtualKey.Delete:
                    if (g_PressCtrl == true && g_PressShift == true)
                    {
                        if (FileListButtonContent.Text.ToString() == "")
                            break;
                        else
                            DeleteItem_Click(null, null);
                        break;
                    }
                    else
                    {
                        if (SelectedEffect == null)
                            return;

                        SelectedEffect.Layer.DeleteEffectLine(SelectedEffect);
                        break;
                    }
                case Windows.System.VirtualKey.Control:
                    g_PressCtrl = true;
                    break;
                case Windows.System.VirtualKey.Shift:
                    g_PressShift = true;
                    SpacePage.SpaceScrollViewer.VerticalScrollMode = ScrollMode.Disabled;
                    LayerPage.TrackScrollViewer.VerticalScrollMode = ScrollMode.Disabled;
                    break;
                case Windows.System.VirtualKey.Back:
                    if (g_PressCtrl == false && g_PressShift == false)
                    {
                        if (SelectedEffect == null)
                            return;

                        SelectedEffect.Layer.DeleteEffectLine(SelectedEffect);
                    }
                    break;
                case Windows.System.VirtualKey.Z:
                    if (g_isFirstTimePressZ && SpacePage.isMouseInSpacePage) //just run one time when Z pressed
                    {
                        Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Custom, 101); //101 release  102 hold
                        g_isFirstTimePressZ = false;
                        SpacePage.OnZKeyPressed();
                    }
                    break;
                case Windows.System.VirtualKey.R:
                    if (FileListButtonContent.Text.ToString() == "")
                        break;
                    if (g_PressCtrl == true)
                        RenameItem_Click(null, null);
                    break;
                case Windows.System.VirtualKey.N:
                    if (g_PressCtrl == true)
                        NewFileButton_Click(null, null);
                    break;
                case Windows.System.VirtualKey.I:
                    if (g_PressCtrl == true)
                        ImportButton_Click(null, null);
                    break;
                case Windows.System.VirtualKey.E:
                    if (g_PressCtrl == true && g_PressShift == false)
                        ExportButton_Click(null, null);
                    break;
            }
        }

        private void CoreWindow_KeyUp(CoreWindow sender, KeyEventArgs args)
        {
            switch (args.VirtualKey)
            {
                case Windows.System.VirtualKey.Z:
                    Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
                    g_isFirstTimePressZ = true;
                    SpacePage.OnZKeyRelease();
                    break;
                case Windows.System.VirtualKey.Shift:
                    g_PressShift = false;
                    SpacePage.SpaceScrollViewer.VerticalScrollMode = ScrollMode.Enabled;
                    LayerPage.TrackScrollViewer.VerticalScrollMode = ScrollMode.Enabled;
                    break;
                case Windows.System.VirtualKey.Control:
                    g_PressCtrl = false;
                    break;
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
            Window.Current.CoreWindow.SizeChanged += CoreWindow_SizeChanged;
            g_PressShift = false;
            g_PressCtrl = false;
            SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += this.OnCloseRequest;
            g_LocalSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            // EffectBlocks
            for (int i = 0; i < GetCommonEffects().Length; i++)
                EffectBlockListView.Items.Add(i);

            oldSortingPositions = new Dictionary<DeviceModel, Point>();

            if (FileListButtonContent.Text.ToString() == "")
            {
                RenameItem.IsEnabled = false;
                DeleteItem.IsEnabled = false;
            }
        }
        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            Log.Debug("[MainPage_Loaded] Intialize ...");

            await IntializeFileOperations();
            SpaceFrame.Navigate(typeof(SpacePage));
            SpacePage = SpacePage.Self;

            LayerFrame.Navigate(typeof(LayerPage));
            LayerPage = LayerPage.Self;

            //Start SocketClient
            MaskManager.GetInstance().ShowMask(MaskType.NoSupportDevice);
            Log.Debug("[MainPage_Loaded] Start client thread");
            startclient();
            Log.Debug("[MainPage_Loaded] Start client thread complete");
            LoadSettings();
            Log.Debug("[MainPage_Loaded] LoadSettings complete");
            DeviceUpdatesPage.Self.UpdateButton_Click(null, null);

            var visibleBounds = ApplicationView.GetForCurrentView().VisibleBounds;
            ResizeButton(visibleBounds.Width);
        }
        private void LoadSettings()
        {
            bool successful;
            successful = float.TryParse(g_LocalSettings.Values["SpaceZooming"] as string, out float percent);
            SpacePage.SetSpaceZoomPercent(successful ? percent : 50);

            successful = int.TryParse(g_LocalSettings.Values["LayerLevel"] as string, out int level);
            LayerPage.LayerZoomSlider.Value = successful ? level : 4;

            #region -- Recent Color --
            string value;
            value = g_LocalSettings.Values["RecentColor1"] as string;
            g_RecentColor[0].HexColor = string.IsNullOrEmpty(value) ? "#00000000" : value;

            value = g_LocalSettings.Values["RecentColor2"] as string;
            g_RecentColor[1].HexColor = string.IsNullOrEmpty(value) ? "#00000000" : value;

            value = g_LocalSettings.Values["RecentColor3"] as string;
            g_RecentColor[2].HexColor = string.IsNullOrEmpty(value) ? "#00000000" : value;

            value = g_LocalSettings.Values["RecentColor4"] as string;
            g_RecentColor[3].HexColor = string.IsNullOrEmpty(value) ? "#00000000" : value;

            value = g_LocalSettings.Values["RecentColor5"] as string;
            g_RecentColor[4].HexColor = string.IsNullOrEmpty(value) ? "#00000000" : value;

            value = g_LocalSettings.Values["RecentColor6"] as string;
            g_RecentColor[5].HexColor = string.IsNullOrEmpty(value) ? "#00000000" : value;

            value = g_LocalSettings.Values["RecentColor7"] as string;
            g_RecentColor[6].HexColor = string.IsNullOrEmpty(value) ? "#00000000" : value;

            value = g_LocalSettings.Values["RecentColor8"] as string;
            g_RecentColor[7].HexColor = string.IsNullOrEmpty(value) ? "#00000000" : value;
            #endregion
        }

        #region -- Event --
        private async void ConnectedDevicesButton_Click(object sender, RoutedEventArgs e)
        {
            ConnectedDevicesDialog dialog = new ConnectedDevicesDialog();
            await dialog.ShowAsync();
        }

        private void EffectBlockListView_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            int effIdx = (int)e.Items[0];
            e.Data.Properties.Add("EffectIndex", effIdx);
            SpacePage.SetSpaceStatus(SpaceStatus.DraggingEffectBlock);

            // Workaround for keeping EffectBlock in Pressed state
            var ebList = FindAllControl<EffectBlock>(EffectBlockListView, typeof(EffectBlock));
            var ebIndex = EffectBlockListView.Items.IndexOf(effIdx);
            var eb = ebList[ebIndex];
            eb.Dragging = true;
            VisualStateManager.GoToState(eb, "Pressed", false);
        }
        private void EffectBlockListView_DragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
        {
            SpacePage.SetSpaceStatus(SpaceStatus.Watching);
            LayerPage.UpdateSupportLine(-1);

            // Workaround for preventing EffctBlock from keeping another status after completing
            var ebList = FindAllControl<EffectBlock>(EffectBlockListView, typeof(EffectBlock));
            foreach (var eb in ebList)
            {
                eb.Dragging = false;
                VisualStateManager.GoToState(eb, "Normal", false);
            }
        }

        static public int SetLayerCounter = 1;
        private void SetLayerButton_Click(object sender, RoutedEventArgs e)
        {
            LayerModel layer = new LayerModel();
            List<int> selectedIndex;

            layer.Name = "Layer " + SetLayerCounter++;

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

            LayerPage.InsertLayer(layer, 0);
            Log.Debug("[SetLayerButton] Add the layer : " + layer.Name);
            LayerPage.CheckedLayer = layer;
            LayerPage.TrackScrollViewer.ChangeView(null, 0, null, false);
        }
        private void SortDeviceButton_Click(object sender, RoutedEventArgs e)
        {
            EditDoneButton.IsEnabled = true;
            EditBarTextBlock.Text = resourceLoader.GetString("SortDeviceTitle");
            MaskManager.GetInstance().ShowMask(MaskType.Editing);
            SpacePage.SetSpaceStatus(SpaceStatus.Sorting);

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
                MainGrid.Children.Remove(EffectBlockMask);

                Grid.SetColumn(SpaceFrame, 0);
                Grid.SetColumnSpan(SpaceFrame, columnSpans + 1);
            }
            else
            {
                Grid.SetColumn(EffectBlockScrollViewer, 0);
                Grid.SetColumn(EffectBlockMask, 0);

                MainGrid.Children.Add(EffectBlockScrollViewer);
                MainGrid.Children.Add(EffectBlockMask);

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
                MainGrid.Children.Remove(EffectInfoMask);
                Grid.SetColumnSpan(SpaceFrame, columnSpans + 1);
            }
            else
            {
                Grid.SetColumn(EffectInfoFrame, 2);
                Grid.SetColumn(EffectInfoMask, 2);

                MainGrid.Children.Add(EffectInfoFrame);
                MainGrid.Children.Add(EffectInfoMask);

                Grid.SetColumnSpan(SpaceFrame, columnSpans - 1);
            }
        }

        bool _isPressed = false;
        private void WindowSliderGrid_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var fe = sender as FrameworkElement;
            bool _hasCapture = fe.CapturePointer(e.Pointer);
            _isPressed = true;
        }
        private void WindowSliderGrid_PointerMoved(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var fe = sender as FrameworkElement;
            PointerPoint ptrPt = e.GetCurrentPoint(fe);
            PointerPoint ptrPt2 = e.GetCurrentPoint(MainGrid);
            Point position = ptrPt.Position;

            if (_isPressed)
            {
                double value = MainGrid.ActualHeight - ptrPt2.Position.Y;

                if (value <= 100 || value >= 500)
                    return;

                MainGrid.RowDefinitions[3].Height =
                    new GridLength(MainGrid.ActualHeight - ptrPt2.Position.Y, GridUnitType.Pixel);
            }
        }
        private void WindowSliderGrid_PointerReleased(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            _isPressed = false;
            Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 0);
        }
        private void WindowSliderGrid_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.SizeNorthSouth, 0);
        }
        private void WindowSliderGrid_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (!_isPressed)
                Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 0);
        }

        private void EditDoneButton_Click(object sender, RoutedEventArgs e)
        {
            if (SpacePage.GetSpaceStatus() == SpaceStatus.ReEditing)
            {
                LayerModel layer = LayerPage.CheckedLayer;
                DeviceZonesPairList oldpairs = new DeviceZonesPairList();
                DeviceZonesPairList newpairs = new DeviceZonesPairList();

                foreach (DeviceModel dm in SpacePage.DeviceModelCollection)
                {
                    var oldzones = layer.GetDeviceZones(dm.Type);
                    oldpairs.Add(new DevicZonesPair(dm.Type, oldzones));

                    List<int> selectedIndex = new List<int>();

                    foreach (var zone in dm.AllZones)
                    {
                        if (zone.Selected == true)
                            selectedIndex.Add(zone.Index);
                    }

                    layer.SetDeviceZones(dm.Type, selectedIndex.ToArray());
                    newpairs.Add(new DevicZonesPair(dm.Type, selectedIndex.ToArray()));
                }

                ReUndoManager.Store(new EditZonesCommand(layer, oldpairs, newpairs));
                SpacePage.WatchLayer(layer);
            }
            else // Sorting
            {
                var dmPositions = new Dictionary<DeviceModel, Tuple<double, double>>();

                foreach (var pair in oldSortingPositions)
                {
                    DeviceModel dm = pair.Key;
                    double moveX = dm.PixelLeft - pair.Value.X;
                    double moveY = dm.PixelTop - pair.Value.Y;

                    SpacePage.Self.DeleteOverlappingTempDevice(dm);
                    SpacePage.Self.MoveMousePosition(dm, RoundToGrid(moveX), RoundToGrid(moveY));
                    dmPositions.Add(dm, new Tuple<double, double>(moveX, moveY));
                }

                ReUndoManager.Store(new MoveDevicesCommand(dmPositions));
                SpacePage.GoToBlankEditing();
            }

            MaskManager.GetInstance().ShowMask(MaskType.None);
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
                SpacePage.GoToBlankEditing();

                foreach (var pair in oldSortingPositions)
                {
                    pair.Key.PixelLeft = pair.Value.X;
                    pair.Key.PixelTop = pair.Value.Y;
                }
            }

            SpacePage.StopScrollTimer();
            MaskManager.GetInstance().ShowMask(MaskType.None);
        }
        public void ShowReEditMask(LayerModel layer)
        {
            EditBarTextBlock.Text = resourceLoader.GetString("EditLayerText") + layer.Name;
            MaskManager.GetInstance().ShowMask(MaskType.Editing);
        }
        private void HideMask()
        {
            MaskManager.GetInstance().ShowMask(MaskType.None);
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
            Log.Debug("[SendMessageToServer] Message : " + request);

            if (!IsConnection)
            {
                Log.Debug("[SendMessageToServer] Service is disconnected ...");
                return;
            }

            Stream streamOut = socket.OutputStream.AsStreamForWrite();
            StreamWriter writer = new StreamWriter(streamOut);
            await writer.WriteLineAsync(request);
            await writer.FlushAsync();
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
                    Log.Debug("[ReceiveData] Connecting");
                    port = "6667";
                    ip = "127.0.0.1";
                    cm = "I'm the message from client";
                    HostName serverHost = new HostName("127.0.0.1");
                    string serverPort = port;
                    Log.Debug("[ReceiveData] ConnectAsync");
                    await socket.ConnectAsync(serverHost, serverPort);
                    Log.Debug("[ReceiveData] RunAsync");
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        await SpacePage.Rescan();
                    });
                    IsConnection = true;
                    Log.Debug("[ReceiveData] Connected");
                }
                catch (Exception ex)
                {
                    Log.Debug("[ReceiveData] Connect failed, try again ...");
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

                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        //from Service message
                        Log.Debug("[ReceiveData] Rescan ...");
                        await SpacePage.Rescan();
                    });
                    string[] infoArray = response.Split(new char[3] { '[', ']', ',' });
                    if ((infoArray[1] == "PlugIn") && (infoArray[2] != " "))
                    {
                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                        {
                            needToUpdate = true;
                            SettingBtnNewTab.Visibility = Visibility.Visible;

                            var dialog = GetCurrentContentDialog();

                            if (dialog != null)
                            {
                                if (SpacePage.Self.DeviceModelCollection.FindAll(find => find.Plugged == true).Count == 0)
                                {
                                    NoSupportedDeviceContent.Text = "To start editing the lighting profile, the latest software version of connected devices are required.";
                                    GoToUpdateBtn.Visibility = Visibility.Visible;
                                }
                                else if (!(dialog is DeviceUpdatePromptDialog))
                                {
                                    dupd = new DeviceUpdatePromptDialog();
                                    g_ContentDialog = dupd;
                                }
                            }
                            else
                            {
                                if (SpacePage.Self.DeviceModelCollection.FindAll(find => find.Plugged == true).Count == 0)
                                {
                                    NoSupportedDeviceContent.Text = "To start editing the lighting profile, the latest software version of connected devices are required.";
                                    GoToUpdateBtn.Visibility = Visibility.Visible;
                                }
                                else
                                {
                                    if (CanShowDeviceUpdateDialog)
                                    {
                                        dupd = new DeviceUpdatePromptDialog();
                                        await dupd.ShowAsync();
                                    }
                                    else
                                    {
                                        dupd = new DeviceUpdatePromptDialog();
                                        g_ContentDialog = dupd;
                                    }
                                }
                            }
                        });
                    }
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

        public async void ShowDeviceUpdateDialogOrNot()
        {
            if (g_ContentDialog != null && CanShowDeviceUpdateDialog)
            {
                g_ContentDialog = new DeviceUpdatePromptDialog();
                await g_ContentDialog.ShowAsync();
            }
        }

        public async void ShowTutorialDialogOrNot()
        {
            TutorialDialog td = new TutorialDialog();
            await td.ShowAsync();
        }

        public async void ShowShortcutsDialog()
        {
            HotKeyListDialog hld = new HotKeyListDialog();
            await hld.ShowAsync();
        }

        private async void DebugButton_Click(object sender, RoutedEventArgs e)
        {
            //SpacePage.RescanFake();
        }

        public class MoveDevicesCommand : IReUndoCommand
        {
            private Dictionary<DeviceModel, Tuple<double, double>> _dmMoveOffset;

            public MoveDevicesCommand(Dictionary<DeviceModel, Tuple<double, double>> dmMoveOffset)
            {
                _dmMoveOffset = dmMoveOffset;
            }

            public void ExecuteRedo()
            {
                foreach (var pair in _dmMoveOffset)
                {
                    DeviceModel dm = pair.Key;
                    double moveX = pair.Value.Item1;
                    double moveY = pair.Value.Item2;

                    dm.PixelLeft += moveX;
                    dm.PixelTop += moveY;
                    SpacePage.Self.DeleteOverlappingTempDevice(dm);
                    SpacePage.Self.MoveMousePosition(dm, RoundToGrid(moveX), RoundToGrid(moveY));
                }
            }
            public void ExecuteUndo()
            {
                foreach (var pair in _dmMoveOffset)
                {
                    DeviceModel dm = pair.Key;
                    double moveX = pair.Value.Item1;
                    double moveY = pair.Value.Item2;

                    dm.PixelLeft -= moveX;
                    dm.PixelTop -= moveY;
                    SpacePage.Self.DeleteOverlappingTempDevice(dm);
                    SpacePage.Self.MoveMousePosition(dm, RoundToGrid(-moveX), RoundToGrid(-moveY));
                }
            }
        }
        public class EditZonesCommand : IReUndoCommand
        {
            LayerModel _layer;
            private DeviceZonesPairList _old_zoneIDs;
            private DeviceZonesPairList _new_zoneIDs;

            public EditZonesCommand(LayerModel layer, DeviceZonesPairList old_zoneIDs, DeviceZonesPairList new_zoneIDs)
            {
                _layer = layer;
                _old_zoneIDs = old_zoneIDs;
                _new_zoneIDs = new_zoneIDs;
            }

            public void ExecuteRedo()
            {
                foreach (var deviceZones in _new_zoneIDs)
                {
                    int type = deviceZones.Item1;
                    int[] zones = deviceZones.Item2;
                    _layer.SetDeviceZones(type, zones);
                }

                LayerPage.Self.CheckedLayer = _layer;
            }
            public void ExecuteUndo()
            {
                foreach (var deviceZones in _old_zoneIDs)
                {
                    int type = deviceZones.Item1;
                    int[] zones = deviceZones.Item2;
                    _layer.SetDeviceZones(type, zones);
                }

                LayerPage.Self.CheckedLayer = _layer;
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            // Register a handler for BackRequested events and set the
            // visibility of the Back button
            SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;

            WindowsPage.Self.WindowsGrid.Visibility = Visibility.Collapsed;
            WindowsPage.Self.WindowsGrid1.Visibility = Visibility.Visible;
            WindowsPage.Self.WindowsFrame1.Navigate(typeof(SettingsPage), needToUpdate, new SuppressNavigationTransitionInfo());
        }

        public void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            if (WindowsPage.Self.WindowsFrame1.Content is SettingsPage)
            {
                WindowsPage.Self.WindowsGrid.Visibility = Visibility.Visible;
                WindowsPage.Self.WindowsGrid1.Visibility = Visibility.Collapsed;
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
                SystemNavigationManager.GetForCurrentView().BackRequested -= OnBackRequested;
            }
            else
            {
                if (WindowsPage.Self.WindowsFrame1.CanGoBack)
                {
                    e.Handled = true;
                    WindowsPage.Self.WindowsFrame1.GoBack();
                }
            }
        }

        private void TutorialItem_Click(object sender, RoutedEventArgs e)
        {
            ShowTutorialDialogOrNot();
        }

        private void ShortcutsItem_Click(object sender, RoutedEventArgs e)
        {
            ShowShortcutsDialog();
        }

        private void CoreWindow_SizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            ResizeButton(e.Size.Width);
        }

        public void ResizeButton(double width)
        {
            if ((width < 1204) && (WindowsSizeFlag != 1))
            {
                SetLayerButton.Width = 40;
                SortDeviceButton.Width = 40;
                SaveAndApplyButton.Width = 40;
                WindowsSizeFlag = 1;
            }
            else if ((width >= 1204) && (WindowsSizeFlag != -1))
            {
                SetLayerButton.Width = 160;
                SortDeviceButton.Width = 180;
                SaveAndApplyButton.Width = 160;
                WindowsSizeFlag = -1;
            }
        }
    }
}

