using AuraEditor.Common;
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
        internal Effect _selectedEffectLine;

        public void UpdateEffectInfoGrid(Effect effect)
        {
            _selectedEffectLine = effect;
            Border border = effect.UIBorder;
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

            switch (effectType)
            {
                case 0:
                    ColorRect.Visibility = Visibility.Visible;
                    TempoGroup.Visibility = Visibility.Collapsed;
                    DirectionGroup.Visibility = Visibility.Collapsed;
                    break;
                case 1:
                    ColorRect.Visibility = Visibility.Visible;
                    TempoGroup.Visibility = Visibility.Visible;
                    DirectionGroup.Visibility = Visibility.Collapsed;
                    break;
                case 2:
                    ColorRect.Visibility = Visibility.Collapsed;
                    TempoGroup.Visibility = Visibility.Visible;
                    DirectionGroup.Visibility = Visibility.Collapsed;
                    break;
                case 3:
                    ColorRect.Visibility = Visibility.Collapsed;
                    TempoGroup.Visibility = Visibility.Visible;
                    DirectionGroup.Visibility = Visibility.Visible;
                    break;
                case 4:
                    ColorRect.Visibility = Visibility.Visible;
                    TempoGroup.Visibility = Visibility.Collapsed;
                    DirectionGroup.Visibility = Visibility.Collapsed;
                    break;
                case 5:
                    ColorRect.Visibility = Visibility.Collapsed;
                    TempoGroup.Visibility = Visibility.Collapsed;
                    DirectionGroup.Visibility = Visibility.Collapsed;
                    break;
                case 6:
                    ColorRect.Visibility = Visibility.Collapsed;
                    TempoGroup.Visibility = Visibility.Collapsed;
                    DirectionGroup.Visibility = Visibility.Collapsed;
                    break;
                default:
                    ColorRect.Visibility = Visibility.Collapsed;
                    TempoGroup.Visibility = Visibility.Collapsed;
                    DirectionGroup.Visibility = Visibility.Collapsed;
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
        }

        private void ColorPickerOk_Click(object sender, RoutedEventArgs e)
        {
            Border border = _selectedEffectLine.UIBorder;
            Color resultColor = ColorPicker.Color;
            SolidColorBrush scb = new SolidColorBrush(resultColor);

            _selectedEffectLine.Info.Color = resultColor;
            ColorRect.Fill = scb;
            border.Background = scb;
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
            if (_selectedEffectLine == null)
                return;

            EffectInfo ei = _selectedEffectLine.Info;

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
            
            EffectInfo ei = _selectedEffectLine.Info;
            ei.Min = double.Parse(MinTextBox.Text);
            ei.Max = double.Parse(MaxTextBox.Text);
            ei.WaveLength = double.Parse(WaveLenTextBox.Text);
            ei.Freq = double.Parse(FreqTextBox.Text);
            ei.Phase = double.Parse(PhaseTextBox.Text);
            ei.Start = double.Parse(StartTextBox.Text);

            //switch (tb.Name)
            //{
            //    case "MinTextBox":
            //        ei.Min = double.Parse(tb.Text);
            //        break;
            //    case "MaxTextBox":
            //        ei.Max = double.Parse(tb.Text);
            //        break;
            //    case "WaveLenTextBox":
            //        ei.WaveLength = double.Parse(tb.Text);
            //        break;
            //    case "FreqTextBox":
            //        ei.Freq = double.Parse(tb.Text);
            //        break;
            //    case "PhaseTextBox":
            //        ei.Phase = double.Parse(tb.Text);
            //        break;
            //    case "StartTextBox":
            //        ei.Start = double.Parse(tb.Text);
            //        break;
            //}
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

            EffectInfo ei = _selectedEffectLine.Info;
            ei.Min = double.Parse(MinTextBox.Text);
            ei.Max = double.Parse(MaxTextBox.Text);
            ei.WaveLength = double.Parse(WaveLenTextBox.Text);
            ei.Freq = double.Parse(FreqTextBox.Text);
            ei.Phase = double.Parse(PhaseTextBox.Text);
            ei.Start = double.Parse(StartTextBox.Text);
        }
    }
}