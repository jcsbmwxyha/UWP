using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
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
        public const string yahooURL = @"https://tw.dictionary.search.yahoo.com/search?p=";
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

            // creates a index between 0 and count - 1
            Random rnd = new Random();
            CurrentVocIndex = rnd.Next(0, _vocObCollection.Count);
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            // creates a index between 0 and count - 1
            Random rnd = new Random();
            CurrentVocIndex = rnd.Next(0, _vocObCollection.Count);
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

        private void SoundButton_Click(object sender, RoutedEventArgs e)
        {
            Task<string> source = GetWebPageSourceAsync(EnglishTextBlock.Text);
        }
        private async Task<string> GetWebPageSourceAsync(string eng)
        {
            MediaElement PlayMusic = new MediaElement();
            StorageFolder baseFolder = ApplicationData.Current.LocalFolder;
            StorageFolder mp3Folder = await CheckOrCreateFolder(baseFolder, "mp3");
            StorageFile destinationFile = null;

            string mp3filename = eng + ".mp3";
            string mp3folderpath = mp3Folder.Path;
            string httpResponseBody = "";

            if (await mp3Folder.TryGetItemAsync(mp3filename) == null)
            {
                string endString = "mp3";
                string startString = "https:";

                HttpClient httpClient = new HttpClient();

                Uri requestUri = new Uri(yahooURL + eng);

                //Send the GET request asynchronously and retrieve the response as a string.
                HttpResponseMessage httpResponse = new HttpResponseMessage();

                //Send the GET request
                httpResponse = await httpClient.GetAsync(requestUri);
                httpResponse.EnsureSuccessStatusCode();
                httpResponseBody = await httpResponse.Content.ReadAsStringAsync();

                string stringTemp;
                int startIndex, endIndex;

                endIndex = httpResponseBody.IndexOf(endString);
                stringTemp = httpResponseBody.Substring(0, endIndex + endString.Length);
                startIndex = stringTemp.LastIndexOf(startString);
                stringTemp = stringTemp.Substring(startIndex);
                stringTemp = stringTemp.Replace("\\", "");
                //CommonHelper.ShowMessage(stringTemp);

                Uri downloadAddress = new Uri(stringTemp, UriKind.Absolute);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(downloadAddress);
                WebResponse response = await request.GetResponseAsync();
                Stream stream = response.GetResponseStream();
                destinationFile = await mp3Folder.CreateFileAsync(mp3filename, CreationCollisionOption.GenerateUniqueName);

                await FileIO.WriteBytesAsync(destinationFile, ReadStream(stream));
            }
            else
            {
                destinationFile = await StorageFile.GetFileFromPathAsync(mp3folderpath + "\\" + mp3filename);
            }

            PlayMusic.SetSource(await destinationFile.OpenAsync(FileAccessMode.Read), destinationFile.ContentType);
            PlayMusic.Play();

            //SoundButton.IsEnabled = true;
            return httpResponseBody;
        }
        private async Task<StorageFolder> CheckOrCreateFolder(StorageFolder sf, string folderName)
        {
            IReadOnlyList<StorageFolder> folderList = await sf.GetFoldersAsync();

            foreach (StorageFolder folder in folderList)
            {
                if (folder.Name == folderName)
                    return folder;
            }

            return await sf.CreateFolderAsync(folderName);
        }
        private byte[] ReadStream(Stream stream)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }

        }

        private void YahooButton_Click(object sender, RoutedEventArgs e)
        {
            DefaultLaunch(EnglishTextBlock.Text);
        }
        async void DefaultLaunch(string s)
        {
            Uri u;
            u = new Uri(yahooURL + s.Replace(" ", "+"));

            var success = await Windows.System.Launcher.LaunchUriAsync(u);
        }
    }
}
