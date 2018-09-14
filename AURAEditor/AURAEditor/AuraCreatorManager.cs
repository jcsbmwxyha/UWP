using AuraEditor.Common;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using static AuraEditor.Common.LuaHelper;
using static AuraEditor.Common.EffectHelper;

namespace AuraEditor
{
    public class AuraCreatorManager
    {
        static AuraCreatorManager _instance;
        static public AuraCreatorManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new AuraCreatorManager();

                return _instance;
            }
        }
        static public int secondsPerTimeUnit; // TimeUnit : the seconds between two long lines
        static public double pixelsPerTimeUnit = 200;
        static StackPanel TimelineStackPanel;
        static Canvas TimelineScaleCanvas;

        public ObservableCollection<DeviceLayer> DeviceLayerCollection { get; set; }
        public List<Device> GlobalDevices;
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

        private AuraCreatorManager()
        {
            DeviceLayerCollection = new ObservableCollection<DeviceLayer>();
            GlobalDevices = new List<Device>();

            TimelineStackPanel = MainPage.MainPageInstance.TrackStackPanel;
            TimelineScaleCanvas = MainPage.MainPageInstance.ScaleCanvas;
        }

        #region Layer
        public void AddDeviceLayer(DeviceLayer layer)
        {
            layer.UICanvas.Background = AuraEditorColorHelper.GetTimelineBackgroundColor(3);
            DeviceLayerCollection.Add(layer);
            TimelineStackPanel.Children.Add(layer.UICanvas);
        }
        public void InsertDeviceLayer(DeviceLayer layer, int index)
        {
            layer.UICanvas.Background = AuraEditorColorHelper.GetTimelineBackgroundColor(3);
            DeviceLayerCollection.Insert(index, layer);
            TimelineStackPanel.Children.Insert(index, layer.UICanvas);
        }
        public void RemoveDeviceLayer(int index)
        {
            DeviceLayerCollection.RemoveAt(index);
            TimelineStackPanel.Children.RemoveAt(index);
        }
        public void RemoveDeviceLayer(DeviceLayer layer)
        {
            DeviceLayerCollection.Remove(layer);
            TimelineStackPanel.Children.Remove(layer.UICanvas);
        }
        public int GetLayerCount()
        {
            return DeviceLayerCollection.Count;
        }
        public void ClearAllLayer()
        {
            TimelineStackPanel.Children.Clear();
            DeviceLayerCollection.Clear();
        }
        #endregion

        #region Global devices
        public Device GetGlobalDeviceByType(int type)
        {
            return GlobalDevices.Find(x => x.Type == type);
        }
        #endregion

        #region Timeline scale
        static public double GetPixelsPerSecond()
        {
            return (int)pixelsPerTimeUnit / secondsPerTimeUnit;
        }
        public void SetTimeUnit(int newSecondsPerTimeUnit)
        {
            double rate = (double)secondsPerTimeUnit / newSecondsPerTimeUnit;

            secondsPerTimeUnit = newSecondsPerTimeUnit;
            DrawTimelineScale();

            foreach (var layer in DeviceLayerCollection)
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

            foreach (DeviceLayer layer in DeviceLayerCollection)
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
            TimeSpan ts = new TimeSpan(0, 0, secondsPerTimeUnit);
            TimeSpan interval = new TimeSpan(0, 0, secondsPerTimeUnit);
            int minimumScaleUnitLength = (int)(pixelsPerTimeUnit / 2);
            int width = (int)TimelineScaleCanvas.ActualWidth;
            int height = (int)TimelineScaleCanvas.ActualHeight;
            int y1_short = (int)(height / 1.5);
            int y1_long = height / 2;
            double y2 = height;
            int linePerTimeUnit = (int)(pixelsPerTimeUnit / minimumScaleUnitLength);
            int totalLineCount = width / minimumScaleUnitLength;

            TimelineScaleCanvas.Children.Clear();

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

                    TimelineScaleCanvas.Children.Add(tb);
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

                TimelineScaleCanvas.Children.Add(line);
            }
        }
        #endregion

        #region Lua script
        public string PrintLuaScript()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(RequireLine);
            sb.Append("EventProvider = ");
            sb.Append(PrintTable(GetEventProviderTable()));
            sb.Append("\n");
            sb.Append("Viewport = ");
            sb.Append(PrintTable(GetViewportTable()));
            sb.Append("\n");
            sb.Append("Event = ");
            sb.Append(PrintTable(GetEventTable()));
            sb.Append("\n");
            sb.Append("GlobalSpace = ");
            sb.Append(PrintTable(GetGlobalSpaceTable()));
            sb.Append("\n");
            return sb.ToString();
        }
        private Table GetEventProviderTable()
        {
            Table eventProviderTable = CreateNewTable();
            Table queueTable = GetQueueTable();
            DynValue generateEventDV = RegisterAndGetDV(GenerateEventFunctionString);

            eventProviderTable.Set("queue", DynValue.NewTable(queueTable));
            eventProviderTable.Set("period", DynValue.NewNumber(PlayTime));
            eventProviderTable.Set("generateEvent", generateEventDV);

            return eventProviderTable;
        }
        private Table GetQueueTable()
        {
            Table queueTable;
            Table queueItemTable;
            int effectCount;
            int queueIndex;

            queueTable = CreateNewTable();
            effectCount = 0;
            queueIndex = 1;

            foreach (DeviceLayer layer in DeviceLayerCollection)
            {
                if (layer.Eye == false)
                    continue;

                foreach (TimelineEffect eff in layer.TimelineEffects)
                {
                    // Give uniqle index for all effects
                    eff.LuaName = GetEffectName(eff.Type) + effectCount.ToString();
                    effectCount++;

                    queueItemTable = CreateNewTable();
                    queueItemTable.Set("Effect", DynValue.NewString(eff.LuaName));
                    queueItemTable.Set("Viewport", DynValue.NewString(layer.Name));

                    if (IsTriggerEffects(eff.Type))
                        queueItemTable.Set("Trigger", DynValue.NewString("KeyboardInput"));
                    else if (MainPage.MainPageInstance.RepeatMode == true)
                        queueItemTable.Set("Trigger", DynValue.NewString("Period"));
                    else
                        queueItemTable.Set("Trigger", DynValue.NewString("OneTime"));

                    queueItemTable.Set("Delay", DynValue.NewNumber(eff.StartTime));
                    queueItemTable.Set("Duration", DynValue.NewNumber(eff.DurationTime));
                    queueTable.Set(queueIndex, DynValue.NewTable(queueItemTable));
                    queueIndex++;
                }
            }

            return queueTable;
        }
        private Table GetViewportTable()
        {
            Table viewPortTable;
            Table layerTable;

            viewPortTable = CreateNewTable();

            foreach (DeviceLayer layer in DeviceLayerCollection)
            {
                layerTable = layer.ToTable();
                viewPortTable.Set(layer.Name, DynValue.NewTable(layerTable));
            }

            return viewPortTable;
        }
        private Table GetEventTable()
        {
            Table eventTable = CreateNewTable();

            foreach (DeviceLayer layer in DeviceLayerCollection)
            {
                if (layer.Eye == false)
                    continue;

                foreach (TimelineEffect eff in layer.TimelineEffects)
                    eventTable.Set(eff.LuaName, DynValue.NewTable(eff.ToTable()));
            }

            return eventTable;
        }
        private Table GetGlobalSpaceTable()
        {
            Table globalSpaceTable = CreateNewTable();

            foreach (Device d in GlobalDevices)
                globalSpaceTable.Set(d.Name, DynValue.NewTable(d.ToTable()));

            return globalSpaceTable;
        }
        private string PrintTable(Table tb, int tab = 0)
        {
            StringBuilder sb = new StringBuilder();

            string paranthesesTabString = "";
            string otherTabString = "";

            for (int i = 0; i < tab; i++)
            {
                paranthesesTabString += "\t";
            }
            otherTabString = paranthesesTabString + "\t";

            sb.Append(paranthesesTabString + "{\n");
            foreach (var key in tb.Keys)
            {
                DynValue keyDV;
                string keyName = "";
                string keyValue = "";

                // key name
                if (key.Type.ToString() == "String")
                {
                    keyName = "[\"" + key.String + "\"]";
                    keyDV = tb.Get(key.String);
                }
                else
                {
                    keyName = "[" + key.Number.ToString() + "]";
                    keyDV = tb.Get(key.Number);
                }

                // key value
                if (keyDV.Table != null)
                {
                    keyValue = PrintTable(keyDV.Table, tab + 1);
                    sb.Append(otherTabString + keyName + " =\n" + keyValue + ",\n");
                }
                else
                {
                    if (keyDV.String != null)
                    {
                        keyValue = "\"" + keyDV.String + "\"";
                    }
                    else if (keyDV.Function != null)
                    {
                        keyValue = GetFunctionString(keyDV);
                    }
                    else
                    {
                        keyValue = keyDV.Number.ToString();
                    }
                    sb.Append(otherTabString + keyName + " = " + keyValue + ",\n");
                }
            }

            sb.Append(paranthesesTabString + "}");
            return sb.ToString();
        }
        #endregion
    }
}
