using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Input;
using static AuraEditor.Common.ControlHelper;
using static AuraEditor.Common.Definitions;
using static AuraEditor.Common.XmlHelper;
using static AuraEditor.Common.EffectHelper;
using Windows.Foundation;
using MoonSharp.Interpreter;
using static AuraEditor.AuraSpaceManager;
using Windows.UI.Xaml.Shapes;
using Windows.UI;
using System.Xml;

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

        public XmlNode ToXmlNodeForScript()
        {
            XmlNode deviceNode = CreateXmlNodeOfFile("device");

            XmlNode modelNode = CreateXmlNodeOfFile("model");
            modelNode.InnerText = Name.ToString();
            deviceNode.AppendChild(modelNode);

            string type = "";
            switch (Type)
            {
                case 0: type = "Notebook"; break;
                case 1: type = "Mouse"; break;
                case 2: type = "Keyboard"; break;
                case 3: type = "Headset"; break;
            }
            XmlNode typeNode = CreateXmlNodeOfFile("type");
            typeNode.InnerText = type.ToString();
            deviceNode.AppendChild(typeNode);

            XmlNode locationNode = GetLocationXmlNode();
            deviceNode.AppendChild(locationNode);

            return deviceNode;
        }
        private XmlNode GetLocationXmlNode()
        {
            XmlNode locationNode = CreateXmlNodeOfFile("location");

            XmlNode xNode = CreateXmlNodeOfFile("x");
            xNode.InnerText = GridPosition.X.ToString();
            locationNode.AppendChild(xNode);

            XmlNode yNode = CreateXmlNodeOfFile("y");
            yNode.InnerText = GridPosition.Y.ToString();
            locationNode.AppendChild(yNode);

            return locationNode;
        }

        public XmlNode ToXmlNodeForUserData()
        {
            XmlNode deviceNode = CreateXmlNodeOfFile("device");

            XmlAttribute attribute = CreateXmlAttributeOfFile("name");
            attribute.Value = Name;
            deviceNode.Attributes.Append(attribute);

            XmlNode typeNode = CreateXmlNodeOfFile("type");
            typeNode.InnerText = Type.ToString();
            deviceNode.AppendChild(typeNode);

            XmlNode xNode = CreateXmlNodeOfFile("x");
            xNode.InnerText = GridPosition.X.ToString();
            deviceNode.AppendChild(xNode);

            XmlNode yNode = CreateXmlNodeOfFile("y");
            yNode.InnerText = GridPosition.Y.ToString();
            deviceNode.AppendChild(yNode);

            return deviceNode;
        }
    }
}