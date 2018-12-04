using AuraEditor.Common;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Windows.Graphics.Imaging;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;

namespace AuraEditor.Models
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
                RaisePropertyChanged("Image");
            }
        }

        public SpecialZoneModel() : base()
        {
        }

        override public async void ChangeStatus(RegionStatus status)
        {
            Color color;
            if (status == RegionStatus.Normal)
            {
                color = Colors.White;
                Selected = false;
            }
            else if (status == RegionStatus.NormalHover)
            {
                color = new Color { A = 255, R = 255, G = 0, B = 41 };
                Selected = false;
            }
            else if (status == RegionStatus.Selected)
            {
                color = new Color { A = 255, R = 255, G = 0, B = 41 };
                Selected = true;
            }
            else if (status == RegionStatus.Watching)
            {
                color = new Color { A = 255, R = 4, G = 61, B = 246 };
                Selected = true;
            }
            else
            {
                color = Colors.Red;
                Selected = true;
            }

            _sb = SoftwareBitmap.Convert(_sb, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight);
            ChangeSpecialFrameColor(color);
            _sb = SoftwareBitmap.Convert(_sb, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);

            var source = new SoftwareBitmapSource();
            await source.SetBitmapAsync(_sb);

            ImageSource = source;
        }
        private unsafe void ChangeSpecialFrameColor(Color c)
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
