﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using System.Runtime.InteropServices;
using Windows.UI;
using AuraEditor.Common;
using Windows.UI.Xaml.Media.Animation;

// 內容對話方塊項目範本已記錄在 https://go.microsoft.com/fwlink/?LinkId=234238

namespace AuraEditor.Dialogs
{
    public sealed partial class ColorPickerDialog : ContentDialog
    {
        [ComImport]
        [Guid("5B0D3235-4DBA-4D44-865E-8F1D0E4FD04D")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        unsafe interface IMemoryBufferByteAccess
        {
            void GetBuffer(out byte* buffer, out uint capacity);
        }
        private int WindowsSizeFlag = 0;

        // Color Ring
        SoftwareBitmap colorRingSoftwareBitmap;
        SoftwareBitmap selectColorAreaSoftwareBitmap;
        SoftwareBitmap squareSoftwareBitmap;
        Point colorRingCenter;
        public Point _preCirclePoint;
        double seletcedColorEllipse_r;
        double squareSideLength;
        int RecentCount;

        public delegate void ColorChangeCallBack(Color c);
        public ColorChangeCallBack OnColorChange;
        public double _preAngle;
        bool _squarePressing;

        private double _hue;
        public double Hue
        {
            get { return _hue; }
            set
            {
                if (_hue != value)
                {
                    _hue = value;
                }
            }
        }
        public double Saturation;
        public double Value;
        private Color _currentColor;
        public Color CurrentColor
        {
            get { return _currentColor; }
            set
            {
                if (value == _currentColor)
                    return;

                TextBox_R.Text = value.R.ToString();
                TextBox_G.Text = value.G.ToString();
                TextBox_B.Text = value.B.ToString();
                OnColorChange?.Invoke(value);
                _currentColor = value;
            }
        }
        public Color PreColor { get; set; }

        public class RecentColor
        {
            public int ColorA { get; set; }
            public int ColorR { get; set; }
            public int ColorG { get; set; }
            public int ColorB { get; set; }
            public RecentColor()
            {
                ColorA = 0;
                ColorR = -1;
                ColorG = -1;
                ColorB = -1;
            }
        }

        public static RecentColor[] g_RecentColor = new RecentColor[8];
        public int ComeFrom = 1;

        public ColorPickerDialog(Color c)
        {
            this.InitializeComponent();
            ComeFrom = 1;
            RecentCount = 0;
            _preAngle = 0;
            _preCirclePoint = new Point(0, 0);
            PreColor = c;
            _currentColor = PreColor;
            for (int i = 0; i < 8; i++)
                g_RecentColor[i] = new RecentColor();
            Window.Current.CoreWindow.SizeChanged += CurrentWindow_SizeChanged;
            Task curtask = Task.Run(async () => await CreateColorRingImage());
            curtask.Wait();
            Task curtask1 = Task.Run(async () => await SelectColorAreaImage());
            curtask1.Wait();
            Task curtask2 = Task.Run(async () => await CreateSquareImage());
            curtask2.Wait();
        }

        private Layer m_DeviceLayer;
        public ColorPickerDialog(Color c, Layer DeviceLayer)
        {
            this.InitializeComponent();
            ComeFrom = 2;
            m_DeviceLayer = DeviceLayer;
            RecentCount = 0;
            _preAngle = 0;
            _preCirclePoint = new Point(0, 0);
            PreColor = c;
            _currentColor = PreColor;
            for (int i = 0; i < 8; i++)
                g_RecentColor[i] = new RecentColor();
            Window.Current.CoreWindow.SizeChanged += CurrentWindow_SizeChanged;
            Task curtask = Task.Run(async () => await CreateColorRingImage());
            curtask.Wait();
            Task curtask1 = Task.Run(async () => await SelectColorAreaImage());
            curtask1.Wait();
            Task curtask2 = Task.Run(async () => await CreateSquareImage());
            curtask2.Wait();
        }

        private void ColorPickerDialog_Loaded(object sender, RoutedEventArgs e)
        {
            seletcedColorEllipse_r = SeletcedColorEllipse.Height / 2;
            colorRingCenter = new Point(RingGrid.ActualWidth / 2, RingGrid.ActualHeight / 2);
            squareSideLength = 160;

            // Initialize ++++
            TextBox_R.Text = _currentColor.R.ToString();
            TextBox_G.Text = _currentColor.G.ToString();
            TextBox_B.Text = _currentColor.B.ToString();
            //SelectedPolygonColor.Fill = new SolidColorBrush(_currentColor);

            HSVColor hsv = Math2.RGBToHSV(_currentColor);
            Hue = hsv.H;
            Saturation = hsv.S;
            Value = hsv.V;
            SelectRingImgRotation.Angle = Hue;
            _preAngle = Hue;

            // Compute SeletcedColorEllipse position
            Point targetPoint = Math2.SVToPoint(Saturation, Value, squareSideLength);
            //targetPoint.X -= seletcedColorEllipse_r;
            //targetPoint.Y -= seletcedColorEllipse_r;

            CompositeTransform ct = SeletcedColorEllipseCompositeTransform;
            ct.TranslateX = targetPoint.X;
            ct.TranslateY = targetPoint.Y;
            _preCirclePoint = new Point(ct.TranslateX, ct.TranslateY);
            // Initialize ----


            ChangeColorRingColor();
            ChangeSelectAreaColor(Hue, Saturation, Value);
            ChangeSquareColor(Hue);
        }

        private async void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            Window.Current.CoreWindow.SizeChanged -= CurrentWindow_SizeChanged;
            if (ComeFrom == 1)
            {
                //From Main Page
                this.Hide();
            }
            else if(ComeFrom == 2)
            {
                //From Trigger Dialog
                this.Hide();
                ContentDialog triggerDialog = new TriggerDialog(m_DeviceLayer);
                await triggerDialog.ShowAsync();
            }
            else
            {
                this.Hide();
            }
        }

