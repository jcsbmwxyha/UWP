using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using VocabularyTest.Common;
using VocabularyTest.Dialog;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace VocabularyTest
{
    enum Website { Google = 0, Yahoo };

    public sealed partial class MainPage : Page
    {
        StorageFile _volStorageFile;
        public const string _defaultFileName = "vols.dat";
        public const string yahooURL = @"https://tw.dictionary.search.yahoo.com/search?p=";
        public const string googleURL = @"https://translate.google.com.tw/#en/zh-TW/";

        private ObservableCollection<Vocabulary> _myVolsList;
        public ObservableCollection<Vocabulary> MyVolsList
        {
            get => _myVolsList;
            set => _myVolsList = value;
        }

        private bool _saveBtnEnabled;
        public bool SaveBtnEnabled
        {
            get => _saveBtnEnabled;
            set
            {
                if (value == true)
                {
                    //SoundButton.IsEnabled = false;
                    VocabularyListBox.SelectedIndex = -1;
                    SaveButton.IsEnabled = true;
                }
                else
                {
                    SaveButton.IsEnabled = false;
                }
                _saveBtnEnabled = value;
            }
        }

        int _selectedItemIndex;
        public int SelectedItemIndex
        {
            get => _selectedItemIndex;
            set
            {
                if (_selectedItemIndex != value)
                {
                    List<VocabularyListItem> listItem =
                        ControlHelper.FindAllControl<VocabularyListItem>(VocabularyListBox, typeof(VocabularyListItem));

                    // update content
                    if (_selectedItemIndex != -1)
                    {
                        VocabularyListItem oldItem = listItem[_selectedItemIndex];
                        oldItem.IsSeleted = false;
                        oldItem.UpdateContent();
                    }

                    if (value == -1)
                    {
                        ClearScreen();
                    }
                    else
                    {
                        VocabularyListItem newItem = listItem[value];
                        newItem.IsSeleted = true;
                        newItem.UpdateContent();

                        // update text
                        Vocabulary vol = VocabularyListBox.SelectedItem as Vocabulary;
                        Paragraph paragraph = new Paragraph();
                        Run run = new Run();
                        eventLog.TextWrapping = TextWrapping.Wrap;
                        run.Text = vol.English + "\n" + vol.Chinese;
                        paragraph.Inlines.Add(run);

                        VocabularyRichTextBlock.Blocks.Clear();
                        VocabularyRichTextBlock.Blocks.Insert(0, paragraph);
                    }

                    _selectedItemIndex = value;
                    VocabularyListBox.SelectedIndex = value;
                }
            }
        }

        public MainPage()
        {
            this.InitializeComponent();
            SaveBtnEnabled = false;
        }

        async Task<ObservableCollection<Vocabulary>> GetVocabularies()
        {
            // Method
            // StorageFolder Folder = await StorageFolder.GetFolderFromPathAsync("...");
            // StorageFolder Folder = await KnownFolders.GetFolderForUserAsync(null /* current user */, KnownFolderId.PicturesLibrary);
            // StorageFolder InstallationFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            // StorageFile File = await InstallationFolder.GetFileAsync("Assets\vols.dat");
            // StorageFile File = (StorageFile)await InstallationFolder.TryGetItemAsync("Assets\vols.dat");

            // Step 1 : Get Folder & File
            ObservableCollection<Vocabulary> vols = new ObservableCollection<Vocabulary>();
            StorageFolder InstallationFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            _volStorageFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/vols.dat"));

            // Step 2 : Parsing
            string fileContent = await FileIO.ReadTextAsync(_volStorageFile);
            string[] stringSeparators = new string[] { "\r\n" };
            string[] result = fileContent.Split(stringSeparators, StringSplitOptions.None);

            for (int i = 0; i + 1 < result.Length; i += 2)
            {
                if (result[i] != "" && result[i] != "")
                    vols.Add(new Vocabulary(result[i], result[i + 1], ""));
            }

            return vols;
        }


        private void VocabularyListBox_LostFocus(object sender, RoutedEventArgs e)
        {
            //SelectedItemIndex = -1;
        }
        private void VocabularyListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedItemIndex = VocabularyListBox.SelectedIndex;
        }
        private void ClearScreen()
        {
            VocabularyRichTextBlock.Blocks.Clear();
        }

        //private void SoundButton_Click(object sender, RoutedEventArgs e)
        //{
        //    Vocabulary volD = VocabularyListBox.SelectedItem as Vocabulary;

        //    if (volD == null || volD.English.Contains(" "))
        //    {
        //        CommonHelper.ShowMessage("No sound can play!");
        //        return;
        //    }

        //    //SoundButton.IsEnabled = false;
        //    Task<string> source = GetWebPageSourceAsync(volD.English);
        //}

        //private async Task<string> GetWebPageSourceAsync(string eng)
        //{
        //    MediaElement PlayMusic = new MediaElement();
        //    StorageFolder picturesLibrary = await KnownFolders.GetFolderForUserAsync(null /* current user */, KnownFolderId.PicturesLibrary);
        //    StorageFolder mp3folder = await picturesLibrary.GetFolderAsync("MP3");
        //    StorageFile destinationFile = null;

        //    string mp3filename = eng + ".mp3";
        //    string mp3folderpath = mp3folder.Path;
        //    string httpResponseBody = "";

        //    if (await mp3folder.TryGetItemAsync(mp3filename) == null)
        //    {
        //        string endString = "mp3";
        //        string startString = "https:";

        //        Windows.Web.Http.HttpClient httpClient = new Windows.Web.Http.HttpClient();

        //        Uri requestUri = new Uri(@"https://tw.dictionary.search.yahoo.com/search?p=" + eng);

        //        //Send the GET request asynchronously and retrieve the response as a string.
        //        Windows.Web.Http.HttpResponseMessage httpResponse = new Windows.Web.Http.HttpResponseMessage();

        //        //Send the GET request
        //        httpResponse = await httpClient.GetAsync(requestUri);
        //        httpResponse.EnsureSuccessStatusCode();
        //        httpResponseBody = await httpResponse.Content.ReadAsStringAsync();

        //        string fileContent = await FileIO.ReadTextAsync(_volStorageFile);
        //        string[] stringSeparators = new string[] { "\r\n" };
        //        string stringTemp;
        //        int startIndex, endIndex;

        //        endIndex = httpResponseBody.IndexOf(endString);
        //        stringTemp = httpResponseBody.Substring(0, endIndex + endString.Length);
        //        startIndex = stringTemp.LastIndexOf(startString);
        //        stringTemp = stringTemp.Substring(startIndex);
        //        stringTemp = stringTemp.Replace("\\", "");
        //        //ShowMessage(stringTemp);

        //        Uri downloadAddress = new Uri(stringTemp, UriKind.Absolute);
        //        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(downloadAddress);
        //        WebResponse response = await request.GetResponseAsync();
        //        Stream stream = response.GetResponseStream();
        //        destinationFile = await mp3folder.CreateFileAsync(mp3filename, CreationCollisionOption.GenerateUniqueName);

        //        await Windows.Storage.FileIO.WriteBytesAsync(destinationFile, ReadStream(stream));
        //    }
        //    else
        //    {
        //        destinationFile = await StorageFile.GetFileFromPathAsync(mp3folderpath + "\\" + mp3filename);
        //    }

        //    PlayMusic.SetSource(await destinationFile.OpenAsync(FileAccessMode.Read), destinationFile.ContentType);
        //    PlayMusic.Play();

        //    //SoundButton.IsEnabled = true;
        //    return httpResponseBody;
        //}

        //private byte[] ReadStream(Stream stream)
        //{
        //    byte[] buffer = new byte[16 * 1024];
        //    using (MemoryStream ms = new MemoryStream())
        //    {
        //        int read;
        //        while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
        //        {
        //            ms.Write(buffer, 0, read);
        //        }
        //        return ms.ToArray();
        //    }

        //}

        //private void YahooButton_Click(object sender, RoutedEventArgs e)
        //{
        //    if (VocabularyListBox.SelectedItem is Vocabulary volD)
        //    {
        //        DefaultLaunch(volD.English, Website.Yahoo);
        //    }
        //}
        //private void GoogleButton_Click(object sender, RoutedEventArgs e)
        //{
        //    if (VocabularyListBox.SelectedItem is Vocabulary volD)
        //    {
        //        DefaultLaunch(volD.English, Website.Google);
        //    }
        //}

        //async void DefaultLaunch(string s, Website web)
        //{
        //    Uri u;

        //    if (web == Website.Yahoo)
        //        u = new Uri(yahooURL + s.Replace(" ", "+"));
        //    else
        //        u = new Uri(googleURL + s.Replace(" ", "%20"));

        //    var success = await Windows.System.Launcher.LaunchUriAsync(u);
        //}

        //private void VolDeleteBtn_Click(object sender, RoutedEventArgs e)
        //{
        //    Button b = sender as Button;

        //    //VolData vd = VocabularyListBox.SelectedItem as VolData;
        //    MyVolsList.RemoveAt(VocabularyListBox.SelectedIndex);
        //    VocabularyListBox.ItemsSource = null;
        //    VocabularyListBox.ItemsSource = MyVolsList;
        //}

        private void UpdateEventLog(string s)
        {
            Paragraph paragraph = new Paragraph();
            Run run = new Run();
            eventLog.TextWrapping = TextWrapping.Wrap;
            run.Text = s;
            paragraph.Inlines.Add(run);
            eventLog.Blocks.Insert(0, paragraph);
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_volStorageFile == null)
                return;

            string result = "";
            foreach (Vocabulary vd in MyVolsList)
            {
                result += vd.English + "\r\n" + vd.Chinese + "\r\n";
            }

            if (!String.IsNullOrEmpty(result))
            {
                await FileIO.WriteTextAsync(_volStorageFile, result);
            }

            SaveBtnEnabled = false;
        }
        private async void SaveAsButton_Click(object sender, RoutedEventArgs e)
        {
            var savePicker = new FileSavePicker();
            savePicker.SuggestedStartLocation =
                Windows.Storage.Pickers.PickerLocationId.Desktop;
            // Dropdown of file types the user can save the file as
            savePicker.FileTypeChoices.Add("Plain Text", new List<string>() { ".dat" });
            // Default file name if the user does not type one in or select a file to replace
            savePicker.SuggestedFileName = "MyVols";

            StorageFile saveFile = await savePicker.PickSaveFileAsync();

            if (saveFile != null)
            {
                // Prevent updates to the remote version of the file until
                // we finish making changes and call CompleteUpdatesAsync.
                Windows.Storage.CachedFileManager.DeferUpdates(saveFile);

                // write to file
                string result = "";

                foreach (Vocabulary vd in MyVolsList)
                {
                    result += vd.English + "\r\n" + vd.Chinese + "\r\n";
                }

                if (!String.IsNullOrEmpty(result))
                {
                    await FileIO.WriteTextAsync(saveFile, result);
                }

                // Let Windows know that we're finished changing the file so
                // the other app can update the remote version of the file.
                // Completing updates may require Windows to ask for user input.
                Windows.Storage.Provider.FileUpdateStatus status =
                    await Windows.Storage.CachedFileManager.CompleteUpdatesAsync(saveFile);
                
                _volStorageFile = saveFile;
            }
        }
        private async void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker fileOpenPicker = new FileOpenPicker();
            fileOpenPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            fileOpenPicker.FileTypeFilter.Add(".dat");
            fileOpenPicker.ViewMode = PickerViewMode.Thumbnail;

            var inputFile = await fileOpenPicker.PickSingleFileAsync();

            if (inputFile == null)
            {
                // The user cancelled the picking operation
                return;
            }

            string fileContent = await FileIO.ReadTextAsync(inputFile);
            string[] stringSeparators = new string[] { "\r\n" };
            string[] result = fileContent.Split(stringSeparators, StringSplitOptions.None);
            
            ObservableCollection<Vocabulary> vols = new ObservableCollection<Vocabulary>();

            for (int i = 0; i + 1 < result.Length; i += 2)
            {
                if (result[i] != "" && result[i] != "")
                    vols.Add(new Vocabulary(result[i], result[i + 1], ""));
            }

            MyVolsList = vols;
            _volStorageFile = inputFile;
            VocabularyListBox.ItemsSource = MyVolsList;
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (MyVolsList == null)
                MyVolsList = new ObservableCollection<Vocabulary>();

            Vocabulary vol = new Vocabulary("", "", "");
            EditDialog dialog = new EditDialog(vol);
            await dialog.ShowAsync();

            if (vol.English != "" && vol.Chinese != "")
            {
                MyVolsList.Add(vol);
                VocabularyListBox.ItemsSource = MyVolsList;
            }
        }

        private void MainGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            VocabularyRichTextBlockGrid.Width = MainGrid.ColumnDefinitions[0].ActualWidth;
            eventLogGrid.Width = MainGrid.ColumnDefinitions[0].ActualWidth;
            VocabularyListBox.Width = MainGrid.ColumnDefinitions[1].ActualWidth;
        }
    }
}
