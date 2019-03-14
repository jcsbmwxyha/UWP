using AuraEditor.Common;
using AuraEditor.Dialogs;
using AuraEditor.Models;
using AuraEditor.ViewModels;
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
using Windows.UI.Xaml.Navigation;
using static AuraEditor.Common.Definitions;

// 空白頁項目範本已記錄在 https://go.microsoft.com/fwlink/?LinkId=234238

namespace AuraEditor.Pages
{
    /// <summary>
    /// 可以在本身使用或巡覽至框架內的空白頁面。
    /// </summary>
    public sealed partial class EffectInfoPage : Page
    {
        private EffectInfoModel m_Info;

        static public EffectInfoPage Self;

        bool _angleImgPressing;
        Point AngleImgCenter;

        private double _oldSpeedValue;
        private double _currentSpeedValue = 1;

        private int _oldRainbowSpecialModeValue;
        private int _currentRainbowSpecialModeValue = 1;

        private int _oldColorModeSelectionValue;
        private int _currentColorModeSelectionValue = 1;

        public EffectInfoPage()
        {
            this.InitializeComponent();
            Self = this;
            this.DataContextChanged += (s, e) => Bindings.Update();

            AngleImgCenter = new Point(40, 40);
            AngleTextBox.Text = "0";
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            m_Info = e.Parameter as EffectInfoModel;

            if (m_Info == null)
            {
                EffectInfoStackPanel.Visibility = Visibility.Collapsed;
                return;
            }

            this.DataContext = m_Info;

            ColorPatternModel patternModel = new ColorPatternModel(m_Info);
            patternModel.CurrentColorPoints[0].IsChecked = true;
            ColorPattern.DataContext = patternModel;
            if (m_Info.RainbowSpecialMode == 1)
            {
                ClockwiseRbt.IsChecked = true;
            }
            else if (m_Info.RainbowSpecialMode == 2)
            {
                CountclockwiseRbt.IsChecked = true;
            }
            else if (m_Info.RainbowSpecialMode == 3)
            {
                OutwardRbt.IsChecked = true;
            }
            else if (m_Info.RainbowSpecialMode == 4)
            {
                InwardRbt.IsChecked = true;
            }
            switch (m_Info.ColorModeSelection)
            {
                case 1:
                    Single.IsChecked = true;

                    SingleColorBg.Opacity = 1;
                    SingleColorBg.IsEnabled = true;

                    DoubleColor.Opacity = 0.5;
                    DoubleColor.IsEnabled = false;

                    RandomRangeSlider.Opacity = 0.5;
                    RandomRangeSlider.IsEnabled = false;
                    RandomTextBlock.Opacity = 0.5;
                    break;
                case 2:
                    Random.IsChecked = true;

                    SingleColorBg.Opacity = 0.5;
                    SingleColorBg.IsEnabled = false;

                    DoubleColor.Opacity = 0.5;
                    DoubleColor.IsEnabled = false;

                    RandomRangeSlider.Opacity = 1;
                    RandomRangeSlider.IsEnabled = true;
                    RandomTextBlock.Opacity = 1;
                    break;
                case 4:
                    DoubleRb.IsChecked = true;

                    SingleColorBg.Opacity = 0.5;
                    SingleColorBg.IsEnabled = false;

                    DoubleColor.Opacity = 1;
                    DoubleColor.IsEnabled = true;

                    RandomRangeSlider.Opacity = 0.5;
                    RandomRangeSlider.IsEnabled = false;
                    RandomTextBlock.Opacity = 0.5;
                    break;
            }
            Bindings.Update();
        }

