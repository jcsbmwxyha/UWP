using CsvParse;
using FrameCoordinatesGenerator.Common;
using FrameCoordinatesGenerator.Models;
using FrameCoordinatesGenerator.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using static FrameCoordinatesGenerator.Common.Math2;

namespace FrameCoordinatesGenerator
{
    public sealed partial class MainPage : Page
    {
        static public MainPage Self;

        private InputCsvData gInputCsvData;
        private MouseEventCtrl gMouseEventCtrl;
        private BlueFrameImage gBlueFrameImage;
        private ObservableCollection<IndexingFrameModel> gIndexingFrameModels;
        private DeviceView gPugioDV;
        private DeviceView gVerifyDV;
        //private List<Rect> gLedRects;

        public MainPage()
        {
            Self = this;
            this.InitializeComponent();
            gIndexingFrameModels = new ObservableCollection<IndexingFrameModel>();
            IntializeMouseEventCtrl();
        }
        private void IntializeMouseEventCtrl()
        {
            List<MouseDetectedRegion> regions = new List<MouseDetectedRegion>();

            gMouseEventCtrl = new MouseEventCtrl
            {
                MonitorMaxRect = new Rect(new Point(0, 0), new Point(1600, 1000)),
                DetectionRegions = regions.ToArray()
            };
        }
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            gPugioDV = CreatePugioDeviceView();
            VerifyCanvas.Children.Add(gPugioDV);
        }
        private DeviceView CreatePugioDeviceView()
        {
            BitmapImage bitmapImage = new BitmapImage(new Uri(this.BaseUri, "/Assets/PUGIO.png"));

            DeviceModel pugioDM = new DeviceModel
            {
                Name = "Pugio",
                Image = bitmapImage,
                PixelLeft = 1 * GridPixels,
                PixelTop = 1 * GridPixels,
                PixelWidth = 8 * GridPixels,
                PixelHeight = 10 * GridPixels
            };

            var view = new DeviceView
            {
                DataContext = pugioDM
            };

            return view;
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

            gBlueFrameImage = await BlueFrameImage.CreateInstanceAsync(softwareBitmap);
            DeviceImage.Source = gBlueFrameImage.GetImageSource();
            LoadPathTextBlock.Text = inputFile.Path;
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

            gInputCsvData = new InputCsvData(csvFile);
            LoadCSVPathTextBlock.Text = csvFile.Path;
        }
        #endregion

        #region -- Start --
        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (gBlueFrameImage == null)
                return;

            if (gInputCsvData != null)
                await gInputCsvData.StartParsingAsync();

