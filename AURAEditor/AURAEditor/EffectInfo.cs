using Windows.UI;
using static AuraEditor.Common.XmlHelper;
using static AuraEditor.Common.EffectHelper;
using System.Xml;
using AuraEditor.Common;

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
        }

        public XmlNode ToXmlNodeForScript()
        {
            XmlNode effectNode = CreateXmlNodeOfFile("effect");

            effectNode.AppendChild(GetViewportTransformXmlNode());
            effectNode.AppendChild(GetWaveListXmlNode());
            effectNode.AppendChild(GetInitColorXmlNode(InitColor, Random));

            return effectNode;
        }
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
            angleNode.InnerText = "0";
            rotateNode.AppendChild(angleNode);

            string methodString = "";
            XmlNode methodNode = GetMethodXmlNode(Type);
            CreateXmlNodeOfFile("method");
            if (GetEffectName(Type) == "Static") { methodString = "point"; }
            else if (GetEffectName(Type) == "Breath") { methodString = "point"; }
            else if (GetEffectName(Type) == "ColorCycle") { methodString = "point"; }
            else if (GetEffectName(Type) == "Rainbow") { methodString = "OrthogonaProject"; }
            else if (GetEffectName(Type) == "Strobing") { methodString = "point"; }
            else if (GetEffectName(Type) == "Comet") { methodString = "OrthogonaProject"; }
            else if (GetEffectName(Type) == "Reactive") { methodString = "limitRadius"; }
            else if (GetEffectName(Type) == "Laser") { methodString = "distance"; }
            else if (GetEffectName(Type) == "Radius") { methodString = "limitRadius"; }
            else if (GetEffectName(Type) == "Ripple") { methodString = "radius"; }
            else if (GetEffectName(Type) == "Star") { methodString = "randomRadius"; }
            else { methodString = "point"; }
            XmlAttribute attribute = CreateXmlAttributeOfFile("key");
            attribute.Value = methodString;
            methodNode.Attributes.Append(attribute);

            viewportTransformNode.AppendChild(rotateNode);
            viewportTransformNode.AppendChild(methodNode);

            return viewportTransformNode;
        }
        private XmlNode GetMethodXmlNode(int effType)
        {
            string methodString = "point";
            XmlNode methodNode = CreateXmlNodeOfFile("method");

            if (GetEffectName(Type) == "Static") { methodString = "point"; }
            else if (GetEffectName(Type) == "Breath") { methodString = "point"; }
            else if (GetEffectName(Type) == "ColorCycle") { methodString = "point"; }
            else if (GetEffectName(Type) == "Rainbow") { methodString = "OrthogonaProject"; }
            else if (GetEffectName(Type) == "Strobing") { methodString = "point"; }
            else if (GetEffectName(Type) == "Comet") { methodString = "OrthogonaProject"; }
            else if (GetEffectName(Type) == "Reactive") { methodString = "limitRadius"; }
            else if (GetEffectName(Type) == "Laser") { methodString = "distance"; }
            else if (GetEffectName(Type) == "Radius") { methodString = "limitRadius"; }
            else if (GetEffectName(Type) == "Ripple") { methodString = "radius"; }
            else if (GetEffectName(Type) == "Star") { methodString = "randomRadius"; }

            XmlAttribute attribute = CreateXmlAttributeOfFile("key");
            attribute.Value = methodString;
            methodNode.Attributes.Append(attribute);

            if (GetEffectName(effType) == "Ripple" ||
                GetEffectName(effType) == "Reactive" ||
                GetEffectName(effType) == "Laser")
            {
                XmlNode inputNode = CreateXmlNodeOfFile("input");
                inputNode.InnerText = "keyPressX";
                methodNode.AppendChild(inputNode);

                XmlNode inputNode2 = CreateXmlNodeOfFile("input");
                inputNode2.InnerText = "keyPressY";
                methodNode.AppendChild(inputNode2);
            }

            return methodNode;
        }
        private XmlNode GetWaveListXmlNode()
        {
            XmlNode waveListNode = CreateXmlNodeOfFile("waveList");
            XmlNode waveNode = CreateXmlNodeOfFile("wave");

            XmlNode typeNode = CreateXmlNodeOfFile("type");
            typeNode.InnerText = GetDefaultWaveType(Type);
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

            XmlNode freqNode = CreateXmlNodeOfFile("freq");
            freqNode.InnerText = GetFreq(Type, Speed);
            waveNode.AppendChild(freqNode);

            XmlNode phaseNode = CreateXmlNodeOfFile("phase");
            phaseNode.InnerText = GetPhase(Type);
            waveNode.AppendChild(phaseNode);

            XmlNode startNode = CreateXmlNodeOfFile("start");
            startNode.InnerText = GetStart(Type).ToString();
            waveNode.AppendChild(startNode);

            XmlNode velocityNode = CreateXmlNodeOfFile("velocity");
            velocityNode.InnerText = GetVelocity(Type, Speed).ToString();
            waveNode.AppendChild(velocityNode);

            waveNode.AppendChild(GetDefaultBindToSlotXmlNode(Type));
            waveListNode.AppendChild(waveNode);
            return waveListNode;
        }
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
        static private string GetDefaultWaveType(int effType)
        {
            if (GetEffectName(effType) == "Static") return "ConstantWave";
            else if (GetEffectName(effType) == "Breath") return "SineWave";
            else if (GetEffectName(effType) == "ColorCycle") return "QuarterSineWave";
            else if (GetEffectName(effType) == "Rainbow") return "QuarterSineWave";
            else if (GetEffectName(effType) == "Strobing") return "SineWave";
            else if (GetEffectName(effType) == "Comet") return "TriangleWave";
            else if (GetEffectName(effType) == "Reactive") return "SineWave";
            else if (GetEffectName(effType) == "Laser") return "SineWave";
            else if (GetEffectName(effType) == "Radius") return "SineWave";
            else if (GetEffectName(effType) == "Ripple") return "SineWave";
            else if (GetEffectName(effType) == "Star") return "SineWave";

            return "SineWave";
        }
        static private double GetMax(int effType)
        {
            if (GetEffectName(effType) == "Breath") return 0.5;
            else if (GetEffectName(effType) == "Strobing") return 0.5;
            else if (GetEffectName(effType) == "Comet") return 0.1;
            else if (GetEffectName(effType) == "Star") return 0.7;
            return 1;
        }
        static private double GetMin(int effType)
        {
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
            else if (GetEffectName(effType) == "Star") return 10;

            return 10;
        }
        static private string GetFreq(int effType, int speed)
        {
            double result = 0;

            if (speed == 0)
            {
                if (GetEffectName(effType) == "Breath") result = 0.1;
                else if (GetEffectName(effType) == "ColorCycle") result = -0.02;
                else if (GetEffectName(effType) == "Rainbow") result = 0.01;
                else if (GetEffectName(effType) == "Strobing") result = 0.2;
                else if (GetEffectName(effType) == "Comet") result = 0.01;
                else if (GetEffectName(effType) == "Reactive") result = 1;
                else if (GetEffectName(effType) == "Laser") return "Random";
                else if (GetEffectName(effType) == "Ripple") result = 1;
                else if (GetEffectName(effType) == "Star") return "Random";
            }
            else if (speed == 1)
            {
                if (GetEffectName(effType) == "Breath") result = 0.2;
                else if (GetEffectName(effType) == "ColorCycle") result = -0.04;
                else if (GetEffectName(effType) == "Rainbow") result = 0.02;
                else if (GetEffectName(effType) == "Strobing") result = 0.4;
                else if (GetEffectName(effType) == "Comet") result = 0.02;
                else if (GetEffectName(effType) == "Reactive") result = 2;
                else if (GetEffectName(effType) == "Laser") return "Random";
                else if (GetEffectName(effType) == "Ripple") result = 2;
                else if (GetEffectName(effType) == "Star") return "Random";
            }
            else if (speed == 2)
            {
                if (GetEffectName(effType) == "Breath") result = 0.4;
                else if (GetEffectName(effType) == "ColorCycle") result = -0.08;
                else if (GetEffectName(effType) == "Rainbow") result = 0.04;
                else if (GetEffectName(effType) == "Strobing") result = 0.8;
                else if (GetEffectName(effType) == "Comet") result = 0.04;
                else if (GetEffectName(effType) == "Reactive") result = 4;
                else if (GetEffectName(effType) == "Laser") return "Random";
                else if (GetEffectName(effType) == "Ripple") result = 4;
                else if (GetEffectName(effType) == "Star") return "Random";
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
            if (GetEffectName(effType) == "Comet") return -5;

            return 0;
        }
        static private double GetVelocity(int effType, int speed)
        {
            if (speed == 0)
            {
                if (GetEffectName(effType) == "Comet") return 10;
                else if (GetEffectName(effType) == "Laser") return 10;
                else if (GetEffectName(effType) == "Ripple") return 15;
            }
            else if (speed == 1)
            {
                if (GetEffectName(effType) == "Comet") return 15;
                else if (GetEffectName(effType) == "Laser") return 15;
                else if (GetEffectName(effType) == "Ripple") return 10;
            }
            else if (speed == 2)
            {
                if (GetEffectName(effType) == "Comet") return 20;
                else if (GetEffectName(effType) == "Laser") return 20;
                else if (GetEffectName(effType) == "Ripple") return 15;
            }

            return 0;
        }
        static private XmlNode GetDefaultBindToSlotXmlNode(int effType)
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
            else if (GetEffectName(effType) == "Radius")
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
            bindToSlotXmlNode.AppendChild(slotNode);

            // second slot
            if (GetEffectName(effType) == "Ripple")
            {
                XmlNode slotNode2 = CreateXmlNodeOfFile("slot");
                XmlAttribute attribute2 = CreateXmlAttributeOfFile("key");
                attribute2.Value = "HUE";
                slotNode2.Attributes.Append(attribute2);
                bindToSlotXmlNode.AppendChild(slotNode2);
            }

            return bindToSlotXmlNode;
        }

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

            return effectNode;
        }
    }
}
