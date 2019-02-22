using AuraEditor.Common;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
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
        public async Task SetSoftwareBitmapAsync(SoftwareBitmap sb)
        {
            _sb = sb;
            _sb = SoftwareBitmap.Convert(_sb, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight);
            InitialFrameColor();
            _sb = SoftwareBitmap.Convert(_sb, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);

            _imagesource = new SoftwareBitmapSource();
            await _imagesource.SetBitmapAsync(_sb);
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
                                dataInBytes[pixelIndex + 1] = (byte)41;
                                dataInBytes[pixelIndex + 2] = (byte)255;
                                dataInBytes[pixelIndex + 3] = (byte)2;
                            }
                            else
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

            if (_myStatus == status)
                return;

            _myStatus = status;

            switch (_myStatus)
            {
                case RegionStatus.Normal:
                    outer = Colors.White;
                    inner = new Color { A = 2, R = 255, G = 255, B = 255 };
                    Selected = false;
                    break;
                case RegionStatus.NormalHover:
                    outer = Colors.White;
                    inner = new Color { A = 99, R = 255, G = 0, B = 41 };
                    Selected = false;
                    break;
                case RegionStatus.Selected:
                    outer = new Color { A = 255, R = 255, G = 0, B = 41 };
                    inner = new Color { A = 2, R = 255, G = 255, B = 255 };
                    Selected = true;
                    break;
                case RegionStatus.SelectedHover:
                    outer = new Color { A = 255, R = 255, G = 0, B = 41 };
                    inner = new Color { A = 99, R = 255, G = 0, B = 41 };
                    Selected = true;
                    break;
                case RegionStatus.Watching:
                    outer = new Color { A = 255, R = 4, G = 61, B = 246 };
                    inner = new Color { A = 2, R = 255, G = 255, B = 255 };
                    Selected = true;
                    break;
                default:
                    outer = Colors.Red;
                    inner = Colors.Red;
                    Selected = true;
                    break;
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
                            if (dataInBytes[pixelIndex + 3] != 0)
                            {
                                if (dataInBytes[pixelIndex + 3] == 2 || dataInBytes[pixelIndex + 3] == 99) // inner
                                {
                                    dataInBytes[pixelIndex + 0] = (byte)inner.B;
                                    dataInBytes[pixelIndex + 1] = (byte)inner.G;
                                    dataInBytes[pixelIndex + 2] = (byte)inner.R;
                                    dataInBytes[pixelIndex + 3] = (byte)inner.A;
                                }
                                else // outer
                                {
                                    dataInBytes[pixelIndex + 0] = (byte)outer.B;
                                    dataInBytes[pixelIndex + 1] = (byte)outer.G;
                                    dataInBytes[pixelIndex + 2] = (byte)outer.R;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
