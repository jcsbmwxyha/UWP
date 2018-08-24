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

namespace AuraEditor
{
    public class LightZone
    {
        public Shape Frame;
        public int Index;
        public Rect RelativeZoneRect;
        public Rect AbsoluteZoneRect
        {
            get
            {
                CompositeTransform ct = Frame.RenderTransform as CompositeTransform;
                return new Rect(
                    new Point(ct.TranslateX, ct.TranslateY),
                    new Point(ct.TranslateX + RelativeZoneRect.Width, ct.TranslateY + RelativeZoneRect.Height)
                    );
            }
        }
        public bool Selected;
        public int ZIndex;
        
        public LightZone(Point deviceGridPosition, LedUI led)
        {
            int deviceLeft;
            int deviceTop;
            int frameLeft;
            int frameTop;
            int frameRight;
            int frameBottom;

            deviceLeft = (int)deviceGridPosition.X * GridWidthPixels;
            deviceTop = (int)deviceGridPosition.Y * GridWidthPixels;
            frameLeft = led.Left;
            frameTop = led.Top;
            frameRight = led.Right;
            frameBottom = led.Bottom;

            Index = led.Index;
            Selected = false;
            RelativeZoneRect = new Rect(
                new Point(frameLeft, frameTop),
                new Point(frameRight, frameBottom));
            Frame = CreateRectangle( new Rect(
                    new Point(frameLeft + deviceLeft, frameTop + deviceTop),
                    new Point(frameRight + deviceLeft, frameBottom + deviceTop)));
            Frame.SetValue(Canvas.ZIndexProperty, led.ZIndex);
            ZIndex = led.ZIndex;
        }
        private Rectangle CreateRectangle(Windows.Foundation.Rect Rect)
        {
            CompositeTransform ct = new CompositeTransform
            {
                TranslateX = Rect.X,
                TranslateY = Rect.Y
            };

            Rectangle rectangle = new Rectangle
            {
                Fill = new SolidColorBrush(Colors.Transparent),
                StrokeThickness = 3,
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
