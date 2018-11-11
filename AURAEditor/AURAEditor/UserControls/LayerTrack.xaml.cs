using System;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using static AuraEditor.Common.EffectHelper;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace AuraEditor.UserControls
{
    public sealed partial class LayerTrack : UserControl
    {
        private Layer m_Layer { get { return this.DataContext as Layer; } }

        public LayerTrack()
        {
            this.InitializeComponent();
        }
        private void Track_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
        }
        private async void Track_DragOver(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.Text))
            {
                e.DragUIOverride.IsCaptionVisible = false;
                e.DragUIOverride.IsGlyphVisible = false;
                var effectname = await e.DataView.GetTextAsync();

                if (!IsCommonEffect(effectname))
                    e.AcceptedOperation = DataPackageOperation.None;
                else
                {
                    e.AcceptedOperation = DataPackageOperation.Copy;
                }
            }
        }
        private async void Track_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.Text))
            {
                var effectname = await e.DataView.GetTextAsync();
                int type = GetEffectIndex(effectname);

                TimelineEffect effect = new TimelineEffect(this, type);
                m_Layer.AddTimelineEffect(effect);
                MainPage.Self.SelectedEffectLine = effect;
                MainPage.Self.NeedSave = true;
            }
        }

    }
}
