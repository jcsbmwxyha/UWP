using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AuraEditor.Common.EffectHelper;

namespace AuraEditor.Common
{
    static class LuaHelper
    {
        static private Script m_Script;
        static private Dictionary<DynValue, string> m_LuaFunctionDictionary;

        #region Hard code
        public const string RequireLine = "require(\"script//global\")";
        public const string RandomHueString = @"function() return math.random() end";
        public const string GenerateEventFunctionString = @"
        function() 
			local timer = ( Provider:clock():getClock() )*1000
			math.randomseed( os.time() )
			for key,value in pairs(EventProvider[""queue""]) do
				local vp = Provider:GetViewport(EventProvider[""queue""][key][""Viewport""])
                local executeName = EventProvider[""queue""][key][""Effect""]
                local delayMilisecond = EventProvider[""queue""][key][""Delay""]
                local lifeTime = EventProvider[""queue""][key][""Duration""]
                local trigger = EventProvider[""queue""][key][""Trigger""]
				if (trigger == ""OneTime"") then
					if (not global.OneTimeDone) then
                        Provider:makeEvent(vp, Event[executeName] , delayMilisecond , lifeTime)
                    end
                elseif(trigger == ""Period"") then
					if (not global.PeriodDone) then
                        Provider:makeEvent(vp, Event[executeName] , delayMilisecond , lifeTime)
                    end
                elseif(trigger == ""KeyboardInput"") then
                   local keyPressCount = #AURA.keyStates
					for j = 1 , keyPressCount do
						global.keyPressX = AURA.keyStates[j].X
                        global.keyPressY = AURA.keyStates[j].Y
                        global.keystrokeStrength = global.keystrokeStrength + 1
                        Provider:makeEvent(vp, Event[executeName], delayMilisecond, lifeTime)
                    end
					if (global.keystrokeStrength > 5) then
                        Provider:makeEvent(vp, Event[""Ripple""] , 2000 , 5000)
                        global.keystrokeStrength = 0					-- Reset keystrokeStrength, if it increases to MAXIMUN
                    end
					if (global.keystrokeStrength > 0) then
                        global.keystrokeStrength = global.keystrokeStrength - 0.3
                    end
                end
            end
			if (not global.OneTimeDone) then
                global.OneTimeDone = true
			end
			if (timer > EventProvider[""period""]) then
               global.PeriodDone = false
				Provider:clock():reset()
			else
               global.PeriodDone = true
           end
    end";

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
        #endregion

        static LuaHelper()
        {
            m_Script = new Script();
            m_LuaFunctionDictionary = new Dictionary<DynValue, string>();
        }

        static public Table CreateNewTable()
        {
            string s = "table={}";
            Table table = m_Script.DoString(s + "\nreturn table").Table;

            return table;
        }
        static public DynValue RegisterAndGetDV(string functionString)
        {
            DynValue dv = m_Script.LoadFunction(functionString);
            m_LuaFunctionDictionary.Add(dv, functionString);

            return dv;
        }
        static public string GetFunctionString(DynValue dv)
        {
            return m_LuaFunctionDictionary[dv];
        }
        //static public Table GetViewportTransformTable(int effectIdx)
        //{
        //    Table viewportTransformTable = CreateNewTable();
        //    Table usageTable = CreateNewTable();
        //    Table rotateTable = CreateNewTable();
        //    string usage = "";
        //    string func = "";

        //    func = PointViewportTransformFunc;

