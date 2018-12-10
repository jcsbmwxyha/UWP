using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Xml;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using AuraEditor.Pages;
using AuraEditor.UserControls;
using static AuraEditor.Common.Definitions;
using static AuraEditor.Common.EffectHelper;
using static AuraEditor.Common.XmlHelper;
using static AuraEditor.AuraSpaceManager;

namespace AuraEditor
{
    public class AuraLayerManager
    {
        static public AuraLayerManager Self;

        private PlayerCursor m_PlayerCursor;
        private ListView m_LayerListView;
        private StackPanel m_TrackStackPanel;
        private ItemsControl m_BackgroundItemsControl;
        private Canvas m_TimelineScaleCanvas;
        private Canvas m_TrackCanvas;
        private Frame m_EffectInfoFrame;

        public ObservableCollection<Layer> Layers { get; set; }
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
                    AuraSpaceManager.Self.WatchLayer(value);

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
                    MainPage.Self.NeedSave = true;
                }
            }
        }
        public TimelineEffect CopiedEffect;

        static public int SecondsPerTimeUnit; // TimeUnit : the seconds between two long lines
        static public double PixelsPerSecond { get { return PixelsPerTimeUnit / SecondsPerTimeUnit; } }
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

        public AuraLayerManager()
        {
            Self = this;
            m_TrackStackPanel = MainPage.Self.TrackStackPanel;
            m_TimelineScaleCanvas = MainPage.Self.ScaleCanvas;
            m_TrackCanvas = MainPage.Self.TrackCanvas;
            m_PlayerCursor = MainPage.Self.PlayerCursor_Head;
            m_EffectInfoFrame = MainPage.Self.EffectInfoFrame;

            Layers = new ObservableCollection<Layer>();
            m_LayerListView = MainPage.Self.LayerListView;
            m_LayerListView.ItemsSource = Layers;
            m_BackgroundItemsControl = MainPage.Self.LayerBackgroundItemsControl;
            m_BackgroundItemsControl.ItemsSource = Layers;
            Layers.CollectionChanged += LayersChanged;
        }

        #region Layer
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
                    m_TrackStackPanel.Children.Remove(layer.UI_Track);
                    break;
                case NotifyCollectionChangedAction.Add:
                    layer = e.NewItems[0] as Layer;
                    layerIndex = e.NewStartingIndex;
                    m_TrackStackPanel.Children.Insert(layerIndex, layer.UI_Track);
                    break;
            }

            for (int i = 0; i < Layers.Count; i++)
            {
                if (Layers[i].Name == "")
                    Layers[i].Name = "Layer " + (i + 1).ToString();
            }

            AuraSpaceManager.Self.SetSpaceStatus(SpaceStatus.Init);
            m_TrackCanvas.Height = Layers.Count * 52;
        }
        public int GetLayerCount()
        {
            return Layers.Count;
        }
        public void Clean()
        {
            CheckedLayer = null;
            m_TrackStackPanel.Children.Clear();
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

        #region Timeline scale
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

            result.Add(MainPage.Self.Player.GetCursorPosition());
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
            int width = (int)m_TimelineScaleCanvas.ActualWidth;
            int height = (int)m_TimelineScaleCanvas.ActualHeight;
            int y1_short = (int)(height / 1.5);
            int y1_long = height / 2;
            double y2 = height;
            int linePerTimeUnit = (int)(PixelsPerTimeUnit / minimumScaleUnitLength);
            int totalLineCount = width / minimumScaleUnitLength;

            m_TimelineScaleCanvas.Children.Clear();
            m_TimelineScaleCanvas.Children.Add(m_PlayerCursor);

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

                    m_TimelineScaleCanvas.Children.Add(tb);
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

                m_TimelineScaleCanvas.Children.Add(line);
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
