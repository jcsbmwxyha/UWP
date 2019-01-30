﻿using AuraEditor.Common;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media.Imaging;

// 使用者控制項項目範本記載於 https://go.microsoft.com/fwlink/?LinkId=234236

namespace AuraEditor.UserControls
{
    public sealed partial class RangeSlider : UserControl
    {
        [ComImport]
        [Guid("5B0D3235-4DBA-4D44-865E-8F1D0E4FD04D")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        unsafe interface IMemoryBufferByteAccess
        {
            void GetBuffer(out byte* buffer, out uint capacity);
        }

        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        public double RangeMin
        {
            get { return (double)GetValue(RangeMinProperty); }
            set
            {
                SetValue(RangeMinProperty, value);
            }
        }

        public double RangeMax
        {
            get { return (double)GetValue(RangeMaxProperty); }
            set
            {
                SetValue(RangeMaxProperty, value);
            }
        }

        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register("Minimum", typeof(double), typeof(RangeSlider), new PropertyMetadata(0.0));

        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register("Maximum", typeof(double), typeof(RangeSlider), new PropertyMetadata(1.0));

        public static readonly DependencyProperty RangeMinProperty = DependencyProperty.Register("RangeMin", typeof(double), typeof(RangeSlider), new PropertyMetadata(0.0, OnRangeMinPropertyChanged));

        public static readonly DependencyProperty RangeMaxProperty = DependencyProperty.Register("RangeMax", typeof(double), typeof(RangeSlider), new PropertyMetadata(1.0, OnRangeMaxPropertyChanged));

        SoftwareBitmap randomBgSoftwareBitmap;
        public RangeSlider()
        {
            this.InitializeComponent();

            Task curtask = Task.Run(async () => await CreateRandomBgImage());
            curtask.Wait();
        }

        private void RangeSlider_Loaded(object sender, RoutedEventArgs e)
        {
            ChangeRandomBgColor();
        }
        private static void OnRangeMinPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var slider = (RangeSlider)d;
            var newValue = (double)e.NewValue;

            if (newValue < slider.Minimum)
            {
                slider.RangeMin = slider.Minimum;
            }
            else if (newValue > slider.Maximum)
            {
                slider.RangeMin = slider.Maximum;
            }
            else
            {
                slider.RangeMin = newValue;
            }

            if (slider.RangeMin >= slider.RangeMax)
            {
                slider.RangeMin = slider.RangeMax - 1;
            }

            slider.UpdateMinThumb(slider.RangeMin);
        }

        private static void OnRangeMaxPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var slider = (RangeSlider)d;
            var newValue = (double)e.NewValue;

            if (newValue < slider.Minimum)
            {
                slider.RangeMax = slider.Minimum;
            }
            else if (newValue > slider.Maximum)
            {
                slider.RangeMax = slider.Maximum;
            }
            else
            {
                slider.RangeMax = newValue;
            }

            if (slider.RangeMax <= slider.RangeMin)
            {
                slider.RangeMax = slider.RangeMin + 1;
            }

            slider.UpdateMaxThumb(slider.RangeMax);
        }

        public void UpdateMinThumb(double min, bool update = false)
        {
            if (ContainerCanvas != null)
            {
                if (update || !MinThumb.IsDragging)
                {
                    var relativeLeft = ((min - Minimum) / (Maximum - Minimum)) * ContainerCanvas.ActualWidth;

                    Canvas.SetLeft(MinThumb, relativeLeft);

                    H_Rectangle.Width = min / (Maximum - Minimum) * ContainerCanvas.ActualWidth;
                }
            }
        }

        public void UpdateMaxThumb(double max, bool update = false)
        {
            if (ContainerCanvas != null)
            {
                if (update || !MaxThumb.IsDragging)
                {
                    var relativeRight = (max - Minimum) / (Maximum - Minimum) * ContainerCanvas.ActualWidth;

                    Canvas.SetLeft(MaxThumb, relativeRight);
                    Canvas.SetLeft(R_Rectangle, relativeRight);
                    
                    R_Rectangle.Width = (Maximum - max) / (Maximum - Minimum) * ContainerCanvas.ActualWidth;
                }
            }
        }

