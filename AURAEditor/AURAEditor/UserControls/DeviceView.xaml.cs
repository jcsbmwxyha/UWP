using AuraEditor.Models;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using static AuraEditor.Common.ControlHelper;
using static AuraEditor.Common.Math2;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace AuraEditor.UserControls
{
    public sealed partial class DeviceView : UserControl
    {
        private DeviceModel m_DeviceModel { get { return this.DataContext as DeviceModel; } }
        private Point _oldPixelPosition;

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
            ManipulationDelta -= Device_ManipulationDelta;
            ManipulationCompleted -= Device_ManipulationCompleted;
            PointerPressed -= Device_PointerPressed;
            PointerReleased -= Device_PointerReleased;
            PointerEntered -= Device_PointerEntered;
            PointerExited -= Device_PointerExited;

            ManipulationDelta += Device_ManipulationDelta;
            ManipulationCompleted += Device_ManipulationCompleted;
            PointerPressed += Device_PointerPressed;
            PointerReleased += Device_PointerReleased;
            PointerEntered += Device_PointerEntered;
            PointerExited += Device_PointerExited;
        }
        private void DisableManipulation()
        {
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

        private void Device_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            SetPosition(
                TT.X + e.Delta.Translation.X / AuraSpaceManager.Self.SpaceZoomFactor,
                TT.Y + e.Delta.Translation.Y / AuraSpaceManager.Self.SpaceZoomFactor);

            Canvas.SetZIndex(this, 3);
        }
        private void Device_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            SetPosition(
                RoundToGrid(TT.X),
                RoundToGrid(TT.Y));

            if (!AuraSpaceManager.Self.IsPiling(m_DeviceModel))
            {
                AuraSpaceManager.Self.DeleteOverlappingTempDevice(m_DeviceModel);
                AuraSpaceManager.Self.MoveDeviceMousePosition(m_DeviceModel,
                    RoundToGrid(TT.X - _oldPixelPosition.X),
                    RoundToGrid(TT.Y - _oldPixelPosition.Y));
            }
            else
            {
                SetPositionByAnimation(_oldPixelPosition.X, _oldPixelPosition.Y);
            }

            MainPage.Self.NeedSave = true;
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
            AnimationStart(TT, "Y", runTime, source, targetY);
        }
    }
}
