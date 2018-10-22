using Windows.UI;
using static AuraEditor.Common.XmlHelper;
using static AuraEditor.Common.EffectHelper;
using System.Xml;
using AuraEditor.Common;
using System.Collections.Generic;

namespace AuraEditor
{
    public class EffectInfo
    {
        public string Name;
        public int Type;
        public Color InitColor;
        public int Brightness;
        public int Speed;
        public double Angle;
        public int Direction;
        public bool Random;
        public int High;
        public int Low;
        public List<ColorPoint> ColorPointList;

        public EffectInfo()
        {
        }

        public EffectInfo(int type)
        {
            Name = GetEffectName(type);
            Type = type;
            InitColor = Colors.Red;
            Brightness = 3;
            Speed = 1;
            Direction = 2;
            Angle = 90;
            Random = false;
            High = 60;
            Low = 30;
            ColorPointList = new List<ColorPoint>(MainPage.Self.CallDefaultList()[0]);
        }

        public XmlNode ToXmlNodeForScript()
        {
            XmlNode effectNode = CreateXmlNodeOfFile("effect");

            effectNode.AppendChild(GetViewportTransformXmlNode());
            effectNode.AppendChild(GetWaveListXmlNode());
            effectNode.AppendChild(GetInitColorXmlNode(InitColor, Random));

            return effectNode;
        }
        #region viewportTransform Node
        private XmlNode GetViewportTransformXmlNode()
        {
            XmlNode viewportTransformNode = CreateXmlNodeOfFile("viewportTransform");
            double[] hsl = AuraEditorColorHelper.RgbTOHsl(InitColor);

            XmlNode rotateNode = CreateXmlNodeOfFile("rotate");

            XmlNode xNode = CreateXmlNodeOfFile("x");
            xNode.InnerText = "0";
            rotateNode.AppendChild(xNode);

            XmlNode yNode = CreateXmlNodeOfFile("y");
            yNode.InnerText = "0";
            rotateNode.AppendChild(yNode);

            XmlNode angleNode = CreateXmlNodeOfFile("angle");
            angleNode.InnerText = (((Angle - 90) / 360) * -1).ToString();
            rotateNode.AppendChild(angleNode);
            
            XmlNode methodNode = GetMethodXmlNode(Type);
            viewportTransformNode.AppendChild(rotateNode);
            viewportTransformNode.AppendChild(methodNode);

            return viewportTransformNode;
        }
        static private XmlNode GetMethodXmlNode(int effType)
        {
            string methodString = "point";
            XmlNode methodNode = CreateXmlNodeOfFile("method");


            if (GetEffectName(effType) == "Static" ||
                GetEffectName(effType) == "Breath" ||
                GetEffectName(effType) == "ColorCycle" ||
                GetEffectName(effType) == "Strobing")
            {
                methodString = "point";
            }
            else if (GetEffectName(effType) == "Rainbow" ||
                     GetEffectName(effType) == "Comet" ||
                     GetEffectName(effType) == "Tide")
            {
                XmlNode inputNode = CreateXmlNodeOfFile("input");
                inputNode.InnerText = "0";
                methodNode.AppendChild(inputNode);

                methodString = "OrthogonaProject";
            }
            else if (GetEffectName(effType) == "Reactive")
            {
                XmlNode inputNode = CreateXmlNodeOfFile("input");
                inputNode.InnerText = "keyPressX";
                methodNode.AppendChild(inputNode);

                XmlNode inputNode2 = CreateXmlNodeOfFile("input");
                inputNode2.InnerText = "keyPressY";
                methodNode.AppendChild(inputNode2);

                XmlNode inputNode3 = CreateXmlNodeOfFile("input");
                inputNode3.InnerText = "0.5";
                methodNode.AppendChild(inputNode3);

                methodString = "limitRadius";
            }
            else if (GetEffectName(effType) == "Laser")
            {
                XmlNode inputNode = CreateXmlNodeOfFile("input");
                inputNode.InnerText = "keyPressX";
                methodNode.AppendChild(inputNode);

                XmlNode inputNode2 = CreateXmlNodeOfFile("input");
                inputNode2.InnerText = "keyPressY";
                methodNode.AppendChild(inputNode2);

                methodString = "distance";
            }
            else if (GetEffectName(effType) == "Ripple")
            {
                XmlNode inputNode = CreateXmlNodeOfFile("input");
                inputNode.InnerText = "keyPressX";
                methodNode.AppendChild(inputNode);

                XmlNode inputNode2 = CreateXmlNodeOfFile("input");
                inputNode2.InnerText = "keyPressY";
                methodNode.AppendChild(inputNode2);

                methodString = "radius";
            }
            else if(GetEffectName(effType) == "Star")
            {
                XmlNode inputNode = CreateXmlNodeOfFile("input");
                inputNode.InnerText = "10";
                methodNode.AppendChild(inputNode);

                XmlNode inputNode2 = CreateXmlNodeOfFile("input");
                inputNode2.InnerText = "0.5";
                methodNode.AppendChild(inputNode2);

                methodString = "randomRadius";
            }

            XmlAttribute attribute = CreateXmlAttributeOfFile("key");
            attribute.Value = methodString;
            methodNode.Attributes.Append(attribute);
            
            return methodNode;
        }
        #endregion

