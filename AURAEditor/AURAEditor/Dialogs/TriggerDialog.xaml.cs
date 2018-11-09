using AuraEditor.UserControls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using static AuraEditor.Common.EffectHelper;
using static AuraEditor.Common.ControlHelper;
using static AuraEditor.Common.Math2;
using System.Threading.Tasks;

// 內容對話方塊項目範本已記錄在 https://go.microsoft.com/fwlink/?LinkId=234238

namespace AuraEditor.Dialogs
{
    public sealed partial class TriggerDialog : ContentDialog
    {
        private Layer m_DeviceLayer;
        private ObservableCollection<TriggerEffect> m_EffectList;
        private string defaultEffect;

        public List<ColorPoint> ColorPoints = new List<ColorPoint>();
        public List<List<ColorPoint>> DefaultColorList = new List<List<ColorPoint>>();
        public List<ColorPoint> CustomizeColorPoints = new List<ColorPoint>();

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

        public TriggerDialog(Layer layer)
        {
            this.InitializeComponent();
            SetDefaultPattern();

            m_DeviceLayer = layer;
            TriggerActionButton.Content = m_DeviceLayer.TriggerAction;

            m_EffectList = new ObservableCollection<TriggerEffect>();

            if (m_DeviceLayer.TriggerEffects != null)
            {
                foreach (var e in m_DeviceLayer.TriggerEffects)
                {
                    m_EffectList.Add(e);
                }
            }

            TriggerEffectListView.ItemsSource = m_EffectList;
            defaultEffect = GetTriggerEffect()[0];

            foreach (var effectName in GetTriggerEffect())
            {
                MenuFlyoutItem mfi = new MenuFlyoutItem();
                mfi.Text = effectName;
                mfi.Style = (Style)Application.Current.Resources["RogMenuFlyoutItemStyle1"];
                mfi.Click += EffectSelected;
                EffectSelectionMenuFlyout.Items.Add(mfi);
            }
        }

        private void EffectSelected(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuFlyoutItem;
            string selectedName = item.Text;

            if (selectedName == EffectSelectionButton.Content as string)
                return;

            int type = GetEffectIndex(selectedName);
            m_EffectList[SelectedIndex].ChangeType(type);
            FillOutUIParameter(m_EffectList[SelectedIndex].Info);
            ShowUIGroups(m_EffectList[SelectedIndex].Info);

            ListViewItem lvi = TriggerEffectListView.ContainerFromIndex(SelectedIndex) as ListViewItem;
            TriggerBlock tb = FindAllControl<TriggerBlock>(lvi, typeof(TriggerBlock))[0];
            tb.Update();
        }

        private void AddEffectButton_Click(object sender, RoutedEventArgs e)
        {
            TriggerEffect effect = new TriggerEffect(m_DeviceLayer, defaultEffect);
            m_EffectList.Add(effect);
        }
        private void FillOutParameterByIndex(int index)
        {
            FillOutUIParameter(m_EffectList[index].Info);
            ShowUIGroups(m_EffectList[index].Info);
        }
        private void FillOutUIParameter(EffectInfo effectInfo)
        {
            string effName = GetEffectName(effectInfo.Type);
            EffectSelectionButton.Content = effName;
            
            RadioButtonBg.Background = new SolidColorBrush(effectInfo.InitColor);
            RandomCheckBox.IsChecked = effectInfo.Random;
            SpeedSlider.Value = effectInfo.Speed;
        }

        private void ShowUIGroups(EffectInfo effectInfo)
        {
            string effName = GetEffectName(effectInfo.Type);
            if (effName == "Ripple")
            {
                ColorGroup.Visibility = Visibility.Visible;
                //PatternGroup.Visibility = Visibility.Visible;
                SpeedGroup.Visibility = Visibility.Visible;
            }
            else if (effName == "Reactive")
            {
                ColorGroup.Visibility = Visibility.Visible;
                //PatternGroup.Visibility = Visibility.Collapsed;
                SpeedGroup.Visibility = Visibility.Visible;
            }
            else if (effName == "Laser")
            {
                ColorGroup.Visibility = Visibility.Visible;
                //PatternGroup.Visibility = Visibility.Collapsed;
                SpeedGroup.Visibility = Visibility.Visible;
            }
        }