        private void ContainerCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var relativeLeft = ((RangeMin - Minimum) / (Maximum - Minimum)) * ContainerCanvas.ActualWidth;
            var relativeRight = (RangeMax - Minimum) / (Maximum - Minimum) * ContainerCanvas.ActualWidth;

            Canvas.SetLeft(MinThumb, relativeLeft);
            Canvas.SetLeft(MaxThumb, relativeRight);
        }

        private void MinThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var min = DragThumb(MinThumb, 0, Canvas.GetLeft(MaxThumb), e.HorizontalChange);
            UpdateMinThumb(min, true);
            RangeMin = Math.Round(min);
        }

        private void MaxThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var max = DragThumb(MaxThumb, Canvas.GetLeft(MinThumb), ContainerCanvas.ActualWidth, e.HorizontalChange);
            UpdateMaxThumb(max, true);
            RangeMax = Math.Round(max);
        }

        private double DragThumb(Thumb thumb, double min, double max, double offset)
        {
            var currentPos = Canvas.GetLeft(thumb);
            var nextPos = currentPos + offset;

            nextPos = Math.Max(min, nextPos);
            nextPos = Math.Min(max, nextPos);

            return (Minimum + (nextPos / ContainerCanvas.ActualWidth) * (Maximum - Minimum));
        }

        private void MinThumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            UpdateMinThumb(RangeMin);
            Canvas.SetZIndex(MinThumb, 10);
            Canvas.SetZIndex(MaxThumb, 0);
        }

        private void MaxThumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            UpdateMaxThumb(RangeMax);
            Canvas.SetZIndex(MinThumb, 0);
            Canvas.SetZIndex(MaxThumb, 10);
        }

        private async Task CreateRandomBgImage()
        {
            string randomBgImage = @"Assets\RandomSlider\rainbow_bg.png";

            SoftwareBitmap softwareBitmap;
            StorageFolder InstallationFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            StorageFile randomBgImgFile = await InstallationFolder.GetFileAsync(randomBgImage);

            using (IRandomAccessStream printingFileStream = await randomBgImgFile.OpenAsync(FileAccessMode.Read))
            {
                BitmapDecoder printingDecoder = await BitmapDecoder.CreateAsync(printingFileStream);
                softwareBitmap = await printingDecoder.GetSoftwareBitmapAsync();
            }
            randomBgSoftwareBitmap = softwareBitmap;
        }

        private async void ChangeRandomBgColor()
        {
            randomBgSoftwareBitmap = SoftwareBitmap.Convert(randomBgSoftwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight);
            ChangeRandomBgColorPixel();
            randomBgSoftwareBitmap = SoftwareBitmap.Convert(randomBgSoftwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);

            var source = new SoftwareBitmapSource();
            await source.SetBitmapAsync(randomBgSoftwareBitmap);
            RandomBgImg.Source = source;
        }

        private unsafe void ChangeRandomBgColorPixel()
        {
            using (BitmapBuffer buffer = randomBgSoftwareBitmap.LockBuffer(BitmapBufferAccessMode.Write))
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

                    for (int row = 0; row < imgHeight; row++)
                    {
                        for (int col = 0; col < imgWidth; col++)
                        {
                            double hue = col * (360 / imgWidth);
                            HSVColor hsv = new HSVColor(hue, 1.0, 1.0);
                            Color color = hsv.GetRGB();

                            int pixelIndex = bufferLayout.Stride * row + 4 * col;
                            if (dataInBytes[pixelIndex + 3] != 0)
                            {
                                dataInBytes[pixelIndex + 0] = (byte)color.B;
                                dataInBytes[pixelIndex + 1] = (byte)color.G;
                                dataInBytes[pixelIndex + 2] = (byte)color.R;
                            }
                        }
                    }
                }
            }
        }
    }
}