using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// 內容對話方塊項目範本已記錄在 https://go.microsoft.com/fwlink/?LinkId=234238

namespace AuraEditor.Dialogs
{
    public class TutorialItem
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string Image { get; set; }
    }
    public sealed partial class TutorialDialog : ContentDialog
    {
        //private List<TutorialItem> TutorialFlipViewData = new List<TutorialItem>();
        public ObservableCollection<TutorialItem> TutorialFlipViewData { get; private set; }
        public TutorialDialog()
        {
            this.InitializeComponent();
        }
        private void TutorialDialog_Loaded(object sender, RoutedEventArgs e)
        {
            TutorialFlipViewData = new ObservableCollection<TutorialItem>();

            TutorialFlipViewData.Add(new TutorialItem()
            {
                Title = "Step 1. Arrange devices",
                Content = "Click the \"Device arrangement\" button,  then drag to rearrange your devices according to the actual relative position.",
                Image = "ms-appx:///Assets/Tutorial/asus_ac_arrangedevice_bg.png"
            });
            TutorialFlipViewData.Add(new TutorialItem()
            {
                Title = "Step 2. Create layers",
                Content = "Click or drag-select the lighting areas from the device display, and click the \"Set as a layer\" button to group these areas and create a layer.",
                Image = "ms-appx:///Assets/Tutorial/asus_ac_createlayers_bg.png"
            });
            TutorialFlipViewData.Add(new TutorialItem()
            {
                Title = "Step 3. Add lighting effects",
                Content = "Drag a lighting effect block from the left side to the timeline panel, then you can adjust the length, start and end time of the lighting effect block.\nYou can also click the \"Trigger event settings\" button of each layer to add a trigger effect.",
                Image = "ms-appx:///Assets/Tutorial/asus_ac_addlightingeffects_bg.png"
            });
            TutorialFlipViewData.Add(new TutorialItem()
            {
                Title = "Step 4. Manage lighting effect properties",
                Content = "Adjust the properties of the lighting effect block on the right panel.",
                Image = "ms-appx:///Assets/Tutorial/asus_ac_mamageeffect_bg.png"
            });
            TutorialFlipViewData.Add(new TutorialItem()
            {
                Title = "Step 5. Preview or apply the lighting profile",
                Content = "You can click the \"Play\" button to preview the adjusted lighting profile or click the \"Save & apply\"  button to save and apply the lighting profile to your devices.",
                Image = "ms-appx:///Assets/Tutorial/asus_ac_previeworapply_bg.png"
            });
            TutorialFlipView.ItemsSource = TutorialFlipViewData;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            WindowsPage.Self.TutorialDoneOrNot = true;
            this.Hide();
            MainPage.Self.CanShowDeviceUpdateDialog = true;
            MainPage.Self.ShowDeviceUpdateDialogOrNot();
        }

        private void SkipButton_Click(object sender, RoutedEventArgs e)
        {
            WindowsPage.Self.TutorialDoneOrNot = true;
            this.Hide();
            MainPage.Self.CanShowDeviceUpdateDialog = true;
            MainPage.Self.ShowDeviceUpdateDialogOrNot();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if(TutorialFlipView.SelectedIndex != 4)
                TutorialFlipView.SelectedIndex += 1;
        }

        private void TutorialFlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FlipView fv = sender as FlipView;

            if (fv.SelectedIndex == TutorialFlipView.Items.Count - 1)
            {
                SkipBtn.Visibility = Visibility.Collapsed;
                NextBtn.Visibility = Visibility.Collapsed;
                CloseBtn.Visibility = Visibility.Visible;
            }
            else
            {
                SkipBtn.Visibility = Visibility.Visible;
                NextBtn.Visibility = Visibility.Visible;
                CloseBtn.Visibility = Visibility.Collapsed;
            }
        }
    }
}
