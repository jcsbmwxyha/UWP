OneTimeDone = false
PeriodDone = false
keystrokeStrength = 0
keyPressX = 0
keyPressY = 0


RandomHue = function()
	return math.random()
end

EventProvider = {

	name = "Star",

	period = 10000,

	queue = {

		["1"] = { Effect = "Star" , Viewport = "Group1" , Trigger = "Period" , Delay = 0 , Duration = 10000 },
		["2"] = { Effect = "Star" , Viewport = "Group1" , Trigger = "Period" , Delay = 200 , Duration = 10000 },
		["3"] = { Effect = "Star" , Viewport = "Group1" , Trigger = "Period" , Delay = 400 , Duration = 10000 },
		["4"] = { Effect = "Star" , Viewport = "Group1" , Trigger = "Period" , Delay = 600 , Duration = 10000 },
		["5"] = { Effect = "Star" , Viewport = "Group1" , Trigger = "Period" , Delay = 800 , Duration = 10000 },
		["6"] = { Effect = "Star" , Viewport = "Group1" , Trigger = "Period" , Delay = 1200 , Duration = 10000 },
		["7"] = { Effect = "Star" , Viewport = "Group1" , Trigger = "Period" , Delay = 1400 , Duration = 10000 },
		["8"] = { Effect = "Star" , Viewport = "Group1" , Trigger = "Period" , Delay = 1600 , Duration = 10000 },
		["9"] = { Effect = "Star" , Viewport = "Group1" , Trigger = "Period" , Delay = 1800 , Duration = 10000 },
		["10"] = { Effect = "Star" , Viewport = "Group1" , Trigger = "Period" , Delay = 2000 , Duration = 10000 },
		["11"] = { Effect = "Star" , Viewport = "Group1" , Trigger = "Period" , Delay = 0 , Duration = 10000 },
		["12"] = { Effect = "Star" , Viewport = "Group1" , Trigger = "Period" , Delay = 200 , Duration = 10000 },
		["13"] = { Effect = "Star" , Viewport = "Group1" , Trigger = "Period" , Delay = 400 , Duration = 10000 },
		["14"] = { Effect = "Star" , Viewport = "Group1" , Trigger = "Period" , Delay = 600 , Duration = 10000 },
		["15"] = { Effect = "Star" , Viewport = "Group1" , Trigger = "Period" , Delay = 800 , Duration = 10000 },
		["16"] = { Effect = "Star" , Viewport = "Group1" , Trigger = "Period" , Delay = 1200 , Duration = 10000 },
		["17"] = { Effect = "Star" , Viewport = "Group1" , Trigger = "Period" , Delay = 1600 , Duration = 10000 },
		["18"] = { Effect = "Star" , Viewport = "Group1" , Trigger = "Period" , Delay = 1800 , Duration = 10000 },
		["19"] = { Effect = "Star" , Viewport = "Group1" , Trigger = "Period" , Delay = 2000 , Duration = 10000 },
		["20"] = { Effect = "Star" , Viewport = "Group1" , Trigger = "Period" , Delay = 0 , Duration = 10000 },
		["21"] = { Effect = "Star" , Viewport = "Group1" , Trigger = "Period" , Delay = 200 , Duration = 10000 },
		["22"] = { Effect = "Star" , Viewport = "Group1" , Trigger = "Period" , Delay = 400 , Duration = 10000 },
		["23"] = { Effect = "Star" , Viewport = "Group1" , Trigger = "Period" , Delay = 600 , Duration = 10000 },
		["24"] = { Effect = "Star" , Viewport = "Group1" , Trigger = "Period" , Delay = 800 , Duration = 10000 },
		["25"] = { Effect = "Star" , Viewport = "Group1" , Trigger = "Period" , Delay = 1200 , Duration = 10000 },
		["26"] = { Effect = "Star" , Viewport = "Group1" , Trigger = "Period" , Delay = 1400 , Duration = 10000 },
		["27"] = { Effect = "Star" , Viewport = "Group1" , Trigger = "Period" , Delay = 1600 , Duration = 10000 },
		["28"] = { Effect = "Star" , Viewport = "Group1" , Trigger = "Period" , Delay = 1800 , Duration = 10000 },
		["29"] = { Effect = "Star" , Viewport = "Group1" , Trigger = "Period" , Delay = 2000 , Duration = 10000 },
		["30"] = { Effect = "Star" , Viewport = "Group1" , Trigger = "Period" , Delay = 0 , Duration = 10000 },

	},

	
	generateEvent = function() 
			local timer = ( Provider:clock():getClock() )*1000

			for key,value in pairs(EventProvider["queue"]) do

				local vp = Provider:GetViewport(EventProvider["queue"][key]["Viewport"])
				local executeName = EventProvider["queue"][key]["Effect"]
				local delayMilisecond = EventProvider["queue"][key]["Delay"]
				local lifeTime = EventProvider["queue"][key]["Duration"]
				local trigger = EventProvider["queue"][key]["Trigger"]

				if (trigger == "OneTime") then

					if (not OneTimeDone) then
						Provider:makeEvent(vp, Event[executeName] , delayMilisecond , lifeTime)
						OneTimeDone = true
					end
			
				elseif (trigger == "Period") then

					if (not PeriodDone) then
						Provider:makeEvent(vp, Event[executeName] , delayMilisecond , lifeTime)
					end

				elseif (trigger == "KeyboardInput") then
					local keyPressCount = #AURA.keyStates
				
					for j = 1 , keyPressCount do
						keyPressX = AURA.keyStates[j].X
						keyPressY = AURA.keyStates[j].Y
						keystrokeStrength = keystrokeStrength + 1

						Provider:makeEvent(vp, Event[executeName] , delayMilisecond , lifeTime)
					end

					if (keystrokeStrength > 0) then
						keystrokeStrength = keystrokeStrength - 0.2
					end
				end
			end	

			if ( timer > EventProvider["period"]) then
				PeriodDone = false
				Provider:clock():reset()
			else
				PeriodDone = true
			end

    end
}

