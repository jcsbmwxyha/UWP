using AuraEditor.Dialogs;
using System;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using AuraEditor.Dialogs;

// 使用者控制項項目範本記載於 https://go.microsoft.com/fwlink/?LinkId=234236

namespace AuraEditor.UserControls
{
    public sealed partial class ColorPointBt : UserControl
    {
        public delegate void ReDrawCallBack();
        public ReDrawCallBack OnRedraw;
        public double X
        {
            get
            {
                CompositeTransform ct = this.RenderTransform as CompositeTransform;
                return ct.TranslateX;
            }
            set
            {
                CompositeTransform ct = this.RenderTransform as CompositeTransform;
                ct.TranslateX = value;
            }
        }

        public double LeftBorder { get; set; }
        public double RightBorder { get; set; }

        public bool FromTriggerDialog = false;
        public TriggerDialog m_td;

        public ColorPointBt()
        {
            this.InitializeComponent();
        }

        public ColorPointBt(TriggerDialog td)
        {
            this.InitializeComponent();
            FromTriggerDialog = true;
            m_td = td;
        }

        private void ColorPointBtn_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (X + e.Delta.Translation.X < LeftBorder)
            {
                X = LeftBorder;
                return;
            }
            if (X + e.Delta.Translation.X > RightBorder)
            {
                X = RightBorder;
                return;
            }
            X += e.Delta.Translation.X;
            OnRedraw?.Invoke();
        }

        private async void ColorPointBtn_DoubleTapped(object sender, RoutedEventArgs e)
        {
            if (FromTriggerDialog)
            {
                m_td.Hide();
            }
            Color newColor = await OpenColorPickerWindow(((SolidColorBrush)ColorPointBg.Background).Color);

            ColorPointBg.Background = new SolidColorBrush(newColor);
            OnRedraw?.Invoke();
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