        #region waveList Node
        private XmlNode GetWaveListXmlNode()
        {
            XmlNode waveListNode = CreateXmlNodeOfFile("waveList");
            XmlNode waveNode = CreateXmlNodeOfFile("wave");

            XmlNode typeNode = CreateXmlNodeOfFile("type");
            typeNode.InnerText = GetWaveType(Type);
            waveNode.AppendChild(typeNode);

            XmlNode maxNode = CreateXmlNodeOfFile("max");
            maxNode.InnerText = GetMax(Type).ToString();
            waveNode.AppendChild(maxNode);

            XmlNode minNode = CreateXmlNodeOfFile("min");
            minNode.InnerText = GetMin(Type).ToString();
            waveNode.AppendChild(minNode);

            XmlNode lengthNode = CreateXmlNodeOfFile("length");
            lengthNode.InnerText = GetLength(Type).ToString();
            waveNode.AppendChild(lengthNode);

            if (GetEffectName(Type) == "Tide")
            {
                XmlNode freqNode = CreateXmlNodeOfFile("freq");
                freqNode.InnerText = (0.25).ToString();
                waveNode.AppendChild(freqNode);
            }
            else
            {
                XmlNode freqNode = CreateXmlNodeOfFile("freq");
                freqNode.InnerText = GetFreq(Type, Speed);
                waveNode.AppendChild(freqNode);
            }

            XmlNode phaseNode = CreateXmlNodeOfFile("phase");
            phaseNode.InnerText = GetPhase(Type);
            waveNode.AppendChild(phaseNode);

            XmlNode startNode = CreateXmlNodeOfFile("start");
            startNode.InnerText = GetStart(Type).ToString();
            waveNode.AppendChild(startNode);

            XmlNode velocityNode = CreateXmlNodeOfFile("velocity");
            velocityNode.InnerText = GetVelocity(Type, Speed).ToString();
            waveNode.AppendChild(velocityNode);

            XmlNode isCycleNode = CreateXmlNodeOfFile("isCycle");
            isCycleNode.InnerText = GetIsCycle(Type).ToString();
            waveNode.AppendChild(isCycleNode);

            if (GetEffectName(Type) == "Tide")
            {
                waveNode.AppendChild(GetBindToSignalXmlNode(Type, Speed));
            }

            if (GetEffectName(Type) == "Rainbow")
            {
                waveNode.AppendChild(GetBindToSlotXmlNode(Type, "HUE"));
                waveNode.AppendChild(GetCustomized(ColorPointList, 0));
            }
            else
            {
                waveNode.AppendChild(GetBindToSlotXmlNode(Type));
            }
            waveListNode.AppendChild(waveNode);

            if (GetEffectName(Type) == "Rainbow")
            {
                XmlNode waveNode1 = CreateXmlNodeOfFile("wave");

                XmlNode typeNode1 = CreateXmlNodeOfFile("type");
                typeNode1.InnerText = GetWaveType(Type);
                waveNode1.AppendChild(typeNode1);

                XmlNode maxNode1 = CreateXmlNodeOfFile("max");
                maxNode1.InnerText = GetMax(Type).ToString();
                waveNode1.AppendChild(maxNode1);

                XmlNode minNode1 = CreateXmlNodeOfFile("min");
                minNode1.InnerText = GetMin(Type).ToString();
                waveNode1.AppendChild(minNode1);

                XmlNode lengthNode1 = CreateXmlNodeOfFile("length");
                lengthNode1.InnerText = GetLength(Type).ToString();
                waveNode1.AppendChild(lengthNode1);

                XmlNode freqNode1 = CreateXmlNodeOfFile("freq");
                freqNode1.InnerText = GetFreq(Type, Speed);
                waveNode1.AppendChild(freqNode1);

                XmlNode phaseNode1 = CreateXmlNodeOfFile("phase");
                phaseNode1.InnerText = GetPhase(Type);
                waveNode1.AppendChild(phaseNode1);

                XmlNode startNode1 = CreateXmlNodeOfFile("start");
                startNode1.InnerText = GetStart(Type).ToString();
                waveNode1.AppendChild(startNode1);

                XmlNode velocityNode1 = CreateXmlNodeOfFile("velocity");
                velocityNode1.InnerText = GetVelocity(Type, Speed).ToString();
                waveNode1.AppendChild(velocityNode1);

                XmlNode isCycleNode1 = CreateXmlNodeOfFile("isCycle");
                isCycleNode1.InnerText = GetIsCycle(Type).ToString();
                waveNode1.AppendChild(isCycleNode1);

                waveNode1.AppendChild(GetBindToSlotXmlNode(Type, "SATURATION"));
                waveNode1.AppendChild(GetCustomized(ColorPointList, 1));
                waveListNode.AppendChild(waveNode1);

                XmlNode waveNode2 = CreateXmlNodeOfFile("wave");

                XmlNode typeNode2 = CreateXmlNodeOfFile("type");
                typeNode2.InnerText = GetWaveType(Type);
                waveNode2.AppendChild(typeNode2);

                XmlNode maxNode2 = CreateXmlNodeOfFile("max");
                maxNode2.InnerText = GetMax(Type).ToString();
                waveNode2.AppendChild(maxNode2);

                XmlNode minNode2 = CreateXmlNodeOfFile("min");
                minNode2.InnerText = GetMin(Type).ToString();
                waveNode2.AppendChild(minNode2);

                XmlNode lengthNode2 = CreateXmlNodeOfFile("length");
                lengthNode2.InnerText = GetLength(Type).ToString();
                waveNode2.AppendChild(lengthNode2);

                XmlNode freqNode2 = CreateXmlNodeOfFile("freq");
                freqNode2.InnerText = GetFreq(Type, Speed);
                waveNode2.AppendChild(freqNode2);

                XmlNode phaseNode2 = CreateXmlNodeOfFile("phase");
                phaseNode2.InnerText = GetPhase(Type);
                waveNode2.AppendChild(phaseNode2);

                XmlNode startNode2 = CreateXmlNodeOfFile("start");
                startNode2.InnerText = GetStart(Type).ToString();
                waveNode2.AppendChild(startNode2);

                XmlNode velocityNode2 = CreateXmlNodeOfFile("velocity");
                velocityNode2.InnerText = GetVelocity(Type, Speed).ToString();
                waveNode2.AppendChild(velocityNode2);

                XmlNode isCycleNode2 = CreateXmlNodeOfFile("isCycle");
                isCycleNode2.InnerText = GetIsCycle(Type).ToString();
                waveNode2.AppendChild(isCycleNode2);

                waveNode2.AppendChild(GetBindToSlotXmlNode(Type, "LIGHTNESS"));
                waveNode2.AppendChild(GetCustomized(ColorPointList, 2));
                waveListNode.AppendChild(waveNode2);
            }

            return waveListNode;
        }
        static private string GetWaveType(int effType)
        {
            if (GetEffectName(effType) == "Static") return "ConstantWave";
            else if (GetEffectName(effType) == "Breath") return "SineWave";
            else if (GetEffectName(effType) == "ColorCycle") return "QuarterSineWave";
            else if (GetEffectName(effType) == "Rainbow") return "CustomStepWave";
            else if (GetEffectName(effType) == "Strobing") return "SawtoothWave";
            else if (GetEffectName(effType) == "Comet") return "TriangleWave";
            else if (GetEffectName(effType) == "Reactive") return "SineWave";
            else if (GetEffectName(effType) == "Laser") return "SineWave";
            else if (GetEffectName(effType) == "Ripple") return "SineWave";
            else if (GetEffectName(effType) == "Star") return "SineWave";
            else if (GetEffectName(effType) == "Tide") return "SineWave";

            return "SineWave";
        }
        static private double GetMax(int effType)
        {
            if (GetEffectName(effType) == "Breath") return 0.5;
            else if (GetEffectName(effType) == "Strobing") return 0.5;
            else if (GetEffectName(effType) == "Comet") return 0.1;
            else if (GetEffectName(effType) == "Star") return 1;
            else if (GetEffectName(effType) == "Tide") return 0.4;
            return 1;
        }
        static private double GetMin(int effType)
        {
            if (GetEffectName(effType) == "Star") return 0.7;
            else if (GetEffectName(effType) == "Tide") return 0.2;
            return 0;
        }
        static private double GetLength(int effType)
        {
            if (GetEffectName(effType) == "Static") return 10;
            else if (GetEffectName(effType) == "Breath") return 23;
            else if (GetEffectName(effType) == "ColorCycle") return 23;
            else if (GetEffectName(effType) == "Rainbow") return 64;
            else if (GetEffectName(effType) == "Strobing") return 23;
            else if (GetEffectName(effType) == "Comet") return 3;
            else if (GetEffectName(effType) == "Reactive") return 1;
            else if (GetEffectName(effType) == "Laser") return 10;
            else if (GetEffectName(effType) == "Ripple") return 10;
            else if (GetEffectName(effType) == "Star") return 2;
            else if (GetEffectName(effType) == "Tide") return 22;

            return 10;
        }
        static private string GetFreq(int effType, int speed)
        {
            double result = 0;

            if (speed == 0)
            {
                if (GetEffectName(effType) == "Breath") result = 0.1;
                else if (GetEffectName(effType) == "ColorCycle") result = -0.02;
                else if (GetEffectName(effType) == "Rainbow") result = 0.04;
                else if (GetEffectName(effType) == "Strobing") result = 0.8;
                else if (GetEffectName(effType) == "Comet") result = 0.01;
                else if (GetEffectName(effType) == "Reactive") result = 1;
                else if (GetEffectName(effType) == "Laser") return "Random";
                else if (GetEffectName(effType) == "Ripple") result = 1;
                else if (GetEffectName(effType) == "Star") return "Random";
                else if (GetEffectName(effType) == "Tide") result = 0.05;
            }
            else if (speed == 1)
            {
                if (GetEffectName(effType) == "Breath") result = 0.2;
                else if (GetEffectName(effType) == "ColorCycle") result = -0.04;
                else if (GetEffectName(effType) == "Rainbow") result = 0.08;
                else if (GetEffectName(effType) == "Strobing") result = 1.2;
                else if (GetEffectName(effType) == "Comet") result = 0.02;
                else if (GetEffectName(effType) == "Reactive") result = 2;
                else if (GetEffectName(effType) == "Laser") return "Random";
                else if (GetEffectName(effType) == "Ripple") result = 2;
                else if (GetEffectName(effType) == "Star") return "Random";
                else if (GetEffectName(effType) == "Tide") result = 0.1;
            }
            else if (speed == 2)
            {
                if (GetEffectName(effType) == "Breath") result = 0.4;
                else if (GetEffectName(effType) == "ColorCycle") result = -0.08;
                else if (GetEffectName(effType) == "Rainbow") result = 0.1;
                else if (GetEffectName(effType) == "Strobing") result = 1.6;
                else if (GetEffectName(effType) == "Comet") result = 0.04;
                else if (GetEffectName(effType) == "Reactive") result = 4;
                else if (GetEffectName(effType) == "Laser") return "Random";
                else if (GetEffectName(effType) == "Ripple") result = 4;
                else if (GetEffectName(effType) == "Star") return "Random";
                else if (GetEffectName(effType) == "Tide") result = 0.3;
            }

            return result.ToString();
        }
        static private string GetPhase(int effType)
        {
            if (GetEffectName(effType) == "Star")
                return "Random";

            return "0";
        }
        static private double GetStart(int effType)
        {
            if (GetEffectName(effType) == "Tide") return -5;

            return 0;
        }
        static private double GetVelocity(int effType, int speed)
        {
            if (speed == 0)
            {
                if (GetEffectName(effType) == "Comet") return 10;
                else if (GetEffectName(effType) == "Laser") return 10;
                else if (GetEffectName(effType) == "Ripple") return 10;
            }
            else if (speed == 1)
            {
                if (GetEffectName(effType) == "Comet") return 15;
                else if (GetEffectName(effType) == "Laser") return 15;
                else if (GetEffectName(effType) == "Ripple") return 15;
            }
            else if (speed == 2)
            {
                if (GetEffectName(effType) == "Comet") return 20;
                else if (GetEffectName(effType) == "Laser") return 20;
                else if (GetEffectName(effType) == "Ripple") return 20;
            }

            return 0;
        }
        static private int GetIsCycle(int effType)
        {
            if (GetEffectName(effType) == "Comet") return 1;

            return 0;
        }
        static private XmlNode GetBindToSignalXmlNode(int effType, int speed)
        {
            XmlNode bindToSignalXmlNode = CreateXmlNodeOfFile("bindToSignal");

            XmlNode sourceNode = CreateXmlNodeOfFile("source");
            sourceNode.InnerText = "Formula";
            bindToSignalXmlNode.AppendChild(sourceNode);

            XmlNode targetNode = CreateXmlNodeOfFile("target");
            targetNode.InnerText = "length";
            bindToSignalXmlNode.AppendChild(targetNode);

            XmlNode amplifyNode = CreateXmlNodeOfFile("amplify");
            amplifyNode.InnerText = "1";
            bindToSignalXmlNode.AppendChild(amplifyNode);

            XmlNode offsetNode = CreateXmlNodeOfFile("offset");
            offsetNode.InnerText = "0";
            bindToSignalXmlNode.AppendChild(offsetNode);

            XmlNode typeNode = CreateXmlNodeOfFile("type");
            typeNode.InnerText = GetWaveType(effType);
            bindToSignalXmlNode.AppendChild(typeNode);

            XmlNode maxNode = CreateXmlNodeOfFile("max");
            maxNode.InnerText = "29";
            bindToSignalXmlNode.AppendChild(maxNode);

            XmlNode minNode = CreateXmlNodeOfFile("min");
            minNode.InnerText = "0";
            bindToSignalXmlNode.AppendChild(minNode);

            XmlNode lengthNode = CreateXmlNodeOfFile("length");
            lengthNode.InnerText = "25";
            bindToSignalXmlNode.AppendChild(lengthNode);
            
            XmlNode freqNode = CreateXmlNodeOfFile("freq");
            freqNode.InnerText = GetFreq(effType, speed);
            bindToSignalXmlNode.AppendChild(freqNode);

            return bindToSignalXmlNode;
        }

