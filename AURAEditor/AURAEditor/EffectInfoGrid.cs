﻿using AuraEditor.Common;
using AuraEditor.Dialogs;
using AuraEditor.UserControls;
using System;
using Windows.Foundation;
using Windows.System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Shapes;
using static AuraEditor.Common.EffectHelper;
using static AuraEditor.Common.ControlHelper;
using System.Threading.Tasks;
using System.Collections.Generic;
using Windows.UI.Xaml.Media.Imaging;

namespace AuraEditor
{
    public sealed partial class MainPage : Page
    {
        public List<ColorPoint> ColorPoints = new List<ColorPoint>();
        
        public TimelineEffect SelectedEffect
        {
            get
            {
                return LayerManager.CheckedEffect;
            }
            set
            {
                LayerManager.CheckedEffect = value;
            }
        }

        public void ClearEffectInfoGrid()
        {
            Title.Text = "";
            ResetBtn.Visibility = Visibility.Collapsed;
            EffectInfoGroup.Visibility = Visibility.Collapsed;
        }
        public void UpdateEffectInfoGrid(TimelineEffect effect)
        {
            ShowEffectInfoGroupsByType(effect.Type);
            UpdateUIEffectContents(effect.Info);
        }

        private void ShowEffectInfoGroupsByType(int effectType)
        {
            ResetBtn.Visibility = Visibility.Visible;
            EffectInfoGroup.Visibility = Visibility.Visible;
            Title.Text = GetEffectName(effectType);

            switch (Title.Text)
            {
                case "Static":
                    ColorGroup.Visibility = Visibility.Visible;
                    RandomCheckBox.Visibility = Visibility.Collapsed;
                    PatternGroup.Visibility = Visibility.Collapsed;
                    BrightnessGroup.Visibility = Visibility.Collapsed;
                    SpeedGroup.Visibility = Visibility.Collapsed;
                    DirectionGroup.Visibility = Visibility.Collapsed;
                    AngleGroup.Visibility = Visibility.Collapsed;
                    TemperatureGroup.Visibility = Visibility.Collapsed;
                    break;
                case "Breath":
                    ColorGroup.Visibility = Visibility.Visible;
                    RandomCheckBox.Visibility = Visibility.Collapsed;
                    PatternGroup.Visibility = Visibility.Collapsed;
                    BrightnessGroup.Visibility = Visibility.Collapsed;
                    SpeedGroup.Visibility = Visibility.Visible;
                    DirectionGroup.Visibility = Visibility.Collapsed;
                    AngleGroup.Visibility = Visibility.Collapsed;
                    TemperatureGroup.Visibility = Visibility.Collapsed;
                    break;
                case "ColorCycle":
                    ColorGroup.Visibility = Visibility.Collapsed;
                    RandomCheckBox.Visibility = Visibility.Collapsed;
                    PatternGroup.Visibility = Visibility.Collapsed;
                    BrightnessGroup.Visibility = Visibility.Collapsed;
                    SpeedGroup.Visibility = Visibility.Visible;
                    DirectionGroup.Visibility = Visibility.Collapsed;
                    AngleGroup.Visibility = Visibility.Collapsed;
                    TemperatureGroup.Visibility = Visibility.Collapsed;
                    break;
                case "Rainbow":
                    ColorGroup.Visibility = Visibility.Collapsed;
                    RandomCheckBox.Visibility = Visibility.Collapsed;
                    PatternGroup.Visibility = Visibility.Visible;
                    BrightnessGroup.Visibility = Visibility.Collapsed;
                    SpeedGroup.Visibility = Visibility.Visible;
                    DirectionGroup.Visibility = Visibility.Collapsed;
                    AngleGroup.Visibility = Visibility.Visible;
                    TemperatureGroup.Visibility = Visibility.Collapsed;
                    break;
                case "Strobing":
                    ColorGroup.Visibility = Visibility.Visible;
                    RandomCheckBox.Visibility = Visibility.Collapsed;
                    PatternGroup.Visibility = Visibility.Collapsed;
                    BrightnessGroup.Visibility = Visibility.Collapsed;
                    SpeedGroup.Visibility = Visibility.Visible;
                    DirectionGroup.Visibility = Visibility.Collapsed;
                    AngleGroup.Visibility = Visibility.Collapsed;
                    TemperatureGroup.Visibility = Visibility.Collapsed;
                    break;
                case "Comet":
                    ColorGroup.Visibility = Visibility.Visible;
                    RandomCheckBox.Visibility = Visibility.Visible;
                    PatternGroup.Visibility = Visibility.Collapsed;
                    BrightnessGroup.Visibility = Visibility.Collapsed;
                    SpeedGroup.Visibility = Visibility.Visible;
                    DirectionGroup.Visibility = Visibility.Collapsed;
                    AngleGroup.Visibility = Visibility.Visible;
                    TemperatureGroup.Visibility = Visibility.Collapsed;
                    break;
                case "Star":
                    ColorGroup.Visibility = Visibility.Visible;
                    RandomCheckBox.Visibility = Visibility.Visible;
                    PatternGroup.Visibility = Visibility.Collapsed;
                    BrightnessGroup.Visibility = Visibility.Collapsed;
                    SpeedGroup.Visibility = Visibility.Visible;
                    DirectionGroup.Visibility = Visibility.Collapsed;
                    AngleGroup.Visibility = Visibility.Collapsed;
                    TemperatureGroup.Visibility = Visibility.Collapsed;
                    break;
                case "Tide":
                    ColorGroup.Visibility = Visibility.Visible;
                    RandomCheckBox.Visibility = Visibility.Collapsed;
                    PatternGroup.Visibility = Visibility.Collapsed;
                    BrightnessGroup.Visibility = Visibility.Collapsed;
                    SpeedGroup.Visibility = Visibility.Visible;
                    DirectionGroup.Visibility = Visibility.Collapsed;
                    AngleGroup.Visibility = Visibility.Visible;
                    TemperatureGroup.Visibility = Visibility.Collapsed;
                    break;
                case "Raidus":
                    ColorGroup.Visibility = Visibility.Collapsed;
                    RandomCheckBox.Visibility = Visibility.Collapsed;
                    PatternGroup.Visibility = Visibility.Collapsed;
                    BrightnessGroup.Visibility = Visibility.Collapsed;
                    SpeedGroup.Visibility = Visibility.Collapsed;
                    DirectionGroup.Visibility = Visibility.Collapsed;
                    AngleGroup.Visibility = Visibility.Collapsed;
                    TemperatureGroup.Visibility = Visibility.Collapsed;
                    break;
                case "Reactive":
                    ColorGroup.Visibility = Visibility.Visible;
                    RandomCheckBox.Visibility = Visibility.Visible;
                    PatternGroup.Visibility = Visibility.Collapsed;
                    BrightnessGroup.Visibility = Visibility.Collapsed;
                    SpeedGroup.Visibility = Visibility.Visible;
                    DirectionGroup.Visibility = Visibility.Collapsed;
                    AngleGroup.Visibility = Visibility.Collapsed;
                    TemperatureGroup.Visibility = Visibility.Collapsed;
                    break;
                case "Laser":
                    ColorGroup.Visibility = Visibility.Visible;
                    RandomCheckBox.Visibility = Visibility.Visible;
                    PatternGroup.Visibility = Visibility.Collapsed;
                    BrightnessGroup.Visibility = Visibility.Collapsed;
                    SpeedGroup.Visibility = Visibility.Visible;
                    DirectionGroup.Visibility = Visibility.Collapsed;
                    AngleGroup.Visibility = Visibility.Collapsed;
                    TemperatureGroup.Visibility = Visibility.Collapsed;
                    break;
                case "Ripple":
                    ColorGroup.Visibility = Visibility.Visible;
                    RandomCheckBox.Visibility = Visibility.Visible;
                    PatternGroup.Visibility = Visibility.Visible;
                    BrightnessGroup.Visibility = Visibility.Collapsed;
                    SpeedGroup.Visibility = Visibility.Visible;
                    DirectionGroup.Visibility = Visibility.Collapsed;
                    AngleGroup.Visibility = Visibility.Collapsed;
                    TemperatureGroup.Visibility = Visibility.Collapsed;
                    break;
                case "Music":
                    ColorGroup.Visibility = Visibility.Collapsed;
                    RandomCheckBox.Visibility = Visibility.Collapsed;
                    PatternGroup.Visibility = Visibility.Collapsed;
                    BrightnessGroup.Visibility = Visibility.Collapsed;
                    SpeedGroup.Visibility = Visibility.Collapsed;
                    DirectionGroup.Visibility = Visibility.Collapsed;
                    AngleGroup.Visibility = Visibility.Collapsed;
                    TemperatureGroup.Visibility = Visibility.Collapsed;
                    break;
                case "Smart":
                    ColorGroup.Visibility = Visibility.Collapsed;
                    RandomCheckBox.Visibility = Visibility.Collapsed;
                    PatternGroup.Visibility = Visibility.Collapsed;
                    BrightnessGroup.Visibility = Visibility.Collapsed;
                    SpeedGroup.Visibility = Visibility.Collapsed;
                    DirectionGroup.Visibility = Visibility.Collapsed;
                    AngleGroup.Visibility = Visibility.Collapsed;
                    TemperatureGroup.Visibility = Visibility.Visible;
                    break;
                default:
                    ColorGroup.Visibility = Visibility.Collapsed;
                    RandomCheckBox.Visibility = Visibility.Collapsed;
                    PatternGroup.Visibility = Visibility.Collapsed;
                    BrightnessGroup.Visibility = Visibility.Collapsed;
                    SpeedGroup.Visibility = Visibility.Collapsed;
                    DirectionGroup.Visibility = Visibility.Collapsed;
                    AngleGroup.Visibility = Visibility.Collapsed;
                    TemperatureGroup.Visibility = Visibility.Collapsed;
                    break;
            }
        }
        private void UpdateUIEffectContents(EffectInfo info)
        {
            RadioButtonBg.Background = new SolidColorBrush(info.InitColor);
            if (info.Random)
            {
                RandomCheckBox.IsChecked = true;
                RadioButtonBg.Opacity = 0.5;
                RadioButtonBg.IsEnabled = false;
            }
            else
            {
                RandomCheckBox.IsChecked = false;
                RadioButtonBg.Opacity = 1;
                RadioButtonBg.IsEnabled = true;
            }
            BrightnessSlider.Value = info.Brightness;
            SpeedSlider.Value = info.Speed;
            switch (info.Direction)
            {
                case 1: Left.IsChecked = true; break;
                case 2: Right.IsChecked = true; break;
                case 3: Up.IsChecked = true; break;
                case 4: Down.IsChecked = true; break;
            }
            AngleStoryboardStart(info.Angle);
            AngleTextBox.Text = info.Angle.ToString();
            PatternCanvas.Children.Clear();
            ColorPoints.Clear();
            foreach (var item in info.ColorPointList)
            {
                ColorPoints.Add(new ColorPoint(item));
            }
            ShowColorPointUI(ColorPoints);
            ReDrawMultiPointRectangle();
            SegmentationSwitch.IsOn = info.ColorSegmentation;
        }