        private void RBtnDefault_Click(object sender, RoutedEventArgs e)
        {
            RadioButton Rbtn = sender as RadioButton;
            EnterColor(((SolidColorBrush)Rbtn.Background).Color);
        }

        private void RBtnRecent_Click(object sender, RoutedEventArgs e)
        {
            RadioButton Rbtn = sender as RadioButton;
            EnterColor(((SolidColorBrush)Rbtn.Background).Color);
        }

        private void CurrentWindow_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            if (e.Size.Width > 664 && WindowsSizeFlag != 1)
            {
                ColorPickerGrid.Width = 664;
                ColorPickerGrid.Height = 716;
                BackgroundImage.Source = new BitmapImage(new Uri(this.BaseUri, "../Assets/EffectInfoGroup/asus_gc_aura_customize_colorpick_selected_colorpicker_bg.png"));
                Col_1.Width = new GridLength(364, GridUnitType.Star);
                Col_2.Width = new GridLength(300, GridUnitType.Star);
                RingGrid.Margin = new Thickness(60, 0, 0, 0);
                ColorSetGrid.Margin = new Thickness(68, 48, 0, 0);
                SelectGrid.Margin = new Thickness(56, 0, 72, 0);
                Grid_Selected.Width = 172;
                Grid_Selected.Height = 32;
                TextBox_R.Width = 123;
                TextBox_G.Width = 123;
                TextBox_B.Width = 123;
                Grid_R.Margin = new Thickness(25, 26, 0, 0);
                Grid_G.Margin = new Thickness(25, 16, 0, 0);
                Grid_B.Margin = new Thickness(25, 16, 0, 0);
                RBtnDefault1.Margin = new Thickness(0, 0, 12, 0);
                RBtnDefault2.Margin = new Thickness(0, 0, 12, 0);
                RBtnDefault3.Margin = new Thickness(0, 0, 12, 0);
                RBtnDefault4.Margin = new Thickness(0, 0, 12, 0);
                RBtnDefault5.Margin = new Thickness(0, 0, 12, 0);
                RBtnDefault6.Margin = new Thickness(0, 0, 12, 0);
                RBtnDefault7.Margin = new Thickness(0, 0, 12, 0);
                RBtnDefault8.Margin = new Thickness(0, 0, 12, 0);
                RBtnDefault9.Margin = new Thickness(0, 0, 12, 0);
                AddRecentBtn.Margin = new Thickness(0, 0, 12, 0);
                RBtnRecent_1.Margin = new Thickness(0, 0, 12, 0);
                RBtnRecent_2.Margin = new Thickness(0, 0, 12, 0);
                RBtnRecent_3.Margin = new Thickness(0, 0, 12, 0);
                RBtnRecent_4.Margin = new Thickness(0, 0, 12, 0);
                RBtnRecent_5.Margin = new Thickness(0, 0, 12, 0);
                RBtnRecent_6.Margin = new Thickness(0, 0, 12, 0);
                RBtnRecent_7.Margin = new Thickness(0, 0, 12, 0);
                RBtnRecent_8.Margin = new Thickness(0, 0, 12, 0);
                WindowsSizeFlag = 1;
            }
            else if (e.Size.Width < 664 && WindowsSizeFlag != -1)
            {
                ColorPickerGrid.Width = 500;
                ColorPickerGrid.Height = 716;
                BackgroundImage.Source = new BitmapImage(new Uri(this.BaseUri, "../Assets/EffectInfoGroup/asus_gc_aura_customize_colorpick_selected_colorpicker_small_bg.png"));
                Col_1.Width = new GridLength(328, GridUnitType.Star);
                Col_2.Width = new GridLength(172, GridUnitType.Star);
                RingGrid.Margin = new Thickness(25, 0, 0, 0);
                ColorSetGrid.Margin = new Thickness(18, 48, 0, 0);
                SelectGrid.Margin = new Thickness(12, 0, 24, 0);
                Grid_Selected.Width = 136;
                Grid_Selected.Height = 32;
                TextBox_R.Width = 108;
                TextBox_G.Width = 108;
                TextBox_B.Width = 108;
                Grid_R.Margin = new Thickness(9, 26, 0, 0);
                Grid_G.Margin = new Thickness(9, 16, 0, 0);
                Grid_B.Margin = new Thickness(9, 16, 0, 0);
                RBtnDefault1.Margin = new Thickness(0, 0, 4, 0);
                RBtnDefault2.Margin = new Thickness(0, 0, 4, 0);
                RBtnDefault3.Margin = new Thickness(0, 0, 4, 0);
                RBtnDefault4.Margin = new Thickness(0, 0, 4, 0);
                RBtnDefault5.Margin = new Thickness(0, 0, 4, 0);
                RBtnDefault6.Margin = new Thickness(0, 0, 4, 0);
                RBtnDefault7.Margin = new Thickness(0, 0, 4, 0);
                RBtnDefault8.Margin = new Thickness(0, 0, 4, 0);
                RBtnDefault9.Margin = new Thickness(0, 0, 4, 0);
                AddRecentBtn.Margin = new Thickness(0, 0, 4, 0);
                RBtnRecent_1.Margin = new Thickness(0, 0, 4, 0);
                RBtnRecent_2.Margin = new Thickness(0, 0, 4, 0);
                RBtnRecent_3.Margin = new Thickness(0, 0, 4, 0);
                RBtnRecent_4.Margin = new Thickness(0, 0, 4, 0);
                RBtnRecent_5.Margin = new Thickness(0, 0, 4, 0);
                RBtnRecent_6.Margin = new Thickness(0, 0, 4, 0);
                RBtnRecent_7.Margin = new Thickness(0, 0, 4, 0);
                RBtnRecent_8.Margin = new Thickness(0, 0, 4, 0);
                WindowsSizeFlag = -1;
            }
        }

