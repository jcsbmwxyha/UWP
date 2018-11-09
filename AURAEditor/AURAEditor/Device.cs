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
using System;

namespace AuraEditor
{
    public enum DeviceStatus
    {
        OnStage = 0,
        Temp,
    }

    public class Device
    {
        public string Name { get; set; }
        public int Type { get; set; }
        public Point GridPosition
        {
            get
            {
                CompositeTransform ct = m_Container.RenderTransform as CompositeTransform;

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
        public LightZone[] LightZones { get; set; }
        public DeviceStatus Status { get; set; }

        private Border m_Container { get; set; }
        public Border GetContainer() { return m_Container; }
        private Image m_Image;
        private Rectangle m_DotRect;
        public double GridWidth
        {
            get
            {
                return m_Image.Width / GridPixels;
            }
        }
        public double GridHeight
        {
            get
            {
                return m_Image.Height / GridPixels;
            }
        }

        public Device(Image img, LightZone[] zones)
        {
            m_Image = img;
            LightZones = zones;

            Canvas childCanvas = new Canvas();
            childCanvas.VerticalAlignment = 0;
            childCanvas.Children.Add(img);

            m_DotRect = new Rectangle
            {
                Style = (Style)Application.Current.Resources["DeviceDotRectangle"],
                // Declaring StrokeDashArray in style is useless ... I don't know why
                StrokeDashArray = new DoubleCollection() { 3, 3 },
                Width = img.Width,
                Height = img.Height,
                Opacity = 0
            };
            childCanvas.Children.Add(m_DotRect);

            foreach (var zone in zones)
            {
                childCanvas.Children.Add(zone.MyFrameworkElement);
            }

            m_Container = new Border
            {
                RenderTransform = new CompositeTransform(),
                ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY,
                Child = childCanvas,
                Width = img.Width,
                Height = img.Height,
            };
        }

        #region Mouse event
        public void EnableManipulation()
        {
            m_Container.ManipulationDelta -= ImageManipulationDelta;
            m_Container.ManipulationCompleted -= ImageManipulationCompleted;
            m_Container.PointerPressed -= ImagePointerPressed;
            m_Container.PointerReleased -= ImagePointerReleased;
            m_Container.PointerEntered -= ImagePointerEntered;
            m_Container.PointerExited -= ImagePointerExited;
            
            m_Container.ManipulationDelta += ImageManipulationDelta;
            m_Container.ManipulationCompleted += ImageManipulationCompleted;
            m_Container.PointerPressed += ImagePointerPressed;
            m_Container.PointerReleased += ImagePointerReleased;
            m_Container.PointerEntered += ImagePointerEntered;
            m_Container.PointerExited += ImagePointerExited;
        }
        public void DisableManipulation()
        {
            m_Container.ManipulationDelta -= ImageManipulationDelta;
            m_Container.ManipulationCompleted -= ImageManipulationCompleted;
            m_Container.PointerPressed -= ImagePointerPressed;
            m_Container.PointerReleased -= ImagePointerReleased;
            m_Container.PointerEntered -= ImagePointerEntered;
            m_Container.PointerExited -= ImagePointerExited;
            m_DotRect.Opacity = 0;
            m_Image.Opacity = 1;
        }
        private void ImageManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            CompositeTransform ct = m_Container.RenderTransform as CompositeTransform;

            SetPosition(
                ct.TranslateX + e.Delta.Translation.X / AuraSpaceManager.Self.SpaceZoomFactor,
                ct.TranslateY + e.Delta.Translation.Y / AuraSpaceManager.Self.SpaceZoomFactor);
        }
        private void ImageManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            CompositeTransform ct = m_Container.RenderTransform as CompositeTransform;

            SetPosition(
                RoundToGrid(ct.TranslateX),
                RoundToGrid(ct.TranslateY));

            if (!AuraSpaceManager.Self.IsOverlapping(this))
            {
                AuraSpaceManager.Self.DeleteOverlappingTempDevice(this);
                AuraSpaceManager.Self.MoveDeviceMousePosition(this,
                    RoundToGrid(ct.TranslateX - _oldPixelPosition.X),
                    RoundToGrid(ct.TranslateY - _oldPixelPosition.Y));
            }
            else
            {
                SetPositionByAnimation(_oldPixelPosition.X, _oldPixelPosition.Y);
            }
            
            m_DotRect.Opacity = 0.6;
            m_Image.Opacity = 1;
            MainPage.Self.NeedSave = true;
        }
        private void ImagePointerPressed(object sender, PointerRoutedEventArgs e)
        {
            CompositeTransform ct = m_Container.RenderTransform as CompositeTransform;

            _oldPixelPosition = new Point(ct.TranslateX, ct.TranslateY);
            m_DotRect.Opacity = 1;
            m_Image.Opacity = 0.6;
        }
        private void ImagePointerReleased(object sender, PointerRoutedEventArgs e)
        {
            m_DotRect.Opacity = 0.6;
            m_Image.Opacity = 1;
        }
        private void ImagePointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor
                = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.SizeAll, 0);

