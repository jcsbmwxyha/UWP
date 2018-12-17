﻿using System;
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

// 空白頁項目範本已記錄在 https://go.microsoft.com/fwlink/?LinkId=234238

namespace AuraEditor.Pages
{
    public sealed partial class SpacePage : Page
    {
        static public SpacePage Self;

        public SpacePage()
        {
            this.InitializeComponent();
            Self = this;
            SpaceScrollViewer.RegisterPropertyChangedCallback(ScrollViewer.ZoomFactorProperty, (s, e) => ZoomFactorChangedCallback(s, e));
            m_NoSupportedDeviceGrid = MainPage.Self.NoSupportedDeviceGrid;
            m_SetLayerButton = MainPage.Self.SetLayerButton;
            m_SetLayerRectangle = MainPage.Self.SetLayerRectangle;
            m_EditOKButton = MainPage.Self.EditOKButton;
            m_MouseEventCtrl = IntializeMouseEventCtrl();
            m_ScrollTimerClock = InitializeScrollTimer();
            _mouseDirection = 0;
            _spaceZoomFactor = 1;

            DeviceModelCollection = new List<DeviceModel>();
            SetSpaceStatus(SpaceStatus.Init);
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

        private Grid m_NoSupportedDeviceGrid;
        private Button m_SetLayerButton;
        private Rectangle m_SetLayerRectangle;
        private Button m_EditOKButton;
        private MouseEventCtrl m_MouseEventCtrl;
        private DispatcherTimer m_ScrollTimerClock;

        public List<DeviceModel> DeviceModelCollection;
        public int GetCurrentOperatingGridWidth
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
        public int GetCurrentOperatingGridHeight
        {
            get
            {
                double top = 999;
                double bottom = 0;

                DeviceModelCollection.ForEach(d => { if (d.PixelTop < top) { top = d.PixelTop; } });
                DeviceModelCollection.ForEach(d => { if (d.PixelBottom > bottom) { bottom = d.PixelBottom; } });

                return (int)(bottom - top);
            }
        }
        public DeviceModel GetCurrentDeviceByType(int type)
        {
            return DeviceModelCollection.Find(x => x.Type == type);
        }

        #region -- Devices --
        public async void FillCurrentIngroupDevices()
        {
            List<SyncDevice> syncDevices = ConnectedDevicesDialog.Self.GetIngroupDevices();
            DeviceContent deviceContent;
            DeviceModel dm;

            foreach (var d in syncDevices)
            {
                deviceContent = await DeviceContent.GetDeviceContent(d);

                if (deviceContent == null)
                    continue;

                Rect r = new Rect(0, 0, deviceContent.GridWidth, deviceContent.GridHeight);
                Point p = GetFreeRoomPositionForRect(r);
                dm = await deviceContent.ToDeviceModel();
                dm.PixelLeft = p.X;
                dm.PixelTop = p.Y;

                DeviceModelCollection.Add(dm);
            }

            RefreshSpaceScrollViewer();
        }
        public async void OnIngroupDevicesChanged()
        {
            List<SyncDevice> ingroupDevices = ConnectedDevicesDialog.Self.GetIngroupDevices();
            List<SyncDevice> newSD = new List<SyncDevice>();
            List<DeviceModel> tempToStage = new List<DeviceModel>();
            List<DeviceModel> stageToTemp = DeviceModelCollection.FindAll(d => d.Status == DeviceStatus.OnStage);

            foreach (var sd in ingroupDevices)
            {
                DeviceModel dm = DeviceModelCollection.Find(d => d.Name == sd.Name);
                if (dm == null)
                {
                    newSD.Add(sd);

                    // delete temp data because new device is plugging
                    LayerPage.Self.ClearTypeData(sd.Type);
                    DeviceModelCollection.RemoveAll(d => d.Type == GetTypeByTypeName(sd.Type));
                }
                else if (dm.Status == DeviceStatus.Temp)
                    tempToStage.Add(dm);
                else if (dm.Status == DeviceStatus.OnStage)
                    stageToTemp.RemoveAll(d => d.Name == sd.Name);
            }

            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                foreach (var dm in stageToTemp)
                {
                    dm.Status = DeviceStatus.Temp;
                }
                foreach (var dm in tempToStage)
                {
                    dm.Status = DeviceStatus.OnStage;
                }
                foreach (var sd in newSD)
                {
                    DeviceContent deviceContent = await DeviceContent.GetDeviceContent(sd);

                    if (deviceContent == null)
                        continue;

                    Rect r = new Rect(0, 0, deviceContent.GridWidth, deviceContent.GridHeight);
                    Point p = GetFreeRoomPositionForRect(r);
                    DeviceModel dm = await deviceContent.ToDeviceModel();
                    dm.PixelLeft = p.X;
                    dm.PixelTop = p.Y;
                    dm.Status = DeviceStatus.OnStage;
                    DeviceModelCollection.Add(dm);
                }
                RefreshSpaceScrollViewer();
            });
        }
        public void RefreshSpaceScrollViewer()
        {
            List<MouseDetectedRegion> regions = new List<MouseDetectedRegion>();

            SpaceCanvas.Children.Clear();
            SpaceCanvas.Children.Add(GridImage);
            SpaceCanvas.Children.Add(MouseRectangle);

            var onStageList = DeviceModelCollection.FindAll(d => d.Status == DeviceStatus.OnStage);

            if (onStageList.Count == 0)
            {
                m_NoSupportedDeviceGrid.Visibility = Visibility.Visible;
                return;
            }
            else
            {
                m_NoSupportedDeviceGrid.Visibility = Visibility.Collapsed;
            }

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
            UnselectAllZones();
        }

