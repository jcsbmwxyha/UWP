using AuraEditor.UserControls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using static AuraEditor.Common.EffectHelper;
using static AuraEditor.Common.ControlHelper;

// 內容對話方塊項目範本已記錄在 https://go.microsoft.com/fwlink/?LinkId=234238

namespace AuraEditor.Dialogs
{
    public sealed partial class TriggerDialog : ContentDialog
    {
        DeviceLayer m_DeviceLayer;
        ObservableCollection<TriggerEffect> m_EffectList;
        internal FlyoutBase m_flyoutBase;

        private int _selectedIndex = -1;
        public int SelectedIndex
        {
            get
            {
                return _selectedIndex;
            }
            set
            {
                if (_selectedIndex != value)
                {
                    _selectedIndex = value;
                    if (value == -1)
                    {
                        EffectInfoStackPanel.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        EffectInfoStackPanel.Visibility = Visibility.Visible;

                        FillOutParameterByIndex(value);
                    }

                }
            }
        }

        public TriggerDialog(DeviceLayer layer)
        {
            this.InitializeComponent();

            m_DeviceLayer = layer;
            m_EffectList = new ObservableCollection<TriggerEffect>();

            if (m_DeviceLayer.TriggerEffects != null)
            {
                foreach (var e in m_DeviceLayer.TriggerEffects)
                {
                    m_EffectList.Add(e);
                }
            }

            TriggerEffectListView.ItemsSource = m_EffectList;
        }
        private void AddEffectButton_Click(object sender, RoutedEventArgs e)
        {
            if (EffectComboBox.SelectedItem != null)
            {
                string effectName = EffectComboBox.SelectedItem.ToString();
                TriggerEffect effect = new TriggerEffect(m_DeviceLayer, effectName);
                m_EffectList.Add(effect);
            }
        }
        private void FillOutParameterByIndex(int index)
        {
            FillOutParameter(m_EffectList[index].Info);
        }
        private void FillOutParameter(EffectInfo info)
        {
            string effName = GetEffectName(info.Type);
            if (effName == "Ripple")
                EffectComboBox.SelectedIndex = 0;
            else if (effName == "Reactive")
                EffectComboBox.SelectedIndex = 1;
            if (effName == "Laser")
                EffectComboBox.SelectedIndex = 2;

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

        private void TriggerEffectListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView lv = sender as ListView;
            SelectedIndex = lv.SelectedIndex;
        }
        private void EffectComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string name = EffectComboBox.SelectedItem as string;
            int type = GetEffectIndex(name);

            if (SelectedIndex == -1)
            {
                // Initialization has not been completed
                return;
            }

            m_EffectList[SelectedIndex].ChangeType(type);
            FillOutParameter(m_EffectList[SelectedIndex].Info);

            // TODO : Use MVVM to update content
            ListViewItem item = TriggerEffectListView.ContainerFromIndex(SelectedIndex) as ListViewItem;
            TriggerBlock tb = FindAllControl<TriggerBlock>(item, typeof(TriggerBlock))[0];
            tb.Update();
        }

        private void ColorRect_Pressed(object sender, PointerRoutedEventArgs e)
        {
            m_flyoutBase = Flyout.GetAttachedFlyout(sender as Rectangle);
            Flyout.ShowAttachedFlyout(sender as Rectangle);
        }
        private void ColorPickerOk_Click(object sender, RoutedEventArgs e)
        {
            EffectInfo info = m_EffectList[SelectedIndex].Info;
            Color resultColor = ColorPicker.Color;
            SolidColorBrush scb = new SolidColorBrush(resultColor);

            info.InitColor = resultColor;
            ColorRect.Fill = scb;
            m_flyoutBase.Hide();
        }
        private void ColorPickerCancel_Click(object sender, RoutedEventArgs e)
        {
            m_flyoutBase.Hide();
        }
        private void WaveTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EffectInfo ei = m_EffectList[SelectedIndex].Info;
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

            EffectInfo ei = m_EffectList[SelectedIndex].Info;
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

            EffectInfo ei = m_EffectList[SelectedIndex].Info;
            ei.Waves[0].Min = double.Parse(MinTextBox.Text);
            ei.Waves[0].Max = double.Parse(MaxTextBox.Text);
            ei.Waves[0].WaveLength = double.Parse(WaveLenTextBox.Text);
            ei.Waves[0].Freq = double.Parse(FreqTextBox.Text);
            ei.Waves[0].Phase = double.Parse(PhaseTextBox.Text);
            ei.Waves[0].Start = double.Parse(StartTextBox.Text);
            ei.Waves[0].Velocity = double.Parse(VelocityTextBox.Text);
        }

        public void DeleteTriggerEffect(TriggerEffect eff)
        {
            if (m_EffectList.IndexOf(eff) == SelectedIndex)
            {
                SelectedIndex = -1;
            }

            m_EffectList.Remove(eff);
        }
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_EffectList.Count; i++)
            {
                m_EffectList[i].StartTime = i * 1000;
            }

            m_DeviceLayer.TriggerEffects = m_EffectList.ToList();
            this.Hide();
        }
    }
}
