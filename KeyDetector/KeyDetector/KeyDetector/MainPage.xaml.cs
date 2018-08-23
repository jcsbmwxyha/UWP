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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace KeyDetector
{
    [ComImport]
    [Guid("5b0d3235-4dba-4d44-865e-8f1d0e4fd04d")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    unsafe interface IMemoryBufferByteAccess
    {
        void GetBuffer(out byte* buffer, out uint capacity);
    }

    class ImageInfo
    {
        public SoftwareBitmap SoftwareBitmap { get; set; }
        public List<Rect> KeyRects { get; set; }
        public bool[,] HasPixelArray { get; set; }

        public ImageInfo()
        {
            KeyRects = new List<Rect>();
        }
    }

    public sealed partial class MainPage : Page
    {
        ImageInfo g_CurrentImageInfo = new ImageInfo();

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

            g_CurrentImageInfo.SoftwareBitmap = softwareBitmap;
            await SetMainGridImage(softwareBitmap);
        }
        private async Task SetMainGridImage(SoftwareBitmap softwareBitmap)
        {
            var source = new SoftwareBitmapSource();
            await source.SetBitmapAsync(softwareBitmap);
            Image keyboardImg = new Image
            {
                Stretch = 0,
                HorizontalAlignment = 0,
                VerticalAlignment = 0,
                Source = source
            };

            MainGrid.Children.Clear();
            MainGrid.Children.Add(keyboardImg);
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (g_CurrentImageInfo.SoftwareBitmap == null)
                return;

            GetHasPixelArray();
            FindKeys();
            ShowResultOfKeys();
        }
        private unsafe void GetHasPixelArray()
        {
            SoftwareBitmap softwareBitmap = g_CurrentImageInfo.SoftwareBitmap;
            int widthPixels;
            int heightPixels;
            bool[,] hasPixelArray;

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
                    hasPixelArray = new bool[heightPixels, widthPixels];

                    for (int y = 0; y < heightPixels; y++)
                    {
                        for (int x = 0; x < widthPixels; x++)
                        {
                            int pixelIndex = bufferLayout.StartIndex + bufferLayout.Stride * y + 4 * x;

                            if (dataInBytes[pixelIndex + 3] != 0)
                            {
                                hasPixelArray[y, x] = true;
                            }
                            else
                            {
                                hasPixelArray[y, x] = false;
                            }
                        }
                    }
                }
            }

            g_CurrentImageInfo.HasPixelArray = hasPixelArray;
        }
        private void FindKeys()
        {
            List<Rect> keyRects = g_CurrentImageInfo.KeyRects;
            bool[,] hasPixelArray = g_CurrentImageInfo.HasPixelArray;
            int widthPixels = hasPixelArray.GetLength(1);
            int heightPixels = hasPixelArray.GetLength(0);

            for (int y = 0; y < heightPixels; y++)
            {
                for (int x = 0; x < widthPixels; x++)
                {
                    if ((hasPixelArray[y, x] == true))
                    {
                        Point pixel = new Point(x, y);

                        if (!OnOneOfKeyRects(pixel))
                        {
                            keyRects.Add(FindKeyPoint(pixel));
                        }
                    }
                }
            }
        }
        private bool OnOneOfKeyRects(Point p)
        {
            List<Rect> keyRects = g_CurrentImageInfo.KeyRects;

            for (int index = 0; index < keyRects.Count; index++)
            {
                if (keyRects[index].Contains(p))
                {
                    return true;
                }
            }
            return false;
        }
        private Rect FindKeyPoint(Point firstPixel)
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
            bool[,] hasPixelArray = g_CurrentImageInfo.HasPixelArray;

            while (hasPixelArray[y, x] == true)
            {
                if (hasPixelArray[y, x - 1] == true)
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
            bool[,] hasPixelArray = g_CurrentImageInfo.HasPixelArray;

            while (hasPixelArray[y, x] == true)
            {
                if (hasPixelArray[y, x + 1] == true)
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
            bool[,] hasPixelArray = g_CurrentImageInfo.HasPixelArray;

            // Start from rightmost point
            while (hasPixelArray[y, x] == true)
            {
                if (hasPixelArray[y + 1, x] == true)
                    y++;
                else
                    x--;
            }

            return new Point(x + 1, y);
        }
        private void ShowResultOfKeys()
        {
            List<Rect> keyRects = g_CurrentImageInfo.KeyRects;

            for (int i = 0; i < keyRects.Count; i++)
            {
                Rectangle rectangle = CreateRectangle(keyRects[i]);
                MainGrid.Children.Add(rectangle);
            }
        }
        private Rectangle CreateRectangle(Windows.Foundation.Rect Rect)
        {
            CompositeTransform ct = new CompositeTransform
            {
                TranslateX = Rect.X,
                TranslateY = Rect.Y
            };

            Rectangle rectangle = new Rectangle
            {
                Fill = new SolidColorBrush(Colors.Transparent),
                StrokeThickness = 1,
                RenderTransform = ct,
                Width = Rect.Width + 1,
                Height = Rect.Height + 1,
                HorizontalAlignment = 0,
                VerticalAlignment = 0,
            };

            rectangle.Stroke = new SolidColorBrush(Colors.Red);

            return rectangle;
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (g_CurrentImageInfo.KeyRects.Count == 0)
                return;

            string result = GetCodingResultString();
            var savePicker = new FileSavePicker();
            StorageFile saveFile;

            savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            savePicker.FileTypeChoices.Add("Plain Text", new List<string>() { ".txt" });
            savePicker.SuggestedFileName = "MyTxt";
            saveFile = await savePicker.PickSaveFileAsync();

            if (saveFile != null)
            {
                // Prevent updates to the remote version of the file until
                // we finish making changes and call CompleteUpdatesAsync.
                Windows.Storage.CachedFileManager.DeferUpdates(saveFile);

                // write to file
                await Windows.Storage.FileIO.WriteTextAsync(saveFile, result);

                // Let Windows know that we're finished changing the file so
                // the other app can update the remote version of the file.
                // Completing updates may require Windows to ask for user input.
                Windows.Storage.Provider.FileUpdateStatus status =
                    await Windows.Storage.CachedFileManager.CompleteUpdatesAsync(saveFile);

                PathTextBlock.Text = saveFile.Path;
            }
        }
        private string GetCodingResultString()
        {
            List<Rect> keyRects = g_CurrentImageInfo.KeyRects;
            string result = "";

            for (int i = 0; i < keyRects.Count; i++)
            {
                Rect r = AddPaddingValue(keyRects[i]);

                // comment
                if (i % 10 == 0)
                {
                    result += "// key " + i + " ~ " + (i + 9).ToString() + "\r\n";
                }

                result +=
                    "{ " + r.Left +
                    ", " + r.Top +
                    ", " + r.Right +
                    ", " + r.Bottom + "} ,\r\n";
            }

            return result;
        }
        private Rect AddPaddingValue(Rect r)
        {
            int paddingValue = 2;

            return new Rect(
                new Point(r.Left - paddingValue, r.Top - paddingValue),
                new Point(r.Right + paddingValue, r.Bottom + paddingValue)
                );
        }
        private string GetCsvResultString()
        {
            string result = "";

            // TODO

            return result;
        }
    }
}
