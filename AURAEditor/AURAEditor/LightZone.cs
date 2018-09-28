using AuraEditor.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using static AuraEditor.Common.Definitions;
using static AuraEditor.Common.ControlHelper;

namespace AuraEditor
{
    public class LightZone
    {
        public Shape Frame;
        public int Index;
        public int ZIndex;
        public Rect RelativeZoneRect
        {
            get
            {
                CompositeTransform ct = Frame.RenderTransform as CompositeTransform;

                return new Rect(
                    new Point(
                        ct.TranslateX, //x
                        ct.TranslateY), //y
                    new Point(
                        ct.TranslateX + Frame.Width, //w
                        ct.TranslateY + Frame.Height) //h
                );
            }
        }
        public Rect AbsoluteZoneRect
        {
            get
            {
                Border b = FindParentControl<Border>(Frame, typeof(Border));
                CompositeTransform ct = b.RenderTransform as CompositeTransform;

                return new Rect(
                    new Point(
                        ct.TranslateX + RelativeZoneRect.Left, //x
                        ct.TranslateY + RelativeZoneRect.Top), //y
                    new Point(
                        ct.TranslateX + RelativeZoneRect.Left + RelativeZoneRect.Width, //w
                        ct.TranslateY + RelativeZoneRect.Top + RelativeZoneRect.Height) //h
                );
            }
        }
        public bool Selected;
        
        public LightZone(Point deviceGridPosition, LedUI led)
        {
            int frameLeft = led.Left;
            int frameTop = led.Top;
            int frameRight = led.Right;
            int frameBottom = led.Bottom;

            Index = led.Index;
            Selected = false;
            Frame = CreateRectangle(new Rect(
                    new Point(frameLeft, frameTop),
                    new Point(frameRight, frameBottom))
                    );
            Frame.SetValue(Canvas.ZIndexProperty, led.ZIndex);
            ZIndex = led.ZIndex;
        }
        private Rectangle CreateRectangle(Rect Rect)
        {
            CompositeTransform ct = new CompositeTransform
            {
                TranslateX = Rect.X,
                TranslateY = Rect.Y
            };

            Rectangle rectangle = new Rectangle
            {
                Fill = new SolidColorBrush(Colors.Transparent),
                StrokeThickness = 2,
                RenderTransform = ct,
                Width = Rect.Width,
                Height = Rect.Height,
                HorizontalAlignment = 0,
                VerticalAlignment = 0,
                RadiusX = 3,
                RadiusY = 4
            };

            rectangle.Stroke = new SolidColorBrush(Colors.Black);

            return rectangle;
        }
        public void Frame_StatusChanged(int regionIndex, RegionStatus status)
        {
            if (status == RegionStatus.Normal)
            {
                Frame.Stroke = new SolidColorBrush(Colors.Black);
                Frame.Fill = new SolidColorBrush(Colors.Transparent);
                Selected = false;
            }
            else if (status == RegionStatus.NormalHover)
            {
                Frame.Stroke = new SolidColorBrush(Colors.Black);
                Frame.Fill = new SolidColorBrush(new Color { A = 100, R = 0, G = 0, B = 123 });
                Selected = false;
            }
            else if (status == RegionStatus.Selected)
            {
                Frame.Stroke = new SolidColorBrush(Colors.Red);
                Frame.Fill = new SolidColorBrush(Colors.Transparent);
                Selected = true;
            }
            else
            {
                Frame.Stroke = new SolidColorBrush(Colors.Red);
                Frame.Fill = new SolidColorBrush(new Color { A = 100, R = 0, G = 0, B = 123 });
                Selected = true;
            }
        }
    }
}
