using AuraEditor.Common;
using AuraEditor.Dialogs;
using AuraEditor.Models;
using AuraEditor.ViewModels;
using System;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using static AuraEditor.Common.ControlHelper;
using static AuraEditor.Common.EffectHelper;

// 空白頁項目範本已記錄在 https://go.microsoft.com/fwlink/?LinkId=234238

namespace AuraEditor.Pages
{
    /// <summary>
    /// 可以在本身使用或巡覽至框架內的空白頁面。
    /// </summary>
    public sealed partial class TriggerEffectInfoPage : Page
    {
        private TriggerEffect m_TriggerEffect;

        private EffectInfoModel _oldEffectInfoModel;
        private EffectInfoModel _currentEffectInfoModel;

        private int _oldColorModeSelectionValue;
        private int _currentColorModeSelectionValue = 1;

        private double _oldSpeedValue;
        private double _currentSpeedValue = 1;
        private EffectInfoModel m_Info
        {
            get
            {
                if (m_TriggerEffect != null)
                    return m_TriggerEffect.Info;
                else
                    return null;
            }
        }

        public TriggerEffectInfoPage()
        {
            this.InitializeComponent();

            
            for (int i = 0; i < GetTriggerEffect().Length; i++)
            {
                int effectIndex = i + GetCommonEffect().Length;
                MenuFlyoutItem mfi = new MenuFlyoutItem();
                mfi.Text = GetEffectNameByNumString(effectIndex.ToString());
                mfi.Name = GetEffEngName(effectIndex);
                mfi.Style = (Style)Application.Current.Resources["RogMenuFlyoutItemStyle1"];
                mfi.Click += EffectSelected;
                EffectSelectionMenuFlyout.Items.Add(mfi);
            }
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            m_TriggerEffect = e.Parameter as TriggerEffect;

            if (m_TriggerEffect == null)
            {
                EffectInfoStackPanel.Visibility = Visibility.Collapsed;
                return;
            }

            this.DataContext = m_Info;
            ColorPattern.DataContext = new ColorPatternModel(m_Info);
            SetColorMode(m_TriggerEffect.Info);
            Bindings.Update();
        }

        private void EffectSelected(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuFlyoutItem;
            string selectedName = item.Name;

            if (selectedName == EffectSelectionButton.Content as string)
                return;

            _oldEffectInfoModel = new EffectInfoModel(m_TriggerEffect.Info);

            int type = GetEffIdxByEng(selectedName);
            m_TriggerEffect.ChangeType(type);
            ColorPattern.DataContext = new ColorPatternModel(m_Info);
            SetColorMode(m_TriggerEffect.Info);

            _currentEffectInfoModel = new EffectInfoModel(m_TriggerEffect.Info);
            ReUndoManager.Store(new EffectSelectedCommand(m_TriggerEffect, _oldEffectInfoModel, _currentEffectInfoModel));
        }

        private void SetColorMode(EffectInfoModel effectInfo)
        {
            TriggerColorPickerButtonBg.Background = new SolidColorBrush(effectInfo.InitColor);
            switch (effectInfo.ColorModeSelection)
            {
                case 1:
                    Single.IsChecked = true;

                    TriggerColorPickerButtonBg.Opacity = 1;
                    TriggerColorPickerButtonBg.IsEnabled = true;

                    RandomGroup.Opacity = 0.5;

                    PatternGroup.Opacity = 0.5;
                    ColorPattern.IsEnabled = false;
                    PatternSwitch.IsEnabled = false;
                    break;
                case 2:
                    Random.IsChecked = true;

                    TriggerColorPickerButtonBg.Opacity = 0.5;
                    TriggerColorPickerButtonBg.IsEnabled = false;

                    RandomGroup.Opacity = 1;

                    PatternGroup.Opacity = 0.5;
                    ColorPattern.IsEnabled = false;
                    PatternSwitch.IsEnabled = false;
                    break;
                case 3:
                    Pattern.IsChecked = true;

                    TriggerColorPickerButtonBg.Opacity = 0.5;
                    TriggerColorPickerButtonBg.IsEnabled = false;

                    RandomGroup.Opacity = 0.5;

                    PatternGroup.Opacity = 1;
                    ColorPattern.IsEnabled = true;
                    PatternSwitch.IsEnabled = true;
                    break;
            }
        }

