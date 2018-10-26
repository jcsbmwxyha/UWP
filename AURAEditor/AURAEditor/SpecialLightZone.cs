using AuraEditor.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace AuraEditor
{
    class SpecialLightZone : LightZone
    {
        [ComImport]
        [Guid("5B0D3235-4DBA-4D44-865E-8F1D0E4FD04D")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        unsafe interface IMemoryBufferByteAccess
        {
            void GetBuffer(out byte* buffer, out uint capacity);
        }

        //public Image SpecialFrame;
        private SoftwareBitmap specialFrameSB;

        public SpecialLightZone(LedUI led) : base(led)
        {
        }
        public async Task CreateSpecialFrame(LedUI led)
        {
            StorageFile pngFile = await StorageFile.GetFileFromPathAsync(led.PNG_Path);

            using (IRandomAccessStream stream = await pngFile.OpenAsync(FileAccessMode.Read))
            {
                // Create the decoder from the stream
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);

                // Get the SoftwareBitmap representation of the file
                specialFrameSB = await decoder.GetSoftwareBitmapAsync();
            }

            SoftwareBitmapSource source = new SoftwareBitmapSource();
            specialFrameSB = SoftwareBitmap.Convert(specialFrameSB, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
            await source.SetBitmapAsync(specialFrameSB);

            int frameLeft = led.Left;
            int frameTop = led.Top;
            int frameRight = led.Right;
            int frameBottom = led.Bottom;

            CompositeTransform ct = new CompositeTransform
            {
                TranslateX = frameLeft,
                TranslateY = frameTop
            };

            Image image = new Image
            {
                RenderTransform = ct,
                Source = source,
                Width = frameRight - frameLeft,
                Height = frameBottom - frameTop,
            };

            MyFrameworkElement = image;
        }

        override public async void ChangeStatus(RegionStatus status)
        {
            Color color;
            if (status == RegionStatus.Normal)
            {
                color = Colors.Black;
                Selected = false;
            }
            else if (status == RegionStatus.NormalHover)
            {
                color = Colors.Green;
                Selected = false;
            }
            else if (status == RegionStatus.Selected)
            {
                color = Colors.Red;
                Selected = true;
            }
            else if (status == RegionStatus.Watching)
            {
                color = Colors.Yellow;
                Selected = true;
            }
            else
            {
                color = Colors.Red;
                Selected = true;
            }

            specialFrameSB = SoftwareBitmap.Convert(specialFrameSB, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight);
            ChangeSpecialFrameColor(color);
            specialFrameSB = SoftwareBitmap.Convert(specialFrameSB, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);

            var source = new SoftwareBitmapSource();
            await source.SetBitmapAsync(specialFrameSB);

            Image image = MyFrameworkElement as Image;
            image.Source = source;
        }
        private unsafe void ChangeSpecialFrameColor(Color c)
        {
            using (BitmapBuffer buffer = specialFrameSB.LockBuffer(BitmapBufferAccessMode.Write))
            {
                using (var reference = buffer.CreateReference())
                {
                    byte* dataInBytes;
                    uint capacity;
                    ((IMemoryBufferByteAccess)reference).GetBuffer(out dataInBytes, out capacity);

                    // Fill-in the BGRA plane
                    BitmapPlaneDescription bufferLayout = buffer.GetPlaneDescription(0);
                    double imgWidth = bufferLayout.Width;
                    double imgHeight = bufferLayout.Height;
                    double imgCenterW = bufferLayout.Width / 2;
                    double imgCenterH = bufferLayout.Height / 2;

                    for (int row = 0; row < imgHeight; row++)
                    {
                        for (int col = 0; col < imgWidth; col++)
                        {
                            int pixelIndex = bufferLayout.Stride * row + 4 * col;
                            if (dataInBytes[pixelIndex + 3] != 0)
                            {
                                dataInBytes[pixelIndex + 0] = (byte)c.B;
                                dataInBytes[pixelIndex + 1] = (byte)c.G;
                                dataInBytes[pixelIndex + 2] = (byte)c.R;
                            }
                        }
                    }
                }
            }
        }
    }
}
