using Windows.UI.Xaml;
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Controls.Primitives;
using AuraEditor.Common;
using Windows.UI.Xaml.Media;
using static AuraEditor.Common.Definitions;

namespace AuraEditor
{
    public sealed partial class MainPage : Page
    {
        public class TimelinePlayer
        {
            private Storyboard IconStoryboard;
            private DispatcherTimer TimerClock;
            private DateTime baseDateTime;
            private TextBlock ClockText;
            private ScrollViewer IconScrollViewer;
            private TranslateTransform IconTranslateTransform;
            private DoubleAnimation animation;

            public TimelinePlayer()
            {
                IconStoryboard = new Storyboard();
                IconStoryboard.Completed += IconStoryboard_Completed;
                
                ClockText = MainPageInstance.ClockText;
                IconScrollViewer = MainPageInstance.IconScrollViewer;
                IconTranslateTransform = MainPageInstance.IconTranslateTransform;

                TimerClock = new DispatcherTimer();
                TimerClock.Tick += Timer_Tick;
                TimerClock.Interval = new TimeSpan(0, 0, 0, 0, 10); // 10 ms

                InitializeAnimation();
            }
            private void InitializeAnimation()
            {
                animation = new DoubleAnimation();
                Storyboard.SetTargetProperty(animation, "X");
                Storyboard.SetTarget(animation, MainPageInstance.IconTranslateTransform);
                animation.EnableDependentAnimation = true;
            }
            public void Play()
            {
                AuraCreatorManager manager = AuraCreatorManager.Instance;
                double duration = manager.PlayTime;
                double from = 0;
                double to = manager.RightmostPosition;

                IconScrollViewer.Visibility = Visibility.Visible;
                baseDateTime = DateTime.Now;
                TimerClock.Start();
                StartStoryboard(duration, from, to);
            }
            public void OnZoomChange(double rate)
            {
                if (IconStoryboard.GetCurrentState() != ClockState.Active)
                    return;

                double currentTime = GetStoryCurrentTime();
                double duration = animation.Duration.TimeSpan.TotalMilliseconds;
                double leftTime = duration - currentTime;
                double from = IconTranslateTransform.X * rate;
                double to = AuraCreatorManager.Instance.RightmostPosition;

                StartStoryboard(leftTime, from, to);
            }
            public void Stop()
            {
                IconStoryboard.Stop();
                TimerClock.Stop();
                IconScrollViewer.Visibility = Visibility.Collapsed;
            }
            private void IconStoryboard_Completed(object sender, object e)
            {
                TimerClock.Stop();
                IconScrollViewer.Visibility = Visibility.Collapsed;
            }
            private void Timer_Tick(object sender, object e)
            {
                ClockText.Text = DateTime.Now.Subtract(baseDateTime).ToString("mm\\:ss\\.ff");
            }
            private double GetStoryCurrentTime()
            {
                Duration d = IconStoryboard.Duration;
                return IconStoryboard.GetCurrentTime().TotalMilliseconds;
            }
            private void StartStoryboard(double duration, double from, double to)
            {
                animation.Duration = TimeSpan.FromMilliseconds(duration);
                animation.From = from;
                animation.To = to;

                IconStoryboard.Stop();
                IconStoryboard.Children.Clear();
                IconStoryboard.Children.Add(animation);
                IconStoryboard.Begin();
            }
        }

        static TimelinePlayer _timelinePlayer;
        private int timelineZoomLevel;
        public int TimelineZoomLevel
        {
            set
            {
                if (timelineZoomLevel != value)
                {
                    int newSecondsPerTimeUnit = GetSecondsPerTimeUnitByLevel(value);
                    int oldSecondsPerTimeUnit = GetSecondsPerTimeUnitByLevel(timelineZoomLevel);
                    double rate = (double)oldSecondsPerTimeUnit / newSecondsPerTimeUnit;

                    _auraCreatorManager.SetTimeUnit(newSecondsPerTimeUnit);

                    if (_timelinePlayer != null)
                    {
                        _timelinePlayer.OnZoomChange(rate);
                    }

                    timelineZoomLevel = value;
                }
            }
        }
        public bool RepeatMode
        {
            get { return RepeatButton.IsChecked == true; }
        }
        
        private void InitializeTimelineStructure()
        {
            TimelineZoomLevel = 2;
            _timelinePlayer = new TimelinePlayer();
        }
        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            await(new ServiceViewModel()).AuraEditorTrigger();
            
            ScrollWindowToLeftTop();
            _timelinePlayer.Play();
        }
        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            _timelinePlayer.Stop();
        }
        private void TrackScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            ScrollViewer sv = sender as ScrollViewer;

            LayerScrollViewer.ChangeView(null, sv.VerticalOffset, null, true);
            ScaleScrollViewer.ChangeView(sv.HorizontalOffset, null, null, true);
        }
        private void IconScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            ScrollViewer sv = sender as ScrollViewer;

            LayerScrollViewer.ChangeView(null, sv.VerticalOffset, null, true);
            TrackScrollViewer.ChangeView(sv.HorizontalOffset, sv.VerticalOffset, null, true);
            ScaleScrollViewer.ChangeView(sv.HorizontalOffset, null, null, true);
        }
        private void ZoomSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (_auraCreatorManager != null)
                TimelineZoomLevel = (int)ZoomSlider.Value;
        }
        private void ScrollWindowToLeftTop()
        {
            TrackScrollViewer.ChangeView(0, 0, null, true);
            ScaleScrollViewer.ChangeView(0, null, null, true);
            LayerScrollViewer.ChangeView(null, 0, null, true);
            IconScrollViewer.ChangeView(0, 0, null, true);
        }
    }
}