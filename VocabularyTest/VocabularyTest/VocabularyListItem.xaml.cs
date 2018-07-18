using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using VocabularyTest.Common;
using VocabularyTest.Dialog;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.Web.Http;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace VocabularyTest
{
    public sealed partial class VocabularyListItem : UserControl
    {
        public Vocabulary MyVocabulary { get { return this.DataContext as Vocabulary; } }
        public bool IsSeleted { get; set; }

        public VocabularyListItem()
        {
            this.InitializeComponent();
            this.DataContextChanged += (s, e) => Bindings.Update();
            IsSeleted = false;
        }
        public void UpdateContent()
        {
            Bindings.Update();
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            EditDialog dialog = new EditDialog(MyVocabulary);
            await dialog.ShowAsync();
            Bindings.Update();

            var frame = (Frame)Window.Current.Content;
            var page = (MainPage)frame.Content;
            page.SaveBtnEnabled = true;
            page.UpdateSelectedVocContent();
        }
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var frame = (Frame)Window.Current.Content;
            var page = (MainPage)frame.Content;
            page.DeleteVocabulary(MyVocabulary);
            page.SaveBtnEnabled = true;
        }
        private void SoundButton_Click(object sender, RoutedEventArgs e)
        {
            if (MyVocabulary != null)
            {
                Task<string> source = GetWebPageSourceAsync(MyVocabulary.English);
            }
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

                Uri requestUri = new Uri(CommonHelper.yahooURL + eng);

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

        private async void GoogleButton_Click(object sender, RoutedEventArgs e)
        {
            Uri u = new Uri(CommonHelper.googleURL + MyVocabulary.English.Replace(" ", "%20"));
            var success = await Windows.System.Launcher.LaunchUriAsync(u);
        }
        private async void YahooButton_Click(object sender, RoutedEventArgs e)
        {
            Uri u = new Uri(CommonHelper.yahooURL + MyVocabulary.English.Replace(" ", "+"));
            var success = await Windows.System.Launcher.LaunchUriAsync(u);
        }
        private void StarButton_Click(object sender, RoutedEventArgs e)
        {
            if (MyVocabulary.Star == true)
            {
                StarButton.Content = "\uE24A";
                MyVocabulary.Star = false;
            }
            else
            {
                StarButton.Content = "\uE249";
                MyVocabulary.Star = true;
            }
        }

        private void EarButton_Click(object sender, RoutedEventArgs e)
        {
            if (MyVocabulary.Ear == true)
            {
                EarButton.Content = "\uE001";
                MyVocabulary.Ear = false;
            }
            else
            {
                EarButton.Content = "\uF270";
                MyVocabulary.Ear = true;
            }
        }
    }
}
