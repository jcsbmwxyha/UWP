using Windows.UI.Xaml;
using System;
using Windows.UI.Xaml.Controls;

namespace AuraEditor
{
    public sealed partial class MainPage : Page
    {
        public bool RepeatMode
        {
            get
            {
                if (RepeatButton.IsChecked == true)
                    return true;
                else
                    return false;
            }
        }
        DispatcherTimer timelineTimerClock = new DispatcherTimer();
        DateTime baseDateTime;

        private async void TimelineTimer_Play(object sender, RoutedEventArgs e)
        {
            await(new ServiceViewModel()).AuraEditorTrigger();
            TimelineStoryboard.Begin();
            timelineTimerClock.Tick += Timer_Tick;
            timelineTimerClock.Interval = new TimeSpan(0, 0, 0, 0, 10);
            baseDateTime = DateTime.Now;
            timelineTimerClock.Start();

            TimelineIconCanvas.Height = TimelineStackPanel.ActualHeight + TimelineScaleScrollViewer.ActualHeight;
            TimelineIconCanvas.Width = TimelineStackPanel.ActualWidth;

            TimelineLayerScrollViewer.ChangeView(0, 0, null, true);
            TimelineScaleScrollViewer.ChangeView(0, null, null, true);
            LayerScrollViewer.ChangeView(null, 0, null, true);

            TimelineIconScrollViewer.ChangeView(0, 0, null, true);
            TimelineIconScrollViewer.Visibility = Visibility.Visible;
        }

        private async void CreateXML(string xmlstring)
        {
            var doc2 = new Windows.Data.Xml.Dom.XmlDocument();
            doc2.LoadXml(xmlstring);

            // save xml to a file
            var file = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFileAsync("Space.xml",
                Windows.Storage.CreationCollisionOption.ReplaceExisting);
            await doc2.SaveToFileAsync(file);
        }

        private void TimelineTimer_Stop(object sender, RoutedEventArgs e)
        {
            TimelineStoryboard.Stop();
            SpaceLineStoryboard.Stop();
            timelineTimerClock.Stop();

            TimelineIconScrollViewer.Visibility = Visibility.Collapsed;
        }

        private void Timer_Click(object sender, RoutedEventArgs e)
        {
            timelineTimerClock.Tick += Timer_Tick;
            timelineTimerClock.Interval = new TimeSpan(0, 0, 0, 0, 10);
            timelineTimerClock.Start();
        }

        private void Timer_Tick(object sender, object e)
        {
            TimelineTimerText.Text = DateTime.Now.Subtract(baseDateTime).ToString("mm\\:ss\\.ff");
        }
    }
}