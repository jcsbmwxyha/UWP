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

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VocabularyTest.Dialog
{
    public sealed partial class StartTestDialog : ContentDialog
    {
        List<Vocabulary> _voclist;
        int _vocIndex;

        public StartTestDialog(List<Vocabulary> vocs)
        {
            this.InitializeComponent();
            _voclist = vocs;

            // creates a number between 0 and count
            Random rnd = new Random();
            _vocIndex = rnd.Next(0, _voclist.Count- 1);
            EnglishTextBlock.Text = _voclist[_vocIndex].English;
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            // creates a number between 0 and count
            Random rnd = new Random();
            _vocIndex = rnd.Next(0, _voclist.Count - 1);

            ChineseTextBlock.Text = "";
            EnglishTextBlock.Text = _voclist[_vocIndex].English;
        }

        private void AnswerButton_Click(object sender, RoutedEventArgs e)
        {
            ChineseTextBlock.Text = _voclist[_vocIndex].Chinese;
        }
    }
}
