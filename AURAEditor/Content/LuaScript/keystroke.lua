require("script//global")

EventProvider = {

	name = "Combine All together",

	period = 23000,

	queue = {

		["4"] = { Effect = "Laser" , Viewport = "All" , Trigger = "KeyboardInput" , Delay = 0 , Duration = 2000 },
		["5"] = { Effect = "Radius" , Viewport = "All" , Trigger = "KeyboardInput" , Delay = 700 , Duration = 500 },
		--["6"] = { Effect = "Rainbow" , Viewport = "All" , Trigger = "OneTime" , Delay = 0 , Duration = -1 }
		--["6"] = { Effect = "Breath" , Viewport = "All" , Trigger = "OneTime" , Delay = 0 , Duration = -1 }

	},

	
	generateEvent = function() 
			local timer = ( Provider:clock():getClock() )*1000
			math.randomseed( os.time() )
			for key,value in pairs(EventProvider["queue"]) do

				local vp = Provider:GetViewport(EventProvider["queue"][key]["Viewport"])
				local executeName = EventProvider["queue"][key]["Effect"]
				local delayMilisecond = EventProvider["queue"][key]["Delay"]
				local lifeTime = EventProvider["queue"][key]["Duration"]
				local trigger = EventProvider["queue"][key]["Trigger"]

				if (trigger == "OneTime") then

					if (not global.OneTimeDone) then
						Provider:makeEvent(vp, Event[executeName] , delayMilisecond , lifeTime)
					end
			
				elseif (trigger == "Period") then

					if (not global.PeriodDone) then
						Provider:makeEvent(vp, Event[executeName] , delayMilisecond , lifeTime)
					end

				elseif (trigger == "KeyboardInput") then
					local keyPressCount = #AURA.keyStates
				
					for j = 1 , keyPressCount do
						global.keyPressX = AURA.keyStates[j].X
						global.keyPressY = AURA.keyStates[j].Y
						global.keystrokeStrength = global.keystrokeStrength + 1

						Provider:makeEvent(vp, Event[executeName] , delayMilisecond , lifeTime)

					end

					if (global.keystrokeStrength > 5) then
						Provider:makeEvent(vp, Event["Ripple"] , 2000 , 5000)
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

			if ( timer > EventProvider["period"]) then
				global.PeriodDone = false
				Provider:clock():reset()
			else
				global.PeriodDone = true
			end

    end
}

