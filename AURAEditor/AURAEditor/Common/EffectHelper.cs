using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AuraEditor.Common
{
    class EffectHelper
    {
        static string[] _commonEffects =
        {
            "Static",
            "Breath",
            "ColorCycle",
            "Rainbow",
            "Strobing",
            "Comet",
            "Star",
        };
        static string[] _triggerEffects =
        {
            "Raidus",
            "Reactive",
            "Laser",
            "Ripple",
        };
        static string[] _otherTriggerEffects =
        {
            "Music",
            "Smart",
        };

        static public ObservableCollection<string> GetCommonEffectList()
        {
            ObservableCollection<string> collection = new ObservableCollection<string>();

            foreach (string name in _commonEffects)
            {
                collection.Add(name);
            }

            return collection;
        }
        static public ObservableCollection<string> GetTriggerEffectList()
        {
            ObservableCollection<string> collection = new ObservableCollection<string>();

            foreach (string name in _triggerEffects)
            {
                collection.Add(name);
            }

            return collection;
        }
        static public ObservableCollection<string> GetOtherTriggerEffectList()
        {
            ObservableCollection<string> collection = new ObservableCollection<string>();

            foreach (string name in _otherTriggerEffects)
            {
                collection.Add(name);
            }

            return collection;
        }

        static public int GetEffectIndex(string effectName)
        {
            List<string> effectList = new List<string>();
            effectList.AddRange(_commonEffects);
            effectList.AddRange(_triggerEffects);
            effectList.AddRange(_otherTriggerEffects);

            // remove index
            char[] charArray = effectName.ToCharArray();
            foreach (char c in charArray)
            {
                if (Char.IsNumber(c))
                {
                    effectName = effectName.Replace(c.ToString(), "");
                    break;
                }
            }

            for (int idx = 0; idx < effectList.Count; idx++)
            {
                if (effectName.Equals(effectList[idx]))
                    return idx;
            }

            return -1;
        }
        static public string GetEffectName(int effectIdx)
        {
            List<string> effectList = new List<string>();
            effectList.AddRange(_commonEffects);
            effectList.AddRange(_triggerEffects);
            effectList.AddRange(_otherTriggerEffects);

            if (effectIdx < effectList.Count)
                return effectList[effectIdx];
            else
                return "";
        }
        static public bool IsCommonEffect(string effectName)
        {
            foreach (string s in _commonEffects)
            {
                if (s == effectName)
                    return true;
            }

            return false;
        }

        public const string PointViewportTransformFunc = @"
	        	function(viewport)
	        		return AURA.ViewPortTransform.point(viewport)
	        	end";
        public const string OrthogonaProjectViewportTransformFunc = @"
	        	function(viewport)
	        	    return AURA.ViewPortTransform.OrthogonaProject(viewport, 3.14/4 )
	        	end";
        public const string LimitRadiusViewportTransformFunc1 = @"
	        	function(viewport)
	        		return AURA.ViewPortTransform.limitRadius(viewport, global.keyPressX , global.keyPressY, 0.5)
	        	end";
        public const string LimitRadiusViewportTransformFunc2 = @"
	        	function(viewport)
	        		return AURA.ViewPortTransform.limitRadius(viewport, global.keyPressX , global.keyPressY, global.keystrokeStrength * 2)
	        	end";
        public const string LimitRadiusViewportTransformFunc3 = @"
		        function(viewport)
		        	return AURA.ViewPortTransform.limitRadius(viewport, math.random(1,23) , math.random(1,6), 0.5)
		        end";
        public const string DistanceViewportTransformFunc = @"
		        function(viewport)
		        	return AURA.ViewPortTransform.distance(viewport, global.keyPressX , global.keyPressY)
		        end";
        public const string RadiusViewportTransformFunc = @"
        		function(viewport)
        			return AURA.ViewPortTransform.radius(viewport, 11 , 3)
        		end";
        
        static public Table GetViewportTransformTable(Dictionary<DynValue, string> dictionary, Script script, int effectIdx)
        {
            Table viewportTransformTable = CreateNewTable(script);
            Table usageTable = CreateNewTable(script);
            string usage = "";
            string func = "";

            func = PointViewportTransformFunc;

            if (GetEffectName(effectIdx) == "Static") { usage = "point"; func = PointViewportTransformFunc; }
            else if (GetEffectName(effectIdx) == "Breath") { usage = "point"; func = PointViewportTransformFunc; }
            else if (GetEffectName(effectIdx) == "ColorCycle") { usage = "point"; func = PointViewportTransformFunc; }
            else if (GetEffectName(effectIdx) == "Rainbow") { usage = "OrthogonaProject"; func = OrthogonaProjectViewportTransformFunc; }
            else if (GetEffectName(effectIdx) == "Strobing") { usage = "OrthogonaProject"; func = PointViewportTransformFunc; }
            else if (GetEffectName(effectIdx) == "Comet") { usage = "OrthogonaProject"; func = OrthogonaProjectViewportTransformFunc; }
            else if (GetEffectName(effectIdx) == "Reactive") { usage = "limitRadius"; func = LimitRadiusViewportTransformFunc1; }
            else if (GetEffectName(effectIdx) == "Laser") { usage = "distance"; func = DistanceViewportTransformFunc; }
            else if (GetEffectName(effectIdx) == "Radius") { usage = "limitRadius"; func = LimitRadiusViewportTransformFunc2; }
            else if (GetEffectName(effectIdx) == "Ripple") { usage = "radius"; func = RadiusViewportTransformFunc; }
            else if (GetEffectName(effectIdx) == "Star") { usage = "limitRadius"; func = LimitRadiusViewportTransformFunc3; }

            usageTable.Set(1, DynValue.NewString(usage));
            viewportTransformTable.Set("usage", DynValue.NewTable(usageTable));
            DynValue dv = script.LoadFunction(func);
            viewportTransformTable.Set("func", dv);
            dictionary.Add(dv, func);

            return viewportTransformTable;
        }

        static public Table GetBindToSlotTable(Script script, int effectIdx)
        {
            Table bindToSlotTable = CreateNewTable(script);
            string bindToSlotString = "";
            string bindToSlotString2 = "HUE";

            if (GetEffectName(effectIdx) == "Static") bindToSlotString = "LIGHTNESS";
            else if (GetEffectName(effectIdx) == "Breath") bindToSlotString = "LIGHTNESS";
            else if (GetEffectName(effectIdx) == "ColorCycle") bindToSlotString = "HUE";
            else if (GetEffectName(effectIdx) == "Rainbow") bindToSlotString = "HUE";
            else if (GetEffectName(effectIdx) == "Strobing") bindToSlotString = "LIGHTNESS";
            else if (GetEffectName(effectIdx) == "Comet") bindToSlotString = "ALPHA";
            else if (GetEffectName(effectIdx) == "Reactive") bindToSlotString = "ALPHA";
            else if (GetEffectName(effectIdx) == "Laser") bindToSlotString = "ALPHA";
            else if (GetEffectName(effectIdx) == "Radius") bindToSlotString = "ALPHA";
            else if (GetEffectName(effectIdx) == "Ripple") bindToSlotString = "ALPHA";
            else if (GetEffectName(effectIdx) == "Star") bindToSlotString = "LIGHTNESS";

            bindToSlotTable.Set(1, DynValue.NewString(bindToSlotString));

            if (GetEffectName(effectIdx) == "Comet" || GetEffectName(effectIdx) == "Reactive" || GetEffectName(effectIdx) == "Ripple")
                bindToSlotTable.Set(2, DynValue.NewString(bindToSlotString2));

            return bindToSlotTable;
        }

        static public int GetSuggestedWaveTypeValue(int effectIdx)
        {
            if (GetEffectName(effectIdx) == "Static") return 4;
            else if (GetEffectName(effectIdx) == "Breath") return 4;
            else if (GetEffectName(effectIdx) == "ColorCycle") return 2;
            else if (GetEffectName(effectIdx) == "Rainbow") return 2;
            else if (GetEffectName(effectIdx) == "Strobing") return 4;
            else if (GetEffectName(effectIdx) == "Comet") return 0;
            else if (GetEffectName(effectIdx) == "Reactive") return 0;
            else if (GetEffectName(effectIdx) == "Laser") return 0;
            else if (GetEffectName(effectIdx) == "Radius") return 0;
            else if (GetEffectName(effectIdx) == "Ripple") return 0;
            else if (GetEffectName(effectIdx) == "Star") return 0;
            return 0;
        }
        static public double GetSuggestedMinValue(int effectIdx)
        {
            return 0;
        }
        static public double GetSuggestedMaxValue(int effectIdx)
        {
            if (GetEffectName(effectIdx) == "Static") return 0.5;
            else if (GetEffectName(effectIdx) == "Breath") return 0.5;
            else if (GetEffectName(effectIdx) == "ColorCycle") return 1;
            else if (GetEffectName(effectIdx) == "Rainbow") return 1;
            else if (GetEffectName(effectIdx) == "Strobing") return 0.5;
            else if (GetEffectName(effectIdx) == "Comet") return 1;
            else if (GetEffectName(effectIdx) == "Reactive") return 1;
            else if (GetEffectName(effectIdx) == "Laser") return 1;
            else if (GetEffectName(effectIdx) == "Radius") return 1;
            else if (GetEffectName(effectIdx) == "Ripple") return 1;
            else if (GetEffectName(effectIdx) == "Star") return 0.5;
            return 0;
        }
        static public double GetSuggestedWaveLenValue(int effectIdx)
        {
            if (GetEffectName(effectIdx) == "Static") return 23;
            else if (GetEffectName(effectIdx) == "Breath") return 23;
            else if (GetEffectName(effectIdx) == "ColorCycle") return 23;
            else if (GetEffectName(effectIdx) == "Rainbow") return 64;
            else if (GetEffectName(effectIdx) == "Strobing") return 23;
            else if (GetEffectName(effectIdx) == "Comet") return 2;
            else if (GetEffectName(effectIdx) == "Reactive") return 1;
            else if (GetEffectName(effectIdx) == "Laser") return 8;
            else if (GetEffectName(effectIdx) == "Radius") return 0;
            else if (GetEffectName(effectIdx) == "Ripple") return 40;
            else if (GetEffectName(effectIdx) == "Star") return 10;
            return 0;
        }
        static public double GetSuggestedFreqValue(int effectIdx)
        {
            if (GetEffectName(effectIdx) == "Static") return 0;
            else if (GetEffectName(effectIdx) == "Breath") return 0.2;
            else if (GetEffectName(effectIdx) == "ColorCycle") return -0.01;
            else if (GetEffectName(effectIdx) == "Rainbow") return 0.04;
            else if (GetEffectName(effectIdx) == "Strobing") return 2;
            else if (GetEffectName(effectIdx) == "Comet") return 1;
            else if (GetEffectName(effectIdx) == "Reactive") return 1;
            else if (GetEffectName(effectIdx) == "Laser") return 0.1;
            else if (GetEffectName(effectIdx) == "Radius") return 0;
            else if (GetEffectName(effectIdx) == "Ripple") return 1;
            else if (GetEffectName(effectIdx) == "Star") return 0.5;
            return 0;
        }
        static public double GetSuggestedPhaseValue(int effectIdx)
        {
            return 0;
        }
        static public double GetSuggestedVelocityValue(int effectIdx)
        {
            if (GetEffectName(effectIdx) == "Static") return 0;
            else if (GetEffectName(effectIdx) == "Breath") return 0;
            else if (GetEffectName(effectIdx) == "ColorCycle") return 0;
            else if (GetEffectName(effectIdx) == "Rainbow") return 0;
            else if (GetEffectName(effectIdx) == "Strobing") return 0;
            else if (GetEffectName(effectIdx) == "Comet") return 20;
            else if (GetEffectName(effectIdx) == "Reactive") return 0;
            else if (GetEffectName(effectIdx) == "Laser") return 20;
            else if (GetEffectName(effectIdx) == "Radius") return 0;
            else if (GetEffectName(effectIdx) == "Ripple") return 5;
            else if (GetEffectName(effectIdx) == "Star") return 0;
            return 0;
        }

        static public double GetTriggerString(int effectIdx)
        {
            return 0;
        }

        static private Table CreateNewTable(Script script)
        {
            string s = "table={}";
            Table table = script.DoString(s + "\nreturn table").Table;

            return table;
        }
    }
}
