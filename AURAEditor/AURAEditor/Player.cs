using Windows.UI.Xaml;
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using static AuraEditor.Common.Definitions;
using static AuraEditor.Common.ControlHelper;
using Windows.Storage;

namespace AuraEditor
{
    public sealed partial class MainPage : Page
    {
        public class TimelinePlayer
        {
            private Storyboard IconStoryboard;
            private DispatcherTimer TimerClock;
            private DateTime baseDateTime;
            private TextBlock clockText;
            private ScrollViewer iconScrollViewer;
            private TranslateTransform iconTranslateTransform;
            private DoubleAnimation animation;

            public TimelinePlayer()
            {
                IconStoryboard = new Storyboard();
                IconStoryboard.Completed += IconStoryboard_Completed;

                clockText = Self.ClockText;
                iconScrollViewer = Self.IconScrollViewer;
                iconTranslateTransform = Self.IconTranslateTransform;

                TimerClock = new DispatcherTimer();
                TimerClock.Tick += Timer_Tick;
                TimerClock.Interval = new TimeSpan(0, 0, 0, 0, 10); // 10 ms

                InitializeAnimation();
            }
            private void InitializeAnimation()
            {
                animation = new DoubleAnimation();
                Storyboard.SetTargetProperty(animation, "X");
                Storyboard.SetTarget(animation, iconTranslateTransform);
                animation.EnableDependentAnimation = true;
            }
            public void Play()
            {
                double duration = AuraLayerManager.Self.PlayTime;
                double from = 0;
                double to = AuraLayerManager.Self.RightmostPosition;

                iconScrollViewer.Visibility = Visibility.Visible;
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
                double from = iconTranslateTransform.X * rate;
                double to = AuraLayerManager.Self.RightmostPosition;

                StartStoryboard(leftTime, from, to);
            }
            public void Stop()
            {
                IconStoryboard.Stop();
                TimerClock.Stop();
                iconScrollViewer.Visibility = Visibility.Collapsed;
            }
            private void IconStoryboard_Completed(object sender, object e)
            {
                TimerClock.Stop();
                //clockText.Text = TimeSpan.FromMilliseconds((int)AuraLayerManager.Self.PlayTime).ToString("mm\\:ss\\.ff");
                clockText.Text = "00:00:00";
                iconScrollViewer.Visibility = Visibility.Collapsed;
            }
            private void Timer_Tick(object sender, object e)
            {
                // Even stop TimerClock, Timer_Tick() still have the chance to be called,
                // so we should ingnore it if timer is stopped.
                if (TimerClock.IsEnabled)
                    clockText.Text = DateTime.Now.Subtract(baseDateTime).ToString("mm\\:ss\\.ff");
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

        #region Layer Zoom level
        private int _oldLayerZoomLevel;
        private void LayerZoomSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (LayerManager == null)
                return;

            int newLevel = (int)LayerZoomSlider.Value;

            if (_oldLayerZoomLevel != newLevel)
            {
                int newSecondsPerTimeUnit = GetSecondsPerTimeUnitByLevel(newLevel);
                int oldSecondsPerTimeUnit = GetSecondsPerTimeUnitByLevel(_oldLayerZoomLevel);
                double rate = (double)oldSecondsPerTimeUnit / newSecondsPerTimeUnit;

                LayerManager.SetTimeUnit(newSecondsPerTimeUnit);

                if (_timelinePlayer != null)
                {
                    _timelinePlayer.OnZoomChange(rate);
                }

                _oldLayerZoomLevel = newLevel;
            }
        }
        private void PlusButton_Click(object sender, RoutedEventArgs e)
        {
            LayerZoomSlider.Value += 1;
        }
        private void MinusButton_Click(object sender, RoutedEventArgs e)
        {
            LayerZoomSlider.Value -= 1;
        }
        #endregion

        private void InitializePlayerStructure()
        {
            _timelinePlayer = new TimelinePlayer();
            LayerZoomSlider.Value = 5;
        }

        #region Jump to beginning or end
        public double TimelineScrollHorOffset
        {
            get { return (double)GetValue(ScrollHorOffseProperty); }
            set { SetValue(ScrollHorOffseProperty, (double)value); }
        }

        public static readonly DependencyProperty ScrollHorOffseProperty =
            DependencyProperty.Register("TimelineScrollHorOffset", typeof(double), typeof(MainPage),
                new PropertyMetadata(0, ScrollTimeLinePropertyChangedCallback));

        static private void ScrollTimeLinePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as MainPage).TrackScrollViewer.ChangeView((double)e.NewValue, null, null, true);
            (d as MainPage).ScaleScrollViewer.ChangeView((double)e.NewValue, null, null, true);
        }

        private void JumpToBeginningButton_Click(object sender, RoutedEventArgs e)
        {
            double source = ScaleScrollViewer.HorizontalOffset;
            double target = 0;
            AnimationStart(this, "TimelineScrollHorOffset", 200, source, target);
        }
        private void JumpToEndButton_Click(object sender, RoutedEventArgs e)
        {
            double source = ScaleScrollViewer.HorizontalOffset;
            double target = LayerManager.RightmostPosition;
            AnimationStart(this, "TimelineScrollHorOffset", 200, source, target);
        }
        #endregion

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            StorageFolder folder = await StorageFolder.GetFolderFromPathAsync("C:\\ProgramData\\ASUS\\AURA Creator\\script");
            StorageFile sf = await folder.CreateFileAsync("LastScript.xml", Windows.Storage.CreationCollisionOption.ReplaceExisting);
            await Windows.Storage.FileIO.WriteTextAsync(sf, PrintScriptXml(false));

            await (new ServiceViewModel()).AuraEditorTrigger();

            ScrollWindowToLeftTop();
            _timelinePlayer.Play();
        }
        private void TitleScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            ScrollViewer sv = sender as ScrollViewer;
            TrackScrollViewer.ChangeView(null, sv.VerticalOffset, null, true);
            BackgroundScrollViewer.ChangeView(null, sv.VerticalOffset, null, true);
        }
        private void TrackScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            ScrollViewer sv = sender as ScrollViewer;
            TitleScrollViewer.ChangeView(null, sv.VerticalOffset, null, true);
            BackgroundScrollViewer.ChangeView(null, sv.VerticalOffset, null, true);
            ScaleScrollViewer.ChangeView(sv.HorizontalOffset, null, null, true);
        }
        private void IconScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            ScrollViewer sv = sender as ScrollViewer;
            TitleScrollViewer.ChangeView(null, sv.VerticalOffset, null, true);
            TrackScrollViewer.ChangeView(sv.HorizontalOffset, sv.VerticalOffset, null, true);
            ScaleScrollViewer.ChangeView(sv.HorizontalOffset, null, null, true);
        }
        private void ScrollWindowToLeftTop()
        {
            TrackScrollViewer.ChangeView(0, 0, null, true);
            ScaleScrollViewer.ChangeView(0, null, null, true);
            TitleScrollViewer.ChangeView(null, 0, null, true);
            IconScrollViewer.ChangeView(0, 0, null, true);
        }
    }
}