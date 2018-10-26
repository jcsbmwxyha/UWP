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
    public sealed partial class DeviceLayerItem : UserControl
    {
        private DeviceLayer m_DeviceLayer { get { return this.DataContext as DeviceLayer; } }
        public bool IsChecked
        {
            get
            {
                if (DeviceLayerRadioButton.IsChecked == true)
                    return true;
                else
                    return false;
            }
            set
            {
                DeviceLayerRadioButton.IsChecked = value;
            }
        }

        public DeviceLayerItem()
        {
            this.InitializeComponent();
            //this.DataContextChanged += (s, e) => Bindings.Update();
        }

        private void EyeToggleButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton tb = FindControl<ToggleButton>(this, typeof(ToggleButton), "EyeToggleButton");

            if (tb != null)
            {
                if (tb.IsChecked == false)
                    m_DeviceLayer.Eye = false;
                else
                    m_DeviceLayer.Eye = true;
            }

            SelectMe();
        }
        private async void TriggerDialogButton_Click(object sender, RoutedEventArgs e)
        {
            SelectMe();
            ContentDialog triggerDialog = new TriggerDialog(m_DeviceLayer);
            await triggerDialog.ShowAsync();

            if (m_DeviceLayer.TriggerEffects.Count != 0)
            {
                m_DeviceLayer.UICanvas.GoToState("Trigger");
                VisualStateManager.GoToState(DeviceLayerRadioButton, "Trigger", false);
            }
            else
            {
                m_DeviceLayer.UICanvas.GoToState("NoTrigger");
                VisualStateManager.GoToState(DeviceLayerRadioButton, "NoTrigger", false);
            }

            MainPage.Self.NeedSave = true;
        }
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            SelectMe();
            AuraSpaceManager.Self.ReEdit(m_DeviceLayer);
            MainPage.Self.ReEdit(m_DeviceLayer);
        }

        #region State change
        private void DeviceLayerRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (DeviceLayerRadioButton.IsChecked == true)
            {
                m_DeviceLayer.UICanvas.GoToState("Checked");
            }
            else
            {
                m_DeviceLayer.UICanvas.GoToState("Normal");
            }
            AuraSpaceManager.Self.WatchCurrentLayer();
        }
        private void DeviceLayerRadioButton_Unchecked(object sender, RoutedEventArgs e)
        {
            m_DeviceLayer.UICanvas.GoToState("Normal");
        }
        private void DeviceLayerRadioButton_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (DeviceLayerRadioButton.IsChecked == true)
            {
                m_DeviceLayer.UICanvas.GoToState("CheckedPointerOver");
            }
            else
            {
                m_DeviceLayer.UICanvas.GoToState("PointerOver");
            }
        }
        private void DeviceLayerRadioButton_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (DeviceLayerRadioButton.IsChecked == true)
            {
                m_DeviceLayer.UICanvas.GoToState("Checked");
            }
            else
            {
                m_DeviceLayer.UICanvas.GoToState("Normal");
            }
        }
        private void SelectMe()
        {
            DeviceLayerRadioButton.IsChecked = true;
        }
        #endregion
    }
}
