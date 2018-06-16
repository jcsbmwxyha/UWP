﻿using System.Collections.Generic;
using Windows.UI.Xaml;
using System;
using Windows.UI.Xaml.Controls;
using AuraEditor.Common;
using Windows.ApplicationModel.DataTransfer;
using System.Collections.ObjectModel;
using Windows.UI;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Input;
using Windows.Foundation;
using Windows.UI.Xaml.Media.Imaging;
using System.Text;
using MoonSharp.Interpreter;

namespace AuraEditor
{
    public class EffectInfo
    {
        public Color Color { get; set; }
        public int WaveType { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
        public double WaveLength { get; set; }
        public double Freq { get; set; }
        public double Phase { get; set; }
        public double Start { get; set; }

        public EffectInfo()
        {
            Color = Colors.Red;
            WaveType = 0;
            Min = 0;
            Max = 0;
            WaveLength = 0;
            Freq  = 0;
            Phase = 0;
            Start = 0;
        }
    }

    public class Effect
    {
        public DeviceGroup MyDeviceGroup { get; set; }
        public string EffectLuaName { get; set; }
        public int EffectType { get; set; }
        public Border UIBorder { get; }
        private int _start;
        public int Start
        {
            get { return _start; }
            set
            {
                CompositeTransform ct = UIBorder.RenderTransform as CompositeTransform;
                ct.TranslateX = value;
                _start = value;
            }
        }
        private int _duration;
        public int Duration {
            get { return _duration; }
            set
            {
                _duration = value;
                UIBorder.Width = value;
            }
        }
        private EffectInfo _info;
        public EffectInfo Info
        {
            get { return _info; }
            set
            {
                if ((EffectType != 3) && (EffectType != 6) && (EffectType != 2))
                    UIBorder.Background = new SolidColorBrush(value.Color);
                _info = value;
            }
        }

        private bool _cursorSizeRight;
        private bool _cursorSizeLeft;
        private bool _cursorMove;

        public Effect(DeviceGroup dg, int effectType)
        {
            MyDeviceGroup = dg;
            EffectType = effectType;
            UIBorder = CreateUIBorder(effectType);
            Start = (int)MyDeviceGroup.GetFirstSpaceCanPut();
            Duration = 100;
            Info = new EffectInfo();

            _cursorSizeRight = false;
            _cursorSizeLeft = false;
            _cursorMove = false;
        }
        public Border CreateUIBorder(int effectType)
        {
            string effectname = EffectHelper.GetEffectName(effectType);

            TextBlock tb = new TextBlock
            {
                Text = effectname,
                FontSize = 22,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            Border border = new Border
            {
                BorderBrush = new SolidColorBrush(Colors.Black),
                Height = 50,
                BorderThickness = new Thickness(3, 3, 3, 3),
                Padding = new Thickness(5),
                CornerRadius = new CornerRadius(15),
                HorizontalAlignment = HorizontalAlignment.Center,
                Child = tb,
                ManipulationMode = ManipulationModes.TranslateX
            };

            border.PointerPressed += EffectLine_PointerPressed;
            border.ManipulationStarted += EffectLine_ManipulationStarted;
            border.ManipulationCompleted += EffectLine_ManipulationCompleted;
            border.ManipulationDelta += EffectLine_ManipulationDelta;
            border.PointerReleased += EffectLine_PointerReleased;
            border.PointerMoved += EffectLine_PointerMoved;
            border.PointerExited += EffectLine_PointerExited;


            if (effectType == EffectHelper.GetEffectIndex("Smart"))
                border.Background = AuraEditorColorHelper.GetSmartBrush();
            else if (effectType == EffectHelper.GetEffectIndex("Rainbow"))
                border.Background = AuraEditorColorHelper.GetRainbowBrush();
            else
                border.Background = new SolidColorBrush(Windows.UI.Colors.Red);

            CompositeTransform ct = new CompositeTransform
            {
                TranslateX = 0
            };
            border.RenderTransform = ct;

            return border;
        }
        private void EffectLine_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var border = sender as Border;
            border.Opacity = 0.5;

            Point position = e.GetCurrentPoint(border).Position;
            bool _hasCapture = border.CapturePointer(e.Pointer);

            if (position.X > border.Width - 5)
                _cursorSizeRight = true;
            else if (position.X > 5)
                _cursorMove = true;
            else
                _cursorSizeLeft = true;

            var frame = (Frame)Window.Current.Content;
            var page = (MainPage)frame.Content;

            string effectname = ((TextBlock)border.Child).Text;

            page.UpdateEffectInfoGrid(this);
        }
        void EffectLine_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
        }
        void EffectLine_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var fe = sender as FrameworkElement;
            CompositeTransform ct = fe.RenderTransform as CompositeTransform;

