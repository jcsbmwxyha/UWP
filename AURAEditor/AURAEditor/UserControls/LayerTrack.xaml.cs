using System;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
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

        public void AddEffectline(EffectLine el)
        {
            Track.Children.Add(el);
        }
        public void RemoveEffectline(EffectLine el)
        {
            Track.Children.Remove(el);
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

                TimelineEffect effect = new TimelineEffect(type);
                m_Layer.AddTimelineEffect(effect);
                effect.UI.IsChecked = true;
                MainPage.Self.NeedSave = true;
            }
        }

        private void Track_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            m_Layer.IsChecked = true;
        }
        private void Track_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (m_Layer.IsChecked == true)
                m_Layer.UI_Background.GoToState("CheckedPointerOver");
            else
                m_Layer.UI_Background.GoToState("PointerOver");
        }
        private void Track_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (m_Layer.IsChecked == true)
                m_Layer.UI_Background.GoToState("Checked");
            else
                m_Layer.UI_Background.GoToState("Normal");
        }

        private void PasteItem_Click(object sender, RoutedEventArgs e)
        {
            var copy = AuraLayerManager.Self.CopiedEffect;

            if (copy == null)
                return;

            m_Layer.AddTimelineEffect(TimelineEffect.CloneEffect(copy));
        }
    }
}
