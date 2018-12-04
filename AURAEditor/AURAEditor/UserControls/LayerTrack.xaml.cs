using System;
using System.Linq;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using static AuraEditor.Common.EffectHelper;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace AuraEditor.UserControls
{
    public sealed partial class LayerTrack : UserControl
    {
        private Layer m_Layer { get { return this.DataContext as Layer; } }
        private Point insertPosition;

        public LayerTrack()
        {
            this.InitializeComponent();
        }

        private void Track_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data == null)
                return;

            e.DragUIOverride.IsCaptionVisible = false;
            e.DragUIOverride.IsGlyphVisible = false;

            var pair = e.Data.Properties.FirstOrDefault();
            string effName = pair.Value as string;
            if (effName != null)
                e.AcceptedOperation = DataPackageOperation.Copy;
        }
        private void Track_Drop(object sender, DragEventArgs e)
        {
            var pair = e.Data.Properties.FirstOrDefault();
            string effName = pair.Value as string;
            int type = GetEffectIndex(effName);

            TimelineEffect effect = new TimelineEffect(type);
            effect.StartTime = m_Layer.GetFirstRoomPosition(1000); // 1s
            m_Layer.AddTimelineEffect(effect);
            AuraLayerManager.Self.CheckedEffect = effect;
            MainPage.Self.NeedSave = true;
        }

        private void Track_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            AuraLayerManager.Self.CheckedLayer = m_Layer;

            var fe = sender as FrameworkElement;
            insertPosition = e.GetCurrentPoint(fe).Position;
        }
        private void Track_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (AuraLayerManager.Self.CheckedLayer == m_Layer)
                m_Layer.VisualState = "CheckedPointerOver";
            else
                m_Layer.VisualState = "PointerOver";
        }
        private void Track_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (AuraLayerManager.Self.CheckedLayer == m_Layer)
                m_Layer.VisualState = "Checked";
            else
                m_Layer.VisualState = "Normal";
        }

        private void PasteItem_Click(object sender, RoutedEventArgs e)
        {
            var copy = AuraLayerManager.Self.CopiedEffect;

            if (copy == null)
                return;

            m_Layer.InsertTimelineEffectFitly(TimelineEffect.CloneEffect(copy));
        }
    }
}
