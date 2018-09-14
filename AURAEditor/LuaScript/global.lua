-- It is Aura module

global = {

	OneTimeDone = false,
	PeriodDone = false,
	keystrokeStrength = 0,
	keyPressX = 0,
	keyPressY = 0,

	RandomHue = function()
		return math.random()
	end,

	MataData = {
		ViewportTransform = {
			OrthogonaProject = { 
				input = { viewport = "2D" , anlge = "double" },
				output = { viewport = "1D" },
				discription = { "Rearrange LEDs to 1D, through orthogonal projection with an angle /n" }
			},
			
			point = {
				input = { viewport = "2D" },
				output = { viewport = "1D" },
				discription = { "Make all LEDs as a point/n" }
			},

			distance = { 
				input = { viewport = "2D" , X = "double" , Y = "double" },
				output = { viewport = "1D" },
				discription = { "Rearrange LEDs to 1D by distance from (X, Y), specify on the same X or Y coordinate LEDs /n" }
			},
			
			radius = {
				input = { viewport = "2D" , X = "double" , Y = "double" },
				output = { viewport = "1D" },
				discription = { "Rearrange LEDs to 1D by distance from (X, Y) /n" }
			
			
			},

			limitRadius = {
				input = { viewport = "2D" , X = "double" , Y = "double", radius = "double" },
				output = { viewport = "1D" },
				discription = { "Rearrange LEDs to 1D by distance from (X, Y), and in the limit of radius  /n" }
			},
		},

		WaveType =  { 
		
			["1"] = "SineWave" ,			["2"] = "HalfSineWave",				["3"] = "QuarterSineWave",
			["4"] = "SquareWave",			["5"] = "TriangleWave",				["6"] = "SawToothleWave",	
		
		},
		

		BindToSlot = { ["1"] = "HUE",	["2"] = "SATURATION",	["3"] = "LIGHTNESS",	["4"] = "ALPHA" },

		DeviceType = { 

			["1"] = "MotherBoard",	["2"] = "MotherBoard_Addresable", ["3"] = "Desktop" , ["4"] = "VGA",
			
			["5"] = "Display", ["6"] = "Headset", ["7"] = "Microphone", ["8"] = "External_HardDrive",
			
			["9"] = "External_BlueRay", ["10"] =  "DRAM" , ["11"] = "Keyboard" ,  ["12"] = "Notebook",
			
			["13"] = "Notebook_Lite" , ["14"] = "Mouse" , ["15"] = "Chassis" , ["16"] = "Projector",

		}
	}

}

return global