using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Input;
using static AuraEditor.Common.ControlHelper;
using static AuraEditor.Common.Definitions;
using static AuraEditor.Common.LuaHelper;
using Windows.Foundation;
using MoonSharp.Interpreter;
using static AuraEditor.AuraSpaceManager;
using Windows.UI.Xaml.Shapes;
using Windows.UI;

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
        public Border Border { get; set; }
        public LightZone[] LightZones { get; set; }

        public Device(Image img, LightZone[] zones)
        {
            LightZones = zones;

            Canvas childCanvas = new Canvas();
            childCanvas.VerticalAlignment = 0;
            childCanvas.Children.Add(img);

            foreach (var zone in zones)
            {
                childCanvas.Children.Add(zone.Frame);
            }

            Border = new Border
            {
                RenderTransform = new CompositeTransform(),
                ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY,
                BorderBrush = new SolidColorBrush(Windows.UI.Colors.Orange),
                BorderThickness = new Thickness(0),
                Child = childCanvas
            };
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
            Border.Opacity = 0.5;
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
            Border.Opacity = 1;

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
            CompositeTransform ct = Border.RenderTransform as CompositeTransform;
            ct.TranslateX = x;
            ct.TranslateY = y;
        }
        private void SetPositionByAnimation(double x, double y)
        {
            CompositeTransform ct = Border.RenderTransform as CompositeTransform;
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