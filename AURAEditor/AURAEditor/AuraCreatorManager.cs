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

namespace AuraEditor
{
    public sealed partial class MainPage : Page
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
                    Effect effect = GetRightmostEffect();

                    return (effect != null) ? effect.StartTime + effect.DurationTime : 0;
                }
            }
            public double RightmostPosition
            {
                get
                {
                    Effect effect = GetRightmostEffect();

                    return (effect != null) ? effect.UI_X + effect.UI_Width : 0;
                }
            }

            private Script script;
            private Dictionary<DynValue, string> _functionDictionary;
            private TriggerDeviceLayer _triggerlayer;
            
            private AuraCreatorManager()
            {
                script = new Script();
                DeviceLayerCollection = new ObservableCollection<DeviceLayer>();
                _functionDictionary = new Dictionary<DynValue, string>();
                GlobalDevices = new List<Device>();
                InitialTriggerDeviceLayer();
                
                TimelineStackPanel = MainPageInstance.TrackStackPanel;
                TimelineScaleCanvas = MainPageInstance.ScaleCanvas;
            }

            #region Layer
            public void ShowTriggerDeviceLayer()
            {
                if (DeviceLayerCollection.Count > 0 && DeviceLayerCollection[0] is TriggerDeviceLayer)
                    return;

                InsertDeviceLayer(_triggerlayer, 0);
            }
            public void HideTriggerDeviceLayer()
            {
                if (DeviceLayerCollection.Count > 0 && DeviceLayerCollection[0] is TriggerDeviceLayer)
                    RemoveDeviceLayer(0);
            }
            public void AddDeviceLayer(DeviceLayer layer)
            {
                layer.UICanvas.Background = AuraEditorColorHelper.GetTimelineBackgroundColor(3);
                DeviceLayerCollection.Add(layer);
                TimelineStackPanel.Children.Add(layer.UICanvas);
            }
            public void InsertDeviceLayer(DeviceLayer layer, int index)
            {
                if (layer is TriggerDeviceLayer)
                    layer.UICanvas.Background = AuraEditorColorHelper.GetTimelineBackgroundColor(0);
                else if (DeviceLayerCollection.Count % 2 == 0)
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
            private void InitialTriggerDeviceLayer()
            {
                _triggerlayer = new TriggerDeviceLayer();

                _triggerlayer.LayerName = "Trigger Effect";
                _triggerlayer.UICanvas.Background = AuraEditorColorHelper.GetTimelineBackgroundColor(0);
                _triggerlayer.AddDeviceZones(0, new int[] { -1 });
            }
            #endregion

            #region Global devices
            public Device GetGlobalDeviceByType(int type)
            {
                return GlobalDevices.Find(x => x.DeviceType == type);
            }
            public void SetGlobalDevices(List<Device> devices)
            {
                GlobalDevices = devices;
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
                    foreach (var effect in layer.Effects)
                    {
                        effect.UI_X = effect.UI_X * rate;
                        effect.UI_Width = effect.UI_Width * rate;
                    }
                }
            }
            private Effect GetRightmostEffect()
            {
                double position = 0;
                double rightmostPosition = 0;
                Effect rightmostEffect = null;

                foreach (DeviceLayer layer in DeviceLayerCollection)
                {
                    foreach (var effect in layer.Effects)
                    {
                        position = effect.UI_X + effect.UI_Width;

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
                sb.Append("require(\"script//global\")\n\n");
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
            private Table CreateNewTable()
            {
                string s = "table={}";
                Table table = script.DoString(s + "\nreturn table").Table;

                return table;
            }
            private Table GetEventProviderTable()
            {
                Table eventProviderTable;
                Table queueTable;
                DynValue generateEventDV;

                queueTable = GetQueueTable();
                generateEventDV = script.LoadFunction(Constants.GenerateEventFunctionString);
                _functionDictionary.Add(generateEventDV, Constants.GenerateEventFunctionString);

                eventProviderTable = CreateNewTable();
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

                    foreach (Effect eff in layer.Effects)
                    {
                        // Give uniqle index for all effects
                        eff.EffectLuaName = EffectHelper.GetEffectName(eff.EffectType) + effectCount.ToString();
                        effectCount++;

                        queueItemTable = CreateNewTable();
                        queueItemTable.Set("Effect", DynValue.NewString(eff.EffectLuaName));
                        queueItemTable.Set("Viewport", DynValue.NewString(layer.LayerName));

                        if (EffectHelper.IsTriggerEffects(eff.EffectType))
                            queueItemTable.Set("Trigger", DynValue.NewString("KeyboardInput"));
                        else if (MainPageInstance.RepeatMode == true)
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
            private Table GetGenerateEventTable()
            {
                Table generateEventTable = CreateNewTable();

                return generateEventTable;
            }
            private Table GetViewportTable()
            {
                Table viewPortTable;
                Table layerTable;

                viewPortTable = CreateNewTable();

                foreach (DeviceLayer layer in DeviceLayerCollection)
                {
                    layerTable = GetLayerTable(layer);
                    viewPortTable.Set(layer.LayerName, DynValue.NewTable(layerTable));
                }

                return viewPortTable;
            }
            private Table GetLayerTable(DeviceLayer layer)
            {
                Table layerTable;
                Dictionary<int, int[]> deviceToZonesDictionary;
                Device device;

                layerTable = CreateNewTable();
                deviceToZonesDictionary = layer.GetDeviceToZonesDictionary();

                foreach (KeyValuePair<int, int[]> pair in deviceToZonesDictionary)
                {
                    device = GetGlobalDeviceByType(pair.Key);
                    layerTable.Set(device.DeviceName, DynValue.NewTable(GetDeviceTable(pair)));
                }

                return layerTable;
            }
            private Table GetDeviceTable(KeyValuePair<int, int[]> pair)
            {
                Table deviceTable;
                Table locationTable;
                Table usageTable;
                Device device;

                deviceTable = CreateNewTable();
                device = GetGlobalDeviceByType(pair.Key);
                locationTable = GetLocationTable(device);
                usageTable = GetUsageTable(pair.Value);

                deviceTable.Set("name", DynValue.NewString(device.DeviceName));

                if (device.DeviceType == 0)
                    deviceTable.Set("DeviceType", DynValue.NewString("Notebook"));
                else if (device.DeviceType == 1)
                    deviceTable.Set("DeviceType", DynValue.NewString("Mouse"));
                else if (device.DeviceType == 2)
                    deviceTable.Set("DeviceType", DynValue.NewString("Keyboard"));
                else
                    deviceTable.Set("DeviceType", DynValue.NewString("Headset"));

                deviceTable.Set("location", DynValue.NewTable(locationTable));
                deviceTable.Set("usage", DynValue.NewTable(usageTable));

                return deviceTable;
            }
            private Table GetUsageTable(int[] zones)
            {
                Table usageTable;
                int count;

                usageTable = CreateNewTable();
                count = 1;

                foreach (int phyIndex in zones)
                {
                    usageTable.Set(count, DynValue.NewNumber(phyIndex));
                    count++;
                };

                return usageTable;
            }
            private Table GetEventTable()
            {
                Table eventTable = CreateNewTable();

                foreach (DeviceLayer gp in DeviceLayerCollection)
                {
                    if (gp.Eye == false)
                        continue;

                    foreach (Effect eff in gp.Effects)
                        eventTable.Set(eff.EffectLuaName, DynValue.NewTable(GetEffectTable(eff)));
                }

                return eventTable;
            }
            private Table GetEffectTable(Effect eff)
            {
                Table effectTable;
                Table initColorTable;
                Table viewportTransformTable;
                Table bindToSlotTable;
                Table waveTable;

                effectTable = CreateNewTable();
                initColorTable = GetInitColorTable(eff);
                viewportTransformTable = EffectHelper.GetViewportTransformTable(_functionDictionary, script, eff.EffectType);
                bindToSlotTable = EffectHelper.GetBindToSlotTable(script, eff.EffectType);
                waveTable = GetWaveTable(eff);

                effectTable.Set("initColor", DynValue.NewTable(initColorTable));
                effectTable.Set("viewportTransform", DynValue.NewTable(viewportTransformTable));
                effectTable.Set("bindToSlot", DynValue.NewTable(bindToSlotTable));
                effectTable.Set("wave", DynValue.NewTable(waveTable));

                return effectTable;
            }
            private Table GetInitColorTable(Effect eff)
            {
                Table initColorTable = CreateNewTable();

                Color c = eff.Info.Color;
                double[] hsl = AuraEditorColorHelper.RgbTOHsl(c);
                DynValue randomHue_dv;

                if (EffectHelper.GetEffectName(eff.EffectType) == "Star")
                {
                    randomHue_dv = script.LoadFunction(Constants.RandomHueString);
                    _functionDictionary.Add(randomHue_dv, Constants.RandomHueString);
                    initColorTable.Set("hue", randomHue_dv);
                }
                else
                    initColorTable.Set("hue", DynValue.NewNumber(hsl[0]));

                initColorTable.Set("saturation", DynValue.NewNumber(hsl[1]));
                initColorTable.Set("lightness", DynValue.NewNumber(hsl[2]));
                initColorTable.Set("alpha", DynValue.NewNumber(c.A / 255));

                return initColorTable;
            }
            private Table GetWaveTable(Effect eff)
            {
                Table waveTable = CreateNewTable();
                EffectInfo info = eff.Info;
                Color c = eff.Info.Color;
                string waveTypeString = "";

                switch (info.WaveType)
                {
                    case 0: waveTypeString = "SineWave"; break;
                    case 1: waveTypeString = "HalfSineWave"; break;
                    case 2: waveTypeString = "QuarterSineWave"; break;
                    case 3: waveTypeString = "SquareWave"; break;
                    case 4: waveTypeString = "TriangleWave"; break;
                    case 5: waveTypeString = "SawToothleWave"; break;
                }

                waveTable.Set("waveType", DynValue.NewString(waveTypeString));
                waveTable.Set("min", DynValue.NewNumber(info.Min));
                waveTable.Set("max", DynValue.NewNumber(info.Max));
                waveTable.Set("waveLength", DynValue.NewNumber(info.WaveLength));
                waveTable.Set("freq", DynValue.NewNumber(info.Freq));
                waveTable.Set("phase", DynValue.NewNumber(info.Phase));
                waveTable.Set("start", DynValue.NewNumber(info.Start));
                waveTable.Set("velocity", DynValue.NewNumber(info.Velocity));

                return waveTable;
            }
            private Table GetGlobalSpaceTable()
            {
                Table globalSpaceTable = CreateNewTable();

                foreach (Device d in GlobalDevices)
                    globalSpaceTable.Set(d.DeviceName, DynValue.NewTable(GetGlobalDeviceTable(d)));

                return globalSpaceTable;
            }
            private Table GetGlobalDeviceTable(Device d)
            {
                Table deviceTable = CreateNewTable();
                Table locationTable = GetLocationTable(d);
                string deviceTypeName = "";

                switch (d.DeviceType)
                {
                    case 0: deviceTypeName = "Notebook"; break;
                    case 1: deviceTypeName = "Mouse"; break;
                    case 2: deviceTypeName = "Keyboard"; break;
                    case 3: deviceTypeName = "Headset"; break;
                }

                deviceTable.Set("DeviceType", DynValue.NewString(deviceTypeName));
                deviceTable.Set("location", DynValue.NewTable(locationTable));

                return deviceTable;
            }
            private Table GetLocationTable(Device d)
            {
                Table locationTable = CreateNewTable();

                locationTable.Set("x", DynValue.NewNumber(d.X));
                locationTable.Set("y", DynValue.NewNumber(d.Y));

                return locationTable;
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
                            keyValue = _functionDictionary[keyDV];
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
}
