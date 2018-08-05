using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace AuraEditor.UserControls
{
    public sealed partial class EffectListItem : UserControl
    {
        public string MyText { get { return this.DataContext as string; } }

        public EffectListItem()
        {
            this.InitializeComponent();
        }

        private void Grid_DragStarting(UIElement sender, DragStartingEventArgs args)
        {
            var frame = (Frame)Window.Current.Content;
            var page = (MainPage)frame.Content;

            args.DragUI.SetContentFromBitmapImage(page.tempEffectUIBitmap);
            args.Data.RequestedOperation = DataPackageOperation.Copy;
            args.Data.SetText(MyText);

            page.UpdateSpaceGridOperations(SpaceStatus.DragingEffectListItem);
        }

        private void Grid_DropCompleted(UIElement sender, DropCompletedEventArgs args)
        {
            var frame = (Frame)Window.Current.Content;
            var page = (MainPage)frame.Content;
            
            page.UpdateSpaceGridOperations(SpaceStatus.Normal);
        }
    }
}
