﻿using AuraEditor.Common;
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
        public bool Hover;
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
                StrokeThickness = 1,
                RenderTransform = ct,
                Width = Rect.Width,
                Height = Rect.Height,
                HorizontalAlignment = 0,
                VerticalAlignment = 0,
                RadiusX = 2,
                RadiusY = 3
            };

            rectangle.Stroke = new SolidColorBrush(Colors.Black);

            return rectangle;
        }
        virtual public void OnReceiveMouseEvent(MouseEvent mouseEvent)
        {
            if (mouseEvent == MouseEvent.Click)
            {
                if (Selected == true)
                {
                    if (Hover == true)
                        ChangeStatus(RegionStatus.NormalHover);
                    else
                        ChangeStatus(RegionStatus.Normal);
                }
                else
                {
                    if (Hover == true)
                        ChangeStatus(RegionStatus.SelectedHover);
                    else
                        ChangeStatus(RegionStatus.Selected);
                }
            }
            else if (mouseEvent == MouseEvent.Hover)
            {
                if (Selected == true)
                {
                    ChangeStatus(RegionStatus.SelectedHover);
                }
                else
                {
                    ChangeStatus(RegionStatus.NormalHover);
                }
            }
            else // Unhover
            {
                if (Selected == true)
                {
                    ChangeStatus(RegionStatus.Selected);
                }
                else
                {
                    ChangeStatus(RegionStatus.Normal);
                }
            }
        }
        virtual public async void ChangeStatus(RegionStatus status)
        {
            Shape shape = MyFrameworkElement as Shape;

            if (status == RegionStatus.Normal)
            {
                shape.Stroke = new SolidColorBrush(Colors.Black);
                shape.Fill = new SolidColorBrush(Colors.Transparent);
                Selected = false;
                Hover = false;
            }
            else if (status == RegionStatus.NormalHover)
            {
                shape.Stroke = new SolidColorBrush(Colors.Black);
                shape.Fill = new SolidColorBrush(new Color { A = 100, R = 0, G = 0, B = 123 });
                Selected = false;
                Hover = true;
            }
            else if (status == RegionStatus.Selected)
            {
                shape.Stroke = new SolidColorBrush(Colors.Red);
                shape.Fill = new SolidColorBrush(Colors.Transparent);
                Selected = true;
                Hover = false;
            }
            else if (status == RegionStatus.SelectedHover)
            {
                shape.Stroke = new SolidColorBrush(Colors.Red);
                shape.Fill = new SolidColorBrush(new Color { A = 100, R = 0, G = 0, B = 123 });
                Selected = true;
                Hover = true;
            }
            else if (status == RegionStatus.Watching)
            {
                shape.Stroke = new SolidColorBrush(Colors.Yellow);
                shape.Fill = new SolidColorBrush(Colors.Transparent);
                Selected = true;
                Hover = false;
            }
        }
    }
}
