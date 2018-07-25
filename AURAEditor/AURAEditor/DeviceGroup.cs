#define DEBUG
using System.Collections.Generic;
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
using AuraEditor.UserControls;
using Windows.UI.Xaml.Shapes;
using Constants = AuraEditor.Common.Constants;

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
        public double Velocity { get; set; }

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
            Velocity = 0;
        }
        public EffectInfo(int type)
        {
            Color = Colors.Red;
            WaveType = EffectHelper.GetSuggestedWaveTypeValue(type);
            Min = EffectHelper.GetSuggestedMinValue(type);
            Max = EffectHelper.GetSuggestedMaxValue(type);
            WaveLength = EffectHelper.GetSuggestedWaveLenValue(type);
            Freq = EffectHelper.GetSuggestedFreqValue(type);
            Phase = EffectHelper.GetSuggestedPhaseValue(type);
            Start = 0;
            Velocity = EffectHelper.GetSuggestedVelocityValue(type);
        }
    }

    public class Effect
    {
        public DeviceLayer MyDeviceLayer { get; set; }
        public string EffectName { get; set; }
        public string EffectLuaName { get; set; }
        public int EffectType { get; set; }
        public EffectLine EffectLineUI { get; }
        private int _start;
        public int Start
        {
            get { return _start; }
            set
            {
                CompositeTransform ct = EffectLineUI.RenderTransform as CompositeTransform;
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
                EffectLineUI.Width = value;
            }
        }
        private EffectInfo _info;
        public EffectInfo Info
        {
            get { return _info; }
            set
            {
                if ((EffectType != 3) && (EffectType != 6) && (EffectType != 2))
                    EffectLineUI.Background = new SolidColorBrush(value.Color);
                _info = value;
            }
        }

        public Effect(DeviceLayer dg, int effectType)
        {
            MyDeviceLayer = dg;
            EffectType = effectType;
            EffectName = EffectHelper.GetEffectName(effectType);
            EffectLineUI = CreateEffectUI(effectType);
            EffectLineUI.DataContext = this;
            Start = (int)MyDeviceLayer.GetFirstSpaceCanPut();
            Duration = 100;
            Info = new EffectInfo(effectType);
        }
        private EffectLine CreateEffectUI(int effectType)
        {
            
            EffectLine el = new EffectLine
            {
                Height = 34,
                Width = 100,
                HorizontalAlignment = HorizontalAlignment.Center,
                ManipulationMode = ManipulationModes.TranslateX
            };

            CompositeTransform ct = el.RenderTransform as CompositeTransform;
            ct.TranslateY = 5;

            return el;
        }
    }
    public class LightZone
    {
        public Shape Frame;
        public int PhysicalIndex;
        public int UIindex;
        public Rect RelativeZoneRect;
        public Rect AbsoluteZoneRect
        {
            get
            {
                CompositeTransform ct = Frame.RenderTransform as CompositeTransform;
                return new Rect(
                    new Point(ct.TranslateX, ct.TranslateY),
                    new Point(ct.TranslateX + RelativeZoneRect.Width, ct.TranslateY + RelativeZoneRect.Height)
                    );
            }
        }
        public bool Selected;

        public LightZone(int p_idx, int ui_idx, int parentX, int parentY, int x1, int y1, int x2, int y2)
        {
            PhysicalIndex = p_idx;
            UIindex = ui_idx;
            Selected = false;

            RelativeZoneRect = new Rect(new Point(x1, y1), new Point(x2, y2));
            Frame = CreateRectangle(
                new Rect(
                    new Point(x1 + parentX * Constants.GridLength, y1 + parentY * Constants.GridLength),
                    new Point(x2 + parentX * Constants.GridLength, y2 + parentY * Constants.GridLength))
                );
        }
        private Rectangle CreateRectangle(Windows.Foundation.Rect Rect)
        {
            CompositeTransform ct = new CompositeTransform
            {
                TranslateX = Rect.X,
                TranslateY = Rect.Y
            };

            Rectangle rectangle = new Rectangle
            {
                Fill = new SolidColorBrush(Colors.Transparent),
                StrokeThickness = 3,
                RenderTransform = ct,
                Width = Rect.Width,
                Height = Rect.Height,
                HorizontalAlignment = 0,
                VerticalAlignment = 0,
                RadiusX = 3,
                RadiusY = 4
            };

            rectangle.Stroke = new SolidColorBrush(Colors.Black);

            return rectangle;
        }
        public void Frame_StatusChanged(int regionIndex, int status)
        {
            if (status == RegionStatus.Normal)
            {
                Frame.Stroke = new SolidColorBrush(Colors.Black);
                Frame.Fill = new SolidColorBrush(Colors.Transparent);
                Selected = false;
            }
            else if (status == RegionStatus.NormalHover)
            {
                Frame.Stroke = new SolidColorBrush(Colors.Black);
                Frame.Fill = new SolidColorBrush(new Color { A = 100, R = 0, G = 0, B = 123 });
                Selected = false;
            }
            else if (status == RegionStatus.Selected)
            {
                Frame.Stroke = new SolidColorBrush(Colors.Red);
                Frame.Fill = new SolidColorBrush(Colors.Transparent);
                Selected = true;
            }
            else
            {
                Frame.Stroke = new SolidColorBrush(Colors.Red);
                Frame.Fill = new SolidColorBrush(new Color { A = 100, R = 0, G = 0, B = 123 });
                Selected = true;
            }
        }
    }
    public class Device
    {
        public string DeviceName { get; set; }
        public int DeviceType { get; set; }
        public double X
        {
            get
            {
                CompositeTransform ct = DeviceImg.RenderTransform as CompositeTransform;
                return ct.TranslateX / Constants.GridLength;
            }
            set
            {
                CompositeTransform ct = DeviceImg.RenderTransform as CompositeTransform;
                ct.TranslateX = value * Constants.GridLength;
            }
        }
        public double Y
        {
            get
            {
                CompositeTransform ct = DeviceImg.RenderTransform as CompositeTransform;
                return ct.TranslateY / Constants.GridLength;
            }
            set
            {
                CompositeTransform ct = DeviceImg.RenderTransform as CompositeTransform;
                ct.TranslateY = value * Constants.GridLength;
            }
        }
        public double W { get; set; }
        public double H { get; set; }
        public Image DeviceImg { get; set; }
        public string DeviceImgPath { get; set; }
        public LightZone[] LightZones { get; set; }

        public Device(Image img)
        {
            DeviceImg = img;
            EnableManipulation();
        }
        public void EnableManipulation()
        {
            DeviceImg.ManipulationStarted += ImageManipulationStarted;
            DeviceImg.ManipulationDelta += ImageManipulationDelta;
            DeviceImg.ManipulationCompleted += ImageManipulationCompleted;
            DeviceImg.PointerEntered += ImagePointerEntered;
            DeviceImg.PointerExited += ImagePointerExited;
        }
        public void DisableManipulation()
        {
            DeviceImg.ManipulationStarted -= ImageManipulationStarted;
            DeviceImg.ManipulationDelta -= ImageManipulationDelta;
            DeviceImg.ManipulationCompleted -= ImageManipulationCompleted;
            DeviceImg.PointerEntered -= ImagePointerEntered;
            DeviceImg.PointerExited -= ImagePointerExited;
        }
        private void ImageManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            var fe = sender as FrameworkElement;
            fe.Opacity = 0.5;

            var frame = (Frame)Window.Current.Content;
            var page = (MainPage)frame.Content;
            page.DragingDeviceImage = true;
        }
        private void ImageManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            Image img = sender as Image;
            CompositeTransform ct = img.RenderTransform as CompositeTransform;
            ct.TranslateX += e.Delta.Translation.X;
            ct.TranslateY += e.Delta.Translation.Y;

            foreach(var zone in LightZones)
            {
                ct = zone.Frame.RenderTransform as CompositeTransform;

                ct.TranslateX += e.Delta.Translation.X;
                ct.TranslateY += e.Delta.Translation.Y;
            }
        }
        private void ImageManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            Image img = sender as Image;
            img.Opacity = 1;

            CompositeTransform ct = img.RenderTransform as CompositeTransform;
            CompositeTransform zone_ct;

            ct.TranslateX = (int)ct.TranslateX / Constants.GridLength * Constants.GridLength;
            ct.TranslateY = (int)ct.TranslateY / Constants.GridLength * Constants.GridLength;

            foreach (var zone in LightZones)
            {
                zone_ct = zone.Frame.RenderTransform as CompositeTransform;

                zone_ct.TranslateX = (int)ct.TranslateX + zone.RelativeZoneRect.Left;
                zone_ct.TranslateY = (int)ct.TranslateY + zone.RelativeZoneRect.Top;
            }

            var frame = (Frame)Window.Current.Content;
            var page = (MainPage)frame.Content;
            page.UpdateDeviceZoneRegions();
        }
        private void ImagePointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.SizeAll, 0);

            var frame = (Frame)Window.Current.Content;
            var page = (MainPage)frame.Content;
            page.DragingDeviceImage = true;
        }
        private void ImagePointerExited(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);

            var frame = (Frame)Window.Current.Content;
            var page = (MainPage)frame.Content;
            page.DragingDeviceImage = false;
        }
    }

    public class DeviceLayer
    {
        public string LayerName { get; set; }
        public List<Effect> Effects;
        public Canvas UICanvas;
        public bool Eye { get; set; }
        Dictionary<int, int[]> _deviceToZonesDictionary;

        public DeviceLayer(string name = "")
        {
            LayerName = name;
            //_devices = new List<Device>();
            Effects = new List<Effect>();
            UICanvas = CreateUICanvas();
            _deviceToZonesDictionary = new Dictionary<int, int[]>();
            Eye = true;
        }
        public Dictionary<int, int[]> GetDeviceToZonesDictionary()
        {
            return _deviceToZonesDictionary;
        }
        public void AddDeviceZones(Dictionary<int, int[]> dictionary)
        {
            if (dictionary == null)
                return;

            foreach (var item in dictionary)
            {
                if (!_deviceToZonesDictionary.ContainsKey(item.Key))
                {
                    _deviceToZonesDictionary.Add(item.Key, item.Value);
                }
            }
        }
        public void AddDeviceZones(int type, int[] indexes)
        {
            _deviceToZonesDictionary.Add(type, indexes);
        }
        public void AddEffect(Effect effect)
        {
            Effects.Add(effect);
            UICanvas.Children.Add(effect.EffectLineUI);
        }
        private async void Canvas_DragOver(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.Text))
            {
                var effectname = await e.DataView.GetTextAsync();

                if (!EffectHelper.IsCommonEffect(effectname))
                    e.AcceptedOperation = DataPackageOperation.None;
                else
                    e.AcceptedOperation = DataPackageOperation.Copy;
            }
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
        public int InsertEffectLine(Effect selectedEffect, int leftposition, int w)
        {
            int oldStart = selectedEffect.Start;

            Effect coveredEffectLine = null;
            int newLeftposition = leftposition;

            // Step 1 : Determine if the leftpoint on someone effectline
            foreach (Effect e in Effects)
            {
                if (e != selectedEffect && e.Start <= leftposition && e.Start + e.Duration > leftposition)
                {
                    coveredEffectLine = e;
                    break;
                }
            }

            // Step 2 : Calculate leftpoint position
            if (coveredEffectLine != null)
            {
                // if have same Start, move coveredEffectLine to back
                if (coveredEffectLine.Start != leftposition)
                    newLeftposition = coveredEffectLine.Start + coveredEffectLine.Duration;
            }

            // Step 3 : determine all effectlines position on the right hand side
            bool needToAdjustPosition = false;
            int distanceToMove = 0;
            foreach (Effect e in Effects)
            {
                // if there is overlap to selected effectline
                if (e != selectedEffect && e.Start >= newLeftposition && e.Start < newLeftposition + w)
                {
                    needToAdjustPosition = true;
                    int len = selectedEffect.Duration - (e.Start - newLeftposition);

                    // There may be many e which is overlap to selected effectline.
                    // We should find the longgest distance.
                    if (len > distanceToMove)
                        distanceToMove = len;
                }
            }

            if (needToAdjustPosition == true)
            {
                if (newLeftposition < oldStart)
                    foreach (Effect e in Effects)
                    {
                        if (e != selectedEffect && e.Start >= newLeftposition && e.Start < oldStart)
                        {
                            e.Start += distanceToMove;
                        }
                    }
                else
                    foreach (Effect e in Effects)
                    {
                        if (e != selectedEffect && e.Start >= newLeftposition/* && e.Start < newLeftposition + w*/)
                        {
                            e.Start += distanceToMove;
                        }
                    }
            }

            return newLeftposition;
        }
        public void OnCursorSizeRight(Effect effect, int x, int w)
        {
            Effect overlapEff = IsEffectLineOverlap(effect, x, w);
            int moveLength = 0;

            if (overlapEff != null)
            {
                moveLength = (x + w) - overlapEff.Start;
                foreach (Effect e in Effects)
                {
                    if (!e.Equals(effect))
                    {
                        if (x < e.Start)
                            e.Start += moveLength;
                    }
                }
            }
        }
        public Effect IsEffectLineOverlap(Effect effect, int x, int w)
        {
            foreach (Effect e in Effects)
            {
                if (!e.Equals(effect))
                {
                    if ((x < e.Start + e.Duration) && (x + w > e.Start))
                        return e;
                }
            }
            return null;
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
            Thickness margin = new Thickness(0, 3, 0, 3);
            Canvas canvas = new Canvas
            {
                Width = 5000,
                Height = 44,
                Margin = margin
            };

            canvas.DragOver += Canvas_DragOver;
            canvas.Drop += Canvas_Drop;

            return canvas;
        }
    }
    public class TriggerDeviceLayer : DeviceLayer
    {
        Dictionary<int, int[]> deviceToZonesDictionary;

        public TriggerDeviceLayer()
        {
            LayerName = "Trigger Effect";
            Effects = new List<Effect>();
            UICanvas = CreateUICanvas();


            UICanvas.Background = new SolidColorBrush(Colors.Purple);

            deviceToZonesDictionary = new Dictionary<int, int[]>();
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
        private Canvas CreateUICanvas()
        {
            Thickness margin = new Thickness(0, 3, 0, 3);
            Canvas canvas = new Canvas
            {
                Width = 5000,
                Height = 44,
                Margin = margin
            };

            canvas.DragOver += Canvas_DragOver;
            canvas.Drop += Canvas_Drop;

            return canvas;
        }
    }

    public class AuraCreatorManager
    {
        public ObservableCollection<DeviceLayer> DeviceLayerCollection { get; set; }
        static StackPanel TimeLineStackPanel;
        Script script;
        Dictionary<DynValue, string> _functionDictionary;
        public List<Device> GlobalDevices;

        public AuraCreatorManager(StackPanel sp)
        {
            script = new Script();
            TimeLineStackPanel = sp;
            DeviceLayerCollection = new ObservableCollection<DeviceLayer>();
            _functionDictionary = new Dictionary<DynValue, string>();
            GlobalDevices = new List<Device>();
            AddTriggerDeviceLayer();
        }
        private void AddTriggerDeviceLayer()
        {
            TriggerDeviceLayer tdg = new TriggerDeviceLayer();
            tdg.LayerName = "Trigger Effect";
            tdg.UICanvas.Background = AuraEditorColorHelper.GetTimeLineBackgroundColor(0);

            tdg.AddDeviceZones(0, new int[] { -1 });
            tdg.AddDeviceZones(1, new int[] { -1 });
            //tdg.AddDeviceZones(2, new int[] { -1 });
            //tdg.AddDeviceZones(3, new int[] { -1 });

            DeviceLayerCollection.Add(tdg);
            TimeLineStackPanel.Children.Add(tdg.UICanvas);
        }
        public void AddDeviceLayer(DeviceLayer dg)
        {
            if (DeviceLayerCollection.Count % 2 == 0)
                dg.UICanvas.Background = AuraEditorColorHelper.GetTimeLineBackgroundColor(3);
            else
                dg.UICanvas.Background = AuraEditorColorHelper.GetTimeLineBackgroundColor(3);

            DeviceLayerCollection.Add(dg);
            TimeLineStackPanel.Children.Add(dg.UICanvas);
        }
        public void RemoveDeviceLayer(DeviceLayer dg)
        {
            DeviceLayerCollection.Remove(dg);
            TimeLineStackPanel.Children.Remove(dg.UICanvas);
        }
        public void SetGlobalDevices(List<Device> devices)
        {
            GlobalDevices = devices;
        }
        public void ClearAllLayer()
        {
            TimeLineStackPanel.Children.Clear();
            DeviceLayerCollection.Clear();
        }
        public Device GetGlobalDevice(int type)
        {
            return GlobalDevices.Find(x => x.DeviceType == type);
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
            Table eventProviderTable = CreateNewTable();
            Table queueTable = CreateNewTable();
            Table queueItemTable;

            int effectCount = 0;
            int queueIndex = 1;
            foreach (DeviceLayer gp in DeviceLayerCollection)
            {
                if (gp.Eye == false)
                    continue;

                foreach (Effect eff in gp.Effects)
                {
                    queueItemTable = CreateNewTable();

                    // Give uniqle index for all of effect lines
                    eff.EffectLuaName = EffectHelper.GetEffectName(eff.EffectType) + effectCount.ToString();
                    effectCount++;

                    queueItemTable.Set("Effect", DynValue.NewString(eff.EffectLuaName));
                    queueItemTable.Set("Viewport", DynValue.NewString(gp.LayerName));

                    if (EffectHelper.GetEffectName(eff.EffectType) == "Comet")
                        queueItemTable.Set("Trigger", DynValue.NewString("Period"));
                    else if(EffectHelper.IsTriggerEffects(eff.EffectType))
                        queueItemTable.Set("Trigger", DynValue.NewString("KeyboardInput"));
                    else
                        queueItemTable.Set("Trigger", DynValue.NewString("OneTime"));

                    queueItemTable.Set("Delay", DynValue.NewNumber(eff.Start * 10));
                    queueItemTable.Set("Duration", DynValue.NewNumber(eff.Duration * 10));
                    queueTable.Set(queueIndex, DynValue.NewTable(queueItemTable));
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
            Table layerTable;
            Table deviceTable;
            Table layoutTable;
            Table locationTable;
            Table usageTable;

            int layerIndex = 1;
            foreach (DeviceLayer dg in DeviceLayerCollection)
            {
                Dictionary<int, int[]> deviceToZonesDictionary = dg.GetDeviceToZonesDictionary();
                layerTable = CreateNewTable();

                foreach (KeyValuePair<int, int[]> pair in deviceToZonesDictionary)
                {
                    Device d = GetGlobalDevice(pair.Key);

                    layoutTable = CreateNewTable();
                    locationTable = CreateNewTable();
                    deviceTable = CreateNewTable();
                    usageTable = CreateNewTable();
                    
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

                    deviceTable.Set("location", DynValue.NewTable(locationTable));

                    usageTable = CreateNewTable();

                    if (dg is TriggerDeviceLayer)
                    {
                        int count = 1;
                        foreach (var gd in GlobalDevices)
                        {
                            foreach (var zone in gd.LightZones)
                            {
                                usageTable.Set(count, DynValue.NewNumber(zone.PhysicalIndex));
                                count++;
                            }
                        }
                    }
                    else if (pair.Value != null)
                    {
                        int count = 1;
                        foreach (int phyIndex in pair.Value)
                        {
                            usageTable.Set(count, DynValue.NewNumber(phyIndex));
                            count++;
                        };
                    }
                    //else
                    //{
                    //    for (int count = 0; count < 169; count++)
                    //        usageTable.Set(count, DynValue.NewNumber(count));
                    //}
                    deviceTable.Set("usage", DynValue.NewTable(usageTable));

                    layerTable.Set(d.DeviceName, DynValue.NewTable(deviceTable));
                }
                viewPortTable.Set(dg.LayerName, DynValue.NewTable(layerTable));
                layerIndex++;
            }
            return viewPortTable;
        }
        private Table GetEventTable() {
            Table eventTable = CreateNewTable();
            Table effectTable;
            Table initColorTable;
            Table waveTable;

            foreach (DeviceLayer gp in DeviceLayerCollection)
            {
                if (gp.Eye == false)
                    continue;

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
                    waveTable.Set("velocity", DynValue.NewNumber(info.Velocity));
                    effectTable.Set("wave", DynValue.NewTable(waveTable));

                    eventTable.Set(eff.EffectLuaName, DynValue.NewTable(effectTable));
                }
            }
            return eventTable;
        }
        private Table GetGlobalSpaceTable()
        {
            Table globalSpace = CreateNewTable();
            Table deviceTable;
            Table locationTable;
            string deviceTypeName = "";

            foreach (Device d in GlobalDevices)
            {
                deviceTable = CreateNewTable();
                locationTable = CreateNewTable();

                locationTable.Set("x", DynValue.NewNumber(d.X));
                locationTable.Set("y", DynValue.NewNumber(d.Y));

                switch (d.DeviceType)
                {
                    case 0: deviceTypeName = "Notebook"; break;
                    case 1: deviceTypeName = "Mouse"; break;
                    case 2: deviceTypeName = "Keyboard"; break;
                    case 3: deviceTypeName = "Headset"; break;
                }
                deviceTable.Set("DeviceType", DynValue.NewString(deviceTypeName));
                deviceTable.Set("location", DynValue.NewTable(locationTable));

                globalSpace.Set(d.DeviceName, DynValue.NewTable(deviceTable));
            }

            return globalSpace;
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