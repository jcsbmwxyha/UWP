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

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        SoftwareBitmap g_softwareBitmap;
        List<Rect> keyRects = new List<Rect>();
        bool[,] HasPixelArray;
        int PngWidth;
        int PngHeight;

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
            {
                // The user cancelled the picking operation
                return;
            }

            MainGrid.Children.Clear();
            keyRects.Clear();
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
            
            g_softwareBitmap = softwareBitmap;
            var source = new SoftwareBitmapSource();
            await source.SetBitmapAsync(g_softwareBitmap);

            Image keyboardImg = new Image
            {
                Stretch = 0,
                HorizontalAlignment = 0,
                VerticalAlignment = 0,
                Source = source
            };
            MainGrid.Children.Add(keyboardImg);
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (g_softwareBitmap == null)
                return;

            GetPixelArray(g_softwareBitmap);
            FindKeys();
            DrawRectangle();
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (keyRects.Count == 0)
                return;

            string filename = "keys.txt";
            StorageFolder picturesLibrary = await KnownFolders.GetFolderForUserAsync(null /* current user */, KnownFolderId.PicturesLibrary);
            StorageFile outputFile = (StorageFile)await picturesLibrary.TryGetItemAsync(filename);

            if (outputFile == null)
            {
                outputFile = await picturesLibrary.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
            }
            outputFile = await picturesLibrary.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
            string result = "";

            int expandValue = 2;
            for (int i = 0; i < keyRects.Count; i++)
            {
                Rect r = keyRects[i];

                if (i % 10 == 0) // comment
                    result += "// key " + i + " ~ " + (i + 9).ToString() + "\r\n";

                result += "{ " + (r.Left - expandValue) + ", " + (r.Top - expandValue) + ", "
                    + (r.Right + expandValue) + ", " + (r.Bottom + expandValue) + "} ,\r\n";
            }

            if (!String.IsNullOrEmpty(result))
            {
                await FileIO.WriteTextAsync(outputFile, result);
            }

            PathTextBlock.Text = "Path: Pictures/" + filename;
        }

        private unsafe void GetPixelArray(SoftwareBitmap softwareBitmap)
        {
            using (BitmapBuffer buffer = softwareBitmap.LockBuffer(BitmapBufferAccessMode.Write))
            {
                using (var reference = buffer.CreateReference())
                {
                    byte* dataInBytes;
                    uint capacity;
                    ((IMemoryBufferByteAccess)reference).GetBuffer(out dataInBytes, out capacity);

                    // Fill-in the BGRA plane
                    BitmapPlaneDescription bufferLayout = buffer.GetPlaneDescription(0);

                    PngWidth = bufferLayout.Width;
                    PngHeight = bufferLayout.Height;
                    HasPixelArray = new bool[PngHeight, PngWidth];

                    for (int y = 0; y < PngHeight; y++)
                    {
                        for (int x = 0; x < PngWidth; x++)
                        {
                            int pixelIndex = bufferLayout.StartIndex + bufferLayout.Stride * y + 4 * x;
                            if (dataInBytes[pixelIndex + 3] != 0)
                            {
                                HasPixelArray[y, x] = true;
                            }
                            else
                            {
                                HasPixelArray[y, x] = false;
                            }
                        }
                    }
                }
            }
        }

        private void FindKeys()
        {
            for (int y = 0; y < PngHeight; y++)
            {
                for (int x = 0; x < PngWidth; x++)
                {
                    if ((HasPixelArray[y, x] == true))
                    {
                        bool needfindkey = true;

                        for (int index = 0; index < keyRects.Count; index++)
                        {
                            if (keyRects[index].Contains(new Point(x, y)))
                            {
                                needfindkey = false;
                                break;
                            }
                        }

                        if (needfindkey == true)
                        {
                            Point pixel = new Point(x, y);
                            FindKeyPoint(pixel, out Point lefttop, out Point rightbottom);
                            rightbottom = new Point(rightbottom.X + 1, rightbottom.Y + 1);
                            keyRects.Add(new Rect(lefttop, rightbottom));
                        }
                    }
                }
            }
        }
    
        private void FindKeyPoint(Point pixel, out Point lefttop, out Point rightbottom)
        {
            int left = (int)FindLeftmostPoint(pixel).X;
            int top = (int)FindTopPoint(pixel).Y;
            Point rightMostPoint = FindRightmostPoint(pixel);
            int right = (int)rightMostPoint.X;
            int bottom = (int)FindBottomPoint(rightMostPoint).Y;

            lefttop = new Point(left, top);
            rightbottom = new Point(right, bottom);
        }

        private Point FindLeftmostPoint(Point p)
        {
            int x = (int)p.X;
            int y = (int)p.Y;

            while (HasPixelArray[y, x] == true)
            {
                if (HasPixelArray[y, x - 1] == true)
                    x--;
                else
                    y++;
            }

            return new Point(x, y - 1);
        }

        private Point FindTopPoint(Point p)
        {
            return p;
        }

        private Point FindRightmostPoint(Point p)
        {
            int x = (int)p.X;
            int y = (int)p.Y;

            while (HasPixelArray[y, x] == true)
            {
                if (HasPixelArray[y, x + 1] == true)
                    x++;
                else
                    y++;
            }

            return new Point(x, y - 1);
        }

        private Point FindBottomPoint(Point p)
        {
            // p is the rightmost point
            int x = (int)p.X;
            int y = (int)p.Y;

            while (HasPixelArray[y, x] == true)
            {
                if (HasPixelArray[y + 1, x] == true)
                    y++;
                else
                    x--;
            }

            return new Point(x + 1, y);
        }

        private void DrawRectangle()
        {
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
                Width = Rect.Width,
                Height = Rect.Height,
                HorizontalAlignment = 0,
                VerticalAlignment = 0,
            };

            rectangle.Stroke = new SolidColorBrush(Colors.Red);

            return rectangle;
        }
    }
}
