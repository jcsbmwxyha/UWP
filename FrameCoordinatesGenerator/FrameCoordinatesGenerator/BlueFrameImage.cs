using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

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
    class BlueFrameImage
    {
        private SoftwareBitmap m_SoftwareBitmap;
        private bool[,] m_BluePixelArray;
        private SoftwareBitmapSource mSource;
        private int PixelWidth
        {
            get
            {
                return m_BluePixelArray.GetLength(1);
            }
        }
        private int PixelHeight
        {
            get
            {
                return m_BluePixelArray.GetLength(0);
            }
        }
        public SoftwareBitmapSource GetImageSource() { return mSource; }

        private BlueFrameImage(SoftwareBitmap softwareBitmap)
        {
            m_SoftwareBitmap = softwareBitmap;
            m_BluePixelArray = GetBluePixelArray(softwareBitmap);
        }
        static public async Task<BlueFrameImage> CreateInstanceAsync(SoftwareBitmap softwareBitmap)
        {
            BlueFrameImage bfi = new BlueFrameImage(softwareBitmap);
            bfi.mSource = new SoftwareBitmapSource();
            await bfi.mSource.SetBitmapAsync(softwareBitmap);

            return bfi;
        }


        private unsafe bool[,] GetBluePixelArray(SoftwareBitmap softwareBitmap)
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
            int width = m_BluePixelArray.GetLength(1);
            int height = m_BluePixelArray.GetLength(0);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if ((m_BluePixelArray[y, x] == true))
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
                new Point(right, bottom));
        }
        private Point FindLeftTopPoint(Point pixel)
        {
            int x = (int)pixel.X;
            int y = (int)pixel.Y;

            while (m_BluePixelArray[y, x] == true)
            {
                if (m_BluePixelArray[y, x - 1] == true)
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

            while (m_BluePixelArray[y, x] == true)
            {
                if (m_BluePixelArray[y, x + 1] == true)
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

            while (m_BluePixelArray[y, x] == true)
            {
                if (m_BluePixelArray[y + 1, x] == true)
                    y++;
                else
                    break;
            }

            return y;
        }

        private List<Rect> GetKeys()
        {
            List<Rect> result = new List<Rect>();
            int widthPixels = m_BluePixelArray.GetLength(1);
            int heightPixels = m_BluePixelArray.GetLength(0);

            for (int y = 0; y < heightPixels; y++)
            {
                for (int x = 0; x < widthPixels; x++)
                {
                    if ((m_BluePixelArray[y, x] == true))
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

            while (m_BluePixelArray[y, x] == true)
            {
                if (m_BluePixelArray[y, x - 1] == true)
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

            while (m_BluePixelArray[y, x] == true)
            {
                if (m_BluePixelArray[y, x + 1] == true)
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
            while (m_BluePixelArray[y, x] == true)
            {
                if (m_BluePixelArray[y + 1, x] == true)
                    y++;
                else
                    x--;
            }

            return new Point(x + 1, y);
        }

        public List<Rect> GetSortedRects(ParsingMode mode, int difference)
        {
            List<Rect> result;
            if (mode == ParsingMode.Frame)
                result = GetFrames();
            else
                result = GetKeys();

            return SortingByGroup(result, difference);
        }
        private List<Rect> SortingByGroup(List<Rect> rects, int difference)
        {
            List<Rect> result = new List<Rect>();
            rects = SortByY(rects);

            List<Rect> group = new List<Rect>();
            for (int i = 0; i < rects.Count; i++)
            {
                if (i == 0)
                {
                    group.Add(rects[i]);
                }
                else if (rects[i].Top - rects[i - 1].Top <= difference)
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
    }
}
