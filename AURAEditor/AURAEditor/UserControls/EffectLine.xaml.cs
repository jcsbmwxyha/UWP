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
            if (_cursorMove)
            {
                if (MyEffect.UI_X + e.Delta.Translation.X < 0)
                    return;
                MyEffect.UI_X += e.Delta.Translation.X;
            }
            else if (_cursorSizeRight)
            {
                if (e.Position.X > 50)
                    MyEffect.UI_Width = e.Position.X;

                if (e.Delta.Translation.X > 0)
                    MyEffect.Layer.OnCursorSizeRight(MyEffect);
            }
            else if (_cursorSizeLeft)
            {
                double move = e.Delta.Translation.X;

                // If effectline expand to the left, we should check if it will overlap others?
                if (move < 0 && MyEffect.Layer.FindEffectByPosition(MyEffect.UI_X + move) != null)
                    return;

                if (MyEffect.UI_Width - move > 50)
                {
                    MyEffect.UI_X += move;
                    MyEffect.UI_Width -= move;
                }
            }
        }
        void EffectLine_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            double left;
            double right;
            double width;
            //fe.Opacity = 1;

            left = MyEffect.UI_X;
            right = MyEffect.UI_X + MyEffect.UI_Width;

            if (_cursorSizeLeft)
            {
                left = left / 10 * 10;
                width = right - left;
            }
            else if (_cursorSizeRight)
            {
                right = right / 10 * 10;
                width = right - left;
            }
            else // move
            {
                width = right - left;
                left = left / 10 * 10;
            }

            MyEffect.UI_X = left;
            MyEffect.UI_Width = width;
            MyEffect.UI_X = MyEffect.Layer.InsertEffectLine(MyEffect);
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
                    if ((_cursorSizeRight | _cursorSizeLeft | _cursorMove) == false)
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
            MainPage.MainPageInstance.SelectedEffectLine = MyEffect;
        }
    }
}
