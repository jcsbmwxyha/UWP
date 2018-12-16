using AuraEditor.Pages;
using System;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using static AuraEditor.Common.ControlHelper;
using static AuraEditor.Common.Math2;
using static AuraEditor.Common.StorageHelper;
using CoreCursor = Windows.UI.Core.CoreCursor;

// 使用者控制項項目範本記載於 https://go.microsoft.com/fwlink/?LinkId=234236

namespace AuraEditor.UserControls
{
    public sealed partial class EffectLine : UserControl
    {
        public TimelineEffect MyEffect { get { return this.DataContext as TimelineEffect; } }

        private ScrollViewer m_ScrollViewer;
        private DispatcherTimer m_ScrollTimerClock;
        private double _tempSizeAllPosition;
        private bool _isPressed;
        private double Left
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
        private double Right { get { return Left + Width; } }
        private double[] alignPositions;

        #region Intelligent auto scroll
        private int _mouseDirection;
        public enum CursorState
        {
            None = 0,
            SizeAll = 1,
            SizeLeft = 2,
            SizeRight = 3,
        }
        private CursorState _mouseState;
        private CursorState mouseState
        {
            get
            {
                return _mouseState;
            }
            set
            {
                if (_mouseState != value)
                {
                    if (value == CursorState.None)
                        Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 0);
                    else if (value == CursorState.SizeAll)
                        Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.SizeAll, 0);
                    else if (value == CursorState.SizeLeft || value == CursorState.SizeRight)
                        Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.SizeWestEast, 0);

