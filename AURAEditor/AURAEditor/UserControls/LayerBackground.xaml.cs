using System;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using static AuraEditor.Common.EffectHelper;

// 使用者控制項項目範本記載於 https://go.microsoft.com/fwlink/?LinkId=234236

namespace AuraEditor.UserControls
{
    public sealed partial class LayerBackground : UserControl
    {
        private Layer m_Layer { get { return this.DataContext as Layer; } }
        public LayerBackground()
        {
            this.InitializeComponent();
        }
        public void GoToState(string State)
        {
            VisualStateManager.GoToState(this, State, false);
        }
    }
}
