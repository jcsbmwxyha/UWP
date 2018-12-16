using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Xml;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Shapes;
using AuraEditor.Models;
using static AuraEditor.Common.ControlHelper;
using static AuraEditor.Common.Definitions;
using static AuraEditor.Common.EffectHelper;
using static AuraEditor.Common.MetroEventSource;
using static AuraEditor.Common.StorageHelper;
using static AuraEditor.Common.XmlHelper;
using static AuraEditor.Pages.SpacePage;

namespace AuraEditor.Pages
{
    public sealed partial class LayerPage : Page
    {
        static public LayerPage Self;

        public LayerPage()
        {
            this.InitializeComponent();
            Self = this;
            m_EffectInfoFrame = MainPage.Self.EffectInfoFrame;

            Layers = new ObservableCollection<Layer>();
            LayerListView.ItemsSource = Layers;
            LayerBackgroundItemsControl.ItemsSource = Layers;
            Layers.CollectionChanged += LayersChanged;
            InitializePlayerStructure();
        }

        private Frame m_EffectInfoFrame;

        private Layer _checkedLayer;
        public Layer CheckedLayer
        {
            get
            {
                return _checkedLayer;
            }
            set
            {
                if (_checkedLayer == value)
                    return;

                if (_checkedLayer != null)
                {
                    _checkedLayer.VisualState = "Normal";
                }

                if (value != null)
                {
                    value.VisualState = "Checked";
                    SpacePage.Self.WatchLayer(value);

                    if (CheckedEffect == null || CheckedEffect.Layer != value)
                    {
                        var find = value.GetFirstOnRightSide(0);

                        if (find != null)
                            CheckedEffect = find;
                        else
                            CheckedEffect = null;
                    }
                }
                else
                {
                    CheckedEffect = null;
                }

                _checkedLayer = value;
            }
        }

        private TimelineEffect _checkedEffect;
        public TimelineEffect CheckedEffect
        {
            get
            {
                return _checkedEffect;
            }
            set
            {
                if (_checkedEffect == value)
                    return;

                if (value == null)
                {
                    if (_checkedEffect != null)
                    {
                        _checkedEffect.IsChecked = false;
                        _checkedEffect = null;
                        m_EffectInfoFrame.Navigate(typeof(EffectInfoPage));
                    }
                }
                else
                {
                    _checkedEffect = value;
                    value.IsChecked = true;
                    m_EffectInfoFrame.Navigate(typeof(EffectInfoPage), _checkedEffect);
                    NeedSave = true;
                }
            }
        }
        public TimelineEffect CopiedEffect;

        public double PlayTime
        {
            get
            {
                TimelineEffect effect = GetRightmostEffect();

                return (effect != null) ? effect.StartTime + effect.DurationTime : 0;
            }
        }
        public double RightmostPosition
        {
            get
            {
                TimelineEffect effect = GetRightmostEffect();

                return (effect != null) ? effect.Right : 0;
            }
        }

        #region -- Layers --
        public ObservableCollection<Layer> Layers { get; set; }

        public void AddLayer(Layer layer)
        {
            Layers.Add(layer);
        }
        public void RemoveLayer(Layer layer)
        {
            Layers.Remove(layer);
        }
        private void LayersChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            CheckedLayer = null;

            Layer layer;
            int layerIndex;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                    layer = e.OldItems[0] as Layer;
                    TrackStackPanel.Children.Remove(layer.UI_Track);
                    break;
                case NotifyCollectionChangedAction.Add:
                    layer = e.NewItems[0] as Layer;
                    layerIndex = e.NewStartingIndex;
                    TrackStackPanel.Children.Insert(layerIndex, layer.UI_Track);
                    break;
            }

            for (int i = 0; i < Layers.Count; i++)
            {
                if (Layers[i].Name == "")
                    Layers[i].Name = "Layer " + (i + 1).ToString();
            }