        private async Task CreateColorRingImage()
        {
            string colorRingImage = @"Assets\ColorPicker\asus_gc_aura_customize_colorpick_selected_colorring_mask.png";

            SoftwareBitmap softwareBitmap;
            StorageFolder InstallationFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            StorageFile colorRingImgFile = await InstallationFolder.GetFileAsync(colorRingImage);

            using (IRandomAccessStream printingFileStream = await colorRingImgFile.OpenAsync(FileAccessMode.Read))
            {
                BitmapDecoder printingDecoder = await BitmapDecoder.CreateAsync(printingFileStream);
                softwareBitmap = await printingDecoder.GetSoftwareBitmapAsync();
            }
            colorRingSoftwareBitmap = softwareBitmap;
        }

        private async Task SelectColorAreaImage()
        {
            string selectColorAreaImage = @"Assets\ColorPicker\asus_gc_aura_customize_colorpick_selected_colormask_btn.png";

            SoftwareBitmap softwareBitmap;
            StorageFolder InstallationFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            StorageFile selectColorAreaImgFile = await InstallationFolder.GetFileAsync(selectColorAreaImage);

            using (IRandomAccessStream printingFileStream = await selectColorAreaImgFile.OpenAsync(FileAccessMode.Read))
            {
                BitmapDecoder printingDecoder = await BitmapDecoder.CreateAsync(printingFileStream);
                softwareBitmap = await printingDecoder.GetSoftwareBitmapAsync();
            }
            selectColorAreaSoftwareBitmap = softwareBitmap;
        }

