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
using static AuraEditor.AuraSpaceManager;

namespace AuraEditor
{
    public class Device
    {
        public string Name { get; set; }
        public int Type { get; set; }
        public Point GridPosition
        {
            get
            {
                CompositeTransform ct = Border.RenderTransform as CompositeTransform;

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
        public double Width
        {
            get
            {
                return Border.Width / GridPixels;
            }
            set
            {
                Border.Width = value * GridPixels;
            }
        }
        public double Height
        {
            get
            {
                return Border.Height / GridPixels;
            }
            set
            {
                Border.Height = value * GridPixels;
            }
        }
        private Image Image { get; set; }
        public Border Border { get; set; }
        public LightZone[] LightZones { get; set; }

        public Device(Image img)
        {
            Image = img;
            //Image.Tapped += DeviceImg_Tapped;

            Border = new Border();
            Border.RenderTransform = new CompositeTransform();
            Border.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Orange);
            Border.BorderThickness = new Thickness(0);
            Border.Child = Image;
        }
        public void EnableManipulation()
        {
            Border.ManipulationStarted -= ImageManipulationStarted;
            Border.ManipulationDelta -= ImageManipulationDelta;
            Border.ManipulationCompleted -= ImageManipulationCompleted;
            Border.PointerEntered -= ImagePointerEntered;
            Border.PointerExited -= ImagePointerExited;

            Border.ManipulationStarted += ImageManipulationStarted;
            Border.ManipulationDelta += ImageManipulationDelta;
            Border.ManipulationCompleted += ImageManipulationCompleted;
            Border.PointerEntered += ImagePointerEntered;
            Border.PointerExited += ImagePointerExited;

            Border.BorderThickness = new Thickness(0.8);
        }
        public void DisableManipulation()
        {
            Border.ManipulationStarted -= ImageManipulationStarted;
            Border.ManipulationDelta -= ImageManipulationDelta;
            Border.ManipulationCompleted -= ImageManipulationCompleted;
            Border.PointerEntered -= ImagePointerEntered;
            Border.PointerExited -= ImagePointerExited;

            Border.BorderThickness = new Thickness(0);
        }

        #region Mouse event
        private void DeviceImg_Tapped(object sender, TappedRoutedEventArgs e)
        {
            AuraSpaceManager.Self.SetSpaceStatus(SpaceStatus.Normal);
        }
        private void ImageManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            CompositeTransform ct = Border.RenderTransform as CompositeTransform;

            _oldPixelPosition = new Point(ct.TranslateX, ct.TranslateY);
            Image.Opacity = 0.5;
        }
        private void ImageManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            CompositeTransform ct = Border.RenderTransform as CompositeTransform;

            SetPosition(
                ct.TranslateX + e.Delta.Translation.X,
                ct.TranslateY + e.Delta.Translation.Y);
        }
        private void ImageManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            CompositeTransform ct = Border.RenderTransform as CompositeTransform;

            SetPosition(
                RoundToGrid(ct.TranslateX),
                RoundToGrid(ct.TranslateY));
            Image.Opacity = 1;

            if (!AuraSpaceManager.Self.IsOverlapping(this))
            {
                AuraSpaceManager.Self.MoveDevicePosition(this,
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
            SetPositionOfBorder(x, y);
            SetPositionOfAllZones(x, y);
        }
        private void SetPositionOfImage(double x, double y)
        {
            CompositeTransform ct = Image.RenderTransform as CompositeTransform;
            ct.TranslateX = x;
            ct.TranslateY = y;
        }
        private void SetPositionOfBorder(double x, double y)
        {
            CompositeTransform ct = Border.RenderTransform as CompositeTransform;
            ct.TranslateX = x;
            ct.TranslateY = y;
        }
        private void SetPositionOfAllZones(double imageX, double imageY)
        {
            CompositeTransform zone_ct;
            foreach (var zone in LightZones)
            {
                zone_ct = zone.Frame.RenderTransform as CompositeTransform;
                zone_ct.TranslateX = imageX + zone.RelativeZoneRect.Left;
                zone_ct.TranslateY = imageY + zone.RelativeZoneRect.Top;
            }
        }
        private void SetPositionByAnimation(double x, double y)
        {
            CompositeTransform ct = Border.RenderTransform as CompositeTransform;
            CompositeTransform zone_ct;
            double runTime = 300;
            double source;
            double targetX;
            double targetY;

            source = ct.TranslateX;
            targetX = x;
            AnimationStart(Border.RenderTransform, "TranslateX", runTime, source, targetX);

            source = ct.TranslateY;
            targetY = y;
            AnimationStart(Border.RenderTransform, "TranslateY", runTime, source, targetY);

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