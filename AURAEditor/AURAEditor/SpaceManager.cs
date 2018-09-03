using Windows.UI.Xaml;
using System;
using Windows.UI.Xaml.Controls;
using AuraEditor.Common;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Input;
using Windows.UI.Input;
using Windows.UI.Xaml.Shapes;
using Windows.UI;
using static AuraEditor.Common.ControlHelper;
using static AuraEditor.Common.Definitions;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using Windows.Storage.Streams;
using System.Linq;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace AuraEditor
{
    public enum SpaceStatus
    {
        None = 0,
        Normal = 1,
        DragingDevice = 2,
        WatchingLayer = 3,
        DragingEffectBlock = 4,
    }

    public sealed partial class MainPage : Page
    {
        MouseEventCtrl _mouseEventCtrl;
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
                SpaceAreaCanvas.PointerPressed -= SpaceGrid_PointerPressed;
                SpaceAreaCanvas.PointerMoved -= SpaceGrid_PointerMoved;
                SpaceAreaCanvas.PointerReleased -= SpaceGrid_PointerReleased;
                SpaceAreaCanvas.RightTapped -= SpaceGrid_RightTapped;

                SpaceAreaCanvas.PointerPressed += SpaceGrid_PointerPressed;
                SpaceAreaCanvas.PointerMoved += SpaceGrid_PointerMoved;
                SpaceAreaCanvas.PointerReleased += SpaceGrid_PointerReleased;
                SpaceAreaCanvas.RightTapped += SpaceGrid_RightTapped;
                SetLayerButton.IsEnabled = true;
            }
            else if (value == SpaceStatus.WatchingLayer || value == SpaceStatus.DragingEffectBlock)
            {
                DisableAllDevicesOperation();
                SpaceAreaCanvas.PointerPressed -= SpaceGrid_PointerPressed;
                SpaceAreaCanvas.PointerPressed += SpaceGrid_PointerPressed;

                SpaceAreaCanvas.PointerMoved -= SpaceGrid_PointerMoved;
                SpaceAreaCanvas.PointerReleased -= SpaceGrid_PointerReleased;
                SpaceAreaCanvas.RightTapped -= SpaceGrid_RightTapped;
                SetLayerButton.IsEnabled = false;
            }
            else if (value == SpaceStatus.DragingDevice)
            {
                EableAllDevicesOperation();
                SpaceAreaCanvas.PointerPressed -= SpaceGrid_PointerPressed;
                SpaceAreaCanvas.PointerMoved -= SpaceGrid_PointerMoved;
                SpaceAreaCanvas.PointerReleased -= SpaceGrid_PointerReleased;
                SpaceAreaCanvas.RightTapped -= SpaceGrid_RightTapped;
                SetLayerButton.IsEnabled = true;
            }
        }

        private void IntializeSpaceGrid()
        {
            _mouseEventCtrl = IntializeMouseEventCtrl();
            SetSpaceStatus(SpaceStatus.Normal);

            if (true /* If Aura file does not exist*/)
            {
                FillWithIngroupDevices();
            }
            else if (true /* If Aura file exist*/)
            {
            }
        }
        private MouseEventCtrl IntializeMouseEventCtrl()
        {
            List<MouseDetectionRegion> regions = new List<MouseDetectionRegion>();

            MouseEventCtrl mec = new MouseEventCtrl
            {
                MonitorMaxRect = new Rect(new Point(0, 0), new Point(2000, 1000)),
                DetectionRegions = regions.ToArray()
            };

            return mec;
        }
        public void RefreshSpaceGrid()
        {
            List<Device> devices = _auraCreatorManager.GlobalDevices;
            List<MouseDetectionRegion> regions = new List<MouseDetectionRegion>();

            SpaceAreaCanvas.Children.Clear();
            SpaceAreaCanvas.Children.Add(GridImage);
            SpaceAreaCanvas.Children.Add(mouseRectangle);

            foreach (Device d in devices)
            {
                SpaceAreaCanvas.Children.Add(d.Image);
                SortByZIndex(d.LightZones);

                foreach (var zone in d.LightZones)
                {
                    SpaceAreaCanvas.Children.Add(zone.Frame);

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

            _mouseEventCtrl.DetectionRegions = regions.ToArray();
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

        public void MoveDevicePosition(Device device, double offsetX, double offsetY)
        {
            _mouseEventCtrl.MoveGroupRects(device.Type, offsetX, offsetY);
        }
        private void UnselectAllLayers()
        {
            List<DeviceLayerListViewItem> layers =
                FindAllControl<DeviceLayerListViewItem>(LayerListView, typeof(DeviceLayerListViewItem));

            foreach (var layer in layers)
            {
                layer.IsChecked = false;
            }

            LayerListView.SelectedIndex = -1;
        }
        private void UnselectAllZones()
        {
            foreach (var d in _auraCreatorManager.GlobalDevices)
            {
                foreach (var zone in d.LightZones)
                {
                    Shape shape = zone.Frame;
                    shape.Stroke = new SolidColorBrush(Colors.Black);
                    shape.Fill = new SolidColorBrush(Colors.Transparent);
                }
            }

            _mouseEventCtrl.SetAllRegionsStatus(RegionStatus.Normal);
        }

        public void WatchLayer(DeviceLayer layer)
        {
            SetSpaceStatus(SpaceStatus.WatchingLayer);
            UnselectAllZones();

            Dictionary<int, int[]> dictionary = layer.GetDeviceToZonesDictionary();

            // According to the layer, assign selection status for every zone
            foreach (KeyValuePair<int, int[]> pair in dictionary)
            {
                Device d = _auraCreatorManager.GetGlobalDeviceByType(pair.Key);
                int[] indexes = pair.Value;

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
            UnselectAllLayers();
            UnselectAllZones();
        }

        #region UI Element
        private void SetLayerButton_Click(object sender, RoutedEventArgs e)
        {
            //NamedDialog namedDialog = new NamedDialog();
            List<Device> devices = _auraCreatorManager.GlobalDevices;
            DeviceLayer layer = new DeviceLayer();
            List<int> selectedIndex;

            layer.LayerName = "Layer " + (_auraCreatorManager.GetLayerCount());

            foreach (Device d in devices)
            {
                selectedIndex = new List<int>();

                foreach (var zone in d.LightZones)
                {
                    if (zone.Selected == true)
                    {
                        selectedIndex.Add(zone.Index);
                    }
                }

                layer.AddDeviceZones(d.Type, selectedIndex.ToArray());
            }

            _auraCreatorManager.AddDeviceLayer(layer);
            UnselectAllZones();
        }
        private void DragDevImgToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            SetSpaceStatus(SpaceStatus.DragingDevice);
        }
        private void DragDevImgToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            SetSpaceStatus(SpaceStatus.Normal);
        }
        private void SpaceZoomComboxBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string content = e.AddedItems[0].ToString();
            switch (content)
            {
                case "0 %":
                    SpaceAreaScrollViewer.ChangeView(SpaceAreaScrollViewer.HorizontalOffset,
                        SpaceAreaScrollViewer.VerticalOffset, 1, true);
                    break;
                case "50 %":
                    SpaceAreaScrollViewer.ChangeView(SpaceAreaScrollViewer.HorizontalOffset,
                        SpaceAreaScrollViewer.VerticalOffset, 1.5f, true);
                    break;
                case "100 %":
                    SpaceAreaScrollViewer.ChangeView(SpaceAreaScrollViewer.HorizontalOffset,
                        SpaceAreaScrollViewer.VerticalOffset, 2, true);
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
            _mouseEventCtrl.OnMousePressed(Position);
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
                _mouseEventCtrl.OnMouseMoved(Position, true);
                bool _hasCapture = fe.CapturePointer(e.Pointer);
            }
            else
            {
                _mouseEventCtrl.OnMouseMoved(Position, false);
                return; // no need to draw mouse rectangle
            }

            // Draw mouse rectangle
            Rect r = _mouseEventCtrl.MouseRect;
            CompositeTransform ct = mouseRectangle.RenderTransform as CompositeTransform;

            ct.TranslateX = r.X;
            ct.TranslateY = r.Y;
            mouseRectangle.Width = r.Width;
            mouseRectangle.Height = r.Height;
        }
        private void SpaceGrid_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            Rect r = _mouseEventCtrl.MouseRect;
            _mouseEventCtrl.OnMouseReleased();
            mouseRectangle.Width = 0;
            mouseRectangle.Height = 0;
        }
        private void SpaceGrid_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            _mouseEventCtrl.OnRightTapped();
        }
        #endregion

        #region Ingroup devices
        private async void FillWithIngroupDevices()
        {
            List<string> namesOfIngroupDevices = await GetIngroupDevices();

            DeviceContent deviceContent;
            Device device;

            for (int i = 0; i < namesOfIngroupDevices.Count; i++)
            {
                deviceContent = await DeviceContent.GetDeviceContent(namesOfIngroupDevices[i]);

                if (deviceContent == null)
                    continue;

                if (i == 0) // local
                    device = deviceContent.ToDevice();
                else // other
                {
                    Rect r = new Rect(0, 0, deviceContent.UI_Width, deviceContent.UI_Height);
                    Point p = GetFreeRoomPositionForRect(r);
                    device = deviceContent.ToDevice(p);
                }

                _auraCreatorManager.GlobalDevices.Add(device);
            }

            // For developing
            /*
            deviceContent = await GetDeviceContent("GLADIUS II");
            device = CreateDeviceFromContent(deviceContent);
            _auraCreatorManager.GlobalDevices.Add(device);
            */

            RefreshSpaceGrid();
        }
        private async Task<List<string>> GetIngroupDevices()
        {
            XmlDocument xmlDoc = await GetIngroupDevicesXmlDoc();

            List<string> results = new List<string>();
            string localDevice = GetLocalDevice(xmlDoc);
            List<string> otherDevices = GetOtherDevice(xmlDoc);

            // Put local at first
            results.Add(localDevice);
            results.AddRange(otherDevices);

            return results;
        }
        private async Task<XmlDocument> GetIngroupDevicesXmlDoc()
        {
            StorageFolder folder = await StorageFolder.GetFolderFromPathAsync("C:\\ProgramData\\ASUS");
            folder = await EnterOrCreateFolder(folder, "AURA Creator");
            folder = await EnterOrCreateFolder(folder, "Devices");
            StorageFile sf = await folder.GetFileAsync("ingroupdevice.xml");

            XmlDocument devicesXml = new XmlDocument();
            devicesXml.Load(await sf.OpenStreamForReadAsync());

            return devicesXml;
        }
        private string GetLocalDevice(XmlDocument devicesXml)
        {
            XmlNode ingroupdevice = devicesXml.SelectSingleNode("ingroupdevice");
            XmlNodeList deviceNodes = ingroupdevice.SelectNodes("device");
            string result = "";

            foreach (XmlNode node in deviceNodes)
            {
                XmlElement element = (XmlElement)node;

                if (element.GetAttribute("local") == "1")
                {
                    return element.GetAttribute("name");
                }
            }

            return result;
        }
        private List<string> GetOtherDevice(XmlDocument devicesXml)
        {
            XmlNode ingroupdevice = devicesXml.SelectSingleNode("ingroupdevice");
            XmlNodeList deviceNodes = ingroupdevice.SelectNodes("device");
            List<string> results = new List<string>();

            foreach (XmlNode node in deviceNodes)
            {
                XmlElement element = (XmlElement)node;

                if (element.GetAttribute("local") != "1")
                {
                    results.Add(element.GetAttribute("name"));
                }
            }

            return results;
        }
        private async void RescanIngroupDevices()
        {
            List<string> namesOfIngroupDevices = await GetIngroupDevices();

            List<Device> globalDevices = _auraCreatorManager.GlobalDevices;
            List<string> namesOfGlobalDevices = new List<string>();

            foreach (var d in globalDevices)
            {
                namesOfGlobalDevices.Add(d.Name);
            }

            var needToAdd = namesOfIngroupDevices.Except(namesOfGlobalDevices);
            var needToRemove = namesOfGlobalDevices.Except(namesOfIngroupDevices);

            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                foreach (var d_name in needToRemove)
                {
                    Device device = _auraCreatorManager.GlobalDevices.Find(x => x.Name == d_name);

                    if (device != null)
                    {
                        _auraCreatorManager.GlobalDevices.Remove(device);
                    }
                }

                foreach (var d_name in needToAdd)
                {
                    DeviceContent deviceContent = await DeviceContent.GetDeviceContent(d_name);

                    if (deviceContent == null)
                        continue;

                    Rect r = new Rect(0, 0, deviceContent.UI_Width, deviceContent.UI_Height);
                    Point p = GetFreeRoomPositionForRect(r);
                    Device device = deviceContent.ToDevice(p);

                    _auraCreatorManager.GlobalDevices.Add(device);
                }

                RefreshSpaceGrid();
            });
        }
        #endregion

        private void EableAllDevicesOperation()
        {
            foreach (var d in _auraCreatorManager.GlobalDevices)
            {
                d.EnableManipulation();
            }
        }
        private void DisableAllDevicesOperation()
        {
            foreach (var d in _auraCreatorManager.GlobalDevices)
            {
                d.DisableManipulation();
            }
        }
        public bool IsOverlapping(Device testDev)
        {
            foreach (var d in _auraCreatorManager.GlobalDevices)
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
            foreach (var d in _auraCreatorManager.GlobalDevices)
            {
                if (ControlHelper.IsOverlapping(gridRect.X, gridRect.Width, d.GridPosition.X, d.Width) &&
                    ControlHelper.IsOverlapping(gridRect.Y, gridRect.Height, d.GridPosition.Y, d.Height))
                {
                    return d;
                }
            }
            return null;
        }
    }
}