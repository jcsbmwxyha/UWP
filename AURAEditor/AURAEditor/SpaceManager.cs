﻿using Windows.UI.Xaml;
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
    public sealed partial class MainPage : Page
    {
        MouseEventCtrl _mouseEventCtrl;
        public bool DragingDeviceImage {
            set
            {
                if (value)
                {
                    SpacePanel.PointerPressed -= Image_PointerPressed;
                    SpacePanel.PointerMoved -= Image_PointerMoved;
                    SpacePanel.PointerReleased -= Image_PointerReleased;
                    SpacePanel.RightTapped -= Image_RightTapped;
                }
                else
                {
                    SpacePanel.PointerPressed += Image_PointerPressed;
                    SpacePanel.PointerMoved += Image_PointerMoved;
                    SpacePanel.PointerReleased += Image_PointerReleased;
                    SpacePanel.RightTapped += Image_RightTapped;
                }
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
            SpacePanel.Children.Clear();
            SpacePanel.Children.Add(GridImage);
            List<Device> devices = _deviceGroupManager.GlobalDevices;
            List<MouseDetectionRegion> regions = new List<MouseDetectionRegion>();
            
            foreach (Device d in devices)
            {
                SpacePanel.Children.Add(d.DeviceImg);

                foreach (var zone in d.LightZones)
                {
                    SpacePanel.Children.Add(zone.Frame);

                    MouseDetectionRegion r = new MouseDetectionRegion()
                    {
                        Index = -1,
                        DetectionRect = zone.AbsoluteZoneRect,
                        Hover = false,
                        Selected = zone.Selected,
                        Callback = zone.Frame_StatusChanged
                    };
                    regions.Add(r);
                }
            }

            _mouseEventCtrl.DetectionRegions = regions.ToArray();
        }
        public void UpdateSpaceGrid(DeviceGroup dg)
        {
            List<Device> devices = _deviceGroupManager.GlobalDevices;
            Dictionary<int, int[]> dictionary = dg.GetDeviceToZonesDictionary();
            List<MouseDetectionRegion> regions = new List<MouseDetectionRegion>();

            // 1. Reset all zones
            foreach (var d in _deviceGroupManager.GlobalDevices)
            {
                foreach (var zone in d.LightZones)
                {
                    Shape shape = zone.Frame;
                    shape.Stroke = new SolidColorBrush(Colors.Black);
                    shape.Fill = new SolidColorBrush(Colors.Transparent);
                    zone.Selected = false;
                }
            }

            // 2. According to the dg, assign selection status for every zone
            foreach (KeyValuePair<int, int[]> pair in dictionary)
            {
                Device d = _deviceGroupManager.GetGlobalDevice(pair.Key);
                int[] phyIndexes = pair.Value;

                foreach (var zone in d.LightZones)
                {
                    Shape shape = zone.Frame;

                    if (Array.Find(phyIndexes, x => x == zone.PhysicalIndex) > 0)
                    {
                        shape.Stroke = new SolidColorBrush(Colors.Yellow);
                        shape.Fill = new SolidColorBrush(Colors.Transparent);
                        zone.Selected = true;
                    }
                }
            }

            // 3. Update mouse regions
            foreach (Device d in devices)
            {
                foreach (var zone in d.LightZones)
                {
                    MouseDetectionRegion r = new MouseDetectionRegion()
                    {
                        Index = -1,
                        DetectionRect = zone.AbsoluteZoneRect,
                        Hover = false,
                        Selected = zone.Selected,
                        Callback = zone.Frame_StatusChanged
                    };
                    regions.Add(r);
                }
            }

            _mouseEventCtrl.DetectionRegions = regions.ToArray();
        }
        public void UpdateDeviceZoneRegions()
        {
            List<Device> devices = _deviceGroupManager.GlobalDevices;
            List<MouseDetectionRegion> regions = new List<MouseDetectionRegion>();

            foreach (Device d in devices)
            {
                foreach (var zone in d.LightZones)
                {
                    MouseDetectionRegion r = new MouseDetectionRegion()
                    {
                        Index = -1,
                        DetectionRect = zone.AbsoluteZoneRect,
                        Hover = false,
                        Selected = zone.Selected,
                        Callback = zone.Frame_StatusChanged
                    };
                    regions.Add(r);
                }
            }

            _mouseEventCtrl.DetectionRegions = regions.ToArray();
        }

        private async void SetLayerButton_Click(object sender, RoutedEventArgs e)
        {
            NamedDialog namedDialog = new NamedDialog(null);
            List<Device> devices = _deviceGroupManager.GlobalDevices;
            DeviceGroup dg = new DeviceGroup();
            dg.GroupName = namedDialog.CustomizeName;
            List<int> selectedIndex;

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

                dg.AddDeviceZones(d.DeviceType, selectedIndex.ToArray());
            }
            
            var result = await namedDialog.ShowAsync();
            if (result == ContentDialogResult.None)
                return;

            dg.GroupName = namedDialog.CustomizeName;
            if (dg.GroupName == "")
                return;
            
            _deviceGroupManager.AddDeviceGroup(dg);
            GroupListView.SelectedIndex = 0;
        }

        private void Image_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var fe = sender as FrameworkElement;
            Point Position = e.GetCurrentPoint(fe).Position;
            _mouseEventCtrl.OnMousePressed(Position);
            bool _hasCapture = fe.CapturePointer(e.Pointer);
        }
        private void Image_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
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

            ct.TranslateX = r.X - SpaceAreaScrollViewer.HorizontalOffset;
            ct.TranslateY = r.Y - SpaceAreaScrollViewer.VerticalOffset;
            mouseRectangle.Width = r.Width;
            mouseRectangle.Height = r.Height;
        }
        private void Image_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            Rect r = _mouseEventCtrl.MouseRect;
            _mouseEventCtrl.OnMouseReleased();
            mouseRectangle.Width = 0;
            mouseRectangle.Height = 0;
        }
        private void Image_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            _mouseEventCtrl.OnRightTapped();
        }

    }
}