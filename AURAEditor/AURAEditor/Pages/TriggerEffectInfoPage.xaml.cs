using AuraEditor.Dialogs;
using AuraEditor.Models;
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

            foreach (var effectName in GetTriggerEffect())
            {
                MenuFlyoutItem mfi = new MenuFlyoutItem();
                mfi.Text = effectName;
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
            ColorPattern.DataContext = new ColorPatternModel(m_Info, ColorPattern.ItemsCanvas);
            SetColorMode(m_TriggerEffect.Info);
            Bindings.Update();
        }

        private void EffectSelected(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuFlyoutItem;
            string selectedName = item.Text;

            if (selectedName == EffectSelectionButton.Content as string)
                return;

            int type = GetEffectIndex(selectedName);
            m_TriggerEffect.ChangeType(type);
            ColorPattern.DataContext = new ColorPatternModel(m_Info, ColorPattern.ItemsCanvas);
            SetColorMode(m_TriggerEffect.Info);
        }

        private void SetColorMode(EffectInfoModel effectInfo)
        {
            TriggerColorPickerButtonBg.Background = new SolidColorBrush(effectInfo.InitColor);
            switch (effectInfo.ColorModeSelection)
            {
                case 1:
                    Single.IsChecked = true;

                    ColorGroup.Opacity = 1;
                    TriggerColorPickerButtonBg.IsEnabled = true;
                    
                    RandomGroup.Opacity = 0.5;

                    PatternGroup.Opacity = 0.5;
                    ColorPattern.IsEnabled = false;
                    PatternSwitch.IsEnabled = false;
                    break;
                case 2:
                    Random.IsChecked = true;

                    ColorGroup.Opacity = 0.5;
                    TriggerColorPickerButtonBg.IsEnabled = false;
                    
                    RandomGroup.Opacity = 1;

                    PatternGroup.Opacity = 0.5;
                    ColorPattern.IsEnabled = false;
                    PatternSwitch.IsEnabled = false;
                    break;
                case 3:
                    Pattern.IsChecked = true;

                    ColorGroup.Opacity = 0.5;
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

                    ColorGroup.Opacity = 1;
                    TriggerColorPickerButtonBg.IsEnabled = true;
                    
                    RandomGroup.Opacity = 0.5;

                    PatternGroup.Opacity = 0.5;
                    ColorPattern.IsEnabled = false;
                    PatternSwitch.IsEnabled = false;
                    break;
                case "Random":
                    m_Info.ColorModeSelection = 2;

                    ColorGroup.Opacity = 0.5;
                    TriggerColorPickerButtonBg.IsEnabled = false;
                    
                    RandomGroup.Opacity = 1;

                    PatternGroup.Opacity = 0.5;
                    ColorPattern.IsEnabled = false;
                    PatternSwitch.IsEnabled = false;
                    break;
                case "Pattern":
                    m_Info.ColorModeSelection = 3;

                    ColorGroup.Opacity = 0.5;
                    TriggerColorPickerButtonBg.IsEnabled = false;
                    
                    RandomGroup.Opacity = 0.5;

                    PatternGroup.Opacity = 1;
                    ColorPattern.IsEnabled = true;
                    PatternSwitch.IsEnabled = true;
                    break;
            }
        }

        private void SpeedValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (sender is Slider slider)
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
    }
}