            SpacePage.Self.SetSpaceStatus(SpaceStatus.Init);
            TrackCanvas.Height = Layers.Count * 52;
        }
        public int GetLayerCount()
        {
            return Layers.Count;
        }
        public void Clean()
        {
            CheckedLayer = null;
            TrackStackPanel.Children.Clear();
            Layers.Clear();
        }
        public void ClearTypeData(int deviceType)
        {
            foreach (var layer in Layers)
            {
                layer.GetZoneDictionary().Remove(deviceType);
            }
        }
        public void ClearTypeData(string deviceTypeName)
        {
            int type = GetTypeByTypeName(deviceTypeName);
            foreach (var layer in Layers)
            {
                layer.GetZoneDictionary().Remove(type);
            }
        }
        #endregion

        public void UpdateSupportLine(double align)
        {
            if (align != 0)
            {
                SupportLine.Visibility = Visibility.Visible;
                SupportLineTranslateTransform.X = align - ScaleScrollViewer.HorizontalOffset;
            }
            else
            {
                SupportLine.Visibility = Visibility.Collapsed;
            }
        }

        private void LayerScrollGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var width = LayerScrollGrid.ActualWidth - 10;

            if (width > 0)
            {
                var v1 = FindControl<ScrollBar>(TrackScrollViewer, typeof(ScrollBar), "HorizontalScrollBar");
                v1.Width = LayerScrollGrid.ActualWidth - 10;
            }
        }

        #region -- Player --
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
                double to = LayerPage.Self.RightmostPosition;
                double duration = LayerPage.PositionToTime(to) - LayerPage.PositionToTime(from);

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
                double to = LayerPage.Self.RightmostPosition;

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

        private void InitializePlayerStructure()
        {
            playerModel = new PlayerModel();

            Player = new TimelinePlayer(playerModel);
            ClockText.DataContext = playerModel;
            PlayerCursor_Head.DataContext = playerModel;
            PlayerCursor_Tail.DataContext = playerModel;

            LayerZoomSlider.Value = 2;
        }

        public double TimelineCursorPosition
        {
            get { return (double)GetValue(CursorPositionProperty); }
            set { SetValue(CursorPositionProperty, (double)value); }
        }

        public static readonly DependencyProperty CursorPositionProperty =
            DependencyProperty.Register("TimelineCursorPosition", typeof(double), typeof(LayerPage),
                new PropertyMetadata(0, CursorChangedCallback));

        static private void CursorChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // When player stop storyboard, it will send interger 0 to this function.
            // It will cause crash becasue interger can not convert to double.
            if (e.NewValue is Int32)
                (d as LayerPage).playerModel.Position = 0;
            else
                (d as LayerPage).playerModel.Position = (double)e.NewValue;
        }

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (PlayerCursorTranslateTransform.X > RightmostPosition)
                return;

            StorageFolder folder = await StorageFolder.GetFolderFromPathAsync("C:\\ProgramData\\ASUS\\AURA Creator");
            StorageFile sf = await folder.CreateFileAsync("LastScript.xml", Windows.Storage.CreationCollisionOption.ReplaceExisting);
            await Windows.Storage.FileIO.WriteTextAsync(sf, MainPage.Self.GetLastScript(false));
            Log.Debug("[PlayButton] Save LastScript : " + sf.Path);

            long StartTime = (long)PositionToTime(Player.GetCursorPosition());

            Log.Debug("[PlayButton] Bef AuraEditorTrigger");
            await (new ServiceViewModel()).AuraEditorTrigger(StartTime);
            Log.Debug("[PlayButton] Aft AuraEditorTrigger");

            //ScrollWindowToBeginning();
            Player.Play();
        }
        private async void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            Player.Pause();
            Log.Debug("[PauseButton] Bef AuraEditorStopEngine");
            await (new ServiceViewModel()).AuraEditorStopEngine();
            Log.Debug("[PauseButton] Aft AuraEditorStopEngine");
        }
        private async void StopButton_Click(object sender, RoutedEventArgs e)
        {
            Player.Stop();
            await (new ServiceViewModel()).AuraEditorStopEngine();
        }
        #endregion

        #region -- Layer Zoom level --
        private int _oldLayerZoomLevel;
        private void LayerZoomSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (Self == null)
                return;

            int newLevel = (int)LayerZoomSlider.Value;

            if (_oldLayerZoomLevel != newLevel)
            {
                int newSecondsPerTimeUnit = GetSecondsPerTimeUnitByLevel(newLevel);
                int oldSecondsPerTimeUnit = GetSecondsPerTimeUnitByLevel(_oldLayerZoomLevel);
                double rate = (double)oldSecondsPerTimeUnit / newSecondsPerTimeUnit;

                SetTimeUnit(newSecondsPerTimeUnit);

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

        #region -- Timeline Scale --
        static public int SecondsPerTimeUnit; // TimeUnit : the seconds between two long lines
        static public double PixelsPerSecond { get { return PixelsPerTimeUnit / SecondsPerTimeUnit; } }

        static public double PositionToTime(double position)
        {
            return (position / PixelsPerSecond) * 1000;
        }
        public void SetTimeUnit(int newSecondsPerTimeUnit)
        {
            double rate = (double)SecondsPerTimeUnit / newSecondsPerTimeUnit;

            SecondsPerTimeUnit = newSecondsPerTimeUnit;
            DrawTimelineScale();

            foreach (var layer in Layers)
            {
                foreach (var effect in layer.TimelineEffects)
                {
                    effect.Left = effect.Left * rate;
                    effect.Width = effect.Width * rate;
                }
            }
        }
        private TimelineEffect GetRightmostEffect()
        {
            double position = 0;
            double rightmostPosition = 0;
            TimelineEffect rightmostEffect = null;

            foreach (Layer layer in Layers)
            {
                foreach (var effect in layer.TimelineEffects)
                {
                    position = effect.Left + effect.Width;

                    if (position > rightmostPosition)
                    {
                        rightmostPosition = position;
                        rightmostEffect = effect;
                    }
                }
            }
            return rightmostEffect;
        }
        public double[] GetAlignPositions(TimelineEffect eff)
        {
            Layer layer = eff.Layer;
            List<double> result = new List<double>();
            int i = Layers.IndexOf(layer);

            result.Add(Player.GetCursorPosition());
            result.AddRange(Layers[i].GetHeadAndTailPositions(eff));
            if (i > 0)
                result.AddRange(Layers[i - 1].GetHeadAndTailPositions(null));
            if (i < Layers.Count - 1)
                result.AddRange(Layers[i + 1].GetHeadAndTailPositions(null));
            return result.ToArray();
        }

        private void DrawTimelineScale()
        {
            TimeSpan ts = new TimeSpan(0, 0, SecondsPerTimeUnit);
            TimeSpan interval = new TimeSpan(0, 0, SecondsPerTimeUnit);
            int minimumScaleUnitLength = (int)(PixelsPerTimeUnit / 2);
            int width = (int)ScaleCanvas.ActualWidth;
            int height = (int)ScaleCanvas.ActualHeight;
            int y1_short = (int)(height / 1.5);
            int y1_long = height / 2;
            double y2 = height;
            int linePerTimeUnit = (int)(PixelsPerTimeUnit / minimumScaleUnitLength);
            int totalLineCount = width / minimumScaleUnitLength;

            ScaleCanvas.Children.Clear();
            ScaleCanvas.Children.Add(PlayerCursor_Head);

            for (int i = 1; i < totalLineCount; i++)
            {
                int x = minimumScaleUnitLength * i;
                int y1;

                if (i % linePerTimeUnit == 0)
                {
                    y1 = y1_long;

                    CompositeTransform ct = new CompositeTransform
                    {
                        TranslateX = x + 10,
                        TranslateY = 5
                    };

                    TextBlock tb = new TextBlock
                    {
                        Text = ts.ToString("mm\\:ss"),
                        RenderTransform = ct,
                        Foreground = new SolidColorBrush(Colors.White)
                    };

                    ScaleCanvas.Children.Add(tb);
                    ts = ts.Add(interval);
                }
                else
                    y1 = y1_short;

                Line line = new Line
                {
                    X1 = x,
                    Y1 = y1,
                    X2 = x,
                    Y2 = y2,
                    Stroke = new SolidColorBrush(Colors.White)
                };

                ScaleCanvas.Children.Add(line);
            }
        }
        #endregion

        #region -- Scroll Event --
        public double TimelineScrollHorOffset
        {
            get { return (double)GetValue(ScrollHorOffsetProperty); }
            set { SetValue(ScrollHorOffsetProperty, (double)value); }
        }

        public static readonly DependencyProperty ScrollHorOffsetProperty =
            DependencyProperty.Register("TimelineScrollHorOffset", typeof(double), typeof(LayerPage),
                new PropertyMetadata(0, ScrollTimeLinePropertyChangedCallback));

        static private void ScrollTimeLinePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as LayerPage).TrackScrollViewer.ChangeView((double)e.NewValue, null, null, true);
            (d as LayerPage).ScaleScrollViewer.ChangeView((double)e.NewValue, null, null, true);
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
            double target = RightmostPosition;
            AnimationStart(this, "TimelineScrollHorOffset", 200, source, target);
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
        #endregion

        #region -- Trash Can --
        private void LayerListView_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            Layer layer = e.Items[0] as Layer;
            e.Data.Properties.Add("layer", layer);
        }
        private void TrashCanButton_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data == null)
                return;

            e.DragUIOverride.IsCaptionVisible = false;
            e.DragUIOverride.IsGlyphVisible = false;

            var pair = e.Data.Properties.FirstOrDefault();
            Layer layer = pair.Value as Layer;
            if (layer != null)
                e.AcceptedOperation = DataPackageOperation.Copy;
        }
        private void TrashCanButton_Drop(object sender, DragEventArgs e)
        {
            var pair = e.Data.Properties.FirstOrDefault();
            Layer layer = pair.Value as Layer;
            RemoveLayer(layer);
            SpacePage.Self.SetSpaceStatus(SpaceStatus.Init);
            MainPage.Self.SelectedEffect = null;
            NeedSave = true;
        }
        private void TrashCanButton_Click(object sender, RoutedEventArgs e)
        {
            Layer layer = CheckedLayer;
            if (layer != null)
            {
                RemoveLayer(layer);
                SpacePage.Self.SetSpaceStatus(SpaceStatus.Init);
                MainPage.Self.SelectedEffect = null;
                NeedSave = true;
            }
        }
        #endregion

        public XmlNode ToXmlNodeForUserData()
        {
            XmlNode layersNode = CreateXmlNode("layers");

            foreach (var layer in Layers)
            {
                layersNode.AppendChild(layer.ToXmlNodeForUserData());
            }

            return layersNode;
        }
    }
}
