using AuraEditor.Common;
using AuraEditor.Dialogs;
using AuraEditor.UserControls;
using System;
using System.Diagnostics;
using Windows.Foundation;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Shapes;
using static AuraEditor.Common.EffectHelper;
using System.Threading.Tasks;

namespace AuraEditor
{
    public sealed partial class MainPage : Page
    {
        internal FlyoutBase m_flyoutBase;

        private TimelineEffect _selectedEffectLine;
        public TimelineEffect SelectedEffectLine
        {
            get
            {
                return _selectedEffectLine;
            }
            set
            {
                if (value == null)
                {
                    ClearEffectInfoGrid();
                }
                else if (_selectedEffectLine == value)
                {
                    _selectedEffectLine.UI.IsSelected = true;
                }
                else
                {
                    if (_selectedEffectLine != null)
                        _selectedEffectLine.UI.IsSelected = false;

                    _selectedEffectLine = value;
                    UpdateEffectInfoGrid(value);
                }
            }
        }

        public void ClearEffectInfoGrid()
        {
            Title.Text = "";
            ResetBtn.Visibility = Visibility.Collapsed;
            ColorRect.Visibility = Visibility.Collapsed;
            EffectInfoGroup.Visibility = Visibility.Collapsed;
        }
        private void UpdateEffectInfoGrid(TimelineEffect effect)
        {
            //EffectLine border = effect.UI;
            ShowEffectInfoGroupsByType(effect.Type);
            UpdateGroupContents(effect.Info);
            UpdateUIEffectContents(effect.UInfo);

            //if (border.Background is SolidColorBrush)
            //{
            //    ColorRect.Fill = (SolidColorBrush)border.Background;
            //}
        }

