using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using AuraEditor.Dialogs;
using static AuraEditor.Common.ControlHelper;
using static AuraEditor.AuraSpaceManager;

// 使用者控制項項目範本記載於 https://go.microsoft.com/fwlink/?LinkId=234236

namespace AuraEditor.UserControls
{
    public sealed partial class LayerTitle : UserControl
    {
        private Layer m_Layer { get { return this.DataContext as Layer; } }
        public bool IsChecked
        {
            get
            {
                if (LayerRadioButton.IsChecked == true)
                    return true;
                else
                    return false;
            }
            set
            {
                LayerRadioButton.IsChecked = value;
            }
        }

        public LayerTitle()
        {
            this.InitializeComponent();
            this.DataContextChanged += (s, e) => Bindings.Update();
        }
        public void Update()
        {
            Bindings.Update();
        }

        #region Framework element
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

            LayerRadioButton.IsChecked = true;
        }
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            LayerRadioButton.IsChecked = true;
            AuraSpaceManager.Self.ReEdit(m_Layer);
            MainPage.Self.ReEdit(m_Layer);
        }
        private async void TriggerDialogButton_Click(object sender, RoutedEventArgs e)
        {
            LayerRadioButton.IsChecked = true;

            ContentDialog triggerDialog = new TriggerDialog(m_Layer);
            await triggerDialog.ShowAsync();

            if (m_Layer.TriggerEffects.Count != 0)
            {
                m_Layer.UI_Background.GoToState("Trigger");
                VisualStateManager.GoToState(this, "Trigger", false);
            }
            else
            {
                m_Layer.UI_Background.GoToState("NoTrigger");
                VisualStateManager.GoToState(this, "NoTrigger", false);
            }

            MainPage.Self.NeedSave = true;
        }
        #endregion

        #region State change
        private void LayerRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (LayerRadioButton.IsChecked == true)
            {
                VisualStateManager.GoToState(this, "Checked", false);
                m_Layer.UI_Background.GoToState("Checked");
            }
            else
            {
                VisualStateManager.GoToState(this, "Normal", false);
                m_Layer.UI_Background.GoToState("Normal");
            }
            AuraSpaceManager.Self.WatchCurrentLayer();
        }
        private void LayerRadioButton_Unchecked(object sender, RoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "Normal", false);
            m_Layer.UI_Background.GoToState("Normal");
        }

        private void MyGrid_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            LayerRadioButton.IsChecked = true;
        }
        private void MyGrid_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (LayerRadioButton.IsChecked == true)
            {
                VisualStateManager.GoToState(this, "CheckedPointerOver", false);
                m_Layer.UI_Background.GoToState("CheckedPointerOver");
            }
            else
            {
                VisualStateManager.GoToState(this, "PointerOver", false);
                m_Layer.UI_Background.GoToState("PointerOver");
            }
        }
        private void MyGrid_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (LayerRadioButton.IsChecked == true)
            {
                VisualStateManager.GoToState(this, "Checked", false);
                m_Layer.UI_Background.GoToState("Checked");
            }
            else
            {
                VisualStateManager.GoToState(this, "Normal", false);
                m_Layer.UI_Background.GoToState("Normal");
            }
        }
        #endregion
    }
}
