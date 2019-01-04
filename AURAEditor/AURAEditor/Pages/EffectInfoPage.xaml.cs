using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using AuraEditor.Common;
using AuraEditor.Dialogs;
using static AuraEditor.Common.ControlHelper;
using static AuraEditor.Common.Definitions;
using static AuraEditor.Common.EffectHelper;
using Windows.UI.Xaml.Navigation;

// 空白頁項目範本已記錄在 https://go.microsoft.com/fwlink/?LinkId=234238

namespace AuraEditor.Pages
{
    /// <summary>
    /// 可以在本身使用或巡覽至框架內的空白頁面。
    /// </summary>
    public sealed partial class EffectInfoPage : Page
    {
        private EffectInfo m_Info;

        bool _angleImgPressing;
        Point AngleImgCenter;
        public List<ColorPoint> ColorPoints = new List<ColorPoint>();
        public List<ColorPoint> CustomizeColorPoints = new List<ColorPoint>();

        public EffectInfoPage()
        {
            this.InitializeComponent();

            AngleImgCenter = new Point(40, 40);
            AngleTextBox.Text = "0";

            foreach (var item in DefaultColorList)
            {
                SetListBorder(item);
            }

            SetDefaultPattern();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            m_Info = e.Parameter as EffectInfo;

            if (m_Info == null)
                EffectInfoStackPanel.Visibility = Visibility.Collapsed;

            this.DataContext = m_Info;
            Bindings.Update();
        }
        public void SetDefaultPattern()
        {
            CustomizeRainbow.Foreground = new SolidColorBrush(Colors.White);

            //Button Color
            LinearGradientBrush Pattern1 = new LinearGradientBrush();
            Pattern1.StartPoint = new Point(0, 0.5);
            Pattern1.EndPoint = new Point(1, 0.5);
            for (int i = 0; i < Definitions.DefaultColorList[0].Count; i++)
            {
                Pattern1.GradientStops.Add(new GradientStop { Color = DefaultColorList[0][i].Color, Offset = DefaultColorList[0][i].Offset });
            }

            // Use the brush to paint the rectangle.
            DefaultRainbow1.Foreground = Pattern1;

            // Button Color  
            LinearGradientBrush Pattern2 = new LinearGradientBrush();
            Pattern2.StartPoint = new Point(0, 0.5);
            Pattern2.EndPoint = new Point(1, 0.5);
            for (int i = 0; i < Definitions.DefaultColorList[1].Count; i++)
            {
                Pattern2.GradientStops.Add(new GradientStop { Color = DefaultColorList[1][i].Color, Offset = DefaultColorList[1][i].Offset });
            }

            // Use the brush to paint the rectangle.
            DefaultRainbow2.Foreground = Pattern2;

            // Button Color  
            LinearGradientBrush Pattern3 = new LinearGradientBrush();
            Pattern3.StartPoint = new Point(0, 0.5);
            Pattern3.EndPoint = new Point(1, 0.5);
            for (int i = 0; i < Definitions.DefaultColorList[2].Count; i++)
            {
                Pattern3.GradientStops.Add(new GradientStop { Color = DefaultColorList[2][i].Color, Offset = DefaultColorList[2][i].Offset });
            }

            // Use the brush to paint the rectangle.
            DefaultRainbow3.Foreground = Pattern3;

            // Button Color  
            LinearGradientBrush Pattern4 = new LinearGradientBrush();
            Pattern4.StartPoint = new Point(0, 0.5);
            Pattern4.EndPoint = new Point(1, 0.5);
            for (int i = 0; i < Definitions.DefaultColorList[3].Count; i++)
            {
                Pattern4.GradientStops.Add(new GradientStop { Color = DefaultColorList[3][i].Color, Offset = DefaultColorList[3][i].Offset });
            }

            // Use the brush to paint the rectangle.
            DefaultRainbow4.Foreground = Pattern4;

            // Button Color  
            LinearGradientBrush Pattern5 = new LinearGradientBrush();
            Pattern5.StartPoint = new Point(0, 0.5);
            Pattern5.EndPoint = new Point(1, 0.5);
            for (int i = 0; i < Definitions.DefaultColorList[4].Count; i++)
            {
                Pattern5.GradientStops.Add(new GradientStop { Color = DefaultColorList[4][i].Color, Offset = DefaultColorList[4][i].Offset });
            }

            // Use the brush to paint the rectangle.
            DefaultRainbow5.Foreground = Pattern5;

            // Button Color  
            LinearGradientBrush Pattern6 = new LinearGradientBrush();
            Pattern6.StartPoint = new Point(0, 0.5);
            Pattern6.EndPoint = new Point(1, 0.5);
            for (int i = 0; i < Definitions.DefaultColorList[5].Count; i++)
            {
                Pattern6.GradientStops.Add(new GradientStop { Color = DefaultColorList[5][i].Color, Offset = DefaultColorList[5][i].Offset });
            }

            // Use the brush to paint the rectangle.
            DefaultRainbow6.Foreground = Pattern6;
        }