Viewport = {

	Left = {
		Flaire = {
			name = "Flaire",
			DeviceType = "Keyboard",
			layout = { weight = 23 , height = 6 },
			location = { x = 0, y = 0 },
			usageRange = {
						["1"] = { from =  23*0 + 0 , to = 23*0 + 7 },
						["2"] = { from =  23*1 + 0 , to = 23*1 + 7 },
						["3"] = { from =  23*2 + 0 , to = 23*2 + 7 },
						["4"] = { from =  23*3 + 0 , to = 23*3 + 7 },
						["5"] = { from =  23*4 + 0 , to = 23*4 + 7 },
						["6"] = { from =  23*5 + 0 , to = 23*5 + 7 }
			}
		},
		G703VI ={
			name = "G703VI",
			DeviceType = "Notebook",
			location = { x = 0, y = 0 },
			usageRange = {
						["1"] = { from =  23*0 + 0 , to = 23*0 + 22 },
						["2"] = { from =  23*1 + 0 , to = 23*1 + 22 },
						["3"] = { from =  23*2 + 0 , to = 23*2 + 22 },
						["4"] = { from =  23*3 + 0 , to = 23*3 + 22 },
						["5"] = { from =  23*4 + 0 , to = 23*4 + 22 },
						["6"] = { from =  23*5 + 0 , to = 23*5 + 22 }
			}
		
		},
	},

	Centor = {
		Flaire = {
			name = "Flaire",
			DeviceType = "Keyboard",
			layout = { weight = 23 , height = 6 },
			location = { x = 0, y = 0 },
			usageRange = {
						["1"] = { from =  23*0 + 8 , to = 23*0 + 15 },
						["2"] = { from =  23*1 + 8 , to = 23*1 + 15 },
						["3"] = { from =  23*2 + 8 , to = 23*2 + 15 },
						["4"] = { from =  23*3 + 8 , to = 23*3 + 15 },
						["5"] = { from =  23*4 + 8 , to = 23*4 + 15 },
						["6"] = { from =  23*5 + 8 , to = 23*5 + 15 }
			}
		},
		G703VI ={
			name = "G703VI",
			DeviceType = "Notebook",
			location = { x = 0, y = 0 },
			usageRange = {
						["1"] = { from =  23*0 + 0 , to = 23*0 + 22 },
						["2"] = { from =  23*1 + 0 , to = 23*1 + 22 },
						["3"] = { from =  23*2 + 0 , to = 23*2 + 22 },
						["4"] = { from =  23*3 + 0 , to = 23*3 + 22 },
						["5"] = { from =  23*4 + 0 , to = 23*4 + 22 },
						["6"] = { from =  23*5 + 0 , to = 23*5 + 22 }
			}
		
		},
	},

	Right = {
		Flaire = {
			name = "Flaire",
			DeviceType = "Keyboard",
			layout = { weight = 23 , height = 6 },
			location = { x = 0, y = 0 },
			usageRange = {
						["1"] = { from =  23*0 + 16 , to = 23*0 + 22 },
						["2"] = { from =  23*1 + 16 , to = 23*1 + 22 },
						["3"] = { from =  23*2 + 16 , to = 23*2 + 22 },
						["4"] = { from =  23*3 + 16 , to = 23*3 + 22 },
						["5"] = { from =  23*4 + 16 , to = 23*4 + 22 },
						["6"] = { from =  23*5 + 16 , to = 23*5 + 22 }
			}
		},
		G703VI ={
			name = "G703VI",
			DeviceType = "Notebook",
			location = { x = 0, y = 0 },
			usageRange = {
						["1"] = { from =  23*0 + 0 , to = 23*0 + 22 },
						["2"] = { from =  23*1 + 0 , to = 23*1 + 22 },
						["3"] = { from =  23*2 + 0 , to = 23*2 + 22 },
						["4"] = { from =  23*3 + 0 , to = 23*3 + 22 },
						["5"] = { from =  23*4 + 0 , to = 23*4 + 22 },
						["6"] = { from =  23*5 + 0 , to = 23*5 + 22 }
			}
		
		},
	},
	
	Up = {
		Flaire = {
			name = "Flaire",
			DeviceType = "Keyboard",
			layout = { weight = 23 , height = 6 },
			location = { x = 0, y = 0 },
			usageRange = {
						["1"] = { from =  23*0 + 0 , to = 23*0 + 22 },
						["2"] = { from =  23*1 + 0 , to = 23*1 + 22 },
						["3"] = { from =  23*2 + 0 , to = 23*2 + 22 },
						["4"] = { from =  23*3 + 0 , to = 23*3 + 15 },
			}
		},
		G703VI ={
			name = "G703VI",
			DeviceType = "Notebook",
			location = { x = 0, y = 0 },
			usageRange = {
						["1"] = { from =  23*0 + 0 , to = 23*0 + 22 },
						["2"] = { from =  23*1 + 0 , to = 23*1 + 22 },
						["3"] = { from =  23*2 + 0 , to = 23*2 + 22 },
						["4"] = { from =  23*3 + 0 , to = 23*3 + 22 },
						["5"] = { from =  23*4 + 0 , to = 23*4 + 22 },
						["6"] = { from =  23*5 + 0 , to = 23*5 + 22 }
			}
		
		},
	},

	Down = {
		Flaire = {
			name = "Flaire",
			DeviceType = "Keyboard",
			layout = { weight = 23 , height = 6 },
			location = { x = 0, y = 0 },
			usageRange = {
						["4"] = { from =  23*3 + 0 , to = 23*3 + 22 },
						["5"] = { from =  23*4 + 0 , to = 23*4 + 22 },
						["6"] = { from =  23*5 + 0 , to = 23*5 + 22 }
			}
		},
		G703VI ={
			name = "G703VI",
			DeviceType = "Notebook",
			location = { x = 0, y = 0 },
			usageRange = {
						["1"] = { from =  23*0 + 0 , to = 23*0 + 22 },
						["2"] = { from =  23*1 + 0 , to = 23*1 + 22 },
						["3"] = { from =  23*2 + 0 , to = 23*2 + 22 },
						["4"] = { from =  23*3 + 0 , to = 23*3 + 22 },
						["5"] = { from =  23*4 + 0 , to = 23*4 + 22 },
						["6"] = { from =  23*5 + 0 , to = 23*5 + 22 }
			}
		
		},
	},

	All = {
		Flaire = {
			name = "Flaire",
			DeviceType = "Keyboard",
			location = { x = 0, y = 0 },
			usageRange = {
						["1"] = { from =  23*0 + 0 , to = 23*0 + 22 },
						["2"] = { from =  23*1 + 0 , to = 23*1 + 22 },
						["3"] = { from =  23*2 + 0 , to = 23*2 + 22 },
						["4"] = { from =  23*3 + 0 , to = 23*3 + 22 },
						["5"] = { from =  23*4 + 0 , to = 23*4 + 22 },
						["6"] = { from =  23*5 + 0 , to = 23*5 + 22 }
			}
		},
		G703VI ={
			name = "G703VI",
			DeviceType = "Notebook",
			location = { x = 0, y = 0 },
			usageRange = {
						["1"] = { from =  23*0 + 0 , to = 23*0 + 22 },
						["2"] = { from =  23*1 + 0 , to = 23*1 + 22 },
						["3"] = { from =  23*2 + 0 , to = 23*2 + 22 },
						["4"] = { from =  23*3 + 0 , to = 23*3 + 22 },
						["5"] = { from =  23*4 + 0 , to = 23*4 + 22 },
						["6"] = { from =  23*5 + 0 , to = 23*5 + 22 }
			}
		
		},
		GL12CM = {
			name = "GL12CM",
			DeviceType = "Desktop",
			location = { x = 0, y = 0 },
			usageRange = {
						["1"] = { from =  0 , to = 25 },
			}
		},
		Pugio = {
			name = "Pugio",
			DeviceType = "Mouse",
			location = { x = 0, y = 0 },
			usage = { ["1"] = 1,  ["2"] = 2,  ["3"] = 3 }
			
		}
	}
}

