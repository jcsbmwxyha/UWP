require("script//global")

doGenerateEvent = function() 
		local timer = getTime()
		math.randomseed( os.time() )

		if (not isPeriodic(timer)) then
			return
		end

		for key,value in pairs(EventProvider["queue"]) do

			local vp = Provider:GetViewport(EventProvider["queue"][key]["Viewport"])
			local executeName = EventProvider["queue"][key]["Effect"]
			local delayMilisecond = EventProvider["queue"][key]["Delay"]
			local lifeTime = EventProvider["queue"][key]["Duration"]
			local trigger = EventProvider["queue"][key]["Trigger"]
			local layer = EventProvider["queue"][key]["Layer"]

			if (trigger == "OneTime") then

				doOneTimeEffect( vp, executeName, delayMilisecond , lifeTime, layer )

			elseif (trigger == "Period") then

				doPeriodEffect2( vp, executeName, delayMilisecond , lifeTime, layer )

			elseif (trigger == "KeyboardInput" ) then

				doKeyboardOneClick(vp, executeName, delayMilisecond , lifeTime, layer)

			elseif (trigger == "OneClick" ) then

				doKeyboardOneClick(vp, executeName, delayMilisecond , lifeTime, layer )

			elseif (trigger == "DoubleClick") then

				doKeyboardDoubleClick( vp, executeName, delayMilisecond , lifeTime, layer )

			elseif (trigger == "AudioPeak") then

				if (not global.PeriodDone) then

					if ( AURA.AudioPeak > global.LastAudioPeak ) then
						global.LastAudioPeak = AURA.AudioPeak
						global.isPeakRising = true				

					elseif (AURA.AudioPeak < global.LastAudioPeak and global.isPeakRising ) then
						global.LastAudioPeak = 0
						global.isPeakRising = false

						if (AURA.AudioPeak > 0.35 ) then 
							Provider:makeEvent2(vp, executeName , delayMilisecond , lifeTime, layer)
						end
					end
				end

			end
		end

		doOneTimeEffectStuff()

		doPeriodEffectStuff(timer)
end



doPeriodEffect = function( viewprot, name, delay , duration )

	math.randomseed( os.time() )
	if (not global.PeriodDone) then
		Provider:makeEvent( viewprot, Event[name] , delay , duration)
	end
end

doPeriodEffect2 = function( viewprot, name, delay , duration, layer)

	math.randomseed( os.time() )
	if (not global.PeriodDone) then
		Provider:makeEvent2( viewprot, name , delay , duration, layer)
	end
end

doOneTimeEffect = function( viewprot, event, delay , duration, layer)

	if (not global.OneTimeDone) then
		Provider:makeEvent2(viewprot, event , delay , duration, layer)
	end
end

doKeyboardDoubleClick = function( viewprot, name, delay , duration, layer)

	local keyPressCount = #AURA.keyStates

	local currentTime = getTime()

	for j = 1 , keyPressCount do

		if( global.keyPressX == AURA.keyStates[j].X and
				global.keyPressY == AURA.keyStates[j].Y) then

			local deltaT = currentTime - global.keyLastPressTime

			if ( deltaT < global.keyDoubleClickDuration	and deltaT > global.keyDebounceTime ) then
				Provider:makeEvent2( viewprot, name , delay , duration, layer)
			end
		end

		global.keyPressX = AURA.keyStates[j].X
		global.keyPressY = AURA.keyStates[j].Y
		global.keyLastPressTime = getTime()
	end

end



doKeyboardOneClick = function( viewprot, name, delay , duration, layer)

	local keyPressCount = #AURA.keyStates

	for j = 1 , keyPressCount do
		global.keyPressX = AURA.keyStates[j].X
		global.keyPressY = AURA.keyStates[j].Y
		global.keystrokeStrength = global.keystrokeStrength + 1
		Provider:makeEvent2( viewprot, name, delay , duration, layer)

		global.keyLastPressTime = getTime()
	end

	if (global.keystrokeStrength > 0) then
		global.keystrokeStrength = global.keystrokeStrength - 0.2
	end

end


getTime = function()
	return (Provider:clock():getClock()) * 1000
end

isPeriodic = function(timer)

	local isPeriod = true

	if (EventProvider["isPeriodic"] == "false") then
		isPeriod = false
	end

	if ( (not isPeriod) and (timer > EventProvider["period"]) ) then
		-- Just execute one time
		return false
	else
		-- Repeat effect in queue periodicly
		return true
	end

end

doOneTimeEffectStuff = function()

	if (not global.OneTimeDone) then
		global.OneTimeDone = true
	end

end


doPeriodEffectStuff = function(timer)

	if ( timer > EventProvider["period"]) then
		global.PeriodDone = false
		Provider:clock():reset()
	else
		global.PeriodDone = true
	end

end