            if ((ct.TranslateX + e.Delta.Translation.X < 0))
                return;

            if (_cursorMove)
            {
                ct.TranslateX += e.Delta.Translation.X;
            }
            else if (_cursorSizeRight)
            {
                if (e.Position.X > 50)
                    fe.Width = e.Position.X;
            }
            else if (_cursorSizeLeft)
            {
                if (fe.Width - e.Delta.Translation.X > 50)
                {
                    ct.TranslateX += e.Delta.Translation.X;
                    fe.Width -= e.Delta.Translation.X;
                }
            }
        }
        void EffectLine_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        { }
        private void EffectLine_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            int leftPosition, rightPosition, width;
            var fe = sender as FrameworkElement;
            fe.Opacity = 1;

            CompositeTransform ct = fe.RenderTransform as CompositeTransform;
            leftPosition = (int)ct.TranslateX / 10 * 10;

            if (_cursorSizeLeft)
            {
                rightPosition = (int)ct.TranslateX + (int)fe.Width;
                width = rightPosition - leftPosition;
            }
            else
            {
                width = (int)fe.Width / 10 * 10;
            }

            if (MyDeviceGroup.IsEffectLineOverlap(this, leftPosition, width))
            {
                ct.TranslateX = Start;
                fe.Width = Duration;
                return;
            }
            
            Start = leftPosition;
            Duration = width;