        private async void ColorRadioBtn_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ContentDialog dialog = GetCurrentContentDialog();
            if (dialog != null)
            {
                dialog.Hide();
                Color newColor = await OpenColorPickerWindow(((SolidColorBrush)TriggerColorPickerButtonBg.Background).Color);
                m_Info.InitColor = newColor;
                TriggerColorPickerButtonBg.Background = new SolidColorBrush(m_Info.InitColor);
                await dialog.ShowAsync();
            }
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
        private void ColorModeSelection_Click(object sender, RoutedEventArgs e)
        {
            RadioButton colormodeselectionBtn = sender as RadioButton;
            switch (colormodeselectionBtn.Name)
            {
                case "Single":
                    m_Info.ColorModeSelection = 1;

                    TriggerColorPickerButtonBg.Opacity = 1;
                    TriggerColorPickerButtonBg.IsEnabled = true;
                    
                    RandomGroup.Opacity = 0.5;

                    PatternGroup.Opacity = 0.5;
                    ColorPattern.IsEnabled = false;
                    PatternSwitch.IsEnabled = false;
                    break;
                case "Random":
                    m_Info.ColorModeSelection = 2;

                    TriggerColorPickerButtonBg.Opacity = 0.5;
                    TriggerColorPickerButtonBg.IsEnabled = false;
                    
                    RandomGroup.Opacity = 1;

                    PatternGroup.Opacity = 0.5;
                    ColorPattern.IsEnabled = false;
                    PatternSwitch.IsEnabled = false;
                    break;
                case "Pattern":
                    m_Info.ColorModeSelection = 3;

                    TriggerColorPickerButtonBg.Opacity = 0.5;
                    TriggerColorPickerButtonBg.IsEnabled = false;
                    
                    RandomGroup.Opacity = 0.5;

                    PatternGroup.Opacity = 1;
                    ColorPattern.IsEnabled = true;
                    PatternSwitch.IsEnabled = true;
                    break;
            }

            if (m_Info.ColorModeSelection != _currentColorModeSelectionValue)
            {
                _oldColorModeSelectionValue = _currentColorModeSelectionValue;
                _currentColorModeSelectionValue = m_Info.ColorModeSelection;
                ReUndoManager.Store(new ColorModeSelectionChangeCommand(m_Info, _oldColorModeSelectionValue, _currentColorModeSelectionValue));
            }
        }

        private void SegmentationSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (m_Info.ColorSegmentation != SegmentationSwitch.IsOn)
            {
                ReUndoManager.Store(new PatternModeChangeCommand(m_Info, SegmentationSwitch.IsOn));
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
                _oldSpeedValue = m_Info.Speed;
                _currentSpeedValue = slider.Value;
                ReUndoManager.Store(new MoveSpeedCommand(m_Info, _oldSpeedValue, _currentSpeedValue));
            }
        }

        #region ReUndo
        public class EffectSelectedCommand : IReUndoCommand
        {
            private TriggerEffect _triggerEffect;
            private EffectInfoModel _oldEffectInfoModelValue;
            private EffectInfoModel _currentEffectInfoModelValue;

            public EffectSelectedCommand(TriggerEffect triggerEffect, EffectInfoModel oldEffectInfoModelValue, EffectInfoModel currentEffectInfoModelValue)
            {
                _triggerEffect = triggerEffect;
                _oldEffectInfoModelValue = oldEffectInfoModelValue;
                _currentEffectInfoModelValue = currentEffectInfoModelValue;
            }

            public void ExecuteRedo()
            {
                _triggerEffect.Info = _currentEffectInfoModelValue;
            }

            public void ExecuteUndo()
            {
                _triggerEffect.Info = _oldEffectInfoModelValue;
            }
        }

        public class ColorModeSelectionChangeCommand : IReUndoCommand
        {
            private int _oldColorModeValue;
            private int _currentColorModeValue;
            private EffectInfoModel _info;

            public ColorModeSelectionChangeCommand(EffectInfoModel info, int oldColorModeValue, int currentColorModeValue)
            {
                _info = info;
                _oldColorModeValue = oldColorModeValue;
                _currentColorModeValue = currentColorModeValue;
            }

            public void ExecuteRedo()
            {
                _info.ColorModeSelection = _currentColorModeValue;
            }

            public void ExecuteUndo()
            {
                _info.ColorModeSelection = _oldColorModeValue;
            }
        }

        public class PatternModeChangeCommand : IReUndoCommand
        {
            private bool _patternModeChangeValue;
            private EffectInfoModel _info;

            public PatternModeChangeCommand(EffectInfoModel info, bool patternModeChangeValue)
            {
                _info = info;
                _patternModeChangeValue = patternModeChangeValue;
            }

            public void ExecuteRedo()
            {
                _info.ColorSegmentation = _patternModeChangeValue;
            }

            public void ExecuteUndo()
            {
                _info.ColorSegmentation = !_patternModeChangeValue;
            }
        }

        public class MoveSpeedCommand : IReUndoCommand
        {
            private double _oldSpeedSliderValue;
            private double _currentSpeedSliderValue;
            private EffectInfoModel _info;

            public MoveSpeedCommand(EffectInfoModel info, double oldSpeedSliderValue, double currentSpeedSliderValue)
            {
                _info = info;
                _oldSpeedSliderValue = oldSpeedSliderValue;
                _currentSpeedSliderValue = currentSpeedSliderValue;
            }

            public void ExecuteRedo()
            {
                _info.Speed = (int)_currentSpeedSliderValue;
            }

            public void ExecuteUndo()
            {
                _info.Speed = (int)_oldSpeedSliderValue;
            }
        }
        #endregion
    }
}
