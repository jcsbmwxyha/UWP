using System;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using static AuraEditor.Common.ControlHelper;
using CoreCursor = Windows.UI.Core.CoreCursor;

// 使用者控制項項目範本記載於 https://go.microsoft.com/fwlink/?LinkId=234236

namespace AuraEditor.UserControls
{
    public sealed partial class EffectLine : UserControl
    {
        public TimelineEffect MyEffect { get { return this.DataContext as TimelineEffect; } }

        private ScrollViewer m_ScrollViewer;
        private DispatcherTimer m_ScrollTimerClock;
        private double _allPosition;
        private bool _isPressed;

        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
           "TestIsChecked",
           typeof(bool),
           typeof(EffectLine),
           new PropertyMetadata(null, new PropertyChangedCallback(OnTestIsCheckedChanged))
        );

        private static void OnTestIsCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as EffectLine).EffectlineRadioButton.IsChecked = (bool)e.NewValue;
        }
        public bool TestIsChecked
        {
            get {
                return (bool)GetValue(LabelProperty);
            }
            set {
                SetValue(LabelProperty, value);
            }
        }

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
        //public bool IsChecked
        //{
        //    get
        //    {
        //        if (EffectlineRadioButton.IsChecked != true)
        //            return false;
        //        else
        //            return true;
        //    }
        //    set
        //    {
        //        if (EffectlineRadioButton.IsChecked != value)
        //        {
        //            EffectlineRadioButton.IsChecked = value;
        //        }
        //    }
        //}

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
                    if (X > 0)
                    {
                        double offset = X - move;
                        if (offset < 0)
                        {
                            X = 0;
                            Width += offset;
                        }
                        else
                        {

                            X -= move;
                            Width += move;
                        }
                    }
                }
                else if (mouseState == CursorState.SizeAll)
                {
                    if (X > 0)
                    {
                        double offset = X - move;
                        if (offset < 0)
                            X = 0;
                        else
                            X -= move;
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
                    X += move;
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

        #region event
        private void EffectLine_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            m_ScrollTimerClock.Start();
            _isPressed = true;
            if (mouseState == CursorState.SizeAll)
                _allPosition = e.Position.X;
            this.Opacity = 0.5;
            this.SetValue(Canvas.ZIndexProperty, 3);
        }
        private void EffectLine_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (mouseState == CursorState.SizeAll)
            {
                if (X + e.Position.X - _allPosition < 0)
                    return;

                X += e.Position.X - _allPosition;
            }
            else if (mouseState == CursorState.SizeRight)
            {
                if (e.Position.X <= 50)
                    return;

                Width = e.Position.X;
            }
            else if (mouseState == CursorState.SizeLeft)
            {
                double move = e.Position.X;

                if (move < 0) // To left
                {
                    if (X <= 0)
                        return;
                    // We should check if it will overlap another
                    if (MyEffect.Layer.WhichIsOn(X + move) != null)
                        return;
                }
                if (Width - move <= 50)
                    return;

                X += move;
                Width -= move;
            }
        }
        private async void EffectLine_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            double keepWidth;
            _isPressed = false;
            m_ScrollTimerClock.Stop();

            if (mouseState == CursorState.SizeLeft)
            {
                keepWidth = Width;
                X = RoundToTens(X);
                Width = RoundToTens(keepWidth);
            }
            else if (mouseState == CursorState.SizeRight)
            {
                keepWidth = Width;
                Width = RoundToTens(keepWidth);
            }
            else // move
            {
                X = RoundToTens(X);
            }

            await MyEffect.Layer.TryPlaceEffect(MyEffect);
            mouseState = CursorState.None;
            MainPage.Self.NeedSave = true;
            this.Opacity = 1;
            this.SetValue(Canvas.ZIndexProperty, 0);
        }
        private void EffectlineRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (MyEffect != null)
                AuraLayerManager.Self.CheckedLayer = MyEffect.Layer;
        }
        private void EffectLine_Click(object sender, RoutedEventArgs e)
        {
            AuraLayerManager.Self.CheckedEffect = MyEffect;
        }
        private void EffectlineRadioButton_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            AuraLayerManager.Self.CheckedEffect = MyEffect;
        }

        private void CopyItem_Click(object sender, RoutedEventArgs e)
        {
            AuraLayerManager.Self.CopiedEffect = TimelineEffect.CloneEffect(MyEffect);
        }
        private void PasteItem_Click(object sender, RoutedEventArgs e)
        {
            if (AuraLayerManager.Self.CopiedEffect == null)
                return;

            var copy = TimelineEffect.CloneEffect(AuraLayerManager.Self.CopiedEffect);
            copy.TestX = this.Right;
            MyEffect.Layer.AddAndInsertTimelineEffect(copy);
        }
        private void CutItem_Click(object sender, RoutedEventArgs e)
        {
            AuraLayerManager.Self.CopiedEffect = TimelineEffect.CloneEffect(MyEffect);
            MyEffect.Layer.DeleteEffectLine(this.DataContext as TimelineEffect);
        }
        private void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            MyEffect.Layer.DeleteEffectLine(this.DataContext as TimelineEffect);
        }
        #endregion
    }
}
