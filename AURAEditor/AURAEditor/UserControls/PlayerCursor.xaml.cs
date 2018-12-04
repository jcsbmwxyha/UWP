using AuraEditor.Models;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using static AuraEditor.UserControls.EffectLine;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace AuraEditor.UserControls
{
    public sealed partial class PlayerCursor : UserControl
    {
        public PlayerModel m_Model { get { return this.DataContext as PlayerModel; } }

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

        public PlayerCursor()
        {
            this.InitializeComponent();
        }

        private void PlayerCursor_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            TranslateTransform tt = this.RenderTransform as TranslateTransform;
            if (tt.X + e.Delta.Translation.X < 0)
                tt.X = 0;
            else
                tt.X += e.Delta.Translation.X;
        }

        private void PlayerCursor_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            mouseState = CursorState.SizeAll;
        }

        private void PlayerCursor_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            mouseState = CursorState.None;
        }
    }
}
