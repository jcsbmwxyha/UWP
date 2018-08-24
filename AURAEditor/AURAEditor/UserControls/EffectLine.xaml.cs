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
using AuraEditor.Common;
using CoreCursor = Windows.UI.Core.CoreCursor;
using Windows.UI.Core;
using static AuraEditor.Common.ControlHelper;

// 使用者控制項項目範本記載於 https://go.microsoft.com/fwlink/?LinkId=234236

namespace AuraEditor.UserControls
{
    public sealed partial class EffectLine : UserControl
    {
        public Effect MyEffectLine { get { return this.DataContext as Effect; } }

        public enum CursorState
        {
            None = 0,
            SizeAll = 1,
            SizeLeft = 2,
            SizeRight = 3,
        }
        private CursorState mouseState;
        public CursorState MouseState
        {
            get
            {
                return mouseState;
            }
            set
            {
                if (mouseState != value)
                {
                    if (value == CursorState.None)
                        Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 0);
                    else if (value == CursorState.SizeAll)
                        Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.SizeAll, 0);
                    else if (value == CursorState.SizeLeft || value == CursorState.SizeRight)
                        Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.SizeWestEast, 0);

                    mouseState = value;
                }
            }
        }

        public EffectLine()
        {
            this.InitializeComponent();
        }

        void EffectLine_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (MouseState == CursorState.SizeAll)
            {
                if (MyEffectLine.UI_X + e.Delta.Translation.X < 0)
                    return;

                MyEffectLine.UI_X += e.Delta.Translation.X;
            }
            else if (MouseState == CursorState.SizeRight)
            {
                if (e.Position.X <= 50)
                    return;

                MyEffectLine.UI_Width = e.Position.X;

                if (e.Delta.Translation.X > 0) // To right
                {
                    // We should check if it will overlap others
                    DeviceLayer myLayer = MyEffectLine.Layer;
                    Effect overlappedEL = myLayer.TestAndGetFirstOverlappingEffect(MyEffectLine);

                    if (overlappedEL != null)
                        myLayer.PushAllEffectsWhichOnTheRight(MyEffectLine, e.Delta.Translation.X);
                }
            }
            else if (MouseState == CursorState.SizeLeft)
            {
                double move = e.Delta.Translation.X;

                if (move < 0) // To left
                {
                    // We should check if it will overlap others
                    if (MyEffectLine.Layer.FindEffectByPosition(MyEffectLine.UI_X + move) != null)
                        return;
                }

                if (MyEffectLine.UI_Width - move <= 50)
                    return;

                MyEffectLine.UI_X += move;
                MyEffectLine.UI_Width -= move;
            }
        }
        void EffectLine_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            double keepWidth;

            if (MouseState == CursorState.SizeLeft)
            {
                keepWidth = MyEffectLine.UI_Width;
                MyEffectLine.UI_X = RoundToTens(MyEffectLine.UI_X);
                MyEffectLine.UI_Width = RoundToTens(keepWidth);
            }
            else if (MouseState == CursorState.SizeRight)
            {
                keepWidth = MyEffectLine.UI_Width;
                MyEffectLine.UI_Width = RoundToTens(keepWidth);
            }
            else // move
            {
                MyEffectLine.UI_X = RoundToTens(MyEffectLine.UI_X);
            }

            MyEffectLine.Layer.InsertEffectLine(MyEffectLine);
            MouseState = CursorState.None;
        }
        private void EffectLine_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            FrameworkElement fe = sender as FrameworkElement;
            EffectLine el = fe.Parent as EffectLine;
            PointerPoint ptrPt = e.GetCurrentPoint(el);
            Point position = e.GetCurrentPoint(el).Position;

            //System.Diagnostics.Debug.WriteLine(position.X);
            if (ptrPt.Properties.IsLeftButtonPressed)
                return;

            if (position.X > el.Width - 5)
            {
                MouseState = CursorState.SizeRight;
            }
            else if (position.X < 5)
            {
                MouseState = CursorState.SizeLeft;
            }
            else
            {
                MouseState = CursorState.SizeAll;
            }
        }
        private void EffectLine_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            FrameworkElement fe = sender as FrameworkElement;
            EffectLine el = fe.Parent as EffectLine;
            PointerPoint ptrPt = e.GetCurrentPoint(el);

            // ManipulationCompleted will handle it if the mouse is pressed
            if (!ptrPt.Properties.IsLeftButtonPressed)
                MouseState = CursorState.None;
        }
        private void EffectLine_Click(object sender, RoutedEventArgs e)
        {
            MainPage.MainPageInstance.SelectedEffectLine = MyEffectLine;
        }
    }
}
