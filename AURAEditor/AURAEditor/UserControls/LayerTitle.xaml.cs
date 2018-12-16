using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using AuraEditor.Dialogs;
using AuraEditor.Pages;
using static AuraEditor.Common.StorageHelper;

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

        private void EyeToggleButton_Click(object sender, RoutedEventArgs e)
        {
            LayerPage.Self.CheckedLayer = m_Layer;
        }
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            LayerPage.Self.CheckedLayer = m_Layer;
            SpacePage.Self.ReEditZones_Start(m_Layer);
            MainPage.Self.ShowReEditMask(m_Layer);
        }
        private async void TriggerDialogButton_Click(object sender, RoutedEventArgs e)
        {
            LayerPage.Self.CheckedLayer = m_Layer;

            ContentDialog triggerDialog = new TriggerDialog(m_Layer);
            await triggerDialog.ShowAsync();

            if (m_Layer.TriggerEffects.Count != 0)
                m_Layer.IsTriggering = true;
            else
                m_Layer.IsTriggering = false;

            NeedSave = true;
        }

        private void MyGrid_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            LayerPage.Self.CheckedLayer = m_Layer;
        }
        private void MyGrid_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (LayerPage.Self.CheckedLayer == m_Layer)
                m_Layer.VisualState = "CheckedPointerOver";
            else
                m_Layer.VisualState = "PointerOver";
        }
        private void MyGrid_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (LayerPage.Self.CheckedLayer == m_Layer)
                m_Layer.VisualState = "Checked";
            else
                m_Layer.VisualState = "Normal";
        }

        #region -- Layer Name --
        private void EditNameButton_Click(object sender, RoutedEventArgs e)
        {
            EditNameButton.Visibility = Visibility.Collapsed;
            NameTextBlock.Visibility = Visibility.Collapsed;
            NameTextBox.Visibility = Visibility.Visible;

            NameTextBox.Text = NameTextBlock.Text;
            NameTextBox.Focus(FocusState.Programmatic);
        }
        private void NameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            NameTextBlock.Text = NameTextBox.Text;
            EditNameButton.Visibility = Visibility.Visible;
            NameTextBlock.Visibility = Visibility.Visible;
            NameTextBox.Visibility = Visibility.Collapsed;
        }
        private void NameTextBox_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                if (NameTextBox.Text != "")
                {
                    SpacePage.Self.SpaceScrollViewer.Focus(FocusState.Programmatic);
                }
            }
        }
        #endregion
    }
}
