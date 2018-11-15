using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Xml;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using AuraEditor.UserControls;
using static AuraEditor.Common.ControlHelper;
using static AuraEditor.Common.EffectHelper;
using static AuraEditor.Common.XmlHelper;
using static AuraEditor.AuraSpaceManager;

namespace AuraEditor
{
    public class AuraLayerManager
    {
        static public AuraLayerManager Self;

        private ListView m_LayerListView;
        private StackPanel m_TrackStackPanel;
        private StackPanel m_BackgroundStackPanel;
        private Canvas m_TimelineScaleCanvas;

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
                if (_checkedLayer != value)
                {
                    if (value != null)
                    {
                        value.UI_Background.GoToState("Checked");
                        AuraSpaceManager.Self.WatchLayer(value);

                        var effect = value.TimelineEffects.Find(eff => eff.UI.IsChecked == true);

                        if (effect == null)
                            effect = value.FindFirstEffectOnTheRight(0);

                        if (effect != null)
                            effect.UI.IsChecked = true;
                        else
                            MainPage.Self.SelectedEffectLine = null;
                    }

                    if (_checkedLayer != null)
                    {
                        _checkedLayer.UI_Background.GoToState("Normal");
                    }

                    _checkedLayer = value;
                }
            }
        }

        public TimelineEffect CopiedEffect;
        static public int SecondsPerTimeUnit; // TimeUnit : the seconds between two long lines
        static public double PixelsPerTimeUnit;
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

                return (effect != null) ? effect.UI.X + effect.UI.Width : 0;
            }
        }

        public AuraLayerManager()
        {
            Self = this;
            m_TrackStackPanel = MainPage.Self.TrackStackPanel;
            m_BackgroundStackPanel = MainPage.Self.BackgroundStackPanel;
            m_TimelineScaleCanvas = MainPage.Self.ScaleCanvas;
            m_LayerListView = MainPage.Self.LayerListView;

            Layers = new ObservableCollection<Layer>();
            Layers.CollectionChanged += LayersChanged;
            PixelsPerTimeUnit = 200;
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
            m_LayerListView.ItemsSource = Layers;
            m_TrackStackPanel.Children.Clear();
            m_BackgroundStackPanel.Children.Clear();
            CheckedLayer = null;

            for (int i = 0; i < Layers.Count; i++)
            {
                Layers[i].Name = "Layer " + (i + 1).ToString();
                m_TrackStackPanel.Children.Add(Layers[i].UI_Track);
                m_BackgroundStackPanel.Children.Add(Layers[i].UI_Background);
            }

            // TODO : Use MVVM instead of Update()
            List<LayerTitle> items = FindAllControl<LayerTitle>(m_LayerListView, typeof(LayerTitle));
            foreach (var item in items)
            {
                item.Update();
            }

            AuraSpaceManager.Self.SetSpaceStatus(SpaceStatus.Normal);
        }
        public int GetLayerCount()
        {
            return Layers.Count;
        }
        public void Clean()
        {
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
        static public double GetPixelsPerSecond()
        {
            return (int)PixelsPerTimeUnit / SecondsPerTimeUnit;
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
                    effect.UI.X = effect.UI.X * rate;
                    effect.UI.Width = effect.UI.Width * rate;
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
                    position = effect.UI.X + effect.UI.Width;

                    if (position > rightmostPosition)
                    {
                        rightmostPosition = position;
                        rightmostEffect = effect;
                    }
                }
            }
            return rightmostEffect;
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
