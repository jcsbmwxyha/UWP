using AuraEditor.Common;
using AuraEditor.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using static AuraEditor.MainPage;
using static AuraEditor.Common.EffectHelper;
using MoonSharp.Interpreter;

namespace AuraEditor
{
    public class Effect : IEffect
    {
        public DeviceLayer Layer { get; set; }
        public string Name { get; set; }
        public string LuaName { get; set; }
        public int Type { get; set; }
        public EffectLine UI { get; }
        public EffectInfo Info { get; set; }
        public double StartTime
        {
            get
            {
                double timeUnits = UI.X / AuraCreatorManager.pixelsPerTimeUnit;
                double seconds = timeUnits * AuraCreatorManager.secondsPerTimeUnit;

                return seconds * 1000;
            }
            set
            {
                double seconds = value / 1000;
                double timeUnits = seconds / AuraCreatorManager.secondsPerTimeUnit;

                UI.X = timeUnits * AuraCreatorManager.pixelsPerTimeUnit;
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

        public Effect(DeviceLayer layer, int effectType)
        {
            Layer = layer;
            Type = effectType;
            Name = GetEffectName(effectType);
            UI = CreateEffectUI(effectType);
            UI.DataContext = this;
            UI.X = (int)Layer.GetFirstFreeRoomPosition();
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

        public virtual Table ToEventTable() { return null; }
    }

    public class RainbowEffect : Effect
    {
        public RainbowEffect(DeviceLayer layer, int effectType) : base(layer, effectType)
        {
        }
        public override Table ToEventTable() { return null; }
    }
}