            m_DotRect.Opacity = 0.6;
            m_Image.Opacity = 1;
        }
        private void ImagePointerExited(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor
                = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);

            m_DotRect.Opacity = 0;
            m_Image.Opacity = 1;
        }
        #endregion

        private void SetPosition(double x, double y)
        {
            CompositeTransform ct = m_Container.RenderTransform as CompositeTransform;
            ct.TranslateX = x;
            ct.TranslateY = y;
        }
        private void SetPositionByAnimation(double x, double y)
        {
            CompositeTransform ct = m_Container.RenderTransform as CompositeTransform;
            double runTime = 300;
            double source;
            double targetX;
            double targetY;

            source = ct.TranslateX;
            targetX = x;
            AnimationStart(m_Container.RenderTransform, "TranslateX", runTime, source, targetX);

            source = ct.TranslateY;
            targetY = y;
            AnimationStart(m_Container.RenderTransform, "TranslateY", runTime, source, targetY);
        }

        public XmlNode ToXmlNodeForScript()
        {
            XmlNode deviceNode = CreateXmlNode("device");

            XmlNode modelNode = CreateXmlNode("model");
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
            XmlNode typeNode = CreateXmlNode("type");
            typeNode.InnerText = type.ToString();
            deviceNode.AppendChild(typeNode);

            XmlNode locationNode = GetLocationXmlNode();
            deviceNode.AppendChild(locationNode);

            return deviceNode;
        }
        private XmlNode GetLocationXmlNode()
        {
            XmlNode locationNode = CreateXmlNode("location");

            XmlNode xNode = CreateXmlNode("x");
            xNode.InnerText = GridPosition.X.ToString();
            locationNode.AppendChild(xNode);

            XmlNode yNode = CreateXmlNode("y");
            yNode.InnerText = GridPosition.Y.ToString();
            locationNode.AppendChild(yNode);

            return locationNode;
        }

        public XmlNode ToXmlNodeForUserData()
        {
            XmlNode deviceNode = CreateXmlNode("device");

            XmlAttribute attributeName = CreateXmlAttributeOfFile("name");
            attributeName.Value = Name;
            deviceNode.Attributes.Append(attributeName);

            XmlAttribute attributeType = CreateXmlAttributeOfFile("type");
            attributeType.Value = GetTypeNameByType(Type);
            deviceNode.Attributes.Append(attributeType);

            XmlNode xNode = CreateXmlNode("x");
            xNode.InnerText = GridPosition.X.ToString();
            deviceNode.AppendChild(xNode);

            XmlNode yNode = CreateXmlNode("y");
            yNode.InnerText = GridPosition.Y.ToString();
            deviceNode.AppendChild(yNode);

            return deviceNode;
        }
    }
}