using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        ObservableCollection<Vocabulary> _vocObCollection;
        int _currentVocIndex;
        public int CurrentVocIndex
        {
            get
            {
                return _currentVocIndex;
            }
            set
            {
                _currentVocIndex = value;

                EnglishTextBlock.Text = _vocObCollection[_currentVocIndex].English;
                KKTextBlock.Text = "";
                ChineseTextBlock.Text = "";

                if (_vocObCollection[_currentVocIndex].Star == true)
                    StarButton.Content = "\uE249";
                else
                    StarButton.Content = "\uE24A";
            }
        }

        public StartTestDialog(ObservableCollection<Vocabulary> vocs)
        {
            this.InitializeComponent();
            _vocObCollection = vocs;

            // creates a number between 0 and count
            Random rnd = new Random();
            CurrentVocIndex = rnd.Next(0, _vocObCollection.Count- 1);
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            // creates a number between 0 and count
            Random rnd = new Random();
            CurrentVocIndex = rnd.Next(0, _vocObCollection.Count - 1);
        }

        private void AnswerButton_Click(object sender, RoutedEventArgs e)
        {
            KKTextBlock.Text = _vocObCollection[_currentVocIndex].KK;
            ChineseTextBlock.Text = _vocObCollection[_currentVocIndex].Chinese;
        }

        private void StarButton_Click(object sender, RoutedEventArgs e)
        {
            _vocObCollection[CurrentVocIndex].Star ^= true;

            if (_vocObCollection[_currentVocIndex].Star == true)
                StarButton.Content = "\uE249";
            else
                StarButton.Content = "\uE24A";
        }
    }
}
