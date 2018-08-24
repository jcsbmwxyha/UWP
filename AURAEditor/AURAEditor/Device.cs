#define DEBUG
using System.Collections.Generic;
using Windows.UI.Xaml;
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Input;
using static AuraEditor.MainPage;
using static AuraEditor.Common.ControlHelper;
using static AuraEditor.Common.Definitions;
using Windows.Foundation;

namespace AuraEditor
{
    public class Device
    {
        public string Name { get; set; }
        public int Type { get; set; }
        public Point GridPosition {
            get
            {
                CompositeTransform ct = Image.RenderTransform as CompositeTransform;

                return new Point(
                    ct.TranslateX / GridWidthPixels,
                    ct.TranslateY / GridWidthPixels);
            }
            set
            {
                SetImagePixelPosition(
                    value.X * GridWidthPixels,
                    value.Y * GridWidthPixels);
            }
        }
        private Point _oldPixelPosition;
        public double Width { get; set; }
        public double Height { get; set; }
        public Image Image { get; set; }
        public LightZone[] LightZones { get; set; }

        public Device(Image img)
        {
            Image = img;
            Image.Tapped += DeviceImg_Tapped;
        }
        public void EnableManipulation()
        {
            Image.ManipulationStarted -= ImageManipulationStarted;
            Image.ManipulationDelta -= ImageManipulationDelta;
            Image.ManipulationCompleted -= ImageManipulationCompleted;
            Image.PointerEntered -= ImagePointerEntered;
            Image.PointerExited -= ImagePointerExited;

            Image.ManipulationStarted += ImageManipulationStarted;
            Image.ManipulationDelta += ImageManipulationDelta;
            Image.ManipulationCompleted += ImageManipulationCompleted;
            Image.PointerEntered += ImagePointerEntered;
            Image.PointerExited += ImagePointerExited;
        }
        public void DisableManipulation()
        {
            Image.ManipulationStarted -= ImageManipulationStarted;
            Image.ManipulationDelta -= ImageManipulationDelta;
            Image.ManipulationCompleted -= ImageManipulationCompleted;
            Image.PointerEntered -= ImagePointerEntered;
            Image.PointerExited -= ImagePointerExited;
        }

        #region Mouse event
        private void DeviceImg_Tapped(object sender, TappedRoutedEventArgs e)
        {
            MainPageInstance.SetSpaceStatus(SpaceStatus.Normal);
        }
        private void ImageManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            CompositeTransform ct = Image.RenderTransform as CompositeTransform;

            _oldPixelPosition = new Point(ct.TranslateX, ct.TranslateY);
            Image.Opacity = 0.5;
        }
        private void ImageManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            CompositeTransform ct = Image.RenderTransform as CompositeTransform;

            SetImagePixelPosition(
                ct.TranslateX + e.Delta.Translation.X,
                ct.TranslateY + e.Delta.Translation.Y);
        }
        private void ImageManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            CompositeTransform ct = Image.RenderTransform as CompositeTransform;

            SetImagePixelPosition(
                RoundToGrid(ct.TranslateX),
                RoundToGrid(ct.TranslateY));
            Image.Opacity = 1;

            if (!MainPageInstance.IsOverlapping(this))
            {
                MainPageInstance.MoveDevicePosition(this,
                    RoundToGrid(ct.TranslateX - _oldPixelPosition.X),
                    RoundToGrid(ct.TranslateY - _oldPixelPosition.Y));
            }
            else
            {
                SetImagePixelPosition(_oldPixelPosition.X, _oldPixelPosition.Y);
            }
        }
        private void ImagePointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor 
                = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.SizeAll, 0);
        }
        private void ImagePointerExited(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor 
                = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
        }
        #endregion

        private void SetImagePixelPosition(double x, double y)
        {
            CompositeTransform ct = Image.RenderTransform as CompositeTransform;
            CompositeTransform zone_ct;

            ct.TranslateX = x;
            ct.TranslateY = y;

            foreach (var zone in LightZones)
            {
                zone_ct = zone.Frame.RenderTransform as CompositeTransform;
                zone_ct.TranslateX = ct.TranslateX + zone.RelativeZoneRect.Left;
                zone_ct.TranslateY = ct.TranslateY + zone.RelativeZoneRect.Top;
            }
        }
    }
}