        private void ShowEffectInfoGroupsByType(int effectType)
        {
            ResetBtn.Visibility = Visibility.Visible;
            EffectInfoGroup.Visibility = Visibility.Visible;
            Title.Text = GetEffectName(effectType);

            switch (Title.Text)
            {
                case "Static":
                    ColorRect.Visibility = Visibility.Visible;
                    WaveTypeComboBox.Visibility = Visibility.Collapsed;
                    MinTextBox.Visibility = Visibility.Collapsed;
                    MaxTextBox.Visibility = Visibility.Visible;
                    WaveLenTextBox.Visibility = Visibility.Visible;
                    FreqTextBox.Visibility = Visibility.Collapsed;
                    PhaseTextBox.Visibility = Visibility.Visible;
                    VelocityTextBox.Visibility = Visibility.Collapsed;
                    StartTextBox.Visibility = Visibility.Collapsed;
                    //steven UI
                    ColorGroup.Visibility = Visibility.Visible;
                    RandomCheckBox.Visibility = Visibility.Collapsed;
                    BrightnessGroup.Visibility = Visibility.Collapsed;
                    SpeedGroup.Visibility = Visibility.Collapsed;
                    DirectionGroup.Visibility = Visibility.Collapsed;
                    AngleGroup.Visibility = Visibility.Collapsed;
                    TemperatureGroup.Visibility = Visibility.Collapsed;
                    break;
                case "Breath":
                    ColorRect.Visibility = Visibility.Visible;
                    WaveTypeComboBox.Visibility = Visibility.Visible;
                    MinTextBox.Visibility = Visibility.Visible;
                    MaxTextBox.Visibility = Visibility.Visible;
                    WaveLenTextBox.Visibility = Visibility.Visible;
                    FreqTextBox.Visibility = Visibility.Visible;
                    PhaseTextBox.Visibility = Visibility.Visible;
                    VelocityTextBox.Visibility = Visibility.Collapsed;
                    StartTextBox.Visibility = Visibility.Collapsed;
                    //steven UI
                    ColorGroup.Visibility = Visibility.Visible;
                    RandomCheckBox.Visibility = Visibility.Collapsed;
                    BrightnessGroup.Visibility = Visibility.Collapsed;
                    SpeedGroup.Visibility = Visibility.Visible;
                    DirectionGroup.Visibility = Visibility.Collapsed;
                    AngleGroup.Visibility = Visibility.Collapsed;
                    TemperatureGroup.Visibility = Visibility.Collapsed;
                    break;
                case "ColorCycle":
                    ColorRect.Visibility = Visibility.Collapsed;
                    WaveTypeComboBox.Visibility = Visibility.Visible;
                    MinTextBox.Visibility = Visibility.Visible;
                    MaxTextBox.Visibility = Visibility.Visible;
                    WaveLenTextBox.Visibility = Visibility.Visible;
                    FreqTextBox.Visibility = Visibility.Visible;
                    PhaseTextBox.Visibility = Visibility.Visible;
                    VelocityTextBox.Visibility = Visibility.Collapsed;
                    StartTextBox.Visibility = Visibility.Collapsed;
                    //steven UI
                    ColorGroup.Visibility = Visibility.Collapsed;
                    RandomCheckBox.Visibility = Visibility.Collapsed;
                    BrightnessGroup.Visibility = Visibility.Collapsed;
                    SpeedGroup.Visibility = Visibility.Visible;
                    DirectionGroup.Visibility = Visibility.Collapsed;
                    AngleGroup.Visibility = Visibility.Collapsed;
                    TemperatureGroup.Visibility = Visibility.Collapsed;
                    break;
                case "Rainbow":
                    ColorRect.Visibility = Visibility.Collapsed;
                    WaveTypeComboBox.Visibility = Visibility.Visible;
                    MinTextBox.Visibility = Visibility.Visible;
                    MaxTextBox.Visibility = Visibility.Visible;
                    WaveLenTextBox.Visibility = Visibility.Visible;
                    FreqTextBox.Visibility = Visibility.Visible;
                    PhaseTextBox.Visibility = Visibility.Visible;
                    VelocityTextBox.Visibility = Visibility.Collapsed;
                    StartTextBox.Visibility = Visibility.Collapsed;
                    //steven UI
                    ColorGroup.Visibility = Visibility.Collapsed;
                    RandomCheckBox.Visibility = Visibility.Collapsed;
                    BrightnessGroup.Visibility = Visibility.Collapsed;
                    SpeedGroup.Visibility = Visibility.Visible;
                    DirectionGroup.Visibility = Visibility.Visible;
                    AngleGroup.Visibility = Visibility.Visible;
                    TemperatureGroup.Visibility = Visibility.Collapsed;
                    break;
                case "Strobing":
                    ColorRect.Visibility = Visibility.Visible;
                    WaveTypeComboBox.Visibility = Visibility.Visible;
                    MinTextBox.Visibility = Visibility.Visible;
                    MaxTextBox.Visibility = Visibility.Visible;
                    WaveLenTextBox.Visibility = Visibility.Visible;
                    FreqTextBox.Visibility = Visibility.Visible;
                    PhaseTextBox.Visibility = Visibility.Visible;
                    VelocityTextBox.Visibility = Visibility.Collapsed;
                    StartTextBox.Visibility = Visibility.Collapsed;
                    //steven UI
                    ColorGroup.Visibility = Visibility.Visible;
                    RandomCheckBox.Visibility = Visibility.Collapsed;
                    BrightnessGroup.Visibility = Visibility.Collapsed;
                    SpeedGroup.Visibility = Visibility.Visible;
                    DirectionGroup.Visibility = Visibility.Visible;
                    AngleGroup.Visibility = Visibility.Visible;
                    TemperatureGroup.Visibility = Visibility.Collapsed;
                    break;
                case "Comet":
                    ColorRect.Visibility = Visibility.Visible;
                    WaveTypeComboBox.Visibility = Visibility.Visible;
                    MinTextBox.Visibility = Visibility.Visible;
                    MaxTextBox.Visibility = Visibility.Visible;
                    WaveLenTextBox.Visibility = Visibility.Visible;
                    FreqTextBox.Visibility = Visibility.Visible;
                    PhaseTextBox.Visibility = Visibility.Visible;
                    VelocityTextBox.Visibility = Visibility.Visible;
                    StartTextBox.Visibility = Visibility.Collapsed;
                    //steven UI
                    ColorGroup.Visibility = Visibility.Visible;
                    RandomCheckBox.Visibility = Visibility.Visible;
                    BrightnessGroup.Visibility = Visibility.Collapsed;
                    SpeedGroup.Visibility = Visibility.Visible;
                    DirectionGroup.Visibility = Visibility.Collapsed;
                    AngleGroup.Visibility = Visibility.Collapsed;
                    TemperatureGroup.Visibility = Visibility.Collapsed;
                    break;
                case "Star":
                    ColorRect.Visibility = Visibility.Visible;
                    WaveTypeComboBox.Visibility = Visibility.Visible;
                    MinTextBox.Visibility = Visibility.Visible;
                    MaxTextBox.Visibility = Visibility.Visible;
                    WaveLenTextBox.Visibility = Visibility.Visible;
                    FreqTextBox.Visibility = Visibility.Visible;
                    PhaseTextBox.Visibility = Visibility.Visible;
                    VelocityTextBox.Visibility = Visibility.Collapsed;
                    StartTextBox.Visibility = Visibility.Collapsed;
                    //steven UI
                    ColorGroup.Visibility = Visibility.Visible;
                    RandomCheckBox.Visibility = Visibility.Visible;
                    BrightnessGroup.Visibility = Visibility.Collapsed;
                    SpeedGroup.Visibility = Visibility.Visible;
                    DirectionGroup.Visibility = Visibility.Collapsed;
                    AngleGroup.Visibility = Visibility.Collapsed;
                    TemperatureGroup.Visibility = Visibility.Collapsed;
                    break;
                case "Raidus":
                    ColorRect.Visibility = Visibility.Collapsed;
                    WaveTypeComboBox.Visibility = Visibility.Visible;
                    MinTextBox.Visibility = Visibility.Visible;
                    MaxTextBox.Visibility = Visibility.Visible;
                    WaveLenTextBox.Visibility = Visibility.Visible;
                    FreqTextBox.Visibility = Visibility.Visible;
                    PhaseTextBox.Visibility = Visibility.Visible;
                    VelocityTextBox.Visibility = Visibility.Collapsed;
                    StartTextBox.Visibility = Visibility.Collapsed;
                    //steven UI
                    ColorGroup.Visibility = Visibility.Collapsed;
                    RandomCheckBox.Visibility = Visibility.Collapsed;
                    BrightnessGroup.Visibility = Visibility.Collapsed;
                    SpeedGroup.Visibility = Visibility.Collapsed;
                    DirectionGroup.Visibility = Visibility.Collapsed;
                    AngleGroup.Visibility = Visibility.Collapsed;
                    TemperatureGroup.Visibility = Visibility.Collapsed;
                    break;
                case "Reactive":
                    ColorRect.Visibility = Visibility.Visible;
                    WaveTypeComboBox.Visibility = Visibility.Visible;
                    MinTextBox.Visibility = Visibility.Visible;
                    MaxTextBox.Visibility = Visibility.Visible;
                    WaveLenTextBox.Visibility = Visibility.Visible;
                    FreqTextBox.Visibility = Visibility.Visible;
                    PhaseTextBox.Visibility = Visibility.Visible;
                    VelocityTextBox.Visibility = Visibility.Collapsed;
                    StartTextBox.Visibility = Visibility.Collapsed;
                    //steven UI
                    ColorGroup.Visibility = Visibility.Visible;
                    RandomCheckBox.Visibility = Visibility.Visible;
                    BrightnessGroup.Visibility = Visibility.Collapsed;
                    SpeedGroup.Visibility = Visibility.Visible;
                    DirectionGroup.Visibility = Visibility.Collapsed;
                    AngleGroup.Visibility = Visibility.Collapsed;
                    TemperatureGroup.Visibility = Visibility.Collapsed;
                    break;
                case "Laser":
                    ColorRect.Visibility = Visibility.Visible;
                    WaveTypeComboBox.Visibility = Visibility.Visible;
                    MinTextBox.Visibility = Visibility.Visible;
                    MaxTextBox.Visibility = Visibility.Visible;
                    WaveLenTextBox.Visibility = Visibility.Visible;
                    FreqTextBox.Visibility = Visibility.Visible;
                    PhaseTextBox.Visibility = Visibility.Visible;
                    VelocityTextBox.Visibility = Visibility.Visible;
                    StartTextBox.Visibility = Visibility.Collapsed;
                    //steven UI
                    ColorGroup.Visibility = Visibility.Visible;
                    RandomCheckBox.Visibility = Visibility.Visible;
                    BrightnessGroup.Visibility = Visibility.Collapsed;
                    SpeedGroup.Visibility = Visibility.Visible;
                    DirectionGroup.Visibility = Visibility.Collapsed;
                    AngleGroup.Visibility = Visibility.Collapsed;
                    TemperatureGroup.Visibility = Visibility.Collapsed;
                    break;
                case "Ripple":
                    ColorRect.Visibility = Visibility.Visible;
                    WaveTypeComboBox.Visibility = Visibility.Visible;
                    MinTextBox.Visibility = Visibility.Visible;
                    MaxTextBox.Visibility = Visibility.Visible;
                    WaveLenTextBox.Visibility = Visibility.Visible;
                    FreqTextBox.Visibility = Visibility.Visible;
                    PhaseTextBox.Visibility = Visibility.Visible;
                    VelocityTextBox.Visibility = Visibility.Visible;
                    StartTextBox.Visibility = Visibility.Collapsed;
                    //steven UI
                    ColorGroup.Visibility = Visibility.Visible;
                    RandomCheckBox.Visibility = Visibility.Visible;
                    BrightnessGroup.Visibility = Visibility.Collapsed;
                    SpeedGroup.Visibility = Visibility.Visible;
                    DirectionGroup.Visibility = Visibility.Collapsed;
                    AngleGroup.Visibility = Visibility.Collapsed;
                    TemperatureGroup.Visibility = Visibility.Collapsed;
                    break;
                case "Music":
                    ColorRect.Visibility = Visibility.Collapsed;
                    WaveTypeComboBox.Visibility = Visibility.Collapsed;
                    MinTextBox.Visibility = Visibility.Collapsed;
                    MaxTextBox.Visibility = Visibility.Collapsed;
                    WaveLenTextBox.Visibility = Visibility.Collapsed;
                    FreqTextBox.Visibility = Visibility.Collapsed;
                    PhaseTextBox.Visibility = Visibility.Collapsed;
                    VelocityTextBox.Visibility = Visibility.Collapsed;
                    StartTextBox.Visibility = Visibility.Collapsed;
                    //steven UI
                    ColorGroup.Visibility = Visibility.Collapsed;
                    RandomCheckBox.Visibility = Visibility.Collapsed;
                    BrightnessGroup.Visibility = Visibility.Collapsed;
                    SpeedGroup.Visibility = Visibility.Collapsed;
                    DirectionGroup.Visibility = Visibility.Collapsed;
                    AngleGroup.Visibility = Visibility.Collapsed;
                    TemperatureGroup.Visibility = Visibility.Collapsed;
                    break;
                case "Smart":
                    ColorRect.Visibility = Visibility.Collapsed;
                    WaveTypeComboBox.Visibility = Visibility.Collapsed;
                    MinTextBox.Visibility = Visibility.Collapsed;
                    MaxTextBox.Visibility = Visibility.Collapsed;
                    WaveLenTextBox.Visibility = Visibility.Collapsed;
                    FreqTextBox.Visibility = Visibility.Collapsed;
                    PhaseTextBox.Visibility = Visibility.Collapsed;
                    VelocityTextBox.Visibility = Visibility.Collapsed;
                    StartTextBox.Visibility = Visibility.Collapsed;
                    //steven UI
                    ColorGroup.Visibility = Visibility.Collapsed;
                    RandomCheckBox.Visibility = Visibility.Collapsed;
                    BrightnessGroup.Visibility = Visibility.Collapsed;
                    SpeedGroup.Visibility = Visibility.Collapsed;
                    DirectionGroup.Visibility = Visibility.Collapsed;
                    AngleGroup.Visibility = Visibility.Collapsed;
                    TemperatureGroup.Visibility = Visibility.Visible;
                    break;
                default:
                    ColorRect.Visibility = Visibility.Collapsed;
                    WaveTypeComboBox.Visibility = Visibility.Collapsed;
                    MinTextBox.Visibility = Visibility.Collapsed;
                    MaxTextBox.Visibility = Visibility.Collapsed;
                    WaveLenTextBox.Visibility = Visibility.Collapsed;
                    FreqTextBox.Visibility = Visibility.Collapsed;
                    PhaseTextBox.Visibility = Visibility.Collapsed;
                    VelocityTextBox.Visibility = Visibility.Collapsed;
                    StartTextBox.Visibility = Visibility.Collapsed;
                    //steven UI
                    ColorGroup.Visibility = Visibility.Collapsed;
                    RandomCheckBox.Visibility = Visibility.Collapsed;
                    BrightnessGroup.Visibility = Visibility.Collapsed;
                    SpeedGroup.Visibility = Visibility.Collapsed;
                    DirectionGroup.Visibility = Visibility.Collapsed;
                    AngleGroup.Visibility = Visibility.Collapsed;
                    TemperatureGroup.Visibility = Visibility.Collapsed;
                    break;
            }
        }
        private void UpdateGroupContents(EffectInfo info)
        {
            ColorRect.Fill = new SolidColorBrush(info.InitColor);
            WaveTypeComboBox.SelectedIndex = info.Waves[0].WaveType;
            MinTextBox.Text = info.Waves[0].Min.ToString();
            MaxTextBox.Text = info.Waves[0].Max.ToString();
            WaveLenTextBox.Text = info.Waves[0].WaveLength.ToString();
            FreqTextBox.Text = info.Waves[0].Freq.ToString();
            PhaseTextBox.Text = info.Waves[0].Phase.ToString();
            StartTextBox.Text = info.Waves[0].Start.ToString();
            VelocityTextBox.Text = info.Waves[0].Velocity.ToString();
        }
        private void UpdateUIEffectContents(UIInfo Uinfo)
        {
            RadioButtonBg.Background = new SolidColorBrush(Uinfo.InitColor);
            if (Uinfo.Random)
            {
                RandomCheckBox.IsChecked = true;
                RadioButtonBg.Background = new SolidColorBrush(Colors.Gray);
                RadioButtonBg.IsEnabled = false;
            }
            else
            {
                RandomCheckBox.IsChecked = false;
                RadioButtonBg.Background = new SolidColorBrush(Uinfo.InitColor);
                RadioButtonBg.IsEnabled = true;
            }
            BrightnessSlider.Value = Uinfo.Brightness;
            SpeedSlider.Value = Uinfo.Speed;
            switch (Uinfo.Direction)
            {
                case 1: Left.IsChecked = true; break;
                case 2: Right.IsChecked = true; break;
                case 3: Up.IsChecked = true; break;
                case 4: Down.IsChecked = true; break;
            }
            AngleStoryboardStart(Uinfo.Angle);
            AngleTextBox.Text = Uinfo.Angle.ToString();
        }