        //    if (GetEffectName(effectIdx) == "Static") { usage = "point"; func = PointViewportTransformFunc; }
        //    else if (GetEffectName(effectIdx) == "Breath") { usage = "point"; func = PointViewportTransformFunc; }
        //    else if (GetEffectName(effectIdx) == "ColorCycle") { usage = "point"; func = PointViewportTransformFunc; }
        //    else if (GetEffectName(effectIdx) == "Rainbow") { usage = "OrthogonaProject"; func = OrthogonaProjectViewportTransformFunc; }
        //    else if (GetEffectName(effectIdx) == "Strobing") { usage = "OrthogonaProject"; func = PointViewportTransformFunc; }
        //    else if (GetEffectName(effectIdx) == "Comet") { usage = "OrthogonaProject"; func = OrthogonaProjectViewportTransformFunc; }
        //    else if (GetEffectName(effectIdx) == "Reactive") { usage = "limitRadius"; func = LimitRadiusViewportTransformFunc1; }
        //    else if (GetEffectName(effectIdx) == "Laser") { usage = "distance"; func = DistanceViewportTransformFunc; }
        //    else if (GetEffectName(effectIdx) == "Radius") { usage = "limitRadius"; func = LimitRadiusViewportTransformFunc2; }
        //    else if (GetEffectName(effectIdx) == "Ripple") { usage = "radius"; func = RadiusViewportTransformFunc; }
        //    else if (GetEffectName(effectIdx) == "Star") { usage = "limitRadius"; func = LimitRadiusViewportTransformFunc3; }

        //    usageTable.Set(1, DynValue.NewString(usage));
        //    viewportTransformTable.Set("usage", DynValue.NewTable(usageTable));

        //    rotateTable.Set("x", DynValue.NewNumber(0));
        //    rotateTable.Set("y", DynValue.NewNumber(0));
        //    rotateTable.Set("angle", DynValue.NewNumber(0));
        //    viewportTransformTable.Set("rotate", DynValue.NewTable(rotateTable));

        //    DynValue dv = m_Script.LoadFunction(func);
        //    viewportTransformTable.Set("func", dv);
        //    m_LuaFunctionDictionary.Add(dv, func);

        //    return viewportTransformTable;
        //}
        //static public Table GetBindToSlotTable(int effectIdx)
        //{
        //    Table bindToSlotTable = CreateNewTable();
        //    string bindToSlotString = "";
        //    string bindToSlotString2 = "HUE";

        //    if (GetEffectName(effectIdx) == "Static") bindToSlotString = "LIGHTNESS";
        //    else if (GetEffectName(effectIdx) == "Breath") bindToSlotString = "LIGHTNESS";
        //    else if (GetEffectName(effectIdx) == "ColorCycle") bindToSlotString = "HUE";
        //    else if (GetEffectName(effectIdx) == "Rainbow") bindToSlotString = "HUE";
        //    else if (GetEffectName(effectIdx) == "Strobing") bindToSlotString = "LIGHTNESS";
        //    else if (GetEffectName(effectIdx) == "Comet") bindToSlotString = "ALPHA";
        //    else if (GetEffectName(effectIdx) == "Reactive") bindToSlotString = "ALPHA";
        //    else if (GetEffectName(effectIdx) == "Laser") bindToSlotString = "ALPHA";
        //    else if (GetEffectName(effectIdx) == "Radius") bindToSlotString = "ALPHA";
        //    else if (GetEffectName(effectIdx) == "Ripple") bindToSlotString = "ALPHA";
        //    else if (GetEffectName(effectIdx) == "Star") bindToSlotString = "LIGHTNESS";

        //    bindToSlotTable.Set(1, DynValue.NewString(bindToSlotString));

        //    if (GetEffectName(effectIdx) == "Comet" || GetEffectName(effectIdx) == "Reactive" || GetEffectName(effectIdx) == "Ripple")
        //        bindToSlotTable.Set(2, DynValue.NewString(bindToSlotString2));

        //    return bindToSlotTable;
        //}