        private void ShowGroups(string name)
        {
            ResetBtn.Visibility = Visibility.Visible;
            //EffectInfoGroup.Visibility = Visibility.Visible;
            //Title.Text = name;

            switch (name)
            {
                case "Static":
                    ColorGroup.Visibility = Visibility.Visible;
                    RandomCheckBox.Visibility = Visibility.Collapsed;
                    PatternGroup.Visibility = Visibility.Collapsed;
                    BrightnessGroup.Visibility = Visibility.Collapsed;
                    SpeedGroup.Visibility = Visibility.Collapsed;
                    AngleGroup.Visibility = Visibility.Collapsed;
                    TemperatureGroup.Visibility = Visibility.Collapsed;
                    break;
                case "Breath":
                    ColorGroup.Visibility = Visibility.Visible;
                    RandomCheckBox.Visibility = Visibility.Collapsed;
                    PatternGroup.Visibility = Visibility.Collapsed;
                    BrightnessGroup.Visibility = Visibility.Collapsed;
                    SpeedGroup.Visibility = Visibility.Visible;
                    AngleGroup.Visibility = Visibility.Collapsed;
                    TemperatureGroup.Visibility = Visibility.Collapsed;
                    break;
                case "ColorCycle":
                    ColorGroup.Visibility = Visibility.Collapsed;
                    RandomCheckBox.Visibility = Visibility.Collapsed;
                    PatternGroup.Visibility = Visibility.Collapsed;
                    BrightnessGroup.Visibility = Visibility.Collapsed;
                    SpeedGroup.Visibility = Visibility.Visible;
                    AngleGroup.Visibility = Visibility.Collapsed;
                    TemperatureGroup.Visibility = Visibility.Collapsed;
                    break;
                case "Rainbow":
                    ColorGroup.Visibility = Visibility.Collapsed;
                    RandomCheckBox.Visibility = Visibility.Collapsed;
                    PatternGroup.Visibility = Visibility.Visible;
                    BrightnessGroup.Visibility = Visibility.Collapsed;
                    SpeedGroup.Visibility = Visibility.Visible;
                    AngleGroup.Visibility = Visibility.Visible;
                    TemperatureGroup.Visibility = Visibility.Collapsed;
                    break;
                case "Strobing":
                    ColorGroup.Visibility = Visibility.Visible;
                    RandomCheckBox.Visibility = Visibility.Collapsed;
                    PatternGroup.Visibility = Visibility.Collapsed;
                    BrightnessGroup.Visibility = Visibility.Collapsed;
                    SpeedGroup.Visibility = Visibility.Visible;
                    AngleGroup.Visibility = Visibility.Collapsed;
                    TemperatureGroup.Visibility = Visibility.Collapsed;
                    break;
                case "Comet":
                    ColorGroup.Visibility = Visibility.Visible;
                    RandomCheckBox.Visibility = Visibility.Visible;
                    PatternGroup.Visibility = Visibility.Collapsed;
                    BrightnessGroup.Visibility = Visibility.Collapsed;
                    SpeedGroup.Visibility = Visibility.Visible;
                    AngleGroup.Visibility = Visibility.Visible;
                    TemperatureGroup.Visibility = Visibility.Collapsed;
                    break;
                case "Star":
                    ColorGroup.Visibility = Visibility.Visible;
                    RandomCheckBox.Visibility = Visibility.Visible;
                    PatternGroup.Visibility = Visibility.Collapsed;
                    BrightnessGroup.Visibility = Visibility.Collapsed;
                    SpeedGroup.Visibility = Visibility.Visible;
                    AngleGroup.Visibility = Visibility.Collapsed;
                    TemperatureGroup.Visibility = Visibility.Collapsed;
                    break;
                case "Tide":
                    ColorGroup.Visibility = Visibility.Visible;
                    RandomCheckBox.Visibility = Visibility.Collapsed;
                    PatternGroup.Visibility = Visibility.Collapsed;
                    BrightnessGroup.Visibility = Visibility.Collapsed;
                    SpeedGroup.Visibility = Visibility.Visible;
                    AngleGroup.Visibility = Visibility.Visible;
                    TemperatureGroup.Visibility = Visibility.Collapsed;
                    break;
                case "Raidus":
                    ColorGroup.Visibility = Visibility.Collapsed;
                    RandomCheckBox.Visibility = Visibility.Collapsed;
                    PatternGroup.Visibility = Visibility.Collapsed;
                    BrightnessGroup.Visibility = Visibility.Collapsed;
                    SpeedGroup.Visibility = Visibility.Collapsed;
                    AngleGroup.Visibility = Visibility.Collapsed;
                    TemperatureGroup.Visibility = Visibility.Collapsed;
                    break;
                case "Reactive":
                    ColorGroup.Visibility = Visibility.Visible;
                    RandomCheckBox.Visibility = Visibility.Visible;
                    PatternGroup.Visibility = Visibility.Collapsed;
                    BrightnessGroup.Visibility = Visibility.Collapsed;
                    SpeedGroup.Visibility = Visibility.Visible;
                    //DirectionGroup.Visibility = Visibility.Collapsed;
                    AngleGroup.Visibility = Visibility.Collapsed;
                    TemperatureGroup.Visibility = Visibility.Collapsed;
                    break;
                case "Laser":
                    ColorGroup.Visibility = Visibility.Visible;
                    RandomCheckBox.Visibility = Visibility.Visible;
                    PatternGroup.Visibility = Visibility.Collapsed;
                    BrightnessGroup.Visibility = Visibility.Collapsed;
                    SpeedGroup.Visibility = Visibility.Visible;
                    AngleGroup.Visibility = Visibility.Collapsed;
                    TemperatureGroup.Visibility = Visibility.Collapsed;
                    break;
                case "Ripple":
                    ColorGroup.Visibility = Visibility.Visible;
                    RandomCheckBox.Visibility = Visibility.Visible;
                    PatternGroup.Visibility = Visibility.Visible;
                    BrightnessGroup.Visibility = Visibility.Collapsed;
                    SpeedGroup.Visibility = Visibility.Visible;
                    AngleGroup.Visibility = Visibility.Collapsed;
                    TemperatureGroup.Visibility = Visibility.Collapsed;
                    break;
                case "Music":
                    ColorGroup.Visibility = Visibility.Collapsed;
                    RandomCheckBox.Visibility = Visibility.Collapsed;
                    PatternGroup.Visibility = Visibility.Collapsed;
                    BrightnessGroup.Visibility = Visibility.Collapsed;
                    SpeedGroup.Visibility = Visibility.Collapsed;
                    AngleGroup.Visibility = Visibility.Collapsed;
                    TemperatureGroup.Visibility = Visibility.Collapsed;
                    break;
                case "Smart":
                    ColorGroup.Visibility = Visibility.Collapsed;
                    RandomCheckBox.Visibility = Visibility.Collapsed;
                    PatternGroup.Visibility = Visibility.Collapsed;
                    BrightnessGroup.Visibility = Visibility.Collapsed;
                    SpeedGroup.Visibility = Visibility.Collapsed;
                    AngleGroup.Visibility = Visibility.Collapsed;
                    TemperatureGroup.Visibility = Visibility.Visible;
                    break;
                default:
                    ColorGroup.Visibility = Visibility.Collapsed;
                    RandomCheckBox.Visibility = Visibility.Collapsed;
                    PatternGroup.Visibility = Visibility.Collapsed;
                    BrightnessGroup.Visibility = Visibility.Collapsed;
                    SpeedGroup.Visibility = Visibility.Collapsed;
                    AngleGroup.Visibility = Visibility.Collapsed;
                    TemperatureGroup.Visibility = Visibility.Collapsed;
                    break;
            }
        }
        //private void UpdateGroups(EffectInfo info)
        //{
        //    RadioButtonBg.Background = new SolidColorBrush(info.InitColor);
        //    if (info.Random)
        //    {
        //        RandomCheckBox.IsChecked = true;
        //        RadioButtonBg.Opacity = 0.5;
        //        RadioButtonBg.IsEnabled = false;
        //    }
        //    else
        //    {
        //        RandomCheckBox.IsChecked = false;
        //        RadioButtonBg.Opacity = 1;
        //        RadioButtonBg.IsEnabled = true;
        //    }
        //    BrightnessSlider.Value = info.Brightness;
        //    SpeedSlider.Value = info.Speed;
        //    if (SpeedSlider.Value == 0)
        //    {
        //        SlowPoint.Source = new BitmapImage(new Uri(this.BaseUri, "ms-appx:///Assets/EffectInfoGroup/asus_gc_slider2 control_n.png"));
        //        MediumPoint.Source = new BitmapImage(new Uri(this.BaseUri, "ms-appx:///Assets/EffectInfoGroup/asus_gc_slider2 control_d.png"));
        //        FastPoint.Source = new BitmapImage(new Uri(this.BaseUri, "ms-appx:///Assets/EffectInfoGroup/asus_gc_slider2 control_d.png"));
        //    }
        //    AngleStoryboardStart(info.Angle);
        //    AngleTextBox.Text = info.Angle.ToString();
        //    PatternCanvas.Children.Clear();
        //    ColorPoints.Clear();
        //    foreach (var item in info.ColorPointList)
        //    {
        //        ColorPoints.Add(new ColorPoint(item));
        //    }
        //    ShowColorPointUI(ColorPoints);
        //    ReDrawMultiPointRectangle();
        //    SegmentationSwitch.IsOn = info.ColorSegmentation;
        //}

