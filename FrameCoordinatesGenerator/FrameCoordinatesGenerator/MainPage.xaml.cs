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

    class ImagePixelStructure
    {
        enum DetectMode
        {
            frame = 0,
            key = 1,
        }

        private SoftwareBitmap m_softwareBitmap;
        private List<Rect> m_frameRects;
        private bool[,] m_pixelBoolArray;
        private DetectMode mode = DetectMode.frame;

        public ImagePixelStructure(SoftwareBitmap softwareBitmap)
        {
            m_softwareBitmap = softwareBitmap;
            m_pixelBoolArray = GetBoolPixelArray(softwareBitmap);
            m_frameRects = FindFrames();
        }

        public List<Rect> GetFrameRects()
        {
            return m_frameRects;
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

                            if (dataInBytes[pixelIndex + 0] - dataInBytes[pixelIndex + 1] >= 30)
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
        private List<Rect> FindFrames()
        {
            List<Rect> frames = new List<Rect>();
            int widthPixels = m_pixelBoolArray.GetLength(1);
            int heightPixels = m_pixelBoolArray.GetLength(0);

            for (int y = 0; y < heightPixels; y++)
            {
                for (int x = 0; x < widthPixels; x++)
                {
                    if ((m_pixelBoolArray[y, x] == true))
                    {
                        Point pixel = new Point(x, y);

                        if (!AlreadyHasFrame(frames, pixel))
                        {
                            if (mode == DetectMode.frame)
                                frames.Add(GetFrame(pixel));
                            else
                                frames.Add(GetKey(pixel));
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
                if (mode == DetectMode.key)
                {
                    if (frameRect.Contains(pixel))
                    {
                        return true;
                    }
                }

                if ((frameRect.X == leftTop.X) && (frameRect.Y == leftTop.Y))
                {
                    return true;
                }
            }

            return false;
        }
        private Point FindLeftTopPoint(Point pixel)
        {
            int x = (int)pixel.X;
            int y = (int)pixel.Y;

            while (m_pixelBoolArray[y, x] == true)
            {
                if (m_pixelBoolArray[y, x - 1] == true)
                    x--;
                else
                    y--;
            }

            return new Point(x, y + 1);
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
        private int GetRightmostValue(Point firstPixel)
        {
            int x = (int)firstPixel.X;
            int y = (int)firstPixel.Y;

            while (m_pixelBoolArray[y, x] == true)
            {
                if (m_pixelBoolArray[y, x + 1] == true)
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

            while (m_pixelBoolArray[y, x] == true)
            {
                if (m_pixelBoolArray[y + 1, x] == true)
                    y++;
                else
                    break;
            }

            return y;
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

            while (m_pixelBoolArray[y, x] == true)
            {
                if (m_pixelBoolArray[y, x - 1] == true)
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

            while (m_pixelBoolArray[y, x] == true)
            {
                if (m_pixelBoolArray[y, x + 1] == true)
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
            while (m_pixelBoolArray[y, x] == true)
            {
                if (m_pixelBoolArray[y + 1, x] == true)
                    y++;
                else
                    x--;
            }

            return new Point(x + 1, y);
        }
    }

    public sealed partial class MainPage : Page
    {
        ImagePixelStructure g_CurrentImageInfo;

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

            g_CurrentImageInfo = new ImagePixelStructure(softwareBitmap);
            await SetMainGridImage(softwareBitmap);
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

            MainGrid.Children.Clear();
            MainGrid.Children.Add(image);
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (g_CurrentImageInfo == null)
                return;

            List<Rect> frames = g_CurrentImageInfo.GetFrameRects();
            ShowResultOfFrames(frames);
        }
        private void ShowResultOfFrames(List<Rect> frameRects)
        {
            for (int i = 0; i < frameRects.Count; i++)
            {
                Rectangle rectangle = CreateRectangle(frameRects[i]);
                TextBlock textBlock = CreateTextBlock(frameRects[i], (i + 1).ToString());

                MainGrid.Children.Add(rectangle);
                MainGrid.Children.Add(textBlock);
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
            if (g_CurrentImageInfo == null || g_CurrentImageInfo.GetFrameRects().Count == 0)
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
                    PathTextBlock.Text = csvFile.Path;
                }
                catch (Exception ee)
                {
                    PathTextBlock.Text = ee.ToString();
                }
            }
        }
        private async void SaveResultToCsv(StorageFile csvFile)
        {
            // For overwrite file
            await FileIO.WriteBytesAsync(csvFile, new byte[0]);

            using (CsvFileWriter csvWriter = new CsvFileWriter(await csvFile.OpenStreamForWriteAsync()))
            {
                List<Rect> frameRects = g_CurrentImageInfo.GetFrameRects();
                CsvRow firstRow = new CsvRow
                    {
                        "Index",
                        "LeftTop_x",
                        "LeftTop_y",
                        "RightBottom_x",
                        "RightBottom_y"
                    };
                csvWriter.WriteRow(firstRow);

                for (int i = 0; i < frameRects.Count; i++)
                {
                    CsvRow row = new CsvRow
                    {
                        "Frame " + (i + 1).ToString(),
                        frameRects[i].X.ToString(),
                        frameRects[i].Y.ToString(),
                        (frameRects[i].Right + 1).ToString(),
                        (frameRects[i].Bottom + 1).ToString()
                    };

                    csvWriter.WriteRow(row);
                }

                csvWriter.Close();
            }
        }
    }
}
