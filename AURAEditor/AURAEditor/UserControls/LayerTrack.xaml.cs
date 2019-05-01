using AuraEditor.Models;
using AuraEditor.Pages;
using AuraEditor.ViewModels;
using System;
using System.Linq;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using static AuraEditor.Common.EffectHelper;
using static AuraEditor.Common.Math2;
using static AuraEditor.Common.MetroEventSource;
using static AuraEditor.Common.StorageHelper;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace AuraEditor.UserControls
{
    public sealed partial class LayerTrack : UserControl
    {
        private LayerModel m_Layer { get { return this.DataContext as LayerModel; } }
        private double[] needAlignPositions;
        private double align;

        public LayerTrack()
        {
            this.InitializeComponent();
        }
        private void LayerTrack_Unloaded(object sender, RoutedEventArgs e)
        {
            Bindings.StopTracking();
        }

        private void Track_DragEnter(object sender, DragEventArgs e)
        {
            needAlignPositions = LayerPage.Self.GetAlignPositions(m_Layer);
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
            {
                e.AcceptedOperation = DataPackageOperation.Copy;

                // Try align
                var dragOverPosition = e.GetPosition(this);
                var actualDragOverX = dragOverPosition.X - EffectBlock.LastDraggingPoint.X;

                if (GetAlignPosition(actualDragOverX, ref align))
                    LayerPage.Self.UpdateSupportLine(align);
                else
                    LayerPage.Self.UpdateSupportLine(-1);
            }
        }
        private void Track_Drop(object sender, DragEventArgs e)
        {
            var pair = e.Data.Properties.FirstOrDefault();
            string effName = pair.Value as string;
            int type = Int32.Parse(effName);
            var dropPosition = e.GetPosition(this);
            var actualDropX = dropPosition.X - EffectBlock.LastDraggingPoint.X;
            EffectLineViewModel effect = new EffectLineViewModel(type);

            if (align > 0)
                effect.Left = align;
            else
                effect.Left = actualDropX >= 0 ? actualDropX : 0;

            m_Layer.InsertTimelineEffectFitly(effect);
            LayerPage.Self.CheckedEffect = effect;
            LayerPage.Self.UpdateSupportLine(-1);
            NeedSave = true;
            Log.Debug("[Track_Drop] " + m_Layer.Name + " was added effect : " + GetEffectName(Int32.Parse(effName)).ToString());
        }
        private bool GetAlignPosition(double p, ref double result)
        {
            int range = 8;
            foreach (var ap in needAlignPositions)
            {
                if (ap - range < p && p < ap + range)
                {
                    result = ap;
                    return true;
                }
            }

            // Align time scale
            double round_p = RoundToTarget(p, 100);
            if (Math.Abs(round_p - p) < range)
            {
                result = round_p;
                return true;
            }

            result = 0;
            return false;
        }

        private void Track_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            LayerPage.Self.CheckedLayer = m_Layer;

            var fe = sender as FrameworkElement;
        }
        private void Track_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (m_Layer != null)
            {
                if (LayerPage.Self.CheckedLayer == m_Layer)
                    m_Layer.VisualState = "CheckedPointerOver";
                else
                    m_Layer.VisualState = "PointerOver";
            }
        }
        private void Track_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (m_Layer != null)
            {
                if (LayerPage.Self.CheckedLayer == m_Layer)
                    m_Layer.VisualState = "Checked";
                else
                    m_Layer.VisualState = "Normal";
            }
          
        }
        private void PasteItem_Click(object sender, RoutedEventArgs e) 
        {
            var copy = LayerPage.Self.CopiedEffect;

            if (copy == null)
                return;

            m_Layer.InsertTimelineEffectFitly(new EffectLineViewModel(copy));
        }
    }
}
