using System;
using System.Collections.Generic;
using System.Xml;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using AuraEditor.Common;
using AuraEditor.Dialogs;
using AuraEditor.Models;
using AuraEditor.UserControls;
using static AuraEditor.Common.Definitions;
using static AuraEditor.Common.EffectHelper;
using static AuraEditor.Common.XmlHelper;
using static AuraEditor.Common.MetroEventSource;
using System.Threading.Tasks;

// 空白頁項目範本已記錄在 https://go.microsoft.com/fwlink/?LinkId=234238

namespace AuraEditor.Pages
{
    public sealed partial class SpacePage : Page
    {
        static public SpacePage Self;
        public static bool isMouseInSpacePage = false;
        public SpacePage()
        {
            this.InitializeComponent();
            Self = this;
            SpaceScrollViewer.RegisterPropertyChangedCallback(ScrollViewer.ZoomFactorProperty, (s, e) => ZoomFactorChangedCallback(s, e));
            m_SetLayerButton = MainPage.Self.SetLayerButton;
            m_SetLayerRectangle = MainPage.Self.SetLayerRectangle;
            m_EditDoneButton = MainPage.Self.EditDoneButton;
            m_MouseEventCtrl = IntializeMouseEventCtrl();
            m_ScrollTimerClock = InitializeScrollTimer();
            _mouseDirection = 0;
            _spaceZoomFactor = 1;

            DeviceModelCollection = new List<DeviceModel>();
            GoToBlankEditing();
        }

        private MouseEventCtrl IntializeMouseEventCtrl()
        {
            List<MouseDetectedRegion> regions = new List<MouseDetectedRegion>();

            MouseEventCtrl mec = new MouseEventCtrl
            {
                MonitorMaxRect = new Rect(new Point(0, 0), new Point(SpaceCanvas.Width, SpaceCanvas.Height)),
                DetectionRegions = regions.ToArray()
            };

            return mec;
        }
        private DispatcherTimer InitializeScrollTimer()
        {
            DispatcherTimer timerClock = new DispatcherTimer();
            timerClock.Tick += Timer_Tick;
            timerClock.Interval = new TimeSpan(0, 0, 0, 0, 5); // 10 ms

            return timerClock;
        }

        private Button m_SetLayerButton;
        private Rectangle m_SetLayerRectangle;
        private Button m_EditDoneButton;
        private MouseEventCtrl m_MouseEventCtrl;
        private DispatcherTimer m_ScrollTimerClock;
        private Rect m_CurrentScreenRect;
        public List<DeviceModel> DeviceModelCollection;
        public int OperatingGridWidth
        {
            get
            {
                double leftmost = 999;
                double rightmost = 0;

                DeviceModelCollection.ForEach(d => { if (d.PixelLeft < leftmost) { leftmost = d.PixelLeft; } });
                DeviceModelCollection.ForEach(d => { if (d.PixelRight > rightmost) { rightmost = d.PixelRight; } });

                return (int)((rightmost - leftmost) / GridPixels);
            }
        }
        public int OperatingGridHeight
        {
            get
            {
                double top = 999;
                double bottom = 0;

                DeviceModelCollection.ForEach(d => { if (d.PixelTop < top) { top = d.PixelTop; } });
                DeviceModelCollection.ForEach(d => { if (d.PixelBottom > bottom) { bottom = d.PixelBottom; } });

                return (int)(bottom - top) / GridPixels;
            }
        }
        public Point GetCanvasRightBottomPoint()
        {
            return new Point(SpaceCanvas.Width, SpaceCanvas.Height);
        }
        public Rect GetOperatingPixelRect()
        {
            DeviceModel leftmostDM = DeviceModelCollection[0];
            DeviceModel rightmostDM = DeviceModelCollection[0];
            DeviceModel topDM = DeviceModelCollection[0];
            DeviceModel bottomDM = DeviceModelCollection[0];

            var dms = DeviceModelCollection.FindAll(find => find.Sync == true && find.Plugged == true);

            foreach(var dm in dms)
            {
                if (dm.PixelLeft < leftmostDM.PixelLeft) { leftmostDM = dm; }
                if (dm.PixelRight > rightmostDM.PixelRight) { rightmostDM = dm; }
                if (dm.PixelTop < topDM.PixelTop) { topDM = dm; }
                if (dm.PixelBottom > bottomDM.PixelBottom) { bottomDM = dm; }
            }

            return new Rect(
                leftmostDM.PixelLeft,
                topDM.PixelTop,
                rightmostDM.PixelRight - leftmostDM.PixelLeft,
                bottomDM.PixelBottom - topDM.PixelTop);
        }

