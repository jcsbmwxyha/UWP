#define DEBUG
using System.Collections.Generic;
using Windows.UI.Xaml;
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Input;
using Constants = AuraEditor.Common.Constants;
using static AuraEditor.MainPage;

namespace AuraEditor
{
    public class Device
    {
        public string DeviceName { get; set; }
        public int DeviceType { get; set; }
        private double _oldX;
        public double X
        {
            get
            {
                CompositeTransform ct = DeviceImg.RenderTransform as CompositeTransform;
                return ct.TranslateX / Constants.GridLength;
            }
            set
            {
                CompositeTransform ct = DeviceImg.RenderTransform as CompositeTransform;
                ct.TranslateX = value * Constants.GridLength;
            }
        }
        private double _oldY;
        public double Y
        {
            get
            {
                CompositeTransform ct = DeviceImg.RenderTransform as CompositeTransform;
                return ct.TranslateY / Constants.GridLength;
            }
            set
            {
                CompositeTransform ct = DeviceImg.RenderTransform as CompositeTransform;
                ct.TranslateY = value * Constants.GridLength;
            }
        }
        public double W { get; set; }
        public double H { get; set; }
        public Image DeviceImg { get; set; }
        public LightZone[] LightZones { get; set; }

        public Device(Image img)
        {
            DeviceImg = img;

            DeviceImg.PointerPressed += Image_PointerPressed;
            DeviceImg.PointerReleased += Image_PointerReleased;
            DeviceImg.Tapped += DeviceImg_Tapped;
        }
        public void EnableManipulation()
        {
            DeviceImg.ManipulationStarted -= ImageManipulationStarted;
            DeviceImg.ManipulationDelta -= ImageManipulationDelta;
            DeviceImg.ManipulationCompleted -= ImageManipulationCompleted;
            DeviceImg.PointerEntered -= ImagePointerEntered;
            DeviceImg.PointerExited -= ImagePointerExited;

            DeviceImg.ManipulationStarted += ImageManipulationStarted;
            DeviceImg.ManipulationDelta += ImageManipulationDelta;
            DeviceImg.ManipulationCompleted += ImageManipulationCompleted;
            DeviceImg.PointerEntered += ImagePointerEntered;
            DeviceImg.PointerExited += ImagePointerExited;
        }
        public void DisableManipulation()
        {
            DeviceImg.ManipulationStarted -= ImageManipulationStarted;
            DeviceImg.ManipulationDelta -= ImageManipulationDelta;
            DeviceImg.ManipulationCompleted -= ImageManipulationCompleted;
            DeviceImg.PointerEntered -= ImagePointerEntered;
            DeviceImg.PointerExited -= ImagePointerExited;
        }

        private void DeviceImg_Tapped(object sender, TappedRoutedEventArgs e)
        {
            MainPageInstance.SetSpaceStatus(SpaceStatus.Normal);
        }
        private void Image_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            //MainPageInstance.UpdateSpaceGridOperations(SpaceStatus.DragingDevice);
        }
        private void Image_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            //MainPageInstance.UpdateSpaceGridOperations(SpaceStatus.Normal);
        }
        private void ImageManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            var fe = sender as FrameworkElement;
            fe.Opacity = 0.5;

            _oldX = X;
            _oldY = Y;
        }
        private void ImageManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            Image img = sender as Image;
            CompositeTransform ct = img.RenderTransform as CompositeTransform;
            ct.TranslateX += e.Delta.Translation.X;
            ct.TranslateY += e.Delta.Translation.Y;

            foreach(var zone in LightZones)
            {
                ct = zone.Frame.RenderTransform as CompositeTransform;

                ct.TranslateX += e.Delta.Translation.X;
                ct.TranslateY += e.Delta.Translation.Y;
            }
        }
        private void ImageManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            Image img = sender as Image;
            CompositeTransform ct = img.RenderTransform as CompositeTransform;
            CompositeTransform zone_ct;

            // TODO : ++ functionalized  this part
            img.Opacity = 1;
            ct.TranslateX = (int)ct.TranslateX / Constants.GridLength * Constants.GridLength;
            ct.TranslateY = (int)ct.TranslateY / Constants.GridLength * Constants.GridLength;

            foreach (var zone in LightZones)
            {
                zone_ct = zone.Frame.RenderTransform as CompositeTransform;
                zone_ct.TranslateX = (int)ct.TranslateX + zone.RelativeZoneRect.Left;
                zone_ct.TranslateY = (int)ct.TranslateY + zone.RelativeZoneRect.Top;
            }
            // TODO : --

            MainPageInstance.SetDevicePosition(this,
                (int)(X - _oldX) * Constants.GridLength,
                (int)(Y - _oldY) * Constants.GridLength);
        }
        private void ImagePointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor 
                = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.SizeAll, 0);

            //MainPageInstance.UpdateSpaceGridStatus(SpaceStatus.DragingDevice);
        }
        private void ImagePointerExited(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor 
                = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
            
            //MainPageInstance.UpdateSpaceGridStatus(SpaceStatus.Normal);
        }
    }
}