        private void TriggerEffectListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView lv = sender as ListView;
            SelectedIndex = lv.SelectedIndex;
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
            m_DeviceLayer.TriggerEffects = m_EffectList.ToList();
            this.Hide();
        }

        private async void ColorRadioBtn_Tapped(object sender, TappedRoutedEventArgs e)
        {
            EffectInfo Info = m_EffectList[SelectedIndex].Info;
            this.Hide();
            Color newColor = await OpenColorPickerWindow(((SolidColorBrush)RadioButtonBg.Background).Color);
            Info.InitColor = newColor;
            RadioButtonBg.Background = new SolidColorBrush(newColor);
        }
        public async Task<Color> OpenColorPickerWindow(Color c)
        {
            m_DeviceLayer.TriggerEffects = m_EffectList.ToList();
            ColorPickerDialog colorPickerDialog = new ColorPickerDialog(c, m_DeviceLayer);
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

        private void DefaultRainbow_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem mf = sender as MenuFlyoutItem;
            TriggerPatternCanvas.Children.Clear();
            foreach (var item in ColorPoints)
            {
                item.UI.OnRedraw -= ReDrawMultiPointRectangle;
            }
            ColorPoints.Clear();

            foreach (var item in DefaultColorList[(int)Char.GetNumericValue(mf.Name[mf.Name.Length - 1]) - 1])
            {
                ColorPoints.Add(new ColorPoint(item));
            }
            ShowColorPointUI(ColorPoints);
            MultiPointRectangle.Fill = PatternButton.Background = mf.Foreground;
        }

        private void CustomizeRainbow_Click(object sender, RoutedEventArgs e)
        {
            TriggerPatternCanvas.Children.Clear();
            foreach (var item in ColorPoints)
            {
                item.UI.OnRedraw -= ReDrawMultiPointRectangle;
            }
            ColorPoints.Clear();

            foreach (var item in CustomizeColorPoints)
            {
                ColorPoints.Add(new ColorPoint(item));
            }
            ShowColorPointUI(ColorPoints);
            MultiPointRectangle.Fill = PatternButton.Background = CustomizeRainbow.Foreground;
        }

        public void ShowColorPointUI(List<ColorPoint> cl)
        {
            for (int i = 0; i < cl.Count; i++)
            {
                cl[i].UI.OnRedraw += ReDrawMultiPointRectangle;
                TriggerPatternCanvas.Children.Add(cl[i].UI);
            }
        }

        private void PlusItemBt(object sender, RoutedEventArgs e)
        {
            ColorPoint newColorPointBt = new ColorPoint();
            AddColorPoint(newColorPointBt);
            newColorPointBt.UI.OnRedraw += ReDrawMultiPointRectangle;
            ReDrawMultiPointRectangle();
        }

        public void AddColorPoint(ColorPoint colorPoint)
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
                        TriggerPatternCanvas.Children.Add(colorPoint.UI);
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

        public void RemoveColorPoint()
        {
            if (ColorPoints.Count > 2)
            {
                foreach (var item in ColorPoints)
                {
                    List<RadioButton> items = FindAllControl<RadioButton>(item.UI, typeof(RadioButton));

                    if (items[0].IsChecked == true)
                    {
                        item.UI.OnRedraw -= ReDrawMultiPointRectangle;
                        ColorPoints.Remove(item);
                        TriggerPatternCanvas.Children.Remove(item.UI);
                        break;
                    }
                }
            }
        }

