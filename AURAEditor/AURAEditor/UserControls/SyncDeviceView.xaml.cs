using AuraEditor.Dialogs;
using AuraEditor.Models;
using Windows.UI.Xaml.Controls;

// 使用者控制項項目範本記載於 https://go.microsoft.com/fwlink/?LinkId=234236

namespace AuraEditor.UserControls
{
    public sealed partial class SyncDeviceView : UserControl
    {
        public SyncDeviceModel m_SyncDeviceModel { get { return this.DataContext as SyncDeviceModel; } }
        public SyncDeviceView()
        {
            this.InitializeComponent();
        }
    }
}
