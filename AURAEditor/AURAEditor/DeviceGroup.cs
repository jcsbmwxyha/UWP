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
using static AuraEditor.MainPage;

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
        public DeviceLayer Layer { get; set; }
        public string EffectName { get; set; }
        public string EffectLuaName { get; set; }
        public int EffectType { get; set; }
        public EffectLine UI { get; }
        public double UI_X
        {
            get
            {
                CompositeTransform ct = UI.RenderTransform as CompositeTransform;
                return ct.TranslateX;
            }
            set
            {
                CompositeTransform ct = UI.RenderTransform as CompositeTransform;
                ct.TranslateX = value;
            }
        }
        public double UI_Width {
            get { return UI.Width; }
            set
            {
                UI.Width = value;
            }
        }
        public double StartTime
        {
            get
            {
                CompositeTransform ct = UI.RenderTransform as CompositeTransform;
                double timeUnits = ct.TranslateX / AuraCreatorManager.pixelsPerTimeUnit;
                double seconds = timeUnits * AuraCreatorManager.secondsPerTimeUnit;

                return seconds * 1000;
            }
            set
            {
                CompositeTransform ct = UI.RenderTransform as CompositeTransform;
                double seconds = value / 1000;
                double timeUnits = seconds / AuraCreatorManager.secondsPerTimeUnit;

                ct.TranslateX = timeUnits * AuraCreatorManager.pixelsPerTimeUnit;
            }
        }
        public double DurationTime
        {
            get
            {
                double timeUnits = UI.Width / AuraCreatorManager.pixelsPerTimeUnit;
                double seconds = timeUnits * AuraCreatorManager.secondsPerTimeUnit;

                return seconds * 1000;
            }
            set
            {
                double seconds = value / 1000;
                double timeUnits = seconds / AuraCreatorManager.secondsPerTimeUnit;

                UI.Width = timeUnits * AuraCreatorManager.pixelsPerTimeUnit;
            }
        }

        private EffectInfo _info;
        public EffectInfo Info
        {
            get { return _info; }
            set
            {
                if ((EffectType != 3) && (EffectType != 6) && (EffectType != 2))
                    UI.Background = new SolidColorBrush(value.Color);
                _info = value;
            }
        }

        public Effect(DeviceLayer layer, int effectType)
        {
            Layer = layer;
            EffectType = effectType;
            EffectName = EffectHelper.GetEffectName(effectType);
            UI = CreateEffectUI(effectType);
            UI.DataContext = this;
            UI_X = (int)Layer.GetFirstFreeRoomPosition();
            DurationTime = 1000; // 1s
            Info = new EffectInfo(effectType);
        }
        private EffectLine CreateEffectUI(int effectType)
        {
            EffectLine el = new EffectLine
            {
                Height = 34,
                Width = AuraCreatorManager.GetPixelsPerSecond(),
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
        public void Frame_StatusChanged(int regionIndex, RegionStatus status)
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
        private double _oldX;
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
        private double _oldY;
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
        public LightZone[] LightZones { get; set; }

        public Device(Image img)
        {
            DeviceImg = img;

            DeviceImg.PointerPressed += Image_PointerPressed;
            DeviceImg.PointerReleased += Image_PointerReleased;
            DeviceImg.Tapped += DeviceImg_Tapped;
        }
        public void EnableManipulation()
        {
            DeviceImg.ManipulationStarted -= ImageManipulationStarted;
            DeviceImg.ManipulationDelta -= ImageManipulationDelta;
            DeviceImg.ManipulationCompleted -= ImageManipulationCompleted;
            DeviceImg.PointerEntered -= ImagePointerEntered;
            DeviceImg.PointerExited -= ImagePointerExited;

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

        private void DeviceImg_Tapped(object sender, TappedRoutedEventArgs e)
        {
            MainPageInstance.SetSpaceStatus(SpaceStatus.Normal);
        }
        private void Image_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            //MainPageInstance.UpdateSpaceGridOperations(SpaceStatus.DragingDevice);
        }
        private void Image_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            //MainPageInstance.UpdateSpaceGridOperations(SpaceStatus.Normal);
        }
        private void ImageManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            var fe = sender as FrameworkElement;
            fe.Opacity = 0.5;

            _oldX = X;
            _oldY = Y;
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
            CompositeTransform ct = img.RenderTransform as CompositeTransform;
            CompositeTransform zone_ct;

            // TODO : ++ functionalized  this part
            img.Opacity = 1;
            ct.TranslateX = (int)ct.TranslateX / Constants.GridLength * Constants.GridLength;
            ct.TranslateY = (int)ct.TranslateY / Constants.GridLength * Constants.GridLength;

            foreach (var zone in LightZones)
            {
                zone_ct = zone.Frame.RenderTransform as CompositeTransform;
                zone_ct.TranslateX = (int)ct.TranslateX + zone.RelativeZoneRect.Left;
                zone_ct.TranslateY = (int)ct.TranslateY + zone.RelativeZoneRect.Top;
            }
            // TODO : --

            MainPageInstance.SetDevicePosition(this,
                (int)(X - _oldX) * Constants.GridLength,
                (int)(Y - _oldY) * Constants.GridLength);
        }
        private void ImagePointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor 
                = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.SizeAll, 0);

            //MainPageInstance.UpdateSpaceGridStatus(SpaceStatus.DragingDevice);
        }
        private void ImagePointerExited(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor 
                = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
            
            //MainPageInstance.UpdateSpaceGridStatus(SpaceStatus.Normal);
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
            UICanvas.Children.Add(effect.UI);
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
        public double InsertEffectLine(Effect selectedEffect)
        {
            double oldLeft = selectedEffect.UI_X;
            double width = selectedEffect.UI_Width;
            double newLeft = oldLeft;
            Effect coveredEffect = null;
            bool needToAdjustPosition = false;
            double distanceToMove = 0;

            // Step 1 : Determine if X of selectedEffect on someone effectline
            foreach (Effect e in Effects)
            {
                if (e != selectedEffect && e.UI_X <= oldLeft && e.UI_X + e.UI_Width > oldLeft)
                {
                    coveredEffect = e;
                    break;
                }
            }

            // Step 2 : Calculate leftpoint position
            if (coveredEffect != null)
            {
                // if have same Start, move coveredEffectLine behind selectedEffect
                if (coveredEffect.UI_X != oldLeft)
                    newLeft = coveredEffect.UI_X + coveredEffect.UI_Width;
            }

            // Step 3 : determine all effectlines position on the right hand side
            foreach (Effect e in Effects)
            {
                // if there is overlap to selected effectline
                if (e != selectedEffect && e.UI_X >= newLeft && e.UI_X < newLeft + width)
                {
                    needToAdjustPosition = true;
                    double len = selectedEffect.UI_Width - (e.UI_X - newLeft);

                    // There may be many e which is overlap to selected effectline.
                    // We should find the longgest distance.
                    if (len > distanceToMove)
                        distanceToMove = len;
                }
            }

            if (needToAdjustPosition == true)
            {
                if (newLeft < oldLeft)
                    foreach (Effect e in Effects)
                    {
                        if (e != selectedEffect && e.UI_X >= newLeft && e.UI_X < oldLeft)
                        {
                            e.UI_X += distanceToMove;
                        }
                    }
                else
                    foreach (Effect e in Effects)
                    {
                        if (e != selectedEffect && e.UI_X >= newLeft/* && e.Start < newLeftposition + w*/)
                        {
                            e.UI_X += distanceToMove;
                        }
                    }
            }

            return newLeft;
        }
        public void OnCursorSizeRight(Effect effect)
        {
            Effect overlapEff = TestOverlap(effect);
            double left = effect.UI_X;
            double width = effect.UI_Width;
            double moveLength = 0;

            if (overlapEff != null)
            {
                moveLength = (left + width) - overlapEff.UI_X;
                foreach (Effect e in Effects)
                {
                    if (!e.Equals(effect))
                    {
                        if (left < e.UI_X)
                            e.UI_X += moveLength;
                    }
                }
            }
        }
        public Effect TestOverlap(Effect effect)
        {
            double left = effect.UI_X;
            double width = effect.UI_Width;

            foreach (Effect e in Effects)
            {
                if (!e.Equals(effect))
                {
                    if ((left < e.UI_X + e.UI_Width) && (left + width > e.UI_X))
                        return e;
                }
            }
            return null;
        }
        public Effect FindEffectByPosition(double x)
        {
            foreach (Effect e in Effects)
            {
                double left = e.UI_X;
                double width = e.UI_Width;

                if ((left <= x) && (left + width >= x))
                    return e;
            }
            return null;
        }
        public double GetFirstFreeRoomPosition()
        {
            double RoomX = 0;

            for (int i = 0; i < Effects.Count; i++)
            {
                Effect effect = Effects[i];
                if (RoomX <= effect.UI_X && effect.UI_X < RoomX + AuraCreatorManager.GetPixelsPerSecond())
                {
                    RoomX = effect.UI_X + effect.UI_Width;
                    i = -1; // rescan every effect line
                }
            }
            
            return RoomX;
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
}