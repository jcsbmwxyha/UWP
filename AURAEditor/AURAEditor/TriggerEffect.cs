﻿using AuraEditor.Models;
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
        public static TriggerEffect Clone(TriggerEffect tmp)
        {
            TriggerEffect triggerEffect_clone = new TriggerEffect(tmp.Type);
            triggerEffect_clone.Info = tmp.Info.Clone() as EffectInfoModel;
            return triggerEffect_clone;
        }
    }
}