        public DeviceModel GetCurrentDeviceByType(int type)
        {
            return DeviceModelCollection.Find(x => x.Type == type);
        }

        #region -- Devices --
        public void FillCurrentIngroupDevices()
        {
            List<DeviceModel> onStageDms = DeviceModelCollection.FindAll(d => d.Plugged == true);
            DeviceModelCollection.Clear();

            foreach (var dm in onStageDms)
            {
                if (dm == null)
                    continue;

                Rect r = new Rect(0, 0, dm.PixelWidth, dm.PixelHeight);
                Point p = GetFreeRoomPositionForRect(r);
                dm.PixelLeft = p.X;
                dm.PixelTop = p.Y;

                DeviceModelCollection.Add(dm);
            }

            RefreshStage();
        }
        public void SendSyncStateToService()
        {
            var dms = DeviceModelCollection.FindAll(find => find.Plugged == true);

            string s = "[SyncStatus]";

            foreach(var dm in dms)
            {
                s += dm.ModelName + ",";
                s += dm.Sync == true ? "1," : "0,";
            }

            MainPage.Self.SendMessageToServer(s);
        }
        public void RefreshStage()
        {
            List<MouseDetectedRegion> regions = new List<MouseDetectedRegion>();

            SpaceCanvas.Children.Clear();
            SpaceCanvas.Children.Add(GridImage);
            SpaceCanvas.Children.Add(RestrictLineLeft);
            SpaceCanvas.Children.Add(RestrictLineRight);
            SpaceCanvas.Children.Add(RestrictLineTop);
            SpaceCanvas.Children.Add(RestrictLineBottom);
            SpaceCanvas.Children.Add(MouseRectangle);

            var onStageList = DeviceModelCollection.FindAll(d => d.Plugged == true && d.Sync == true);

            foreach (var dm in onStageList)
            {
                List<ZoneModel> allzones = dm.AllZones;
                SortByZIndex(allzones);

                foreach (var zone in allzones)
                {
                    Rect relative = zone.GetRect();
                    Rect absolute = new Rect(
                        new Point(relative.Left + dm.PixelLeft, relative.Top + dm.PixelTop),
                        new Point(relative.Right + dm.PixelLeft, relative.Bottom + dm.PixelTop)
                        );

                    MouseDetectedRegion r = new MouseDetectedRegion()
                    {
                        RegionIndex = -1,
                        DetectionRect = absolute,
                        GroupIndex = dm.Type
                    };

                    r.Callback = zone.OnReceiveMouseEvent;

                    regions.Add(r);
                }

                DeviceView view = new DeviceView();
                view.DataContext = dm;
                SpaceCanvas.Children.Add(view);
            }

            m_MouseEventCtrl.DetectionRegions = regions.ToArray();
            GoToBlankEditing();
            StopScrollTimer();


            if (DeviceModelCollection.Count == 0)
                MaskManager.GetInstance().ShowMask(MaskType.NoSupportDevice);
            else if (DeviceModelCollection.Find(find => find.Sync == true) == null)
                MaskManager.GetInstance().ShowMask(MaskType.NoSyncDevice);
            else
                MaskManager.GetInstance().ShowMask(MaskType.None);
        }

        public void ClearTempDeviceData()
        {
            var list = DeviceModelCollection.FindAll(dm => dm.Plugged == false);
            list.ForEach(dm => LayerPage.Self.ClearDeviceData(dm.Type));
            DeviceModelCollection.RemoveAll(d => d.Plugged == false);
        }
        public void Clean()
        {
            IntializeMouseEventCtrl();
            //DeviceModelCollection.Clear();
            GoToBlankEditing();
        }

