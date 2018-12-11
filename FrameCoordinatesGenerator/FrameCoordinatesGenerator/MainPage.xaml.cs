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
        ImagePixelStructure g_ImagePixelStructure;
        LedCsvData g_LedCsvData;

        Image currentImage;
        List<PreLoadFrameModel> g_PreLoadFrameModels;
        
        public MainPage()
        {
            this.InitializeComponent();
            g_PreLoadFrameModels = new List<PreLoadFrameModel>();
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

            g_LedCsvData = new LedCsvData(csvFile);
            await g_LedCsvData.StartParsingAsync();
        }
        #endregion

        #region -- Start --
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (g_ImagePixelStructure == null)
                return;
            
            List<Rect> imageRectResult;
            imageRectResult = g_ImagePixelStructure.GetRects(ParsingMode.Frame);
            imageRectResult = SortingByGroup(imageRectResult);
            PreLoad(imageRectResult);
        }
        private List<Rect> SortingByGroup(List<Rect> rects)
        {
            List<Rect> result = new List<Rect>();
            bool tryParse = int.TryParse(DifferenceTextBox.Text, out int offset);
            if (tryParse == false) offset = 5;

            rects = SortByY(rects);

            List<Rect> group = new List<Rect>();
            for (int i = 0; i < rects.Count; i++)
            {
                if (i == 0)
                {
                    group.Add(rects[i]);
                }
                else if (rects[i].Top - rects[i - 1].Top <= offset)
                {
                    group.Add(rects[i]);
                }
                else
                {
                    result.AddRange(SortByX(group));
                    group.Clear();
                    group.Add(rects[i]);
                }
            }

            result.AddRange(SortByX(group));
            return result;
        }
        private List<Rect> SortByX(List<Rect> rects)
        {
            for (int i = 0; i < rects.Count - 1; i++)
            {
                for (int j = 0; j < rects.Count - 1 - i; j++)
                {
                    if (rects[j].Left > rects[j + 1].Left)
                    {
                        Rect temp = rects[j];
                        rects[j] = rects[j + 1];
                        rects[j + 1] = temp;
                    }
                }
            }
            return rects;
        }
        private List<Rect> SortByY(List<Rect> rects)
        {
            for (int i = 0; i < rects.Count - 1; i++)
            {
                for (int j = 0; j < rects.Count - 1 - i; j++)
                {
                    if (rects[j].Top > rects[j + 1].Top)
                    {
                        Rect temp = rects[j];
                        rects[j] = rects[j + 1];
                        rects[j + 1] = temp;
                    }
                }
            }
            return rects;
        }
        private void PreLoad(List<Rect> frameRects)
        {
            ImageGrid.Children.Clear();
            ImageGrid.Children.Add(currentImage);
            int[] indexArray = null;

            if (g_LedCsvData != null)
                indexArray = g_LedCsvData.GetIndexOrderArray();

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
        private Rectangle CreateRectangle(Rect rect)
        {
            CompositeTransform ct = new CompositeTransform
            {
                TranslateX = rect.X,
                TranslateY = rect.Y
            };

            Rectangle rectangle = new Rectangle
            {
                Fill = new SolidColorBrush(Colors.Transparent),
                StrokeThickness = 1,
                RenderTransform = ct,
                Width = rect.Width + 1,
                Height = rect.Height + 1,
                HorizontalAlignment = 0,
                VerticalAlignment = 0,
            };

            rectangle.Stroke = new SolidColorBrush(Colors.Red);

            return rectangle;
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

            int sourceW = g_ImagePixelStructure.PixelWidth;
            int sourceH = g_ImagePixelStructure.PixelHeight;
            
            using (CsvFileWriter csvWriter = new CsvFileWriter(await csvFile.OpenStreamForWriteAsync()))
            {
                for (int i = 0; i < g_LedCsvData.DataRows.Count; i++)
                {

                }

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

                int count = g_LedCsvData.DataRows.Count;
                var rows = g_LedCsvData.DataRows;

                for (int i = 0; i < count; i++)
                {
                    if (count < g_LedCsvData.LedDataRowStartIndex)
                        csvWriter.WriteRow(rows[i]);
                    else if (count < g_LedCsvData.LedDataRowStartIndex) // Append parameter name
                    {
                        rows[i].Add()
                    }
                    else // Append parameter
                    {

                    }

                    CsvRow row = new CsvRow
                    {
                        "LED " + i.ToString(),
                        1.ToString(), // exist
                        imageRectResult[i].X.ToString(),
                        imageRectResult[i].Y.ToString(),
                        (imageRectResult[i].Right + 1).ToString(),
                        (imageRectResult[i].Bottom + 1).ToString(),
                        "", // PNG
                        1.ToString(), // Z index
                    };

                    csvWriter.WriteRow(row);
                }

                csvWriter.Close();
            }
        }
        #endregion
    }
}