                    _mouseState = value;
                }
            }
        }
        private void Timer_Tick(object sender, object e)
        {
            int move = 10;
            if (_mouseDirection == 1)
            {
                if (m_ScrollViewer.HorizontalOffset == 0)
                    return;

                m_ScrollViewer.ChangeView(
                    m_ScrollViewer.HorizontalOffset - move,
                    m_ScrollViewer.VerticalOffset,
                    1, true);

                if (mouseState == CursorState.SizeLeft)
                {
                    if (Left > 0)
                    {
                        double offset = Left - move;
                        if (offset < 0)
                        {
                            Left = 0;
                            Width += offset;
                        }
                        else
                        {

                            Left -= move;
                            Width += move;
                        }
                    }
                }
                else if (mouseState == CursorState.SizeAll)
                {
                    if (Left > 0)
                    {
                        double offset = Left - move;
                        if (offset < 0)
                            Left = 0;
                        else
                            Left -= move;
                    }
                }
            }
            else if (_mouseDirection == 2)
            { }
            else if (_mouseDirection == 3)
            {
                m_ScrollViewer.ChangeView(
                    m_ScrollViewer.HorizontalOffset + move,
                    m_ScrollViewer.VerticalOffset,
                    1, true);

                if (mouseState == CursorState.SizeRight)
                {
                    Width += move;
                }
                else if (mouseState == CursorState.SizeAll)
                {
                    Left += move;
                }
            }
        }
        private void EffectLine_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            FrameworkElement fe = sender as FrameworkElement;
            EffectLine el = fe.Parent as EffectLine;
            PointerPoint ptrPt = e.GetCurrentPoint(el);
            Point position = e.GetCurrentPoint(el).Position;

            if (_isPressed)
            {
                // Getting ScrollViewer is speculative, but it do the trick.
                m_ScrollViewer = FindParentControl<ScrollViewer>(MyEffect.Layer.UI_Track, typeof(ScrollViewer));
                Point position2 = e.GetCurrentPoint(MyEffect.Layer.UI_Track).Position;

                Rect screenRect = new Rect(
                    m_ScrollViewer.HorizontalOffset,
                    m_ScrollViewer.VerticalOffset,
                    m_ScrollViewer.ActualWidth,
                    m_ScrollViewer.ActualHeight);

                if (position2.X > screenRect.Right - 100)
                    _mouseDirection = 3;
                else if (position2.X < screenRect.Left)
                    _mouseDirection = 1;
                else
                    _mouseDirection = 2;
            }

            if (ptrPt.Properties.IsLeftButtonPressed)
                return;

            if (position.X > el.Width - 5)
            {
                mouseState = CursorState.SizeRight;
            }
            else if (position.X < 5)
            {
                mouseState = CursorState.SizeLeft;
            }
            else
            {
                mouseState = CursorState.SizeAll;
            }
        }
        private void EffectLine_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            FrameworkElement fe = sender as FrameworkElement;
            EffectLine el = fe.Parent as EffectLine;
            PointerPoint ptrPt = e.GetCurrentPoint(el);

            // ManipulationCompleted will handle it if the mouse is pressed
            if (!ptrPt.Properties.IsLeftButtonPressed)
                mouseState = CursorState.None;
        }
        #endregion

        public EffectLine()
        {
            this.InitializeComponent();

            m_ScrollTimerClock = new DispatcherTimer();
            m_ScrollTimerClock.Tick += Timer_Tick;
            m_ScrollTimerClock.Interval = new TimeSpan(0, 0, 0, 0, 5); // 10 ms
            _mouseDirection = 0;
            _isPressed = false;
        }
        private void EffectLine_Loaded(object sender, RoutedEventArgs e)
        {
            LoadedStoryboard.Begin();
        }

        #region -- Event --
        private void EffectLine_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            m_ScrollTimerClock.Start();
            _isPressed = true;

            if (mouseState == CursorState.SizeAll)
                _tempSizeAllPosition = e.Position.X;

            this.Opacity = 0.5;
            this.SetValue(Canvas.ZIndexProperty, 3);

            alignPositions = LayerPage.Self.GetAlignPositions(MyEffect);
        }
        private void EffectLine_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            double align = 0;

            if (mouseState == CursorState.SizeAll)
            {
                double tempLeft = Left + e.Position.X - _tempSizeAllPosition;

                if (tempLeft < 0)
                    return;

                // -- Try align --
                double tempRight = tempLeft + Width;

                if (GetAlignPosition(tempLeft, ref align)) // Align left
                {
                    Left = align;
                }
                else if (GetAlignPosition(tempRight, ref align)) // Align right
                {
                    Left = align - Width;
                }
                else
                {
                    Left += e.Position.X - _tempSizeAllPosition;
                }
            }
            else if (mouseState == CursorState.SizeRight)
            {
                if (e.Position.X <= 50)
                    return;

                // -- Try align --
                if (GetAlignPosition(Left + e.Position.X, ref align))
                {
                    Width = align - Left;
                }
                else
                {
                    Width = e.Position.X;
                }
            }
            else if (mouseState == CursorState.SizeLeft)
            {
                double move = e.Position.X;
                double tempRight = Right;

                if (move < 0) // To left
                {
                    if (Left <= 0)
                        return;

                    // Hit another
                    if (MyEffect.Layer.WhichIsOn(Left + move) != null)
                        return;
                }
                if (Width - move <= 50)
                    return;

                // -- Try align --
                if (GetAlignPosition(Left + e.Position.X, ref align))
                {
                    Left = align;
                    Width = tempRight - Left;
                }
                else
                {
                    Left += move;
                    Width -= move;
                }
            }

            LayerPage.Self.UpdateSupportLine(align);
        }
        private void EffectLine_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            _isPressed = false;
            m_ScrollTimerClock.Stop();

            LayerPage.Self.UpdateSupportLine(0);
            MyEffect.Layer.MoveToFitPosition(MyEffect);
            mouseState = CursorState.None;
            NeedSave = true;
            this.Opacity = 1;
            this.SetValue(Canvas.ZIndexProperty, 0);
        }
        private bool GetAlignPosition(double p, ref double result)
        {
            int range = 8;
            foreach (var ap in alignPositions)
            {
                if (ap - range < p && p < ap + range)
                {
                    result = ap;
                    return true;
                }
            }

            // Align time scale
            double round_p = RoundToTarget(p, 100);
            if (Math.Abs(round_p - p) < range)
            {
                result = round_p;
                return true;
            }

            return false;
        }

        private void EffectlineRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (MyEffect != null)
                LayerPage.Self.CheckedLayer = MyEffect.Layer;
        }
        private void EffectlineRadioButton_Unchecked(object sender, RoutedEventArgs e)
        {
        }
        private void EffectLine_Click(object sender, RoutedEventArgs e)
        {
            LayerPage.Self.CheckedEffect = MyEffect;
        }
        private void EffectlineRadioButton_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            LayerPage.Self.CheckedEffect = MyEffect;
        }
        #endregion

        #region -- Right-clicked menu --
        private void CopyItem_Click(object sender, RoutedEventArgs e)
        {
            LayerPage.Self.CopiedEffect = TimelineEffect.CloneEffect(MyEffect);
        }
        private void PasteItem_Click(object sender, RoutedEventArgs e)
        {
            if (LayerPage.Self.CopiedEffect == null)
                return;

            var copy = TimelineEffect.CloneEffect(LayerPage.Self.CopiedEffect);
            copy.Left = this.Right;
            MyEffect.Layer.InsertTimelineEffectFitly(copy);
        }
        private void CutItem_Click(object sender, RoutedEventArgs e)
        {
            LayerPage.Self.CopiedEffect = TimelineEffect.CloneEffect(MyEffect);
            MyEffect.Layer.DeleteEffectLine(this.DataContext as TimelineEffect);
        }
        private void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            MyEffect.Layer.DeleteEffectLine(this.DataContext as TimelineEffect);
        }
        #endregion
    }
}
