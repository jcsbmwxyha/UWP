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
            private Storyboard PlayerCursorStoryboard;
            private DoubleAnimation animation;
            private PlayerModel pm;

            public TimelinePlayer(PlayerModel pm)
            {
                this.pm = pm;

                PlayerCursorStoryboard = new Storyboard();
                PlayerCursorStoryboard.Completed += (s, e) => pm.Position = 0;

                animation = new DoubleAnimation();
                animation.EnableDependentAnimation = true;
                Storyboard.SetTarget(animation, Self);
                Storyboard.SetTargetProperty(animation, "TimelineCursorPosition");
            }

            public void Play()
            {
                double from = pm.Position;
                double to = AuraLayerManager.Self.RightmostPosition;
                double duration = AuraLayerManager.PositionToTime(to) - AuraLayerManager.PositionToTime(from);

                StartStoryboard(duration, from, to);
            }
            public void Pause()
            {
                PlayerCursorStoryboard.Pause();
            }
            public void Stop()
            {
                PlayerCursorStoryboard.Stop();
                pm.Position = 0;
            }
            public double GetCursorPosition()
            {
                return pm.Position;
            }

            public void OnLevelChanged(double rate)
            {
                pm.Position = pm.Position * rate;

                if (PlayerCursorStoryboard.GetCurrentState() != ClockState.Active)
                    return;

                double currentTime = GetStoryCurrentTime();
                double duration = animation.Duration.TimeSpan.TotalMilliseconds;
                double leftTime = duration - currentTime;
                double from = pm.Position;
                double to = AuraLayerManager.Self.RightmostPosition;

                StartStoryboard(leftTime, from, to);
            }
            private double GetStoryCurrentTime()
            {
                Duration d = PlayerCursorStoryboard.Duration;
                return PlayerCursorStoryboard.GetCurrentTime().TotalMilliseconds;
            }
            private void StartStoryboard(double duration, double from, double to)
            {
                animation.Duration = TimeSpan.FromMilliseconds(duration);
                animation.From = from;
                animation.To = to;

                PlayerCursorStoryboard.Stop();
                PlayerCursorStoryboard.Children.Clear();
                PlayerCursorStoryboard.Children.Add(animation);
                PlayerCursorStoryboard.Begin();
            }
        }

        public TimelinePlayer Player;
        PlayerModel playerModel;

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
            playerModel = new PlayerModel();

            Player = new TimelinePlayer(playerModel);
            ClockText.DataContext = playerModel;
            PlayerCursor_Head.DataContext = playerModel;
            PlayerCursor_Tail.DataContext = playerModel;

            LayerZoomSlider.Value = 2;
        }

        #region Cursor position property
        public double TimelineCursorPosition
        {
            get { return (double)GetValue(CursorPositionProperty); }
            set { SetValue(CursorPositionProperty, (double)value); }
        }

        public static readonly DependencyProperty CursorPositionProperty =
            DependencyProperty.Register("TimelineCursorPosition", typeof(double), typeof(MainPage),
                new PropertyMetadata(0, CursorChangedCallback));

        static private void CursorChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // When player stop storyboard, it will send interger 0 to this function.
            // It will cause crash becasue interger can not convert to double.
            if (e.NewValue is Int32)
                (d as MainPage).playerModel.Position = 0;
            else
                (d as MainPage).playerModel.Position = (double)e.NewValue;
        }
        #endregion

        #region Jump to beginning or end
        public double TimelineScrollHorOffset
        {
            get { return (double)GetValue(ScrollHorOffsetProperty); }
            set { SetValue(ScrollHorOffsetProperty, (double)value); }
        }

        public static readonly DependencyProperty ScrollHorOffsetProperty =
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
            if (PlayerCursorTranslateTransform.X > LayerManager.RightmostPosition)
                return;

            StorageFolder folder = await StorageFolder.GetFolderFromPathAsync("C:\\ProgramData\\ASUS\\AURA Creator");
            StorageFile sf = await folder.CreateFileAsync("LastScript.xml", Windows.Storage.CreationCollisionOption.ReplaceExisting);
            await Windows.Storage.FileIO.WriteTextAsync(sf, GetLastScript(false));

            long StartTime = (long)AuraLayerManager.PositionToTime(Player.GetCursorPosition());
            await (new ServiceViewModel()).AuraEditorTrigger(StartTime);

            //ScrollWindowToBeginning();
            Player.Play();
        }
        private async void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            Player.Pause();
            await(new ServiceViewModel()).AuraEditorStopEngine();
        }
        private async void StopButton_Click(object sender, RoutedEventArgs e)
        {
            Player.Stop();
            await(new ServiceViewModel()).AuraEditorStopEngine();
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
        private void ScrollWindowToBeginning()
        {
            TrackScrollViewer.ChangeView(0, null, null, true);
            ScaleScrollViewer.ChangeView(0, null, null, true);
        }
    }
}
