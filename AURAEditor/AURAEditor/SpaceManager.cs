using AuraEditor.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Foundation;
using Windows.UI.Xaml.Shapes;
using System.Xml;
using Windows.Storage;
using Windows.UI.Xaml.Media;
using Windows.UI;
using System.IO;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml.Input;
using Windows.UI.Input;
using static AuraEditor.Common.StorageHelper;
using static AuraEditor.Common.XmlHelper;
using static AuraEditor.Common.EffectHelper;
using AuraEditor.Dialogs;

namespace AuraEditor
{
    public class AuraSpaceManager
    {
        static public AuraSpaceManager Self;

        #region SpaceStatus
        public enum SpaceStatus
        {
            None = 0,
            Normal = 1,
            DragingDevice = 2,
            WatchingLayer = 3,
            DragingEffectBlock = 4,
            ReEditing = 5,
        }
        private SpaceStatus spaceGridStatus;
        public SpaceStatus GetSpaceStatus()
        {
            return spaceGridStatus;
        }
        public void SetSpaceStatus(SpaceStatus value)
        {
            if (value == spaceGridStatus)
                return;

            spaceGridStatus = value;

            if (value == SpaceStatus.Normal)
            {
                DisableAllDevicesOperation();
                m_SpaceCanvas.PointerPressed -= SpaceGrid_PointerPressed;
                m_SpaceCanvas.PointerMoved -= SpaceGrid_PointerMoved;
                m_SpaceCanvas.PointerReleased -= SpaceGrid_PointerReleased;
                m_SpaceCanvas.RightTapped -= SpaceGrid_RightTapped;

                m_SpaceCanvas.PointerPressed += SpaceGrid_PointerPressed;
                m_SpaceCanvas.PointerMoved += SpaceGrid_PointerMoved;
                m_SpaceCanvas.PointerReleased += SpaceGrid_PointerReleased;
                m_SpaceCanvas.RightTapped += SpaceGrid_RightTapped;
                m_SetLayerButton.IsEnabled = true;
                OnNormalState();
            }
            else if (value == SpaceStatus.ReEditing)
            {
                DisableAllDevicesOperation();
                m_SpaceCanvas.PointerPressed -= SpaceGrid_PointerPressed;
                m_SpaceCanvas.PointerMoved -= SpaceGrid_PointerMoved;
                m_SpaceCanvas.PointerReleased -= SpaceGrid_PointerReleased;
                m_SpaceCanvas.RightTapped -= SpaceGrid_RightTapped;

                m_SpaceCanvas.PointerPressed += SpaceGrid_PointerPressed;
                m_SpaceCanvas.PointerMoved += SpaceGrid_PointerMoved;
                m_SpaceCanvas.PointerReleased += SpaceGrid_PointerReleased;
                m_SpaceCanvas.RightTapped += SpaceGrid_RightTapped;
                m_SetLayerButton.IsEnabled = true;
            }
            else if (value == SpaceStatus.WatchingLayer)
            {
                DisableAllDevicesOperation();
                m_SpaceCanvas.PointerPressed -= SpaceGrid_PointerPressed;
                m_SpaceCanvas.PointerPressed += SpaceGrid_PointerPressed;

                m_SpaceCanvas.PointerMoved -= SpaceGrid_PointerMoved;
                m_SpaceCanvas.PointerReleased -= SpaceGrid_PointerReleased;
                m_SpaceCanvas.RightTapped -= SpaceGrid_RightTapped;
                m_SetLayerButton.IsEnabled = false;
                WatchCurrentLayer();
            }
            else if (value == SpaceStatus.DragingEffectBlock)
            {
                DisableAllDevicesOperation();
                m_SpaceCanvas.PointerPressed -= SpaceGrid_PointerPressed;
                m_SpaceCanvas.PointerPressed += SpaceGrid_PointerPressed;

                m_SpaceCanvas.PointerMoved -= SpaceGrid_PointerMoved;
                m_SpaceCanvas.PointerReleased -= SpaceGrid_PointerReleased;
                m_SpaceCanvas.RightTapped -= SpaceGrid_RightTapped;
                m_SetLayerButton.IsEnabled = false;
            }
            else if (value == SpaceStatus.DragingDevice)
            {
                EableAllDevicesOperation();
                m_SpaceCanvas.PointerPressed -= SpaceGrid_PointerPressed;
                m_SpaceCanvas.PointerMoved -= SpaceGrid_PointerMoved;
                m_SpaceCanvas.PointerReleased -= SpaceGrid_PointerReleased;
                m_SpaceCanvas.RightTapped -= SpaceGrid_RightTapped;
                m_SetLayerButton.IsEnabled = true;
            }
        }
        private void EableAllDevicesOperation()
        {
            foreach (var d in GlobalDevices)
            {
                d.EnableManipulation();
            }
        }
        private void DisableAllDevicesOperation()
        {
            foreach (var d in GlobalDevices)
            {
                d.DisableManipulation();
            }
        }
        #endregion