        private void ColorPickerOk_Click(object sender, RoutedEventArgs e)
        {
            EffectLine effectLineUI = SelectedEffectLine.UI;
            Color resultColor = ColorPicker.Color;
            SolidColorBrush scb = new SolidColorBrush(resultColor);

            SelectedEffectLine.Info.InitColor = resultColor;
            ColorRect.Fill = scb;
            //border.Background = scb;
            m_flyoutBase.Hide();
        }
        private void ColorPickerCancel_Click(object sender, RoutedEventArgs e)
        {
            m_flyoutBase.Hide();
        }
        private void ColorRect_Pressed(object sender, PointerRoutedEventArgs e)
        {
            m_flyoutBase = Flyout.GetAttachedFlyout(sender as Rectangle);
            Flyout.ShowAttachedFlyout(sender as Rectangle);
        }
        private void WaveTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Initialization has not been completed
            if (SelectedEffectLine == null)
                return;

            EffectInfo ei = SelectedEffectLine.Info;

            string waveName = e.AddedItems[0].ToString();
            switch (waveName)
            {
                case "SineWave":
                    ei.Waves[0].WaveType = 0;
                    break;
                case "HalfSineWave":
                    ei.Waves[0].WaveType = 1;
                    break;
                case "QuarterSineWave":
                    ei.Waves[0].WaveType = 2;
                    break;
                case "SquareWave":
                    ei.Waves[0].WaveType = 3;
                    break;
                case "TriangleWave":
                    ei.Waves[0].WaveType = 4;
                    break;
                case "SawToothleWave":
                    ei.Waves[0].WaveType = 5;
                    break;
                case "ConstantWave":
                    ei.Waves[0].WaveType = 6;
                    break;
            }
        }
        private void EffectInfo_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = sender as TextBox;

