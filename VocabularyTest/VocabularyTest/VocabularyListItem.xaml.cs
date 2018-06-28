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

namespace VocabularyTest
{
    public sealed partial class VocabularyListItem : UserControl
    {
        public Vocabulary MyVocabulary { get { return this.DataContext as Vocabulary; } }

        public VocabularyListItem()
        {
            this.InitializeComponent();
            this.DataContextChanged += (s, e) => Bindings.Update();
        }
        public void UpdateContent()
        {
            Bindings.Update();
        }
        
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void RelativePanel_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }
        private void SoundButton_Click(object sender, RoutedEventArgs e)
        {

        }
        private void GoogleButton_Click(object sender, RoutedEventArgs e)
        {

        }
        private void YahooButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RelativePanel_GotFocus(object sender, RoutedEventArgs e)
        {

        }
    }
}