        static private XmlNode GetBindToSlotXmlNode(int effType)
        {
            XmlNode bindToSlotXmlNode = CreateXmlNodeOfFile("bindToSlot");
            XmlNode slotNode = CreateXmlNodeOfFile("slot");

            XmlAttribute attribute = CreateXmlAttributeOfFile("key");

            if (GetEffectName(effType) == "Static")
            {
                attribute.Value = "ALPHA";
                slotNode.Attributes.Append(attribute);
            }
            else if (GetEffectName(effType) == "Breath")
            {
                attribute.Value = "LIGHTNESS";
                slotNode.Attributes.Append(attribute);
            }
            else if (GetEffectName(effType) == "ColorCycle")
            {
                attribute.Value = "HUE";
                slotNode.Attributes.Append(attribute);
            }
            else if (GetEffectName(effType) == "Rainbow")
            {
                attribute.Value = "HUE";
                slotNode.Attributes.Append(attribute);
            }
            else if (GetEffectName(effType) == "Strobing")
            {
                attribute.Value = "LIGHTNESS";
                slotNode.Attributes.Append(attribute);
            }
            else if (GetEffectName(effType) == "Comet")
            {
                attribute.Value = "ALPHA";
                slotNode.Attributes.Append(attribute);
            }
            else if (GetEffectName(effType) == "Reactive")
            {
                attribute.Value = "ALPHA";
                slotNode.Attributes.Append(attribute);
            }
            else if (GetEffectName(effType) == "Laser")
            {
                attribute.Value = "ALPHA";
                slotNode.Attributes.Append(attribute);
            }
            else if (GetEffectName(effType) == "Ripple")
            {
                attribute.Value = "ALPHA";
                slotNode.Attributes.Append(attribute);
            }
            else if (GetEffectName(effType) == "Star")
            {
                attribute.Value = "LIGHTNESS";
                slotNode.Attributes.Append(attribute);
            }
            else if (GetEffectName(effType) == "Tide")
            {
                attribute.Value = "ALPHA";
                slotNode.Attributes.Append(attribute);
            }
            bindToSlotXmlNode.AppendChild(slotNode);

            // second slot
            //if (GetEffectName(effType) == "Ripple")
            //{
            //    XmlNode slotNode2 = CreateXmlNodeOfFile("slot");
            //    XmlAttribute attribute2 = CreateXmlAttributeOfFile("key");
            //    attribute2.Value = "HUE";
            //    slotNode2.Attributes.Append(attribute2);
            //    bindToSlotXmlNode.AppendChild(slotNode2);
            //}

            return bindToSlotXmlNode;
        }
        static private XmlNode GetBindToSlotXmlNode(int effType, string slotkey)
        {
            XmlNode bindToSlotXmlNode = CreateXmlNodeOfFile("bindToSlot");
            XmlNode slotNode = CreateXmlNodeOfFile("slot");
            XmlAttribute attribute = CreateXmlAttributeOfFile("key");

            attribute.Value = slotkey;
            slotNode.Attributes.Append(attribute);
            bindToSlotXmlNode.AppendChild(slotNode);

            return bindToSlotXmlNode;
        }