        private ScrollViewer m_SpaceScrollViewer;
        private Canvas m_SpaceCanvas;
        private Rectangle m_MouseRectangle;
        private Image m_GridImage;
        private Button m_SetLayerButton;
        private MouseEventCtrl m_MouseEventCtrl;
        private DispatcherTimer m_ScrollTimerClock;
        private int _mouseDirection;
        public float SpaceZoomFactor { get; private set; }
        public List<Device> GlobalDevices;
        public List<Device> TempDevices;

        public AuraSpaceManager()
        {
            Self = this;
            m_SpaceScrollViewer = MainPage.Self.SpaceAreaScrollViewer;
            m_SpaceCanvas = MainPage.Self.SpaceAreaCanvas;
            m_GridImage = MainPage.Self.GridImage;
            m_MouseRectangle = MainPage.Self.MouseRectangle;
            m_SetLayerButton = MainPage.Self.SetLayerButton;
            m_MouseEventCtrl = IntializeMouseEventCtrl();
            m_ScrollTimerClock = InitializeScrollTimer();
            _mouseDirection = 0;
            SpaceZoomFactor = 1;

            GlobalDevices = new List<Device>();
            TempDevices = new List<Device>();
            SetSpaceStatus(SpaceStatus.Normal);
        }
        private MouseEventCtrl IntializeMouseEventCtrl()
        {
            List<MouseDetectionRegion> regions = new List<MouseDetectionRegion>();

            MouseEventCtrl mec = new MouseEventCtrl
            {
                MonitorMaxRect = new Rect(new Point(0, 0), new Point(2700, 1500)),
                //new Point(m_SpaceCanvas.ActualWidth, m_SpaceCanvas.ActualHeight)),
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
        private void Timer_Tick(object sender, object e)
        {
            int offset = 10;
            if (_mouseDirection == 1)
            {
                m_SpaceScrollViewer.ChangeView(
                    m_SpaceScrollViewer.HorizontalOffset - offset,
                    m_SpaceScrollViewer.VerticalOffset - offset,
                    SpaceZoomFactor, true);
            }
            else if (_mouseDirection == 2)
            {
                m_SpaceScrollViewer.ChangeView(
                    m_SpaceScrollViewer.HorizontalOffset,
                    m_SpaceScrollViewer.VerticalOffset - offset,
                    SpaceZoomFactor, true);
            }
            else if (_mouseDirection == 3)
            {
                m_SpaceScrollViewer.ChangeView(
                    m_SpaceScrollViewer.HorizontalOffset + offset,
                    m_SpaceScrollViewer.VerticalOffset - offset,
                    SpaceZoomFactor, true);
            }
            else if (_mouseDirection == 4)
            {
                m_SpaceScrollViewer.ChangeView(
                    m_SpaceScrollViewer.HorizontalOffset - offset,
                    m_SpaceScrollViewer.VerticalOffset,
                    SpaceZoomFactor, true);
            }
            else if (_mouseDirection == 5) { }
            else if (_mouseDirection == 6)
            {
                m_SpaceScrollViewer.ChangeView(
                    m_SpaceScrollViewer.HorizontalOffset + offset,
                    m_SpaceScrollViewer.VerticalOffset,
                    SpaceZoomFactor, true);
            }
            else if (_mouseDirection == 7)
            {
                m_SpaceScrollViewer.ChangeView(
                    m_SpaceScrollViewer.HorizontalOffset - offset,
                    m_SpaceScrollViewer.VerticalOffset + offset,
                    SpaceZoomFactor, true);
            }
            else if (_mouseDirection == 8)
            {
                m_SpaceScrollViewer.ChangeView(
                    m_SpaceScrollViewer.HorizontalOffset,
                    m_SpaceScrollViewer.VerticalOffset + offset,
                    SpaceZoomFactor, true);
            }
            else if (_mouseDirection == 9)
            {
                m_SpaceScrollViewer.ChangeView(
                    m_SpaceScrollViewer.HorizontalOffset + offset,
                    m_SpaceScrollViewer.VerticalOffset + offset,
                    SpaceZoomFactor, true);
            }
        }
        public void RefreshSpaceGrid()
        {
            List<MouseDetectionRegion> regions = new List<MouseDetectionRegion>();

            m_SpaceCanvas.Children.Clear();
            m_SpaceCanvas.Children.Add(m_GridImage);
            m_SpaceCanvas.Children.Add(m_MouseRectangle);

            foreach (Device d in GlobalDevices)
            {
                m_SpaceCanvas.Children.Add(d.Border);
                SortByZIndex(d.LightZones);

                foreach (var zone in d.LightZones)
                {
                    MouseDetectionRegion r = new MouseDetectionRegion()
                    {
                        RegionIndex = -1,
                        DetectionRect = zone.AbsoluteZoneRect,
                        GroupIndex = d.Type
                    };

                    r.Callback = zone.OnReceiveMouseEvent;

                    regions.Add(r);
                }
            }

            m_MouseEventCtrl.DetectionRegions = regions.ToArray();
            UnselectAllZones();
        }
        public void Reset()
        {
            IntializeMouseEventCtrl();
            GlobalDevices.Clear();
            TempDevices.Clear();
            SetSpaceStatus(SpaceStatus.Normal);
        }
        private void SortByZIndex(LightZone[] zones)
        {
            int count = zones.Length;

            // Bubble sort
            for (int i = 0; i < count - 1; i++)
            {
                for (int j = 0; j < count - 1 - i; j++)
                {
                    if (zones[j].ZIndex < zones[j + 1].ZIndex)
                    {
                        LightZone z = zones[j];
                        zones[j] = zones[j + 1];
                        zones[j + 1] = z;
                    }
                }
            }
        }
        public void MoveDeviceMousePosition(Device device, double offsetX, double offsetY)
        {
            m_MouseEventCtrl.MoveGroupRects(device.Type, offsetX, offsetY);
        }
        public void UnselectAllZones()
        {
            foreach (var d in GlobalDevices)
            {
                foreach (var zone in d.LightZones)
                {
                    zone.ChangeStatus(RegionStatus.Normal);
                }
            }
        }
        public void WatchCurrentLayer()
        {
            SetSpaceStatus(SpaceStatus.WatchingLayer);
            UnselectAllZones();
            DeviceLayer layer = AuraLayerManager.Self.GetSelectedLayer();
            Dictionary<int, int[]> dictionary = layer.GetZoneDictionary();

            // According to the layer, assign selection status for every zone
            foreach (KeyValuePair<int, int[]> pair in dictionary)
            {
                Device d = GetGlobalDeviceByType(pair.Key);
                int[] indexes = pair.Value;

                if (d == null)
                    continue;

                foreach (var zone in d.LightZones)
                {
                    if (Array.Find(indexes, x => x == zone.Index) > 0)
                    {
                        zone.ChangeStatus(RegionStatus.Watching);
                    }
                }
            }
        }
        private void OnNormalState()
        {
            if (AuraLayerManager.Self != null)
            {
                AuraLayerManager.Self.UnselectAllLayers();
                UnselectAllZones();
            }
        }
        public void DeleteOverlappingTempDevice(Device testDev)
        {
            List<Device> _tempDevices = new List<Device>(TempDevices);
            foreach (var d in _tempDevices)
            {
                if (testDev.Equals(d))
                    continue;

                if (ControlHelper.IsOverlapping(testDev.GridPosition.X, testDev.Width, d.GridPosition.X, d.Width) &&
                    ControlHelper.IsOverlapping(testDev.GridPosition.Y, testDev.Height, d.GridPosition.Y, d.Height))
                {
                    TempDevices.Remove(d);
                    AuraLayerManager.Self.ClearDeviceData(d.Type);
                }
            }
        }

        public void ReEdit(DeviceLayer layer)
        {
            SetSpaceStatus(SpaceStatus.ReEditing);
            UnselectAllZones();

            Dictionary<int, int[]> dictionary = layer.GetZoneDictionary();

            // According to the layer, assign selection status for every zone
            foreach (KeyValuePair<int, int[]> pair in dictionary)
            {
                Device d = GetGlobalDeviceByType(pair.Key);
                int[] indexes = pair.Value;

                if (d == null)
                    continue;

                foreach (var zone in d.LightZones)
                {
                    if (Array.Find(indexes, x => x == zone.Index) > 0)
                    {
                        zone.ChangeStatus(RegionStatus.Selected);
                    }
                }
            }
        }

        public void ClearTempDeviceData()
        {
            List<Device> _tempDevices = new List<Device>(TempDevices);
            foreach (var d in _tempDevices)
            {
                TempDevices.Remove(d);
                AuraLayerManager.Self.ClearDeviceData(d.Type);
            }
        }
        public bool IsOverlapping(Device testDev)
        {
            foreach (var d in GlobalDevices)
            {
                if (testDev.Equals(d))
                    continue;

                if (ControlHelper.IsOverlapping(testDev.GridPosition.X, testDev.Width, d.GridPosition.X, d.Width) &&
                    ControlHelper.IsOverlapping(testDev.GridPosition.Y, testDev.Height, d.GridPosition.Y, d.Height))
                {
                    return true;
                }
            }
            return false;
        }
        private Point GetFreeRoomPositionForRect(Rect rect)
        {
            Device overlappingDevice = GetFirstOverlappingDevice(rect);

            if (overlappingDevice == null)
                return new Point(rect.X, rect.Y);
            else
            {
                return GetFreeRoomPositionForRect(new Rect(
                    overlappingDevice.GridPosition.X + overlappingDevice.Width,
                    overlappingDevice.GridPosition.Y,
                    overlappingDevice.Width,
                    overlappingDevice.Height));
            }
        }
        private Device GetFirstOverlappingDevice(Rect gridRect)
        {
            foreach (var d in GlobalDevices)
            {
                if (ControlHelper.IsOverlapping(gridRect.X, gridRect.Width, d.GridPosition.X, d.Width) &&
                    ControlHelper.IsOverlapping(gridRect.Y, gridRect.Height, d.GridPosition.Y, d.Height))
                {
                    return d;
                }
            }
            return null;
        }
        public Device GetGlobalDeviceByType(int type)
        {
            return GlobalDevices.Find(x => x.Type == type);
        }

        #region About ingroup devices
        public async void FillStageWithDevices()
        {
            List<SyncDevice> deviceNodes = ConnectedDevicesDialog.Self.GetIngroupDevices();
            DeviceContent deviceContent;
            Device device;

            foreach (var d in deviceNodes)
            {
                deviceContent = await DeviceContent.GetDeviceContent(d);

                if (deviceContent == null)
                    continue;

                Rect r = new Rect(0, 0, deviceContent.GridWidth, deviceContent.GridHeight);
                Point p = GetFreeRoomPositionForRect(r);
                device = await deviceContent.ToDevice(p);

                GlobalDevices.Add(device);
            }

            RefreshSpaceGrid();
        }
        public async void RefreshStageDevices(List<SyncDevice> ingroupDevices)
        {
            List<string> namesOfGlobalDevices = new List<string>();

            foreach (var d in GlobalDevices)
            {
                namesOfGlobalDevices.Add(d.Name);
            }

            List<SyncDevice> needToAddToStage = new List<SyncDevice>(ingroupDevices);
            foreach (var name in namesOfGlobalDevices)
            {
                needToAddToStage.RemoveAll(x => x.Name == name);
            }

            List<string> needToRemoveFromStage = new List<string>(namesOfGlobalDevices);
            foreach (var d in ingroupDevices)
            {
                needToRemoveFromStage.RemoveAll(x => x == d.Name);
            }

            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                foreach (var name in needToRemoveFromStage)
                {
                    Device device = GlobalDevices.Find(x => x.Name == name);

                    if (device != null)
                    {
                        TempDevices.Add(device);
                        GlobalDevices.Remove(device);
                    }
                }

                foreach (var node in needToAddToStage)
                {
                    DeviceContent deviceContent = await DeviceContent.GetDeviceContent(node);

                    if (deviceContent == null)
                        continue;

                    Rect r = new Rect(0, 0, deviceContent.GridWidth, deviceContent.GridHeight);
                    Point p = GetFreeRoomPositionForRect(r);
                    Device device = await deviceContent.ToDevice(p);

                    GlobalDevices.Add(device);

                    // Delete same type temp data
                    TempDevices.RemoveAll(x => x.Type == device.Type);
                }

                RefreshSpaceGrid();
            });
        }
        #endregion

