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
using System.Collections.ObjectModel;
using AuraEditor.Models;
using System.Linq;

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

        public EffectInfoPage()
        {
            this.InitializeComponent();

            AngleImgCenter = new Point(40, 40);
            AngleTextBox.Text = "0";
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            m_Info = e.Parameter as EffectInfo;

            if (m_Info == null)
            {
                EffectInfoStackPanel.Visibility = Visibility.Collapsed;
                return;
            }

            this.DataContext = m_Info;

            ColorPatternModel patternModel = new ColorPatternModel(m_Info);
            ColorPattern.DataContext = patternModel;
            Bindings.Update();
        }
        private void ShowGroups(string name)
        {
            ResetBtn.Visibility = Visibility.Visible;

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

        private void ResetBtn_Tapped(object sender, TappedRoutedEventArgs e)
        {
            m_Info.InitColor = Colors.Red;
            m_Info.Brightness = 3;
            m_Info.Speed = 1;
            m_Info.Angle = 90;
            m_Info.Random = false;
            m_Info.High = 60;
            m_Info.Low = 30;
            m_Info.ColorPointList = new List<ColorPointModel>(DefaultColorPointListCollection[5]); // TODO
            m_Info.ColorSegmentation = false;
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
    }
}
