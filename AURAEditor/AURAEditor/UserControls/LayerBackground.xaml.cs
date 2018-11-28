using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// 使用者控制項項目範本記載於 https://go.microsoft.com/fwlink/?LinkId=234236

namespace AuraEditor.UserControls
{
    public sealed partial class LayerBackground : UserControl
    {
        public Layer m_Layer { get { return this.DataContext as Layer; } }

        public LayerBackground()
        {
            this.InitializeComponent();
            this.DataContextChanged += (s, e) => Bindings.Update();
        }
    }
}
