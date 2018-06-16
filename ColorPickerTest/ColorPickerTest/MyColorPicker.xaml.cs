using System;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Media.Animation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Graphics.Imaging;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

// 空白頁項目範本已記錄在 https://go.microsoft.com/fwlink/?LinkId=234238

namespace ColorPickerTest
{
    /// <summary>
    /// 可以在本身使用或巡覽至框架內的空白頁面。
    /// </summary>
    ///

    [ComImport]
    [Guid("5b0d3235-4dba-4d44-865e-8f1d0e4fd04d")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    unsafe interface IMemoryBufferByteAccess
    {
        void GetBuffer(out byte* buffer, out uint capacity);
    }

    public sealed partial class MyColorPicker : ContentDialog
    {
        public delegate void ColorChangeCallBack(Color c);
        public ColorChangeCallBack OnColorChange;
        public double _preAngle;
        public Point _preCirclePoint;

        bool _pointerMovedStory = false;
        bool _trianglePressing;
        bool _circlePressing;
        bool _textboxFocusing;
        bool _animating;

        private double _hue;
        public double Hue
        {
            get { return _hue; }
            set
            {
                if (_hue != value)
                {
                    ChangeTriangleColor(value);
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
                SelectedPolygonColor.Fill = new SolidColorBrush(value);
                _currentColor = value;
            }
        }

        public Color PreColor { get; set; }
        public Boolean ColorPickerResult;

        // Triangle
        SoftwareBitmap triangleSoftwareBitmap;
        double triangleSide;
        Point triangleCenter;
        Point trianglePoint1;
        Point trianglePoint2;
        Point trianglePoint3;
        double seletcedColorEllipse_r;

        public MyColorPicker(Color c)
        {
            InitializeComponent();

            ColorPickerResult = false;
            _hue = -1;
            _preAngle = 0;
            _preCirclePoint = new Point(0, 0);
            _trianglePressing = false;
            _circlePressing = false;
            _textboxFocusing = false;
            _animating = false;
            PreColor = c;

            Task curtask = Task.Run(async () => await CreateTriangleImage());
            curtask.Wait();
        }
        private void ColorPickerWindow_Loaded(object sender, RoutedEventArgs e)
        {
            seletcedColorEllipse_r = SeletcedColorEllipse.Height / 2;

            // Ring
            HiddenEllipse.Width = HiddenEllipse.Height = ImageGrid.ActualWidth * 5 / 6;
            ColorRing.Width = ImageGrid.ActualWidth * 5 / 6;
            ColorRing.Height = ImageGrid.ActualWidth * 5 / 6 * (390 / 388); // 393/388 is image of Height/Width
            ColorRing.RenderTransformOrigin = new Point(0.5, (5 + (388 / 2)) / 391);

            //triangleSide = triangleSoftwareBitmap.PixelWidth;
            triangleSide = ImageGrid.ActualWidth * 56 / 100;
            triangleCenter = new Point(ImageGrid.ActualWidth / 2, ImageGrid.ActualHeight / 2);

            // Grid layout
            double grid_c0h = triangleCenter.Y - (Math.Sqrt(3) * triangleSide / 3);
            double grid_c1h = Math.Sqrt(3) * triangleSide / 2;
            double grid_r0w = (ImageGrid.ActualWidth - triangleSide) / 2;
            ImageGrid.RowDefinitions[0].Height = new GridLength(grid_c0h);
            ImageGrid.RowDefinitions[1].Height = new GridLength(grid_c1h);
            ImageGrid.ColumnDefinitions[1].Width = new GridLength(triangleSide);

            // triangle layout
            trianglePoint1 = new Point(grid_r0w, grid_c0h + grid_c1h);
            trianglePoint2 = new Point(trianglePoint1.X + triangleSide, trianglePoint1.Y);
            trianglePoint3 = new Point((trianglePoint1.X + trianglePoint2.X) / 2, grid_c0h);
            TriangleImg.RenderTransformOrigin = new Point(0.5, 2.0 / 3.0);
            HiddenTriangle.Points.Add(trianglePoint1);
            HiddenTriangle.Points.Add(trianglePoint2);
            HiddenTriangle.Points.Add(trianglePoint3);
            HiddenTriangle.RenderTransformOrigin = new Point(0.5, 0.5);

            // selectedColorEllipse default on the Red position
            _preCirclePoint = new Point(trianglePoint3.X, trianglePoint3.Y);

            EnterColor(PreColor, true);
        }
        private async Task CreateTriangleImage()
        {
            string triangleImage = @"Assets\colorpick_triangle.png";

            SoftwareBitmap softwareBitmap;
            StorageFolder InstallationFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            StorageFile triangleImgFile = await InstallationFolder.GetFileAsync(triangleImage);

            using (IRandomAccessStream printingFileStream = await triangleImgFile.OpenAsync(FileAccessMode.Read))
            {
                BitmapDecoder printingDecoder = await BitmapDecoder.CreateAsync(printingFileStream);
                softwareBitmap = await printingDecoder.GetSoftwareBitmapAsync();
            }
            triangleSoftwareBitmap = softwareBitmap;
        }

        public void EnterColor(Color c, bool animation)
        {
            if (c.A == 0) // The recent color has no color
            { c.R = c.G = c.B = 0; } // Black

            CurrentColor = c;
            HSVColor hsv = Math2.RGBToHSV(c);
            Hue = hsv.H;
            Saturation = hsv.S;
            Value = hsv.V;
            CompositeTransform ct = SeletcedColorEllipseCompositeTransform;

            // Compute SeletcedColorEllipse position
            Point targetPoint = Math2.SVToPoint(Saturation, Value, triangleSide);
            targetPoint.X = trianglePoint1.X + targetPoint.X;
            targetPoint.Y = trianglePoint1.Y - targetPoint.Y;
            targetPoint = Math2.RotatePoint(targetPoint, triangleCenter, -Hue);
            targetPoint.X -= seletcedColorEllipse_r;
            targetPoint.Y -= seletcedColorEllipse_r;

            if (animation)
            {
                ColorPickerStoryboardStart(Hue, targetPoint);
            }
            else
            {
                TriangleImgRotation.Angle = Hue;
                SelesctedRingImgRotation.Angle = Hue;
                _preAngle = Hue;
                PolygonTransform.Angle = Hue;

                ct.TranslateX = targetPoint.X;
                ct.TranslateY = targetPoint.Y;
                _preCirclePoint = new Point(ct.TranslateX, ct.TranslateY);
            }
        }
        public void EnterHSV(double h, double s, double v, bool animation)
        {
            Hue = h;
            Saturation = s;
            Value = v;
            CurrentColor = Math2.HSVToRGB(Hue, Saturation, Value);
            CompositeTransform ct = SeletcedColorEllipseCompositeTransform;
            Point targetPoint;

            // Compute SeletcedColorEllipse position
            targetPoint = Math2.RotatePoint(
                new Point(ct.TranslateX + seletcedColorEllipse_r, ct.TranslateY + seletcedColorEllipse_r),
                triangleCenter,
                _preAngle - Hue
                );
            targetPoint.X = targetPoint.X - seletcedColorEllipse_r;
            targetPoint.Y = targetPoint.Y - seletcedColorEllipse_r;

            if (animation)
            {
                ColorPickerStoryboardStart(_preAngle, targetPoint);
            }
            else
            {
                TriangleImgRotation.Angle = Hue;
                SelesctedRingImgRotation.Angle = Hue;
                _preAngle = Hue;
                PolygonTransform.Angle = Hue;

                ct.TranslateX = targetPoint.X;
                ct.TranslateY = targetPoint.Y;
                _preCirclePoint = new Point(ct.TranslateX, ct.TranslateY);
            }
        }
        public void EnterHSV(double h, double s, double v, Point targetPoint, bool animation)
        {
            Hue = h;
            Saturation = s;
            Value = v;
            CurrentColor = Math2.HSVToRGB(Hue, Saturation, Value);
            CompositeTransform ct = SeletcedColorEllipseCompositeTransform;

            if (animation)
            {
                ColorPickerStoryboardStart(_preAngle, targetPoint);
            }
            else
            {
                TriangleImgRotation.Angle = Hue;
                SelesctedRingImgRotation.Angle = Hue;
                _preAngle = Hue;
                PolygonTransform.Angle = Hue;

                ct.TranslateX = targetPoint.X;
                ct.TranslateY = targetPoint.Y;
                _preCirclePoint = new Point(ct.TranslateX, ct.TranslateY);
            }
        }

        private void RBtnDefault_Tapped(object sender, TappedRoutedEventArgs e)
        {
            RadioButton Rbtn = sender as RadioButton;
            EnterColor(((SolidColorBrush)Rbtn.Background).Color, true);
        }
        private void RBtnRecent_Tapped(object sender, TappedRoutedEventArgs e)
        {
            RadioButton Rbtn = sender as RadioButton;
            EnterColor(((SolidColorBrush)Rbtn.Background).Color, true);
        }

        private void OKBtn_Click(object sender, RoutedEventArgs e)
        {
            int ColorCount = 0;
            ColorPickerResult = true;
            this.Hide();

        }
        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            ColorPickerResult = false;
            this.Hide();
        }

        private void TextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            TextBox textBoxContent = sender as TextBox;
            //Press ESC to exit window
            if (e.Key == Windows.System.VirtualKey.Escape)
            {
                ColorPickerResult = false;
                this.Hide();
            }

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
            _textboxFocusing = true;
        }
        private void ColorTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if ((TextBox_R.Text != "") && (TextBox_G.Text != "") && (TextBox_B.Text != ""))
                TextToColor(TextBox_R.Text, TextBox_G.Text, TextBox_B.Text);

            _textboxFocusing = false;
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

            EnterColor(textColor, true);
        }