        private void SetDefaultPattern()
        {
            DefaultColorList = MainPage.Self.CallDefaultList();
            CustomizeRainbow.Foreground = new SolidColorBrush(Colors.White);

            //Button Color
            LinearGradientBrush Pattern1 = new LinearGradientBrush();
            Pattern1.StartPoint = new Point(0, 0.5);
            Pattern1.EndPoint = new Point(1, 0.5);
            for (int i = 0; i < DefaultColorList[0].Count; i++)
            {
                Pattern1.GradientStops.Add(new GradientStop { Color = DefaultColorList[0][i].Color, Offset = DefaultColorList[0][i].Offset });
            }

            // Use the brush to paint the rectangle.
            PatternButton.Background = MultiPointRectangle.Fill = DefaultRainbow1.Foreground = Pattern1;
            foreach (var item in DefaultColorList[0])
            {
                ColorPoints.Add(new ColorPoint(item));
            }

            ShowColorPointUI(ColorPoints);

            // Button Color  
            LinearGradientBrush Pattern2 = new LinearGradientBrush();
            Pattern2.StartPoint = new Point(0, 0.5);
            Pattern2.EndPoint = new Point(1, 0.5);
            for (int i = 0; i < DefaultColorList[1].Count; i++)
            {
                Pattern2.GradientStops.Add(new GradientStop { Color = DefaultColorList[1][i].Color, Offset = DefaultColorList[1][i].Offset });
            }

            // Use the brush to paint the rectangle.
            DefaultRainbow2.Foreground = Pattern2;

            // Button Color  
            LinearGradientBrush Pattern3 = new LinearGradientBrush();
            Pattern3.StartPoint = new Point(0, 0.5);
            Pattern3.EndPoint = new Point(1, 0.5);
            for (int i = 0; i < DefaultColorList[2].Count; i++)
            {
                Pattern3.GradientStops.Add(new GradientStop { Color = DefaultColorList[2][i].Color, Offset = DefaultColorList[2][i].Offset });
            }

            // Use the brush to paint the rectangle.
            DefaultRainbow3.Foreground = Pattern3;

            // Button Color  
            LinearGradientBrush Pattern4 = new LinearGradientBrush();
            Pattern4.StartPoint = new Point(0, 0.5);
            Pattern4.EndPoint = new Point(1, 0.5);
            for (int i = 0; i < DefaultColorList[3].Count; i++)
            {
                Pattern4.GradientStops.Add(new GradientStop { Color = DefaultColorList[3][i].Color, Offset = DefaultColorList[3][i].Offset });
            }

            // Use the brush to paint the rectangle.
            DefaultRainbow4.Foreground = Pattern4;

            // Button Color  
            LinearGradientBrush Pattern5 = new LinearGradientBrush();
            Pattern5.StartPoint = new Point(0, 0.5);
            Pattern5.EndPoint = new Point(1, 0.5);
            for (int i = 0; i < DefaultColorList[4].Count; i++)
            {
                Pattern5.GradientStops.Add(new GradientStop { Color = DefaultColorList[4][i].Color, Offset = DefaultColorList[4][i].Offset });
            }

            // Use the brush to paint the rectangle.
            DefaultRainbow5.Foreground = Pattern5;

            // Button Color  
            LinearGradientBrush Pattern6 = new LinearGradientBrush();
            Pattern6.StartPoint = new Point(0, 0.5);
            Pattern6.EndPoint = new Point(1, 0.5);
            for (int i = 0; i < DefaultColorList[5].Count; i++)
            {
                Pattern6.GradientStops.Add(new GradientStop { Color = DefaultColorList[5][i].Color, Offset = DefaultColorList[5][i].Offset });
            }

            // Use the brush to paint the rectangle.
            DefaultRainbow6.Foreground = Pattern6;
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

            PatternButton.Background = CustomizeRainbow.Foreground = MultiPointRectangle.Fill = Pattern;
            CustomizeColorPoints = new List<ColorPoint>(ColorPoints);
            MainPage.Self.SetListBorder(ColorPoints);
        }

        private void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuFlyoutItem;
            string selectedAction = item.Text;
            TriggerActionButton.Content = selectedAction;
            m_DeviceLayer.TriggerAction = selectedAction;
        }
    }
}
