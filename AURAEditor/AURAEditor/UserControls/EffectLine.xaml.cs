using AuraEditor.Common;
using AuraEditor.Pages;
using AuraEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using static AuraEditor.Common.ControlHelper;
using static AuraEditor.Common.Definitions;
using static AuraEditor.Common.EffectHelper;
using static AuraEditor.Common.Math2;
using static AuraEditor.Common.StorageHelper;
using CoreCursor = Windows.UI.Core.CoreCursor;

// 使用者控制項項目範本記載於 https://go.microsoft.com/fwlink/?LinkId=234236

namespace AuraEditor.UserControls
{
    public sealed partial class EffectLine : UserControl
    {
        private EffectLineViewModel elvm { get { return this.DataContext as EffectLineViewModel; } }

        private ScrollViewer m_ScrollViewer;
        private DispatcherTimer m_ScrollTimerClock;
        private double _pressedPosition;
        private double _undoValue;
        private bool _isPressed;
        private double _maxRight;

        private double EffectLeft
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
        private double EffectWidth
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
        private double EffectRight { get { return EffectLeft + EffectWidth; } }
        private double[] alignPositions;

        #region -- Intelligent auto scroll --
        private int _scrollDirection;
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
            if (_scrollDirection == 1)
            {
                if (m_ScrollViewer.HorizontalOffset == 0)
                    return;

                m_ScrollViewer.ChangeView(
                    m_ScrollViewer.HorizontalOffset - move,
                    m_ScrollViewer.VerticalOffset,
                    1, true);

                if (mouseState == CursorState.SizeLeft)
                {
                    if (EffectLeft > 0)
                    {
                        double offset = EffectLeft - move;
                        if (offset < 0)
                        {
                            EffectLeft = 0;
                            EffectWidth += offset;
                        }
                        else
                        {

                            EffectLeft -= move;
                            EffectWidth += move;
                        }
                    }
                }
                else if (mouseState == CursorState.SizeAll)
                {
                    if (EffectLeft > 0)
                    {
                        double offset = EffectLeft - move;
                        if (offset < 0)
                            EffectLeft = 0;
                        else
                            EffectLeft -= move;
                    }
                }

                LayerPage.Self.UpdateSupportLine(-1);
            }
            else if (_scrollDirection == 2)
            { }
            else if (_scrollDirection == 3)
            {
                m_ScrollViewer.ChangeView(
                    m_ScrollViewer.HorizontalOffset + move,
                    m_ScrollViewer.VerticalOffset,
                    1, true);

                if (EffectRight >= _maxRight)
                    return;

                if (mouseState == CursorState.SizeRight)
                {
                    EffectWidth += move;
                }
                else if (mouseState == CursorState.SizeAll)
                {
                    EffectLeft += move;
                }

                LayerPage.Self.UpdateSupportLine(-1);
            }
        }
        private void EffectLine_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (_isPressed)
            {
                // Getting ScrollViewer is speculative, but it do the trick.
                m_ScrollViewer = FindParentControl<ScrollViewer>(this, typeof(ScrollViewer));
                var track = FindParentControl<LayerTrack>(this, typeof(LayerTrack));
                Point positionInTrack = e.GetCurrentPoint(track).Position;

                Rect screenRect = new Rect(
                    m_ScrollViewer.HorizontalOffset,
                    m_ScrollViewer.VerticalOffset,
                    m_ScrollViewer.ActualWidth,
                    m_ScrollViewer.ActualHeight);

                if (positionInTrack.X > screenRect.Right - 100 && screenRect.Right != _maxRight)
                    _scrollDirection = 3;
                else if (positionInTrack.X < screenRect.Left)
                    _scrollDirection = 1;
                else
                    _scrollDirection = 2;
            }

            PointerPoint ptrPt = e.GetCurrentPoint(this);
            Point positionInEffectline = e.GetCurrentPoint(this).Position;

            if (ptrPt.Properties.IsLeftButtonPressed)
                return;

            if (positionInEffectline.X > EffectWidth - 5)
            {
                mouseState = CursorState.SizeRight;
            }
            else if (positionInEffectline.X < 5)
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

        #region -- Move to --
        public EffectLine()
        {
            this.InitializeComponent();
            this.DataContextChanged += (s, e) =>
            {
                Bindings.Update();

                // Do a trick to remove older handler
                elvm.ClearMoveToHandler();
                elvm.MoveTo += MoveAnimation;
            };

            m_ScrollTimerClock = new DispatcherTimer();
            m_ScrollTimerClock.Tick += Timer_Tick;
            m_ScrollTimerClock.Interval = new TimeSpan(0, 0, 0, 0, 5); // 10 ms
            _scrollDirection = 0;
            _isPressed = false;
        }
        private void EffectLine_Loaded(object sender, RoutedEventArgs e)
        {
            LoadedStoryboard.Begin();
        }
        private void EffectLine_Unloaded(object sender, RoutedEventArgs e)
        {
            Bindings.StopTracking();
        }

