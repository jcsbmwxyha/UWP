using AuraEditor.Common;
using System;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using Windows.Graphics.Imaging;
using System.Runtime.InteropServices;
using static AuraEditor.Common.ControlHelper;
using Windows.Storage;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Core;
using Windows.ApplicationModel.Core;

namespace AuraEditor
{
    public class LightZone
    {
        [ComImport]
        [Guid("5B0D3235-4DBA-4D44-865E-8F1D0E4FD04D")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        unsafe interface IMemoryBufferByteAccess
        {
            void GetBuffer(out byte* buffer, out uint capacity);
        }

        public Shape Frame;
        public Image SpecialFrame;
        private SoftwareBitmap specialFrameSB;

        public int Index;
        public int ZIndex;
        public Rect RelativeZoneRect
        {
            get
            {
                CompositeTransform ct;ct = Frame.RenderTransform as CompositeTransform;

                return new Rect(
                    new Point(
                        ct.TranslateX, //x
                        ct.TranslateY), //y
                    new Point(
                        ct.TranslateX + Frame.Width, //w
                        ct.TranslateY + Frame.Height) //h
                );
            }
        }
        public Rect AbsoluteZoneRect
        {
            get
            {
                Border b;
                if (SpecialFrame != null)
                    b = FindParentControl<Border>(SpecialFrame, typeof(Border));
                else
                    b = FindParentControl<Border>(Frame, typeof(Border));
                CompositeTransform ct = b.RenderTransform as CompositeTransform;

                return new Rect(
                    new Point(
                        ct.TranslateX + RelativeZoneRect.Left, //x
                        ct.TranslateY + RelativeZoneRect.Top), //y
                    new Point(
                        ct.TranslateX + RelativeZoneRect.Left + RelativeZoneRect.Width, //w
                        ct.TranslateY + RelativeZoneRect.Top + RelativeZoneRect.Height) //h
                );
            }
        }
        public bool Selected;

        public LightZone(Point deviceGridPosition, LedUI led)
        {
            int frameLeft = led.Left;
            int frameTop = led.Top;
            int frameRight = led.Right;
            int frameBottom = led.Bottom;

            Index = led.Index;
            Selected = false;
            Frame = CreateRectangle(new Rect(
                    new Point(frameLeft, frameTop),
                    new Point(frameRight, frameBottom))
                    );
            Frame.SetValue(Canvas.ZIndexProperty, led.ZIndex);
            ZIndex = led.ZIndex;
        }

        public async Task CreateSpecialFrame(string path)
        {
            StorageFile pngFile = await StorageFile.GetFileFromPathAsync(path);

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

            SpecialFrame = new Image();
            CompositeTransform ct = new CompositeTransform();
            //ct.TranslateX
            SpecialFrame.RenderTransform = new CompositeTransform();
            SpecialFrame.Source = source;
        }

        private Rectangle CreateRectangle(Rect Rect)
        {
            CompositeTransform ct = new CompositeTransform
            {
                TranslateX = Rect.X,
                TranslateY = Rect.Y
            };

            Rectangle rectangle = new Rectangle
            {
                Fill = new SolidColorBrush(Colors.Transparent),
                StrokeThickness = 2,
                RenderTransform = ct,
                Width = Rect.Width,
                Height = Rect.Height,
                HorizontalAlignment = 0,
                VerticalAlignment = 0,
                RadiusX = 3,
                RadiusY = 4
            };

            rectangle.Stroke = new SolidColorBrush(Colors.Black);

            return rectangle;
        }
        public void Frame_StatusChanged(int regionIndex, RegionStatus status)
        {
            if (status == RegionStatus.Normal)
            {
                Frame.Stroke = new SolidColorBrush(Colors.Black);
                Frame.Fill = new SolidColorBrush(Colors.Transparent);
                Selected = false;
            }
            else if (status == RegionStatus.NormalHover)
            {
                Frame.Stroke = new SolidColorBrush(Colors.Black);
                Frame.Fill = new SolidColorBrush(new Color { A = 100, R = 0, G = 0, B = 123 });
                Selected = false;
            }
            else if (status == RegionStatus.Selected)
            {
                Frame.Stroke = new SolidColorBrush(Colors.Red);
                Frame.Fill = new SolidColorBrush(Colors.Transparent);
                Selected = true;
            }
            else
            {
                Frame.Stroke = new SolidColorBrush(Colors.Red);
                Frame.Fill = new SolidColorBrush(new Color { A = 100, R = 0, G = 0, B = 123 });
                Selected = true;
            }
        }
        public async void SpecialFrame_StatusChanged(int regionIndex, RegionStatus status)
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
            SpecialFrame.Source = source;
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
