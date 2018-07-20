using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuraEditor.Common
{
    static class Constants
    {
        public const int MAX_KEYS = 152; // km per sec.
        public const string EVENTPROVIDER_FIRST_STRING = @"require(""global"")";
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


        public const int GridLen = 35;
    }
}