        public async Task Rescan()
        {
            try
            {
                Log.Debug("[Rescan] Start !");

                List<DeviceModel> remainDMs = new List<DeviceModel>(DeviceModelCollection);
                List<DeviceModel> newDMs = new List<DeviceModel>();
                string deviceList = await GetPluggedDevicesFromService();
                string[] deviceArray = deviceList.Split(':');

                foreach (var deviceString in deviceArray)
                {
                    if (deviceString == "")
                        continue;

                    string[] deviceData = deviceString.Split(',');

                    if (deviceData.Length != 6)
                    {
                        Log.Debug("[Rescan] Invalid device info : " + deviceArray);
                        continue;
                    }

                    var get = DeviceModelCollection.Find(find => find.ModelName == deviceData[0]);
                    if (get == null)
                    {
                        DeviceModel dm = await DeviceModel.ToDeviceModelAsync(
                            deviceData[0], deviceData[1], deviceData[2],
                            deviceData[3], deviceData[4], deviceData[5]);

                        if (dm != null)
                            newDMs.Add(dm);
                    }
                    else
                    {
                        if (deviceData[5] == "true")
                        {
                            get.Plugged = true;
                            get.Sync = true;
                        }
                        else if (deviceData[5] == "false")
                        {
                            get.Sync = false;
                        }

                        remainDMs.Remove(get);
                    }
                }

                foreach (var dm in newDMs)
                {
                    Rect r = new Rect(0, 0, dm.PixelWidth, dm.PixelHeight);
                    Point p = GetFreeRoomPositionForRect(r);
                    dm.PixelLeft = p.X;
                    dm.PixelTop = p.Y;
                    dm.Plugged = true;

                    // Delete temp data as same type as new, because new device is plugging
                    LayerPage.Self.ClearDeviceData(dm.Type);

                    if (dm.Type == (int)DeviceType.Notebook || dm.Type == (int)DeviceType.Desktop)
                        DeviceModelCollection.Insert(0, dm);
                    else
                        DeviceModelCollection.Add(dm);
                }

                // The rest mean device was unplugged
                foreach (var dm in remainDMs)
                    dm.Plugged = false;

                RefreshStage();
            }
            catch
            {
                MaskManager.GetInstance().ShowMask(MaskType.NoSupportDevice);
                Log.Debug("[Rescan] Rescan pluggeddevices failed !");
            }
        }
        private async Task<string> GetPluggedDevicesFromService()
        {
            try
            {
                string result = "";

                await (new ServiceViewModel()).AuraCreatorGetDevice("CREATORGETDEVICE");
                int listcount = Int32.Parse(ServiceViewModel.devicename);
                Log.Debug("[GetPluggedDevices] Plugged device count : " + listcount);
                for (int i = 0; i < listcount; i++)
                {
                    await (new ServiceViewModel()).AuraCreatorGetDevice(i.ToString());
                    //string format : Name,DeviceType,SyncStatus
                    string devicename = ServiceViewModel.devicename;
                    Console.WriteLine(devicename);
                    result += devicename + ":";
                }

                Log.Debug("[GetPluggedDevices] Get plugged devices : " + result);
                //return "G703GI,G703GI,G703GI_US,G703GI_US,Notebook,true:Magnus,Magnus,Magnus,Magnus,Microphone,true:";
                //return "G703GI,G703GI,G703GI_US,G703GI_US,Notebook,true:PUGIO,PUGIO,PUGIO,PUGIO,Mouse,true:";
                return result;
            }
            catch (Exception ex)
            {
                Log.Debug("[GetPluggedDevices] Get Failed : " + ex.ToString());
                //return "G703GI,G703GI,G703GI_US,G703GI_US,Notebook,true:Magnus,Magnus,Magnus,Magnus,Microphone,true:";
                //return "G703GI,G703GI,G703GI_US,G703GI_US,Notebook,true:PUGIO,PUGIO,PUGIO,PUGIO,Mouse,true:";
                return null;
            }
        }
        #endregion

        #region -- SpaceStatus --
        public enum SpaceStatus
        {
            None = 0,
            Editing,
            ReEditing,
            Sorting,
            Watching,
            DraggingEffectBlock,
            DraggingWindow,
        }

