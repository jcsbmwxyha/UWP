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

// 內容對話方塊項目範本已記錄在 https://go.microsoft.com/fwlink/?LinkId=234238

namespace VocabularyTest.Dialog
{
    public sealed partial class EditDialog : ContentDialog
    {
        Vocabulary _voc;

        public EditDialog(Vocabulary voc)
        {
            this.InitializeComponent();
            _voc = voc;
            TextBoxEnglish.Text = _voc.English;
            TextBoxChinese.Text = _voc.Chinese;
        }

        private new void PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            SaveText();
        }
        private new void CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

        }
        private void TextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if ((int)e.Key == 0xD) // Enter
            {
                SaveText();
                this.Hide();
            }
        }
        private void SaveText()
        {
            if (TextBoxEnglish.Text != "" && TextBoxChinese.Text != "")
            {
                _voc.English = TextBoxEnglish.Text;
                _voc.Chinese = TextBoxChinese.Text;
            }
        }

    }
}
