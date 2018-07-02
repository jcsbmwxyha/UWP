using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// 空白頁項目範本已記錄在 https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x404

namespace TimeLineTest
{
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
        public int Duration
        {
            get { return _duration; }
            set
            {
                _duration = value;
                UIBorder.Width = value;
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
            Start = 0;
            Duration = 100;

            _cursorSizeRight = false;
            _cursorSizeLeft = false;
            _cursorMove = false;
        }
        public Border CreateUIBorder(int effectType)
        {
            //string effectname = EffectHelper.GetEffectName(effectType);
            string effectname = "123";

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

            border.Background = new SolidColorBrush(Windows.UI.Colors.Red);

            CompositeTransform ct = new CompositeTransform
            {
                TranslateX = 0
            };
            border.RenderTransform = ct;

            return border;
        }
    }
    public class DeviceGroup
    {
        public string GroupName { get; set; }
        public List<Effect> Effects;
        public Canvas UICanvas;
        Dictionary<int, int[]> _deviceToZonesDictionary;

        public DeviceGroup(string name = "")
        {
            GroupName = name;
            //_devices = new List<Device>();
            Effects = new List<Effect>();
            UICanvas = CreateUICanvas();
            _deviceToZonesDictionary = new Dictionary<int, int[]>();
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
            UICanvas.Children.Add(effect.UIBorder);
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
        private Canvas CreateUICanvas()
        {
            Canvas canvas = new Canvas
            {
                Width = 5000,
                Height = 50
            };

            return canvas;
        }
    }

    public sealed partial class MainPage : Page
    {
        public ObservableCollection<DeviceGroup> DeviceGroupCollection { get; set; }

        public MainPage()
        {
            this.InitializeComponent();

            DeviceGroupCollection = new ObservableCollection<DeviceGroup>();

            DeviceGroup dg1 = new DeviceGroup("111");
            DeviceGroup dg2 = new DeviceGroup("222");
            DeviceGroup dg3 = new DeviceGroup("333");

            DeviceGroupCollection.Add(dg1);
            DeviceGroupCollection.Add(dg2);
            DeviceGroupCollection.Add(dg3);

        }
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            // TimeUnit : the seconds between two number(long line)
            int secondsPerTimeUnit = 15;
            int minimumScaleUnitLength = 100;
            int timeUnitLength = 300;

            TimeSpan ts = new TimeSpan(0, 0, secondsPerTimeUnit);
            TimeSpan interval = new TimeSpan(0, 0, secondsPerTimeUnit);

            int width = (int)TimeLineScaleCanvas.ActualWidth;
            int height = (int)TimeLineScaleCanvas.ActualHeight;
            int y1_short = height / 2;
            int y1_long = height / 4;
            double y2 = height;

            int linePerTimeUnit = timeUnitLength / minimumScaleUnitLength;
            int totalLineCount = width / minimumScaleUnitLength;

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
                    
                    TimeLineScaleCanvas.Children.Add(tb);
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

                TimeLineScaleCanvas.Children.Add(line);
            }
        }

        private void DeviceList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        private void ScrollViewer_ViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            ScrollViewer sv = sender as ScrollViewer;
            LeftScrollViewer.ChangeView(null, sv.VerticalOffset, null, true);
        }

    }
}
