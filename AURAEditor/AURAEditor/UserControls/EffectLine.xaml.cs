using AuraEditor.Common;
using AuraEditor.Pages;
using AuraEditor.ViewModels;
using System;
using System.ComponentModel;
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
        public EffectLineViewModel elvm { get { return this.DataContext as EffectLineViewModel; } }

        private ScrollViewer m_ScrollViewer;
        private DispatcherTimer m_ScrollTimerClock;
        private double _tempSizeAllPosition;
        private double _oldValue;
        private bool _isPressed;
        private double ViewModelLeft
        {
            get
            {
                return elvm.Left;
            }
            set
            {
                elvm.Left = value;
            }
        }
        private double ViewModelWidth
        {
            get
            {
                return elvm.Width;
            }
            set
            {
                elvm.Width = value;
            }
        }
        private double Right { get { return ViewModelLeft + ViewModelWidth; } }
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
                    if (ViewModelLeft > 0)
                    {
                        double offset = ViewModelLeft - move;
                        if (offset < 0)
                        {
                            ViewModelLeft = 0;
                            ViewModelWidth += offset;
                        }
                        else
                        {

                            ViewModelLeft -= move;
                            ViewModelWidth += move;
                        }
                    }
                }
                else if (mouseState == CursorState.SizeAll)
                {
                    if (ViewModelLeft > 0)
                    {
                        double offset = ViewModelLeft - move;
                        if (offset < 0)
                            ViewModelLeft = 0;
                        else
                            ViewModelLeft -= move;
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
                    ViewModelWidth += move;
                }
                else if (mouseState == CursorState.SizeAll)
                {
                    ViewModelLeft += move;
                }
            }
        }
        private void EffectLine_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            PointerPoint ptrPt = e.GetCurrentPoint(this);
            Point position = e.GetCurrentPoint(this).Position;

            if (_isPressed)
            {
                // Getting ScrollViewer is speculative, but it do the trick.
                m_ScrollViewer = FindParentControl<ScrollViewer>(this, typeof(ScrollViewer));
                Point position2 = e.GetCurrentPoint(elvm.Layer.UI_Track).Position;


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

            if (position.X > ViewModelWidth - 5)
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
            this.DataContextChanged += (s, e) => Bindings.Update();
            this.DataContextChanged += (s, e) => elvm.MoveTo += MoveAnimation;

            m_ScrollTimerClock = new DispatcherTimer();
            m_ScrollTimerClock.Tick += Timer_Tick;
            m_ScrollTimerClock.Interval = new TimeSpan(0, 0, 0, 0, 5); // 10 ms
            _mouseDirection = 0;
            _isPressed = false;
        }

        private void MoveAnimation(double value)
        {
            AnimationStart(this, "MoveTo", 200, ViewModelLeft, value);
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
            {
                _tempSizeAllPosition = e.Position.X;
                _oldValue = ViewModelLeft;
            }
            else if (mouseState == CursorState.SizeLeft)
            {
                _oldValue = ViewModelLeft;
            }
            else if (mouseState == CursorState.SizeRight)
            {
                _oldValue = Width;
            }

            this.Opacity = 0.5;
            this.SetValue(Canvas.ZIndexProperty, 3);

            alignPositions = LayerPage.Self.GetAlignPositions(elvm);
        }
        private void EffectLine_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            double align = 0;

            if (mouseState == CursorState.SizeAll)
            {
                double tempLeft = ViewModelLeft + e.Position.X - _tempSizeAllPosition;

                if (tempLeft < 0)
                    return;

                // -- Try align --
                double tempRight = tempLeft + ViewModelWidth;

                if (GetAlignPosition(tempLeft, ref align)) // Align left
                {
                    ViewModelLeft = align;
                }
                else if (GetAlignPosition(tempRight, ref align)) // Align right
                {
                    ViewModelLeft = align - ViewModelWidth;
                }
                else
                {
                    ViewModelLeft += e.Position.X - _tempSizeAllPosition;
                }
            }
            else if (mouseState == CursorState.SizeRight)
            {
                if (e.Position.X <= 50)
                    return;

                // -- Try align --
                if (GetAlignPosition(ViewModelLeft + e.Position.X, ref align))
                {
                    ViewModelWidth = align - ViewModelLeft;
                }
                else
                {
                    ViewModelWidth = e.Position.X;
                }
            }
            else if (mouseState == CursorState.SizeLeft)
            {
                double move = e.Position.X;
                double tempRight = Right;

                if (move < 0) // To left
                {
                    if (ViewModelLeft <= 0)
                        return;

                    // Hit another
                    if (elvm.Layer.WhichIsOn(ViewModelLeft + move) != null)
                        return;
                }
                if (ViewModelWidth - move <= 50)
                    return;

                // -- Try align --
                if (GetAlignPosition(ViewModelLeft + e.Position.X, ref align))
                {
                    ViewModelLeft = align;
                    ViewModelWidth = tempRight - ViewModelLeft;
                }
                else
                {
                    ViewModelLeft += move;
                    ViewModelWidth -= move;
                }
            }

            LayerPage.Self.UpdateSupportLine(align);
        }
        private void EffectLine_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            _isPressed = false;
            m_ScrollTimerClock.Stop();

            LayerPage.Self.UpdateSupportLine(0);
            double result = elvm.Layer.MoveToFitPosition(elvm);
            NeedSave = true;
            this.Opacity = 1;
            this.SetValue(Canvas.ZIndexProperty, 0);

            if (mouseState == CursorState.SizeAll)
                ReUndoManager.GetInstance().Store(new MoveEffectCommand(elvm, _oldValue, result));
            else if (mouseState == CursorState.SizeLeft)
                ReUndoManager.GetInstance().Store(new WidthLeftEffectCommand(elvm, _oldValue, ViewModelLeft));
            else if (mouseState == CursorState.SizeRight)
                ReUndoManager.GetInstance().Store(new WidthRightEffectCommand(elvm, _oldValue, Width));

            mouseState = CursorState.None;
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

        private void EffectlineRadioButton_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            LayerPage.Self.CheckedEffect = elvm;
        }
        #endregion

        #region -- Right-clicked menu --
        private void CopyItem_Click(object sender, RoutedEventArgs e)
        {
            LayerPage.Self.CopiedEffect = EffectLineViewModel.Clone(elvm);
        }
        private void PasteItem_Click(object sender, RoutedEventArgs e)
        {
            if (LayerPage.Self.CopiedEffect == null)
                return;

            var copy = EffectLineViewModel.Clone(LayerPage.Self.CopiedEffect);
            copy.Left = this.Right;
            elvm.Layer.InsertTimelineEffectFitly(copy);
        }
        private void CutItem_Click(object sender, RoutedEventArgs e)
        {
            LayerPage.Self.CopiedEffect = elvm;
            elvm.Layer.DeleteEffectLine(elvm);
        }
        private void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            elvm.Layer.DeleteEffectLine(elvm);
        }
        #endregion

        public double MoveTo
        {
            get { return (double)GetValue(MoveToProperty); }
            set { SetValue(MoveToProperty, (double)value); }
        }

        public static readonly DependencyProperty MoveToProperty =
            DependencyProperty.Register("MoveTo", typeof(double), typeof(EffectLine),
                new PropertyMetadata(0, ScrollTimeLinePropertyChangedCallback));

        static private void ScrollTimeLinePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as EffectLine).ViewModelLeft = (double)e.NewValue;
        }

        public class MoveEffectCommand : IReUndoCommand
        {
            private EffectLineViewModel _elvm;
            private double _oldV;
            private double _newV;

            public MoveEffectCommand(EffectLineViewModel elvm, double oldV, double newV)
            {
                _elvm = elvm;
                _oldV = oldV;
                _newV = newV;
            }

            public void ExecuteRedo()
            {
                _elvm.Left = _newV;
            }
            public void ExecuteUndo()
            {
                _elvm.Left = _oldV;
            }
            public bool Conflict()
            {
                if (_elvm.Layer.GetFirstPilingEffect(_elvm) != null)
                    return true;
                else
                    return false;
            }
        }

        public class WidthRightEffectCommand : IReUndoCommand
        {
            private EffectLineViewModel _elvm;
            private double _oldV;
            private double _newV;

            public WidthRightEffectCommand(EffectLineViewModel elvm, double oldV, double newV)
            {
                _elvm = elvm;
                _oldV = oldV;
                _newV = newV;
            }

            public void ExecuteRedo()
            {
                _elvm.Width = _newV;
            }
            public void ExecuteUndo()
            {
                _elvm.Width = _oldV;
            }
        }

        public class WidthLeftEffectCommand : IReUndoCommand
        {
            private EffectLineViewModel _elvm;
            private double _oldV;
            private double _newV;

            public WidthLeftEffectCommand(EffectLineViewModel elvm, double oldV, double newV)
            {
                _elvm = elvm;
                _oldV = oldV;
                _newV = newV;
            }

            public void ExecuteRedo()
            {
                double diff = _oldV - _newV;
                _elvm.Left = _newV;
                _elvm.Width += diff;
            }
            public void ExecuteUndo()
            {
                double diff = _oldV - _newV;
                _elvm.Left = _oldV;
                _elvm.Width -= diff;
            }
        }
    }
}
