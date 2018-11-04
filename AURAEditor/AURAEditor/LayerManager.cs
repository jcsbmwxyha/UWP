using AuraEditor.Common;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using AuraEditor.UserControls;
using Windows.UI.Xaml.Shapes;
using static AuraEditor.Common.ControlHelper;
using static AuraEditor.Common.XmlHelper;
using static AuraEditor.Common.EffectHelper;
using Windows.UI.Xaml.Media;
using Windows.UI;
using System.Collections.ObjectModel;
using System.Xml;
using System.Collections.Specialized;

namespace AuraEditor
{
    public class AuraLayerManager
    {
        static public AuraLayerManager Self;

        private StackPanel m_TimelineStackPanel;
        private Canvas m_TimelineScaleCanvas;
        private ListView m_LayerListView;

        public ObservableCollection<Layer> Layers { get; set; }
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
            m_TimelineStackPanel = MainPage.Self.TrackStackPanel;
            m_TimelineScaleCanvas = MainPage.Self.ScaleCanvas;
            m_LayerListView = MainPage.Self.LayerListView;

            Layers = new ObservableCollection<Layer>();
            Layers.CollectionChanged += LayersChanged;
            PixelsPerTimeUnit = 200;
        }
        #region Layer
        public void AddDeviceLayer(Layer layer)
        {
            Layers.Add(layer);
            m_TimelineStackPanel.Children.Add(layer.UICanvas);
        }
        public void RemoveDeviceLayer(Layer layer)
        {
            Layers.Remove(layer);
            m_TimelineStackPanel.Children.Remove(layer.UICanvas);
        }
        private void LayersChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            for (int i = 0; i < Layers.Count; i++)
            {
                Layers[i].Name = "Layer " + (i + 1).ToString();
            }

            List<DeviceLayerItem> items =
                FindAllControl<DeviceLayerItem>(m_LayerListView, typeof(DeviceLayerItem));

            // TODO : Use MVVM instead of Update()
            foreach (var item in items)
                item.Update();
        }
        public int GetLayerCount()
        {
            return Layers.Count;
        }
        public Layer GetSelectedLayer()
        {
            List<DeviceLayerItem> layers =
                FindAllControl<DeviceLayerItem>(m_LayerListView, typeof(DeviceLayerItem));

            foreach (var layer in layers)
            {
                if (layer.IsChecked == true)
                    return layer.DataContext as Layer;
            }

            return null;
        }
        public void UnselectAllLayers()
        {
            List<DeviceLayerItem> layers =
                FindAllControl<DeviceLayerItem>(m_LayerListView, typeof(DeviceLayerItem));

            foreach (var layer in layers)
            {
                layer.IsChecked = false;
            }

            m_LayerListView.SelectedIndex = -1;
        }
        public void Clean()
        {
            m_TimelineStackPanel.Children.Clear();
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