            _cursorMove = false;
            _cursorSizeRight = false;
            _cursorSizeLeft = false;
        }
        private void EffectLine_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            FrameworkElement fe = sender as FrameworkElement;
            if (e.GetCurrentPoint(fe).Position.X > fe.Width - 5 || e.GetCurrentPoint(fe).Position.X < 5)
                Window.Current.CoreWindow.PointerCursor =
                    new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.SizeWestEast, 0);
            else
                Window.Current.CoreWindow.PointerCursor =
                    new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.SizeAll, 0);
        }
        private void EffectLine_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor =
                new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
        }
    }

    public class Device
    {
        public string DeviceName { get; set; }
        public int DeviceType { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double W { get; set; }
        public double H { get; set; }
        public Image DeviceImg { get; set; }
        public string UriString { get; set; }
        public int[] SelectedZone { get; set; }

        public Device() { }
        public Device(string s, int type, int x, int y)
        {
            DeviceName = s;
            DeviceType = type;

            X = x;
            Y = y;

            if (type == 0)
            {
                UriString = "ms-appx:///Assets/device_local.png";
                W = 21;
                H = 6;
            }
            else if (type == 1)
                UriString = "ms-appx:///Assets/asus_aura_sync_mouse.png";
            else if (type == 2)
            {
                UriString = "ms-appx:///Assets/asus_aura_sync_keyboard.png";
                W = 23;
                H = 6;
            }
            else if (type == 3)
                UriString = "ms-appx:///Assets/asus_aura_sync_headset.png";

            int commonFactor = 71;

            CompositeTransform ct = new CompositeTransform
            {
                TranslateX = commonFactor * X,
                TranslateY = commonFactor * Y
            };

            Image img = new Image
            {
                RenderTransform = ct,
                Width = commonFactor,
                Height = commonFactor,
                Source = new BitmapImage(new Uri(UriString)),
                ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY
            };

            img.ManipulationStarted += DeviceImg_ManipulationStarted;
            img.ManipulationDelta += DeviceImg_ManipulationDelta;
            img.ManipulationCompleted += DeviceImg_ManipulationCompleted;
            img.PointerEntered += DeviceImg_PointerEntered;
            img.PointerExited += DeviceImg_PointerExited;

            DeviceImg = img;
        }
        private void DeviceImg_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            var fe = sender as FrameworkElement;
            fe.Opacity = 0.5;
        }
        private void DeviceImg_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            Image img = sender as Image;
            CompositeTransform ct = img.RenderTransform as CompositeTransform;
            ct.TranslateX += e.Delta.Translation.X;
            ct.TranslateY += e.Delta.Translation.Y;
        }
        private void DeviceImg_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            Image img = sender as Image;
            img.Opacity = 1;

            BitmapImage bs = img.Source as BitmapImage;
            int commonFactor = 71;

            CompositeTransform ct = img.RenderTransform as CompositeTransform;
            X = (int)ct.TranslateX / commonFactor;
            Y = (int)ct.TranslateY / commonFactor;
            ct.TranslateX = X * commonFactor;
            ct.TranslateY = Y * commonFactor;
        }
        private void DeviceImg_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.SizeAll, 0);
        }
        private void DeviceImg_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
        }
    }

    public class DeviceGroup
    {
        public string GroupName { get; set; }
        List<Device> _devices;
        public List<Effect> Effects;
        public Canvas UICanvas;
        
        public DeviceGroup(string name = "")
        {
            GroupName = name;
            _devices = new List<Device>();
            Effects = new List<Effect>();
            UICanvas = CreateUICanvas();
        }
        public List<Device> GetDevices()
        {
            return _devices;
        }
        public void AddDevice(Device device)
        {
            _devices.Add(device);
        }
        public void SetDevices(List<Device> devices)
        {
            _devices = devices;
        }
        public void AddEffect(Effect effect)
        {
            Effects.Add(effect);
            UICanvas.Children.Add(effect.UIBorder);
        }
        private void Canvas_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
        }
        private async void Canvas_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.Text))
            {
                var effectname = await e.DataView.GetTextAsync();
                int type = EffectHelper.GetEffectIndex(effectname);

                Effect effect = new Effect(this, type);
                AddEffect(effect);
            }
        }
        public bool IsEffectLineOverlap(Effect effect, int x, int w)
        {
            foreach (Effect e in Effects)
            {
                if (!e.Equals(effect))
                {
                    if ((x < e.Start + e.Duration) && (x + w > e.Start))
                        return true;
                }
            }
            return false;
        }
        public double GetFirstSpaceCanPut()
        {
            int spaceX = 0;

            for (int i = 0; i < Effects.Count; i++)
            {
                Effect effect = Effects[i];
                if (spaceX <= effect.Start && effect.Start < spaceX + 100)
                {
                    spaceX = effect.Start + effect.Duration;
                    i = -1; // rescan every effect line
                }
            }
            
            return spaceX;
        }
        private Canvas CreateUICanvas()
        {
            Canvas canvas = new Canvas
            {
                Width = 5000,
                Height = 50
            };

            canvas.DragOver += Canvas_DragOver;
            canvas.Drop += Canvas_Drop;

            return canvas;
        }
    }

    public class DeviceGroupManager
    {
        public ObservableCollection<DeviceGroup> DeviceGroupCollection { get; set; }
        static StackPanel TimeLineStackPanel;
        Script script;
        Dictionary<DynValue, string> _functionDictionary;

        public DeviceGroupManager(StackPanel sp)
        {
            script = new Script();
            TimeLineStackPanel = sp;
            DeviceGroupCollection = new ObservableCollection<DeviceGroup>();
            _functionDictionary = new Dictionary<DynValue, string>();
        }
        public void AddDeviceGroup(DeviceGroup dg)
        {
            if (DeviceGroupCollection.Count % 2 == 0)
                dg.UICanvas.Background = AuraEditorColorHelper.GetTimeLineBackgroundColor(true);
            else
                dg.UICanvas.Background = AuraEditorColorHelper.GetTimeLineBackgroundColor(false);

            DeviceGroupCollection.Add(dg);
            TimeLineStackPanel.Children.Add(dg.UICanvas);
        }
        public void ClearAllGroup()
        {
            TimeLineStackPanel.Children.Clear();
            DeviceGroupCollection.Clear();
        }
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
            Table eventProviderTable = CreateNewTable();
            Table queueTable = CreateNewTable();
            Table queueItemtable;

            int effectCount = 0;
            int queueIndex = 1;
            foreach (DeviceGroup gp in DeviceGroupCollection)
            {
                foreach (Effect eff in gp.Effects)
                {
                    queueItemtable = CreateNewTable();

                    // Give uniqle index for all of effect lines
                    eff.EffectLuaName = EffectHelper.GetEffectName(eff.EffectType) + effectCount.ToString();
                    effectCount++;

                    queueItemtable.Set("Effect", DynValue.NewString(eff.EffectLuaName));
                    queueItemtable.Set("Viewport", DynValue.NewString(gp.GroupName));
                    if (EffectHelper.GetEffectName(eff.EffectType) == "Comet")
                        queueItemtable.Set("Trigger", DynValue.NewString("Period"));
                    else
                        queueItemtable.Set("Trigger", DynValue.NewString("OneTime"));
                    queueItemtable.Set("Delay", DynValue.NewNumber(eff.Start * 10));
                    queueItemtable.Set("Duration", DynValue.NewNumber(eff.Duration * 10));
                    queueTable.Set(queueIndex, DynValue.NewTable(queueItemtable));
                    queueIndex++;
                }
            }
            eventProviderTable.Set("queue", DynValue.NewTable(queueTable));

            DynValue generate_dv = script.LoadFunction(Constants.GenerateEventFunctionString);
            _functionDictionary.Add(generate_dv, Constants.GenerateEventFunctionString);
            
            eventProviderTable.Set("period", DynValue.NewNumber(4000));
            eventProviderTable.Set("generateEvent", generate_dv);
            return eventProviderTable;
        }
        private Table GetViewportTable() {
            Table viewPortTable = CreateNewTable();
            Table groupTable;
            Table deviceTable;
            Table layoutTable;
            Table locationTable;
            Table rangeTable;
            Table fromToTable;

            int groupIndex = 1;
            foreach (DeviceGroup dg in DeviceGroupCollection)
            {
                groupTable = CreateNewTable();
                List<Device> devices = dg.GetDevices();

                foreach (Device d in devices)
                {
                    layoutTable = CreateNewTable();
                    locationTable = CreateNewTable();
                    deviceTable = CreateNewTable();
                    rangeTable = CreateNewTable();

                    layoutTable.Set("weight", DynValue.NewNumber(d.W));
                    layoutTable.Set("height", DynValue.NewNumber(d.H));
                    locationTable.Set("x", DynValue.NewNumber(d.X));
                    locationTable.Set("y", DynValue.NewNumber(d.Y));
                    deviceTable.Set("name", DynValue.NewString(d.DeviceName));

                    if (d.DeviceType == 0)
                        deviceTable.Set("DeviceType", DynValue.NewString("Notebook"));
                    else if (d.DeviceType == 1)
                        deviceTable.Set("DeviceType", DynValue.NewString("Mouse"));
                    else if (d.DeviceType == 2)
                        deviceTable.Set("DeviceType", DynValue.NewString("Keyboard"));
                    else
                        deviceTable.Set("DeviceType", DynValue.NewString("Headset"));

                    deviceTable.Set("layout", DynValue.NewTable(layoutTable));
                    deviceTable.Set("location", DynValue.NewTable(locationTable));

                    rangeTable = CreateNewTable();
                    if (d.SelectedZone != null && d.SelectedZone.Length > 0)
                    {
                        int count = 1;
                        foreach (int index in d.SelectedZone)
                        {
                            fromToTable = CreateNewTable();
                            int i = 0;

                            if (d.DeviceType == 0)
                                i = KeyRemap.G703Remap(index);
                            else if (d.DeviceType == 2)
                                i = KeyRemap.FlairRemap(index);

                            fromToTable.Set("from", DynValue.NewNumber(i));
                            fromToTable.Set("to", DynValue.NewNumber(i));
                            rangeTable.Set(count, DynValue.NewTable(fromToTable));
                            count++;
                        };
                    }
                    else
                    {
                        fromToTable = CreateNewTable();
                        fromToTable.Set("from", DynValue.NewNumber(0));
                        fromToTable.Set("to", DynValue.NewNumber(168));
                        rangeTable.Set(1, DynValue.NewTable(fromToTable));
                    }
                    deviceTable.Set("range", DynValue.NewTable(rangeTable));

                    groupTable.Set(d.DeviceName, DynValue.NewTable(deviceTable));
                }
                viewPortTable.Set(dg.GroupName, DynValue.NewTable(groupTable));
                groupIndex++;
            }
            return viewPortTable;
        }
        private Table GetEventTable() {
            Table eventTable = CreateNewTable();
            Table effectTable;
            Table initColorTable;
            Table waveTable;

            foreach (DeviceGroup gp in DeviceGroupCollection)
            {
                foreach (Effect eff in gp.Effects)
                {
                    EffectInfo info = eff.Info;

                    effectTable = CreateNewTable();
                    initColorTable = CreateNewTable();
                    waveTable = CreateNewTable();

                    Color c = eff.Info.Color;
                    double[] hsl = AuraEditorColorHelper.RgbTOHsl(c);
                    if (EffectHelper.GetEffectName(eff.EffectType) == "Star")
                    {
                        DynValue randomHue_dv = script.LoadFunction(Constants.RandomHueString);
                        _functionDictionary.Add(randomHue_dv, Constants.RandomHueString);
                        initColorTable.Set("hue", randomHue_dv);
                    }
                    else
                        initColorTable.Set("hue", DynValue.NewNumber(hsl[0]));
                    initColorTable.Set("saturation", DynValue.NewNumber(hsl[1]));
                    initColorTable.Set("lightness", DynValue.NewNumber(hsl[2]));
                    initColorTable.Set("alpha", DynValue.NewNumber(c.A / 255));
                    effectTable.Set("initColor", DynValue.NewTable(initColorTable));
                    effectTable.Set("viewportTransform", DynValue.NewTable(
                        EffectHelper.GetViewportTransformTable(_functionDictionary, script, eff.EffectType))
                        );
                    effectTable.Set("bindToSlot", DynValue.NewTable(EffectHelper.GetBindToSlotTable(script, eff.EffectType)));

                    string waveTypeString = "";
                    switch(info.WaveType)
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
                    waveTable.Set("velocity", DynValue.NewNumber(EffectHelper.GetVelocityValue(eff.EffectType)));
                    effectTable.Set("wave", DynValue.NewTable(waveTable));

                    eventTable.Set(eff.EffectLuaName, DynValue.NewTable(effectTable));
                }
            }
            return eventTable;
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
    }
}