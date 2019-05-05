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
using AuraEditor.Common;
using AuraEditor.ViewModels;
using Windows.UI.Input;
using Windows.Foundation;
using Windows.UI.Xaml.Input;
using Windows.System.Threading;
using Windows.UI.Core;
using AuraEditor.UserControls;

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

            Layers = new ObservableCollection<LayerModel>();
            LayerListView.ItemsSource = Layers;
            LayerBackgroundItemsControl.ItemsSource = Layers;
            Layers.CollectionChanged += LayersChanged;
            InitializeCursor();
            playerModel = new PlayerModel();

            TimeTextBlockCollection = new List<TextBlock>();
            TimelineScaleInitialize();
            LayerZoomSlider.Value = 2;
        }

        private Frame m_EffectInfoFrame;
        private PlayerModel playerModel;

        private LayerModel _checkedLayer;
        public LayerModel CheckedLayer
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
                        var find = value.GetFirstBehindPosition(0);

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

        private EffectLineViewModel _checkedEffect;
        public EffectLineViewModel CheckedEffect
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
                    m_EffectInfoFrame.Navigate(typeof(EffectInfoPage), _checkedEffect.Model.Info);
                    CheckedLayer = value.Layer;
                    NeedSave = true;
                }
            }
        }
        public EffectLineViewModel CopiedEffect;

        public double PlayTime
        {
            get
            {
                EffectLineViewModel effect = GetRightmostEffect();

                return (effect != null) ? effect.StartTime + effect.DurationTime : 0;
            }
        }
        public double RightmostPosition
        {
            get
            {
                EffectLineViewModel effect = GetRightmostEffect();

                return (effect != null) ? effect.Right : 0;
            }
        }
        private EffectLineViewModel GetRightmostEffect()
        {
            EffectLineViewModel result = null;
            double max = 0;

            foreach (LayerModel layer in Layers)
            {
                EffectLineViewModel eff = layer.GetRightmostEffect();
                if (eff != null && eff.Right > max)
                {
                    result = eff;
                }
            }
            return result;
        }
        private List<TextBlock> TimeTextBlockCollection;

        #region -- Layers --
        private LayerModel _oldRemovedLayer;
        private int _oldRemovedIndex;
        public ObservableCollection<LayerModel> Layers { get; set; }
        public void AddLayer(LayerModel layer)
        {
            int index = Layers.IndexOf(layer);
            Layers.Insert(0, layer);
            ReUndoManager.Store(new AddLayerCommand(layer, index));
        }
        public void RemoveLayer(LayerModel layer)
        {
            int index = Layers.IndexOf(layer);
            Layers.Remove(layer);
            ReUndoManager.Store(new DeleteLayerCommand(layer, index));
        }
        private void LayersChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            LayerModel layer;
            int layerIndex;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                    layer = e.OldItems[0] as LayerModel;
                    _oldRemovedLayer = layer;
                    _oldRemovedIndex = e.OldStartingIndex;
                    break;
                case NotifyCollectionChangedAction.Add:
                    layer = e.NewItems[0] as LayerModel;
                    layerIndex = e.NewStartingIndex;
                    if (layer.Equals(_oldRemovedLayer))
                    {
                        ReUndoManager.Store(new SwapLayerCommand(_oldRemovedIndex, layerIndex));
                    }
                    CheckedLayer = layer;
                    break;
            }

            for (int i = 0; i < Layers.Count; i++)
            {
                if (Layers[i].Name == "")
                    Layers[i].Name = "Layer " + (i + 1).ToString();
            }

            SpacePage.Self.GoToBlankEditing();
            TrackCanvas.Height = Layers.Count * 52;
        }

        public int GetLayerCount()
        {
            return Layers.Count;
        }
        public void Clean()
        {
            CheckedLayer = null;
            Layers.Clear();
        }
        public void ClearDeviceData(int deviceType)
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
            double linePosition = align - ScaleScrollViewer.HorizontalOffset;

            if (linePosition >= 0)
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

        #region -- Cursor --
        private Storyboard cursorStoryboard;
        private DoubleAnimation cursorAnimation;

        private void InitializeCursor()
        {
            cursorStoryboard = new Storyboard();
            cursorStoryboard.Completed += CursorStoryboardCompleted;
            cursorAnimation = new DoubleAnimation();
            cursorAnimation.EnableDependentAnimation = true;
            Storyboard.SetTarget(cursorAnimation, Self);
            Storyboard.SetTargetProperty(cursorAnimation, "TimelineCursorPosition");
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
            {
                return; // Stop at current position
                // (d as LayerPage).playerModel.Position = 0;
            }
            else
                (d as LayerPage).playerModel.Position = (double)e.NewValue;
        }
        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (playerModel.Position >= RightmostPosition)
                return;

            Log.Debug("[PlayButton] Clicked");
            StorageFolder localfolder = ApplicationData.Current.LocalFolder;
            string scriptString = MainPage.Self.GetLastScript();

            StorageFile lastScriptSF = await localfolder.CreateFileAsync("LastScript.xml", Windows.Storage.CreationCollisionOption.ReplaceExisting);
            await Windows.Storage.FileIO.WriteTextAsync(lastScriptSF, scriptString);
            StorageFile playSF = await localfolder.CreateFileAsync("LastPlayScript.xml", Windows.Storage.CreationCollisionOption.ReplaceExisting);
            await Windows.Storage.FileIO.WriteTextAsync(playSF, scriptString);

            Log.Debug("[PlayButton] Save LastScript successfully : " + lastScriptSF.Path);

            long StartTime = (long)PositionToTime(playerModel.Position);

            Log.Debug("[PlayButton] Bef AuraEditorTrigger");
            Log.Debug("[PlayButton] StartTime : " + StartTime.ToString());
            await (new ServiceViewModel()).AuraEditorTrigger(StartTime);
            Log.Debug("[PlayButton] Aft AuraEditorTrigger");

            //ScrollWindowToBeginning();

            playerModel.IsPlaying = true;

            TrackScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            TrackScrollViewer.HorizontalScrollMode = ScrollMode.Disabled;

            if (playerModel.Position < TrackScrollViewer.HorizontalOffset)
            {
                TrackScrollViewer.ChangeView(playerModel.Position, null, null, true);
                playerModel.playerOffset = playerModel.Position + TrackScrollViewer.ActualWidth;
            }
            else
                playerModel.playerOffset = TrackScrollViewer.HorizontalOffset + TrackScrollViewer.ActualWidth;

            double from = playerModel.Position;
            double to = RightmostPosition;
            double duration = LayerPage.PositionToTime(to) - LayerPage.PositionToTime(from);
            StartCursorStoryboard(duration, from, to);
        }
        private async void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            cursorStoryboard.Stop();
            playerModel.IsPlaying = false;

            TrackScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
            TrackScrollViewer.HorizontalScrollMode = ScrollMode.Enabled;

            Log.Debug("[PauseButton] Bef AuraEditorStopEngine");
            await (new ServiceViewModel()).AuraEditorStopEngine();
            Log.Debug("[PauseButton] Aft AuraEditorStopEngine");
        }

        private async void CursorStoryboardCompleted(object sender, object e)
        {
            Log.Debug("[Player] Completed");
            playerModel.IsPlaying = false;
            playerModel.Position = 0;

            // Blank mode
            StorageFolder localfolder = ApplicationData.Current.LocalFolder;
            StorageFile localsf = await localfolder.CreateFileAsync("LastScript.xml", Windows.Storage.CreationCollisionOption.ReplaceExisting);
            await Windows.Storage.FileIO.WriteTextAsync(localsf, "<root><header>AURA_Creator</header><version>1.0</version><effectProvider><period key=\"true\">0</period><queue /></effectProvider><viewport /><effectList /></root>");

            long StartTime = (long)PositionToTime(playerModel.Position);

            TrackScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
            TrackScrollViewer.HorizontalScrollMode = ScrollMode.Enabled;
            TrackScrollViewer.ChangeView(0, null, null, true);

            Log.Debug("[CursorStoryboardCompleted] Bef AuraEditorTrigger");
            await (new ServiceViewModel()).AuraEditorTrigger(0);
            Log.Debug("[CursorStoryboardCompleted] Aft AuraEditorTrigger");
        }
        public void ChangeCursorPosition(double rate)
        {
            playerModel.Position = playerModel.Position * rate;

            if (cursorStoryboard.GetCurrentState() == ClockState.Active)
            {
                double currentTime = GetStoryCurrentTime();
                double duration = cursorAnimation.Duration.TimeSpan.TotalMilliseconds;
                double leftTime = duration - currentTime;
                double from = playerModel.Position;
                double to = LayerPage.Self.RightmostPosition;
                StartCursorStoryboard(leftTime, from, to);
            }
        }
        private double GetStoryCurrentTime()
        {
            Duration d = cursorStoryboard.Duration;
            return cursorStoryboard.GetCurrentTime().TotalMilliseconds;
        }
        private void StartCursorStoryboard(double duration, double from, double to)
        {
            cursorAnimation.Duration = TimeSpan.FromMilliseconds(duration);
            cursorAnimation.From = from;
            cursorAnimation.To = to;

            cursorStoryboard.Stop();
            cursorStoryboard.Children.Clear();
            cursorStoryboard.Children.Add(cursorAnimation);
            cursorStoryboard.Begin();
        }
        #endregion

        #region -- Scale --
        private int _oldLayerZoomLevel;
        private int _zoomLevel;
        public int ZoomLevel
        {
            get
            {
                return _zoomLevel;
            }
            set
            {
                if (_zoomLevel != value)
                {
                    _zoomLevel = value;

                    if (_oldLayerZoomLevel != value)
                    {
                        MSecondsBetweenLongLines = GetMSecondsPerTimeUnitByLevel(value);
                        playerModel.MaxEditWidth = MaxRightPixel;

                        int oldSecondsPerTimeUnit = GetMSecondsPerTimeUnitByLevel(_oldLayerZoomLevel);
                        double rate = (double)oldSecondsPerTimeUnit / MSecondsBetweenLongLines;
                        SetScaleText();

                        ChangeEffectsPosition(rate);
                        ChangeCursorPosition(rate);
                        _oldLayerZoomLevel = value;
                    }
                }
            }
        }
        static public int MSecondsBetweenLongLines; // TimeUnit : the seconds between two long lines
        static public double PixelsPerSecond { get { return (PixelsBetweenLongLines / MSecondsBetweenLongLines) * 1000; } }
        static public double MaxRightPixel { get { return PixelsPerSecond * MaxEditTime; } }

        private void TimelineScaleInitialize()
        {
            MSecondsBetweenLongLines = GetMSecondsPerTimeUnitByLevel(1); // Level 1
            TimeSpan ts = new TimeSpan(0, 0, 0, 0, MSecondsBetweenLongLines);
            TimeSpan interval = new TimeSpan(0, 0, 0, 0, MSecondsBetweenLongLines);

            int pixelsBetweenLines = (int)(PixelsBetweenLongLines / 2);
            int width = (int)MaxRightPixel;
            int height = (int)ScaleCanvas.ActualHeight;
            int y1_short = (int)(height / 1.5);
            int y1_long = height / 2;
            int y2 = height;
            int linesBetweenLongLines = (int)(PixelsBetweenLongLines / pixelsBetweenLines);
            int totalLines = width / pixelsBetweenLines;

            ScaleCanvas.Children.Clear();
            ScaleCanvas.Children.Add(PlayerCursor_Head);

            for (int i = 1; i < totalLines; i++)
            {
                int x = pixelsBetweenLines * i;
                int y1;

                if (i % linesBetweenLongLines == 0)
                {
                    y1 = y1_long;

                    TranslateTransform tt = new TranslateTransform
                    {
                        X = x + 10,
                        Y = 5
                    };

                    TextBlock tb = new TextBlock
                    {
                        Text = ts.ToString("mm\\:ss"),
                        RenderTransform = tt,
                        Foreground = new SolidColorBrush(Colors.White)
                    };

                    ScaleCanvas.Children.Add(tb);
                    TimeTextBlockCollection.Add(tb);
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
        private void ChangeEffectsPosition(double rate)
        {
            foreach (var layer in Layers)
                layer.UpdateTimelineProportion();
        }
        private void SetScaleText()
        {
            TimeSpan ts = new TimeSpan(0, 0, 0, 0, MSecondsBetweenLongLines);
            TimeSpan interval = new TimeSpan(0, 0, 0, 0, MSecondsBetweenLongLines);

            if (MSecondsBetweenLongLines < 1000)
            {
                foreach (var tb in TimeTextBlockCollection)
                {
                    tb.Text = ts.ToString("mm\\:ss\\.ff");
                    ts = ts.Add(interval);
                }
            }
            else
            {
                foreach (var tb in TimeTextBlockCollection)
                {
                    tb.Text = ts.ToString("mm\\:ss");
                    ts = ts.Add(interval);
                }
            }
        }
        static public double PositionToTime(double position)
        {
            return (position / PixelsPerSecond) * 1000;
        }

        private void LayerZoomSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (Self == null)
                return;

            ZoomLevel = (int)LayerZoomSlider.Value;
        }
        private void PlusButton_Click(object sender, RoutedEventArgs e)
        {
            LayerZoomSlider.Value += 1;
        }
        private void MinusButton_Click(object sender, RoutedEventArgs e)
        {
            LayerZoomSlider.Value -= 1;
        }

        public double[] GetAlignPositions(EffectLineViewModel eff)
        {
            LayerModel layer = eff.Layer;
            List<double> result = new List<double>();
            int i = Layers.IndexOf(layer);

            result.Add(playerModel.Position);
            foreach (var l in Layers)
            {
                if (l.Equals(layer))
                    result.AddRange(l.GetAllEffHeadAndTailPositions(eff));
                else
                    result.AddRange(l.GetAllEffHeadAndTailPositions(null));
            }
            return result.ToArray();
        }
        public double[] GetAlignPositions(LayerModel layer)
        {
            List<double> result = new List<double>();
            int i = Layers.IndexOf(layer);

            result.Add(playerModel.Position);
            result.AddRange(Layers[i].GetAllEffHeadAndTailPositions(null));
            if (i > 0)
                result.AddRange(Layers[i - 1].GetAllEffHeadAndTailPositions(null));
            if (i < Layers.Count - 1)
                result.AddRange(Layers[i + 1].GetAllEffHeadAndTailPositions(null));
            return result.ToArray();
        }

        private void ScaleScrollViewer_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            PointerPoint ptrPt = e.GetCurrentPoint(ScaleCanvas);
            playerModel.Position = ptrPt.Position.X;
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

        public void JumpToBeginningButton_Click(object sender, RoutedEventArgs e)
        {
            double source = ScaleScrollViewer.HorizontalOffset;
            double target = 0;
            AnimationStart(this, "TimelineScrollHorOffset", 200, source, target);

            double from = playerModel.Position;
            double to = 0;
            AnimationStart(this, "TimelineCursorPosition", 200, from, to);
        }
        public void JumpToEndButton_Click(object sender, RoutedEventArgs e)
        {
            double source = ScaleScrollViewer.HorizontalOffset;
            double target = RightmostPosition;
            AnimationStart(this, "TimelineScrollHorOffset", 200, source, target);

            double from = playerModel.Position;
            double to = RightmostPosition;
            AnimationStart(this, "TimelineCursorPosition", 200, from, to);
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
            LayerModel layer = e.Items[0] as LayerModel;
            e.Data.Properties.Add("layer", layer);
        }
        private void TrashCanButton_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data == null)
                return;

            e.DragUIOverride.IsCaptionVisible = false;
            e.DragUIOverride.IsGlyphVisible = false;

            var pair = e.Data.Properties.FirstOrDefault();
            LayerModel layer = pair.Value as LayerModel;
            if (layer != null)
                e.AcceptedOperation = DataPackageOperation.Copy;
        }
        private void TrashCanButton_Drop(object sender, DragEventArgs e)
        {
            var pair = e.Data.Properties.FirstOrDefault();
            LayerModel layer = pair.Value as LayerModel;
            RemoveLayer(layer);
            SpacePage.Self.GoToBlankEditing();
            CheckedLayer = null;
            NeedSave = true;
        }
        public void TrashCanButton_Click(object sender, RoutedEventArgs e)
        {
            if (CheckedLayer != null)
            {
                RemoveLayer(CheckedLayer);
                SpacePage.Self.GoToBlankEditing();
                CheckedLayer = null;
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

        private void ClearAllEffectInvoke(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            if(CheckedLayer != null)
            {
                CheckedLayer.ClearAllEffect();
                CheckedEffect = null;
                args.Handled = true;
            }
        }

        private void CopyEffectInvoke(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            if (CheckedEffect == null)
                return;
            
            CopiedEffect = new EffectLineViewModel(CheckedEffect);
            args.Handled = true;
        }

        private void CutEffectInvoke(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            if (CheckedEffect == null)
                return;

            CopiedEffect = new EffectLineViewModel(CheckedEffect);
            CheckedLayer.DeleteEffectLine(CheckedEffect);
            args.Handled = true;
        }

        public bool g_CanPaste = true;
        private void PasteEffectInvoke(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            if (CheckedLayer == null  || g_CanPaste == false || CopiedEffect == null)
                return;

            args.Handled = true;
            g_CanPaste = false;

            var copy = new EffectLineViewModel(CopiedEffect);
            copy.Left = (CheckedEffect != null) ? CheckedEffect.Right : 0;

            if (!CheckedLayer.TryInsertToTimelineFitly(copy))
            {
                // TODO
                return;
            }

            TimeSpan delay = TimeSpan.FromMilliseconds(400);
            ThreadPoolTimer DelayTimer = ThreadPoolTimer.CreateTimer(
                (source) =>
                {
                    Dispatcher.RunAsync(
                       CoreDispatcherPriority.High,
                       () =>
                       {
                           g_CanPaste = true;
                       });
                }, delay);
        }

        public class AddLayerCommand : IReUndoCommand
        {
            private LayerModel _layer;
            private int _index;

            public AddLayerCommand(LayerModel layer, int index)
            {
                _layer = layer;
                _index = index;
            }

            public void ExecuteRedo()
            {
                LayerPage.Self.AddLayer(_layer);
                LayerPage.Self.CheckedLayer = _layer;
            }
            public void ExecuteUndo()
            {
                LayerPage.Self.RemoveLayer(_layer);
            }
        }
        public class SwapLayerCommand : IReUndoCommand
        {
            private int _old;
            private int _new;

            public SwapLayerCommand(int oldIndex, int newIndex)
            {
                _old = oldIndex;
                _new = newIndex;
            }

            public void ExecuteRedo()
            {
                LayerModel layer = LayerPage.Self.Layers[_old];
                LayerPage.Self.Layers.RemoveAt(_old);
                LayerPage.Self.Layers.Insert(_new, layer);
            }
            public void ExecuteUndo()
            {
                LayerModel layer = LayerPage.Self.Layers[_new];
                LayerPage.Self.Layers.RemoveAt(_new);
                LayerPage.Self.Layers.Insert(_old, layer);
            }
        }
        public class DeleteLayerCommand : IReUndoCommand
        {
            private LayerModel _layer;
            private int _index;

            public DeleteLayerCommand(LayerModel layer, int index)
            {
                _layer = layer;
                _index = index;
            }

            public void ExecuteRedo()
            {
                LayerPage.Self.Layers.RemoveAt(_index);
            }
            public void ExecuteUndo()
            {
                LayerPage.Self.Layers.Insert(_index, _layer);
                LayerPage.Self.CheckedLayer = _layer;
            }
        }
    }
}