        public void ClearTempDeviceData()
        {
            var list = DeviceModelCollection.FindAll(dm => dm.Status == DeviceStatus.Temp);
            list.ForEach(dm => LayerPage.Self.ClearTypeData(dm.Type));
            DeviceModelCollection.RemoveAll(d => d.Status == DeviceStatus.Temp);
        }
        public void Clean()
        {
            IntializeMouseEventCtrl();
            DeviceModelCollection.Clear();
            SetSpaceStatus(SpaceStatus.Init);
        }
        #endregion

        #region -- SpaceStatus --
        public enum SpaceStatus
        {
            None = 0,
            Init,
            Editing,
            DraggingDevice,
            WatchingLayer,
            DraggingEffectBlock,
            ReEditing,
            DraggingWindow,
        }

        private SpaceStatus _beforeDragWindowStatus;
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

            if (value == SpaceStatus.Init)
            {
                DisableAllDevicesOperation();
                SpaceCanvas.PointerPressed += SpaceGrid_PointerPressed;
                SpaceCanvas.PointerMoved += SpaceGrid_PointerMoved;
                SpaceCanvas.PointerReleased += SpaceGrid_PointerReleased;
                OnInitState();
            }
            else if (value == SpaceStatus.Editing)
            {
                DisableAllDevicesOperation();
                SpaceCanvas.PointerPressed += SpaceGrid_PointerPressed;
                SpaceCanvas.PointerMoved += SpaceGrid_PointerMoved;
                SpaceCanvas.PointerReleased += SpaceGrid_PointerReleased;
                OnEditingState();
            }
            else if (value == SpaceStatus.ReEditing)
            {
                DisableAllDevicesOperation();
                SpaceCanvas.PointerPressed += SpaceGrid_PointerPressed;
                SpaceCanvas.PointerMoved += SpaceGrid_PointerMoved;
                SpaceCanvas.PointerReleased += SpaceGrid_PointerReleased;
                m_SetLayerButton.IsEnabled = true;
                m_SetLayerRectangle.Visibility = Visibility.Collapsed;
            }
            else if (value == SpaceStatus.WatchingLayer)
            {
                DisableAllDevicesOperation();
                SpaceCanvas.PointerPressed += SpaceGrid_PointerPressed;
                m_SetLayerButton.IsEnabled = false;
                m_SetLayerRectangle.Visibility = Visibility.Visible;
            }
            else if (value == SpaceStatus.DraggingEffectBlock)
            {
                DisableAllDevicesOperation();
                SpaceCanvas.PointerPressed += SpaceGrid_PointerPressed;
                m_SetLayerButton.IsEnabled = false;
                m_SetLayerRectangle.Visibility = Visibility.Visible;
            }
            else if (value == SpaceStatus.DraggingDevice)
            {
                EnableAllDevicesOperation();
                m_SetLayerButton.IsEnabled = true;
                m_SetLayerRectangle.Visibility = Visibility.Collapsed;
            }
            else if (value == SpaceStatus.DraggingWindow)
            {
                DisableAllDevicesOperation();
                SpaceCanvas.PointerPressed += SpaceGrid_PointerPressedForDraggingWindow;
                SpaceCanvas.PointerMoved += SpaceGrid_PointerMovedForDraggingWindow;

                _beforeDragWindowStatus = spaceStatus;
            }

