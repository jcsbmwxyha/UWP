using CsvParse;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;
using Colors = Windows.UI.Colors;

namespace FrameCoordinatesGenerator
{
    public sealed partial class MainPage : Page
    {
        static public MainPage Self;

        ImagePixelStructure g_ImagePixelStructure;
        CsvLoadedData g_CsvLoadedData;
        List<Rect> g_LedRects;
        Image currentImage;
        List<PreLoadFrameModel> g_PreLoadFrameModels;

        public MainPage()
        {
            Self = this;
            this.InitializeComponent();
            g_PreLoadFrameModels = new List<PreLoadFrameModel>();
        }

        public void OnLostFocus()
        {
            for (int i = 0; i < g_PreLoadFrameModels.Count; i++)
            {
                g_PreLoadFrameModels[i].Conflict = false;
            }

            for (int i = 0; i < g_PreLoadFrameModels.Count; i++)
            {
                for (int j = i; j < g_PreLoadFrameModels.Count; j++)
                {
                    if(g_PreLoadFrameModels[i].IntIndex == g_PreLoadFrameModels[j].IntIndex)
                    {
                        g_PreLoadFrameModels[i].Conflict = true;
                        g_PreLoadFrameModels[j].Conflict = true;
                    }
                }
            }
        }

        #region -- Loading --
        private async void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker fileOpenPicker = new FileOpenPicker();
            fileOpenPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            fileOpenPicker.FileTypeFilter.Add(".png");
            fileOpenPicker.ViewMode = PickerViewMode.Thumbnail;

            var inputFile = await fileOpenPicker.PickSingleFileAsync();

            if (inputFile == null)
                return;

            SoftwareBitmap softwareBitmap;

            using (IRandomAccessStream stream = await inputFile.OpenAsync(FileAccessMode.Read))
            {
                // Create the decoder from the stream
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(BitmapDecoder.PngDecoderId, stream);

                // Get the SoftwareBitmap representation of the file
                softwareBitmap = await decoder.GetSoftwareBitmapAsync();
            }

            if (softwareBitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8 || softwareBitmap.BitmapAlphaMode == BitmapAlphaMode.Straight)
            {
                softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
            }

            g_ImagePixelStructure = new ImagePixelStructure(softwareBitmap);
            await SetMainGridImage(softwareBitmap);

            LoadPathTextBlock.Text = inputFile.Path;
        }
        private async Task SetMainGridImage(SoftwareBitmap softwareBitmap)
        {
            var source = new SoftwareBitmapSource();
            await source.SetBitmapAsync(softwareBitmap);
            Image image = new Image
            {
                Stretch = 0,
                HorizontalAlignment = 0,
                VerticalAlignment = 0,
                Source = source
            };

            ImageGrid.Children.Clear();
            ImageGrid.Children.Add(image);

            currentImage = image;
        }

        private async void LoadCSVButton_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker fileOpenPicker = new FileOpenPicker();
            fileOpenPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            fileOpenPicker.FileTypeFilter.Add(".csv");
            fileOpenPicker.ViewMode = PickerViewMode.Thumbnail;

            var csvFile = await fileOpenPicker.PickSingleFileAsync();

            if (csvFile == null)
                return;

