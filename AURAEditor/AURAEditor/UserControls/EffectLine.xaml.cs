using Windows.Foundation;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Core;
using CoreCursor = Windows.UI.Core.CoreCursor;
using static AuraEditor.Common.ControlHelper;

// 使用者控制項項目範本記載於 https://go.microsoft.com/fwlink/?LinkId=234236

namespace AuraEditor.UserControls
{
    public sealed partial class EffectLine : UserControl
    {
        public TimelineEffect MyEffect { get { return this.DataContext as TimelineEffect; } }

        public double X
        {
            get
            {
                CompositeTransform ct = this.RenderTransform as CompositeTransform;
                return ct.TranslateX;
            }
            set
            {
                CompositeTransform ct = this.RenderTransform as CompositeTransform;
                ct.TranslateX = value;
            }
        }
        public double Right { get { return X + Width; } }
        public bool IsSelected {
            set
            {
                if(StatusToggleButton.IsChecked != value)
                {
                    StatusToggleButton.IsChecked = value;
                }
            }
        }

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
                if (X + e.Delta.Translation.X < 0)
                    return;

                X += e.Delta.Translation.X;
            }
            else if (MouseState == CursorState.SizeRight)
            {
                if (e.Position.X <= 50)
                    return;

                Width = e.Position.X;
            }
            else if (MouseState == CursorState.SizeLeft)
            {
                double move = e.Delta.Translation.X;

                if (move < 0) // To left
                {
                    // We should check if it will overlap others
                    if (MyEffect.Layer.FindEffectByPosition(X + move) != null)
                        return;
                }

                if (Width - move <= 50)
                    return;

                X += move;
                Width -= move;
            }
        }
        void EffectLine_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            double keepWidth;

            if (MouseState == CursorState.SizeLeft)
            {
                keepWidth = Width;
                X = RoundToTens(X);
                Width = RoundToTens(keepWidth);
            }
            else if (MouseState == CursorState.SizeRight)
            {
                keepWidth = Width;
                Width = RoundToTens(keepWidth);
            }
            else // move
            {
                X = RoundToTens(X);
            }

            MyEffect.Layer.InsertEffectLine(MyEffect);
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
            MainPage.Self.SelectedEffectLine = MyEffect;
        }
    }
}
