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
    public class TimelineEffect : Effect
    {
        public EffectLine UI { get; }
        public override double StartTime
        {
            get
            {
                double timeUnits = UI.X / AuraLayerManager.PixelsPerTimeUnit;
                double seconds = timeUnits * AuraLayerManager.SecondsPerTimeUnit;

                return seconds * 1000;
            }
            set
            {
                double seconds = value / 1000;
                double timeUnits = seconds / AuraLayerManager.SecondsPerTimeUnit;

                UI.X = timeUnits * AuraLayerManager.PixelsPerTimeUnit;
            }
        }
        public override double DurationTime
        {
            get
            {
                double timeUnits = UI.Width / AuraLayerManager.PixelsPerTimeUnit;
                double seconds = timeUnits * AuraLayerManager.SecondsPerTimeUnit;

                return seconds * 1000;
            }
            set
            {
                double seconds = value / 1000;
                double timeUnits = seconds / AuraLayerManager.SecondsPerTimeUnit;

                UI.Width = timeUnits * AuraLayerManager.PixelsPerTimeUnit;
            }
        }

        public TimelineEffect(int effectType) : base(effectType)
        {
            UI = CreateEffectUI(effectType);
            UI.DataContext = this;
            DurationTime = 1000; // 1s
        }
        private EffectLine CreateEffectUI(int effectType)
        {
            EffectLine el = new EffectLine
            {
                Height = 34,
                Width = AuraLayerManager.GetPixelsPerSecond(),
                ManipulationMode = ManipulationModes.TranslateX
            };

            CompositeTransform ct = el.RenderTransform as CompositeTransform;
            ct.TranslateY = 8;

            return el;
        }

        static public TimelineEffect CloneEffect(TimelineEffect copy)
        {
            TimelineEffect target = new TimelineEffect(copy.Type);

            target.Info = copy.Info.Clone() as EffectInfo;
            target.DurationTime = copy.DurationTime;

            return target;
        }
    }
}