        private void Circle_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (_textboxFocusing || _animating)
                return;

            _circlePressing = true;
            Point currentLocation = e.GetCurrentPoint(ImageGrid).Position;

            double dx = currentLocation.X - triangleCenter.X;
            double dy = currentLocation.Y - triangleCenter.Y;
            //---
            double hue = Math2.ComputeH(dx, dy);
            EnterHSV(hue, Saturation, Value, true);

            //---

            Hue = Math2.ComputeH(dx, dy);
            CurrentColor = Math2.HSVToRGB(Hue, Saturation, Value);

            // Compute SeletcedColorEllipse position
            CompositeTransform ct = SeletcedColorEllipseCompositeTransform;
            Point targetPoint = Math2.RotatePoint(
                new Point(ct.TranslateX + seletcedColorEllipse_r, ct.TranslateY + seletcedColorEllipse_r),
                triangleCenter,
                _preAngle - Hue
                );
            targetPoint.X = targetPoint.X - seletcedColorEllipse_r;
            targetPoint.Y = targetPoint.Y - seletcedColorEllipse_r;

            ColorPickerStoryboardStart(Hue, targetPoint);
        }
        private void Circle_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
        }
        private void Circle_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            _circlePressing = false;
        }

        private void Polygon_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (_textboxFocusing || _animating)
                return;

            _trianglePressing = true;
            Point currentPoint = e.GetCurrentPoint(this.ImageGrid).Position;

            // Rotation current position "Hue" degrees
            Point rotatedPoint = Math2.RotatePoint(currentPoint, triangleCenter, Hue);

            // Let trianglePoint1 become origin
            Point newPoint = new Point(
                Math.Abs(rotatedPoint.X - trianglePoint1.X),
                Math.Abs(rotatedPoint.Y - trianglePoint1.Y));

            Math2.ComputeSV(newPoint, triangleSide, out Saturation, out Value);

            //Compute SeletcedColorEllipse position
            Point targetPoint = new Point(
                  currentPoint.X - seletcedColorEllipse_r,
                  currentPoint.Y - seletcedColorEllipse_r);

            EnterHSV(Hue, Saturation, Value, targetPoint, true);
        }
        private void Polygon_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (_trianglePressing && !_textboxFocusing)
            {
                Point currentPoint = e.GetCurrentPoint(this.ImageGrid).Position;

                // Rotation current position "Hue" degrees
                Point rotatedPoint = Math2.RotatePoint(currentPoint, triangleCenter, Hue);

                // Let trianglePoint1 become origin
                Point newPoint = new Point(
                    Math.Abs(rotatedPoint.X - trianglePoint1.X),
                    Math.Abs(rotatedPoint.Y - trianglePoint1.Y)
                );

                Math2.ComputeSV(newPoint, triangleSide, out Saturation, out Value);
                CurrentColor = Math2.HSVToRGB(Hue, Saturation, Value);

                // Compute SeletcedColorEllipse
                if (_pointerMovedStory)
                {
                    Point targetPoint = new Point(
                      currentPoint.X - seletcedColorEllipse_r,
                      currentPoint.Y - seletcedColorEllipse_r
                    );

                    ColorPickerStoryboardStart(_preAngle, targetPoint);
                }
                else
                {
                    CompositeTransform ct = SeletcedColorEllipseCompositeTransform;
                    ct.TranslateX = currentPoint.X - seletcedColorEllipse_r;
                    ct.TranslateY = currentPoint.Y - seletcedColorEllipse_r;
                    _preCirclePoint = new Point(ct.TranslateX, ct.TranslateY);
                }
            }
        }
        private void Polygon_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            _trianglePressing = false;
        }

        private async void ChangeTriangleColor(double hue)
        {
            triangleSoftwareBitmap = SoftwareBitmap.Convert(triangleSoftwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight);
            ChangeTriangleColorPixel(hue);
            triangleSoftwareBitmap = SoftwareBitmap.Convert(triangleSoftwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);

            var source = new SoftwareBitmapSource();
            await source.SetBitmapAsync(triangleSoftwareBitmap);
            TriangleImg.Source = source;
        }
        private unsafe void ChangeTriangleColorPixel(double hue)
        {
            HSVColor hsv = new HSVColor(hue, 1.0, 1.0);

            using (BitmapBuffer buffer = triangleSoftwareBitmap.LockBuffer(BitmapBufferAccessMode.Write))
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
                            Point pt = new Point(col, bufferLayout.Height - row);

                            Math2.ComputeSV(pt, imgWidth, out hsv.S, out hsv.V);
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

        private void ColorPickerStoryboardStart(double triangleTargetAngle, Point circleTargetPoint)
        {
            _animating = true;
            int runTime = 300;
            var storyboard = new Storyboard();
            var triangleAnimation = new DoubleAnimation();
            var selesctedRingAnimation = new DoubleAnimation();
            var colorCirclePositionX = new DoubleAnimation();
            var colorCirclePositionY = new DoubleAnimation();

            if (_preAngle != triangleTargetAngle)
            {
                triangleTargetAngle = triangleTargetAngle % 360;
                if (_preAngle - triangleTargetAngle > 180)
                {
                    _preAngle -= 360;
                }
                else if (_preAngle - triangleTargetAngle < -180)
                {
                    _preAngle += 360;
                }

                // triangle
                triangleAnimation.Duration = TimeSpan.FromMilliseconds(runTime);
                triangleAnimation.EnableDependentAnimation = true;
                triangleAnimation.From = _preAngle;
                triangleAnimation.To = triangleTargetAngle;
                Storyboard.SetTargetProperty(triangleAnimation, "Angle");
                Storyboard.SetTarget(triangleAnimation, TriangleImgRotation);
                storyboard.Children.Add(triangleAnimation);

                // selected ring
                selesctedRingAnimation.Duration = TimeSpan.FromMilliseconds(runTime);
                selesctedRingAnimation.EnableDependentAnimation = true;
                selesctedRingAnimation.From = _preAngle;
                selesctedRingAnimation.To = triangleTargetAngle;
                Storyboard.SetTargetProperty(selesctedRingAnimation, "Angle");
                Storyboard.SetTarget(selesctedRingAnimation, SelesctedRingImgRotation);
                storyboard.Children.Add(selesctedRingAnimation);

                _preAngle = triangleTargetAngle;
                PolygonTransform.Angle = triangleTargetAngle;
            }

            if (_preCirclePoint.X != circleTargetPoint.X && _preCirclePoint.Y != circleTargetPoint.Y)
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
            storyboard.Completed += ColorPickerStoryboardCompleted;
        }
        private void ColorPickerStoryboardCompleted(object sender, object e)
        {
            _animating = false;
        }
    }
}