        private async void ColorRadioBtn_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Color newColor = await OpenColorPickerWindow(((SolidColorBrush)RadioButtonBg.Background).Color);
            SelectedEffect.Info.InitColor = newColor;
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
                SelectedEffect.Info.Random = true;
                RadioButtonBg.Opacity = 0.5;
                RadioButtonBg.IsEnabled = false;
            }
            else
            {
                SelectedEffect.Info.Random = false;
                RadioButtonBg.Opacity = 1;
                RadioButtonBg.IsEnabled = true;
            }
        }
        private void BrightnessValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            EffectInfo ui = SelectedEffect.Info;
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
            EffectInfo ui = SelectedEffect.Info;
            Slider slider = sender as Slider;
            if (slider != null)
            {
                if (slider.Value == 1)
                {
                    SlowPoint.Source = new BitmapImage(new Uri(this.BaseUri, "Assets/EffectInfoGroup/asus_gc_slider2 control_n.png"));
                    MediumPoint.Source = new BitmapImage(new Uri(this.BaseUri, "Assets/EffectInfoGroup/asus_gc_slider2 control_n.png"));
                    FastPoint.Source = new BitmapImage(new Uri(this.BaseUri, "Assets/EffectInfoGroup/asus_gc_slider2 control_d.png"));
                }
                else if (slider.Value == 2)
                {
                    SlowPoint.Source = new BitmapImage(new Uri(this.BaseUri, "Assets/EffectInfoGroup/asus_gc_slider2 control_n.png"));
                    MediumPoint.Source = new BitmapImage(new Uri(this.BaseUri, "Assets/EffectInfoGroup/asus_gc_slider2 control_n.png"));
                    FastPoint.Source = new BitmapImage(new Uri(this.BaseUri, "Assets/EffectInfoGroup/asus_gc_slider2 control_n.png"));
                }
                else
                {
                    SlowPoint.Source = new BitmapImage(new Uri(this.BaseUri, "Assets/EffectInfoGroup/asus_gc_slider2 control_n.png"));
                    MediumPoint.Source = new BitmapImage(new Uri(this.BaseUri, "Assets/EffectInfoGroup/asus_gc_slider2 control_d.png"));
                    FastPoint.Source = new BitmapImage(new Uri(this.BaseUri, "Assets/EffectInfoGroup/asus_gc_slider2 control_d.png"));
                }
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
            SelectedEffect.Info.Angle = Convert.ToDouble(AngleTextBox.Text);
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
                AngleTextBox.Text = hue.ToString("F0");
                SelectedEffect.Info.Angle = Convert.ToDouble(AngleTextBox.Text);
                AngleStoryboardStart(hue);
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
            if (SelectedEffect != null)
            {
                if (AngleTextBox.Text != "")
                {
                    SelectedEffect.Info.Angle = Convert.ToDouble(AngleTextBox.Text);
                }
            }
        }
        private void AngleStoryboardStart(double AngleImgTargetAngle)
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
        private void IncreaseBtn_Click(object sender, RoutedEventArgs e)
        {
            EffectInfo ui = SelectedEffect.Info;
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
            EffectInfo ui = SelectedEffect.Info;
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
            EffectInfo ui = SelectedEffect.Info;
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
            EffectInfo ui = SelectedEffect.Info;
            ui.InitColor = Colors.Red;
            ui.Brightness = 3;
            ui.Speed = 1;
            ui.Direction = 2;
            ui.Angle = 90;
            ui.Random = false;
            ui.High = 60;
            ui.Low = 30;
            ui.ColorPointList = new List<ColorPoint>(MainPage.Self.CallDefaultList()[5]);
            ui.ColorSegmentation = false;
            UpdateUIEffectContents(ui);
        }

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
            EffectInfo ui = SelectedEffect.Info;
            PatternCanvas.Children.Clear();
            foreach (var item in ColorPoints)
            {
                item.UI.OnRedraw -= ReDrawMultiPointRectangle;
            }
            ColorPoints.Clear();
            if (ui.ColorPointList != null)
                ui.ColorPointList.Clear();

            foreach (var item in cp)
            {
                ColorPoints.Add(new ColorPoint(item));
                ui.ColorPointList.Add(new ColorPoint(item));
            }
            ShowColorPointUI(ColorPoints);
            MultiPointRectangle.Fill = PatternPolygon.Fill = mf.Foreground;
        }

        public void ShowColorPointUI(List<ColorPoint> cl)
        {
            for (int i = 0; i < cl.Count; i++)
            {
                if(i == 0)
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

        public void RemoveColorPoint()
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

        private void SegmentationSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            EffectInfo ui = SelectedEffect.Info;
            ToggleSwitch toggleSwitch = sender as ToggleSwitch;
            if (toggleSwitch != null)
            {
                if (toggleSwitch.IsOn == true)
                {
                    ui.ColorSegmentation = true;
                }
                else
                {
                    ui.ColorSegmentation = false;
                }
            }
        }
    }
}
