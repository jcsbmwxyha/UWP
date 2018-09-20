using AuraEditor.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using static AuraEditor.Common.EffectHelper;
using static AuraEditor.Common.LuaHelper;
using static AuraEditor.Common.ControlHelper;
using AuraEditor.UserControls;
using MoonSharp.Interpreter;

namespace AuraEditor
{
    public class DeviceLayer
    {
        public string Name { get; set; }
        public List<TimelineEffect> TimelineEffects;
        public List<TriggerEffect> TriggerEffects;
        public Canvas UICanvas;
        public bool Eye { get; set; }
        private Dictionary<int, int[]> m_ZoneDictionary;
        public Dictionary<int, int[]> GetZoneDictionary()
        {
            return m_ZoneDictionary;
        }

        public DeviceLayer(string name = "")
        {
            Name = name;
            TimelineEffects = new List<TimelineEffect>();
            TriggerEffects = new List<TriggerEffect>();
            UICanvas = CreateUICanvas();
            m_ZoneDictionary = new Dictionary<int, int[]>();
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

        public void AddDeviceZones(Dictionary<int, int[]> dictionary)
        {
            if (dictionary == null)
                return;

            foreach (var item in dictionary)
            {
                if (!m_ZoneDictionary.ContainsKey(item.Key))
                {
                    m_ZoneDictionary.Add(item.Key, item.Value);
                }
            }
        }
        public void AddDeviceZones(int type, int[] indexes)
        {
            m_ZoneDictionary.Add(type, indexes);
        }
        public void AddTimelineEffect(TimelineEffect effect)
        {
            TimelineEffects.Add(effect);
            UICanvas.Children.Add(effect.UI);
            AnimationStart(effect.UI, "Opacity", 300, 0, 1);
        }

        public async void InsertEffectLine(TimelineEffect insertedEL)
        {
            TimelineEffect overlappedEL = TestAndGetFirstOverlappingEffect(insertedEL);

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
                    double source = inUI.X;
                    double target = source + overUI.Right - inUI.X;

                    await AnimationStartAsync(inUI.RenderTransform, "TranslateX", 200, source, target);
                    InsertEffectLine(insertedEL);
                }
            }
        }
        public void PushAllEffectsWhichOnTheRight(TimelineEffect effect, double move)
        {
            foreach (TimelineEffect e in TimelineEffects)
            {
                if (effect.Equals(e))
                    continue;

                if (effect.UI.X <= e.UI.X)
                {
                    double source = e.UI.X;
                    double target = source + move;

                    AnimationStart(e.UI.RenderTransform, "TranslateX", 200, source, target);
                }
            }
        }
        public TimelineEffect TestAndGetFirstOverlappingEffect(TimelineEffect testEffect)
        {
            TimelineEffect result = null;

            foreach (TimelineEffect e in TimelineEffects)
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
        private bool IsOverlapping(TimelineEffect effect1, TimelineEffect effect2)
        {
            EffectLine UI_1 = effect1.UI;
            EffectLine UI_2 = effect2.UI;

            return ControlHelper.IsOverlapping(
                UI_1.X, UI_1.Width,
                UI_2.X, UI_2.Width);
        }
        public TimelineEffect FindEffectByPosition(double x)
        {
            foreach (TimelineEffect e in TimelineEffects)
            {
                double left = e.UI.X;
                double width = e.UI.Width;

                if ((left <= x) && (x <= left + width))
                    return e;
            }
            return null;
        }
        public TimelineEffect FindFirstEffectOnTheRight(double x)
        {
            TimelineEffect result = null;

            foreach (TimelineEffect e in TimelineEffects)
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

            for (int i = 0; i < TimelineEffects.Count; i++)
            {
                TimelineEffect effect = TimelineEffects[i];
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

                TimelineEffect effect = new TimelineEffect(this, type);
                AddTimelineEffect(effect);
            }
        }

        public Table ToTable()
        {
            AuraCreatorManager manager = AuraCreatorManager.Instance;
            List<Device> globalDevices = manager.GlobalDevices;
            Table layerTable = CreateNewTable();

            foreach (var d in globalDevices)
            {
                Table deviceTable = d.ToTable();
                Table usageTable = GetUsageTable(d.Type);

                deviceTable.Set("usage", DynValue.NewTable(usageTable));
                layerTable.Set(d.Name, DynValue.NewTable(deviceTable));
            }

            return layerTable;
        }
        private Table GetUsageTable(int deviceType)
        {
            Table usageTable = CreateNewTable();
            int[] zoneIndexes = m_ZoneDictionary[deviceType];
            int count = 1;

            foreach (int index in zoneIndexes)
            {
                usageTable.Set(count, DynValue.NewNumber(index));
                count++;
            };

            return usageTable;
        }
    }
}