            if (value == SpaceStatus.Init)
                spaceStatus = SpaceStatus.Editing;
            else
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
        private void OnInitState()
        {
            if (LayerPage.Self != null)
            {
                LayerPage.Self.CheckedLayer = null;
                UnselectAllZones();
            }
            m_SetLayerButton.IsEnabled = false;
            m_SetLayerRectangle.Visibility = Visibility.Visible;
        }
        private void OnEditingState()
        {
            if (LayerPage.Self != null)
            {
                LayerPage.Self.CheckedLayer = null;
            }
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

            SpaceScrollViewer.ChangeView(
                SpaceScrollViewer.HorizontalOffset * rate,
                SpaceScrollViewer.VerticalOffset * rate, newFactor, true);

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

        private void DefaultViewButton_Click(object sender, RoutedEventArgs e)
        {
            SpaceScrollViewer.ChangeView(0, 0, 1, true);
        }
        private void FitAllButton_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #region -- Device Sorting --
        public void MoveDeviceMousePosition(DeviceModel device, double offsetX, double offsetY)
        {
            m_MouseEventCtrl.MoveGroupRects(device.Type, offsetX, offsetY);
        }
        public void DeleteOverlappingTempDevice(DeviceModel testDev)
        {
            List<DeviceModel> tempDevices = DeviceModelCollection.FindAll(d => d.Status == DeviceStatus.Temp);
            foreach (var dm in tempDevices)
            {
                if (testDev.Equals(dm))
                    continue;

                if (ControlHelper.IsPiling(testDev.PixelLeft, testDev.PixelWidth, dm.PixelLeft, dm.PixelWidth) &&
                    ControlHelper.IsPiling(testDev.PixelTop, testDev.PixelHeight, dm.PixelTop, dm.PixelHeight))
                {
                    DeviceModelCollection.Remove(dm);
                    LayerPage.Self.ClearTypeData(dm.Type);
                }
            }
        }
        public void OnDeviceMoved(DeviceModel movedDev)
        {
            List<DeviceModel> dms = DeviceModelCollection.FindAll(d => d.Status == DeviceStatus.OnStage);

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
        public bool IsPiling(DeviceModel testDev)
        {
            List<DeviceModel> dms = DeviceModelCollection.FindAll(d => d.Status == DeviceStatus.OnStage);

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
                    overlappingDM.PixelWidth,
                    overlappingDM.PixelHeight));
            }
        }
        private DeviceModel GetFirstOverlappingDevice(Rect gridRect)
        {
            foreach (var dm in DeviceModelCollection)
            {
                if (ControlHelper.IsPiling(gridRect.X, gridRect.Width, dm.PixelLeft, dm.PixelWidth) &&
                    ControlHelper.IsPiling(gridRect.Y, gridRect.Height, dm.PixelTop, dm.PixelBottom))
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
        public void WatchLayer(Layer layer)
        {
            SetSpaceStatus(SpaceStatus.WatchingLayer);
            UnselectAllZones();

            if (layer == null)
                layer = LayerPage.Self.CheckedLayer;
            if (layer == null)
                return;

            Dictionary<int, int[]> dictionary = layer.GetZoneDictionary();

            // According to the layer, assign selection status for every zone
            foreach (KeyValuePair<int, int[]> pair in dictionary)
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
        public void ReEditZones_Start(Layer layer)
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
            m_EditOKButton.IsEnabled = true;
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
        #endregion

        #region -- Mouse Operations --
        public bool PressShift;
        public bool PressCtrl;
        private int _mouseDirection;

        private void Timer_Tick(object sender, object e)
        {
            int offset = 10;
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

            if (GetSpaceStatus() == SpaceStatus.WatchingLayer)
            {
                SetSpaceStatus(SpaceStatus.Init);
            }

            var fe = sender as FrameworkElement;
            Point Position = e.GetCurrentPoint(fe).Position;
            m_MouseEventCtrl.OnMousePressed(Position);
            m_ScrollTimerClock.Start();
        }
        private void SpaceGrid_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            var fe = sender as FrameworkElement;
            PointerPoint ptrPt = e.GetCurrentPoint(fe);
            Point Position = ptrPt.Position;

            if (ptrPt.Properties.IsLeftButtonPressed)
            {
                m_MouseEventCtrl.OnMouseMoved(Position, true);
                bool _hasCapture = fe.CapturePointer(e.Pointer);
            }
            else
            {
                m_MouseEventCtrl.OnMouseMoved(Position, false);
                return; // no need to draw mouse rectangle
            }

            // Draw mouse rectangle
            Rect r = m_MouseEventCtrl.MouseRect;
            CompositeTransform ct = MouseRectangle.RenderTransform as CompositeTransform;

            ct.TranslateX = r.X;
            ct.TranslateY = r.Y;
            MouseRectangle.Width = r.Width;
            MouseRectangle.Height = r.Height;

            Rect screenRect = new Rect(
                    SpaceScrollViewer.HorizontalOffset / _spaceZoomFactor,
                    SpaceScrollViewer.VerticalOffset / _spaceZoomFactor,
                    SpaceScrollViewer.ActualWidth / _spaceZoomFactor,
                    SpaceScrollViewer.ActualHeight / _spaceZoomFactor);

            if (Position.X > screenRect.Right && Position.Y > screenRect.Bottom)
            {
                _mouseDirection = 9;
            }
            else if (Position.X > screenRect.Right && Position.Y < screenRect.Top)
            {
                _mouseDirection = 3;
            }
            else if (Position.X > screenRect.Right)
            {
                _mouseDirection = 6;
            }
            else if (Position.X < screenRect.Left && Position.Y > screenRect.Bottom)
            {
                _mouseDirection = 7;
            }
            else if (Position.X < screenRect.Left && Position.Y < screenRect.Top)
            {
                _mouseDirection = 1;
            }
            else if (Position.X < screenRect.Left)
            {
                _mouseDirection = 4;
            }
            else if (Position.Y < screenRect.Top)
            {
                _mouseDirection = 2;
            }
            else if (Position.Y > screenRect.Bottom)
            {
                _mouseDirection = 8;
            }
            else
                _mouseDirection = 5;
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
                m_EditOKButton.IsEnabled = false;
            }
            else
            {
                m_SetLayerButton.IsEnabled = true;
                m_SetLayerRectangle.Visibility = Visibility.Collapsed;
                m_EditOKButton.IsEnabled = true;
            }
        }
        #endregion

        #region -- Drag Window --
        private Point m_DragWindowPoint;
        private void SpaceGrid_PointerPressedForDraggingWindow(object sender, PointerRoutedEventArgs e)
        {
            var fe = sender as FrameworkElement;
            Point Position = e.GetCurrentPoint(fe).Position;

            m_DragWindowPoint = Position;
        }
        private void SpaceGrid_PointerMovedForDraggingWindow(object sender, PointerRoutedEventArgs e)
        {
            var fe = sender as FrameworkElement;
            PointerPoint ptrPt = e.GetCurrentPoint(fe);
            Point Position = ptrPt.Position;

            if (ptrPt.Properties.IsLeftButtonPressed)
            {
                double move_x = Position.X - m_DragWindowPoint.X;
                double move_y = Position.Y - m_DragWindowPoint.Y;

                SpaceScrollViewer.ChangeView(
                    SpaceScrollViewer.HorizontalOffset - move_x,
                    SpaceScrollViewer.VerticalOffset - move_y, _spaceZoomFactor, true);
            }
        }
        public void OnZKeyPressed()
        {
            SetSpaceStatus(SpaceStatus.DraggingWindow);
        }
        public void OnZKeyRelease()
        {
            SetSpaceStatus(_beforeDragWindowStatus);
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
    }
}