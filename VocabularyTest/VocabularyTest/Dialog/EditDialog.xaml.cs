using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using VocabularyTest.Common;
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
            if (EnglishTextBox.Text != "")
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

        private async void AutoButton_Click(object sender, RoutedEventArgs e)
        {
            string noteText = "";
            string httpResponseBody = "";
            HttpClient httpClient = new HttpClient();
            Uri requestUri = new Uri(CommonHelper.yahooURL + EnglishTextBox.Text);

            //Send the GET request asynchronously and retrieve the response as a string.
            HttpResponseMessage httpResponse = new HttpResponseMessage();

            //Send the GET request
            httpResponse = await httpClient.GetAsync(requestUri);
            httpResponse.EnsureSuccessStatusCode();
            httpResponseBody = await httpResponse.Content.ReadAsStringAsync();

            // KK ++
            string startString = "KK[";
            string endString = "]";
            KKTextBox.Text = CommonHelper.ParseString(httpResponseBody, startString, endString, true);
            // KK --

            var doc = new HtmlDocument();
            doc.LoadHtml(httpResponseBody);

            var nodes = doc.DocumentNode.SelectNodes("//div");

            // 釋義
            foreach (var node in nodes)
            {
                HtmlAttribute att = node.Attributes["class"];
                if (att != null && att.Value == "compList")
                {
                    var c_doc = new HtmlDocument();
                    c_doc.LoadHtml(node.InnerHtml);

                    var c_nodes = c_doc.DocumentNode.SelectNodes("//span");
                    foreach (var c_node in c_nodes)
                    {
                        noteText += c_node.InnerText + "\n";
                    }

                    break;
                }
            }

            // 更多解釋
            if (noteText == "")
            {
                foreach (var node in nodes)
                {
                    HtmlAttribute att = node.Attributes["class"];
                    if (att != null && att.Value == "compList mt-5 ")
                    {
                        noteText += node.InnerText + "\n";
                    }
                }
            }


            noteText = noteText.Replace("&#39;", "'");
            NoteRichEditBox.Document.SetText(TextSetOptions.None, noteText);
        }
    }
}
