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
using AuraEditor.UserControls;

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
            Effect overlappedEL = TestAndGetFirstOverlappingEffect(insertedEL);

            if (overlappedEL != null)
            {
                EffectLine overUI = overlappedEL.UI;
                EffectLine inUI = insertedEL.UI;

                if (inUI.X <= overUI.X)
                {
                    double move = inUI.Right - overUI.X;
                    PushAllEffectsWhichOnTheRight(insertedEL, move);
                }
                else if (overUI.X < inUI.X)
                {
                    inUI.X += (overUI.Right - inUI.X);
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

                if (effect.UI.X <= e.UI.X)
                {
                    e.UI.X += move;
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
                    else if (e.UI.X < result.UI.X)
                    {
                        result = e;
                    }
                }
            }

            return result;
        }
        private bool IsOverlapping(Effect effect1, Effect effect2)
        {
            EffectLine UI_1 = effect1.UI;
            EffectLine UI_2 = effect2.UI;

            return ControlHelper.IsOverlapping(
                UI_1.X, UI_1.Width,
                UI_2.X, UI_2.Width);
        }
        public Effect FindEffectByPosition(double x)
        {
            foreach (Effect e in Effects)
            {
                double left = e.UI.X;
                double width = e.UI.Width;

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
                if (e.UI.X > x)
                {
                    if (result == null)
                        result = e;

                    if (e.UI.X < result.UI.X)
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
                EffectLine UI = effect.UI;

                if (roomX <= UI.X && UI.X < roomX + AuraCreatorManager.GetPixelsPerSecond())
                {
                    roomX = UI.X + UI.Width;
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
