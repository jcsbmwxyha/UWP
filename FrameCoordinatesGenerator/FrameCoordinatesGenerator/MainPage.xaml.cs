using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using Colors = Windows.UI.Colors;
using CsvParse;

namespace FrameCoordinatesGenerator
{
    [ComImport]
    [Guid("5b0d3235-4dba-4d44-865e-8f1d0e4fd04d")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    unsafe interface IMemoryBufferByteAccess
    {
        void GetBuffer(out byte* buffer, out uint capacity);
    }

    enum ParsingMode
    {
        Frame = 0,
        Key = 1,
    }
    class ImagePixelStructure
    {
        private SoftwareBitmap m_SoftwareBitmap;
        public bool[,] m_PixelBoolArray;

        public ImagePixelStructure(SoftwareBitmap softwareBitmap)
        {
            m_SoftwareBitmap = softwareBitmap;
            m_PixelBoolArray = GetBoolPixelArray(softwareBitmap);
        }
        private unsafe bool[,] GetBoolPixelArray(SoftwareBitmap softwareBitmap)
        {
            int widthPixels;
            int heightPixels;
            bool[,] pixelBoolArray;

            using (BitmapBuffer buffer = softwareBitmap.LockBuffer(BitmapBufferAccessMode.Write))
            {
                using (var reference = buffer.CreateReference())
                {
                    byte* dataInBytes;
                    uint capacity;
                    ((IMemoryBufferByteAccess)reference).GetBuffer(out dataInBytes, out capacity);

                    // Fill-in the BGRA plane
                    BitmapPlaneDescription bufferLayout = buffer.GetPlaneDescription(0);

                    widthPixels = bufferLayout.Width;
                    heightPixels = bufferLayout.Height;
                    pixelBoolArray = new bool[heightPixels, widthPixels];

                    for (int y = 0; y < heightPixels; y++)
                    {
                        for (int x = 0; x < widthPixels; x++)
                        {
                            int pixelIndex = bufferLayout.StartIndex + bufferLayout.Stride * y + 4 * x;

                            if (dataInBytes[pixelIndex + 0] == 255 &&
                                dataInBytes[pixelIndex + 1] == 0 &&
                                dataInBytes[pixelIndex + 2] == 0)
                            {
                                pixelBoolArray[y, x] = true;
                            }
                            else
                            {
                                pixelBoolArray[y, x] = false;
                            }
                        }
                    }
                }
            }

            return pixelBoolArray;
        }

        private List<Rect> GetFrames()
        {
            List<Rect> frames = new List<Rect>();
            int widthPixels = m_PixelBoolArray.GetLength(1);
            int heightPixels = m_PixelBoolArray.GetLength(0);

            for (int y = 0; y < heightPixels; y++)
            {
                for (int x = 0; x < widthPixels; x++)
                {
                    if ((m_PixelBoolArray[y, x] == true))
                    {
                        Point pixel = new Point(x, y);

                        if (!AlreadyHasFrame(frames, pixel))
                        {
                            frames.Add(GetFrame(pixel));
                        }
                    }
                }
            }

            return frames;
        }
        private bool AlreadyHasFrame(List<Rect> frames, Point pixel)
        {
            Point leftTop = FindLeftTopPoint(pixel);

            foreach (var frameRect in frames)
            {
                if ((frameRect.X == leftTop.X) && (frameRect.Y == leftTop.Y))
                {
                    return true;
                }
            }

            return false;
        }
        private Rect GetFrame(Point firstPixel)
        {
            int left = (int)firstPixel.X;
            int top = (int)firstPixel.Y;
            int right = GetRightmostValue(firstPixel);
            int bottom = GetBottomValue(firstPixel);

            return new Rect(
                new Point(left, top),
                new Point(right, bottom)
                );
        }
        private Point FindLeftTopPoint(Point pixel)
        {
            int x = (int)pixel.X;
            int y = (int)pixel.Y;

            while (m_PixelBoolArray[y, x] == true)
            {
                if (m_PixelBoolArray[y, x - 1] == true)
                    x--;
                else
                    y--;
            }

            return new Point(x, y + 1);
        }
        private int GetRightmostValue(Point firstPixel)
        {
            int x = (int)firstPixel.X;
            int y = (int)firstPixel.Y;

            while (m_PixelBoolArray[y, x] == true)
            {
                if (m_PixelBoolArray[y, x + 1] == true)
                    x++;
                else
                    break;
            }

            return x;
        }
        private int GetBottomValue(Point firstPixel)
        {
            int x = (int)firstPixel.X;
            int y = (int)firstPixel.Y;

            while (m_PixelBoolArray[y, x] == true)
            {
                if (m_PixelBoolArray[y + 1, x] == true)
                    y++;
                else
                    break;
            }

            return y;
        }

        private List<Rect> GetKeys()
        {
            List<Rect> result = new List<Rect>();
            int widthPixels = m_PixelBoolArray.GetLength(1);
            int heightPixels = m_PixelBoolArray.GetLength(0);

            for (int y = 0; y < heightPixels; y++)
            {
                for (int x = 0; x < widthPixels; x++)
                {
                    if ((m_PixelBoolArray[y, x] == true))
                    {
                        Point pixel = new Point(x, y);

                        if (!AlreadyHasKey(result, pixel))
                        {
                            result.Add(GetKey(pixel));
                        }
                    }
                }
            }

            return result;
        }
        private bool AlreadyHasKey(List<Rect> keys, Point pixel)
        {
            foreach (var key in keys)
            {
                if (key.Contains(pixel))
                {
                    return true;
                }
            }

            return false;
        }
        private Rect GetKey(Point firstPixel)
        {
            int left = (int)FindLeftmostPoint(firstPixel).X;
            int top = (int)FindTopPoint(firstPixel).Y;

            Point rightmostPoint = FindRightmostPoint(firstPixel);
            int right = (int)rightmostPoint.X;
            int bottom = (int)FindBottomPoint(rightmostPoint).Y;

            return new Rect(
                new Point(left, top),
                new Point(right, bottom)
                );
        }
        private Point FindLeftmostPoint(Point firstPixel)
        {
            int x = (int)firstPixel.X;
            int y = (int)firstPixel.Y;

            while (m_PixelBoolArray[y, x] == true)
            {
                if (m_PixelBoolArray[y, x - 1] == true)
                    x--;
                else
                    y++;
            }

            return new Point(x, y - 1);
        }
        private Point FindTopPoint(Point firstPixel)
        {
            return firstPixel;
        }
        private Point FindRightmostPoint(Point firstPixel)
        {
            int x = (int)firstPixel.X;
            int y = (int)firstPixel.Y;

            while (m_PixelBoolArray[y, x] == true)
            {
                if (m_PixelBoolArray[y, x + 1] == true)
                    x++;
                else
                    y++;
            }

            return new Point(x, y - 1);
        }
        private Point FindBottomPoint(Point rightmostPixel)
        {
            int x = (int)rightmostPixel.X;
            int y = (int)rightmostPixel.Y;

            // Start from rightmost point
            while (m_PixelBoolArray[y, x] == true)
            {
                if (m_PixelBoolArray[y + 1, x] == true)
                    y++;
                else
                    x--;
            }

            return new Point(x + 1, y);
        }

        public List<Rect> GetRects(ParsingMode mode)
        {
            if (mode == ParsingMode.Frame)
                return GetFrames();
            else if (mode == ParsingMode.Key)
                return GetKeys();
            else
                return GetFrames();
        }
    }

    public sealed partial class MainPage : Page
    {
        ImagePixelStructure g_ImagePixelStructure;
        List<Rect> result;

        public MainPage()
        {
            this.InitializeComponent();
        }

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
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (g_ImagePixelStructure == null)
                return;

            result = g_ImagePixelStructure.GetRects(ParsingMode.Frame);
            ShowResult(result);
        }
        private void ShowResult(List<Rect> frameRects)
        {
            for (int i = 0; i < frameRects.Count; i++)
            {
                Rectangle rectangle = CreateRectangle(frameRects[i]);
                TextBlock textBlock = CreateTextBlock(frameRects[i], (i + 1).ToString());

                ImageGrid.Children.Add(rectangle);
                ImageGrid.Children.Add(textBlock);
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
        private TextBlock CreateTextBlock(Rect rect, string index)
        {
            CompositeTransform ct = new CompositeTransform
            {
                TranslateX = rect.X,
                TranslateY = rect.Y
            };
            TextBlock textBlock = new TextBlock
            {
                Foreground = new SolidColorBrush(Colors.Red),
                RenderTransform = ct,
                Width = rect.Width,
                Height = rect.Height,
                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = 0,
                VerticalAlignment = 0,
                Text = index,
                FontSize = 10,
            };

            return textBlock;
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if(result==null)
                result = g_ImagePixelStructure.GetRects(ParsingMode.Frame);

            if (g_ImagePixelStructure == null || result.Count == 0)
                return;

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

            double rateW = 1 / 6f;
            double rateH = 1 / 6f;
            int sourceW = g_ImagePixelStructure.m_PixelBoolArray.GetLength(1);
            int sourceH = g_ImagePixelStructure.m_PixelBoolArray.GetLength(0);

            //if(int.TryParse(TargetWidthTextBox.Text, out int targetW))
            //{
            //    rateW = (double)targetW / (double)sourceW;
            //}
            //if (int.TryParse(TargetHeightTextBox.Text, out int targetH))
            //{
            //    rateH = (double)targetH / (double)sourceH;
            //}

            using (CsvFileWriter csvWriter = new CsvFileWriter(await csvFile.OpenStreamForWriteAsync()))
            {
                CsvRow firstRow = new CsvRow
                    {
                        "Model",
                        csvFile.DisplayName
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
                        "Z_index"
                    };
                csvWriter.WriteRow(secondRow);

                for (int i = 0; i < result.Count; i++)
                {
                    CsvRow row = new CsvRow
                    {
                        "LED " + i.ToString(),
                        1.ToString(), // exist
                        Math.Floor(result[i].X * rateW).ToString(),
                        Math.Floor(result[i].Y * rateH).ToString(),
                        Math.Ceiling((result[i].Right + 1) * rateW).ToString(),
                        Math.Ceiling((result[i].Bottom + 1) * rateH).ToString(),
                        1.ToString(), // Z index
                    };

                    csvWriter.WriteRow(row);
                }

                csvWriter.Close();
            }
        }
    }
}
