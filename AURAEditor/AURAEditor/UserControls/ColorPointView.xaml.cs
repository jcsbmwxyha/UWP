using AuraEditor.Common;
using AuraEditor.Dialogs;
using AuraEditor.Models;
using AuraEditor.Pages;
using AuraEditor.ViewModels;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using static AuraEditor.Common.ControlHelper;
using static AuraEditor.UserControls.ColorPatternView;

// 使用者控制項項目範本記載於 https://go.microsoft.com/fwlink/?LinkId=234236

namespace AuraEditor.UserControls
{
    public sealed partial class ColorPointView : UserControl
    {
        private ColorPointModel m_ColorPointModel { get { return this.DataContext as ColorPointModel; } }
        private ColorPatternModel parent;

        public ColorPointView()
        {
            this.InitializeComponent();
        }

        private void ColorPointBg_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            parent = ColorPatternModel.Self;
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
                parent.OnManipulationDelta();
            }
        }
        private void ColorPointRadioButton_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            parent.OnCustomizeChanged();
        }
        private async void ColorPointRadioButton_DoubleTapped(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = GetCurrentContentDialog();

            if (dialog != null)
                dialog.Hide();

            ColorPickerDialog colorPickerDialog = new ColorPickerDialog(m_ColorPointModel.Color);
            await colorPickerDialog.ShowAsync();

            if (colorPickerDialog.ColorPickerResult)
            {
                m_ColorPointModel.Color = colorPickerDialog.CurrentColor;
                ColorPatternModel.Self.OnCustomizeChanged();
            }

            if (dialog != null)
                await dialog.ShowAsync();
            else
                MainPage.Self.ShowDeviceUpdateDialogOrNot();
        }
    }
}