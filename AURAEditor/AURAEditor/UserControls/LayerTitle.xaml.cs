using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using AuraEditor.Dialogs;
using static AuraEditor.Common.ControlHelper;

// 使用者控制項項目範本記載於 https://go.microsoft.com/fwlink/?LinkId=234236

namespace AuraEditor.UserControls
{
    public sealed partial class LayerTitle : UserControl
    {
        private Layer m_Layer { get { return this.DataContext as Layer; } }

        public LayerTitle()
        {
            this.InitializeComponent();
            this.DataContextChanged += (s, e) => Bindings.Update();
        }
        public void Update()
        {
            Bindings.Update();
        }

        private void EyeToggleButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton tb = FindControl<ToggleButton>(this, typeof(ToggleButton), "EyeToggleButton");

            if (tb != null)
            {
                if (tb.IsChecked == false)
                    m_Layer.Eye = false;
                else
                    m_Layer.Eye = true;
            }

            AuraLayerManager.Self.CheckedLayer = m_Layer;
        }
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            AuraLayerManager.Self.CheckedLayer = m_Layer;
            AuraSpaceManager.Self.ReEditZones_Start(m_Layer);
            MainPage.Self.ShowReEditMask(m_Layer);
        }
        private async void TriggerDialogButton_Click(object sender, RoutedEventArgs e)
        {
            AuraLayerManager.Self.CheckedLayer = m_Layer;

            ContentDialog triggerDialog = new TriggerDialog(m_Layer);
            await triggerDialog.ShowAsync();

            if (m_Layer.TriggerEffects.Count != 0)
            {
                m_Layer.UI_Background.GoToState("Trigger");
                m_Layer.IsTriggering = true;
            }
            else
            {
                m_Layer.UI_Background.GoToState("NoTrigger");
                m_Layer.IsTriggering = false;
            }

            Bindings.Update();
            MainPage.Self.NeedSave = true;
        }

        private void MyGrid_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            AuraLayerManager.Self.CheckedLayer = m_Layer;
        }
        private void MyGrid_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (AuraLayerManager.Self.CheckedLayer == m_Layer)
                m_Layer.UI_Background.GoToState("CheckedPointerOver");
            else
                m_Layer.UI_Background.GoToState("PointerOver");
        }
        private void MyGrid_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (AuraLayerManager.Self.CheckedLayer == m_Layer)
                m_Layer.UI_Background.GoToState("Checked");
            else
                m_Layer.UI_Background.GoToState("Normal");
        }
    }
}