        private void ResetBtn_Tapped(object sender, TappedRoutedEventArgs e)
        {
            m_Info.InitColor = Colors.Red;
            m_Info.Brightness = 3;
            m_Info.Speed = 1;
            m_Info.Angle = 90;
            m_Info.Random = false;
            m_Info.High = 60;
            m_Info.Low = 30;
            m_Info.ColorPointList = new List<ColorPoint>(DefaultColorList[5]);
            m_Info.ColorSegmentation = false;
            //UpdateGroups(m_Info);
        }
        private async void ColorRadioBtn_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Color newColor = await OpenColorPickerWindow(((SolidColorBrush)RadioButtonBg.Background).Color);
            RadioButtonBg.Background = new SolidColorBrush(newColor);
        }
        public async Task<Color> OpenColorPickerWindow(Color c)
        {
            ColorPickerDialog colorPickerDialog = new ColorPickerDialog(c);
            await colorPickerDialog.ShowAsync();

            if (colorPickerDialog.ColorPickerResult)
            {
                return colorPickerDialog.CurrentColor;
            }
            else
            {
                return colorPickerDialog.PreColor;
            }
        }
        private void RandomCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (RandomCheckBox.IsChecked == true)
            {
                RadioButtonBg.Opacity = 0.5;
                RadioButtonBg.IsEnabled = false;
            }
            else
            {
                RadioButtonBg.Opacity = 1;
                RadioButtonBg.IsEnabled = true;
            }
        }
        private void BrightnessValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Slider slider = sender as Slider;
            if (slider != null)
            {
                if (slider.Value == 1)
                {
                    BrightnessTextBlock.Text = "33%";
                    //ImagePoint33.Source = new BitmapImage(new Uri(this.BaseUri, "Assets/AURASettings/asus_gc_slider2 control_h.png"));
                    //ImagePoint66.Source = new BitmapImage(new Uri(this.BaseUri, "Assets/AURASettings/asus_gc_slider2 control_d.png"));
                }
                else if (slider.Value == 2)
                {
                    BrightnessTextBlock.Text = "66%";
                    //ImagePoint33.Source = new BitmapImage(new Uri(this.BaseUri, "Assets/AURASettings/asus_gc_slider2 control_d.png"));
                    //ImagePoint66.Source = new BitmapImage(new Uri(this.BaseUri, "Assets/AURASettings/asus_gc_slider2 control_h.png"));
                }
                else if (slider.Value == 3)
                {
                    BrightnessTextBlock.Text = "100%";
                    //ImagePoint33.Source = new BitmapImage(new Uri(this.BaseUri, "Assets/AURASettings/asus_gc_slider2 control_d.png"));
                    //ImagePoint66.Source = new BitmapImage(new Uri(this.BaseUri, "Assets/AURASettings/asus_gc_slider2 control_d.png"));
                }
                else
                {
                    BrightnessTextBlock.Text = "0%";
                    //ImagePoint33.Source = new BitmapImage(new Uri(this.BaseUri, "Assets/AURASettings/asus_gc_slider2 control_d.png"));
                    //ImagePoint66.Source = new BitmapImage(new Uri(this.BaseUri, "Assets/AURASettings/asus_gc_slider2 control_d.png"));
                }
                m_Info.Brightness = (int)slider.Value;
            }
        }
        private void SpeedValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Slider slider = sender as Slider;
            if (slider != null)
            {
                if (slider.Value == 1)
                {
                    MediumPoint.Source = new BitmapImage(new Uri(this.BaseUri, "ms-appx:///Assets/EffectInfoGroup/asus_gc_slider2 control_n.png"));
                    FastPoint.Source = new BitmapImage(new Uri(this.BaseUri, "ms-appx:///Assets/EffectInfoGroup/asus_gc_slider2 control_d.png"));
                }
                else if (slider.Value == 2)
                {
                    MediumPoint.Source = new BitmapImage(new Uri(this.BaseUri, "ms-appx:///Assets/EffectInfoGroup/asus_gc_slider2 control_n.png"));
                    FastPoint.Source = new BitmapImage(new Uri(this.BaseUri, "ms-appx:///Assets/EffectInfoGroup/asus_gc_slider2 control_n.png"));
                }
                else
                {
                    MediumPoint.Source = new BitmapImage(new Uri(this.BaseUri, "ms-appx:///Assets/EffectInfoGroup/asus_gc_slider2 control_d.png"));
                    FastPoint.Source = new BitmapImage(new Uri(this.BaseUri, "ms-appx:///Assets/EffectInfoGroup/asus_gc_slider2 control_d.png"));
                }
            }
        }

        #region -- Angle --
        private void AngleBgImg_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _angleImgPressing = true;
            Point currentLocation = e.GetCurrentPoint(AngleGrid).Position;

            double dx = currentLocation.X - AngleImgCenter.X;
            double dy = currentLocation.Y - AngleImgCenter.Y;
            double hue = Math2.ComputeH(dx, dy);
            AngleStoryboardStart(hue);
        }
        private void AngleBgImg_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            _angleImgPressing = false;
        }
        private void AngleBgImg_PointerMoved(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Point currentLocation = e.GetCurrentPoint(AngleGrid).Position;
            if (_angleImgPressing)
            {
                double dx = currentLocation.X - AngleImgCenter.X;
                double dy = currentLocation.Y - AngleImgCenter.Y;
                double hue = Math2.ComputeH(dx, dy);
                AngleIcImgRotation.Angle = hue;
            }
        }
        private void AngleTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            TextBox textBoxContent = sender as TextBox;

            if (!e.Key.ToString().Contains("Number"))
            {
                e.Handled = true;
            }

            //Press Enter to change graphic
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                if (textBoxContent.Text != "")
                {
                    AngleStoryboardStart(Convert.ToDouble(textBoxContent.Text));
                }
            }
        }
        private void AngleTextBox_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            string angleText = AngleTextBox.Text;
            char[] originalText = angleText.ToCharArray();

            for (int i = 0; i < originalText.Length; i++)
            {
                char c = originalText[i];
                if (!(Char.IsNumber(c)))
                {
                    if (i == 0 && c == '-')
                        continue;

                    angleText = angleText.Replace(Convert.ToString(c), "");
                    break;
                }
            }
            
            bool successful = Double.TryParse(angleText, out double d);
            if (successful)
            {
                int value = (int)d;
                value = (value + 360) % 360; // +360 make it positive
                AngleTextBox.Text = value.ToString();
            }
            else
            {
                AngleTextBox.Text = "90";
            }
        }
        private void AngleTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (AngleTextBox.Text != "")
            {
                AngleStoryboardStart(Convert.ToDouble(AngleTextBox.Text));
            }
        }
        private void AngleStoryboardStart(double targetAngle)
        {
            int runTime = 300;
            var storyboard = new Storyboard();
            var angleIcAnimation = new DoubleAnimation();
            double sourceAngle = AngleIcImgRotation.Angle;
            if (sourceAngle != targetAngle)
            {
                targetAngle = targetAngle % 360;
                if (sourceAngle - targetAngle > 180)
                {
                    sourceAngle -= 360;
                }
                else if (sourceAngle - targetAngle < -180)
                {
                    sourceAngle += 360;
                }

                // triangle
                angleIcAnimation.Duration = TimeSpan.FromMilliseconds(runTime);
                angleIcAnimation.EnableDependentAnimation = true;
                angleIcAnimation.From = sourceAngle;
                angleIcAnimation.To = targetAngle;
                Storyboard.SetTargetProperty(angleIcAnimation, "Angle");
                Storyboard.SetTarget(angleIcAnimation, AngleIcImgRotation);
                storyboard.Children.Add(angleIcAnimation);
                storyboard.Begin();
            }
        }
        private void IncreaseBtn_Click(object sender, RoutedEventArgs e)
        {
            if ((AngleTextBox.Text != ""))
            {
                double textIncrease = Convert.ToDouble(AngleTextBox.Text);
                textIncrease += 5;
                textIncrease %= 360;
                AngleStoryboardStart(textIncrease);
            }
        }
        private void DecreaseBtn_Click(object sender, RoutedEventArgs e)
        {
            if ((AngleTextBox.Text != ""))
            {
                double textdecrease = Convert.ToDouble(AngleTextBox.Text);
                textdecrease -= 5;
                textdecrease %= 360;
                AngleStoryboardStart(textdecrease);
            }
        }
        #endregion

        private void DefaultRainbow_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem mf = sender as MenuFlyoutItem;
            ClearAndDraw(mf, DefaultColorList[(int)Char.GetNumericValue(mf.Name[mf.Name.Length - 1]) - 1]);
        }

        private void CustomizeRainbow_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem mf = sender as MenuFlyoutItem;
            ClearAndDraw(mf, CustomizeColorPoints);
        }

        private void ClearAndDraw(MenuFlyoutItem mf, List<ColorPoint> cp)
        {
            PatternCanvas.Children.Clear();
            foreach (var item in ColorPoints)
            {
                item.UI.OnRedraw -= ReDrawMultiPointRectangle;
            }
            ColorPoints.Clear();
            if (m_Info.ColorPointList != null)
                m_Info.ColorPointList.Clear();

            foreach (var item in cp)
            {
                ColorPoints.Add(new ColorPoint(item));
                m_Info.ColorPointList.Add(new ColorPoint(item));
            }
            ShowColorPointUI(ColorPoints);
            MultiPointRectangle.Fill = PatternPolygon.Fill = mf.Foreground;
        }

        public void ShowColorPointUI(List<ColorPoint> cl)
        {
            for (int i = 0; i < cl.Count; i++)
            {
                if (i == 0)
                {
                    List<RadioButton> items = FindAllControl<RadioButton>(cl[i].UI, typeof(RadioButton));
                    items[0].IsChecked = true;
                }
                cl[i].UI.OnRedraw += ReDrawMultiPointRectangle; ;
                PatternCanvas.Children.Add(cl[i].UI);
            }
        }

        private void PlusItemBt(object sender, RoutedEventArgs e)
        {
            ColorPoint newColorPointBt = new ColorPoint();
            AddColorPoint(newColorPointBt);
            newColorPointBt.UI.OnRedraw += ReDrawMultiPointRectangle;
            ReDrawMultiPointRectangle();
        }
        private void AddColorPoint(ColorPoint colorPoint)
        {
            int curIndex = 0;
            if (ColorPoints.Count < 7)
            {
                foreach (var item in ColorPoints)
                {
                    List<RadioButton> items = FindAllControl<RadioButton>(item.UI, typeof(RadioButton));

                    if (items[0].IsChecked == true)
                    {
                        curIndex = ColorPoints.IndexOf(item);
                        if ((curIndex + 1) == ColorPoints.Count)
                        {
                            if ((ColorPoints[curIndex].UI.X - ColorPoints[curIndex - 1].UI.X) < 25)
                            {
                                ColorPoints.Add(colorPoint);
                                ColorPoints.Remove(ColorPoints[ColorPoints.Count - 1]);
                                return;
                            }
                            else
                            {
                                colorPoint.UI.X = (ColorPoints[curIndex - 1].UI.X + ColorPoints[curIndex].UI.X) / 2;
                                ColorPoints.Insert(curIndex, colorPoint);
                            }
                        }
                        else
                        {
                            if ((ColorPoints[curIndex + 1].UI.X - ColorPoints[curIndex].UI.X) < 25)
                            {
                                ColorPoints.Add(colorPoint);
                                ColorPoints.Remove(ColorPoints[ColorPoints.Count - 1]);
                                return;
                            }
                            else
                            {
                                colorPoint.UI.X = (ColorPoints[curIndex].UI.X + ColorPoints[curIndex + 1].UI.X) / 2;
                                ColorPoints.Insert(curIndex + 1, colorPoint);
                            }
                        }
                        colorPoint.Color = item.Color;
                        PatternCanvas.Children.Add(colorPoint.UI);
                        break;
                    }
                }
            }
        }
        private void MinusItemBt(object sender, RoutedEventArgs e)
        {
            RemoveColorPoint();
            ReDrawMultiPointRectangle();
        }
        private void RemoveColorPoint()
        {
            if (ColorPoints.Count > 2)
            {
                foreach (var item in ColorPoints)
                {
                    List<RadioButton> items = FindAllControl<RadioButton>(item.UI, typeof(RadioButton));

                    if (items[0].IsChecked == true)
                    {
                        for (int i = 0; i < ColorPoints.Count; i++)
                        {
                            if (item == ColorPoints[i])
                            {
                                if (i != ColorPoints.Count - 1)
                                {
                                    List<RadioButton> items1 = FindAllControl<RadioButton>(ColorPoints[i + 1].UI, typeof(RadioButton));
                                    items1[0].IsChecked = true;
                                }
                                else
                                {
                                    List<RadioButton> items1 = FindAllControl<RadioButton>(ColorPoints[i - 1].UI, typeof(RadioButton));
                                    items1[0].IsChecked = true;
                                }
                            }
                        }
                        item.UI.OnRedraw -= ReDrawMultiPointRectangle;
                        ColorPoints.Remove(item);
                        PatternCanvas.Children.Remove(item.UI);
                        break;
                    }
                }
            }
        }

        public void ReDrawMultiPointRectangle()
        {
            LinearGradientBrush Pattern = new LinearGradientBrush();
            Pattern.StartPoint = new Point(0, 0.5);
            Pattern.EndPoint = new Point(1, 0.5);

            for (int i = 0; i < ColorPoints.Count; i++)
            {
                Pattern.GradientStops.Add(new GradientStop { Color = ColorPoints[i].Color, Offset = ColorPoints[i].Offset });
            }

            PatternPolygon.Fill = CustomizeRainbow.Foreground = MultiPointRectangle.Fill = Pattern;
            CustomizeColorPoints = new List<ColorPoint>(ColorPoints);
            m_Info.ColorPointList = new List<ColorPoint>(ColorPoints);
            SetListBorder(ColorPoints);
        }
    }
}
