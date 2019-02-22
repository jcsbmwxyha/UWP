using AuraEditor.Dialogs;
using AuraEditor.Models;
using System;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using static AuraEditor.Common.ControlHelper;

// 使用者控制項項目範本記載於 https://go.microsoft.com/fwlink/?LinkId=234236

namespace AuraEditor.UserControls
{
    public sealed partial class ColorPointView : UserControl
    {
        private ColorPointModel m_ColorPointModel { get { return this.DataContext as ColorPointModel; } }

        public ColorPointView()
        {
            this.InitializeComponent();
        }

        public ColorPointView(TriggerDialog td)
        {
            this.InitializeComponent();
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
        private async void ColorPointRadioButton_DoubleTapped(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = GetCurrentContentDialog();

            if (dialog != null)
                dialog.Hide();

            ColorPickerDialog colorPickerDialog = new ColorPickerDialog(m_ColorPointModel.Color);
            await colorPickerDialog.ShowAsync();

            if (colorPickerDialog.ColorPickerResult)
                m_ColorPointModel.Color = colorPickerDialog.CurrentColor;
            else
                m_ColorPointModel.Color = colorPickerDialog.PreColor;

            m_ColorPointModel.ParentPattern.OnManipulationCompleted();

            if (dialog != null)
                await dialog.ShowAsync();
            else
                MainPage.Self.ShowDeviceUpdateDialogOrNot();
        }
    }
}