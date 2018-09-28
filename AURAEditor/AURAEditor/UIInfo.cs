using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using static AuraEditor.Common.LuaHelper;
using static AuraEditor.Common.EffectHelper;

namespace AuraEditor
{
    public class UIInfo
    {
        public int Type { get; set; }
        public Color InitColor { get; set; }
        public bool ColorRandom { get; set; }
        public List<WaveInfo> Waves { get; set; }
        public UIInfo(int type)
        {
            Type = type;
            InitColor = Colors.Red;
            ColorRandom = false;
            Waves = new List<WaveInfo>
            {
                new WaveInfo(type)
            };
        }

        public class WaveInfo
        {
            public int Type { get; set; }
            public double Brightness { get; set; }
            public double Speed { get; set; }
            public double Direction { get; set; }
            public double Angle { get; set; }


            public WaveInfo(int type)
            {
                Type = type;
                Brightness = GetDefaultBrightnessType(type);
                Speed = GetDefaultSpeed(type);
                Direction = GetDefaultDirection(type);
                Angle = GetDefaultAngle(type);
            }

            public double GetDefaultBrightnessType(int effectType)
            {
                if (GetEffectName(effectType) == "Static") return 3;
                else if (GetEffectName(effectType) == "Breath") return 3;
                else if (GetEffectName(effectType) == "ColorCycle") return 3;
                else if (GetEffectName(effectType) == "Rainbow") return 3;
                else if (GetEffectName(effectType) == "Strobing") return 3;
                else if (GetEffectName(effectType) == "Comet") return 3;
                else if (GetEffectName(effectType) == "Reactive") return 3;
                else if (GetEffectName(effectType) == "Laser") return 3;
                else if (GetEffectName(effectType) == "Radius") return 3;
                else if (GetEffectName(effectType) == "Ripple") return 3;
                else if (GetEffectName(effectType) == "Star") return 3;
                return 3;
            }

            static private double GetDefaultSpeed(int effectType)
            {
                if (GetEffectName(effectType) == "Static") return 1;
                else if (GetEffectName(effectType) == "Breath") return 1;
                else if (GetEffectName(effectType) == "ColorCycle") return 1;
                else if (GetEffectName(effectType) == "Rainbow") return 1;
                else if (GetEffectName(effectType) == "Strobing") return 1;
                else if (GetEffectName(effectType) == "Comet") return 1;
                else if (GetEffectName(effectType) == "Reactive") return 1;
                else if (GetEffectName(effectType) == "Laser") return 1;
                else if (GetEffectName(effectType) == "Radius") return 1;
                else if (GetEffectName(effectType) == "Ripple") return 1;
                else if (GetEffectName(effectType) == "Star") return 1;
                return 1;
            }

            static private double GetDefaultDirection(int effectType)
            {
                if (GetEffectName(effectType) == "Static") return 2;
                else if (GetEffectName(effectType) == "Breath") return 2;
                else if (GetEffectName(effectType) == "ColorCycle") return 2;
                else if (GetEffectName(effectType) == "Rainbow") return 2;
                else if (GetEffectName(effectType) == "Strobing") return 2;
                else if (GetEffectName(effectType) == "Comet") return 2;
                else if (GetEffectName(effectType) == "Reactive") return 2;
                else if (GetEffectName(effectType) == "Laser") return 2;
                else if (GetEffectName(effectType) == "Radius") return 2;
                else if (GetEffectName(effectType) == "Ripple") return 2;
                else if (GetEffectName(effectType) == "Star") return 2;
                return 2;
            }

            static private double GetDefaultAngle(int effectType)
            {
                if (GetEffectName(effectType) == "Static") return 90;
                else if (GetEffectName(effectType) == "Breath") return 90;
                else if (GetEffectName(effectType) == "ColorCycle") return 90;
                else if (GetEffectName(effectType) == "Rainbow") return 90;
                else if (GetEffectName(effectType) == "Strobing") return 90;
                else if (GetEffectName(effectType) == "Comet") return 90;
                else if (GetEffectName(effectType) == "Reactive") return 90;
                else if (GetEffectName(effectType) == "Laser") return 90;
                else if (GetEffectName(effectType) == "Radius") return 90;
                else if (GetEffectName(effectType) == "Ripple") return 90;
                else if (GetEffectName(effectType) == "Star") return 90;
                return 90;
            }
        }
    }
}
