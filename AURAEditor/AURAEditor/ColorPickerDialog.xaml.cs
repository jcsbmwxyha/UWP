using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// 內容對話方塊項目範本已記錄在 https://go.microsoft.com/fwlink/?LinkId=234238

namespace AuraEditor
{
    public sealed partial class ColorPickerDialog : ContentDialog
    {
        private int WindowsSizeFlag = 0;
        public ColorPickerDialog()
        {
            this.InitializeComponent();
            Window.Current.CoreWindow.SizeChanged += CurrentWindow_SizeChanged;
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            Window.Current.CoreWindow.SizeChanged -= CurrentWindow_SizeChanged;
            this.Hide();
        }

        private void RBtnDefault_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }

        private void RBtnRecent_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }

        private void CurrentWindow_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            if (e.Size.Width > 664 && WindowsSizeFlag != 1)
            {
                ColorPickerGrid.Width = 664;
                ColorPickerGrid.Height = 716;
                BackgroundImage.Source = new BitmapImage(new Uri(this.BaseUri, "Assets/EffectInfoGroup/asus_gc_aura_customize_colorpick_selected_colorpicker_bg.png"));
                Col_1.Width = new GridLength(364, GridUnitType.Star);
                Col_2.Width = new GridLength(300, GridUnitType.Star);
                RingGrid.Margin = new Thickness(60, 0, 0, 0);
                ColorSetGrid.Margin = new Thickness(68, 48, 0, 0);
                SelectGrid.Margin = new Thickness(56, 0, 72, 0);
                Grid_Selected.Width = 172;
                Grid_Selected.Height = 32;
                TextBox_R.Width = 108;
                TextBox_G.Width = 108;
                TextBox_B.Width = 108;
                Grid_R.Margin = new Thickness(35, 26, 0, 0);
                Grid_G.Margin = new Thickness(35, 16, 0, 0);
                Grid_B.Margin = new Thickness(35, 16, 0, 0);
                RBtnDefault1.Margin = new Thickness(0, 0, 12, 0);
                RBtnDefault2.Margin = new Thickness(0, 0, 12, 0);
                RBtnDefault3.Margin = new Thickness(0, 0, 12, 0);
                RBtnDefault4.Margin = new Thickness(0, 0, 12, 0);
                RBtnDefault5.Margin = new Thickness(0, 0, 12, 0);
                RBtnDefault6.Margin = new Thickness(0, 0, 12, 0);
                RBtnDefault7.Margin = new Thickness(0, 0, 12, 0);
                RBtnDefault8.Margin = new Thickness(0, 0, 12, 0);
                RBtnDefault9.Margin = new Thickness(0, 0, 12, 0);
                RBtnRecent_1.Margin = new Thickness(0, 0, 12, 0);
                RBtnRecent_2.Margin = new Thickness(0, 0, 12, 0);
                RBtnRecent_3.Margin = new Thickness(0, 0, 12, 0);
                RBtnRecent_4.Margin = new Thickness(0, 0, 12, 0);
                RBtnRecent_5.Margin = new Thickness(0, 0, 12, 0);
                RBtnRecent_6.Margin = new Thickness(0, 0, 12, 0);
                RBtnRecent_7.Margin = new Thickness(0, 0, 12, 0);
                RBtnRecent_8.Margin = new Thickness(0, 0, 12, 0);
                RBtnRecent_9.Margin = new Thickness(0, 0, 12, 0);
                WindowsSizeFlag = 1;
            }
            else if (e.Size.Width < 664 && WindowsSizeFlag != -1)
            {
                ColorPickerGrid.Width = 500;
                ColorPickerGrid.Height = 716;
                BackgroundImage.Source = new BitmapImage(new Uri(this.BaseUri, "Assets/EffectInfoGroup/asus_gc_aura_customize_colorpick_selected_colorpicker_small_bg.png"));
                Col_1.Width = new GridLength(328, GridUnitType.Star);
                Col_2.Width = new GridLength(172, GridUnitType.Star);
                RingGrid.Margin = new Thickness(25, 0, 0, 0);
                ColorSetGrid.Margin = new Thickness(18, 48, 0, 0);
                SelectGrid.Margin = new Thickness(12, 0, 24, 0);
                Grid_Selected.Width = 136;
                Grid_Selected.Height = 32;
                TextBox_R.Width = 88;
                TextBox_G.Width = 88;
                TextBox_B.Width = 88;
                Grid_R.Margin = new Thickness(19, 26, 0, 0);
                Grid_G.Margin = new Thickness(19, 16, 0, 0);
                Grid_B.Margin = new Thickness(19, 16, 0, 0);
                RBtnDefault1.Margin = new Thickness(0, 0, 4, 0);
                RBtnDefault2.Margin = new Thickness(0, 0, 4, 0);
                RBtnDefault3.Margin = new Thickness(0, 0, 4, 0);
                RBtnDefault4.Margin = new Thickness(0, 0, 4, 0);
                RBtnDefault5.Margin = new Thickness(0, 0, 4, 0);
                RBtnDefault6.Margin = new Thickness(0, 0, 4, 0);
                RBtnDefault7.Margin = new Thickness(0, 0, 4, 0);
                RBtnDefault8.Margin = new Thickness(0, 0, 4, 0);
                RBtnDefault9.Margin = new Thickness(0, 0, 4, 0);
                RBtnRecent_1.Margin = new Thickness(0, 0, 4, 0);
                RBtnRecent_2.Margin = new Thickness(0, 0, 4, 0);
                RBtnRecent_3.Margin = new Thickness(0, 0, 4, 0);
                RBtnRecent_4.Margin = new Thickness(0, 0, 4, 0);
                RBtnRecent_5.Margin = new Thickness(0, 0, 4, 0);
                RBtnRecent_6.Margin = new Thickness(0, 0, 4, 0);
                RBtnRecent_7.Margin = new Thickness(0, 0, 4, 0);
                RBtnRecent_8.Margin = new Thickness(0, 0, 4, 0);
                RBtnRecent_9.Margin = new Thickness(0, 0, 4, 0);
                WindowsSizeFlag = -1;
            }
        }
    }
}