            CreateIndexingFrames();
        }
        private void CreateIndexingFrames()
        {
            List<int> inputCsvIndexes = null;
            bool tryParse = int.TryParse(DifferenceTextBox.Text, out int offset);
            var sortedFrameRects = gBlueFrameImage.GetSortedRects(ParsingMode.Frame, tryParse ? offset : 0);

            gIndexingFrameModels.Clear();

            if (gInputCsvData != null)
                inputCsvIndexes = new List<int>(gInputCsvData.GetIndexOrder());

            for (int i = 0; i < sortedFrameRects.Count; i++)
            {
                IndexingFrameModel model = new IndexingFrameModel()
                {
                    Left = sortedFrameRects[i].X,
                    Top = sortedFrameRects[i].Y,
                    Right = sortedFrameRects[i].Right,
                    Bottom = sortedFrameRects[i].Bottom,
                    LedIndex = i.ToString(),
                };

                if (inputCsvIndexes != null)
                    model.LedIndex = inputCsvIndexes[i].ToString();

                gIndexingFrameModels.Add(model);
            }

            IndexingFrames.ItemsSource = gIndexingFrameModels;
        }
        #endregion

        #region -- Save --
        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (gBlueFrameImage == null || gIndexingFrameModels.Count == 0)
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
                    // For overwrite file
                    await FileIO.WriteBytesAsync(csvFile, new byte[0]);

                    if (gInputCsvData == null)
                        SaveCsv(csvFile);
                    else
                        SaveCsvBasingOnInputCsv(csvFile, gInputCsvData);

                    SavePathTextBlock.Text = csvFile.Path;
                }
                catch (Exception ee)
                {
                    SavePathTextBlock.Text = ee.ToString();
                }
            }
        }
        private async void SaveCsv(StorageFile csvFile)
        {
            // For overwrite file
            await FileIO.WriteBytesAsync(csvFile, new byte[0]);

            using (CsvFileWriter csvWriter = new CsvFileWriter(await csvFile.OpenStreamForWriteAsync()))
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

                for (int i = 0; i < gIndexingFrameModels.Count; i++)
                {
                    CsvRow row = new CsvRow
                        {
                            "LED " + i.ToString(),
                            1.ToString(), // exist
                            gIndexingFrameModels[i].Left.ToString(),
                            gIndexingFrameModels[i].Top.ToString(),
                            (gIndexingFrameModels[i].Right + 1).ToString(),
                            (gIndexingFrameModels[i].Bottom + 1).ToString(),
                            "", // PNG
                            1.ToString(), // Z index
                        };
                    csvWriter.WriteRow(row);
                }

                csvWriter.Close();
            }
        }
        private async void SaveCsvBasingOnInputCsv(StorageFile csvFile, InputCsvData inputCsvData)
        {
            using (CsvFileWriter csvWriter = new CsvFileWriter(await csvFile.OpenStreamForWriteAsync()))
            {
                List<CsvRow> copiedRows = inputCsvData.GetCopiedDataRows();
                FillCoordinate(copiedRows);
                int rowCount = copiedRows.Count;

                for (int i = 0; i < rowCount; i++)
                {
                    csvWriter.WriteRow(copiedRows[i]);
                }

                csvWriter.Close();
            }
        }
        private void FillCoordinate(List<CsvRow> copiedRows)
        {
            int rowCount = copiedRows.Count;

            int column_LeftTopX = gInputCsvData.Column_LeftTopX;
            int column_LeftTopY = gInputCsvData.Column_LeftTopY;
            int column_RightBottomX = gInputCsvData.Column_RightBottomX;
            int column_RightBottomY = gInputCsvData.Column_RightBottomY;

            for (int i = gInputCsvData.AppendRowStartIndex; i < rowCount; i++)
            {
                string index = copiedRows[i][0].ToLower().Replace("led", "").Replace(" ", "");
                IndexingFrameModel findModel = gIndexingFrameModels.FirstOrDefault(
                        model => model.LedIndex.ToLower().Replace("led", "").Replace(" ", "") == index);

                if (findModel != null)
                {
                    copiedRows[i][column_LeftTopX] = findModel.Left.ToString();
                    copiedRows[i][column_LeftTopY] = findModel.Top.ToString();
                    copiedRows[i][column_RightBottomX] = findModel.Right.ToString();
                    copiedRows[i][column_RightBottomY] = findModel.Bottom.ToString();
                }
            }
        }
        #endregion

        #region -- Preview --
        private async void SelectDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            FolderPicker folderPicker = new FolderPicker();
            folderPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            folderPicker.ViewMode = PickerViewMode.Thumbnail;
            folderPicker.FileTypeFilter.Add("*");

            StorageFolder folder = await folderPicker.PickSingleFolderAsync();

            if (folder != null)
                await StartPreviewAsync(folder);
        }
        private async Task StartPreviewAsync(StorageFolder folder)
        {
            try
            {
                VerifyCanvas.Children.Remove(gVerifyDV);

                DeviceModel dm = await DeviceModel.GetDeviceModel(folder);

                gVerifyDV = new DeviceView();
                gVerifyDV.DataContext = dm;
                VerifyCanvas.Children.Add(gVerifyDV);

                var allzones = dm.AllZones;
                SortByZIndex(allzones);
                SetMouseDectectedRegion(new Point(dm.PixelLeft, dm.PixelTop), allzones);

                VerifyStatus.Text = "";
            }
            catch
            {
                VerifyStatus.Text = "輸入錯誤!";
            }
        }
        private void SortByZIndex(List<ZoneModel> zones)
        {
            int count = zones.Count;

            // Bubble sort
            for (int i = 0; i < count - 1; i++)
            {
                for (int j = 0; j < count - 1 - i; j++)
                {
                    if (zones[j].Zindex < zones[j + 1].Zindex)
                    {
                        var z = zones[j];
                        zones[j] = zones[j + 1];
                        zones[j + 1] = z;
                    }
                }
            }
        }
        private void SetMouseDectectedRegion(Point basePt, List<ZoneModel> allzones)
        {
            var regions = new List<MouseDetectedRegion>();

            foreach (var zone in allzones)
            {
                Rect relative = zone.GetRect();
                Rect absolute = new Rect(
                    new Point(relative.Left + basePt.X, relative.Top + basePt.Y),
                    new Point(relative.Right + basePt.X, relative.Bottom + basePt.Y)
                    );

                MouseDetectedRegion r = new MouseDetectedRegion()
                {
                    RegionIndex = -1,
                    DetectionRect = absolute,
                };

                r.Callback = zone.OnReceiveMouseEvent;
                regions.Add(r);
            }

            gMouseEventCtrl.DetectionRegions = regions.ToArray();
        }
        #endregion

        private void SpaceGrid_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            var fe = sender as FrameworkElement;
            PointerPoint ptrPt = e.GetCurrentPoint(fe);
            Point Position = ptrPt.Position;

            if (ptrPt.Properties.IsLeftButtonPressed)
            {
                gMouseEventCtrl.OnMouseMoved(Position, true);
                bool _hasCapture = fe.CapturePointer(e.Pointer);
            }
            else
            {
                gMouseEventCtrl.OnMouseMoved(Position, false);
            }
        }

        public void DectectConflict()
        {
            for (int i = 0; i < gIndexingFrameModels.Count; i++)
            {
                gIndexingFrameModels[i].Conflict = false;
            }

            for (int i = 0; i < gIndexingFrameModels.Count; i++)
            {
                for (int j = i + 1; j < gIndexingFrameModels.Count; j++)
                {
                    if (gIndexingFrameModels[i].LedIndex == gIndexingFrameModels[j].LedIndex)
                    {
                        gIndexingFrameModels[i].Conflict = true;
                        gIndexingFrameModels[j].Conflict = true;
                    }
                }
            }
        }
    }
}
