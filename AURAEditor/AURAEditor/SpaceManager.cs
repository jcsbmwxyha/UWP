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

namespace AuraEditor
{
    public enum SpaceStatus
    {
        None = 0,
        Normal = 1,
        DragingDevice = 2,
        WatchingLayer = 3,
        DragingEffectListItem = 4,
    }

    public sealed partial class MainPage : Page
    {
        MouseEventCtrl _mouseEventCtrl;

        private SpaceStatus spaceGridStatus;
        public SpaceStatus GetSpaceGridCurrentStatus()
        {
            return spaceGridStatus;
        }
        public void UpdateSpaceGridOperations(SpaceStatus value)
        {
            if (value == spaceGridStatus)
                return;

            if(spaceGridStatus==SpaceStatus.WatchingLayer)
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
            else if (value == SpaceStatus.WatchingLayer || value == SpaceStatus.DragingEffectListItem)
            {
                DisableAllDevicesOperation();
                SpaceAreaCanvas.PointerPressed -= SpaceGrid_PointerPressed;
                SpaceAreaCanvas.PointerPressed += SpaceGrid_PointerPressed;

                SpaceAreaCanvas.PointerMoved -= SpaceGrid_PointerMoved;
                SpaceAreaCanvas.PointerReleased -= SpaceGrid_PointerReleased;
                SpaceAreaCanvas.RightTapped -= SpaceGrid_RightTapped;
                SetLayerButton.IsEnabled = false;
            }
            else if (value == SpaceStatus.DragingDevice )
            {
                EableAllDevicesOperation();
                SpaceAreaCanvas.PointerPressed -= SpaceGrid_PointerPressed;
                SpaceAreaCanvas.PointerMoved -= SpaceGrid_PointerMoved;
                SpaceAreaCanvas.PointerReleased -= SpaceGrid_PointerReleased;
                SpaceAreaCanvas.RightTapped -= SpaceGrid_RightTapped;
                SetLayerButton.IsEnabled = true;
            }
        }

        private void OnLeavingWatchingLayerMode()
        {
            UnselectAllLayers();
            ResetAllZones();
        }

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
        public void UpdateSpaceGrid()
        {
            SpaceAreaCanvas.Children.Clear();
            SpaceAreaCanvas.Children.Add(GridImage);
            SpaceAreaCanvas.Children.Add(mouseRectangle);
            List<Device> devices = _auraCreatorManager.GlobalDevices;
            List<MouseDetectionRegion> regions = new List<MouseDetectionRegion>();
            
            foreach (Device d in devices)
            {
                SpaceAreaCanvas.Children.Add(d.DeviceImg);

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
                        GroupIndex = d.DeviceType
                    };
                    regions.Add(r);
                }
            }

            _mouseEventCtrl.DetectionRegions = regions.ToArray();
        }
        public void WatchLayer(DeviceLayer layer)
        {
            UpdateSpaceGridOperations(SpaceStatus.WatchingLayer);
            ResetAllZones();

            if (layer is TriggerDeviceLayer)
                WatchTriggerLayer(layer);
            else
                WatchNormalLayer(layer);
        }
        private void WatchNormalLayer(DeviceLayer layer)
        {
            Dictionary<int, int[]> dictionary = layer.GetDeviceToZonesDictionary();

            // According to the layer, assign selection status for every zone
            foreach (KeyValuePair<int, int[]> pair in dictionary)
            {
                Device d = _auraCreatorManager.GetGlobalDevice(pair.Key);
                int[] phyIndexes = pair.Value;

                foreach (var zone in d.LightZones)
                {
                    Shape shape = zone.Frame;

                    if (Array.Find(phyIndexes, x => x == zone.PhysicalIndex) > 0)
                    {
                        shape.Stroke = new SolidColorBrush(Colors.Yellow);
                        shape.Fill = new SolidColorBrush(Colors.Transparent);
                    }
                }
            }
        }
        private void WatchTriggerLayer(DeviceLayer layer)
        {
            List<Device> devices = _auraCreatorManager.GlobalDevices;

            foreach (var device in devices)
            {
                LightZone[] zones = device.LightZones;

                foreach (var zone in zones)
                {
                    Shape shape = zone.Frame;

                    shape.Stroke = new SolidColorBrush(Colors.Yellow);
                    shape.Fill = new SolidColorBrush(Colors.Transparent);
                }
            }
        }

        public void UpdateDevicePosition(Device device, int offsetX, int offsetY)
        {
            _mouseEventCtrl.UpdateGroupRects(device.DeviceType, offsetX, offsetY);
        }
        private void UnselectAllLayers()
        {
            LayerSelected = false;
            List<DeviceLayerListViewItem> items =
                Common.ControlHelper.FindAllControl<DeviceLayerListViewItem>(LayerListView, typeof(DeviceLayerListViewItem));

            foreach (var item in items)
            {
                item.IsChecked = false;
            }

            LayerListView.SelectedIndex = -1;
        }
        private void ResetAllZones()
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
                        selectedIndex.Add(zone.PhysicalIndex);
                    }
                }

                layer.AddDeviceZones(d.DeviceType, selectedIndex.ToArray());
            }
            
            _auraCreatorManager.AddDeviceLayer(layer);
            ResetAllZones();
        }

        private void SpaceGrid_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (GetSpaceGridCurrentStatus() == SpaceStatus.WatchingLayer)
            {
                UpdateSpaceGridOperations(SpaceStatus.Normal);
            }

            var fe = sender as FrameworkElement;
            Point Position = e.GetCurrentPoint(fe).Position;
            _mouseEventCtrl.OnMousePressed(Position);
            bool _hasCapture = fe.CapturePointer(e.Pointer);
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


        private void DragToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            UpdateSpaceGridOperations(SpaceStatus.DragingDevice);
        }

        private void DragToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateSpaceGridOperations(SpaceStatus.Normal);
        }
    }
}