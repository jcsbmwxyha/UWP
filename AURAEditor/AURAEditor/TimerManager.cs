using Windows.UI.Xaml;
using System;
using Windows.UI.Xaml.Controls;

namespace AuraEditor
{
    public sealed partial class MainPage : Page
    {
        DispatcherTimer timeLineTimerClock = new DispatcherTimer();
        DateTime baseDateTime;

        bool EnableLog = true;

        private async void TimeLineTimer_Play(object sender, RoutedEventArgs e)
        {
            await(new ServiceViewModel()).AuraEditorTrigger();
            TimeLineStoryboard.Begin();
            timeLineTimerClock.Tick += Timer_Tick;
            timeLineTimerClock.Interval = new TimeSpan(0, 0, 0, 0, 10);
            baseDateTime = DateTime.Now;
            timeLineTimerClock.Start();

            TimeLineIconCanvas.Height = TimeLineStackPanel.ActualHeight + TimeLineScaleScrollViewer.ActualHeight;
            TimeLineIconCanvas.Width = TimeLineStackPanel.ActualWidth;

            TimeLineLayerScrollViewer.ChangeView(0, 0, null, true);
            TimeLineScaleScrollViewer.ChangeView(0, null, null, true);
            GroupScrollViewer.ChangeView(null, 0, null, true);

            TimeLineIconScrollViewer.ChangeView(0, 0, null, true);
            TimeLineIconScrollViewer.Visibility = Visibility.Visible;
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

        private void TimeLineTimer_Stop(object sender, RoutedEventArgs e)
        {
            TimeLineStoryboard.Stop();
            SpaceLineStoryboard.Stop();
            timeLineTimerClock.Stop();

            TimeLineIconScrollViewer.Visibility = Visibility.Collapsed;
        }

        private void Timer_Click(object sender, RoutedEventArgs e)
        {
            timeLineTimerClock.Tick += Timer_Tick;
            timeLineTimerClock.Interval = new TimeSpan(0, 0, 0, 0, 10);
            timeLineTimerClock.Start();
        }

        private void Timer_Tick(object sender, object e)
        {
            TimeLineTimerText.Text = DateTime.Now.Subtract(baseDateTime).ToString("mm\\:ss\\.ff");
        }
    }
}