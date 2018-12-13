using FrameCoordinatesGenerator.Models;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using static FrameCoordinatesGenerator.Common.Math2;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace FrameCoordinatesGenerator.Views
{
    public sealed partial class DeviceView : UserControl
    {
        private DeviceModel m_DeviceModel { get { return this.DataContext as DeviceModel; } }

        public DeviceView()
        {
            this.InitializeComponent();
            this.DataContextChanged += (s, e) => Bindings.Update();
        }
    }
}