            g_CsvLoadedData = new CsvLoadedData(csvFile);
            LoadCSVPathTextBlock.Text = csvFile.Path;
        }
        #endregion

        #region -- Start --
        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (g_ImagePixelStructure == null)
                return;

            if (g_CsvLoadedData != null)
                await g_CsvLoadedData.StartParsingAsync();

            bool tryParse = int.TryParse(DifferenceTextBox.Text, out int offset);

            if (tryParse == false)
                g_LedRects = g_ImagePixelStructure.GetOrderedRects(ParsingMode.Frame);
            else
                g_LedRects = g_ImagePixelStructure.GetOrderedRects(ParsingMode.Frame, offset);
            
            PreLoad(g_LedRects);
        }
        private void PreLoad(List<Rect> frameRects)
        {
            ImageGrid.Children.Clear();
            ImageGrid.Children.Add(currentImage);
            int[] indexArray = null;

            if (g_CsvLoadedData != null)
                indexArray = g_CsvLoadedData.GetIndexOrderArray();

            for (int i = 0; i < frameRects.Count; i++)
            {
                PreLoadFrame view = new PreLoadFrame();

                PreLoadFrameModel model = new PreLoadFrameModel()
                {
                    Left = frameRects[i].X,
                    Top = frameRects[i].Y,
                    Right = frameRects[i].Right,
                    Bottom = frameRects[i].Bottom,
                    LedIndex = i.ToString(),
                };

                if (indexArray != null)
                    model.LedIndex = indexArray[i].ToString();

                view.DataContext = model;
                ImageGrid.Children.Add(view);
                g_PreLoadFrameModels.Add(model);
            }
        }
        #endregion

        #region -- Save --
        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (g_ImagePixelStructure == null || g_PreLoadFrameModels.Count == 0)
            {
                StatusTextBlock.Text = "No frame to save !";
                return;
            }
            else
            {
                StatusTextBlock.Text = "";
            }

            var savePicker = new FileSavePicker();

            savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            savePicker.FileTypeChoices.Add("Plain Text", new List<string>() { ".csv" });
            savePicker.SuggestedFileName = "MyCsv";

            StorageFile csvFile;

            csvFile = await savePicker.PickSaveFileAsync();

            if (csvFile != null)
            {
                try
                {
                    SaveResultToCsv(csvFile);
                    SavePathTextBlock.Text = csvFile.Path;
                }
                catch (Exception ee)
                {
                    SavePathTextBlock.Text = ee.ToString();
                }
            }
        }
        private async void SaveResultToCsv(StorageFile csvFile)
        {
            // For overwrite file
            await FileIO.WriteBytesAsync(csvFile, new byte[0]);

            using (CsvFileWriter csvWriter = new CsvFileWriter(await csvFile.OpenStreamForWriteAsync()))
            {
                if (g_CsvLoadedData == null)
                {
                    CsvRow firstRow = new CsvRow
                    {
                        "Model"
                    };
                    csvWriter.WriteRow(firstRow);

                    CsvRow secondRow = new CsvRow
                    {
                        "Parameters",
                        "exist",
                        "LeftTop_x",
                        "LeftTop_y",
                        "RightBottom_x",
                        "RightBottom_y",
                        "PNG",
                        "Z_index"
                    };
                    csvWriter.WriteRow(secondRow);

                    for (int i = 0; i < g_LedRects.Count; i++)
                    {
                        CsvRow row = new CsvRow
                        {
                            "LED " + i.ToString(),
                            1.ToString(), // exist
                            g_LedRects[i].X.ToString(),
                            g_LedRects[i].Y.ToString(),
                            (g_LedRects[i].Right + 1).ToString(),
                            (g_LedRects[i].Bottom + 1).ToString(),
                            "", // PNG
                            1.ToString(), // Z index
                        };
                        csvWriter.WriteRow(row);
                    }
                }
                else
                {
                    int count = g_CsvLoadedData.DataRows.Count;
                    var rows = g_CsvLoadedData.DataRows;

                    for (int i = 0; i < count; i++)
                    {
                        if (i < g_CsvLoadedData.LedDataRowStartIndex)
                        {
                        }
                        else if (i == g_CsvLoadedData.LedDataRowStartIndex) // Append parameter name
                        {
                            rows[i].Add("LeftTop_x");
                            rows[i].Add("LeftTop_y");
                            rows[i].Add("RightBottom_x");
                            rows[i].Add("RightBottom_y");
                            rows[i].Add("PNG");
                            rows[i].Add("Z_index");
                        }
                        else // Append parameter
                        {
                            string index = rows[i][0].ToLower().Replace("led", "").Replace(" ", "");
                            PreLoadFrameModel findModel = g_PreLoadFrameModels.Find(
                                model => model.LedIndex.ToLower().Replace("led", "").Replace(" ", "") == index
                            );

                            if (findModel != null)
                            {
                                int align = g_CsvLoadedData.LedDataColumnEndIndex - rows[i].Count;

                                for (int j = 0; j < align; j++)
                                    rows[i].Add("");

                                rows[i].Add(findModel.Left.ToString());
                                rows[i].Add(findModel.Top.ToString());
                                rows[i].Add(findModel.Right.ToString());
                                rows[i].Add(findModel.Bottom.ToString());
                            }
                        }
                        
                        csvWriter.WriteRow(rows[i]);
                    }
                }

                csvWriter.Close();
            }
        }
        #endregion
    }
}