Viewport = {
	Group1 = {
		Flaire = {
			name = "Flaire",
			DeviceType = "Keyboard",
			layout = { weight = 23 , height = 6 },
			location = { x = 0, y = 0 },
			usageRange = {
						["1"] = { from =  22*0 , to = 23*0 + 22 },
						["2"] = { from =  22*1 , to = 23*1 + 22 },
						["3"] = { from =  22*2 , to = 23*2 + 22 },
						["4"] = { from =  22*3 , to = 23*3 + 22 },
						["5"] = { from =  22*4 , to = 23*4 + 22 },
						["6"] = { from =  22*5 , to = 23*5 + 22 }
			}
		},
		G703VI = {
			name = "G703VI",
			DeviceType = "Notebook",
			layout = { weight = 22 , height = 8 },
			location = { x = 10, y = 1 },
			usageRange = {
						["1"] = { from =  22*0 , to = 22*0 + 7 },
						["2"] = { from =  22*1 , to = 22*1 + 7 },
						["3"] = { from =  22*2 , to = 22*2 + 7 },
						["4"] = { from =  22*3 , to = 22*3 + 7 },
						["5"] = { from =  22*4 , to = 22*4 + 7 },
						["6"] = { from =  22*5 , to = 22*5 + 7 }
			}
		}
	}
}