Event = {
	Static = {

		viewportTransform = {
				usage = { ["0"] = "point" },
				func = function(viewport)
					return AURA.ViewPortTransform.point(viewport)
				end
		},		

		wave = { waveType = "TriangleWave" , min = 0 , max = 0.5 , waveLength = 23, 
				freq = 0 , phase = 3.14/4, start = 0 , velocity = 0 },
				
		initColor = { hue =  0.5 , saturation = 1.0, lightness = 0.5 , alpha = 1.0 },	
		bindToSlot = { ["1"] = "LIGHTNESS" }
	},
	Breath = {

		viewportTransform = {
				usage = { ["0"] = "point" },
				func = function(viewport)
					return AURA.ViewPortTransform.point(viewport)
				end
		},		

		wave = { waveType = "TriangleWave" , min = 0 , max = 0.5 , waveLength = 23, 
				freq = 0.2 , phase = 0, start = 0 , velocity = 0 },
				
		initColor = { hue =  global.RandomHue , saturation = 1.0, lightness = 0.5 , alpha = 1.0 },	
		bindToSlot = { ["1"] = "LIGHTNESS" }
	},
	Strobing = {

		viewportTransform = {
				usage = { ["0"] = "point" },
				func = function(viewport)
					return AURA.ViewPortTransform.point(viewport)
				end
		},		

		wave = { waveType = "TriangleWave" , min = 0 , max = 0.5 , waveLength = 23, 
				freq = 2 , phase = 0, start = 0 , velocity = 0 },
				
		initColor = { hue =  global.RandomHue , saturation = 1.0, lightness = 0.5 , alpha = 1.0 },	
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
					return AURA.ViewPortTransform.limitRadius(viewport, global.keyPressX , global.keyPressY, 0.5)
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
					return AURA.ViewPortTransform.distance(viewport, global.keyPressX , global.keyPressY)
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
					return AURA.ViewPortTransform.limitRadius(viewport, global.keyPressX , global.keyPressY, global.keystrokeStrength * 2)
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
					return AURA.ViewPortTransform.radius(viewport, 11 , 3)
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
					return AURA.ViewPortTransform.limitRadius(viewport, math.random(1,23) , math.random(1,6), 0.5)
				end
		},		

		wave = { waveType = "SineWave" , min = 0 , max = 0.5 , waveLength = 10, 
				freq = 0.5 , phase = 0, start = 0 , velocity = 0 },
				
		initColor = { hue =  global.RandomHue , saturation = 1.0, lightness = 0.5 , alpha = 1.0 },	
		bindToSlot = { ["1"] = "LIGHTNESS" }
	}
}