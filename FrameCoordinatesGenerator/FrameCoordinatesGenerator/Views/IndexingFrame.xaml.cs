using FrameCoordinatesGenerator.Models;
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

// 使用者控制項項目範本記載於 https://go.microsoft.com/fwlink/?LinkId=234236

namespace FrameCoordinatesGenerator
{
    public sealed partial class IndexingFrame : UserControl
    {
        private IndexingFrameModel m_Model { get { return this.DataContext as IndexingFrameModel; } }

        public IndexingFrame()
        {
            this.InitializeComponent();
        }

        private void Grid_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            m_Model.Editing = true;
            MyTextBox.Focus(FocusState.Programmatic);
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            m_Model.LedIndex = MyTextBox.Text;
            m_Model.Editing = false;
            MainPage.Self.DectectConflict();
        }

        private void TextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                if (MyTextBox.Text != "")
                {
                    m_Model.Editing = false;
                    MainPage.Self.ImageScrollViewer.Focus(FocusState.Programmatic);
                }
            }
            else if (!e.Key.ToString().Contains("Number"))
            {
                e.Handled = true;
            }
        }

        private void MyTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            char[] originalText = MyTextBox.Text.ToCharArray();
            if (MyTextBox.Text.Length > 0)
            {
                foreach (char c in originalText)
                {
                    if (!(Char.IsNumber(c)))
                    {
                        MyTextBox.Text = MyTextBox.Text.Replace(Convert.ToString(c), "");
                        break;
                    }
                }
            }
        }
    }
}
