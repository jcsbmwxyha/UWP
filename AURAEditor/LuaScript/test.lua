require("script//global")

EventProvider = {

	period = 200,

	queue = {

		--["12"] = { Effect = "MusicStandard" , Viewport = "All" , Trigger = "OneTime" , Delay = 0 , Duration = -1 },
		
		--["13"] = { Effect = "MusicParty" , Viewport = "All" , Trigger = "OneTime" , Delay = 0 , Duration = -1 },
		--["14"] = { Effect = "MusicFunk" , Viewport = "All" , Trigger = "OneTime" , Delay = 0 , Duration = -1 },

		--["15"] = { Effect = "MusicRomance" , Viewport = "All" , Trigger = "OneTime" , Delay = 0 , Duration = -1 },
		--["16"] = { Effect = "MusicRomance2" , Viewport = "All" , Trigger = "AudioPeakInput" , Delay = 0 , Duration = 10000 },

		--["17"] = { Effect = "MusicHipHop" , Viewport = "All" , Trigger = "OneTime" , Delay = 0 , Duration = -1 },
		--["18"] = { Effect = "MusicHipHop2" , Viewport = "All" , Trigger = "AudioPeakInput" , Delay = 0 , Duration = 10000 },

		--["19"] = { Effect = "MusicSleep" , Viewport = "All" , Trigger = "OneTime" , Delay = 0 , Duration = -1 },
		--["20"] = { Effect = "MusicSleep2" , Viewport = "All" , Trigger = "AudioPeakInput" , Delay = 0 , Duration = 4000 },

		--["21"] = { Effect = "MusicRock" , Viewport = "All" , Trigger = "OneTime" , Delay = 0 , Duration = -1 },
		--["22"] = { Effect = "MusicRock2" , Viewport = "All" , Trigger = "AudioPeakInput" , Delay = 0 , Duration = 10000 },
		
		["1"] = { Effect = "Static" , Viewport = "All" , Trigger = "OneTime" , Delay = 0 , Duration = -1 },
		--["1"] = { Effect = "Laser" , Viewport = "All" , Trigger = "Period" , Delay = 0 , Duration = 5000 },
		--["3"] = { Effect = "Comet" , Viewport = "All" , Trigger = "Period" , Delay = 0 , Duration = 5000 },
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
					math.randomseed( os.time() )
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

					if (global.keystrokeStrength >= 20) then
						Provider:makeEvent(vp, Event["Ripple"] , 1000 , 5000)
						global.keystrokeStrength = 0
					end

					if (global.keystrokeStrength > 0) then
						global.keystrokeStrength = global.keystrokeStrength - 0.2
					end

				elseif (trigger == "AudioPeakInput") then

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
}

Viewport = {

	["All"] = {
		Flaire = {
			name = "Flaire",
			DeviceType = "Keyboard",
			location = { x = 0, y = 0 },
			usageRange = {
						["1"] = { from =  0 , to = 188 },
			}
		},
		Pugio = {
			name = "Pugio",
			DeviceType = "Mouse",
			location = { x = 0, y = 0 },
			usageRange = {
						["1"] = { from =  0 , to = 3 },
			}
		},
		GL12CM = {
			name = "GL12CM",
			DeviceType = "Desktop",
			location = { x = 0, y = 0 },
			usageRange = {
						["1"] = { from =  0 , to = 24 },
			}
		},
		G703 = {
			name = "G703GG",
			DeviceType = "Notebook",
			location = { x = 0, y = 0 },
			usageRange = {
						["1"] = { from =  0 , to = 168 },
			}
		},
		GL504GI = {
			name = "GL504GI",
			DeviceType = "Notebook",
			location = { x = 0, y = 0 },
			usageRange = {
						["1"] = { from =  0 , to = 	4 },
			}
		},
	},
	
}

Event = {
	Static = {

		viewportTransform = {
				usage = { ["0"] = "point" },
				rotate = { x = 0 , y = 0 , angle = 0 },
				func = function(viewport)
					return AURA.ViewPortTransform.point(viewport)
				end
		},		

		wave =	{
					["1"] = 
					{
						waveType = "ConstantWave" , min = 0.0 , max = 1 , 
						waveLength = 1, freq = 0 , phase = 0, start = 0 , velocity = 0, 
						bindToSlot = { ["1"] = "ALPHA" },
					},

		},
				
		initColor = { hue =  0.87 , saturation = 1.0, lightness = 0.5 , alpha = 1.0 },	

	},
	Breath = {

		viewportTransform = {
				usage = { ["0"] = "point" },
				rotate = { x = 0 , y = 0 , angle = 0 },
				func = function(viewport)
					return AURA.ViewPortTransform.point(viewport)
				end
		},		

		wave =	{
					["1"] = 
					{
						waveType = "SineWave" , min = 0.00 , max = 0.5 , 
						waveLength = 23, freq = 0.1 , phase = math.pi/2, start = 0 , velocity = 0, 
						bindToSlot = { ["1"] = "LIGHTNESS" },
					},
		},		

		initColor = { hue =  0.4 , saturation = 1.0, lightness = 0.1 , alpha = 0.2},	

	},
	Strobing = {

		viewportTransform = {
				usage = { ["0"] = "point" },
				rotate = { x = 0 , y = 0 , angle = 0 },
				func = function(viewport)
					return AURA.ViewPortTransform.point(viewport)
				end
		},		

		wave =	{
					["1"] = 
					{
						waveType = "SineWave" , min = 0.00 , max = 0.5 , 
						waveLength = 23, freq = 1 , phase = math.pi/2, start = 0 , velocity = 0, 
						bindToSlot = { ["1"] = "LIGHTNESS" },
					},
		},	
				
		initColor = { hue =  global.RandomHue , saturation = 1.0, lightness = 0.5 , alpha = 1.0 },	

	},
	Rainbow = {
	
		viewportTransform = {
				usage = { ["1"] = "OrthogonaProject" },
				rotate = { x = 0 , y = 0 , angle = 0 },
				func = function(viewport)
					return AURA.ViewPortTransform.OrthogonaProject(viewport, 0 )
				end	
		},
		
		wave =	{
					["1"] = 
					{
						waveType = "Customized" , min = 0.0 , max = 1 , 
						waveLength = 44, freq = 0.1 , phase = 0, start = 0 , velocity = 0, 
						bindToSlot = { ["1"] = "HUE" },
						Customized = {
							[1] = { phase = 0.0	, fx = 0.0 },
							[2] = { phase = 0.25, fx = 0.5 },
							[3] = { phase = 0.5	, fx = 1.0 },
							[4] = { phase = 0.75, fx = 0.5 },
							[5] = { phase = 1.0	, fx = 0.0 },
						},				
					},
		},
			
		initColor = { hue = 0.5 , saturation = 1.0, lightness = 0.5 , alpha = 1.0 },

	},
	ColorCycle = {
		viewportTransform = {
				usage = { ["1"] = "point" },
				rotate = { x = 0 , y = 0 , angle = 0 },
				func = function(viewport)
					return AURA.ViewPortTransform.point(viewport)
				end	
		},
		
		wave= { waveType = "QuarterSineWave" , min = 0 , max = 1 ,
				waveLength = 5, 
				freq = 0.04, phase = 0.0, start = 0 , velocity = 0 ,
				bindToSlot = { ["1"] = "HUE" }
				},
				
		initColor = { hue = 0.5 , saturation = 1.0, lightness = 0.1 , alpha = 1.0 },

	},
	Comet = {

		viewportTransform = {
				usage = { ["1"] = "OrthogonaProject" },
				rotate = { x = 0 , y = 0 , angle = global.RandomHue },
				func = function(viewport)
					return AURA.ViewPortTransform.OrthogonaProject(viewport, 0 )
				end
		},

		wave =	{
					["1"] = 
					{
						waveType = "TriangleWave" , min = 0.00 , max = 1 , 
						waveLength = 3, freq = 0 , phase = math.pi/2, start = -15 , velocity = 15, 
						bindToSlot = { ["1"] = "ALPHA" },
					},
		},

		initColor = { hue = global.RandomHue , saturation = 1.0, lightness = 0.5 , alpha = 1.0 },

	},
	Reactive = {

		viewportTransform = {
				usage = { ["0"] = "limitRadius" },
				rotate = { x = 0 , y = 0 , angle = 0 },
				func = function(viewport)
					return AURA.ViewPortTransform.limitRadius(viewport, global.keyPressX , global.keyPressY, 0.5)
				end
		},		

		wave =	{
					["1"] = 
					{
						waveType = "SineWave" , min = 0.00 , max = 1 , 
						waveLength = 1, freq = 0 , phase = math.pi/2, start = 0 , velocity = 0, 
						bindToSlot = { ["1"] = "ALPHA" , ["2"] = "HUE" },
					},
		},
				
		initColor = { hue = 0.1 , saturation = 1.0, lightness = 0.5 , alpha = 1.0 },	

	},
	Laser = {

		viewportTransform = {
				usage = { ["1"] = "distance" },
				rotate = { x = 0 , y = 0 , angle = 0 },
				func = function(viewport)
					return AURA.ViewPortTransform.distance(viewport, global.keyPressX , global.keyPressY)
				end	
		},

		wave =	{
					["1"] = 
					{
						waveType = "SineWave" , min = 0.00 , max = 1 , 
						waveLength = 10, freq = 0 , phase = math.pi/2, start = 0 , velocity = 15, 
						bindToSlot = { ["1"] = "ALPHA" },
					},
		},
				
		initColor = { hue = 0.3 , saturation = 1.0, lightness = 0.5 , alpha = 1.0 },

	},
	Radius = {

		viewportTransform = {
				usage = { ["0"] = "limitRadius" },
				rotate = { x = 0 , y = 0 , angle = 0 },
				func = function(viewport)
					return AURA.ViewPortTransform.limitRadius(viewport, global.keyPressX , global.keyPressY, global.keystrokeStrength * 2)
				end
		},		

		wave =	{
					["1"] = 
					{
						waveType = "SineWave" , min = 0.00 , max = 1 , 
						waveLength = 10, freq = 0 , phase = math.pi/2, start = 0 , velocity = 15, 
						bindToSlot = { ["1"] = "ALPHA" },
					},
		},
			
		initColor = { hue = 0.7 , saturation = 1.0, lightness = 0.5 , alpha = 1.0 },	
	},

	Ripple = {

		viewportTransform = {
				usage = { ["0"] = "radius" },
				rotate = { x = 0 , y = 0 , angle = 0 },
				func = function(viewport)
					return AURA.ViewPortTransform.radius(viewport, 3 , 11)
				end
		},		
		wave =	{
					["1"] = 
					{
						waveType = "SineWave" , min = 0.00 , max = 1 , 
						waveLength = 5, freq = 0 , phase = math.pi/2, start = 0 , velocity = 10, 
						bindToSlot = { ["1"] = "ALPHA" , ["2"] = "HUE" },
					},
		},
		initColor = { hue = 0.1 , saturation = 1.0, lightness = 0.5 , alpha = 1.0 },	

	},

	Star = {

		viewportTransform = {
				usage = { ["0"] = "limitRadius" },
				rotate = { x = 0 , y = 0 , angle = 0 },
				func = function(viewport)
					return AURA.ViewPortTransform.randomRadius(viewport, 5 , 0.5)
				end
		},		

		wave =	{
					["1"] = 
					{
						waveType = "SineWave" , min = 0.00 , max = 0.5 , 
						waveLength = 2, freq = global.RandomHue , phase = math.pi/2, start = 0 , velocity = 0, 
						bindToSlot = { ["1"] = "LIGHTNESS" },
					},
		},
				
		initColor = { hue =  global.RandomHue , saturation = 1.0, lightness = 0.5 , alpha = 1.0 },	

	},
	oneColumnComet = {

		viewportTransform = {
				usage = { ["0"] = "randomColumn" },
				rotate = { x = 0 , y = 0 , angle = 0 },
				func = function(viewport)
					return AURA.ViewPortTransform.randomColumn(viewport, 1)
				end
		},		

		wave =	{
					["1"] = 
					{
						waveType = "SineWave" , min = 0.00 , max = 0.5 , 
						waveLength = 2, freq = 0 , phase = math.pi/2, start = -15 , velocity = 15, 
						bindToSlot = { ["1"] = "ALPHA" },
					},
		},
				
		initColor = { hue =  global.RandomHue , saturation = 1.0, lightness = 0.5 , alpha = 1.0 },	
	},

	MusicStandard = {

		viewportTransform = {
				usage = { ["0"] = "point" },
				rotate = { x = 0 , y = 0 , angle = 0 },
				func = function(viewport)
					return AURA.ViewPortTransform.point(viewport)
				end
		},		
		wave =	{
					["ColorCycle"] = 
					{
						waveType = "SineWave" , min = 0.0 , max = 1 , 
						waveLength = 10, freq = 0.02 , phase = math.pi/2, start = 0 , velocity = 0, 
						bindToSlot = { ["1"] = "HUE" },
						Customized = {
								[1] = { phase = 0.0	, fx = 0.0 },
								[2] = { phase = 0.4 , fx = 0.4 },
								[3] = { phase = 0.5 , fx = 0.5 },
								[4] = { phase = 0.6 , fx = 0.4 },
								[5] = { phase = 1.0	, fx = 0.0 },
						},
					},
					["Audio"] = 
					{
						waveType = "ConstantWave" , min = 0,  max = 0.5 , 
						waveLength = 1, freq = 0 , phase = 0, start = 0 , velocity = 0, 
						bindToSignal = { source = "AudioPeak", target = "max", amplify = 0.35, offset = 0.0 },
						bindToSlot = { ["1"] = "LIGHTNESS" }
					}
		},
				
		initColor = { hue =  0.2 , saturation = 1.0, lightness = 0.5 , alpha = 1.0 },	
		
	},

	MusicParty = {

		viewportTransform = {
				usage = { ["0"] = "radius" },
				rotate = { x = 0 , y = 0 , angle = 0 },
				func = function(viewport)
					return AURA.ViewPortTransform.radius(viewport, 3 , 11)
				end
		},		
		wave =	{
					["Rainbow"] = 
					{
						waveType = "SawToothleWave" , min = 0.0 , max = 1 , 
						waveLength = 11, freq = 0.1 , phase = math.pi/2, start = 0 , velocity = 0, 
						bindToSlot = { ["1"] = "HUE" },
					},
					["Audio"] = 
					{
						waveType = "SquareWave" , min = 0.5 , max = 0.5 , 
						waveLength = 11, freq = 0 , phase = math.pi/2, start = 0 , velocity = 0, 
						bindToSignal = { source = "AudioPeak", target = "length",  amplify = 1, offset = 0 },
						bindToSlot = { ["1"] = "ALPHA" }
					}
		},
				
		initColor = { hue =  0 , saturation = 1.0, lightness = 0.5 , alpha = 1 },	
		
	},
	
	MusicFunk = {

		viewportTransform = {
				usage = { ["0"] = "mirror" },
				rotate = { x = 0 , y = 0 , angle = 0 },
				func = function(viewport)
					return AURA.ViewPortTransform.mirror(viewport,4)
				end
		},		
		wave =	{
					["ColorCycle"] = 
					{
						waveType = "SquareWave" , min = 0.0 , max = 0.5 , 
						waveLength = 5, freq = 0 , phase = 0, start = 0 , velocity = 0, 
						bindToSignal = { source = "AudioPeak", target = "phase", amplify = 10, offset = 0 },
						bindToSlot = { ["1"] = "HUE" },
					},
					["Audio"] = 
					{
						waveType = "ConstantWave" , min = 0,  max = 0.5 , 
						waveLength = 10, freq = 0 , phase = 0, start = 0 , velocity = 0, 
						bindToSignal = { source = "AudioPeak", target = "max", amplify = 0.35, offset = 0.0 },
						bindToSlot = { ["1"] = "LIGHTNESS" }
					}
		},
				
		initColor = { hue =  0 , saturation = 1.0, lightness = 0 , alpha = 1 },	
		
	},

	MusicRomance = {

		viewportTransform = {
				usage = { ["0"] = "OrthogonaProject" },
				rotate = { x = 0 , y = 0 , angle = 0 },
				func = function(viewport)
					return AURA.ViewPortTransform.OrthogonaProject(viewport, math.pi/2)
				end
		},		
		wave =	{
					["Color"] = 
					{
						waveType = "SineWave" , min = 0.0 , max = 1.0 , 
						waveLength = 44, freq = 0.00 , phase = math.pi/2, start = 0 , velocity = 0, 
						bindToSignal = { source = "AudioPeak", target = "phase", amplify = 2, offset = math.pi/2},
						bindToSlot = { ["1"] = "HUE" },
					},
					["Audio"] = 
					{
						waveType = "ConstantWave" , min = 0,  max = 0.5 , 
						waveLength = 10, freq = 0 , phase = 0, start = 0 , velocity = 0, 
						bindToSignal = { source = "AudioPeak", target = "max", amplify = 0.35, offset = 0.0 },
						bindToSlot = { ["1"] = "LIGHTNESS" }
					},
		},
				
		initColor = { hue =  0 , saturation = 1.0, lightness = 0 , alpha = 1 },	
		
	},

	MusicRomance2 = {

		viewportTransform = {
				usage = { ["0"] = "OrthogonaProject" },
				rotate = { x = 0 , y = 0 , angle = 0 },
				func = function(viewport)
					return AURA.ViewPortTransform.randomRadius(viewport, 4 , 2)
				end
		},		
		wave =	{
					["Color"] = 
					{
						waveType = "SineWave" , min = 0.0 , max = 1.0 , 
						waveLength = 44, freq = 0.10 , phase = 0, start = 0 , velocity = 0, 
						bindToSlot = { ["1"] = "HUE" },
					},
					["Audio"] = 
					{
						waveType = "ConstantWave" , min = 0,  max = 0.5 , 
						waveLength = 1, freq = 0 , phase = 0, start = 0 , velocity = 0, 
						bindToSignal = { source = "AudioPeak", target = "max", amplify = 0.35, offset = 0.0 },
						bindToSlot = { ["1"] = "LIGHTNESS" }
					},
		},
				
		initColor = { hue =  0.5 , saturation = 1.0, lightness = 0.35 , alpha = 1 },	
		
	},


	MusicHipHop = {

		viewportTransform = {
				usage = { ["0"] = "point" },
				rotate = { x = 0 , y = 0 , angle = 0 },
				func = function(viewport)
					return AURA.ViewPortTransform.point(viewport)
				end
		},		
		wave =	{
					["Color"] = 
					{
						waveType = "SineWave" , min = 0.0 , max = 1.0 , 
						waveLength = 10, freq = 0.02 , phase = math.pi/2, start = 0 , velocity = 0, 
						bindToSignal = { source = "AudioPeak", target = "max", amplify = 1, offset = 0.5},
						bindToSlot = { ["1"] = "HUE" },
					},

					["Audio"] = 
					{
						waveType = "ConstantWave" , min = 0,  max = 0.5 , 
						waveLength = 1, freq = 0 , phase = 0, start = 0 , velocity = 0, 
						bindToSignal = { source = "AudioPeak", target = "max", amplify = 0.35, offset = 0.0 },
						bindToSlot = { ["1"] = "LIGHTNESS" }
					},
		},
				
		initColor = { hue =  0.25 , saturation = 1, lightness = 0 , alpha = 0.5 },	
		
	},

	MusicHipHop2 = {

		viewportTransform = {
				usage = { ["0"] = "OrthogonaProject" },
				rotate = { x = 0 , y = 0 , angle = global.RandomHue },
				func = function(viewport)
					return AURA.ViewPortTransform.OrthogonaProject(viewport, 0)
				end
		},		
		wave =	{
					["CometVelocity"] = 
					{
						waveType = "SineWave" , min = 0.5,  max = 0.5 , 
						waveLength = math.random(2,8), freq = 0 , phase = 0, start = -25 , velocity = 0, 
						bindToSignal = { source = "AudioPeak", target = "velocity", amplify = 15, offset = 0 },
						bindToSlot = { ["1"] = "ALPHA" }
					},
					["CometLightness"] = 
					{
						waveType = "SquareWave" , min = 0,  max = 0 , 
						waveLength = 44, freq = 0 , phase = math.pi/2, start = 0 , velocity = 0, 
						bindToSignal = { source = "AudioPeak", target = "min", amplify = 0.5, offset = 0 },
						bindToSlot = { ["1"] = "LIGHTNESS" }
					},
		},
				
		initColor = { hue =  global.RandomHue , saturation = 1.0, lightness = 0.5 , alpha = 1 },	
		
	},

	MusicSleep = {

		viewportTransform = {
				usage = { ["0"] = "OrthogonaProject" },
				rotate = { x = 0 , y = 0 , angle = 0 },
				func = function(viewport)
					return AURA.ViewPortTransform.OrthogonaProject(viewport, math.pi/2)
				end
		},		
		wave =	{
					["Audio"] = 
					{
						waveType = "ConstantWave" , min = 0,  max = 0.5 , 
						waveLength = 1, freq = 0 , phase = 0, start = 0 , velocity = 0, 
						bindToSignal = { source = "AudioPeak", target = "max", amplify = 0.35, offset = 0.0 },
						bindToSlot = { ["1"] = "LIGHTNESS" }
					},
		},
				
		initColor = { hue =  0.6 , saturation = 0.5, lightness = 0 , alpha = 0.5 },	
		
	},

	MusicSleep2 = {

		viewportTransform = {
				usage = { ["0"] = "OrthogonaProject" },
				rotate = { x = 0 , y = 0 , angle = 0 },
				func = function(viewport)
					return AURA.ViewPortTransform.randomColumn(viewport, 1)
				end
		},		
		wave =	{
					["Color"] = 
					{
						waveType = "SineWave" , min = 0.0 , max = 1.0 , 
						waveLength = 44, freq = 0.00 , phase = math.pi/2, start = 0 , velocity = 0, 
						bindToSignal = { source = "AudioPeak", target = "max", amplify = 0.2, offset = 0},
						bindToSlot = { ["1"] = "HUE" },
					},
					["Audio"] = 
					{
						waveType = "SineWave" , min = 0.5,  max = 0.5 , 
						waveLength = math.random(2,8), freq = 0 , phase = 0, start = 0 , velocity = 0, 
						bindToSignal = { source = "AudioPeak", target = "velocity", amplify = 20, offset = 0 },
						bindToSlot = { ["1"] = "ALPHA" }
					},
		},
				
		initColor = { hue =  0.0 , saturation = 1.0, lightness = 0.5 , alpha = 1 },	
		
	},


	MusicRock = {

		viewportTransform = {
				usage = { ["0"] = "point" },
				rotate = { x = 0 , y = 0 , angle = 0 },
				func = function(viewport)
					return AURA.ViewPortTransform.point(viewport)
				end
		},		
		wave =	{
					["Color"] = 
					{
						waveType = "SineWave" , min = 0.0 , max = 1.0 , 
						waveLength = 10, freq = 0.02 , phase = math.pi/2, start = 0 , velocity = 0, 
						bindToSignal = { source = "AudioSpectrum", target = "max", amplify = 1, offset = 0.5},
						bindToSlot = { ["1"] = "HUE" },
					},

					["Audio"] = 
					{
						waveType = "SquareWave" , min = 0,  max = 0 , 
						waveLength = 10, freq = 0 , phase = math.pi/2, start = 0 , velocity = 0, 
						bindToSignal = { source = "AudioSpectrum", target = "min", amplify = 5, offset = 0.0 },
						bindToSlot = { ["1"] = "LIGHTNESS" }
					},
		},
				
		initColor = { hue =  0.25 , saturation = 1, lightness = 0 , alpha = 0.5 },	
		
	},

	MusicRock2 = {

		viewportTransform = {
				usage = { ["0"] = "radius" },
				rotate = { x = 0 , y = 0 , angle = 0 },
				func = function(viewport)
					return AURA.ViewPortTransform.sandwitch(viewport)
				end
		},		
		wave =	{
					["1"] = 
					{
						waveType = "SineWave" , min = 0.1 , max = 1 , 
						waveLength = 1 , freq = 0.0 , phase = 0, start = 0 , velocity = 10, 
						bindToSignal = { source = "AudioPeak", target = "velocity", amplify = 10, offset = 0 },
						bindToSlot = { ["1"] = "ALPHA" },
					},

					["CometLightness"] = 
					{
						waveType = "SquareWave" , min = 0,  max = 0 , 
						waveLength = 44, freq = 0 , phase = math.pi/2, start = 0 , velocity = 0, 
						bindToSignal = { source = "AudioPeak", target = "min", amplify = 0.5, offset = 0 },
						bindToSlot = { ["1"] = "LIGHTNESS" }
					},
		},
				
		initColor = { hue =  global.RandomHue , saturation = 1.0, lightness = 0.5 , alpha = 1 },	
		
	},


	-- Just TEST, not ready
	MusicDot = {

		viewportTransform = {
				usage = { ["0"] = "randomRadius" },
				rotate = { x = 0 , y = 0 , angle = 0 },
				func = function(viewport)
					return AURA.ViewPortTransform.randomRadius(viewport, 3 , 7)
				end
		},		
		wave =	{
					["Rainbow"] = 
					{
						waveType = "SawToothleWave" , min = 0.0 , max = 1 , 
						waveLength = 11, freq = 0.1 , phase = math.pi/2, start = 0 , velocity = 0, 
						bindToSlot = { ["1"] = "HUE" },
					},
					["Audio"] = 
					{
						waveType = "SquareWave" , min = 0 , max = 0.5 , 
						waveLength = 20, freq = 0 , phase = math.pi/2, start = 0 , velocity = 5, 
						bindToSignal = { source = "AudioPeak", target = "length",  amplify = 1, offset = 0 },
						bindToSlot = { ["1"] = "LIGHTNESS" }
					}
		},
				
		initColor = { hue = 0 , saturation = 1.0, lightness = 0.5 , alpha = 0 },	

	},

	MusicComet = {

		viewportTransform = {
				usage = { ["0"] = "sandwitch" },
				rotate = { x = 0 , y = 0 , angle = 0 },
				func = function(viewport)
					return AURA.ViewPortTransform.sandwitch(viewport)
				end
		},		
		wave =	{
					["1"] = 
					{
						waveType = "TriangleWave" , min = 0.00 , max = 1 , 
						waveLength = 3, freq = 0 , phase = math.pi/2, start = 0 , velocity = 15, 
						--bindToSignal = { source = "AudioPeak", target = "velocity", amplify = 20, offset = -10 },
						bindToSlot = { ["1"] = "ALPHA" },
					},
		},
				
		initColor = { hue =  0 , saturation = 1.0, lightness = 0.5 , alpha = 1 },	
		
	},

	MusicParty2 = {

		viewportTransform = {
				usage = { ["0"] = "mirror" },
				rotate = { x = 0 , y = 0 , angle = 0 },
				func = function(viewport)
					return AURA.ViewPortTransform.mirror(viewport, 22)
				end
		},		
		wave =	{
					["Rainbow"] = 
					{
						waveType = "SawToothleWave" , min = 0.0 , max = 1 , 
						waveLength = 22, freq = 0.1 , phase = math.pi/2, start = 0 , velocity = 0, 
						bindToSlot = { ["1"] = "HUE" },
					},
					["Audio"] = 
					{
						waveType = "SquareWave" , min = 0.5 , max = 0.5 , 
						waveLength = 22, freq = 0 , phase = math.pi, start = 0 , velocity = 0, 
						bindToSignal = { source = "AudioPeak", target = "length", amplify = 44, offset = 0 },
						bindToSlot = { ["1"] = "LIGHTNESS" }
					},		},
				
		initColor = { hue =  0 , saturation = 1.0, lightness = 0.5 , alpha = 1 },	
		
	},
}