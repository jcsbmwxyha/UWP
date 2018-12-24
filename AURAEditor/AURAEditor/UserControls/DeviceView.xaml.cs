using System;
using AuraEditor.Models;
using AuraEditor.Pages;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using static AuraEditor.Common.ControlHelper;
using static AuraEditor.Common.Math2;
using static AuraEditor.Common.StorageHelper;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace AuraEditor.UserControls
{
    public sealed partial class DeviceView : UserControl
    {
        private DeviceModel m_DeviceModel { get { return this.DataContext as DeviceModel; } }
        private Point _oldPixelPosition;
        private Point _pressedPosition;

        public DeviceView()
        {
            this.InitializeComponent();
            this.DataContextChanged += (s, e) => Bindings.Update();

            BindingOperations.SetBinding(this, OperationEnabledProperty,
                    new Binding
                    {
                        Path = new PropertyPath("OperationEnabled"),
                        Mode = BindingMode.OneWay
                    });
        }

        #region -- OperationEnabled --
        public bool OperationEnabled
        {
            get { return (bool)GetValue(OperationEnabledProperty); }
            set { SetValue(OperationEnabledProperty, (bool)value); }
        }

        public static readonly DependencyProperty OperationEnabledProperty =
            DependencyProperty.Register("OperationEnabled", typeof(bool), typeof(DeviceView),
                new PropertyMetadata(false, OperationEnabledChangedCallback));

        static private void OperationEnabledChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == true)
                (d as DeviceView).EnableManipulation();
            else
                (d as DeviceView).DisableManipulation();
        }

        private void EnableManipulation()
        {
            ManipulationStarted -= Device_ManipulationStarted;
            ManipulationDelta -= Device_ManipulationDelta;
            ManipulationCompleted -= Device_ManipulationCompleted;
            PointerPressed -= Device_PointerPressed;
            PointerReleased -= Device_PointerReleased;
            PointerEntered -= Device_PointerEntered;
            PointerExited -= Device_PointerExited;

            ManipulationStarted += Device_ManipulationStarted;
            ManipulationDelta += Device_ManipulationDelta;
            ManipulationCompleted += Device_ManipulationCompleted;
            PointerPressed += Device_PointerPressed;
            PointerReleased += Device_PointerReleased;
            PointerEntered += Device_PointerEntered;
            PointerExited += Device_PointerExited;
        }
        private void DisableManipulation()
        {
            ManipulationStarted -= Device_ManipulationStarted;
            ManipulationDelta -= Device_ManipulationDelta;
            ManipulationCompleted -= Device_ManipulationCompleted;
            PointerPressed -= Device_PointerPressed;
            PointerReleased -= Device_PointerReleased;
            PointerEntered -= Device_PointerEntered;
            PointerExited -= Device_PointerExited;
            DashRect.Opacity = 0;
            DeviceImage.Opacity = 1;
        }
        #endregion

        private void Device_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            _pressedPosition = e.Position;
            Canvas.SetZIndex(this, 3);
        }

        private void Device_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var rb_Point = SpacePage.Self.GetCanvasRightBottomPoint();

            var deltaX = e.Position.X - _pressedPosition.X;
            var deltaY = e.Position.Y - _pressedPosition.Y;

            deltaX = (TT.X + deltaX < 0) ? 0 : deltaX;
            deltaY = (TT.Y + deltaY < 0) ? 0 : deltaY;
            deltaX = (TT.X + DashRect.Width + deltaX > rb_Point.X) ? 0 : deltaX;
            deltaY = (TT.Y + DashRect.Height + deltaX > rb_Point.Y) ? 0 : deltaY;

            SetPosition(
                TT.X + deltaX,
                TT.Y + deltaY);
        }
        private void Device_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            var point = SpacePage.Self.GetCanvasRightBottomPoint();

            if (TT.X < 0) TT.X = 0;
            if (TT.Y < 0) TT.Y = 0;
            if (TT.X + DashRect.Width > point.X) TT.X = point.X - DashRect.Width;
            if (TT.Y + DashRect.Height > point.Y) TT.Y = point.Y - DashRect.Height;

            SetPosition(
                RoundToGrid(TT.X),
                RoundToGrid(TT.Y));

            SpacePage.Self.OnDeviceMoveCompleted();

            if (!SpacePage.Self.IsPiling(m_DeviceModel))
            {
                SpacePage.Self.DeleteOverlappingTempDevice(m_DeviceModel);
                SpacePage.Self.MoveDeviceMousePosition(m_DeviceModel,
                    RoundToGrid(TT.X - _oldPixelPosition.X),
                    RoundToGrid(TT.Y - _oldPixelPosition.Y));
            }
            else
            {
                SetPositionByAnimation(_oldPixelPosition.X, _oldPixelPosition.Y);
            }

            NeedSave = true;
            Canvas.SetZIndex(this, 0);
            VisualStateManager.GoToState(this, "Hover", false);
        }
        private void Device_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _oldPixelPosition = new Point(TT.X, TT.Y);
            VisualStateManager.GoToState(this, "Pressed", false);
        }
        private void Device_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "Hover", false);
        }
        private void Device_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor
                = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.SizeAll, 0);

            VisualStateManager.GoToState(this, "Hover", false);
        }
        private void Device_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor
                = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);

            VisualStateManager.GoToState(this, "Normal", false);
        }

        private void SetPosition(double x, double y)
        {
            TT.X = x;
            TT.Y = y;
        }
        private void SetPositionByAnimation(double x, double y)
        {
            double runTime = 300;
            double source;
            double targetX;
            double targetY;

            source = TT.X;
            targetX = x;
            AnimationStart(TT, "X", runTime, source, targetX);

            source = TT.Y;
            targetY = y;
            Storyboard sb = AnimationStart(TT, "Y", runTime, source, targetY);
            sb.Completed += MovedCompleted;
        }

        private void MovedCompleted(object sender, object e)
        {
            SpacePage.Self.OnDeviceMoveCompleted();
        }
    }
}
