using AuraEditor.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using static AuraEditor.MainPage;

namespace AuraEditor
{
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
}
