using AuraEditor.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace AuraEditor
{
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
