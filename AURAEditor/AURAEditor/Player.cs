using Windows.UI.Xaml;
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using static AuraEditor.Common.Definitions;
using static AuraEditor.Common.ControlHelper;
using Windows.Storage;
using AuraEditor.Models;

namespace AuraEditor
{
    public sealed partial class MainPage : Page
    {
        public class TimelinePlayer
        {
            private Storyboard iconStoryboard;
            private DoubleAnimation animation;
            private PlayerModel model;

            public TimelinePlayer(PlayerModel pm)
            {
                iconStoryboard = Self.IconStoryboard;
                iconStoryboard.Completed += (s, e) => model.Position = 0;
                animation = Self.IconAnimation;
                this.model = pm;
            }

            public void Play()
            {
                double from = model.Position;
                double to = AuraLayerManager.Self.RightmostPosition;
                double duration = AuraLayerManager.PositionToTime(to) - AuraLayerManager.PositionToTime(from);

                StartStoryboard(duration * 1000, from, to);
            }
            public void Pause()
            {
                iconStoryboard.Pause();
            }
            public void Stop()
            {
                iconStoryboard.Stop();
                model.Position = 0;
            }
            public double GetPointerPosition()
            {
                return model.Position;
            }

            public void OnLevelChanged(double rate)
            {
                model.Position = model.Position * rate;

                if (iconStoryboard.GetCurrentState() != ClockState.Active)
                    return;

                double currentTime = GetStoryCurrentTime();
                double duration = animation.Duration.TimeSpan.TotalMilliseconds;
                double leftTime = duration - currentTime;
                double from = model.Position;
                double to = AuraLayerManager.Self.RightmostPosition;

                StartStoryboard(leftTime, from, to);
            }
            private double GetStoryCurrentTime()
            {
                Duration d = iconStoryboard.Duration;
                return iconStoryboard.GetCurrentTime().TotalMilliseconds;
            }
            private void StartStoryboard(double duration, double from, double to)
            {
                animation.Duration = TimeSpan.FromMilliseconds(duration);
                animation.From = from;
                animation.To = to;

                iconStoryboard.Stop();
                iconStoryboard.Children.Clear();
                iconStoryboard.Children.Add(animation);
                iconStoryboard.Begin();
            }
        }

        public TimelinePlayer Player;

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

                if (Player != null)
                {
                    Player.OnLevelChanged(rate);
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
            PlayerModel model = new PlayerModel();

            Player = new TimelinePlayer(model);
            ClockText.DataContext = model;
            MyPlayerIcon.DataContext = model;

            LayerZoomSlider.Value = 2;
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
            if (IconTranslateTransform.X > LayerManager.RightmostPosition)
                return;

            StorageFolder folder = await StorageFolder.GetFolderFromPathAsync("C:\\ProgramData\\ASUS\\AURA Creator");
            StorageFile sf = await folder.CreateFileAsync("LastScript.xml", Windows.Storage.CreationCollisionOption.ReplaceExisting);
            await Windows.Storage.FileIO.WriteTextAsync(sf, GetLastScript(false));

            long StartTime = 0;
            await (new ServiceViewModel()).AuraEditorTrigger(StartTime);

            //ScrollWindowToBeginning();
            Player.Play();
        }
        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            Player.Pause();
        }
        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            Player.Stop();
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
        private void ScrollWindowToBeginning()
        {
            TrackScrollViewer.ChangeView(0, null, null, true);
            ScaleScrollViewer.ChangeView(0, null, null, true);
        }
    }
}
