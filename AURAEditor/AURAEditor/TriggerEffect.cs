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
        public TriggerEffect(int effectType) : base(effectType) {}
        public TriggerEffect(string effectName) : base(effectName) {}
    }
}