        private SpaceStatus spaceStatus;
        public SpaceStatus GetSpaceStatus()
        {
            return spaceStatus;
        }
        public void SetSpaceStatus(SpaceStatus value)
        {
            if (value == spaceStatus)
                return;

            SpaceCanvas.PointerPressed -= SpaceGrid_PointerPressedForDraggingWindow;
            SpaceCanvas.PointerPressed -= SpaceGrid_PointerPressed;
            SpaceCanvas.PointerMoved -= SpaceGrid_PointerMovedForDraggingWindow;
            SpaceCanvas.PointerMoved -= SpaceGrid_PointerMoved;
            SpaceCanvas.PointerReleased -= SpaceGrid_PointerReleased;
            SpaceCanvas.PointerReleased -= SpaceGrid_PointerReleasedForCursor;
            RestrictLineLeft.Visibility = Visibility.Collapsed;
            RestrictLineRight.Visibility = Visibility.Collapsed;
            RestrictLineTop.Visibility = Visibility.Collapsed;
            RestrictLineBottom.Visibility = Visibility.Collapsed;
            DisableAllDevicesOperation();

            if (value == SpaceStatus.Editing)
            {
                SpaceCanvas.PointerPressed += SpaceGrid_PointerPressed;
                SpaceCanvas.PointerMoved += SpaceGrid_PointerMoved;
                SpaceCanvas.PointerReleased += SpaceGrid_PointerReleased;
            }
            else if (value == SpaceStatus.ReEditing)
            {
                SpaceCanvas.PointerPressed += SpaceGrid_PointerPressed;
                SpaceCanvas.PointerMoved += SpaceGrid_PointerMoved;
                SpaceCanvas.PointerReleased += SpaceGrid_PointerReleased;
                m_SetLayerButton.IsEnabled = true;
                m_SetLayerRectangle.Visibility = Visibility.Collapsed;
            }
            else if (value == SpaceStatus.Watching)
            {
                SpaceCanvas.PointerPressed += SpaceGrid_PointerPressed;
                m_SetLayerButton.IsEnabled = false;
                m_SetLayerRectangle.Visibility = Visibility.Visible;
            }
            else if (value == SpaceStatus.DraggingEffectBlock)
            {
                SpaceCanvas.PointerPressed += SpaceGrid_PointerPressed;
                m_SetLayerButton.IsEnabled = false;
                m_SetLayerRectangle.Visibility = Visibility.Visible;
            }
            else if (value == SpaceStatus.Sorting)
            {
                EnableAllDevicesOperation();
                m_SetLayerButton.IsEnabled = true;
                m_SetLayerRectangle.Visibility = Visibility.Collapsed;

                RestrictLineLeft.Visibility = Visibility.Visible;
                RestrictLineRight.Visibility = Visibility.Visible;
                RestrictLineTop.Visibility = Visibility.Visible;
                RestrictLineBottom.Visibility = Visibility.Visible;
            }
            else if (value == SpaceStatus.DraggingWindow)
            {
                SpaceCanvas.PointerPressed += SpaceGrid_PointerPressedForDraggingWindow;
                SpaceCanvas.PointerMoved += SpaceGrid_PointerMovedForDraggingWindow;
                SpaceCanvas.PointerReleased += SpaceGrid_PointerReleasedForCursor;
            }

            spaceStatus = value;
        }
        private void EnableAllDevicesOperation()
        {
            foreach (var dm in DeviceModelCollection)
            {
                dm.OperationEnabled = true;
            }
        }
        private void DisableAllDevicesOperation()
        {
            foreach (var dm in DeviceModelCollection)
            {
                dm.OperationEnabled = false;
            }
        }
        public void GoToBlankEditing()
        {
            if (LayerPage.Self != null)
            {
                LayerPage.Self.CheckedLayer = null;
            }

            UnselectAllZones();
            m_SetLayerButton.IsEnabled = false;
            m_SetLayerRectangle.Visibility = Visibility.Visible;
            SetSpaceStatus(SpaceStatus.Editing);
        }
        #endregion

        #region -- Space Zoom Factor --
        private void SpaceZoom_Click(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuFlyoutItem;
            string itemText = item.Text;

            if (itemText == SpaceZoomButton.Content as string)
                return;

            float percent = float.Parse(itemText.Replace(" %", ""));
            SetSpaceZoomPercent(percent);
        }

        public void SpaceZoom_For_Hotkey(bool zoom_in)
        {
            double percent = double.Parse(SpaceZoomButton.Content.ToString().Replace(" %", ""));
            if (zoom_in)
                percent = Math2.FloorToTarget(percent, 25) + 25;
            else
                percent = Math2.CeilingToTarget(percent, 25) - 25;
            if (percent < 25)
                return;
            SetSpaceZoomPercent((float)percent);
        }

