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
                Title = "Step 1. Arrange your devices",
                Content = "Arrange your devices according to the actual relative position by pressing “Device arrangement” and drag and move the devices.",
                Image = "ms-appx:///Assets/Tutorial/asus_ac_arrangedevice_bg.png"
            });
            TutorialFlipViewData.Add(new TutorialItem()
            {
                Title = "Step 2. Create layers",
                Content = "Drag or click to select the lighting areas on the devices, and press “set as a layer ” to group the lighting areas and create a layer with a timeline.",
                Image = "ms-appx:///Assets/Tutorial/asus_ac_createlayers_bg.png"
            });
            TutorialFlipViewData.Add(new TutorialItem()
            {
                Title = "Step 3. Add lighting effects",
                Content = "Drag a lighting effect brick you want from the left side panel and put it on the timeline, and you’re able to modify the lengthand start time of the lighting effect brick. you can also press the “Trigger event” button on a layer to add an action with the trigger effects. ",
                Image = "ms-appx:///Assets/Tutorial/asus_ac_addlightingeffects_bg.png"
            });
            TutorialFlipViewData.Add(new TutorialItem()
            {
                Title = "Step 4. Manage the effect properties",
                Content = "Change the properties of the lighting effect brick on the right side panel.",
                Image = "ms-appx:///Assets/Tutorial/asus_ac_mamageeffect_bg.png"
            });
            TutorialFlipViewData.Add(new TutorialItem()
            {
                Title = "Step 5. Preview or apply your lighting file",
                Content = "You can press “Play” to preview the lighting file or press “Save and apply” to save the lighting file and apply it to your devices. ",
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

        private void DoneButton_Click(object sender, RoutedEventArgs e)
        {
            WindowsPage.Self.TutorialDoneOrNot = true;
            this.Hide();
            MainPage.Self.CanShowDeviceUpdateDialog = true;
            MainPage.Self.ShowDeviceUpdateDialogOrNot();
        }

        private void TutorialFlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FlipView fv = sender as FlipView;

            if (fv.SelectedIndex == TutorialFlipView.Items.Count - 1)
            {
                SkipBtn.Visibility = Visibility.Collapsed;
                NextBtn.Visibility = Visibility.Collapsed;
                DoneBtn.Visibility = Visibility.Visible;
            }
            else
            {
                SkipBtn.Visibility = Visibility.Visible;
                NextBtn.Visibility = Visibility.Visible;
                DoneBtn.Visibility = Visibility.Collapsed;
            }
        }
    }
}
