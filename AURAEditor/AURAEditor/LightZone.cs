using AuraEditor.Common;
using System;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using Windows.Graphics.Imaging;
using System.Runtime.InteropServices;
using static AuraEditor.Common.ControlHelper;
using Windows.Storage;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Core;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml;

namespace AuraEditor
{
    public class LightZone
    {
        public FrameworkElement MyFrameworkElement;

        public int Index;
        public int ZIndex;
        public Rect RelativeZoneRect
        {
            get
            {
                CompositeTransform ct;
                ct = MyFrameworkElement.RenderTransform as CompositeTransform;

                return new Rect(
                    new Point(
                        ct.TranslateX, //x
                        ct.TranslateY), //y
                    new Point(
                        ct.TranslateX + MyFrameworkElement.ActualWidth, //w
                        ct.TranslateY + MyFrameworkElement.ActualHeight) //h
                );
            }
        }
        public Rect AbsoluteZoneRect
        {
            get
            {
                Border b = FindParentControl<Border>(MyFrameworkElement, typeof(Border));
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

        public LightZone(LedUI led)
        {
            int frameLeft = led.Left;
            int frameTop = led.Top;
            int frameRight = led.Right;
            int frameBottom = led.Bottom;

            Index = led.Index;
            Selected = false;
            MyFrameworkElement = CreateRectangle(new Rect(
                    new Point(frameLeft, frameTop),
                    new Point(frameRight, frameBottom))
                    );
            MyFrameworkElement.SetValue(Canvas.ZIndexProperty, led.ZIndex);
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
        virtual public async void Frame_StatusChanged(RegionStatus status)
        {
            Shape shape = MyFrameworkElement as Shape;

            if (status == RegionStatus.Normal)
            {
                shape.Stroke = new SolidColorBrush(Colors.Black);
                shape.Fill = new SolidColorBrush(Colors.Transparent);
                Selected = false;
            }
            else if (status == RegionStatus.NormalHover)
            {
                shape.Stroke = new SolidColorBrush(Colors.Black);
                shape.Fill = new SolidColorBrush(new Color { A = 100, R = 0, G = 0, B = 123 });
                Selected = false;
            }
            else if (status == RegionStatus.Selected)
            {
                shape.Stroke = new SolidColorBrush(Colors.Red);
                shape.Fill = new SolidColorBrush(Colors.Transparent);
                Selected = true;
            }
            else if (status == RegionStatus.Watching)
            {
                shape.Stroke = new SolidColorBrush(Colors.Yellow);
                shape.Fill = new SolidColorBrush(Colors.Transparent);
                Selected = true;
            }
            else
            {
                shape.Stroke = new SolidColorBrush(Colors.Red);
                shape.Fill = new SolidColorBrush(new Color { A = 100, R = 0, G = 0, B = 123 });
                Selected = true;
            }
        }
    }
}
