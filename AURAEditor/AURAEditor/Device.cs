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
using static AuraEditor.Common.LuaHelper;
using Windows.Foundation;
using MoonSharp.Interpreter;

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
                    ct.TranslateX / GridPixels,
                    ct.TranslateY / GridPixels);
            }
            set
            {
                SetPosition(
                    value.X * GridPixels,
                    value.Y * GridPixels);
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

            SetPosition(
                ct.TranslateX + e.Delta.Translation.X,
                ct.TranslateY + e.Delta.Translation.Y);
        }
        private void ImageManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            CompositeTransform ct = Image.RenderTransform as CompositeTransform;

            SetPosition(
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
                SetPositionByAnimation(_oldPixelPosition.X, _oldPixelPosition.Y);
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

        private void SetPosition(double x, double y)
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
        private void SetPositionByAnimation(double x, double y)
        {
            CompositeTransform ct = Image.RenderTransform as CompositeTransform;
            CompositeTransform zone_ct;
            double runTime = 300;
            double source;
            double targetX;
            double targetY;

            source = ct.TranslateX;
            targetX = x;
            AnimationStart(Image.RenderTransform, "TranslateX", runTime, source, targetX);

            source = ct.TranslateY;
            targetY = y;
            AnimationStart(Image.RenderTransform, "TranslateY", runTime, source, targetY);

            foreach (var zone in LightZones)
            {
                double targetZoneX;
                double targetZoneY;

                zone_ct = zone.Frame.RenderTransform as CompositeTransform;

                source = zone_ct.TranslateX;
                targetZoneX = targetX + zone.RelativeZoneRect.Left;
                AnimationStart(zone.Frame.RenderTransform, "TranslateX", runTime, source, targetZoneX);

                source = zone_ct.TranslateY;
                targetZoneY = targetY + zone.RelativeZoneRect.Top;
                AnimationStart(zone.Frame.RenderTransform, "TranslateY", runTime, source, targetZoneY);
            }
        }
        public Table ToTable()
        {
            Table deviceTable = CreateNewTable();
            Table locationTable = GetLocationTable();

            string type = "";

            switch (Type)
            {
                case 0: type = "Notebook"; break;
                case 1: type = "Mouse"; break;
                case 2: type = "Keyboard"; break;
                case 3: type = "Headset"; break;
            }

            deviceTable.Set("name", DynValue.NewString(Name));
            deviceTable.Set("DeviceType", DynValue.NewString(type));
            deviceTable.Set("location", DynValue.NewTable(locationTable));

            return deviceTable;
        }
        private Table GetLocationTable()
        {
            Table locationTable = CreateNewTable();

            locationTable.Set("x", DynValue.NewNumber(GridPosition.X));
            locationTable.Set("y", DynValue.NewNumber(GridPosition.Y));

            return locationTable;
        }
    }
}