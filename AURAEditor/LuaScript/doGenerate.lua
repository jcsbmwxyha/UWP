require("script//global")
require("script//LastScript")

doGenerateEvent = function() 
		local timer = getTime()
		math.randomseed( os.time() )
		for key,value in pairs(EventProvider["queue"]) do

			local vp = Provider:GetViewport(EventProvider["queue"][key]["Viewport"])
			local executeName = EventProvider["queue"][key]["Effect"]
			local delayMilisecond = EventProvider["queue"][key]["Delay"]
			local lifeTime = EventProvider["queue"][key]["Duration"]
			local trigger = EventProvider["queue"][key]["Trigger"]

			if (trigger == "OneTime") then

				doOneTimeEffect( vp, Event[executeName], delayMilisecond , lifeTime )

			elseif (trigger == "Period") then

				doPeriodEffect( vp, Event[executeName], delayMilisecond , lifeTime )

			elseif (trigger == "KeyboardInput" ) then

				doKeyboardOneClick(vp, Event[executeName], delayMilisecond , lifeTime )

			elseif (trigger == "OneClick" ) then

				doKeyboardOneClick(vp, Event[executeName], delayMilisecond , lifeTime )

			elseif (trigger == "DoubleClick") then

				doKeyboardDoubleClick( vp, Event[executeName], delayMilisecond , lifeTime )

			elseif (trigger == "AudioPeak") then

				if (not global.PeriodDone) then

					if ( AURA.AudioPeak > global.LastAudioPeak ) then
						global.LastAudioPeak = AURA.AudioPeak
						global.isPeakRising = true				

					elseif (AURA.AudioPeak < global.LastAudioPeak and global.isPeakRising ) then
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

		if ( timer > EventProvider["period"]) then
			global.PeriodDone = false
			Provider:clock():reset()
		else
			global.PeriodDone = true
		end

end



doPeriodEffect = function( viewprot, event, delay , duration )

	math.randomseed( os.time() )
	if (not global.PeriodDone) then
		Provider:makeEvent( viewprot, event , delay , duration)
	end
end


doOneTimeEffect = function( viewprot, event, delay , duration )

	if (not global.OneTimeDone) then
		Provider:makeEvent(viewprot, event , delay , duration)
	end
end

doKeyboardDoubleClick = function( viewprot, event, delay , duration )

	local keyPressCount = #AURA.keyStates

	local currentTime = getTime()

	for j = 1 , keyPressCount do

		if( global.keyPressX == AURA.keyStates[j].X and
				global.keyPressY == AURA.keyStates[j].Y) then

			local deltaT = currentTime - global.keyLastPressTime

			if ( deltaT < global.keyDoubleClickDuration	and deltaT > global.keyDebounceTime ) then
				Provider:makeEvent(viewprot, event , delay , duration)
			end
		end

		global.keyLastPressTime = getTime()
	end

end



doKeyboardOneClick = function( viewprot, event, delay , duration )

	local keyPressCount = #AURA.keyStates

	for j = 1 , keyPressCount do
		global.keyPressX = AURA.keyStates[j].X
		global.keyPressY = AURA.keyStates[j].Y
		global.keystrokeStrength = global.keystrokeStrength + 1
		Provider:makeEvent(viewprot, event , delay , duration)

		global.keyLastPressTime = getTime()
	end

	if (global.keystrokeStrength > 0) then
		global.keystrokeStrength = global.keystrokeStrength - 0.2
	end

end


getTime = function()
	return (Provider:clock():getClock()) * 1000
end

