using AuraEditor.Dialogs;
using AuraEditor.Models;
using System;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using static AuraEditor.Common.EffectHelper;

// 使用者控制項項目範本記載於 https://go.microsoft.com/fwlink/?LinkId=234236

namespace AuraEditor.UserControls
{
    public sealed partial class ColorPointView : UserControl
    {
        private ColorPointModel m_ColorPointModel { get { return this.DataContext as ColorPointModel; } }
        public bool FromTriggerDialog = false;
        public TriggerDialog m_td;

        public ColorPointView()
        {
            this.InitializeComponent();
        }

        public ColorPointView(TriggerDialog td)
        {
            this.InitializeComponent();
            FromTriggerDialog = true;
            m_td = td;
        }

        private void ColorPointRadioButton_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (TT.X + e.Delta.Translation.X < m_ColorPointModel.LeftBorder)
            {
                TT.X = m_ColorPointModel.LeftBorder;
            }
            else if (TT.X + e.Delta.Translation.X > m_ColorPointModel.RightBorder)
            {
                TT.X = m_ColorPointModel.RightBorder;
            }
            else
            {
                TT.X += e.Delta.Translation.X;
                m_ColorPointModel.ParentPattern.OnManipulationDelta();
            }
        }
        private void ColorPointRadioButton_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            m_ColorPointModel.ParentPattern.OnManipulationCompleted();
        }
        private async void ColorPointtRadioButton_DoubleTapped(object sender, RoutedEventArgs e)
        {
            if (FromTriggerDialog)
            {
                m_td.Hide();
            }
            Color newColor = await OpenColorPickerWindow(((SolidColorBrush)ColorPointBg.Background).Color);

            ColorPointBg.Background = new SolidColorBrush(newColor);
        }

        public async Task<Color> OpenColorPickerWindow(Color c)
        {
            ColorPickerDialog colorPickerDialog;
            if (FromTriggerDialog)
            {
                colorPickerDialog = new ColorPickerDialog(c, m_td);
            }
            else
            {
                colorPickerDialog = new ColorPickerDialog(c);
            }
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
    }
}
