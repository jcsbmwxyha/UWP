using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// 使用者控制項項目範本記載於 https://go.microsoft.com/fwlink/?LinkId=234236

namespace AuraEditor.UserControls
{
    public sealed partial class LayerCanvas : UserControl
    {
        public LayerCanvas()
        {
            this.InitializeComponent();
        }
        public void AddElement(FrameworkElement fe)
        {
            MyCanvas.Children.Add(fe);
        }
        public void GoToState(string State)
        {
            VisualStateManager.GoToState(this, State, false);
        }

        bool _pressCtrl = false;
        bool _pressV = false;
        private void MyCanvas_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.V)
            {
                _pressCtrl = true;
            }
            if (e.Key == VirtualKey.Control)
            {
                _pressV = true;
            }
            if (_pressCtrl & _pressV == true)
            {
                EffectLine el = AuraLayerManager.Self.GetCopiedEffectLine();
                MyCanvas.Children.Add(el);
            }
        }
        private void MyCanvas_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.V)
            {
                _pressCtrl = false;
            }
            if (e.Key == VirtualKey.Control)
            {
                _pressV = false;
            }
        }
    }
}