        //static public int GetSuggestedWaveTypeValue(int effectIdx)
        //{
        //    if (GetEffectName(effectIdx) == "Static") return 4;
        //    else if (GetEffectName(effectIdx) == "Breath") return 4;
        //    else if (GetEffectName(effectIdx) == "ColorCycle") return 2;
        //    else if (GetEffectName(effectIdx) == "Rainbow") return 2;
        //    else if (GetEffectName(effectIdx) == "Strobing") return 4;
        //    else if (GetEffectName(effectIdx) == "Comet") return 0;
        //    else if (GetEffectName(effectIdx) == "Reactive") return 0;
        //    else if (GetEffectName(effectIdx) == "Laser") return 0;
        //    else if (GetEffectName(effectIdx) == "Radius") return 0;
        //    else if (GetEffectName(effectIdx) == "Ripple") return 0;
        //    else if (GetEffectName(effectIdx) == "Star") return 0;
        //    return 0;
        //}
        //static public double GetSuggestedMinValue(int effectIdx)
        //{
        //    return 0;
        //}
        //static public double GetSuggestedMaxValue(int effectIdx)
        //{
        //    if (GetEffectName(effectIdx) == "Static") return 0.5;
        //    else if (GetEffectName(effectIdx) == "Breath") return 0.5;
        //    else if (GetEffectName(effectIdx) == "ColorCycle") return 1;
        //    else if (GetEffectName(effectIdx) == "Rainbow") return 1;
        //    else if (GetEffectName(effectIdx) == "Strobing") return 0.5;
        //    else if (GetEffectName(effectIdx) == "Comet") return 1;
        //    else if (GetEffectName(effectIdx) == "Reactive") return 1;
        //    else if (GetEffectName(effectIdx) == "Laser") return 1;
        //    else if (GetEffectName(effectIdx) == "Radius") return 1;
        //    else if (GetEffectName(effectIdx) == "Ripple") return 1;
        //    else if (GetEffectName(effectIdx) == "Star") return 0.5;
        //    return 0;
        //}
        //static public double GetSuggestedWaveLenValue(int effectIdx)
        //{
        //    if (GetEffectName(effectIdx) == "Static") return 23;
        //    else if (GetEffectName(effectIdx) == "Breath") return 23;
        //    else if (GetEffectName(effectIdx) == "ColorCycle") return 23;
        //    else if (GetEffectName(effectIdx) == "Rainbow") return 64;
        //    else if (GetEffectName(effectIdx) == "Strobing") return 23;
        //    else if (GetEffectName(effectIdx) == "Comet") return 2;
        //    else if (GetEffectName(effectIdx) == "Reactive") return 1;
        //    else if (GetEffectName(effectIdx) == "Laser") return 8;
        //    else if (GetEffectName(effectIdx) == "Radius") return 0;
        //    else if (GetEffectName(effectIdx) == "Ripple") return 40;
        //    else if (GetEffectName(effectIdx) == "Star") return 10;
        //    return 0;
        //}
        //static public double GetSuggestedFreqValue(int effectIdx)
        //{
        //    if (GetEffectName(effectIdx) == "Static") return 0;
        //    else if (GetEffectName(effectIdx) == "Breath") return 0.2;
        //    else if (GetEffectName(effectIdx) == "ColorCycle") return -0.01;
        //    else if (GetEffectName(effectIdx) == "Rainbow") return 0.04;
        //    else if (GetEffectName(effectIdx) == "Strobing") return 2;
        //    else if (GetEffectName(effectIdx) == "Comet") return 1;
        //    else if (GetEffectName(effectIdx) == "Reactive") return 1;
        //    else if (GetEffectName(effectIdx) == "Laser") return 0.1;
        //    else if (GetEffectName(effectIdx) == "Radius") return 0;
        //    else if (GetEffectName(effectIdx) == "Ripple") return 1;
        //    else if (GetEffectName(effectIdx) == "Star") return 0.5;
        //    return 0;
        //}
        //static public double GetSuggestedPhaseValue(int effectIdx)
        //{
        //    return 0;
        //}
        //static public double GetSuggestedVelocityValue(int effectIdx)
        //{
        //    if (GetEffectName(effectIdx) == "Static") return 0;
        //    else if (GetEffectName(effectIdx) == "Breath") return 0;
        //    else if (GetEffectName(effectIdx) == "ColorCycle") return 0;
        //    else if (GetEffectName(effectIdx) == "Rainbow") return 0;
        //    else if (GetEffectName(effectIdx) == "Strobing") return 0;
        //    else if (GetEffectName(effectIdx) == "Comet") return 20;
        //    else if (GetEffectName(effectIdx) == "Reactive") return 0;
        //    else if (GetEffectName(effectIdx) == "Laser") return 20;
        //    else if (GetEffectName(effectIdx) == "Radius") return 0;
        //    else if (GetEffectName(effectIdx) == "Ripple") return 5;
        //    else if (GetEffectName(effectIdx) == "Star") return 0;
        //    return 0;
        //}
    }
}