Event = {
	Breath = {

		viewportTransform = {
				usage = { ["0"] = "rows" },
				func = function(viewport)
					return AURA.ViewPortTransform.point(viewport)
				end
		},		

		wave = { waveType = "TriangleWave" , min = 0 , max = 0.5 , waveLength = 1, 
				freq = 0.2 , phase = 0, start = 0 , velocity = 0 },
				
		initColor = { hue =  0.5 , saturation = 1.0, lightness = 0.5 , alpha = 1.0 },	
		bindToSlot = { ["1"] = "LIGHTNESS" }
	},
	Rainbow = {
	
		viewportTransform = {
				usage = { ["1"] = "OrthogonaProject" },
				func = function(viewport)
					return AURA.ViewPortTransform.OrthogonaProject(viewport, 3.14/4 )
				end	
		},
		
		wave = { waveType = "QuarterSineWave" , min = 0 , max = 1 , waveLength = 84, 
				freq = 0.04, phase = 0.0, start = 0 , velocity = 0 },
				
		initColor = { hue = 0.5 , saturation = 1.0, lightness = 0.5 , alpha = 1.0 },
		bindToSlot = { ["1"] = "HUE" }
	},
	ColorCycle = {
		viewportTransform = {
				usage = { ["1"] = "OrthogonaProject" },
				func = function(viewport)
					return AURA.ViewPortTransform.OrthogonaProject(viewport, 3.14/4 )
				end	
		},
		
		wave = { waveType = "QuarterSineWave" , min = 0 , max = 1 , waveLength = 99999, 
				freq = 0.04, phase = 0.0, start = 0 , velocity = 0 },
				
		initColor = { hue = 0.5 , saturation = 1.0, lightness = 0.5 , alpha = 1.0 },
		bindToSlot = { ["1"] = "HUE" }
	},
	Comet = {

		viewportTransform = {
				usage = { ["1"] = "OrthogonaProject" },
				func = function(viewport)
					return AURA.ViewPortTransform.OrthogonaProject(viewport, 3.14/4 )
				end
		},
		
		wave = { waveType = "SineWave" , min = 0 , max = 1 , waveLength = 2, 
				freq = 1, phase = 0.0, start = 0 , velocity = 20 },
				
		initColor = { hue = 0.5 , saturation = 1.0, lightness = 0.5 , alpha = 1.0 },
		bindToSlot = { ["1"] = "ALPHA" , ["2"] = "HUE" }
	},
	Reactive = {

		viewportTransform = {
				usage = { ["0"] = "limitRadius" },
				func = function(viewport)
					return AURA.ViewPortTransform.limitRadius(viewport, keyPressX , keyPressY, 0.5)
				end
		},		

		wave = { waveType = "SineWave" , min = 0 , max = 1 , waveLength = 1, 
				freq = 1 , phase = 0, start = 0 , velocity = 0 },
				
		initColor = { hue = 0.1 , saturation = 1.0, lightness = 0.5 , alpha = 1.0 },	
		bindToSlot = { ["1"] = "ALPHA" , ["2"] = "HUE" }
	},
	Laser = {

		viewportTransform = {
				usage = { ["1"] = "distance" },
				func = function(viewport)
					return AURA.ViewPortTransform.distance(viewport, keyPressX , keyPressY)
				end	
		},
		
		wave = { waveType = "SineWave" , min = 0 , max = 1 , waveLength = 8, 
				freq = 0.1, phase = 0.5, start = 0 , velocity = 20 },
				
		initColor = { hue = 0.3 , saturation = 1.0, lightness = 0.5 , alpha = 1.0 },
		bindToSlot = { ["1"] = "ALPHA" }
	},
	Radius = {

		viewportTransform = {
				usage = { ["0"] = "limitRadius" },
				func = function(viewport)
					return AURA.ViewPortTransform.limitRadius(viewport, keyPressX , keyPressY, keystrokeStrength * 2)
				end
		},		

		wave = { waveType = "SineWave" , min = 0 , max = 1 , waveLength = 10, 
				freq = 1 , phase = 0, start = 0 , velocity = 0 },
				
		initColor = { hue = 0.7 , saturation = 1.0, lightness = 0.5 , alpha = 1.0 },	
		bindToSlot = { ["1"] = "ALPHA" }
	},
	Ripple = {

		viewportTransform = {
				usage = { ["0"] = "radius" },
				func = function(viewport)
					return AURA.ViewPortTransform.radius(viewport, keyPressX , keyPressY)
				end
		},		

		wave = { waveType = "SineWave" , min = 0 , max = 1 , waveLength = 40, 
				freq = 1 , phase = 0, start = 0 , velocity = 5 },
				
		initColor = { hue = 0.1 , saturation = 1.0, lightness = 0.5 , alpha = 1.0 },	
		bindToSlot = { ["1"] = "ALPHA" , ["2"] = "HUE" }
	},
	Star = {

		viewportTransform = {
				usage = { ["0"] = "limitRadius" },
				func = function(viewport)
					return AURA.ViewPortTransform.limitRadius(viewport, math.random(1,22) , math.random(1,6), 0.5)
				end
		},		

		wave = { waveType = "SineWave" , min = 0 , max = 0.5 , waveLength = 1, 
				freq = 0.25 , phase = 0, start = 0 , velocity = 0 },
				
		initColor = { hue =  RandomHue , saturation = 1.0, lightness = 0.5 , alpha = 1.0 },	
		bindToSlot = { ["1"] = "LIGHTNESS" }
	}
}


-------------------------MataData Library Begin ------------------------
------------- Let UI Konws what options can be showed on App -----------
MataData = {
	ViewportTransform = {

		OrthogonaProject = { 
			input = { viewport = "2D" , anlge = "double" },
			output = { viewport = "1D" },
			discription = { "Rearrange LEDs to 1D, through orthogonal projection with an angle /n" }
		},

		distance = { 
			input = { viewport = "2D" , X = "double" , Y = "double" },
			output = { viewport = "1D" },
			discription = { "Rearrange LEDs to 1D by distance from (X, Y), specify on the same X or Y LEDs /n" }
		},

		limitRadius = {
			input = { viewport = "2D" , X = "double" , Y = "double", radius = "double" },
			output = { viewport = "1D" },
			discription = { "Rearrange LEDs to 1D by distance from (X, Y), and in the limit of radius  /n" }
		},
	},


	-- To do
	WaveType =  {  },

	BindToSlot = {	},

	DeviceType = {  }
}

-------------------------MataData Library End -------------------------