        private void ResetBtn_Tapped(object sender, TappedRoutedEventArgs e)
        {
            m_Info.InitColor = Colors.Red;
            m_Info.DoubleColor1 = Colors.Red;
            m_Info.DoubleColor2 = Colors.Blue;
            m_Info.Brightness = 3;
            m_Info.Speed = 1;
            m_Info.Angle = 90;
            m_Info.RandomRangeMax = 12;
            m_Info.RandomRangeMin = 0;
            m_Info.High = 60;
            m_Info.Low = 30;
            m_Info.CustomizedPattern = new List<ColorPointModel>(DefaultColorPointListCollection[5]); // TODO
            m_Info.ColorSegmentation = true;
            m_Info.RainbowSpecialEffects = false;
            m_Info.RainbowSpecialMode = 1;
        }
        private async void ColorRadioBtn_Tapped(object sender, TappedRoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            Color newColor = await OpenColorPickerWindow(((SolidColorBrush)rb.Background).Color, rb.Name);
            rb.Background = new SolidColorBrush(newColor);

            MainPage.Self.CanShowDeviceUpdateDialog = true;
            MainPage.Self.ShowDeviceUpdateDialogOrNot();
        }
        public async Task<Color> OpenColorPickerWindow(Color c, string ColorBgName)
        {
            ColorPickerDialog colorPickerDialog = new ColorPickerDialog(c);
            await colorPickerDialog.ShowAsync();

            if (colorPickerDialog.ColorPickerResult)
            {
                ReUndoManager.Store(new ColorChangeCommand(LayerPage.Self.CheckedEffect, ColorBgName, colorPickerDialog.PreColor, colorPickerDialog.CurrentColor));
                return colorPickerDialog.CurrentColor;
            }
            else
            {
                return colorPickerDialog.PreColor;
            }
        }
        private void ColorModeSelection_Click(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            switch (rb.Name)
            {
                case "Single":
                    m_Info.ColorModeSelection = 1;

                    SingleColorBg.Opacity = 1;
                    SingleColorBg.IsEnabled = true;

                    DoubleColor.Opacity = 0.5;
                    DoubleColor.IsEnabled = false;

                    RandomRangeSlider.Opacity = 0.5;
                    RandomRangeSlider.IsEnabled = false;
                    RandomTextBlock.Opacity = 0.5;
                    break;
                case "Random":
                    m_Info.ColorModeSelection = 2;

                    SingleColorBg.Opacity = 0.5;
                    SingleColorBg.IsEnabled = false;

                    DoubleColor.Opacity = 0.5;
                    DoubleColor.IsEnabled = false;

                    RandomRangeSlider.Opacity = 1;
                    RandomRangeSlider.IsEnabled = true;
                    RandomTextBlock.Opacity = 1;
                    break;
                case "DoubleRb":
                    m_Info.ColorModeSelection = 4;

                    SingleColorBg.Opacity = 0.5;
                    SingleColorBg.IsEnabled = false;

                    DoubleColor.Opacity = 1;
                    DoubleColor.IsEnabled = true;

                    RandomRangeSlider.Opacity = 0.5;
                    RandomRangeSlider.IsEnabled = false;
                    RandomTextBlock.Opacity = 0.5;
                    break;
            }

            if (m_Info.ColorModeSelection != _currentColorModeSelectionValue)
            {
                _oldColorModeSelectionValue = _currentColorModeSelectionValue;
                _currentColorModeSelectionValue = m_Info.ColorModeSelection;
                ReUndoManager.Store(new ColorModeSelectionChangeCommand(LayerPage.Self.CheckedEffect, _oldColorModeSelectionValue, _currentColorModeSelectionValue));
            }
        }

