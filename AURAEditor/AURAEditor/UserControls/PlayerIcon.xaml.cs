using AuraEditor.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace AuraEditor.UserControls
{
    public sealed partial class PlayerIcon : UserControl
    {
        public PlayerModel m_Model { get { return this.DataContext as PlayerModel; } }
        
        public PlayerIcon()
        {
            this.InitializeComponent();
        }

        private void PlayerIcon_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            TranslateTransform tt = this.RenderTransform as TranslateTransform;
            if (tt.X + e.Delta.Translation.X < 0)
                tt.X = 0;
            else
                tt.X += e.Delta.Translation.X;
        }
    }
}
