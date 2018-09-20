using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AuraEditor.Common.EffectHelper;

namespace AuraEditor
{
    public class TriggerEffect : Effect
    {
        public override double StartTime { get; set; }
        public override double DurationTime
        {
            get
            {
                return 1000; // 1s
            }
            set
            {
            }
        }

        public TriggerEffect(DeviceLayer layer, int effectType) : base(layer, effectType)
        {
            DurationTime = 1000; // 1s
        }
        public TriggerEffect(DeviceLayer layer, string effectName) : base(layer, effectName)
        {
            DurationTime = 1000; // 1s
        }
    }
}
