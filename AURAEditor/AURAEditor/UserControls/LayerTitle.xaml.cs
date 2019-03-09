using AuraEditor.Common;
using AuraEditor.Dialogs;
using AuraEditor.Models;
using AuraEditor.Pages;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using static AuraEditor.Common.StorageHelper;

// 使用者控制項項目範本記載於 https://go.microsoft.com/fwlink/?LinkId=234236

namespace AuraEditor.UserControls
{
    public sealed partial class LayerTitle : UserControl
    {
        private LayerModel m_Layer { get { return this.DataContext as LayerModel; } }

        public LayerTitle()
        {
            this.InitializeComponent();
            //this.DataContextChanged += (s, e) => Bindings.Update();
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
        private void NameTextBlock_DoubleTapped(object sender, Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            NameTextBox.Text = "";
            NameTextBlock.Visibility = Visibility.Collapsed;
            NameTextBox.Visibility = Visibility.Visible;
            NameTextBox.Focus(FocusState.Programmatic);
        }
        private void NameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (NameTextBox.Text.Replace(" ", "") != "")
            {
                ReUndoManager.Store(new RenameCommand(m_Layer, NameTextBlock.Text, NameTextBox.Text));
                NameTextBlock.Text = NameTextBox.Text;
            }
            
            NameTextBlock.Visibility = Visibility.Visible;
            NameTextBox.Visibility = Visibility.Collapsed;
        }
        private void NameTextBox_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                SpacePage.Self.SpaceScrollViewer.Focus(FocusState.Programmatic);
            }
        }

        public class RenameCommand : IReUndoCommand
        {
            private LayerModel _layer;
            private string _oldName;
            private string _newName;

            public RenameCommand(LayerModel layer, string oldName, string newName)
            {
                _layer = layer;
                _oldName = oldName;
                _newName = newName;
            }

            public void ExecuteRedo()
            {
                _layer.Name = _newName;
                LayerPage.Self.CheckedLayer = _layer;
            }
            public void ExecuteUndo()
            {
                _layer.Name = _oldName;
                LayerPage.Self.CheckedLayer = _layer;
            }
        }
        #endregion
    }
}
