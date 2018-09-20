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
                   math.randomseed(os.time() )
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
					if (global.keystrokeStrength >= 20) then
                        Provider:makeEvent(vp, Event[""Ripple""] , 1000 , 5000)
                        global.keystrokeStrength = 0
					end
					if (global.keystrokeStrength > 0) then
                        global.keystrokeStrength = global.keystrokeStrength - 0.2
                    end
                elseif (trigger == ""AudioPeakInput"") then
					if (not global.PeriodDone) then
						if (AURA.AudioPeak > global.LastAudioPeak ) then
                           global.LastAudioPeak = AURA.AudioPeak
                           global.isPeakRising = true
                       elseif (AURA.AudioPeak<global.LastAudioPeak and global.isPeakRising ) then
                           global.LastAudioPeak = 0
                           global.isPeakRising = false
							if (AURA.AudioPeak > 0.3 ) then
                                Provider:makeEvent(vp, Event[executeName] , delayMilisecond , lifeTime)
                            end
                        end
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
                    return AURA.ViewPortTransform.OrthogonaProject(viewport, 0)
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
    }
}
