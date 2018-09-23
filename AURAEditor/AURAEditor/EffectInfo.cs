using AuraEditor.Common;
using MoonSharp.Interpreter;
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
    public class EffectInfo
    {
        public int Type { get; }
        public Color InitColor { get; set; }
        public List<WaveInfo> Waves { get; set; }
        
        public EffectInfo(int type)
        {
            Type = type;
            InitColor = Colors.Red;
            Waves = new List<WaveInfo>
            {
                new WaveInfo(type)
            };
        }
        public Table ToTable()
        {
            Table effect_Table = CreateNewTable();

            Table viewportTransformTable = GetViewportTransformTable(Type);
            Table wave_Table = CreateNewTable();
            Table initColor_Table = GetInitColorTable();
            
            for(int i = 0; i < Waves.Count; i++)
            {
                wave_Table.Set((i + 1), DynValue.NewTable(Waves[i].ToTable()));
            }

            effect_Table.Set("initColor", DynValue.NewTable(initColor_Table));
            effect_Table.Set("viewportTransform", DynValue.NewTable(viewportTransformTable));
            effect_Table.Set("wave", DynValue.NewTable(wave_Table));
            
            return effect_Table;
        }
        private Table GetInitColorTable()
        {
            Table initColor_Table = CreateNewTable();

            Color c = InitColor;
            double[] hsl = AuraEditorColorHelper.RgbTOHsl(c);
            //DynValue randomHue_dv;
            //
            //if (GetEffectName(Type) == "Star")
            //{
            //    randomHue_dv = RegisterAndGetDV(RandomHueString);
            //    initColor_Table.Set("hue", randomHue_dv);
            //}
            //else
            //    initColor_Table.Set("hue", DynValue.NewNumber(hsl[0]));

            initColor_Table.Set("hue", DynValue.NewNumber(hsl[0]));
            initColor_Table.Set("saturation", DynValue.NewNumber(hsl[1]));
            initColor_Table.Set("lightness", DynValue.NewNumber(hsl[2]));
            initColor_Table.Set("alpha", DynValue.NewNumber(c.A / 255));

            return initColor_Table;
        }
        static public Table GetViewportTransformTable(int effectType)
        {
            Table viewportTransformTable = CreateNewTable();
            Table usageTable = CreateNewTable();
            Table rotateTable = CreateNewTable();
            string usage = "";
            string func = "";

            if (GetEffectName(effectType) == "Static") { usage = "point"; func = PointViewportTransformFunc; }
            else if (GetEffectName(effectType) == "Breath") { usage = "point"; func = PointViewportTransformFunc; }
            else if (GetEffectName(effectType) == "ColorCycle") { usage = "point"; func = PointViewportTransformFunc; }
            else if (GetEffectName(effectType) == "Rainbow") { usage = "OrthogonaProject"; func = OrthogonaProjectViewportTransformFunc; }
            else if (GetEffectName(effectType) == "Strobing") { usage = "point"; func = PointViewportTransformFunc; }
            else if (GetEffectName(effectType) == "Comet") { usage = "OrthogonaProject"; func = OrthogonaProjectViewportTransformFunc; }
            else if (GetEffectName(effectType) == "Reactive") { usage = "limitRadius"; func = LimitRadiusViewportTransformFunc1; }
            else if (GetEffectName(effectType) == "Laser") { usage = "distance"; func = DistanceViewportTransformFunc; }
            else if (GetEffectName(effectType) == "Radius") { usage = "limitRadius"; func = LimitRadiusViewportTransformFunc2; }
            else if (GetEffectName(effectType) == "Ripple") { usage = "radius"; func = RadiusViewportTransformFunc; }
            else if (GetEffectName(effectType) == "Star") { usage = "limitRadius"; func = LimitRadiusViewportTransformFunc3; }
            else { usage = "point"; func = PointViewportTransformFunc; }

            usageTable.Set(1, DynValue.NewString(usage));
            rotateTable.Set("x", DynValue.NewNumber(0));
            rotateTable.Set("y", DynValue.NewNumber(0));
            rotateTable.Set("angle", DynValue.NewNumber(0));

            DynValue dv = RegisterAndGetDV(func);

            viewportTransformTable.Set("usage", DynValue.NewTable(usageTable));
            viewportTransformTable.Set("rotate", DynValue.NewTable(rotateTable));
            viewportTransformTable.Set("func", dv);

            return viewportTransformTable;
        }
    }

    public class WaveInfo
    {
        public int Type { get; set; }
        public int WaveType { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
        public double WaveLength { get; set; }
        public double Freq { get; set; }
        public double Phase { get; set; }
        public double Start { get; set; }
        public double Velocity { get; set; }
        public BindToSignalInfo BindToSignal { get; set; }
        public CustomizedInfo Customized { get; set; }
        
        public WaveInfo(int type)
        {
            Type = type;
            WaveType = GetDefaultWaveType(type);
            Min = GetDefaultMin(type);
            Max = GetDefaultMax(type);
            WaveLength = GetDefaultWaveLen(type);
            Freq = GetDefaultFreq(type);
            Phase = GetDefaultPhase(type);
            Start = 0;
            Velocity = GetDefaultVelocity(type);
        }
        public Table ToTable()
        {
            Table waveTable = CreateNewTable();
            Table bindToSlotTable = GetBindToSlotTable(Type);
            
            waveTable.Set("waveType", DynValue.NewString(WaveTypeToString(WaveType)));
            waveTable.Set("min", DynValue.NewNumber(Min));
            waveTable.Set("max", DynValue.NewNumber(Max));
            waveTable.Set("waveLength", DynValue.NewNumber(WaveLength));
            waveTable.Set("freq", DynValue.NewNumber(Freq));
            waveTable.Set("phase", DynValue.NewNumber(Phase));
            waveTable.Set("start", DynValue.NewNumber(Start));
            waveTable.Set("velocity", DynValue.NewNumber(Velocity));

            waveTable.Set("bindToSlot", DynValue.NewTable(bindToSlotTable));

            return waveTable;
        }
        static private Table GetBindToSlotTable(int effectType)
        {
            Table bindToSlotTable = CreateNewTable();
            string bindToSlotString = "";
            string bindToSlotString2 = "HUE";

            if (GetEffectName(effectType) == "Static") bindToSlotString = "ALPHA";
            else if (GetEffectName(effectType) == "Breath") bindToSlotString = "LIGHTNESS";
            else if (GetEffectName(effectType) == "ColorCycle") bindToSlotString = "HUE";
            else if (GetEffectName(effectType) == "Rainbow") bindToSlotString = "HUE";
            else if (GetEffectName(effectType) == "Strobing") bindToSlotString = "LIGHTNESS";
            else if (GetEffectName(effectType) == "Comet") bindToSlotString = "ALPHA";
            else if (GetEffectName(effectType) == "Reactive") bindToSlotString = "ALPHA";
            else if (GetEffectName(effectType) == "Laser") bindToSlotString = "ALPHA";
            else if (GetEffectName(effectType) == "Radius") bindToSlotString = "ALPHA";
            else if (GetEffectName(effectType) == "Ripple") bindToSlotString = "ALPHA";
            else if (GetEffectName(effectType) == "Star") bindToSlotString = "LIGHTNESS";

            bindToSlotTable.Set(1, DynValue.NewString(bindToSlotString));

            if (GetEffectName(effectType) == "Comet" || GetEffectName(effectType) == "Reactive" || GetEffectName(effectType) == "Ripple")
                bindToSlotTable.Set(2, DynValue.NewString(bindToSlotString2));

            return bindToSlotTable;
        }
        static public int StringToWaveType(string waveString)
        {
            switch (waveString)
            {
                case "SineWave": return 0;
                case "HalfSineWave": return 1;
                case "QuarterSineWave": return 2;
                case "SquareWave": return 3;
                case "TriangleWave": return 4;
                case "SawToothleWave": return 5;
                case "ConstantWave": return 6;
            }

            return 0;
        }
        static private string WaveTypeToString(int waveType)
        {
            switch (waveType)
            {
                case 0: return "SineWave";
                case 1: return "HalfSineWave";
                case 2: return "QuarterSineWave";
                case 3: return "SquareWave";
                case 4: return "TriangleWave";
                case 5: return "SawToothleWave";
                case 6: return "ConstantWave";
            }

            return "SineWave";
        }
        static private int GetDefaultWaveType(int effectType)
        {
            if (GetEffectName(effectType) == "Static") return 6;
            else if (GetEffectName(effectType) == "Breath") return 0;
            else if (GetEffectName(effectType) == "ColorCycle") return 2;
            else if (GetEffectName(effectType) == "Rainbow") return 2;
            else if (GetEffectName(effectType) == "Strobing") return 0;
            else if (GetEffectName(effectType) == "Comet") return 4;
            else if (GetEffectName(effectType) == "Reactive") return 0;
            else if (GetEffectName(effectType) == "Laser") return 0;
            else if (GetEffectName(effectType) == "Radius") return 0;
            else if (GetEffectName(effectType) == "Ripple") return 0;
            else if (GetEffectName(effectType) == "Star") return 0;
            return 0;
        }
        static private double GetDefaultMin(int effectType)
        {
            return 0;
        }
        static private double GetDefaultMax(int effectType)
        {
            if (GetEffectName(effectType) == "Static") return 1;
            else if (GetEffectName(effectType) == "Breath") return 0.5;
            else if (GetEffectName(effectType) == "ColorCycle") return 1;
            else if (GetEffectName(effectType) == "Rainbow") return 1;
            else if (GetEffectName(effectType) == "Strobing") return 0.5;
            else if (GetEffectName(effectType) == "Comet") return 1;
            else if (GetEffectName(effectType) == "Reactive") return 1;
            else if (GetEffectName(effectType) == "Laser") return 1;
            else if (GetEffectName(effectType) == "Radius") return 1;
            else if (GetEffectName(effectType) == "Ripple") return 1;
            else if (GetEffectName(effectType) == "Star") return 0.5;
            return 0;
        }
        static private double GetDefaultWaveLen(int effectType)
        {
            if (GetEffectName(effectType) == "Static") return 23;
            else if (GetEffectName(effectType) == "Breath") return 23;
            else if (GetEffectName(effectType) == "ColorCycle") return 23;
            else if (GetEffectName(effectType) == "Rainbow") return 64;
            else if (GetEffectName(effectType) == "Strobing") return 23;
            else if (GetEffectName(effectType) == "Comet") return 2;
            else if (GetEffectName(effectType) == "Reactive") return 1;
            else if (GetEffectName(effectType) == "Laser") return 8;
            else if (GetEffectName(effectType) == "Radius") return 0;
            else if (GetEffectName(effectType) == "Ripple") return 40;
            else if (GetEffectName(effectType) == "Star") return 10;
            return 0;
        }
        static private double GetDefaultFreq(int effectType)
        {
            if (GetEffectName(effectType) == "Static") return 0;
            else if (GetEffectName(effectType) == "Breath") return 0.2;
            else if (GetEffectName(effectType) == "ColorCycle") return -0.01;
            else if (GetEffectName(effectType) == "Rainbow") return 0.04;
            else if (GetEffectName(effectType) == "Strobing") return 2;
            else if (GetEffectName(effectType) == "Comet") return 1;
            else if (GetEffectName(effectType) == "Reactive") return 1;
            else if (GetEffectName(effectType) == "Laser") return 0.1;
            else if (GetEffectName(effectType) == "Radius") return 0;
            else if (GetEffectName(effectType) == "Ripple") return 1;
            else if (GetEffectName(effectType) == "Star") return 0.5;
            return 0;
        }
        static private double GetDefaultPhase(int effectType)
        {
            return 0;
        }
        static private double GetDefaultVelocity(int effectType)
        {
            if (GetEffectName(effectType) == "Static") return 0;
            else if (GetEffectName(effectType) == "Breath") return 0;
            else if (GetEffectName(effectType) == "ColorCycle") return 0;
            else if (GetEffectName(effectType) == "Rainbow") return 0;
            else if (GetEffectName(effectType) == "Strobing") return 0;
            else if (GetEffectName(effectType) == "Comet") return 20;
            else if (GetEffectName(effectType) == "Reactive") return 0;
            else if (GetEffectName(effectType) == "Laser") return 20;
            else if (GetEffectName(effectType) == "Radius") return 0;
            else if (GetEffectName(effectType) == "Ripple") return 5;
            else if (GetEffectName(effectType) == "Star") return 0;
            return 0;
        }
    }   

    public class BindToSignalInfo
    {
        public string Source;
        public string Target;
        public double Amplify;
        public double Offset;

        public Table ToTable()
        {
            Table bindToSignal_Table = CreateNewTable();

            return bindToSignal_Table;
        }
    }

    public class CustomizedInfo
    {
        public Table ToTable()
        {
            Table customized_Table = CreateNewTable();

            return customized_Table;
        }
    }
}