            char[] originalText = tb.Text.ToCharArray();
            foreach (char c in originalText)
            {
                if (!Char.IsNumber(c) && c != '.')
                {
                    tb.Text = tb.Text.Replace(c.ToString(), "");
                    break;
                }
            }

            if (tb.Text == "")
            {
                tb.Text = "0";
            }
        }
        private void EffectInfo_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;

            if (e.Key != VirtualKey.Enter)
                return;

            double number = 0;

            if (double.TryParse(tb.Text, out number))
            {
                if (number > 99999)
                    tb.Text = "99999";
                else
                    tb.Text = number.ToString("0.00");
            }
            else
            {
                return;
            }

            EffectInfo ei = SelectedEffectLine.Info;
            ei.Waves[0].Min = double.Parse(MinTextBox.Text);
            ei.Waves[0].Max = double.Parse(MaxTextBox.Text);
            ei.Waves[0].WaveLength = double.Parse(WaveLenTextBox.Text);
            ei.Waves[0].Freq = double.Parse(FreqTextBox.Text);
            ei.Waves[0].Phase = double.Parse(PhaseTextBox.Text);
            ei.Waves[0].Start = double.Parse(StartTextBox.Text);
            ei.Waves[0].Start = double.Parse(VelocityTextBox.Text);
        }
        private void EffectInfo_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;

            double number = 0;

            if (double.TryParse(tb.Text, out number))
            {
                if (number > 99999)
                    tb.Text = "99999";
                else
                    tb.Text = number.ToString("0.00");
            }
            else
            {
                return;
            }

            EffectInfo ei = SelectedEffectLine.Info;
            ei.Waves[0].Min = double.Parse(MinTextBox.Text);
            ei.Waves[0].Max = double.Parse(MaxTextBox.Text);
            ei.Waves[0].WaveLength = double.Parse(WaveLenTextBox.Text);
            ei.Waves[0].Freq = double.Parse(FreqTextBox.Text);
            ei.Waves[0].Phase = double.Parse(PhaseTextBox.Text);
            ei.Waves[0].Start = double.Parse(StartTextBox.Text);
            ei.Waves[0].Velocity = double.Parse(VelocityTextBox.Text);
        }

        private async void ColorRadioBtn_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Color newColor = await OpenColorPickerWindow(((SolidColorBrush)RadioButtonBg.Background).Color);
            SelectedEffectLine.UInfo.InitColor = newColor;
            RadioButtonBg.Background = new SolidColorBrush(newColor);
        }
        public async Task<Color> OpenColorPickerWindow(Color c)
        {
            ColorPickerDialog colorPickerDialog = new ColorPickerDialog(c);
            await colorPickerDialog.ShowAsync();

            return colorPickerDialog.CurrentColor;
        }
        private void RandomCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (RandomCheckBox.IsChecked == true)
            {
                SelectedEffectLine.UInfo.Random = true;
                RadioButtonBg.Background = new SolidColorBrush(Colors.Gray);
                RadioButtonBg.IsEnabled = false;
            }
            else
            {
                SelectedEffectLine.UInfo.Random = false;
                RadioButtonBg.Background = new SolidColorBrush(SelectedEffectLine.UInfo.InitColor);
                RadioButtonBg.IsEnabled = true;
            }
        }
        private void BrightnessValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            UIInfo ui = SelectedEffectLine.UInfo;
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
                ui.Brightness = (int)slider.Value;
            }
        }
        private void SpeedValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            UIInfo ui = SelectedEffectLine.UInfo;
            Slider slider = sender as Slider;
            if (slider != null)
            {
                ui.Speed = (int)slider.Value;
            }
        }
        private void AngleBgImg_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _angleImgPressing = true;
            Point currentLocation = e.GetCurrentPoint(AngleGrid).Position;

            double dx = currentLocation.X - AngleImgCenter.X;
            double dy = currentLocation.Y - AngleImgCenter.Y;
            double hue = Math2.ComputeH(dx, dy);
            AngleTextBox.Text = hue.ToString("F0");
            SelectedEffectLine.UInfo.Angle = Convert.ToDouble(AngleTextBox.Text);
            AngleStoryboardStart(hue);

        }
        private void AngleBgImg_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            _angleImgPressing = false;
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
        private void AngleTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox AngleChangeText = sender as TextBox;

            char[] originalText = AngleChangeText.Text.ToCharArray();
            if (AngleChangeText.Text.Length > 0)
            {
                foreach (char c in originalText)
                {
                    if (!(Char.IsNumber(c)))
                    {
                        AngleChangeText.Text = AngleChangeText.Text.Replace(Convert.ToString(c), "");
                        break;
                    }
                }
            }

            if ((AngleChangeText.Text != ""))
            {
                if (double.Parse(AngleChangeText.Text) > 360)
                {
                    AngleChangeText.Text = "360";
                }
                else if (double.Parse(AngleChangeText.Text) < 0)
                {
                    AngleChangeText.Text = "0";
                }
            }
        }
        private void AngleStoryboardStart(double AngleImgTargetAngle)
        {
            _angleImgPressing = true;

            if (_angleImgPressing)
            {
                int runTime = 300;
                var storyboard = new Storyboard();
                var angleIcAnimation = new DoubleAnimation();

                if (_preAngle != AngleImgTargetAngle)
                {
                    AngleImgTargetAngle = AngleImgTargetAngle % 360;
                    if (_preAngle - AngleImgTargetAngle > 180)
                    {
                        _preAngle -= 360;
                    }
                    else if (_preAngle - AngleImgTargetAngle < -180)
                    {
                        _preAngle += 360;
                    }

                    // triangle
                    angleIcAnimation.Duration = TimeSpan.FromMilliseconds(runTime);
                    angleIcAnimation.EnableDependentAnimation = true;
                    angleIcAnimation.From = _preAngle;
                    angleIcAnimation.To = AngleImgTargetAngle;
                    Storyboard.SetTargetProperty(angleIcAnimation, "Angle");
                    Storyboard.SetTarget(angleIcAnimation, AngleIcImgRotation);
                    storyboard.Children.Add(angleIcAnimation);
                    storyboard.Begin();

                    _preAngle = AngleImgTargetAngle;
                }
            }
        }
        private void IncreaseBtn_Click(object sender, RoutedEventArgs e)
        {
            UIInfo ui = SelectedEffectLine.UInfo;
            if ((AngleTextBox.Text != ""))
            {
                double textIncrease = Convert.ToDouble(AngleTextBox.Text);
                textIncrease += 5;
                if (textIncrease > 360)
                {
                    textIncrease = 360;
                    AngleTextBox.Text = textIncrease.ToString();
                    AngleStoryboardStart(textIncrease);
                }
                else
                {
                    AngleTextBox.Text = textIncrease.ToString();
                    AngleStoryboardStart(textIncrease);
                }
                ui.Angle = Convert.ToDouble(AngleTextBox.Text);
            }
        }
        private void DecreaseBtn_Click(object sender, RoutedEventArgs e)
        {
            UIInfo ui = SelectedEffectLine.UInfo;
            if ((AngleTextBox.Text != ""))
            {
                double textdecrease = Convert.ToDouble(AngleTextBox.Text);
                textdecrease -= 5;
                if (textdecrease < 0)
                {
                    textdecrease = 0;
                    AngleTextBox.Text = textdecrease.ToString();
                    AngleStoryboardStart(textdecrease);
                }
                else
                {
                    AngleTextBox.Text = textdecrease.ToString();
                    AngleStoryboardStart(textdecrease);
                }
                ui.Angle = textdecrease;
            }
        }
        private void DirectionBtn_Tapped(object sender, TappedRoutedEventArgs e)
        {
            UIInfo ui = SelectedEffectLine.UInfo;
            RadioButton directionBtn = sender as RadioButton;
            switch (directionBtn.Name)
            {
                case "Left":
                    ui.Direction = 1;
                    break;
                case "Right":
                    ui.Direction = 2;
                    break;
                case "Up":
                    ui.Direction = 3;
                    break;
                case "Down":
                    ui.Direction = 4;
                    break;
            }
        }
        private void ResetBtn_Tapped(object sender, TappedRoutedEventArgs e)
        {
            UIInfo ui = SelectedEffectLine.UInfo;
            ui.InitColor = Colors.Red;
            ui.Brightness = 3;
            ui.Speed = 1;
            ui.Direction = 2;
            ui.Angle = 90;
            ui.Random = false;
            ui.High = 60;
            ui.Low = 30;
            UpdateUIEffectContents(ui);
        }
    }
}
