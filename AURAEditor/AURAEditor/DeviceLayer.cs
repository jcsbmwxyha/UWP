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
using static AuraEditor.Common.EffectHelper;

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
            Effects = new List<Effect>();
            UICanvas = CreateUICanvas();
            _deviceToZonesDictionary = new Dictionary<int, int[]>();
            Eye = true;
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
        public void InsertEffectLine(Effect insertedEL)
        {
            Effect overlappedEL = null;

            overlappedEL = TestAndGetFirstOverlappingEffect(insertedEL);

            if (overlappedEL != null)
            {
                if (insertedEL.UI_X <= overlappedEL.UI_X)
                {
                    double move = insertedEL.UI_Right - overlappedEL.UI_X;
                    PushAllEffectsWhichOnTheRight(insertedEL, move);
                }
                else if (overlappedEL.UI_X < insertedEL.UI_X)
                {
                    insertedEL.UI_X += (overlappedEL.UI_Right - insertedEL.UI_X);
                    InsertEffectLine(insertedEL);
                }
            }
        }
        public void PushAllEffectsWhichOnTheRight(Effect effect, double move)
        {
            foreach (Effect e in Effects)
            {
                if (effect.Equals(e))
                    continue;

                if (effect.UI_X <= e.UI_X)
                {
                    e.UI_X += move;
                }
            }
        }
        public Effect TestAndGetFirstOverlappingEffect(Effect testEffect)
        {
            Effect result = null;

            foreach (Effect e in Effects)
            {
                if (e.Equals(testEffect))
                    continue;

                if (IsOverlapping(testEffect, e))
                {
                    if (result == null)
                        result = e;
                    else if (e.UI_X < result.UI_X)
                    {
                        result = e;
                    }
                }
            }

            return result;
        }
        private bool IsOverlapping(Effect effect1, Effect effect2)
        {
            return ControlHelper.IsOverlapping(effect1.UI_X, effect1.UI_Width, effect2.UI_X, effect2.UI_Width);
        }
        public Effect FindEffectByPosition(double x)
        {
            foreach (Effect e in Effects)
            {
                double left = e.UI_X;
                double width = e.UI_Width;

                if ((left <= x) && (x <= left + width))
                    return e;
            }
            return null;
        }
        public Effect FindFirstEffectOnTheRight(double x)
        {
            Effect result = null;

            foreach (Effect e in Effects)
            {
                if (e.UI_X > x)
                {
                    if (result == null)
                        result = e;

                    if (e.UI_X < result.UI_X)
                    {
                        result = e;
                    }
                }
            }
            return result;
        }
        public double GetFirstFreeRoomPosition()
        {
            double roomX = 0;

            for (int i = 0; i < Effects.Count; i++)
            {
                Effect effect = Effects[i];
                if (roomX <= effect.UI_X && effect.UI_X < roomX + AuraCreatorManager.GetPixelsPerSecond())
                {
                    roomX = effect.UI_X + effect.UI_Width;
                    i = -1; // rescan every effect line
                }
            }

            return roomX;
        }

        private async void Canvas_DragOver(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.Text))
            {
                var effectname = await e.DataView.GetTextAsync();

                if (!IsCommonEffect(effectname))
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
                int type = GetEffectIndex(effectname);

                Effect effect = new Effect(this, type);
                AddEffect(effect);
            }
        }
    }
}
