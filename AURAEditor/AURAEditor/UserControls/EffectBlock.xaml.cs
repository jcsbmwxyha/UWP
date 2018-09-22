using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using static AuraEditor.AuraSpaceManager;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace AuraEditor.UserControls
{
    public sealed partial class EffectBlock : UserControl
    {
        public string MyText { get { return this.DataContext as string; } }

        public EffectBlock()
        {
            this.InitializeComponent();
        }

        private void Grid_DragStarting(UIElement sender, DragStartingEventArgs args)
        {
            var page = MainPage.Self;

            args.DragUI.SetContentFromBitmapImage(page.DragEffectIcon);
            args.Data.RequestedOperation = DataPackageOperation.Copy;
            args.Data.SetText(MyText);

            AuraSpaceManager.Self.SetSpaceStatus(SpaceStatus.DragingEffectBlock);
        }
        private void Grid_DropCompleted(UIElement sender, DropCompletedEventArgs args)
        {
            AuraSpaceManager.Self.SetSpaceStatus(SpaceStatus.Normal);
        }
        private void Grid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "PointerOver", false);
        }
        private void Grid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "Normal", false);
        }
        private void Grid_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "Pressed", false);
        }
        private void Grid_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "PointerOver", false);
        }
    }
}
