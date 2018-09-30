using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Core.Preview;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace IntelligentScrollViewer
{
    public sealed partial class MainPage : Page
    {
        bool _pressed;
        int _mousePosition;
        private DispatcherTimer TimerClock;

        public MainPage()
        {
            this.InitializeComponent();

            TimerClock = new DispatcherTimer();
            TimerClock.Tick += Timer_Tick;
            TimerClock.Interval = new TimeSpan(0, 0, 0, 0, 5); // 10 ms

            _mousePosition = 0;
            _pressed = false;
        }
        private void Timer_Tick(object sender, object e)
        {
            Rect screenRect = new Rect(
                       MyScrollViewer.HorizontalOffset,
                       MyScrollViewer.VerticalOffset,
                       MyScrollViewer.Width,
                       MyScrollViewer.Height);

            CompositeTransform ct = Rect.RenderTransform as CompositeTransform;
            int offset = 10;
            if (_mousePosition == 1)
            {
                MyScrollViewer.ChangeView(
                    MyScrollViewer.HorizontalOffset - offset,
                    MyScrollViewer.VerticalOffset - offset,
                    1, true);
                if (screenRect.X != 0)
                    ct.TranslateX = ct.TranslateX - offset;
                if (screenRect.Y != 0)
                    ct.TranslateY = ct.TranslateY - offset;
            }
            if (_mousePosition == 2)
            {
                MyScrollViewer.ChangeView(
                    MyScrollViewer.HorizontalOffset,
                    MyScrollViewer.VerticalOffset - offset,
                    1, true);
                if (screenRect.Y != 0)
                    ct.TranslateY = ct.TranslateY - offset;
            }
            if (_mousePosition == 3)
            {
                MyScrollViewer.ChangeView(
                    MyScrollViewer.HorizontalOffset + offset,
                    MyScrollViewer.VerticalOffset - offset,
                    1, true);

                if (MyScrollViewer.HorizontalOffset + MyScrollViewer.Width != MyCanvas.Width)
                    ct.TranslateX = ct.TranslateX + offset;
                if (screenRect.Y != 0)
                    ct.TranslateY = ct.TranslateY - offset;
            }
            if (_mousePosition == 4)
            {
                MyScrollViewer.ChangeView(
                    MyScrollViewer.HorizontalOffset - offset,
                    MyScrollViewer.VerticalOffset,
                    1, true);
                if (screenRect.X != 0)
                    ct.TranslateX = ct.TranslateX - offset;
            }
            if (_mousePosition == 5) { }
            if (_mousePosition == 6)
            {
                MyScrollViewer.ChangeView(
                    MyScrollViewer.HorizontalOffset + offset,
                    MyScrollViewer.VerticalOffset,
                    1, true);
                if (MyScrollViewer.HorizontalOffset + MyScrollViewer.Width != MyCanvas.Width)
                    ct.TranslateX = ct.TranslateX + offset;
            }
            if (_mousePosition == 7)
            {
                MyScrollViewer.ChangeView(
                    MyScrollViewer.HorizontalOffset - offset,
                    MyScrollViewer.VerticalOffset + offset,
                    1, true);

                if (screenRect.X != 0)
                    ct.TranslateX = ct.TranslateX - offset;
                if (MyScrollViewer.VerticalOffset + MyScrollViewer.Height != MyCanvas.Height)
                    ct.TranslateY = ct.TranslateY + offset;
            }
            if (_mousePosition == 8)
            {
                MyScrollViewer.ChangeView(
                    MyScrollViewer.HorizontalOffset,
                    MyScrollViewer.VerticalOffset + offset,
                    1, true);
                if (MyScrollViewer.VerticalOffset + MyScrollViewer.Height != MyCanvas.Height)
                    ct.TranslateY = ct.TranslateY + offset;
            }
            if (_mousePosition == 9) {
                MyScrollViewer.ChangeView(
                    MyScrollViewer.HorizontalOffset + offset,
                    MyScrollViewer.VerticalOffset + offset,
                    1, true);
                if (MyScrollViewer.HorizontalOffset + MyScrollViewer.Width != MyCanvas.Width)
                    ct.TranslateX = ct.TranslateX + offset;
                if (MyScrollViewer.VerticalOffset + MyScrollViewer.Height != MyCanvas.Height)
                    ct.TranslateY = ct.TranslateY + offset;
            }
        }

        private void Rectangle_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            Rectangle rect = sender as Rectangle;

            CompositeTransform ct = rect.RenderTransform as CompositeTransform;
            ct.TranslateX = ct.TranslateX + e.Delta.Translation.X;
            ct.TranslateY = ct.TranslateY + e.Delta.Translation.Y;
        }

        private void Rect_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var fe = sender as FrameworkElement;
            bool _hasCapture = fe.CapturePointer(e.Pointer);
            _pressed = true;
            TimerClock.Start();
        }

        private void Rect_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (_pressed == true)
            {

                Rectangle rect = sender as Rectangle;
                CompositeTransform ct = rect.RenderTransform as CompositeTransform;

                Rect screenRect = new Rect(
                    MyScrollViewer.HorizontalOffset,
                    MyScrollViewer.VerticalOffset,
                    MyScrollViewer.Width,
                    MyScrollViewer.Height);

                PointerPoint ptrPt = e.GetCurrentPoint(MyCanvas);
                Point Position = ptrPt.Position;

                if (Position.X > screenRect.Right && Position.Y > screenRect.Bottom)
                {
                    _mousePosition = 9;
                }
                else if (Position.X > screenRect.Right && Position.Y < screenRect.Top)
                {
                    _mousePosition = 3;
                }
                else if (Position.X > screenRect.Right)
                {
                    _mousePosition = 6;
                }
                else if (Position.X < screenRect.Left && Position.Y > screenRect.Bottom)
                {
                    _mousePosition = 7;
                }
                else if (Position.X < screenRect.Left && Position.Y < screenRect.Top)
                {
                    _mousePosition = 1;
                }
                else if (Position.X < screenRect.Left)
                {
                    _mousePosition = 4;
                }
                else if (Position.Y < screenRect.Top)
                {
                    _mousePosition = 2;
                }
                else if (Position.Y > screenRect.Bottom)
                {
                    _mousePosition = 8;
                }
                else
                    _mousePosition = 5;
            }
        }

        private void Rect_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            _pressed = false;
            TimerClock.Stop();
        }
    }
}
