using AuraEditor.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using static AuraEditor.Common.EffectHelper;

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
            var page = MainPage.MainPageInstance;

            args.DragUI.SetContentFromBitmapImage(page.DragEffectIcon);
            args.Data.RequestedOperation = DataPackageOperation.Copy;
            args.Data.SetText(MyText);

            page.SetSpaceStatus(SpaceStatus.DragingEffectBlock);
        }

        private void Grid_DropCompleted(UIElement sender, DropCompletedEventArgs args)
        {
            MainPage.MainPageInstance.SetSpaceStatus(SpaceStatus.Normal);
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
