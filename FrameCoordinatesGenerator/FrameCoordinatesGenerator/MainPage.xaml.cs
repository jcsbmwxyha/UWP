using CsvParse;
using FrameCoordinatesGenerator.Common;
using FrameCoordinatesGenerator.Models;
using FrameCoordinatesGenerator.Views;
using System;
using System.Collections.Generic;
using System.IO;
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

        private MouseEventCtrl m_MouseEventCtrl;
        MySoftwareImage g_MySoftwareImage;
        Image currentImage;
        List<PreLoadFrameModel> g_PreLoadFrameModels;

        public MainPage()
        {
            Self = this;
            this.InitializeComponent();
            g_PreLoadFrameModels = new List<PreLoadFrameModel>();
            m_MouseEventCtrl = IntializeMouseEventCtrl();
        }

        public void OnLostFocus()
        {
            for (int i = 0; i < g_PreLoadFrameModels.Count; i++)
            {
                g_PreLoadFrameModels[i].Conflict = false;
            }

            for (int i = 0; i < g_PreLoadFrameModels.Count; i++)
            {
                for (int j = i + 1; j < g_PreLoadFrameModels.Count; j++)
                {
                    if (g_PreLoadFrameModels[i].IntIndex == g_PreLoadFrameModels[j].IntIndex)
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

            g_MySoftwareImage = new MySoftwareImage(softwareBitmap);
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

            new InputCsvData(csvFile);
            LoadCSVPathTextBlock.Text = csvFile.Path;
        }
        #endregion

        #region -- Start --
        List<Rect> g_LedRects;

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (g_MySoftwareImage == null)
                return;

            if (InputCsvData.Self != null)
                await InputCsvData.Self.StartParsingAsync();

            bool tryParse = int.TryParse(DifferenceTextBox.Text, out int offset);

            if (tryParse == false)
                g_LedRects = g_MySoftwareImage.GetSortedRects(ParsingMode.Frame);
            else
                g_LedRects = g_MySoftwareImage.GetSortedRects(ParsingMode.Frame, offset);

            PreLoad(g_LedRects);
        }
        private void PreLoad(List<Rect> frameRects)
        {
            g_PreLoadFrameModels.Clear();
            ImageGrid.Children.Clear();
            ImageGrid.Children.Add(currentImage);
            List<int> indexex = null;

            if (InputCsvData.Self != null)
                indexex = new List<int>(InputCsvData.Self.GetIndexOrder());

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

                if (indexex != null)
                    model.LedIndex = indexex[i].ToString();

                view.DataContext = model;
                ImageGrid.Children.Add(view);
                g_PreLoadFrameModels.Add(model);
            }
        }
        #endregion

        #region -- Save --
        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (g_MySoftwareImage == null || g_PreLoadFrameModels.Count == 0)
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
                    InputCsvData inputCsvData = InputCsvData.Self;

                    if (inputCsvData == null)
                    {
                        SaveCsv(csvFile);
                    }
                    else
                    {
                        SaveCsvBasingOnInputCsv(csvFile, inputCsvData);
                    }

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
            InputCsvData inputCsvData = InputCsvData.Self;
            int rowCount = copiedRows.Count;

            int column_LeftTopX = inputCsvData.Column_LeftTopX;
            int column_LeftTopY = inputCsvData.Column_LeftTopY;
            int column_RightBottomX = inputCsvData.Column_RightBottomX;
            int column_RightBottomY = inputCsvData.Column_RightBottomY;

            for (int i = inputCsvData.AppendRowStartIndex; i < rowCount; i++)
            {
                string index = copiedRows[i][0].ToLower().Replace("led", "").Replace(" ", "");
                PreLoadFrameModel findModel = g_PreLoadFrameModels.Find(
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

        private async void PreviewButton_Click(object sender, RoutedEventArgs e)
        {
            FolderPicker folderPicker = new FolderPicker();
            folderPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            folderPicker.ViewMode = PickerViewMode.Thumbnail;
            folderPicker.FileTypeFilter.Add("*");

            StorageFolder folder = await folderPicker.PickSingleFolderAsync();

            if (folder == null)
                return;

            PreviewCanvas.Children.Clear();
            PreviewCanvas.Children.Add(GridImage);

            DeviceContent dc = await GetDeviceContent(folder);
            DeviceModel dm = await dc.ToDeviceModel(folder, new Point(120, 120));

            DeviceView view = new DeviceView();
            view.DataContext = dm;
            PreviewCanvas.Children.Add(view);

            List<ZoneModel> allzones = dm.AllZones;
            SortByZIndex(allzones);
            List<MouseDetectedRegion> regions = new List<MouseDetectedRegion>();

            foreach (var zone in allzones)
            {
                Rect relative = zone.GetRect();
                Rect absolute = new Rect(
                    new Point(relative.Left + dm.PixelLeft, relative.Top + dm.PixelTop),
                    new Point(relative.Right + dm.PixelLeft, relative.Bottom + dm.PixelTop)
                    );

                MouseDetectedRegion r = new MouseDetectedRegion()
                {
                    RegionIndex = -1,
                    DetectionRect = absolute,
                    GroupIndex = dm.Type
                };

                r.Callback = zone.OnReceiveMouseEvent;

                regions.Add(r);
            }
            m_MouseEventCtrl.DetectionRegions = regions.ToArray();
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

        private async Task<DeviceContent> GetDeviceContent(StorageFolder folder)
        {
            string modelName = folder.Name;

            DeviceContent deviceContent = new DeviceContent();
            
            StorageFile csvFile = await folder.GetFileAsync(modelName + ".csv");
            StorageFile pngFile = await folder.GetFileAsync(modelName + ".png");
            double rateW = 0;
            double rateH = 0;

            deviceContent.DeviceName = modelName;

            int exist_Column = -1;
            int leftTopX_Column = -1;
            int leftTopY_Column = -1;
            int rightBottomX_Column = -1;
            int rightBottomY_Column = -1;
            int z_Column = -1;
            int png_Column = -1;

            int gridW = 5, gridH = 5;
            int originalPixelWidth = 1000;
            int originalPixelHeight = 1000;

            if (pngFile != null)
            {
                using (IRandomAccessStream fileStream = await pngFile.OpenAsync(FileAccessMode.Read))
                {
                    BitmapImage bitmapImage = new BitmapImage();

                    bitmapImage.SetSource(fileStream);
                    deviceContent.Image = bitmapImage;
                    originalPixelWidth = bitmapImage.PixelWidth;
                    originalPixelHeight = bitmapImage.PixelHeight;
                }
            }

            if (csvFile != null)
            {
                using (CsvFileReader csvReader = new CsvFileReader(await csvFile.OpenStreamForReadAsync()))
                {
                    CsvRow row = new CsvRow();
                    while (csvReader.ReadRow(row))
                    {
                        if (row[0].ToLower() == "gridwidth")
                        {
                            gridW = Int32.Parse(row[1]);
                            rateW = (double)(gridW * GridPixels) / originalPixelWidth;
                        }
                        else if (row[0].ToLower() == "gridheight")
                        {
                            gridH = Int32.Parse(row[1]);
                            rateH = (double)(gridH * GridPixels) / originalPixelHeight;
                        }
                        else if (row[0].ToLower() == "parameters")
                        {
                            for (int i = 0; i < row.Count; i++)
                            {
                                if (row[i].ToLower() == "exist") { exist_Column = i; }
                                else if (row[i].ToLower() == "lefttop_x") { leftTopX_Column = i; }
                                else if (row[i].ToLower() == "lefttop_y") { leftTopY_Column = i; }
                                else if (row[i].ToLower() == "rightbottom_x") { rightBottomX_Column = i; }
                                else if (row[i].ToLower() == "rightbottom_y") { rightBottomY_Column = i; }
                                else if (row[i].ToLower() == "z_index") { z_Column = i; }
                                else if (row[i].ToLower() == "png") { png_Column = i; }
                            }
                        }
                        else if (row[0].ToLower().Contains("led "))
                        {
                            if (row[exist_Column] != "1")
                                continue;

                            LedUI ledui = new LedUI()
                            {
                                Index = Int32.Parse(row[0].ToLower().Substring("led ".Length)),
                                Left = (int)Math.Round(Double.Parse(row[leftTopX_Column]) * rateW, 0),
                                Top = (int)Math.Round(Double.Parse(row[leftTopY_Column]) * rateH, 0),
                                Right = (int)Math.Round(Double.Parse(row[rightBottomX_Column]) * rateW, 0),
                                Bottom = (int)Math.Round(Double.Parse(row[rightBottomY_Column]) * rateH, 0),
                                ZIndex = Int32.Parse(row[z_Column]),
                            };

                            if (png_Column != -1 && row[png_Column] != "")
                                ledui.PNG_Path = row[png_Column];

                            deviceContent.Leds.Add(ledui);
                        }
                    }
                }
            }

            deviceContent.GridWidth = gridW;
            deviceContent.GridHeight = gridH;
            return deviceContent;
        }
        
        private void SpaceGrid_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            var fe = sender as FrameworkElement;
            PointerPoint ptrPt = e.GetCurrentPoint(fe);
            Point Position = ptrPt.Position;

            if (ptrPt.Properties.IsLeftButtonPressed)
            {
                m_MouseEventCtrl.OnMouseMoved(Position, true);
                bool _hasCapture = fe.CapturePointer(e.Pointer);
            }
            else
            {
                m_MouseEventCtrl.OnMouseMoved(Position, false);
            }
        }

        private MouseEventCtrl IntializeMouseEventCtrl()
        {
            List<MouseDetectedRegion> regions = new List<MouseDetectedRegion>();

            MouseEventCtrl mec = new MouseEventCtrl
            {
                MonitorMaxRect = new Rect(new Point(0, 0), new Point(1600, 1000)),
                DetectionRegions = regions.ToArray()
            };

            return mec;
        }
    }
}