        static private XmlNode GetCustomized(List<ColorPoint> ColorPointList, int hsv)
        {
            XmlNode customizedNode = CreateXmlNodeOfFile("customized");
            double Scaledown = (double)(ColorPointList.Count - 1) / (double)ColorPointList.Count;

            for (int i = 0; i<ColorPointList.Count; i++)
            {
                double ScaledownOffset = ColorPointList[i].Offset * Scaledown * ColorPointList[ColorPointList.Count - 1].Offset;
                if (i == 0)
                {
                    customizedNode.AppendChild(SetNodeInfoXmlNode(i, 0, ColorPointList[i].Color, hsv));
                }
                customizedNode.AppendChild(SetNodeInfoXmlNode(i, ScaledownOffset, ColorPointList[i].Color, hsv));
                if (i == (ColorPointList.Count - 1))
                {
                    customizedNode.AppendChild(SetNodeInfoXmlNode(i, 1, ColorPointList[0].Color, hsv));
                }
            }
            return customizedNode;
        }
        static private XmlNode SetNodeInfoXmlNode(int key, double offset, Color c, int hsv)
        {
            XmlNode nodeXmlNode = CreateXmlNodeOfFile("node");
            XmlAttribute attribute = CreateXmlAttributeOfFile("key");
            XmlNode phaseXmlNode = CreateXmlNodeOfFile("phase");
            XmlNode fxXmlNode = CreateXmlNodeOfFile("fx");
            double[] hsl = AuraEditorColorHelper.RgbTOHsl(c);

            attribute.Value = key.ToString();
            nodeXmlNode.Attributes.Append(attribute);
            phaseXmlNode.InnerText = offset.ToString();
            nodeXmlNode.AppendChild(phaseXmlNode);
            fxXmlNode.InnerText = hsl[hsv].ToString();
            nodeXmlNode.AppendChild(fxXmlNode);

            return nodeXmlNode;
        }


