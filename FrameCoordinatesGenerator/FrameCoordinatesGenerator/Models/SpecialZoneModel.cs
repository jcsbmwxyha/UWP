using FrameCoordinatesGenerator.Common;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;

namespace FrameCoordinatesGenerator.Models
{
    public class SpecialZoneModel : ZoneModel, INotifyPropertyChanged
    {
        [ComImport]
        [Guid("5B0D3235-4DBA-4D44-865E-8F1D0E4FD04D")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        unsafe interface IMemoryBufferByteAccess
        {
            void GetBuffer(out byte* buffer, out uint capacity);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private SoftwareBitmap _sb;
        public async Task SetSoftwareBitmapAsync(SoftwareBitmap sb)
        {
            _sb = sb;
            //InitialFrameColor();

            _imagesource = new SoftwareBitmapSource();
            await _imagesource.SetBitmapAsync(sb);
            RaisePropertyChanged("Image");
        }

        private SoftwareBitmapSource _imagesource;
        public SoftwareBitmapSource ImageSource
        {
            get
            {
                return _imagesource;
            }
            set
            {
                _imagesource = value;
                RaisePropertyChanged("ImageSource");
            }
        }

        public SpecialZoneModel() : base()
        {
        }

        private unsafe void InitialFrameColor()
        {
            using (BitmapBuffer buffer = _sb.LockBuffer(BitmapBufferAccessMode.Write))
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
                            if (dataInBytes[pixelIndex + 1] == 255) // inner
                            {
                                dataInBytes[pixelIndex + 0] = (byte)0;
                                dataInBytes[pixelIndex + 1] = (byte)255;
                                dataInBytes[pixelIndex + 2] = (byte)0;
                                dataInBytes[pixelIndex + 3] = (byte)1;
                            }
                            else if (dataInBytes[pixelIndex + 3] != 0) // outer
                            {
                                dataInBytes[pixelIndex + 0] = (byte)255;
                                dataInBytes[pixelIndex + 1] = (byte)255;
                                dataInBytes[pixelIndex + 2] = (byte)255;
                            }
                        }
                    }
                }
            }
        }
        override public async void ChangeStatus(RegionStatus status)
        {
            Color outer;
            Color inner;

            if (status == RegionStatus.Normal)
            {
                outer = Colors.White;
                inner = new Color { A = 1, R = 0, G = 255, B = 0 };
                Selected = false;
            }
            else if (status == RegionStatus.NormalHover)
            {
                outer = new Color { A = 255, R = 255, G = 0, B = 41 };
                inner = new Color { A = 254, R = 0, G = 255, B = 0 };
                Selected = false;
            }
            else if (status == RegionStatus.Selected)
            {
                outer = new Color { A = 255, R = 255, G = 0, B = 41 };
                inner = new Color { A = 1, R = 0, G = 255, B = 0 };
                Selected = true;
            }
            else if (status == RegionStatus.Watching)
            {
                outer = new Color { A = 255, R = 4, G = 61, B = 246 };
                inner = new Color { A = 1, R = 0, G = 255, B = 0 };
                Selected = true;
            }
            else
            {
                outer = Colors.Red;
                inner = Colors.Red;
                Selected = true;
            }

            _sb = SoftwareBitmap.Convert(_sb, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight);
            ChangeSpecialFrameColor(outer, inner);
            _sb = SoftwareBitmap.Convert(_sb, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);

            var source = new SoftwareBitmapSource();
            await source.SetBitmapAsync(_sb);

            ImageSource = source;
        }
        private unsafe void ChangeSpecialFrameColor(Color outer, Color inner)
        {
            using (BitmapBuffer buffer = _sb.LockBuffer(BitmapBufferAccessMode.Write))
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
                            if (dataInBytes[pixelIndex + 3] != 0) // outer
                            {
                                dataInBytes[pixelIndex + 0] = (byte)outer.B;
                                dataInBytes[pixelIndex + 1] = (byte)outer.G;
                                dataInBytes[pixelIndex + 2] = (byte)outer.R;
                            }
                            //else if (dataInBytes[pixelIndex + 3] != 0) // inner
                            //{
                            //    dataInBytes[pixelIndex + 3] = (byte)inner.A;
                            //}
                        }
                    }
                }
            }
        }
    }
}
