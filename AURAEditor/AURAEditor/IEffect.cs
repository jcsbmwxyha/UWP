using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuraEditor
{
    interface IEffect
    {
        DeviceLayer Layer { get; set; }
        string Name { get; set; }
        string LuaName { get; set; }
        int Type { get; set; }
        EffectInfo Info { get; set; }
        double StartTime { get; set; }
        double DurationTime { get; set; }

        Table ToEventTable();
    }
}