        private async Task CreateSquareImage()
        {
            string aquareImage = @"Assets\ColorPicker\asus_gc_aura_customize_colorpick_selected_ring_colormask_btn.png";

            SoftwareBitmap softwareBitmap;
            StorageFolder InstallationFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            StorageFile squareImgFile = await InstallationFolder.GetFileAsync(aquareImage);

            using (IRandomAccessStream printingFileStream = await squareImgFile.OpenAsync(FileAccessMode.Read))
            {
                BitmapDecoder printingDecoder = await BitmapDecoder.CreateAsync(printingFileStream);
                softwareBitmap = await printingDecoder.GetSoftwareBitmapAsync();
            }
            squareSoftwareBitmap = softwareBitmap;
        }

        //public async Task<SoftwareBitmap> CreateImage(string imagepath)
        //{
        //    SoftwareBitmap softwareBitmap;
        //    StorageFolder InstallationFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
        //    StorageFile ImgFile = await InstallationFolder.GetFileAsync(imagepath);

        //    using (IRandomAccessStream printingFileStream = await ImgFile.OpenAsync(FileAccessMode.Read))
        //    {
        //        BitmapDecoder printingDecoder = await BitmapDecoder.CreateAsync(printingFileStream);
        //        softwareBitmap = await printingDecoder.GetSoftwareBitmapAsync();
        //    }
        //    return softwareBitmap;
        //}

        private async void ChangeColorRingColor()
        {
            colorRingSoftwareBitmap = SoftwareBitmap.Convert(colorRingSoftwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight);
            ChangeColorRingColorPixel();
            colorRingSoftwareBitmap = SoftwareBitmap.Convert(colorRingSoftwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);

            var source = new SoftwareBitmapSource();
            await source.SetBitmapAsync(colorRingSoftwareBitmap);
            ColorRingImg.Source = source;
        }

        private async void ChangeSelectAreaColor(double hue, double saturation, double value)
        {
            selectColorAreaSoftwareBitmap = SoftwareBitmap.Convert(selectColorAreaSoftwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight);
            ChangeSelectAteaColorPixel(hue, saturation, value);
            selectColorAreaSoftwareBitmap = SoftwareBitmap.Convert(selectColorAreaSoftwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);

            var source = new SoftwareBitmapSource();
            await source.SetBitmapAsync(selectColorAreaSoftwareBitmap);
            SelectedColorShowArea.Source = source;
        }

