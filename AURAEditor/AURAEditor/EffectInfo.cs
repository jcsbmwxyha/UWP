using System;
using System.Collections.Generic;
using System.Xml;
using Windows.UI;
using AuraEditor.Common;
using static AuraEditor.Common.EffectHelper;
using static AuraEditor.Common.Math2;
using static AuraEditor.Common.XmlHelper;

namespace AuraEditor
{
    public class EffectInfo : ICloneable
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
        public bool ColorSegmentation;
        static public double GetLedSpeed(int effType, int speed)
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

        public EffectInfo() {}
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
            ColorPointList = new List<ColorPoint>(MainPage.Self.CallDefaultList()[5]);
            ColorSegmentation = false;
        }

        #region Create script for lighting service
        public XmlNode ToXmlNodeForScript(int zoneCount)
        {
            XmlNode effectNode = CreateXmlNode("effect");

            effectNode.AppendChild(GetViewportTransformXmlNode());
            effectNode.AppendChild(GetWaveListXmlNode(zoneCount));
            effectNode.AppendChild(GetInitColorXmlNode(InitColor, Random));

            return effectNode;
        }

        private XmlNode GetViewportTransformXmlNode()
        {
            XmlNode viewportTransformNode = CreateXmlNode("viewportTransform");
            double[] hsl = AuraEditorColorHelper.RgbTOHsl(InitColor);

            XmlNode rotateNode = CreateXmlNode("rotate");

            XmlNode xNode = CreateXmlNode("x");
            xNode.InnerText = "0";
            rotateNode.AppendChild(xNode);

            XmlNode yNode = CreateXmlNode("y");
            yNode.InnerText = "0";
            rotateNode.AppendChild(yNode);

            XmlNode angleNode = CreateXmlNode("angle");
            angleNode.InnerText = Angle_CreatorToLService(Angle).ToString();
            rotateNode.AppendChild(angleNode);

            XmlNode methodNode = GetMethodXmlNode(Type);
            viewportTransformNode.AppendChild(rotateNode);
            viewportTransformNode.AppendChild(methodNode);

            return viewportTransformNode;
        }
        static private XmlNode GetMethodXmlNode(int effType)
        {
            string methodString = "point";
            XmlNode methodNode = CreateXmlNode("method");


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
                XmlNode inputNode = CreateXmlNode("input");
                inputNode.InnerText = "0";
                methodNode.AppendChild(inputNode);

                methodString = "OrthogonaProject";
            }
            else if (GetEffectName(effType) == "Reactive")
            {
                XmlNode inputNode = CreateXmlNode("input");
                inputNode.InnerText = "keyPressX";
                methodNode.AppendChild(inputNode);

                XmlNode inputNode2 = CreateXmlNode("input");
                inputNode2.InnerText = "keyPressY";
                methodNode.AppendChild(inputNode2);

                XmlNode inputNode3 = CreateXmlNode("input");
                inputNode3.InnerText = "0.5";
                methodNode.AppendChild(inputNode3);

                methodString = "limitRadius";
            }
            else if (GetEffectName(effType) == "Laser")
            {
                XmlNode inputNode = CreateXmlNode("input");
                inputNode.InnerText = "keyPressX";
                methodNode.AppendChild(inputNode);

                XmlNode inputNode2 = CreateXmlNode("input");
                inputNode2.InnerText = "keyPressY";
                methodNode.AppendChild(inputNode2);

                methodString = "distance";
            }
            else if (GetEffectName(effType) == "Ripple")
            {
                XmlNode inputNode = CreateXmlNode("input");
                inputNode.InnerText = "keyPressX";
                methodNode.AppendChild(inputNode);

                XmlNode inputNode2 = CreateXmlNode("input");
                inputNode2.InnerText = "keyPressY";
                methodNode.AppendChild(inputNode2);

                methodString = "radius";
            }
            else if (GetEffectName(effType) == "Star")
            {
                methodString = "shuffle";
            }

            XmlAttribute attribute = CreateXmlAttributeOfFile("key");
            attribute.Value = methodString;
            methodNode.Attributes.Append(attribute);

            return methodNode;
        }

        private XmlNode GetWaveListXmlNode(int zoneCount)
        {
            XmlNode waveListNode = CreateXmlNode("waveList");
            int maxOperatingGridWidth = AuraSpaceManager.Self.MaxOperatingGridWidth;
            int maxOperatingGridHeight = AuraSpaceManager.Self.MaxOperatingGridHeight;
            double temp = Angle_CreatorToLService(Angle);
            int maxOperatingGridLength = MaxOperatingLength(maxOperatingGridWidth, maxOperatingGridHeight, Angle_CreatorToLService(Angle));

            if (GetEffectName(Type) == "Static")
            {
                XmlNode waveNode = CreateXmlNode("wave");
                waveNode.AppendChild(CreateXmlNodeByValue("type", "ConstantWave"));
                waveNode.AppendChild(CreateXmlNodeByValue("max", "1"));
                waveNode.AppendChild(CreateXmlNodeByValue("min", "0"));
                waveNode.AppendChild(CreateXmlNodeByValue("length", "10"));
                waveNode.AppendChild(CreateXmlNodeByValue("freq", "0"));
                waveNode.AppendChild(CreateXmlNodeByValue("phase", "0"));
                waveNode.AppendChild(CreateXmlNodeByValue("start", "0"));
                waveNode.AppendChild(CreateXmlNodeByValue("velocity", "0"));
                waveNode.AppendChild(CreateXmlNodeByValue("isCycle", "0"));
                waveNode.AppendChild(GetBindToSlotXmlNode(new List<string> { "ALPHA" }));
                waveListNode.AppendChild(waveNode);
            }
            else if (GetEffectName(Type) == "Breath")
            {
                XmlNode waveNode = CreateXmlNode("wave");
                waveNode.AppendChild(CreateXmlNodeByValue("type", "SineWave"));
                waveNode.AppendChild(CreateXmlNodeByValue("max", "1"));
                waveNode.AppendChild(CreateXmlNodeByValue("min", "0"));
                waveNode.AppendChild(CreateXmlNodeByValue("length", "23"));
                if (Speed == 0)
                    waveNode.AppendChild(CreateXmlNodeByValue("freq", "0.1"));
                else if (Speed == 1)
                    waveNode.AppendChild(CreateXmlNodeByValue("freq", "0.2"));
                else // 2
                    waveNode.AppendChild(CreateXmlNodeByValue("freq", "0.4"));
                waveNode.AppendChild(CreateXmlNodeByValue("phase", "0"));
                waveNode.AppendChild(CreateXmlNodeByValue("start", "0"));
                waveNode.AppendChild(CreateXmlNodeByValue("velocity", "0"));
                waveNode.AppendChild(CreateXmlNodeByValue("isCycle", "0"));
                waveNode.AppendChild(GetBindToSlotXmlNode(new List<string> { "ALPHA" }));
                waveListNode.AppendChild(waveNode);

                // Breath bug workaround
                XmlNode waveNode2 = CreateXmlNode("wave");
                waveNode2.AppendChild(CreateXmlNodeByValue("type", "SineWave"));
                waveNode2.AppendChild(CreateXmlNodeByValue("max", "0.5"));
                waveNode2.AppendChild(CreateXmlNodeByValue("min", "0"));
                waveNode2.AppendChild(CreateXmlNodeByValue("length", "23"));
                if (Speed == 0)
                    waveNode2.AppendChild(CreateXmlNodeByValue("freq", "0.1"));
                else if (Speed == 1)
                    waveNode2.AppendChild(CreateXmlNodeByValue("freq", "0.2"));
                else // 2
                    waveNode2.AppendChild(CreateXmlNodeByValue("freq", "0.4"));
                waveNode2.AppendChild(CreateXmlNodeByValue("phase", "0"));
                waveNode2.AppendChild(CreateXmlNodeByValue("start", "0"));
                waveNode2.AppendChild(CreateXmlNodeByValue("velocity", "0"));
                waveNode2.AppendChild(CreateXmlNodeByValue("isCycle", "0"));
                waveNode2.AppendChild(GetBindToSlotXmlNode(new List<string> { "LIGHTNESS" }));
                waveListNode.AppendChild(waveNode2);
            }
            else if (GetEffectName(Type) == "ColorCycle")
            {
                XmlNode waveNode = CreateXmlNode("wave");
                waveNode.AppendChild(CreateXmlNodeByValue("type", "QuarterSineWave"));
                waveNode.AppendChild(CreateXmlNodeByValue("max", "1"));
                waveNode.AppendChild(CreateXmlNodeByValue("min", "0"));
                waveNode.AppendChild(CreateXmlNodeByValue("length", "23"));
                if (Speed == 0)
                    waveNode.AppendChild(CreateXmlNodeByValue("freq", "-0.02"));
                else if (Speed == 1)
                    waveNode.AppendChild(CreateXmlNodeByValue("freq", "-0.04"));
                else // 2
                    waveNode.AppendChild(CreateXmlNodeByValue("freq", "-0.08"));
                waveNode.AppendChild(CreateXmlNodeByValue("phase", "0"));
                waveNode.AppendChild(CreateXmlNodeByValue("start", "0"));
                waveNode.AppendChild(CreateXmlNodeByValue("velocity", "0"));
                waveNode.AppendChild(CreateXmlNodeByValue("isCycle", "0"));
                waveNode.AppendChild(GetBindToSlotXmlNode(new List<string> { "HUE" }));
                waveListNode.AppendChild(waveNode);
            }
            else if (GetEffectName(Type) == "Rainbow")
            {
                // wave 1 for HUE
                XmlNode waveNode1 = CreateXmlNode("wave");
                if (ColorSegmentation == true)
                    waveNode1.AppendChild(CreateXmlNodeByValue("type", "CustomStepWave"));
                else // false
                    waveNode1.AppendChild(CreateXmlNodeByValue("type", "CustomLinearWave"));
                waveNode1.AppendChild(CreateXmlNodeByValue("max", "1"));
                waveNode1.AppendChild(CreateXmlNodeByValue("min", "0"));
                waveNode1.AppendChild(CreateXmlNodeByValue("length", maxOperatingGridLength.ToString()));
                if (Speed == 0)
                    waveNode1.AppendChild(CreateXmlNodeByValue("freq", "0.05"));
                else if (Speed == 1)
                    waveNode1.AppendChild(CreateXmlNodeByValue("freq", "0.1"));
                else // 2
                    waveNode1.AppendChild(CreateXmlNodeByValue("freq", "0.2"));
                waveNode1.AppendChild(CreateXmlNodeByValue("phase", "0"));
                waveNode1.AppendChild(CreateXmlNodeByValue("start", "0"));
                waveNode1.AppendChild(CreateXmlNodeByValue("velocity", "0"));
                waveNode1.AppendChild(CreateXmlNodeByValue("isCycle", "0"));
                waveNode1.AppendChild(GetBindToSlotXmlNode(new List<string> { "HUE" }));
                waveNode1.AppendChild(GetCustomizedFromColorPointList(ColorPointList, 0));
                waveListNode.AppendChild(waveNode1);

                // wave 2 for SATURATION
                XmlNode waveNode2 = CreateXmlNode("wave");
                if (ColorSegmentation == true)
                    waveNode2.AppendChild(CreateXmlNodeByValue("type", "CustomStepWave"));
                else // false
                    waveNode2.AppendChild(CreateXmlNodeByValue("type", "CustomLinearWave"));
                waveNode2.AppendChild(CreateXmlNodeByValue("max", "1"));
                waveNode2.AppendChild(CreateXmlNodeByValue("min", "0"));
                waveNode2.AppendChild(CreateXmlNodeByValue("length", maxOperatingGridLength.ToString()));
                if (Speed == 0)
                    waveNode2.AppendChild(CreateXmlNodeByValue("freq", "0.05"));
                else if (Speed == 1)
                    waveNode2.AppendChild(CreateXmlNodeByValue("freq", "0.1"));
                else // 2
                    waveNode2.AppendChild(CreateXmlNodeByValue("freq", "0.2"));
                waveNode2.AppendChild(CreateXmlNodeByValue("phase", "0"));
                waveNode2.AppendChild(CreateXmlNodeByValue("start", "0"));
                waveNode2.AppendChild(CreateXmlNodeByValue("velocity", "0"));
                waveNode2.AppendChild(CreateXmlNodeByValue("isCycle", "0"));
                waveNode2.AppendChild(GetBindToSlotXmlNode(new List<string> { "SATURATION" }));
                waveNode2.AppendChild(GetCustomizedFromColorPointList(ColorPointList, 1));
                waveListNode.AppendChild(waveNode2);

                // wave 3 for LIGHTNESS
                XmlNode waveNode3 = CreateXmlNode("wave");
                if (ColorSegmentation == true)
                    waveNode3.AppendChild(CreateXmlNodeByValue("type", "CustomStepWave"));
                else // false
                    waveNode3.AppendChild(CreateXmlNodeByValue("type", "CustomLinearWave"));
                waveNode3.AppendChild(CreateXmlNodeByValue("max", "1"));
                waveNode3.AppendChild(CreateXmlNodeByValue("min", "0"));
                waveNode3.AppendChild(CreateXmlNodeByValue("length", maxOperatingGridLength.ToString()));
                if (Speed == 0)
                    waveNode3.AppendChild(CreateXmlNodeByValue("freq", "0.05"));
                else if (Speed == 1)
                    waveNode3.AppendChild(CreateXmlNodeByValue("freq", "0.1"));
                else // 2
                    waveNode3.AppendChild(CreateXmlNodeByValue("freq", "0.2"));
                waveNode3.AppendChild(CreateXmlNodeByValue("phase", "0"));
                waveNode3.AppendChild(CreateXmlNodeByValue("start", "0"));
                waveNode3.AppendChild(CreateXmlNodeByValue("velocity", "0"));
                waveNode3.AppendChild(CreateXmlNodeByValue("isCycle", "0"));
                waveNode3.AppendChild(GetBindToSlotXmlNode(new List<string> { "LIGHTNESS" }));
                waveNode3.AppendChild(GetCustomizedFromColorPointList(ColorPointList, 2));
                waveListNode.AppendChild(waveNode3);
            }
            else if (GetEffectName(Type) == "Strobing")
            {
                XmlNode waveNode = CreateXmlNode("wave");
                waveNode.AppendChild(CreateXmlNodeByValue("type", "SawtoothWave"));
                waveNode.AppendChild(CreateXmlNodeByValue("max", "0.5"));
                waveNode.AppendChild(CreateXmlNodeByValue("min", "0"));
                waveNode.AppendChild(CreateXmlNodeByValue("length", "23"));
                if (Speed == 0)
                    waveNode.AppendChild(CreateXmlNodeByValue("freq", "0.8"));
                else if (Speed == 1)
                    waveNode.AppendChild(CreateXmlNodeByValue("freq", "1.2"));
                else // 2
                    waveNode.AppendChild(CreateXmlNodeByValue("freq", "1.6"));
                waveNode.AppendChild(CreateXmlNodeByValue("phase", "0"));
                waveNode.AppendChild(CreateXmlNodeByValue("start", "0"));
                waveNode.AppendChild(CreateXmlNodeByValue("velocity", "0"));
                waveNode.AppendChild(CreateXmlNodeByValue("isCycle", "0"));
                waveNode.AppendChild(GetBindToSlotXmlNode(new List<string> { "LIGHTNESS" }));
                waveListNode.AppendChild(waveNode);
            }
            else if (GetEffectName(Type) == "Comet")
            {
                XmlNode waveNode = CreateXmlNode("wave");
                waveNode.AppendChild(CreateXmlNodeByValue("type", "TriangleWave"));
                waveNode.AppendChild(CreateXmlNodeByValue("max", "0.1"));
                waveNode.AppendChild(CreateXmlNodeByValue("min", "0"));
                waveNode.AppendChild(CreateXmlNodeByValue("length", "3"));
                if (Speed == 0)
                {
                    waveNode.AppendChild(CreateXmlNodeByValue("freq", "0.01"));
                    waveNode.AppendChild(CreateXmlNodeByValue("velocity", "10"));
                }
                else if (Speed == 1)
                {
                    waveNode.AppendChild(CreateXmlNodeByValue("freq", "0.02"));
                    waveNode.AppendChild(CreateXmlNodeByValue("velocity", "15"));
                }
                else // 2
                {
                    waveNode.AppendChild(CreateXmlNodeByValue("freq", "0.04"));
                    waveNode.AppendChild(CreateXmlNodeByValue("velocity", "20"));
                }
                waveNode.AppendChild(CreateXmlNodeByValue("phase", "0"));
                waveNode.AppendChild(CreateXmlNodeByValue("start", "0"));
                waveNode.AppendChild(CreateXmlNodeByValue("isCycle", "1"));
                waveNode.AppendChild(GetBindToSlotXmlNode(new List<string> { "ALPHA" }));
                waveListNode.AppendChild(waveNode);

                if (Random == true)
                {
                    // wave 2
                    XmlNode waveNode2 = CreateXmlNode("wave");
                    waveNode2.AppendChild(CreateXmlNodeByValue("type", "TriangleWave"));
                    waveNode2.AppendChild(CreateXmlNodeByValue("max", "1"));
                    waveNode2.AppendChild(CreateXmlNodeByValue("min", "0"));
                    waveNode2.AppendChild(CreateXmlNodeByValue("length", "10"));
                    waveNode2.AppendChild(CreateXmlNodeByValue("freq", "1"));
                    waveNode2.AppendChild(CreateXmlNodeByValue("phase", "0"));
                    waveNode2.AppendChild(CreateXmlNodeByValue("start", "0"));
                    waveNode2.AppendChild(CreateXmlNodeByValue("velocity", "15"));
                    waveNode2.AppendChild(CreateXmlNodeByValue("isCycle", "1"));
                    waveNode2.AppendChild(GetBindToSlotXmlNode(new List<string> { "HUE" }));
                    waveListNode.AppendChild(waveNode2);
                }
            }
            else if (GetEffectName(Type) == "Tide")
            {
                XmlNode waveNode = CreateXmlNode("wave");
                waveNode.AppendChild(CreateXmlNodeByValue("type", "SineWave"));
                waveNode.AppendChild(CreateXmlNodeByValue("max", "0.4"));
                waveNode.AppendChild(CreateXmlNodeByValue("min", "0.2"));
                waveNode.AppendChild(CreateXmlNodeByValue("length", "23"));
                waveNode.AppendChild(CreateXmlNodeByValue("freq", "0.25"));
                waveNode.AppendChild(CreateXmlNodeByValue("phase", "0"));
                waveNode.AppendChild(CreateXmlNodeByValue("start", "-3"));
                waveNode.AppendChild(CreateXmlNodeByValue("velocity", "0"));
                waveNode.AppendChild(CreateXmlNodeByValue("isCycle", "0"));

                // bindToSignalNode ++
                XmlNode bindToSignalNode = CreateXmlNode("bindToSignal");
                bindToSignalNode.AppendChild(CreateXmlNodeByValue("source", "Formula"));
                bindToSignalNode.AppendChild(CreateXmlNodeByValue("target", "length"));
                bindToSignalNode.AppendChild(CreateXmlNodeByValue("amplify", "1"));
                bindToSignalNode.AppendChild(CreateXmlNodeByValue("offset", "0"));
                bindToSignalNode.AppendChild(CreateXmlNodeByValue("type", "SineWave"));
                bindToSignalNode.AppendChild(CreateXmlNodeByValue("max", (maxOperatingGridLength + 3).ToString())); // because start is -3
                bindToSignalNode.AppendChild(CreateXmlNodeByValue("min", "0"));
                bindToSignalNode.AppendChild(CreateXmlNodeByValue("length", "25"));
                if (Speed == 0)
                {
                    bindToSignalNode.AppendChild(CreateXmlNodeByValue("freq", "0.05"));
                }
                else if (Speed == 1)
                {
                    bindToSignalNode.AppendChild(CreateXmlNodeByValue("freq", "0.1"));
                }
                else // 2
                {
                    bindToSignalNode.AppendChild(CreateXmlNodeByValue("freq", "0.3"));
                }
                waveNode.AppendChild(bindToSignalNode);
                // bindToSignalNode --

                waveNode.AppendChild(GetBindToSlotXmlNode(new List<string> { "ALPHA" }));
                waveListNode.AppendChild(waveNode);
            }
            else if (GetEffectName(Type) == "Star")
            {
                // wave 1
                XmlNode waveNode = CreateXmlNode("wave");
                waveNode.AppendChild(CreateXmlNodeByValue("type", "SineWave"));
                waveNode.AppendChild(CreateXmlNodeByValue("max", "0.8"));
                waveNode.AppendChild(CreateXmlNodeByValue("min", "0.5"));
                waveNode.AppendChild(CreateXmlNodeByValue("length", (zoneCount / 7 + 1).ToString()));
                waveNode.AppendChild(CreateXmlNodeByValue("freq", "Random"));
                waveNode.AppendChild(CreateXmlNodeByValue("phase", "Random"));
                waveNode.AppendChild(CreateXmlNodeByValue("start", "0"));
                waveNode.AppendChild(CreateXmlNodeByValue("velocity", "4"));
                waveNode.AppendChild(CreateXmlNodeByValue("isCycle", "1"));
                waveNode.AppendChild(GetBindToSlotXmlNode(new List<string> { "LIGHTNESS" }));
                waveListNode.AppendChild(waveNode);

                XmlNode waveNode2 = CreateXmlNode("wave");
                waveNode2.AppendChild(CreateXmlNodeByValue("type", "ConstantWave"));
                waveNode2.AppendChild(CreateXmlNodeByValue("max", "1"));
                waveNode2.AppendChild(CreateXmlNodeByValue("min", "0"));
                waveNode2.AppendChild(CreateXmlNodeByValue("length", (zoneCount / 7 + 1).ToString()));
                waveNode2.AppendChild(CreateXmlNodeByValue("freq", "Random"));
                waveNode2.AppendChild(CreateXmlNodeByValue("phase", "Random"));
                waveNode2.AppendChild(CreateXmlNodeByValue("start", "0"));
                waveNode2.AppendChild(CreateXmlNodeByValue("velocity", "4"));
                waveNode2.AppendChild(CreateXmlNodeByValue("isCycle", "1"));
                waveNode2.AppendChild(GetBindToSlotXmlNode(new List<string> { "ALPHA" }));
                waveListNode.AppendChild(waveNode2);

                if (Random == true)
                {
                    // wave 2
                    XmlNode waveNode3 = CreateXmlNode("wave");
                    waveNode3.AppendChild(CreateXmlNodeByValue("type", "SineWave"));
                    waveNode3.AppendChild(CreateXmlNodeByValue("max", "1"));
                    waveNode3.AppendChild(CreateXmlNodeByValue("min", "0"));
                    waveNode3.AppendChild(CreateXmlNodeByValue("length", (zoneCount / 7 + 1).ToString()));
                    waveNode3.AppendChild(CreateXmlNodeByValue("freq", "Random"));
                    waveNode3.AppendChild(CreateXmlNodeByValue("phase", "Random"));
                    waveNode3.AppendChild(CreateXmlNodeByValue("start", "0"));
                    waveNode3.AppendChild(CreateXmlNodeByValue("velocity", "4"));
                    waveNode3.AppendChild(CreateXmlNodeByValue("isCycle", "1"));
                    waveNode3.AppendChild(GetBindToSlotXmlNode(new List<string> { "HUE" }));
                    waveListNode.AppendChild(waveNode3);
                }
            }
            else if (GetEffectName(Type) == "Reactive")
            {
                XmlNode waveNode = CreateXmlNode("wave");
                waveNode.AppendChild(CreateXmlNodeByValue("type", "SineWave"));
                waveNode.AppendChild(CreateXmlNodeByValue("max", "1"));
                waveNode.AppendChild(CreateXmlNodeByValue("min", "0"));
                waveNode.AppendChild(CreateXmlNodeByValue("length", "1"));
                if (Speed == 0)
                    waveNode.AppendChild(CreateXmlNodeByValue("freq", "1"));
                else if (Speed == 1)
                    waveNode.AppendChild(CreateXmlNodeByValue("freq", "2"));
                else // 2
                    waveNode.AppendChild(CreateXmlNodeByValue("freq", "4"));
                waveNode.AppendChild(CreateXmlNodeByValue("phase", "0"));
                waveNode.AppendChild(CreateXmlNodeByValue("start", "0"));
                waveNode.AppendChild(CreateXmlNodeByValue("velocity", "0"));
                waveNode.AppendChild(CreateXmlNodeByValue("isCycle", "0"));
                waveNode.AppendChild(GetBindToSlotXmlNode(new List<string> { "ALPHA" }));
                waveListNode.AppendChild(waveNode);
            }
            else if (GetEffectName(Type) == "Laser")
            {
                XmlNode waveNode = CreateXmlNode("wave");
                waveNode.AppendChild(CreateXmlNodeByValue("type", "SineWave"));
                waveNode.AppendChild(CreateXmlNodeByValue("max", "1"));
                waveNode.AppendChild(CreateXmlNodeByValue("min", "0"));
                waveNode.AppendChild(CreateXmlNodeByValue("length", "10"));
                waveNode.AppendChild(CreateXmlNodeByValue("freq", "Random"));
                waveNode.AppendChild(CreateXmlNodeByValue("phase", "0"));
                waveNode.AppendChild(CreateXmlNodeByValue("start", "0"));
                if (Speed == 0)
                    waveNode.AppendChild(CreateXmlNodeByValue("velocity", "10"));
                else if (Speed == 1)
                    waveNode.AppendChild(CreateXmlNodeByValue("velocity", "15"));
                else // 2
                    waveNode.AppendChild(CreateXmlNodeByValue("velocity", "20"));
                waveNode.AppendChild(CreateXmlNodeByValue("isCycle", "0"));
                waveNode.AppendChild(GetBindToSlotXmlNode(new List<string> { "ALPHA" }));
                waveListNode.AppendChild(waveNode);
            }
            else if (GetEffectName(Type) == "Ripple")
            {
                XmlNode waveNode = CreateXmlNode("wave");
                waveNode.AppendChild(CreateXmlNodeByValue("type", "SineWave"));
                waveNode.AppendChild(CreateXmlNodeByValue("max", "1"));
                waveNode.AppendChild(CreateXmlNodeByValue("min", "0"));
                waveNode.AppendChild(CreateXmlNodeByValue("length", "10"));
                if (Speed == 0)
                {
                    waveNode.AppendChild(CreateXmlNodeByValue("freq", "1"));
                    waveNode.AppendChild(CreateXmlNodeByValue("velocity", "10"));
                }
                else if (Speed == 1)
                {
                    waveNode.AppendChild(CreateXmlNodeByValue("freq", "2"));
                    waveNode.AppendChild(CreateXmlNodeByValue("velocity", "15"));
                }
                else // 2
                {
                    waveNode.AppendChild(CreateXmlNodeByValue("freq", "4"));
                    waveNode.AppendChild(CreateXmlNodeByValue("velocity", "20"));
                }
                waveNode.AppendChild(CreateXmlNodeByValue("phase", "0"));
                waveNode.AppendChild(CreateXmlNodeByValue("start", "0"));
                waveNode.AppendChild(CreateXmlNodeByValue("isCycle", "0"));
                waveNode.AppendChild(GetBindToSlotXmlNode(new List<string> { "ALPHA" }));
                waveListNode.AppendChild(waveNode);
            }

            return waveListNode;
        }
        static private XmlNode GetBindToSlotXmlNode(List<string> slotkeys)
        {
            XmlNode bindToSlotXmlNode = CreateXmlNode("bindToSlot");

            foreach (var key in slotkeys)
            {
                XmlNode slotNode = CreateXmlNode("slot");
                XmlAttribute attribute = CreateXmlAttributeOfFile("key");
                attribute.Value = key;
                slotNode.Attributes.Append(attribute);
                bindToSlotXmlNode.AppendChild(slotNode);
            }

            return bindToSlotXmlNode;
        }
        static private XmlNode GetCustomizedFromColorPointList(List<ColorPoint> ColorPointList, int hsv)
        {
            XmlNode customizedNode = CreateXmlNode("customized");
            double Scaledown = (double)(ColorPointList.Count - 1) / (double)ColorPointList.Count;

            for (int i = 0; i < ColorPointList.Count; i++)
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
            XmlNode nodeXmlNode = CreateXmlNode("node");
            XmlAttribute attribute = CreateXmlAttributeOfFile("key");
            XmlNode phaseXmlNode = CreateXmlNode("phase");
            XmlNode fxXmlNode = CreateXmlNode("fx");
            double[] hsl = AuraEditorColorHelper.RgbTOHsl(c);

            attribute.Value = key.ToString();
            nodeXmlNode.Attributes.Append(attribute);
            phaseXmlNode.InnerText = offset.ToString();
            nodeXmlNode.AppendChild(phaseXmlNode);
            fxXmlNode.InnerText = hsl[hsv].ToString();
            nodeXmlNode.AppendChild(fxXmlNode);

            return nodeXmlNode;
        }

        static private XmlNode GetInitColorXmlNode(Color c, bool random)
        {
            XmlNode initColorNode = CreateXmlNode("initColor");
            double[] hsl = AuraEditorColorHelper.RgbTOHsl(c);

            XmlNode hueNode = CreateXmlNode("hue");
            if (random == true)
            {
                hueNode.InnerText = "Random";
            }
            else
            {
                hueNode.InnerText = hsl[0].ToString();
            }
            initColorNode.AppendChild(hueNode);

            XmlNode saturationNode = CreateXmlNode("saturation");
            saturationNode.InnerText = hsl[1].ToString();
            initColorNode.AppendChild(saturationNode);

            XmlNode lightnessNode = CreateXmlNode("lightness");
            lightnessNode.InnerText = hsl[2].ToString();
            initColorNode.AppendChild(lightnessNode);

            XmlNode alphaNode = CreateXmlNode("alpha");
            alphaNode.InnerText = (c.A / 255).ToString();
            initColorNode.AppendChild(alphaNode);

            return initColorNode;
        }
        #endregion

        public XmlNode ToXmlNodeForUserData()
        {
            XmlNode effectNode = CreateXmlNode("effect");

            XmlNode typeNode = CreateXmlNode("type");
            typeNode.InnerText = Type.ToString();
            effectNode.AppendChild(typeNode);

            XmlNode aNode = CreateXmlNode("a");
            aNode.InnerText = InitColor.A.ToString();
            effectNode.AppendChild(aNode);

            XmlNode rNode = CreateXmlNode("r");
            rNode.InnerText = InitColor.R.ToString();
            effectNode.AppendChild(rNode);

            XmlNode gNode = CreateXmlNode("g");
            gNode.InnerText = InitColor.G.ToString();
            effectNode.AppendChild(gNode);

            XmlNode bNode = CreateXmlNode("b");
            bNode.InnerText = InitColor.B.ToString();
            effectNode.AppendChild(bNode);

            XmlNode brightnessNode = CreateXmlNode("brightness");
            brightnessNode.InnerText = Brightness.ToString();
            effectNode.AppendChild(brightnessNode);

            XmlNode speedNode = CreateXmlNode("speed");
            speedNode.InnerText = Speed.ToString();
            effectNode.AppendChild(speedNode);

            XmlNode angleNode = CreateXmlNode("angle");
            angleNode.InnerText = Angle.ToString();
            effectNode.AppendChild(angleNode);

            XmlNode directionNode = CreateXmlNode("direction");
            directionNode.InnerText = Direction.ToString();
            effectNode.AppendChild(directionNode);

            XmlNode randomNode = CreateXmlNode("random");
            randomNode.InnerText = Random.ToString();
            effectNode.AppendChild(randomNode);

            XmlNode highNode = CreateXmlNode("high");
            highNode.InnerText = High.ToString();
            effectNode.AppendChild(highNode);

            XmlNode lowNode = CreateXmlNode("low");
            lowNode.InnerText = Low.ToString();
            effectNode.AppendChild(lowNode);

            XmlNode colorPointListNode = CreateXmlNode("colorPointList");
            foreach (var item in ColorPointList)
            {
                XmlNode colorPointNode = CreateXmlNode("colorPoint");

                XmlNode colorANode = CreateXmlNode("a");
                colorANode.InnerText = item.Color.A.ToString();
                colorPointNode.AppendChild(colorANode);

                XmlNode colorRNode = CreateXmlNode("r");
                colorRNode.InnerText = item.Color.R.ToString();
                colorPointNode.AppendChild(colorRNode);

                XmlNode colorGNode = CreateXmlNode("g");
                colorGNode.InnerText = item.Color.G.ToString();
                colorPointNode.AppendChild(colorGNode);

                XmlNode colorBNode = CreateXmlNode("b");
                colorBNode.InnerText = item.Color.B.ToString();
                colorPointNode.AppendChild(colorBNode);

                XmlNode offsetNode = CreateXmlNode("offset");
                offsetNode.InnerText = item.Offset.ToString();
                colorPointNode.AppendChild(offsetNode);

                colorPointListNode.AppendChild(colorPointNode);
            }
            effectNode.AppendChild(colorPointListNode);

            XmlNode colorSegmentationNode = CreateXmlNode("colorSegmentation");
            colorSegmentationNode.InnerText = ColorSegmentation.ToString();
            effectNode.AppendChild(colorSegmentationNode);

            return effectNode;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
