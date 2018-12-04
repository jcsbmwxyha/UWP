using AuraEditor.Models;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
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
        }

        public void EnableManipulation()
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
        public void DisableManipulation()
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

        private void Device_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            SetPosition(
                TT.X + e.Delta.Translation.X / AuraSpaceManager.Self.SpaceZoomFactor,
                TT.Y + e.Delta.Translation.Y / AuraSpaceManager.Self.SpaceZoomFactor);

            //AuraSpaceManager.Self.OnDeviceMoved(this);
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

            //AuraSpaceManager.Self.OnDeviceMoveCompleted(this);
            DashRect.Opacity = 0.6;
            DeviceImage.Opacity = 1;
            MainPage.Self.NeedSave = true;
            Canvas.SetZIndex(this, 0);
        }
        private void Device_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _oldPixelPosition = new Point(TT.X, TT.Y);
            DashRect.Opacity = 1;
            DeviceImage.Opacity = 0.6;
        }
        private void Device_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            DashRect.Opacity = 0.6;
            DeviceImage.Opacity = 1;
        }
        private void Device_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor
                = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.SizeAll, 0);

            DashRect.Opacity = 0.6;
            DeviceImage.Opacity = 1;
        }
        private void Device_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor
                = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);

            DashRect.Opacity = 0;
            DeviceImage.Opacity = 1;
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
            AnimationStart(TT, "TranslateX", runTime, source, targetX);

            source = TT.Y;
            targetY = y;
            AnimationStart(TT, "TranslateY", runTime, source, targetY);
        }

    }
}