        private float _spaceZoomFactor;
        public float SpaceZoomFactor
        {
            get
            {
                return _spaceZoomFactor;
            }
        }
        public float GetSpaceZoomPercent()
        {
            return _spaceZoomFactor * SpaceZoomDefaultPercent;
        }
        public void SetSpaceZoomPercent(float percent)
        {
            float newFactor = percent / SpaceZoomDefaultPercent;
            float rate = newFactor / _spaceZoomFactor;

            Task t = Task.Factory.StartNew(async () =>
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    int expired = 10;
                    bool a;
                    do
                    {
                        a = SpaceScrollViewer.ChangeView(
                        SpaceScrollViewer.HorizontalOffset * rate,
                        SpaceScrollViewer.VerticalOffset * rate, newFactor, true);
                        await Task.Delay(100);
                        expired--;
                    }
                    while (!a && expired != 0);
                });
            });

            _spaceZoomFactor = newFactor;
        }
        private void ZoomFactorChangedCallback(DependencyObject s, DependencyProperty e)
        {
            var zoomObject = SpaceScrollViewer.GetValue(e);
            double factor = Convert.ToDouble(zoomObject);
            int zoomPercent = (int)(Math.Round(factor * SpaceZoomDefaultPercent, 1));

            SpaceZoomButton.Content = zoomPercent.ToString() + " %";
            _spaceZoomFactor = (float)factor;
        }

        public void DefaultViewButton_Click(object sender, RoutedEventArgs e)
        {
            SpaceScrollViewer.ChangeView(0, 0, 1, true);
        }
        public void FitAllButton_Click(object sender, RoutedEventArgs e)
        {
            var rect = GetOperatingPixelRect();
            var rateW = rect.Width / SpaceScrollViewer.ActualWidth;
            var rateH = rect.Height / SpaceScrollViewer.ActualHeight;
            double rate;
            double AlignLeft = rect.Left;
            double AlignTop = rect.Top;

            if (rateW > rateH)
            {
                rate = rateW;
                AlignLeft = rect.Left / rate;
                var rectW_Mid = rect.Height / rate / 2;
                var screenW_Mid = SpaceScrollViewer.ActualHeight / 2;
                var moveOffsetToCenter = screenW_Mid - rectW_Mid;
                AlignTop = rect.Top / rate - moveOffsetToCenter;
            }
            else
            {
                rate = rateH;
                var rectH_Mid = rect.Width / rate / 2;
                var screenH_Mid = SpaceScrollViewer.ActualWidth / 2;
                var moveOffsetToCenter = screenH_Mid - rectH_Mid;
                AlignLeft = rect.Left / rate - moveOffsetToCenter;
                AlignTop = rect.Top / rate;
            }

            SpaceScrollViewer.ChangeView(
                AlignLeft, AlignTop,
                (float)(1 / rate), true);
        }
        #endregion

        #region -- Device Sorting --
        public void MoveDeviceToFreeRoom(DeviceModel dm)
        {
            if (dm == null)
                return;

            Rect r = new Rect(0, 0, dm.PixelWidth, dm.PixelHeight);
            Point p = GetFreeRoomPositionForRect(r);
            dm.PixelLeft = p.X;
            dm.PixelTop = p.Y;
        }
        public void MoveMousePosition(DeviceModel device, double offsetX, double offsetY)
        {
            m_MouseEventCtrl.MoveGroupRects(device.Type, offsetX, offsetY);
        }
        public void DeleteOverlappingTempDevice(DeviceModel testDev)
        {
            List<DeviceModel> tempDevices = DeviceModelCollection.FindAll(d => d.Plugged == false);
            foreach (var dm in tempDevices)
            {
                if (testDev.Equals(dm))
                    continue;

                if (ControlHelper.IsPiling(testDev.PixelLeft, testDev.PixelWidth, dm.PixelLeft, dm.PixelWidth) &&
                    ControlHelper.IsPiling(testDev.PixelTop, testDev.PixelHeight, dm.PixelTop, dm.PixelHeight))
                {
                    DeviceModelCollection.Remove(dm);
                    LayerPage.Self.ClearDeviceData(dm.Type);
                }
            }
        }
        public void OnDeviceMoveStarted()
        {
            RefreshCurrentScreenRect();
            m_ScrollTimerClock.Start();
        }
        public void OnDeviceMoved(DeviceModel movedDev)
        {
            if (GetSpaceStatus() != SpaceStatus.Sorting) return;

            List<DeviceModel> dms = DeviceModelCollection.FindAll(d => d.Plugged == true && d.Sync == true);

            foreach (var dm in dms)
            {
                if (movedDev.Equals(dm))
                    continue;

                if (ControlHelper.IsPiling(movedDev.PixelLeft, movedDev.PixelWidth, dm.PixelLeft, dm.PixelWidth) &&
                    ControlHelper.IsPiling(movedDev.PixelTop, movedDev.PixelHeight, dm.PixelTop, dm.PixelHeight))
                {
                    dm.VisualState = "Hover";
                }
                else
                {
                    dm.VisualState = "Normal";
                }
            }
        }
        public void StopScrollTimer()
        {
            m_ScrollTimerClock.Stop();
        }
        public bool IsPiling(DeviceModel testDev)
        {
            List<DeviceModel> dms = DeviceModelCollection.FindAll(d => d.Plugged == true && d.Sync == true);

            foreach (var dm in dms)
            {
                if (testDev.Equals(dm))
                    continue;

                if (ControlHelper.IsPiling(testDev.PixelLeft, testDev.PixelWidth, dm.PixelLeft, dm.PixelWidth) &&
                    ControlHelper.IsPiling(testDev.PixelTop, testDev.PixelHeight, dm.PixelTop, dm.PixelHeight))
                {
                    return true;
                }
            }
            return false;
        }
        public Point GetFreeRoomPositionForRect(Rect rect)
        {
            DeviceModel overlappingDM = GetFirstOverlappingDevice(rect);

            if (overlappingDM == null)
                return new Point(rect.X, rect.Y);
            else
            {
                return GetFreeRoomPositionForRect(new Rect(
                    overlappingDM.PixelRight,
                    overlappingDM.PixelTop,
                    rect.Width,
                    rect.Height));
            }
        }
        private DeviceModel GetFirstOverlappingDevice(Rect rect)
        {
            var dms = DeviceModelCollection.FindAll(d => d.Plugged == true && d.Sync == true);

            foreach (var dm in dms)
            {
                if (ControlHelper.IsPiling(rect.X, rect.Width, dm.PixelLeft, dm.PixelWidth) &&
                    ControlHelper.IsPiling(rect.Y, rect.Height, dm.PixelTop, dm.PixelBottom))
                {
                    return dm;
                }
            }
            return null;
        }
        #endregion

        #region -- Zones --
        private void SortByZIndex(List<ZoneModel> zones)
        {
            int count = zones.Count;

            // Bubble sort
            for (int i = 0; i < count - 1; i++)
            {
                for (int j = 0; j < count - 1 - i; j++)
                {
                    if (zones[j].Zindex < zones[j + 1].Zindex)
                    {
                        var z = zones[j];
                        zones[j] = zones[j + 1];
                        zones[j + 1] = z;
                    }
                }
            }
        }
        public void WatchLayer(LayerModel layer)
        {
            SetSpaceStatus(SpaceStatus.Watching);
            UnselectAllZones();

            if (layer == null)
                layer = LayerPage.Self.CheckedLayer;
            if (layer == null)
                return;

            Dictionary<int, int[]> dictionary = layer.GetZoneDictionary();

            // According to the layer, assign selection status for every zone
            foreach (var pair in dictionary)
            {
                DeviceModel dm = GetCurrentDeviceByType(pair.Key);
                int[] indexes = pair.Value;

                if (dm == null)
                    continue;

                foreach (var zone in dm.AllZones)
                {
                    if (Array.Exists(indexes, x => x == zone.Index) == true)
                    {
                        zone.ChangeStatus(RegionStatus.Watching);
                    }
                }
            }
        }
        public void ReEditZones_Start(LayerModel layer)
        {
            SetSpaceStatus(SpaceStatus.ReEditing);
            UnselectAllZones();

            Dictionary<int, int[]> dictionary = layer.GetZoneDictionary();

            // According to the layer, assign selection status for every zone
            foreach (KeyValuePair<int, int[]> pair in dictionary)
            {
                DeviceModel d = GetCurrentDeviceByType(pair.Key);
                int[] indexes = pair.Value;

                if (d == null)
                    continue;

                foreach (var zone in d.AllZones)
                {
                    if (Array.Exists(indexes, x => x == zone.Index) == true)
                    {
                        zone.ChangeStatus(RegionStatus.Selected);
                    }
                }
            }
            m_EditDoneButton.IsEnabled = true;
        }
        public int GetSelectedZoneCount()
        {
            int count = 0;

            foreach (var d in DeviceModelCollection)
            {
                foreach (var zone in d.AllZones)
                {
                    if (zone.Selected)
                        count++;
                }
            }

            return count;
        }
        public void UnselectAllZones()
        {
            foreach (var d in DeviceModelCollection)
            {
                foreach (var zone in d.AllZones)
                {
                    zone.ChangeStatus(RegionStatus.Normal);
                }
            }
        }

        public void SelectAllZones()
        {
            foreach (var d in DeviceModelCollection)
            {
                foreach (var zone in d.AllZones)
                {
                    zone.ChangeStatus(RegionStatus.Selected);
                }
            }
        }
        #endregion

        #region -- Mouse Operations --
        public bool PressShift;
        public bool PressCtrl;
        private int _mouseDirection;

        private void RefreshCurrentScreenRect()
        {
            var ttv = SpaceScrollViewer.TransformToVisual(Window.Current.Content);
            Point screenCoords = ttv.TransformPoint(new Point(0, 0));
            m_CurrentScreenRect = new Rect(screenCoords.X, screenCoords.Y, SpaceScrollViewer.ActualWidth, SpaceScrollViewer.ActualHeight);
        }
        private void Timer_Tick(object sender, object e)
        {
            int offset = 10;
            var pointerPosition = Windows.UI.Core.CoreWindow.GetForCurrentThread().PointerPosition;
            var x = pointerPosition.X - Window.Current.Bounds.X;
            var y = pointerPosition.Y - Window.Current.Bounds.Y;
            Point position = new Point(x, y);

            if (position.X > m_CurrentScreenRect.Right && position.Y > m_CurrentScreenRect.Bottom)
            {
                _mouseDirection = 9;
            }
            else if (position.X > m_CurrentScreenRect.Right && position.Y < m_CurrentScreenRect.Top)
            {
                _mouseDirection = 3;
            }
            else if (position.X > m_CurrentScreenRect.Right)
            {
                _mouseDirection = 6;
            }
            else if (position.X < m_CurrentScreenRect.Left && position.Y > m_CurrentScreenRect.Bottom)
            {
                _mouseDirection = 7;
            }
            else if (position.X < m_CurrentScreenRect.Left && position.Y < m_CurrentScreenRect.Top)
            {
                _mouseDirection = 1;
            }
            else if (position.X < m_CurrentScreenRect.Left)
            {
                _mouseDirection = 4;
            }
            else if (position.Y < m_CurrentScreenRect.Top)
            {
                _mouseDirection = 2;
            }
            else if (position.Y > m_CurrentScreenRect.Bottom)
            {
                _mouseDirection = 8;
            }
            else
                _mouseDirection = 5;

            if (_mouseDirection == 1)
            {
                SpaceScrollViewer.ChangeView(
                    SpaceScrollViewer.HorizontalOffset - offset,
                    SpaceScrollViewer.VerticalOffset - offset,
                    _spaceZoomFactor, true);
            }
            else if (_mouseDirection == 2)
            {
                SpaceScrollViewer.ChangeView(
                    SpaceScrollViewer.HorizontalOffset,
                    SpaceScrollViewer.VerticalOffset - offset,
                    _spaceZoomFactor, true);
            }
            else if (_mouseDirection == 3)
            {
                SpaceScrollViewer.ChangeView(
                    SpaceScrollViewer.HorizontalOffset + offset,
                    SpaceScrollViewer.VerticalOffset - offset,
                    _spaceZoomFactor, true);
            }
            else if (_mouseDirection == 4)
            {
                SpaceScrollViewer.ChangeView(
                    SpaceScrollViewer.HorizontalOffset - offset,
                    SpaceScrollViewer.VerticalOffset,
                    _spaceZoomFactor, true);
            }
            else if (_mouseDirection == 5) { }
            else if (_mouseDirection == 6)
            {
                SpaceScrollViewer.ChangeView(
                    SpaceScrollViewer.HorizontalOffset + offset,
                    SpaceScrollViewer.VerticalOffset,
                    _spaceZoomFactor, true);
            }
            else if (_mouseDirection == 7)
            {
                SpaceScrollViewer.ChangeView(
                    SpaceScrollViewer.HorizontalOffset - offset,
                    SpaceScrollViewer.VerticalOffset + offset,
                    _spaceZoomFactor, true);
            }
            else if (_mouseDirection == 8)
            {
                SpaceScrollViewer.ChangeView(
                    SpaceScrollViewer.HorizontalOffset,
                    SpaceScrollViewer.VerticalOffset + offset,
                    _spaceZoomFactor, true);
            }
            else if (_mouseDirection == 9)
            {
                SpaceScrollViewer.ChangeView(
                    SpaceScrollViewer.HorizontalOffset + offset,
                    SpaceScrollViewer.VerticalOffset + offset,
                    _spaceZoomFactor, true);
            }
        }
        private void SpaceGrid_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            PressShift = MainPage.Self.g_PressShift;
            PressCtrl = MainPage.Self.g_PressCtrl;
            if (!PressShift && !PressCtrl)
                UnselectAllZones();

            if (GetSpaceStatus() == SpaceStatus.Watching)
            {
                GoToBlankEditing();
            }

            var fe = sender as FrameworkElement;
            Point position = e.GetCurrentPoint(fe).Position;
            m_MouseEventCtrl.OnMousePressed(position);

            RefreshCurrentScreenRect();
            m_ScrollTimerClock.Start();
        }
        private void SpaceGrid_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            var fe = sender as FrameworkElement;
            PointerPoint ptrPt = e.GetCurrentPoint(fe);
            Point position = ptrPt.Position;

            if (ptrPt.Properties.IsLeftButtonPressed)
            {
                m_MouseEventCtrl.OnMouseMoved(position, true);
                bool _hasCapture = fe.CapturePointer(e.Pointer);
            }
            else
            {
                m_MouseEventCtrl.OnMouseMoved(position, false);
                return; // no need to draw mouse rectangle
            }

            // Draw mouse rectangle
            Rect r = m_MouseEventCtrl.MouseRect;
            TranslateTransform tt = MouseRectangle.RenderTransform as TranslateTransform;

            tt.X = r.X;
            tt.Y = r.Y;
            MouseRectangle.Width = r.Width;
            MouseRectangle.Height = r.Height;
        }
        private void SpaceGrid_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            Rect r = m_MouseEventCtrl.MouseRect;
            m_MouseEventCtrl.OnMouseReleased();
            MouseRectangle.Width = 0;
            MouseRectangle.Height = 0;
            m_ScrollTimerClock.Stop();
            _mouseDirection = 0;

            if (GetSelectedZoneCount() == 0)
            {
                m_SetLayerButton.IsEnabled = false;
                m_SetLayerRectangle.Visibility = Visibility.Visible;
                m_EditDoneButton.IsEnabled = false;
            }
            else
            {
                m_SetLayerButton.IsEnabled = true;
                m_SetLayerRectangle.Visibility = Visibility.Collapsed;
                m_EditDoneButton.IsEnabled = true;
            }
        }
        #endregion

        #region -- Drag Window --
        private Point m_DragWindowPoint;
        private void SpaceGrid_PointerPressedForDraggingWindow(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Custom, 102);   //101 release  102 hold
            var fe = sender as FrameworkElement;
            Point position = e.GetCurrentPoint(fe).Position;
            m_DragWindowPoint = position;
        }
        private void SpaceGrid_PointerMovedForDraggingWindow(object sender, PointerRoutedEventArgs e)
        {
            var fe = sender as FrameworkElement;
            PointerPoint ptrPt = e.GetCurrentPoint(fe);
            Point position = ptrPt.Position;

            if (ptrPt.Properties.IsLeftButtonPressed)
            {
                double move_x = position.X - m_DragWindowPoint.X;
                double move_y = position.Y - m_DragWindowPoint.Y;

                SpaceScrollViewer.ChangeView(
                    SpaceScrollViewer.HorizontalOffset - move_x,
                    SpaceScrollViewer.VerticalOffset - move_y, _spaceZoomFactor, true);
            }
        }

        private SpaceStatus _oldStatus;
        public void OnZKeyPressed()
        {
            _oldStatus = GetSpaceStatus();
            SetSpaceStatus(SpaceStatus.DraggingWindow);
        }
        public void OnZKeyRelease()
        {
            SetSpaceStatus(_oldStatus);
        }
        public void OnDraggingEffectBlock()
        {
            _oldStatus = GetSpaceStatus();
            SetSpaceStatus(SpaceStatus.DraggingEffectBlock);
        }
        public void OnReleaseEffectBlock()
        {
            SetSpaceStatus(_oldStatus);
        }

        private void SpaceGrid_PointerReleasedForCursor(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Custom, 101);
        }
        #endregion

        private void LeftSidePanelButton_Click(object sender, RoutedEventArgs e)
        {
            MainPage.Self.OnLeftSidePanelButtonClick();

            if (LeftSideOpenedButton.Visibility == Visibility.Visible)
            {
                LeftSideOpenedButton.Visibility = Visibility.Collapsed;
                LeftSideClosedButton.Visibility = Visibility.Visible;
            }
            else
            {
                LeftSideOpenedButton.Visibility = Visibility.Visible;
                LeftSideClosedButton.Visibility = Visibility.Collapsed;
            }
        }
        private void RightSidePanelButton_Click(object sender, RoutedEventArgs e)
        {
            MainPage.Self.OnRightSidePanelButtonClick();

            if (RightSideOpenedButton.Visibility == Visibility.Visible)
            {
                RightSideOpenedButton.Visibility = Visibility.Collapsed;
                RightSideClosedButton.Visibility = Visibility.Visible;
            }
            else
            {
                RightSideOpenedButton.Visibility = Visibility.Visible;
                RightSideClosedButton.Visibility = Visibility.Collapsed;
            }
        }

        public XmlNode ToXmlNodeForUserData()
        {
            XmlNode spaceNode = CreateXmlNode("space");

            foreach (var dm in DeviceModelCollection)
            {
                spaceNode.AppendChild(dm.ToXmlNodeForUserData());
            }

            return spaceNode;
        }
        private void MouseEnteredSpacePage(object sender, PointerRoutedEventArgs e)
        {
            isMouseInSpacePage = true;
        }
        private void MouseExitedSpacePage(object sender, PointerRoutedEventArgs e)
        {
            isMouseInSpacePage = false;
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
        }
        private void ZoomAddInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            SpaceZoom_For_Hotkey(true);
            args.Handled = true;
        }

        private void ZoomSubtractInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            SpaceZoom_For_Hotkey(false);
            args.Handled = true;
        }

        private void SelectAllZonesInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            if (GetSpaceStatus() == SpaceStatus.Editing || GetSpaceStatus() == SpaceStatus.ReEditing)
            {
                SelectAllZones();
                m_SetLayerButton.IsEnabled = true;
                m_SetLayerRectangle.Visibility = Visibility.Collapsed;
                m_EditDoneButton.IsEnabled = true;
                args.Handled = true;
            }
        }
    }
}
