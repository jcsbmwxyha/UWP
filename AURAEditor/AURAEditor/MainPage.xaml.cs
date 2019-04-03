using AuraEditor.Common;
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
        public ContentDialog g_ContentDialog;
        static private DeviceUpdatePromptDialog dupd;
        public string g_NewPlugInDeviceName;
        public bool CanShowDeviceUpdateDialog = true;
        ApplicationDataContainer g_LocalSettings;
        public RecentColor[] g_RecentColor = new RecentColor[8];
        private Dictionary<DeviceModel, Point> oldSortingPositions;
        
        public bool needToUpdadte = false;

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
        public bool g_CanPaste = true;
        public bool g_isFirstTimePressZ = true;


        private void CoreWindow_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            switch (args.VirtualKey)
            {
                case Windows.System.VirtualKey.Z:
                    if (g_PressCtrl == true)
                    {
                        if (g_PressShift == true)
                            RedoButton_Click(null, null);
                        else
                            UndoButton_Click(null, null);
                        break;
                    }
                    else
                    {
                        if (g_isFirstTimePressZ && SpacePage.isMouseInSpacePage) //just run one time when Z pressed
                        {
                            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Custom, 101); //101 release  102 hold
                            g_isFirstTimePressZ = false;
                            SpacePage.OnZKeyPressed();
                        }
                        break;
                    }
                case Windows.System.VirtualKey.Shift:
                    g_PressShift = true;
                    SpacePage.SpaceScrollViewer.VerticalScrollMode = ScrollMode.Disabled;
                    LayerPage.TrackScrollViewer.VerticalScrollMode = ScrollMode.Disabled;
                    break;
                case Windows.System.VirtualKey.Control:
                    g_PressCtrl = true;
                    break;
                case Windows.System.VirtualKey.X:
                    if (SelectedEffect == null)
                        return;

                    if (g_PressCtrl == true)
                    {
                        LayerPage.CopiedEffect = new EffectLineViewModel(SelectedEffect);
                        SelectedEffect.Layer.DeleteEffectLine(SelectedEffect);
                    }
                    break;
                case Windows.System.VirtualKey.C:
                    if (SelectedEffect == null)
                        return;

                    if (g_PressCtrl == true)
                        LayerPage.CopiedEffect = new EffectLineViewModel(SelectedEffect);
                    break;
                case Windows.System.VirtualKey.V:
                    if (LayerPage.CheckedLayer == null || g_PressCtrl == false || g_CanPaste == false || LayerPage.CopiedEffect == null)
                        return;

                    g_CanPaste = false;

                    var copy = new EffectLineViewModel(LayerPage.CopiedEffect);

                    if (SelectedEffect != null)
                    {
                        copy.Left = SelectedEffect.Right;
                        SelectedEffect.Layer.InsertTimelineEffectFitly(copy);
                    }
                    else
                    {
                        LayerPage.CheckedLayer.InsertTimelineEffectFitly(new EffectLineViewModel(copy));
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
                    break;
                case Windows.System.VirtualKey.Delete:
                    if (g_PressCtrl == true && g_PressShift == true)
                    {
                        if (FileListButton.Content.ToString() == "")
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
                case Windows.System.VirtualKey.Home:
                    if (g_PressShift == true)
                    {
                        LayerPage.JumpToBeginningButton_Click(null, null);
                    }
                    break;
                case Windows.System.VirtualKey.End:
                    if (g_PressShift == true)
                    {
                        LayerPage.JumpToEndButton_Click(null, null);
                    }
                    break;
                case Windows.System.VirtualKey.S:
                    if (g_PressCtrl == true)
                        SaveAndApplyButton_Click(null, null);
                    break;
                case Windows.System.VirtualKey.R:
                    if (FileListButton.Content.ToString() == "")
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
                    if (g_PressShift == true && LayerPage.CheckedLayer != null)
                    {
                        LayerPage.CheckedLayer.ClearAllEffect();
                        LayerPage.CheckedEffect = null;
                    }
                    else if (g_PressCtrl == true && g_PressShift == false)
                        ExportButton_Click(null, null);
                    break;
                case Windows.System.VirtualKey.M:
                    if (g_PressCtrl == true)
                        SortDeviceButton_Click(null, null);
                    break;
                case Windows.System.VirtualKey.Number1:
                    if (g_PressCtrl == true)
                        SpacePage.DefaultViewButton_Click(null, null);
                    break;
                case Windows.System.VirtualKey.Number0:
                    if (g_PressCtrl == true)
                        SpacePage.FitAllButton_Click(null, null);
                    break;
                case Windows.System.VirtualKey.Add:
                    if (g_PressCtrl == true)
                        SpacePage.SpaceZoom_For_Hotkey(true);
                    break;
                case Windows.System.VirtualKey.Subtract:
                    if (g_PressCtrl == true)
                        SpacePage.SpaceZoom_For_Hotkey(false);
                    break;
                case Windows.System.VirtualKey.Enter:
                    if (g_PressCtrl == true)
                        LayerPage.Hotkey_for_Play_and_Puase();
                    break;
                case Windows.System.VirtualKey.D:
                    if (g_PressCtrl == true && LayerPage.CheckedLayer != null)
                    {
                        int index = LayerPage.Layers.IndexOf(LayerPage.CheckedLayer);
                        LayerModel temp_layer = LayerModel.Clone(LayerPage.CheckedLayer);
                        LayerPage.Layers.Insert(index, temp_layer);
                        LayerPage.CheckedLayer = LayerPage.Layers[index+1];
                    }
                    break;
                case Windows.System.VirtualKey.Back:
                    if (g_PressCtrl == true)
                    {
                        int index = LayerPage.Layers.IndexOf(LayerPage.CheckedLayer);
                        LayerPage.TrashCanButton_Click(null, null);
                        if(index>0)
                            LayerPage.CheckedLayer = LayerPage.Layers[index-1];
                        else if(index==0 && LayerPage.Layers.Count>0)
                            LayerPage.CheckedLayer = LayerPage.Layers[0];
                    }
                    break;
                case Windows.System.VirtualKey.A:
                    if (g_PressCtrl == true)
                    {
                        if (SpacePage.GetSpaceStatus() == SpaceStatus.Editing || SpacePage.GetSpaceStatus() == SpaceStatus.ReEditing)
                        {
                            SpacePage.SelectAllZones();
                            SetLayerButton.IsEnabled = true;
                            SetLayerRectangle.Visibility = Visibility.Collapsed;
                            EditDoneButton.IsEnabled = true;
                        }
                    }
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
            g_PressShift = false;
            g_PressCtrl = false;
            SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += this.OnCloseRequest;
            g_LocalSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            //EffectBlockListView.ItemsSource = GetCommonEffectBlocks();
            oldSortingPositions = new Dictionary<DeviceModel, Point>();

            if (FileListButton.Content.ToString() == "")
            {
                RenameItem.IsEnabled = false;
                DeleteItem.IsEnabled = false;
            }
        }
        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            Log.Debug("[MainPage_Loaded] Intialize ...");

            //Disable settings button until check finish
            SettingsButton.IsEnabled = false;
            SettingsButton.Opacity = 0.5;
            // disable end

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

            #region Check for Update and show icon
            await (new ServiceViewModel()).Sendupdatestatus("CreatorCheckVersion");
            // < 0 No checkallbyservice function
            if (ServiceViewModel.returnnum > 0)
            {
                //顯示需要更新

                SettingBtnNewTab.Visibility = Visibility.Visible;
                needToUpdadte = true;
            }
            else
            {
                SettingBtnNewTab.Visibility = Visibility.Collapsed;
                needToUpdadte = false;
            }
            //Enable settings button until check finish
            MainPage.Self.SettingsButton.IsEnabled = true;
            MainPage.Self.SettingsButton.Opacity = 1;
            //Enable end
            #endregion
        }
        private void LoadSettings()
        {
            bool successful;
            successful = float.TryParse(g_LocalSettings.Values["SpaceZooming"] as string, out float percent);
            SpacePage.SetSpaceZoomPercent(successful ? percent : 50);

            successful = int.TryParse(g_LocalSettings.Values["LayerLevel"] as string, out int level);
            LayerPage.LayerZoomSlider.Value = successful ? level : 2;

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
            await ConnectedDevicesDialog.ShowAsync();
        }

        private void EffectBlockListView_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            string effName = e.Items[0] as string;
            e.Data.Properties.Add("EffectName", effName);

            SpacePage.SetSpaceStatus(SpaceStatus.DraggingEffectBlock);

            // Workaround for keeping EffectBlock in Pressed state
            var ebList = FindAllControl<EffectBlock>(EffectBlockListView, typeof(EffectBlock));
            var index = EffectBlockListView.Items.IndexOf(effName);
            var eb = ebList[index];
            eb.Dragging = true;
            VisualStateManager.GoToState(eb, "Pressed", false);
        }
        private void EffectBlockListView_DragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
        {
            SpacePage.SetSpaceStatus(SpaceStatus.Watching);

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
            ReUndoManager.Undo();
        }
        private void RedoButton_Click(object sender, RoutedEventArgs e)
        {
            ReUndoManager.Redo();
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

                NeedSave = true;
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
                SpacePage.GoToBlankEditing();

                foreach (var pair in oldSortingPositions)
                {
                    pair.Key.PixelLeft = pair.Value.X;
                    pair.Key.PixelTop = pair.Value.Y;
                }
            }

            SpacePage.StopScrollTimer();
            HideMask();
        }
        public void ShowReEditMask(LayerModel layer)
        {
            ShowMask("Edit " + layer.Name);
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

                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        //from Service message
                        StatusTextBlock.Text = "Service : " + response;
                        Log.Debug("[ReceiveData] Rescan ...");
                        await ConnectedDevicesDialog.Rescan();
                    });
                    string[] infoArray = response.Split(new char[3] { '[', ']', ',' });
                    if ((infoArray[1] == "PlugIn") && (infoArray[2] != " "))
                    {
                        g_NewPlugInDeviceName = infoArray[2];

                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                        {
                            var dialog = GetCurrentContentDialog();

                            if (dialog != null)
                            {
                                if (!(dialog is DeviceUpdatePromptDialog))
                                {
                                    dupd = new DeviceUpdatePromptDialog(infoArray[2]);
                                    g_ContentDialog = dupd;
                                }
                            }
                            else
                            {
                                if (CanShowDeviceUpdateDialog)
                                {
                                    dupd = new DeviceUpdatePromptDialog(infoArray[2]);
                                    await dupd.ShowAsync();
                                }
                                else
                                {
                                    dupd = new DeviceUpdatePromptDialog(infoArray[2]);
                                    g_ContentDialog = dupd;
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
                g_ContentDialog = new DeviceUpdatePromptDialog(g_NewPlugInDeviceName);
                await g_ContentDialog.ShowAsync();
            }
        }

        private async void DebugButton_Click(object sender, RoutedEventArgs e)
        {
            //ConnectedDevicesDialog.Rescan();
            SpacePage.DeviceModelCollection[0].PixelLeft = 100;
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
            WindowsPage.Self.WindowsFrame1.Navigate(typeof(SettingsPage), needToUpdadte, new SuppressNavigationTransitionInfo());
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

        }

        private async void ShortcutsItem_Click(object sender, RoutedEventArgs e)
        {
            HotKeyListDialog hld = new HotKeyListDialog();
            await hld.ShowAsync();
        }
    }
}

