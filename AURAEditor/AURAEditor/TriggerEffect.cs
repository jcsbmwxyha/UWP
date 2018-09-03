using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AuraEditor.Common.EffectHelper;

namespace AuraEditor
{
    class TriggerEffect : IEffect
    {
        public DeviceLayer Layer { get; set; }
        public string Name { get; set; }
        public string LuaName { get; set; }
        public int Type { get; set; }
        public EffectInfo Info { get; set; }
        public double StartTime
        {
            get
            {
                return 0;
            }
            set
            {
            }
        }
        public double DurationTime
        {
            get
            {
                return 1000; // 1s
            }
            set
            {
            }
        }

        public TriggerEffect(DeviceLayer layer, int effectType)
        {
            Layer = layer;
            Type = effectType;
            Name = GetEffectName(effectType);
            DurationTime = 1000; // 1s
            Info = new EffectInfo(effectType);
        }
        public virtual Table ToEventTable() { return null; }
    }
}
