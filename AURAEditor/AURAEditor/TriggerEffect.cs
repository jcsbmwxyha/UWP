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
        public TriggerEffect(Layer layer, int effectType) : base(layer, effectType) {}
        public TriggerEffect(Layer layer, string effectName) : base(layer, effectName) {}
    }
}