        private async void ChangeSquareColor(double hue)
        {
            squareSoftwareBitmap = SoftwareBitmap.Convert(squareSoftwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight);
            ChangeSquarePixel(hue);
            squareSoftwareBitmap = SoftwareBitmap.Convert(squareSoftwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);

            var source = new SoftwareBitmapSource();
            await source.SetBitmapAsync(squareSoftwareBitmap);
            SquareImg.Source = source;
        }

        private unsafe void ChangeColorRingColorPixel()
        {
            using (BitmapBuffer buffer = colorRingSoftwareBitmap.LockBuffer(BitmapBufferAccessMode.Write))
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
                            double dx = col - imgCenterW;
                            double dy = row - imgCenterH;
                            double hue = Math2.ComputeH(dx, dy);
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

        private unsafe void ChangeSelectAteaColorPixel(double hue, double saturation, double value)
        {
            using (BitmapBuffer buffer = selectColorAreaSoftwareBitmap.LockBuffer(BitmapBufferAccessMode.Write))
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
                    HSVColor hsv = new HSVColor(hue, saturation, value);
                    Color color = hsv.GetRGB();

                    for (int row = 0; row < imgHeight; row++)
                    {
                        for (int col = 0; col < imgWidth; col++)
                        {
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

        private unsafe void ChangeSquarePixel(double hue)
        {
            HSVColor hsv = new HSVColor(hue, 1.0, 1.0);

            using (BitmapBuffer buffer = squareSoftwareBitmap.LockBuffer(BitmapBufferAccessMode.Write))
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
                    //HSVColor hsv = new HSVColor(hue, 1.0, 1.0);
                    //Color color = hsv.GetRGB();

                    for (int row = 0; row < imgHeight; row++)
                    {
                        for (int col = 0; col < imgWidth; col++)
                        {
                            Point pt = new Point(col, bufferLayout.Height - row);

                            //Math2.ComputeSquareSV(pt, 220, out hsv.S, out hsv.V);
                            Math2.ComputeSquareSV(row, col, 160, out hsv.S, out hsv.V);
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

        private void Circle_PointerMoved(object sender, PointerRoutedEventArgs e)
        {

        }

        private void Circle_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            Point currentLocation = e.GetCurrentPoint(RingGrid).Position;
            double dx = currentLocation.X - colorRingCenter.X;
            double dy = currentLocation.Y - colorRingCenter.Y;
            //---
            double hue = Math2.ComputeH(dx, dy);
            //---
            Hue = Math2.ComputeH(dx, dy);
            CurrentColor = Math2.HSVToRGB(Hue, Saturation, Value);

            CompositeTransform ct = SeletcedColorEllipseCompositeTransform;
            Point targetPoint = new Point(ct.TranslateX, ct.TranslateY);
            ColorPickerStoryboardStart(Hue, targetPoint);
        }

        private void Circle_PointerReleased(object sender, PointerRoutedEventArgs e)
        {

        }

        private void ColorPickerStoryboardStart(double selectRingTargetAngle, Point circleTargetPoint)
        {
            //_animating = true;
            int runTime = 300;
            var storyboard = new Storyboard();
            var triangleAnimation = new DoubleAnimation();
            var selesctedRingAnimation = new DoubleAnimation();
            var colorCirclePositionX = new DoubleAnimation();
            var colorCirclePositionY = new DoubleAnimation();

            if (_preAngle != selectRingTargetAngle)
            {
                selectRingTargetAngle = selectRingTargetAngle % 360;
                if (_preAngle - selectRingTargetAngle > 180)
                {
                    _preAngle -= 360;
                }
                else if (_preAngle - selectRingTargetAngle < -180)
                {
                    _preAngle += 360;
                }

                // selected ring
                selesctedRingAnimation.Duration = TimeSpan.FromMilliseconds(runTime);
                selesctedRingAnimation.EnableDependentAnimation = true;
                selesctedRingAnimation.From = _preAngle;
                selesctedRingAnimation.To = selectRingTargetAngle;
                Storyboard.SetTargetProperty(selesctedRingAnimation, "Angle");
                Storyboard.SetTarget(selesctedRingAnimation, SelectRingImgRotation);
                storyboard.Children.Add(selesctedRingAnimation);

                _preAngle = selectRingTargetAngle;
                ChangeSquareColor(selectRingTargetAngle);
            }
            ChangeSelectAreaColor(selectRingTargetAngle, Saturation, Value);

            if (_preCirclePoint.X != circleTargetPoint.X || _preCirclePoint.Y != circleTargetPoint.Y)
            {
                // circle x
                colorCirclePositionX.Duration = TimeSpan.FromMilliseconds(runTime);
                colorCirclePositionX.EnableDependentAnimation = true;
                colorCirclePositionX.From = _preCirclePoint.X;
                colorCirclePositionX.To = circleTargetPoint.X;
                Storyboard.SetTargetProperty(colorCirclePositionX, "TranslateX");
                Storyboard.SetTarget(colorCirclePositionX, SeletcedColorEllipseCompositeTransform);
                storyboard.Children.Add(colorCirclePositionX);

                // circle y
                colorCirclePositionY.Duration = TimeSpan.FromMilliseconds(runTime);
                colorCirclePositionY.EnableDependentAnimation = true;
                colorCirclePositionY.From = _preCirclePoint.Y;
                colorCirclePositionY.To = circleTargetPoint.Y;
                Storyboard.SetTargetProperty(colorCirclePositionY, "TranslateY");
                Storyboard.SetTarget(colorCirclePositionY, SeletcedColorEllipseCompositeTransform);
                storyboard.Children.Add(colorCirclePositionY);

                _preCirclePoint = new Point(circleTargetPoint.X, circleTargetPoint.Y);
            }

            storyboard.Begin();
        }
        
        private void HiddenSquare_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            Point currentLocation = e.GetCurrentPoint(RingGrid).Position;
            _squarePressing = true;

            CompositeTransform ct = SeletcedColorEllipseCompositeTransform;
            ct.TranslateX = currentLocation.X - seletcedColorEllipse_r;
            ct.TranslateY = currentLocation.Y - seletcedColorEllipse_r;

            ColorPickerStoryboardStart(Hue, new Point(ct.TranslateX, ct.TranslateY));

            Saturation = (currentLocation.X - 72) / squareSideLength;
            Value = (squareSideLength - (currentLocation.Y - 72)) / squareSideLength;
            CurrentColor = Math2.HSVToRGB(Hue, Saturation, Value);
            ChangeSelectAreaColor(Hue, Saturation, Value);
        }

        private void HiddenSquare_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (_squarePressing)
            {
                Point currentLocation = e.GetCurrentPoint(RingGrid).Position;

                CompositeTransform ct = SeletcedColorEllipseCompositeTransform;
                ct.TranslateX = currentLocation.X - seletcedColorEllipse_r;
                ct.TranslateY = currentLocation.Y - seletcedColorEllipse_r;
                _preCirclePoint = new Point(ct.TranslateX, ct.TranslateY);

                Saturation = (currentLocation.X - 72) / squareSideLength;
                Value = (squareSideLength - (currentLocation.Y - 72)) / squareSideLength;
                CurrentColor = Math2.HSVToRGB(Hue, Saturation, Value);
                ChangeSelectAreaColor(Hue, Saturation, Value);
            }
        }

        private void HiddenSquare_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            _squarePressing = false;
        }

        private void TextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            TextBox textBoxContent = sender as TextBox;
            //Press ESC to exit window
            //if (e.Key == Windows.System.VirtualKey.Escape)
            //{
            //    ColorPickerResult = false;
            //    this.Hide();
            //}

            if (!e.Key.ToString().Contains("Number"))
            {
                e.Handled = true;
            }

            //Press Enter to change graphic
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                if ((TextBox_R.Text != "") && (TextBox_G.Text != "") && (TextBox_B.Text != ""))
                    TextToColor(TextBox_R.Text, TextBox_G.Text, TextBox_B.Text);
            }
        }
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox ColorChangeText = sender as TextBox;

            char[] originalText = ColorChangeText.Text.ToCharArray();
            if (ColorChangeText.Text.Length > 0)
            {
                foreach (char c in originalText)
                {
                    if (!(Char.IsNumber(c)))
                    {
                        ColorChangeText.Text = ColorChangeText.Text.Replace(Convert.ToString(c), "");
                        break;
                    }
                }
            }

            if ((ColorChangeText.Text != ""))
            {
                if (int.Parse(ColorChangeText.Text) > 255)
                {
                    ColorChangeText.Text = "255";
                }
                else if (int.Parse(ColorChangeText.Text) < 0)
                {
                    ColorChangeText.Text = "0";
                }
            }
        }
        private void ColorTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            
        }
        private void ColorTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if ((TextBox_R.Text != "") && (TextBox_G.Text != "") && (TextBox_B.Text != ""))
                TextToColor(TextBox_R.Text, TextBox_G.Text, TextBox_B.Text);
        }
        public void TextToColor(string r, string g, string b)
        {
            Color textColor = new Color
            {
                A = 255,
                R = (byte)Int32.Parse(r),
                G = (byte)Int32.Parse(g),
                B = (byte)Int32.Parse(b)
            };

            // We don't need to rotate if the same color. Even the new Hue is different from old Hue.
            if (CurrentColor.Equals(textColor))
                return;

            EnterColor(textColor);
        }
        public void EnterColor(Color c)
        {
            if (c.A == 0) // The recent color has no color
            { c.R = c.G = c.B = 0; } // Black

            CurrentColor = c;
            HSVColor hsv = Math2.RGBToHSV(c);
            Hue = hsv.H;
            Saturation = hsv.S;
            Value = hsv.V;

            // Compute SeletcedColorEllipse position
            Point targetPoint = Math2.SVToPoint(Saturation, Value, squareSideLength);
            CompositeTransform ct = SeletcedColorEllipseCompositeTransform;
            ct.TranslateX = targetPoint.X;
            ct.TranslateY = targetPoint.Y;

            ColorPickerStoryboardStart(Hue, targetPoint);
        }

        private void AddRecentBtn_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 8; i++)
            {
                if (g_RecentColor[i].ColorR == -1)
                {
                    RecentCount = i;
                    break;
                }
                else if (g_RecentColor[i].ColorR == Math2.HSVToRGB(Hue, Saturation, Value).R
                    && g_RecentColor[i].ColorG == Math2.HSVToRGB(Hue, Saturation, Value).G
                    && g_RecentColor[i].ColorB == Math2.HSVToRGB(Hue, Saturation, Value).B)
                {
                    RecentCount = i;
                    break;
                }
                else
                {
                    RecentCount = 7;
                }
            }

            for (int i = RecentCount; i > -1; i--)
            {
                if (i == 0)
                {
                    g_RecentColor[i].ColorA = 255;
                    g_RecentColor[i].ColorR = Math2.HSVToRGB(Hue, Saturation, Value).R;
                    g_RecentColor[i].ColorG = Math2.HSVToRGB(Hue, Saturation, Value).G;
                    g_RecentColor[i].ColorB = Math2.HSVToRGB(Hue, Saturation, Value).B;
                }
                else
                {
                    g_RecentColor[i].ColorA = g_RecentColor[i - 1].ColorA;
                    g_RecentColor[i].ColorR = g_RecentColor[i - 1].ColorR;
                    g_RecentColor[i].ColorG = g_RecentColor[i - 1].ColorG;
                    g_RecentColor[i].ColorB = g_RecentColor[i - 1].ColorB;
                }
            }

            if (RecentCount < 8)
            {
                switch (RecentCount)
                {
                    case 0:
                        RBtnRecent_1.Visibility = Visibility.Visible;
                        RBtnRecent_1.IsEnabled = true;
                        break;
                    case 1:
                        RBtnRecent_2.Visibility = Visibility.Visible;
                        RBtnRecent_2.IsEnabled = true;
                        break;
                    case 2:
                        RBtnRecent_3.Visibility = Visibility.Visible;
                        RBtnRecent_3.IsEnabled = true;
                        break;
                    case 3:
                        RBtnRecent_4.Visibility = Visibility.Visible;
                        RBtnRecent_4.IsEnabled = true;
                        break;
                    case 4:
                        RBtnRecent_5.Visibility = Visibility.Visible;
                        RBtnRecent_5.IsEnabled = true;
                        break;
                    case 5:
                        RBtnRecent_6.Visibility = Visibility.Visible;
                        RBtnRecent_6.IsEnabled = true;
                        break;
                    case 6:
                        RBtnRecent_7.Visibility = Visibility.Visible;
                        RBtnRecent_7.IsEnabled = true;
                        break;
                    case 7:
                        RBtnRecent_8.Visibility = Visibility.Visible;
                        RBtnRecent_8.IsEnabled = true;
                        break;
                    default:
                        break;

                }
            }
            RBtnRecent_1.Background = new SolidColorBrush(Color.FromArgb((byte)g_RecentColor[0].ColorA,
                                                                         (byte)g_RecentColor[0].ColorR,
                                                                         (byte)g_RecentColor[0].ColorG,
                                                                         (byte)g_RecentColor[0].ColorB));
            RBtnRecent_2.Background = new SolidColorBrush(Color.FromArgb((byte)g_RecentColor[1].ColorA,
                                                                         (byte)g_RecentColor[1].ColorR,
                                                                         (byte)g_RecentColor[1].ColorG,
                                                                         (byte)g_RecentColor[1].ColorB));
            RBtnRecent_3.Background = new SolidColorBrush(Color.FromArgb((byte)g_RecentColor[2].ColorA,
                                                                         (byte)g_RecentColor[2].ColorR,
                                                                         (byte)g_RecentColor[2].ColorG,
                                                                         (byte)g_RecentColor[2].ColorB));
            RBtnRecent_4.Background = new SolidColorBrush(Color.FromArgb((byte)g_RecentColor[3].ColorA,
                                                                         (byte)g_RecentColor[3].ColorR,
                                                                         (byte)g_RecentColor[3].ColorG,
                                                                         (byte)g_RecentColor[3].ColorB));
            RBtnRecent_5.Background = new SolidColorBrush(Color.FromArgb((byte)g_RecentColor[4].ColorA,
                                                                         (byte)g_RecentColor[4].ColorR,
                                                                         (byte)g_RecentColor[4].ColorG,
                                                                         (byte)g_RecentColor[4].ColorB));
            RBtnRecent_6.Background = new SolidColorBrush(Color.FromArgb((byte)g_RecentColor[5].ColorA,
                                                                         (byte)g_RecentColor[5].ColorR,
                                                                         (byte)g_RecentColor[5].ColorG,
                                                                         (byte)g_RecentColor[5].ColorB));
            RBtnRecent_7.Background = new SolidColorBrush(Color.FromArgb((byte)g_RecentColor[6].ColorA,
                                                                         (byte)g_RecentColor[6].ColorR,
                                                                         (byte)g_RecentColor[6].ColorG,
                                                                         (byte)g_RecentColor[6].ColorB));
            RBtnRecent_8.Background = new SolidColorBrush(Color.FromArgb((byte)g_RecentColor[7].ColorA,
                                                                         (byte)g_RecentColor[7].ColorR,
                                                                         (byte)g_RecentColor[7].ColorG,
                                                                         (byte)g_RecentColor[7].ColorB));
        }
    }
}
