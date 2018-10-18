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
        }
        private SpaceStatus spaceGridStatus;
        public SpaceStatus GetSpaceGridCurrentStatus()
        {
            return spaceGridStatus;
        }
        public void SetSpaceStatus(SpaceStatus value)
        {
            if (value == spaceGridStatus)
                return;

            if (spaceGridStatus == SpaceStatus.WatchingLayer)
            {
                OnLeavingWatchingLayerMode();
            }

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
            }
            else if (value == SpaceStatus.WatchingLayer || value == SpaceStatus.DragingEffectBlock)
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
            FillWithIngroupDevices();
            SetSpaceStatus(SpaceStatus.Normal);
        }
        private MouseEventCtrl IntializeMouseEventCtrl()
        {
            List<MouseDetectionRegion> regions = new List<MouseDetectionRegion>();

            MouseEventCtrl mec = new MouseEventCtrl
            {
                MonitorMaxRect = new Rect(new Point(0, 0), new Point(2700, 1500)),//new Point(m_SpaceCanvas.ActualWidth, m_SpaceCanvas.ActualHeight)),
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
            else if (_mouseDirection == 5) {}
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
                        Hover = false,
                        Selected = zone.Selected,
                        Callback = zone.Frame_StatusChanged,
                        GroupIndex = d.Type
                    };
                    regions.Add(r);
                }
            }

            m_MouseEventCtrl.DetectionRegions = regions.ToArray();
        }
        public void Reset()
        {
            IntializeMouseEventCtrl();
            GlobalDevices.Clear();
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
                    Shape shape = zone.Frame;
                    shape.Stroke = new SolidColorBrush(Colors.Black);
                    shape.Fill = new SolidColorBrush(Colors.Transparent);
                }
            }

            m_MouseEventCtrl.SetAllRegionsStatus(RegionStatus.Normal);
        }
        public void WatchZonesOfLayer(DeviceLayer layer)
        {
            SetSpaceStatus(SpaceStatus.WatchingLayer);
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
                    Shape shape = zone.Frame;

                    if (Array.Find(indexes, x => x == zone.Index) > 0)
                    {
                        shape.Stroke = new SolidColorBrush(Colors.Yellow);
                        shape.Fill = new SolidColorBrush(Colors.Transparent);
                    }
                }
            }
        }
        private void OnLeavingWatchingLayerMode()
        {
            AuraLayerManager.Self.UnselectAllLayers();
            UnselectAllZones();
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

        #region Ingroup devices
        public async void FillWithIngroupDevices()
        {
            List<XmlNode> namesOfIngroupDevices = await GetIngroupDevices();

            DeviceContent deviceContent;
            Device device;

            foreach (var node in namesOfIngroupDevices)
            {
                deviceContent = await DeviceContent.GetDeviceContent(node);

                if (deviceContent == null)
                    continue;

                Rect r = new Rect(0, 0, deviceContent.GridWidth, deviceContent.GridHeight);
                Point p = GetFreeRoomPositionForRect(r);
                device = deviceContent.ToDevice(p);

                GlobalDevices.Add(device);
            }

            RefreshSpaceGrid();
        }
        private async Task<List<XmlNode>> GetIngroupDevices()
        {
            XmlDocument xmlDoc = await GetIngroupDevicesXmlDoc();

            List<XmlNode> results = new List<XmlNode>();
            XmlNode localDevice = GetLocalDevice(xmlDoc);
            List<XmlNode> otherDevices = GetOtherDevice(xmlDoc);

            // Put local at first
            results.Add(localDevice);
            results.AddRange(otherDevices);

            return results;
        }
        private async Task<XmlDocument> GetIngroupDevicesXmlDoc()
        {
            StorageFile sf;
            XmlDocument devicesXml = new XmlDocument(); ;

            try
            {
                sf = await StorageFile.GetFileFromPathAsync("C:\\ProgramData\\ASUS\\AURA Creator\\Devices\\ingroupdevice.xml");
                devicesXml.Load(await sf.OpenStreamForReadAsync());
                return devicesXml;
            }
            catch
            {
                StorageFolder folder = await StorageFolder.GetFolderFromPathAsync("C:\\ProgramData\\ASUS");
                folder = await EnterOrCreateFolder(folder, "AURA Creator");
                folder = await EnterOrCreateFolder(folder, "Devices");
                sf = await folder.GetFileAsync("ingroupdevice.xml");

                devicesXml.Load(await sf.OpenStreamForReadAsync());
                return devicesXml;
            }
        }
        private XmlNode GetLocalDevice(XmlDocument devicesXml)
        {
            XmlNode ingroupdevice = devicesXml.SelectSingleNode("ingroupdevice");
            XmlNodeList deviceNodes = ingroupdevice.SelectNodes("device");

            foreach (XmlNode node in deviceNodes)
            {
                XmlElement element = (XmlElement)node;

                if (element.GetAttribute("type") == "Aac_NBDT")
                {
                    return node;
                }
            }

            return null;
        }
        private List<XmlNode> GetOtherDevice(XmlDocument devicesXml)
        {
            XmlNode ingroupdevice = devicesXml.SelectSingleNode("ingroupdevice");
            XmlNodeList deviceNodes = ingroupdevice.SelectNodes("device");
            List<XmlNode> others = new List<XmlNode>();

            foreach (XmlNode node in deviceNodes)
            {
                XmlElement element = (XmlElement)node;

                if (element.GetAttribute("type") != "Aac_NBDT")
                {
                    others.Add(node);
                }
            }

            return FilterOtherDeviceNodes(others);
        }

        private List<XmlNode> FilterOtherDeviceNodes(List<XmlNode> others)
        {
            List<XmlNode> results = new List<XmlNode>();

            // 1. Determine temp data keep or not
            List<Device> _tempDevices = new List<Device>(TempDevices);
            foreach (Device temp_d in _tempDevices)
            {
                XmlNode node = others.Find(x => (x as XmlElement).GetAttribute("name") == temp_d.Name);

                // temp in ingroups?
                if (node != null)
                {
                    // kick others
                    results.Add(node);
                    others.RemoveAll(x => (x as XmlElement).GetAttribute("type") == GetTypeNameByType(temp_d.Type));
                    TempDevices.Remove(temp_d);
                    GlobalDevices.Add(temp_d);
                }
                else
                {
                    if (others.Find(x => (x as XmlElement).GetAttribute("type") == GetTypeNameByType(temp_d.Type)) != null)
                    {
                        // Because new device will replace temp device
                        TempDevices.Remove(temp_d);
                        AuraLayerManager.Self.ClearDeviceData(temp_d.Type);
                        // delete temp data
                    }
                }
            }

            // 2. Keep device nodes which are plugging, and kick others
            foreach (Device global_d in GlobalDevices)
            {
                XmlNode node = others.Find(x => (x as XmlElement).GetAttribute("name") == global_d.Name);

                if (node != null)
                {
                    // kick others
                    results.Add(node);
                    others.RemoveAll(x => (x as XmlElement).GetAttribute("type") == GetTypeNameByType(global_d.Type));
                }
            }

            // 3. Only retain one node for every type
            for (int i = 0; i < others.Count; i++)
            {
                XmlElement elem = (XmlElement)others[i];
                bool CanAddThisNode = true;

                for (int j = 0; j < i; j++)
                {
                    XmlElement elem2 = (XmlElement)others[j];

                    if (elem.GetAttribute("type") == elem2.GetAttribute("type"))
                        CanAddThisNode = false;
                }

                if (CanAddThisNode)
                    results.Add(others[i]);
            }

            return results;
        }

        public async void RescanIngroupDevices()
        {
            List<XmlNode> ingroupDeviceNodes = await GetIngroupDevices();
            List<string> namesOfGlobalDevices = new List<string>();

            foreach (var d in GlobalDevices)
            {
                namesOfGlobalDevices.Add(d.Name);
            }

            List<XmlNode> needToAdd = new List<XmlNode>(ingroupDeviceNodes);
            foreach(var name in namesOfGlobalDevices)
            {
                needToAdd.RemoveAll(x => (x as XmlElement).GetAttribute("name") == name);
            }

            List<string> needToRemove = new List<string>(namesOfGlobalDevices);
            foreach (var node in ingroupDeviceNodes)
            {
                needToRemove.RemoveAll(x => x == (node as XmlElement).GetAttribute("name"));
            }

            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                foreach (var name in needToRemove)
                {
                    Device device = GlobalDevices.Find(x => x.Name == name);

                    if (device != null)
                    {
                        TempDevices.Add(device);
                        GlobalDevices.Remove(device);
                    }
                }

                foreach (var node in needToAdd)
                {
                    DeviceContent deviceContent = await DeviceContent.GetDeviceContent(node);

                    if (deviceContent == null)
                        continue;

                    Rect r = new Rect(0, 0, deviceContent.GridWidth, deviceContent.GridHeight);
                    Point p = GetFreeRoomPositionForRect(r);
                    Device device = deviceContent.ToDevice(p);

                    GlobalDevices.Add(device);
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
                case "0 %":
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
            if (GetSpaceGridCurrentStatus() == SpaceStatus.WatchingLayer)
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
            if (GetSpaceGridCurrentStatus() != SpaceStatus.Normal)
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
            m_MouseEventCtrl.OnRightTapped();
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
