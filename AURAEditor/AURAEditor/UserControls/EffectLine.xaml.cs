using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// 使用者控制項項目範本記載於 https://go.microsoft.com/fwlink/?LinkId=234236

namespace AuraEditor.UserControls
{
    public sealed partial class EffectLine : UserControl
    {
        public Effect MyEffect { get { return this.DataContext as Effect; } }
        private bool _cursorSizeRight;
        private bool _cursorSizeLeft;
        private bool _cursorMove;

        private bool lightButtonPressed;
        public bool LightButtonPressed {
            get { return lightButtonPressed; }
            set
            {
                if (value ^ lightButtonPressed)
                {
                    if (value == true)
                    {

                    }
                    else
                    {
                        _cursorSizeRight = false;
                        _cursorSizeLeft = false;
                        _cursorMove = false;
                    }

                    lightButtonPressed = value;
                }
            }
        }

        public EffectLine()
        {
            this.InitializeComponent();
        }
        
        void EffectLine_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var fe = sender as FrameworkElement;
            EffectLine el = fe.Parent as EffectLine;
            CompositeTransform ct = el.RenderTransform as CompositeTransform;

            if (_cursorMove)
            {
                if ((ct.TranslateX + e.Delta.Translation.X < 0))
                    return;
                ct.TranslateX += e.Delta.Translation.X;
            }
            else if (_cursorSizeRight)
            {
                MyEffect.MyDeviceGroup.OnCursorSizeRight(MyEffect, (int)(ct.TranslateX), (int)(el.Width + e.Delta.Translation.X));

                if (e.Position.X > 50)
                    el.Width = e.Position.X;
            }
            else if (_cursorSizeLeft)
            {
                if (MyEffect.MyDeviceGroup.IsEffectLineOverlap(MyEffect, (int)(ct.TranslateX + e.Delta.Translation.X), (int)(el.Width - e.Delta.Translation.X)) != null)
                    return;

                if (el.Width - e.Delta.Translation.X > 50)
                {
                    ct.TranslateX += e.Delta.Translation.X;
                    el.Width -= e.Delta.Translation.X;
                }
            }
        }
        void EffectLine_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            int leftPosition, rightPosition, width;
            var fe = sender as FrameworkElement;
            EffectLine el = fe.Parent as EffectLine;
            //fe.Opacity = 1;

            CompositeTransform ct = el.RenderTransform as CompositeTransform;
            leftPosition = (int)ct.TranslateX / 10 * 10;

            if (_cursorSizeLeft)
            {
                rightPosition = (int)ct.TranslateX + (int)el.Width;
                width = rightPosition - leftPosition;
            }
            else
            {
                width = (int)el.Width / 10 * 10;
            }

            leftPosition = MyEffect.MyDeviceGroup.InsertEffectLine(MyEffect, leftPosition, width);
            MyEffect.Start = leftPosition;
            MyEffect.Duration = width;
        }
        private void EffectLine_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            FrameworkElement fe = sender as FrameworkElement;
            EffectLine el = fe.Parent as EffectLine;
            PointerPoint ptrPt = e.GetCurrentPoint(el);
            Point position = e.GetCurrentPoint(el).Position;

            //System.Diagnostics.Debug.WriteLine(position.X);

            if (ptrPt.PointerDevice.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse)
            {
                if (position.X > el.Width - 5)
                {
                    Window.Current.CoreWindow.PointerCursor =
                        new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.SizeWestEast, 0);
                }
                else if (position.X < 5)
                {
                    Window.Current.CoreWindow.PointerCursor =
                          new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.SizeWestEast, 0);
                }
                else
                {
                    Window.Current.CoreWindow.PointerCursor =
                        new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.SizeAll, 0);
                }

                if (ptrPt.Properties.IsLeftButtonPressed)
                {
                    if (_cursorSizeRight | _cursorSizeLeft | _cursorMove == false)
                    {
                        LightButtonPressed = true;
                        if (position.X > el.Width - 5)
                        {
                            _cursorSizeRight = true;
                        }
                        else if (position.X < 5)
                        {
                            _cursorSizeLeft = true;
                        }
                        else
                        {
                            _cursorMove = true;
                        }
                    }
                }
                else
                {
                    LightButtonPressed = false;
                }
            }
        }
        private void EffectLine_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor =
                new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
        }

        private void EffectLine_Click(object sender, RoutedEventArgs e)
        {
            var frame = (Frame)Window.Current.Content;
            var page = (MainPage)frame.Content;
            
            page.UpdateEffectInfoGrid(MyEffect);
        }
    }
}