    #endregion

    #region initColor
    static private XmlNode GetInitColorXmlNode(Color c, bool random)
        {
            XmlNode initColorNode = CreateXmlNodeOfFile("initColor");
            double[] hsl = AuraEditorColorHelper.RgbTOHsl(c);

            XmlNode hueNode = CreateXmlNodeOfFile("hue");
            if (random == true)
            {
                hueNode.InnerText = "Random";
            }
            else
            {
                hueNode.InnerText = hsl[0].ToString();
            }
            initColorNode.AppendChild(hueNode);

            XmlNode saturationNode = CreateXmlNodeOfFile("saturation");
            saturationNode.InnerText = hsl[1].ToString();
            initColorNode.AppendChild(saturationNode);

            XmlNode lightnessNode = CreateXmlNodeOfFile("lightness");
            lightnessNode.InnerText = hsl[2].ToString();
            initColorNode.AppendChild(lightnessNode);

            XmlNode alphaNode = CreateXmlNodeOfFile("alpha");
            alphaNode.InnerText = (c.A / 255).ToString();
            initColorNode.AppendChild(alphaNode);

            return initColorNode;
        }
        #endregion

        public XmlNode ToXmlNodeForUserData()
        {
            XmlNode effectNode = CreateXmlNodeOfFile("effect");

            XmlNode typeNode = CreateXmlNodeOfFile("type");
            typeNode.InnerText = Type.ToString();
            effectNode.AppendChild(typeNode);

            XmlNode aNode = CreateXmlNodeOfFile("a");
            aNode.InnerText = InitColor.A.ToString();
            effectNode.AppendChild(aNode);

            XmlNode rNode = CreateXmlNodeOfFile("r");
            rNode.InnerText = InitColor.R.ToString();
            effectNode.AppendChild(rNode);

            XmlNode gNode = CreateXmlNodeOfFile("g");
            gNode.InnerText = InitColor.G.ToString();
            effectNode.AppendChild(gNode);

            XmlNode bNode = CreateXmlNodeOfFile("b");
            bNode.InnerText = InitColor.B.ToString();
            effectNode.AppendChild(bNode);

            XmlNode brightnessNode = CreateXmlNodeOfFile("brightness");
            brightnessNode.InnerText = Brightness.ToString();
            effectNode.AppendChild(brightnessNode);

            XmlNode speedNode = CreateXmlNodeOfFile("speed");
            speedNode.InnerText = Speed.ToString();
            effectNode.AppendChild(speedNode);

            XmlNode angleNode = CreateXmlNodeOfFile("angle");
            angleNode.InnerText = Angle.ToString();
            effectNode.AppendChild(angleNode);

            XmlNode directionNode = CreateXmlNodeOfFile("direction");
            directionNode.InnerText = Direction.ToString();
            effectNode.AppendChild(directionNode);

            XmlNode randomNode = CreateXmlNodeOfFile("random");
            randomNode.InnerText = Random.ToString();
            effectNode.AppendChild(randomNode);

            XmlNode highNode = CreateXmlNodeOfFile("high");
            highNode.InnerText = High.ToString();
            effectNode.AppendChild(highNode);

            XmlNode lowNode = CreateXmlNodeOfFile("low");
            lowNode.InnerText = Low.ToString();
            effectNode.AppendChild(lowNode);

            XmlNode colorPointListNode = CreateXmlNodeOfFile("colorPointList");
            foreach (var item in ColorPointList)
            {
                XmlNode colorPointNode = CreateXmlNodeOfFile("colorPoint");

                XmlNode colorANode = CreateXmlNodeOfFile("a");
                colorANode.InnerText = item.Color.A.ToString();
                colorPointNode.AppendChild(colorANode);

                XmlNode colorRNode = CreateXmlNodeOfFile("r");
                colorRNode.InnerText = item.Color.R.ToString();
                colorPointNode.AppendChild(colorRNode);

                XmlNode colorGNode = CreateXmlNodeOfFile("g");
                colorGNode.InnerText = item.Color.G.ToString();
                colorPointNode.AppendChild(colorGNode);

                XmlNode colorBNode = CreateXmlNodeOfFile("b");
                colorBNode.InnerText = item.Color.B.ToString();
                colorPointNode.AppendChild(colorBNode);

                XmlNode offsetNode = CreateXmlNodeOfFile("offset");
                offsetNode.InnerText = item.Offset.ToString();
                colorPointNode.AppendChild(offsetNode);

                colorPointListNode.AppendChild(colorPointNode);
            }
            effectNode.AppendChild(colorPointListNode);

            return effectNode;
        }
    }
}