        private void MoveAnimation(double value)
        {
            AnimationStart(this, "MoveTo", 200, EffectLeft, value);
        }

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
            (d as EffectLine).EffectLeft = (double)e.NewValue;
        }
        #endregion

        #region -- Event --
        private void EffectLine_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            m_ScrollTimerClock.Start();
            _isPressed = true;
            _maxRight = LayerPage.MaxRightPixel;

            if (mouseState == CursorState.SizeAll)
            {
                _pressedPosition = e.Position.X;
                _undoValue = EffectLeft;
            }
            else if (mouseState == CursorState.SizeLeft)
            {
                _undoValue = EffectLeft;
            }
            else if (mouseState == CursorState.SizeRight)
            {
                _undoValue = EffectWidth;
            }

            this.Opacity = 0.5;
            var containter = FindParentDependencyObject(this);
            containter.SetValue(Canvas.ZIndexProperty, 3);

            alignPositions = LayerPage.Self.GetAlignPositions(elvm);
        }
        private void EffectLine_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            double align = 0;

            if (mouseState == CursorState.SizeAll)
            {
                double unAlignLeft = EffectLeft + e.Position.X - _pressedPosition;
                double unAlignRight = unAlignLeft + EffectWidth;

                if (unAlignLeft < 0 || unAlignRight > _maxRight)
                    return;

                // -- Try align --
                if (GetAlignPosition(unAlignLeft, ref align)) // Align left
                {
                    EffectLeft = align;
                }
                else if (GetAlignPosition(unAlignRight, ref align)) // Align right
                {
                    EffectLeft = align - EffectWidth;
                }
                else
                {
                    EffectLeft = unAlignLeft;
                }
            }
            else if (mouseState == CursorState.SizeRight)
            {
                double unAlignRight = EffectLeft + e.Position.X;

                if (e.Position.X <= 50 || unAlignRight > _maxRight)
                    return;

                // -- Try align --
                if (GetAlignPosition(EffectLeft + e.Position.X, ref align))
                {
                    EffectWidth = align - EffectLeft;
                }
                else
                {
                    EffectWidth = e.Position.X;
                }
            }
            else if (mouseState == CursorState.SizeLeft)
            {
                double move = e.Position.X;
                double fixedRight = EffectRight;

                if (move < 0) // To left
                {
                    if (EffectLeft <= 0)
                        return;

                    // Hit another
                    if (elvm.Layer.WhichIsOn(EffectLeft + move) != null)
                        return;
                }

                if (EffectWidth - move <= 50)
                    return;

                // -- Try align --
                if (GetAlignPosition(EffectLeft + e.Position.X, ref align))
                {
                    EffectLeft = align;
                    EffectWidth = fixedRight - EffectLeft;
                }
                else
                {
                    EffectLeft += move;
                    EffectWidth -= move;
                }
            }

            LayerPage.Self.UpdateSupportLine(align);
        }
        private void EffectLine_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            _isPressed = false;
            m_ScrollTimerClock.Stop();
            LayerPage.Self.UpdateSupportLine(-1);

            if (elvm.Layer.ExceedAfterApplyingEff(elvm))
            {
                if (mouseState == CursorState.SizeAll)
                {
                    EffectLeft = _undoValue;
                }
                else if (mouseState == CursorState.SizeLeft)
                {
                    double diff = _undoValue - EffectLeft;
                    EffectLeft = _undoValue;
                    EffectWidth -= diff;
                }
                else if (mouseState == CursorState.SizeRight)
                {
                    EffectWidth = _undoValue;
                }

                // TODO : Show Dialog
            }
            else
            {
                double result = elvm.Layer.ApplyEffect(elvm);
                NeedSave = true;

                var containter = FindParentDependencyObject(this);
                containter.SetValue(Canvas.ZIndexProperty, 0);

                if (mouseState == CursorState.SizeAll)
                    ReUndoManager.Store(new MoveEffectCommand(elvm, _undoValue, result));
                else if (mouseState == CursorState.SizeLeft)
                    ReUndoManager.Store(new WidthLeftEffectCommand(elvm, _undoValue, EffectLeft));
                else if (mouseState == CursorState.SizeRight)
                    ReUndoManager.Store(new WidthRightEffectCommand(elvm, _undoValue, EffectWidth));
            }

            mouseState = CursorState.None;
            this.Opacity = 1;
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

            result = -1;
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
            LayerPage.Self.CopiedEffect = new EffectLineViewModel(elvm);
        }
        private void PasteItem_Click(object sender, RoutedEventArgs e)
        {
            if (LayerPage.Self.CopiedEffect == null)
                return;

            var copy = new EffectLineViewModel(LayerPage.Self.CopiedEffect);
            copy.Left = this.EffectRight;

            if (elvm.Layer.TryInsertToTimelineFitly(copy))
            {
                // TODO 
            }
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
                LayerPage.Self.CheckedEffect = _elvm;
            }
            public void ExecuteUndo()
            {
                _elvm.Left = _oldV;
                LayerPage.Self.CheckedEffect = _elvm;
            }
            public bool Conflict()
            {
                if (_elvm.Layer.GetFirstIntersectingEffect(_elvm) != null)
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
                LayerPage.Self.CheckedEffect = _elvm;
            }
            public void ExecuteUndo()
            {
                _elvm.Width = _oldV;
                LayerPage.Self.CheckedEffect = _elvm;
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
                LayerPage.Self.CheckedEffect = _elvm;
            }
            public void ExecuteUndo()
            {
                double diff = _oldV - _newV;
                _elvm.Left = _oldV;
                _elvm.Width -= diff;
                LayerPage.Self.CheckedEffect = _elvm;
            }
        }
    }
}
