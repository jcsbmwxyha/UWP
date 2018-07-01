using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Text;
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
        public const string yahooURL = @"https://tw.dictionary.search.yahoo.com/search?p=";

        public EditDialog(Vocabulary voc)
        {
            this.InitializeComponent();
            _voc = voc;
            EnglishTextBox.Text = _voc.English;
            KKTextBox.Text = _voc.KK;
            ChineseTextBox.Text = _voc.Chinese;
            NoteRichEditBox.Document.SetText(TextSetOptions.None, _voc.Note);
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
            if (EnglishTextBox.Text != "" && ChineseTextBox.Text != "")
            {
                _voc.English = EnglishTextBox.Text;
                _voc.Chinese = ChineseTextBox.Text;
                _voc.KK = KKTextBox.Text;
                NoteRichEditBox.Document.GetText(TextGetOptions.None, out string noteText);

                // Unexplained reason has /r at the end of the Document,
                // so remove it.
                _voc.Note = noteText.Substring(0, noteText.Length - 1);
            }
        }

        private async void KKAuto_Click(object sender, RoutedEventArgs e)
        {
            string httpResponseBody = "";
            HttpClient httpClient = new HttpClient();
            Uri requestUri = new Uri(yahooURL + EnglishTextBox.Text);

            //Send the GET request asynchronously and retrieve the response as a string.
            HttpResponseMessage httpResponse = new HttpResponseMessage();

            //Send the GET request
            httpResponse = await httpClient.GetAsync(requestUri);
            httpResponse.EnsureSuccessStatusCode();
            httpResponseBody = await httpResponse.Content.ReadAsStringAsync();

            string startString = "KK[";
            string endString = "]";
            KKTextBox.Text = ParseString(httpResponseBody, startString, endString, true);
        }
        private string ParseString(string content, string startString, string endString, bool includeStartEnd)
        {
            string result = "";
            int startIndex, endIndex;

            startIndex = content.IndexOf(startString);
            if (includeStartEnd)
                content = content.Substring(startIndex);
            else
                content = content.Substring(startIndex + startString.Length);

            endIndex = content.IndexOf(endString);
            if (includeStartEnd)
                result = content.Substring(0, endIndex + endString.Length);
            else
                result = content.Substring(0, endIndex);

            return result;
        }
    }
}