        private void SegmentationSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (m_Info.ColorSegmentation != SegmentationSwitch.IsOn)
            {
                ReUndoManager.Store(new PatternModeChangeCommand(LayerPage.Self.CheckedEffect, SegmentationSwitch.IsOn));
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
            if (m_Info.Speed != slider.Value)
            {
                _oldSpeedValue = _currentSpeedValue;
                _currentSpeedValue = slider.Value;
                ReUndoManager.Store(new MoveSpeedCommand(LayerPage.Self.CheckedEffect, _oldSpeedValue, _currentSpeedValue));
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
                if(m_Info.Angle != targetAngle)
                {
                    ReUndoManager.Store(new AngleChangeCommand(LayerPage.Self.CheckedEffect, sourceAngle, targetAngle));
                }
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

        private void RainbowRoatationSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (m_Info.RainbowSpecialEffects != RainbowRoatationSwitch.IsOn)
            {
                ReUndoManager.Store(new RainbowSpecialEffectOnOffCommand(LayerPage.Self.CheckedEffect, RainbowRoatationSwitch.IsOn));
            }

            if (RainbowRoatationSwitch.IsOn == true)
            {
                m_Info.RainbowSpecialEffects = true;
                AngleGridCC.IsEnabled = false;
                AngleGridCC.Opacity = 0.5;
                ClockwiseRbtCC.IsEnabled = true;
                ClockwiseRbtCC.Opacity = 1;
            }
            else
            {
                m_Info.RainbowSpecialEffects = false;
                AngleGridCC.IsEnabled = true;
                AngleGridCC.Opacity = 1;
                ClockwiseRbtCC.IsEnabled = false;
                ClockwiseRbtCC.Opacity = 0.5;
            }
        }

        private void RotationRbt_Click(object sender, RoutedEventArgs e)
        {
            RadioButton rotationmodeBtn = sender as RadioButton;
            switch (rotationmodeBtn.Name)
            {
                case "ClockwiseRbt":
                    m_Info.RainbowSpecialMode = 1;
                    break;
                case "CountclockwiseRbt":
                    m_Info.RainbowSpecialMode = 2;
                    break;
                case "OutwardRbt":
                    m_Info.RainbowSpecialMode = 3;
                    break;
                case "InwardRbt":
                    m_Info.RainbowSpecialMode = 4;
                    break;
                default:
                    m_Info.RainbowSpecialMode = 1;
                    break;
            }
            if (m_Info.RainbowSpecialMode != _currentRainbowSpecialModeValue)
            {
                _oldRainbowSpecialModeValue = _currentRainbowSpecialModeValue;
                _currentRainbowSpecialModeValue = m_Info.RainbowSpecialMode;
                ReUndoManager.Store(new RainbowSpecialModeChangeCommand(LayerPage.Self.CheckedEffect, _oldRainbowSpecialModeValue, _currentRainbowSpecialModeValue));
            }
        }
        #endregion

        #region ReUndo

        public class ColorChangeCommand : IReUndoCommand
        {
            private Color _oldColorValue;
            private Color _currentColorValue;
            private string _colorBgName;
            private EffectLineViewModel _checkedEffect;

            public ColorChangeCommand(EffectLineViewModel checkedEffect, string ColorBgName, Color oldColorValue, Color currentColorValue)
            {
                _checkedEffect = checkedEffect;
                _oldColorValue = oldColorValue;
                _currentColorValue = currentColorValue;
                _colorBgName = ColorBgName;
            }

            public void ExecuteRedo()
            {
                LayerPage.Self.CheckedEffect = _checkedEffect;
                switch (_colorBgName)
                {
                    case "SingleColorBg":
                        _checkedEffect.Model.Info.InitColor = _currentColorValue;
                        break;
                    case "DoubleColorBg_1":
                        _checkedEffect.Model.Info.DoubleColor1 = _currentColorValue;
                        break;
                    case "DoubleColorBg_2":
                        _checkedEffect.Model.Info.DoubleColor2 = _currentColorValue;
                        break;
                }
            }

            public void ExecuteUndo()
            {
                LayerPage.Self.CheckedEffect = _checkedEffect;
                switch (_colorBgName)
                {
                    case "SingleColorBg":
                        _checkedEffect.Model.Info.InitColor = _oldColorValue;
                        break;
                    case "DoubleColorBg_1":
                        _checkedEffect.Model.Info.DoubleColor1 = _oldColorValue;
                        break;
                    case "DoubleColorBg_2":
                        _checkedEffect.Model.Info.DoubleColor2 = _oldColorValue;
                        break;
                }
            }
        }

        public class ColorModeSelectionChangeCommand : IReUndoCommand
        {
            private int _oldColorModeValue;
            private int _currentColorModeValue;
            private EffectLineViewModel _checkedEffect;

            public ColorModeSelectionChangeCommand(EffectLineViewModel checkedEffect, int oldColorModeValue, int currentColorModeValue)
            {
                _checkedEffect = checkedEffect;
                _oldColorModeValue = oldColorModeValue;
                _currentColorModeValue = currentColorModeValue;
            }

            public void ExecuteRedo()
            {
                LayerPage.Self.CheckedEffect = _checkedEffect;
                _checkedEffect.Model.Info.ColorModeSelection = _currentColorModeValue;
                SelectionMode(_currentColorModeValue);
            }

            public void ExecuteUndo()
            {
                LayerPage.Self.CheckedEffect = _checkedEffect;
                _checkedEffect.Model.Info.ColorModeSelection = _oldColorModeValue;
                SelectionMode(_oldColorModeValue);
            }

            public void SelectionMode(int mode)
            {
                switch (mode)
                {
                    case 1:
                        EffectInfoPage.Self.Single.IsChecked = true;

                        EffectInfoPage.Self.SingleColorBg.Opacity = 1;
                        EffectInfoPage.Self.SingleColorBg.IsEnabled = true;

                        EffectInfoPage.Self.DoubleColor.Opacity = 0.5;
                        EffectInfoPage.Self.DoubleColor.IsEnabled = false;

                        EffectInfoPage.Self.RandomRangeSlider.Opacity = 0.5;
                        EffectInfoPage.Self.RandomRangeSlider.IsEnabled = false;
                        EffectInfoPage.Self.RandomTextBlock.Opacity = 0.5;
                        break;
                    case 2:
                        EffectInfoPage.Self.Random.IsChecked = true;

                        EffectInfoPage.Self.SingleColorBg.Opacity = 0.5;
                        EffectInfoPage.Self.SingleColorBg.IsEnabled = false;

                        EffectInfoPage.Self.DoubleColor.Opacity = 0.5;
                        EffectInfoPage.Self.DoubleColor.IsEnabled = false;

                        EffectInfoPage.Self.RandomRangeSlider.Opacity = 1;
                        EffectInfoPage.Self.RandomRangeSlider.IsEnabled = true;
                        EffectInfoPage.Self.RandomTextBlock.Opacity = 1;
                        break;
                    case 4:
                        EffectInfoPage.Self.DoubleRb.IsChecked = true;

                        EffectInfoPage.Self.SingleColorBg.Opacity = 0.5;
                        EffectInfoPage.Self.SingleColorBg.IsEnabled = false;

                        EffectInfoPage.Self.DoubleColor.Opacity = 1;
                        EffectInfoPage.Self.DoubleColor.IsEnabled = true;

                        EffectInfoPage.Self.RandomRangeSlider.Opacity = 0.5;
                        EffectInfoPage.Self.RandomRangeSlider.IsEnabled = false;
                        EffectInfoPage.Self.RandomTextBlock.Opacity = 0.5;
                        break;
                }
            }
        }

        public class PatternModeChangeCommand : IReUndoCommand
        {
            private bool _patternModeChangeValue;
            private EffectLineViewModel _checkedEffect;

            public PatternModeChangeCommand(EffectLineViewModel checkedEffect, bool patternModeChangeValue)
            {
                _checkedEffect = checkedEffect;
                _patternModeChangeValue = patternModeChangeValue;
            }

            public void ExecuteRedo()
            {
                LayerPage.Self.CheckedEffect = _checkedEffect;
                _checkedEffect.Model.Info.ColorSegmentation = _patternModeChangeValue;
            }

            public void ExecuteUndo()
            {
                LayerPage.Self.CheckedEffect = _checkedEffect;
                _checkedEffect.Model.Info.ColorSegmentation = !_patternModeChangeValue;
            }
        }

        public class MoveSpeedCommand : IReUndoCommand
        {
            private double _oldSpeedSliderValue;
            private double _currentSpeedSliderValue;
            private EffectLineViewModel _checkedEffect;

            public MoveSpeedCommand(EffectLineViewModel checkedEffect, double oldSpeedSliderValue, double currentSpeedSliderValue)
            {
                _checkedEffect = checkedEffect;
                _oldSpeedSliderValue = oldSpeedSliderValue;
                _currentSpeedSliderValue = currentSpeedSliderValue;
            }

            public void ExecuteRedo()
            {
                LayerPage.Self.CheckedEffect = _checkedEffect;
                _checkedEffect.Model.Info.Speed = (int)_currentSpeedSliderValue;
            }

            public void ExecuteUndo()
            {
                LayerPage.Self.CheckedEffect = _checkedEffect;
                _checkedEffect.Model.Info.Speed = (int)_oldSpeedSliderValue;
            }
        }

        public class AngleChangeCommand : IReUndoCommand
        {
            private double _oldAngleValue;
            private double _currentAngleValue;
            private EffectLineViewModel _checkedEffect;

            public AngleChangeCommand(EffectLineViewModel checkedEffect, double oldAngleValue, double currentAngleValue)
            {
                _checkedEffect = checkedEffect;
                _oldAngleValue = oldAngleValue;
                _currentAngleValue = currentAngleValue;
            }

            public void ExecuteRedo()
            {
                LayerPage.Self.CheckedEffect = _checkedEffect;
                _checkedEffect.Model.Info.Angle = (int)_currentAngleValue;
            }

            public void ExecuteUndo()
            {
                LayerPage.Self.CheckedEffect = _checkedEffect;
                _checkedEffect.Model.Info.Angle = (int)_oldAngleValue;
            }
        }

        public class RainbowSpecialEffectOnOffCommand : IReUndoCommand
        {
            private bool _rainbowSpecialEffectsValue;
            private EffectLineViewModel _checkedEffect;

            public RainbowSpecialEffectOnOffCommand(EffectLineViewModel checkedEffect, bool rainbowSpecialEffectsValue)
            {
                _checkedEffect = checkedEffect;
                _rainbowSpecialEffectsValue = rainbowSpecialEffectsValue;
            }

            public void ExecuteRedo()
            {
                LayerPage.Self.CheckedEffect = _checkedEffect;
                _checkedEffect.Model.Info.RainbowSpecialEffects = _rainbowSpecialEffectsValue;
            }

            public void ExecuteUndo()
            {
                LayerPage.Self.CheckedEffect = _checkedEffect;
                _checkedEffect.Model.Info.RainbowSpecialEffects = !_rainbowSpecialEffectsValue;
            }
        }

        public class RainbowSpecialModeChangeCommand : IReUndoCommand
        {
            private int _oldSpecialModeValue;
            private int _currentSpecialModeValue;
            private EffectLineViewModel _checkedEffect;

            public RainbowSpecialModeChangeCommand(EffectLineViewModel checkedEffect, int oldSpecialModeValue, int currentSpecialModeValue)
            {
                _checkedEffect = checkedEffect;
                _oldSpecialModeValue = oldSpecialModeValue;
                _currentSpecialModeValue = currentSpecialModeValue;
            }

            public void ExecuteRedo()
            {
                LayerPage.Self.CheckedEffect = _checkedEffect;
                _checkedEffect.Model.Info.RainbowSpecialMode = _currentSpecialModeValue;
                SelectionMode(_currentSpecialModeValue);
            }

            public void ExecuteUndo()
            {
                LayerPage.Self.CheckedEffect = _checkedEffect;
                _checkedEffect.Model.Info.RainbowSpecialMode = _oldSpecialModeValue;
                SelectionMode(_oldSpecialModeValue);
            }

            public void SelectionMode(int mode)
            {
                switch (mode)
                {
                    case 1:
                        EffectInfoPage.Self.ClockwiseRbt.IsChecked = true;
                        break;
                    case 2:
                        EffectInfoPage.Self.CountclockwiseRbt.IsChecked = true;
                        break;
                    case 3:
                        EffectInfoPage.Self.OutwardRbt.IsChecked = true;
                        break;
                    case 4:
                        EffectInfoPage.Self.InwardRbt.IsChecked = true;
                        break;
                }
            }
        }
        #endregion
    }
}
