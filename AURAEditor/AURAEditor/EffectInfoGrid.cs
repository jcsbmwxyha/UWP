using AuraEditor.Common;
using AuraEditor.UserControls;
using System;
using Windows.System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace AuraEditor
{
    public sealed partial class MainPage : Page
    {
        internal FlyoutBase m_flyoutBase;

        private Effect _selectedEffectLine;
        public Effect SelectedEffectLine
        {
            get
            {
                return _selectedEffectLine;
            }
            set
            {
                if (value == null)
                    ClearEffectInfoGrid();
                else
                    UpdateEffectInfoGrid(value);

                _selectedEffectLine = value;
            }
        }

        public void ClearEffectInfoGrid()
        {
            Title.Text = "";
            ColorRect.Visibility = Visibility.Collapsed;
            EffectInfoGroup.Visibility = Visibility.Collapsed;
        }
        public void UpdateEffectInfoGrid(Effect effect)
        {
            EffectLine border = effect.UI;
            ShowEffectGroups(effect.EffectType);
            UpdateEffectGroups(effect.Info);
            
            if (border.Background is SolidColorBrush)
            {
                ColorRect.Fill = (SolidColorBrush)border.Background;
            }
        }

        private void ShowEffectGroups(int effectType)
        {
            EffectInfoGroup.Visibility = Visibility.Visible;
            Title.Text = EffectHelper.GetEffectName(effectType);

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
                    break;
            }
            TempoGroup.Visibility = Visibility.Collapsed;
            DirectionGroup.Visibility = Visibility.Collapsed;
        }

        private void UpdateEffectGroups(EffectInfo info)
        {
            WaveTypeComboBox.SelectedIndex = info.WaveType;
            MinTextBox.Text = info.Min.ToString();
            MaxTextBox.Text = info.Max.ToString();
            WaveLenTextBox.Text = info.WaveLength.ToString();
            FreqTextBox.Text = info.Freq.ToString();
            PhaseTextBox.Text = info.Phase.ToString();
            StartTextBox.Text = info.Start.ToString();
            VelocityTextBox.Text = info.Start.ToString();
        }

        private void ColorPickerOk_Click(object sender, RoutedEventArgs e)
        {
            EffectLine effectLineUI = SelectedEffectLine.UI;
            Color resultColor = ColorPicker.Color;
            SolidColorBrush scb = new SolidColorBrush(resultColor);

            SelectedEffectLine.Info.Color = resultColor;
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

        private void TempoRadioButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DirectionRadioButton_Click(object sender, RoutedEventArgs e)
        {

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
                    ei.WaveType = 0;
                    break;
                case "HalfSineWave":
                    ei.WaveType = 1;
                    break;
                case "QuarterSineWave":
                    ei.WaveType = 2;
                    break;
                case "SquareWave":
                    ei.WaveType = 3;
                    break;
                case "TriangleWave":
                    ei.WaveType = 4;
                    break;
                case "SawToothleWave":
                    ei.WaveType = 5;
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
            ei.Min = double.Parse(MinTextBox.Text);
            ei.Max = double.Parse(MaxTextBox.Text);
            ei.WaveLength = double.Parse(WaveLenTextBox.Text);
            ei.Freq = double.Parse(FreqTextBox.Text);
            ei.Phase = double.Parse(PhaseTextBox.Text);
            ei.Start = double.Parse(StartTextBox.Text);
            ei.Start = double.Parse(VelocityTextBox.Text);
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
            ei.Min = double.Parse(MinTextBox.Text);
            ei.Max = double.Parse(MaxTextBox.Text);
            ei.WaveLength = double.Parse(WaveLenTextBox.Text);
            ei.Freq = double.Parse(FreqTextBox.Text);
            ei.Phase = double.Parse(PhaseTextBox.Text);
            ei.Start = double.Parse(StartTextBox.Text);
            ei.Velocity = double.Parse(VelocityTextBox.Text);
        }
    }
}