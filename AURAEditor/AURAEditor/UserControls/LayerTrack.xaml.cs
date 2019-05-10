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
        private EffectLineViewModel draggingEff;
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
            var pair = e.Data.Properties.FirstOrDefault();
            if (e.Data == null || !(pair.Value is int))
                return;

            needAlignPositions = LayerPage.Self.GetAlignPositions(m_Layer);

            int idx = (int)pair.Value;
            draggingEff = new EffectLineViewModel(idx);
        }
        private void Track_DragOver(object sender, DragEventArgs e)
        {
            var pair = e.Data.Properties.FirstOrDefault();
            if (e.Data == null || !(pair.Value is int))
                return;

            if (draggingEff == null)
                return;

            // Try align ++
            var pointerPosition = e.GetPosition(this);
            var dropPosition = pointerPosition.X - EffectBlock.LastDraggingPoint.X;

            if (GetAlignPosition(dropPosition, ref align))
                LayerPage.Self.UpdateSupportLine(align);
            else
                LayerPage.Self.UpdateSupportLine(-1);

            if (align > 0)
                draggingEff.Left = align;
            else
                draggingEff.Left = dropPosition >= 0 ? dropPosition : 0;
            // Try align --

            if (draggingEff.Right > LayerPage.MaxRightPixel || m_Layer.ExceedIfApplyingEff(draggingEff))
            {
                e.DragUIOverride.IsCaptionVisible = true;
                e.DragUIOverride.Caption = "Exceed!";
                e.AcceptedOperation = DataPackageOperation.None;
            }
            else
            {
                e.DragUIOverride.IsCaptionVisible = false;
                e.DragUIOverride.IsGlyphVisible = false;
                e.AcceptedOperation = DataPackageOperation.Copy;
            }
        }
        private void Track_Drop(object sender, DragEventArgs e)
        {
            if (!m_Layer.TryInsertToTimelineFitly(draggingEff))
                return;

            LayerPage.Self.CheckedEffect = draggingEff;
            LayerPage.Self.UpdateSupportLine(-1);
            Log.Debug("[Track_Drop] Drop position : " + draggingEff.Left);
            draggingEff = null;
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

            result = -1;
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

            copy.Left = 0;

            if (!m_Layer.TryInsertToTimelineFitly(new EffectLineViewModel(copy)))
            {
                // TODO
            }
        }
    }
}