        #region UI Element
        private void DragDevImgToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            SetSpaceStatus(SpaceStatus.DragingDevice);
        }
        private void DragDevImgToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            SetSpaceStatus(SpaceStatus.Normal);
        }
        public void SpaceZoomChanged(string value)
        {
            switch (value)
            {
                case "33 %":
                    m_SpaceScrollViewer.ChangeView(m_SpaceScrollViewer.HorizontalOffset,
                        m_SpaceScrollViewer.VerticalOffset, 0.5f, true);
                    SpaceZoomFactor = 0.5f;
                    break;
                case "50 %":
                    m_SpaceScrollViewer.ChangeView(m_SpaceScrollViewer.HorizontalOffset,
                        m_SpaceScrollViewer.VerticalOffset, 1f, true);
                    SpaceZoomFactor = 1f;
                    break;
                case "100 %":
                    m_SpaceScrollViewer.ChangeView(m_SpaceScrollViewer.HorizontalOffset,
                        m_SpaceScrollViewer.VerticalOffset, 2, true);
                    SpaceZoomFactor = 2;
                    break;
            }
        }
        #endregion

        #region Mouse Operations
        private void SpaceGrid_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (GetSpaceStatus() == SpaceStatus.WatchingLayer)
            {
                SetSpaceStatus(SpaceStatus.Normal);
            }

            var fe = sender as FrameworkElement;
            Point Position = e.GetCurrentPoint(fe).Position;
            m_MouseEventCtrl.OnMousePressed(Position);
            m_ScrollTimerClock.Start();
        }
        private void SpaceGrid_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (GetSpaceStatus() != SpaceStatus.Normal && GetSpaceStatus() != SpaceStatus.ReEditing)
                return;

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
            CompositeTransform ct = m_MouseRectangle.RenderTransform as CompositeTransform;

            ct.TranslateX = r.X;
            ct.TranslateY = r.Y;
            m_MouseRectangle.Width = r.Width;
            m_MouseRectangle.Height = r.Height;

            Rect screenRect = new Rect(
                    m_SpaceScrollViewer.HorizontalOffset / SpaceZoomFactor,
                    m_SpaceScrollViewer.VerticalOffset / SpaceZoomFactor,
                    m_SpaceScrollViewer.ActualWidth / SpaceZoomFactor,
                    m_SpaceScrollViewer.ActualHeight / SpaceZoomFactor);

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
            m_MouseRectangle.Width = 0;
            m_MouseRectangle.Height = 0;
            m_ScrollTimerClock.Stop();
        }
        private void SpaceGrid_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            UnselectAllZones();
        }
        #endregion

        public XmlNode ToXmlNodeForUserData()
        {
            XmlNode spaceNode = CreateXmlNodeOfFile("space");

            foreach (var d in GlobalDevices)
            {
                spaceNode.AppendChild(d.ToXmlNodeForUserData());
            }

            return spaceNode;
        }
    }
}
