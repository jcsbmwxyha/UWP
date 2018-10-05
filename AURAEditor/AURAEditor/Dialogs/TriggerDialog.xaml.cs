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
using System.Threading.Tasks;

// 內容對話方塊項目範本已記錄在 https://go.microsoft.com/fwlink/?LinkId=234238

namespace AuraEditor.Dialogs
{
    public sealed partial class TriggerDialog : ContentDialog
    {
        DeviceLayer m_DeviceLayer;
        ObservableCollection<TriggerEffect> m_EffectList;
        internal FlyoutBase m_flyoutBase;

        bool _canChange = false;

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
            FillOutUIParameter(m_EffectList[index].Info);
        }
        private void FillOutUIParameter(EffectInfo effectInfo)
        {
            string effName = GetEffectName(effectInfo.Type);
            if (effName == "Ripple")
                EffectComboBox.SelectedIndex = 0;
            else if (effName == "Reactive")
                EffectComboBox.SelectedIndex = 1;
            else if (effName == "Laser")
                EffectComboBox.SelectedIndex = 2;

            ColorRect.Fill = new SolidColorBrush(effectInfo.InitColor);
            RadioButtonBg.Background = new SolidColorBrush(effectInfo.InitColor);
            RandomCheckBox.IsChecked = effectInfo.Random;
            SpeedSlider.Value = effectInfo.Speed;
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

            if (_canChange == true)
            {
                m_EffectList[SelectedIndex].ChangeType(type);
                FillOutUIParameter(m_EffectList[SelectedIndex].Info);
            }

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

        private async void ColorRadioBtn_Tapped(object sender, TappedRoutedEventArgs e)
        {
            EffectInfo Info = m_EffectList[SelectedIndex].Info;
            Color newColor = await OpenColorPickerWindow(((SolidColorBrush)RadioButtonBg.Background).Color);
            Info.InitColor = newColor;
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
            EffectInfo info = m_EffectList[SelectedIndex].Info;
            if (RandomCheckBox.IsChecked == true)
            {
                info.Random = true;
                RadioButtonBg.Background = new SolidColorBrush(Colors.Gray);
                RadioButtonBg.IsEnabled = false;
            }
            else
            {
                info.Random = false;
                RadioButtonBg.Background = new SolidColorBrush(info.InitColor);
                RadioButtonBg.IsEnabled = true;
            }
        }

        private void SpeedValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            EffectInfo info = m_EffectList[SelectedIndex].Info;
            Slider slider = sender as Slider;
            if (slider != null)
            {
                info.Speed = (int)slider.Value;
            }
        }

        private void EffectInfoStackPanel_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            _canChange = true;
        }

        private void StackPanel_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            _canChange = false;
        }
    }
}
